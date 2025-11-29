using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameConfig;
using GameCore.Singleton;
using Hotfix.Define;
using Hotfix.Utils;
using UnityEngine;
using Logger = GameCore.Log.Logger;
using Random = UnityEngine.Random;

namespace HotfixLogic.Match
{
    /// <summary>
    /// 难度策略管理器
    /// </summary>
    public class DifficultyStrategyManager : LazySingleton<DifficultyStrategyManager>
    {
        private class VirtualNode
        {
            public int ConfigId;
            public bool IsNew;
            public bool IsImmutable; // 是否是旧棋子/障碍物
            public ElementItemData Ref; // 持有新棋子的引用以便回写
            public bool HasData; // 标记该节点是否有数据

            public void SetOld(int configId)
            {
                ConfigId = configId;
                IsNew = false;
                IsImmutable = true;
                Ref = null;
                HasData = true;
            }

            public void SetNew(ElementItemData ele)
            {
                ConfigId = ele.ConfigId;
                IsNew = true;
                IsImmutable = false;
                Ref = ele;
                HasData = true;
            }

            public void Clear()
            {
                HasData = false;
                Ref = null; // 断开引用，防止内存泄漏
            }
        }

        private VirtualNode[,] _cachedVBoard; // 虚拟棋盘缓存
        private List<ElementItemData>[] _cachedColDrops; // 每列的新掉落缓存
        private List<int> _cachedNeighborColors; // 策略2用的临时列表
        private bool _isBuffersInitialized = false;


        private int _totalControlValue = 0; // 当前的总调控值

        private Dictionary<DifficultyStrategyType, int> _activeStrategyLevels =
            new Dictionary<DifficultyStrategyType, int>(); // 记录每个策略生效的等级 (0, 1, 2)

        //记录每个分数变化的数据
        private Dictionary<string, float> _scoreChangedLogEventData = new Dictionary<string, float>();

        private LevelData _curLevelData = null;

        private StringBuilder _sb = new StringBuilder();

        // 保存当前的触发上下文
        private DifficultyTriggerContext _lastContext;

        private int _levelUsingItemLevel = 0;

        /// <summary>
        /// 上次使用道具的关卡
        /// </summary>
        /// <param name="level"></param>
        public void SetUsingItemValue(int level)
        {
            _levelUsingItemLevel = level;
        }

        public DifficultyTriggerContext CreateContext(in LevelData currentLevel, int remainSteps)
        {
            DifficultyTriggerContext context = new DifficultyTriggerContext()
            {
                LevelId = currentLevel.id,
                TotalSteps = currentLevel.stepLimit,
                RemainingSteps = remainSteps,
                ConsecutiveFailures = currentLevel.id == MatchManager.Instance.MaxLevel
                    ? LevelManager.Instance.StagePlayCount
                    : 0,
                ReturnUserDays = 0 // TODO: 待接入正式数据
            };
            return context;
        }

        /// <summary>
        /// 入口：计算调控值并抽取策略
        /// </summary>
        /// <param name="context">触发条件上下文</param>
        public void CalculateDifficultyStrategies(DifficultyTriggerContext context)
        {
            var levelType = MatchManager.Instance.CurrentMatchLevelType;
            if (levelType != MatchLevelType.C && levelType != MatchLevelType.Editor)
                return;
            _lastContext = context;
            float oldControlValue = 0;
            Dictionary<string, float> oldScoreData = new Dictionary<string, float>(_scoreChangedLogEventData);
            foreach (var item in oldScoreData)
            {
                oldControlValue += item.Value;
            }

            ResetDifficulty();

            // 1. 计算总调控值
            int controlValue = CalculateTotalControlValue(context);
            //对比前后两次的值，发送上报
            Dictionary<string, float> changedValue = new Dictionary<string, float>();
            foreach (var item in _scoreChangedLogEventData)
            {
                if (oldScoreData.ContainsKey(item.Key))
                {
                    if (!Mathf.Approximately(oldScoreData[item.Key], item.Value))
                    {
                        changedValue.Add(item.Key, item.Value - oldScoreData[item.Key]);
                    }
                }
            }

            // 固定配置值调整
            LevelStrategyDB db = ConfigMemoryPool.Get<LevelStrategyDB>();
            if (db.TryGetValue(context.LevelId, out var value))
                controlValue += value;
            _totalControlValue = controlValue;

            if (oldControlValue != 0)
                LogValueChangeEvent(_totalControlValue - oldControlValue, _totalControlValue, changedValue);


            // 2. 根据调控值抽取策略
            ApplyStrategySelection(controlValue, context);
            ReportStrategyStatus();
        }

        private void LogValueChangeEvent(float changeValue, int finalValue, Dictionary<string, float> data)
        {
            if(changeValue <= 0)
                return;
            _sb.Clear();
            foreach (var item in data)
            {
                _sb.Append($"{item.Key}={item.Value}");
            }

            Dictionary<string, object> eventData = new Dictionary<string, object>();
            eventData.Add("change_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            eventData.Add("change_value", changeValue.ToString("F"));
            eventData.Add("final_value", finalValue);
            eventData.Add("change_source", _sb.ToString());
            CommonUtil.LogEvent(LogEventKeyDefine.DifficultyStrategyValueChange, eventData);
        }

        /// <summary>
        /// 上报策略生效/失效状态
        /// </summary>
        private void ReportStrategyStatus()
        {
            _sb.Clear();
            bool isFirst = true;

            foreach (var kvp in _activeStrategyLevels)
            {
                if (kvp.Value > 0)
                {
                    if (!isFirst) _sb.Append(" | ");

                    // 获取策略名称 (你可以写个switch case转成中文，或者直接用枚举名)
                    string strategyName = kvp.Key.ToString();
                    _sb.Append($"{strategyName} (Lv{kvp.Value})");

                    isFirst = false;
                }
            }

            string finalContent = _sb.ToString();
            if (string.IsNullOrEmpty(finalContent)) finalContent = "None";

            var reportData = new Dictionary<string, object>
            {
                { "event_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                { "current_control_val", _totalControlValue }, // 此时调控值
                { "content", finalContent } // 生效/失效内容
            };
            CommonUtil.LogEvent(LogEventKeyDefine.DifficultyStrategyEnableChange, reportData);
        }

        /// <summary>
        /// 重置关卡难度数据 
        /// </summary>
        private void ResetDifficulty()
        {
            _totalControlValue = 0;
            _activeStrategyLevels.Clear();

            // 初始化所有策略等级为0
            foreach (DifficultyStrategyType type in Enum.GetValues(typeof(DifficultyStrategyType)))
            {
                _activeStrategyLevels[type] = 0;
            }
        }

        /// <summary>
        /// 计算总调控值
        /// </summary>
        private int CalculateTotalControlValue(DifficultyTriggerContext context)
        {
            StrategyControlValueDB db = ConfigMemoryPool.Get<StrategyControlValueDB>();
            float currentTotal = 0;

            void AddScore(string sourceName, int baseScore)
            {
                if (baseScore <= 0) return;
                float levelFactor = db.GetLevelCorrection(context.LevelId, sourceName);
                // 关卡数增幅后的实际变化量
                float actualChange = baseScore * (1 + levelFactor);

                if (actualChange > 0)
                {
                    currentTotal += actualChange; // 累加总分

                    _scoreChangedLogEventData.TryAdd(sourceName, actualChange);
                    Logger.Debug($"DifficultyStrategyManager] ScoreChanged: {sourceName} = {actualChange}");
                }
            }

            // --- 关卡剩余步数占比 ---
            if (context.TotalSteps > 0)
            {
                float stepRatio = (float)context.RemainingSteps / context.TotalSteps;
                int stepScore = db.GetStepControlValue(stepRatio);

                AddScore("step", stepScore);
            }

            // --- 关内使用道具 ---
            int useItemLevel = Mathf.Max(0, MatchManager.Instance.MaxLevel - _levelUsingItemLevel);
            AddScore("useItem", db.GetUseItemControlValue(useItemLevel));

            // --- 单关失败次数 ---
            int failVal = db.GetFailControlValue(context.ConsecutiveFailures);
            AddScore("failCount", failVal);

            // --- 回流用户 ---
            int returnVal = db.GetUserBackControlValue(context.ReturnUserDays);
            AddScore("returnUser", returnVal);
            return Mathf.RoundToInt(currentTotal);
        }

        /// <summary>
        /// 策略抽取逻辑
        /// </summary>
        private void ApplyStrategySelection(int controlValue, DifficultyTriggerContext context)
        {
            // 判断是否有II类条件
            bool hasClass2Condition = context.ConsecutiveFailures > 0 || context.ReturnUserDays > 0;

            // 确定抽取次数
            int drawCount = 0;
            int[] weights = new int[4]; // 对应 策略1, 2, 3, 4

            // 查表
            if (controlValue >= 22)
            {
                drawCount = hasClass2Condition ? 7 : 5;
                // 权重: 3, 3, 3, 3 (策略1,2,3,4)
                weights = new int[] { 3, 3, 3, 3 };
            }
            else if (controlValue >= 16)
            {
                drawCount = hasClass2Condition ? 6 : 5;
                weights = new int[] { 3, 3, 3, 3 };
                if (!hasClass2Condition) weights[3] = 0;
            }
            else if (controlValue >= 11)
            {
                drawCount = 5;
                weights = new int[] { 3, 3, 3, 3 };
                if (!hasClass2Condition) weights[3] = 0;
            }
            else if (controlValue >= 7)
            {
                drawCount = 4;
                weights = new int[] { 3, 3, 3, 3 };
                if (!hasClass2Condition) weights[3] = 0;
            }
            else if (controlValue >= 4)
            {
                drawCount = 3;
                weights = new int[] { 3, 3, 3, 0 }; // 策略4权重为空
                if (!hasClass2Condition) weights[3] = 0;
            }
            else if (controlValue >= 2)
            {
                drawCount = 2;
                weights = new int[] { 3, 3, 3, 0 };
                if (!hasClass2Condition) weights[3] = 0;
            }
            else if (controlValue >= 1)
            {
                drawCount = 1;
                weights = new int[] { 3, 3, 3, 0 };
                if (!hasClass2Condition) weights[3] = 0;
            }

            // 执行加权随机抽取
            for (int i = 0; i < drawCount; i++)
            {
                int strategyIndex = WeightedRandom(weights);
                if (strategyIndex == -1) break; // 无策略可抽

                DifficultyStrategyType type = (DifficultyStrategyType)(strategyIndex + 1);

                // 检查叠加限制 (策略1,2,3限2次，策略4限1次)
                int maxStack = (type == DifficultyStrategyType.SquareFormation) ? 1 : 2;

                if (_activeStrategyLevels[type] < maxStack)
                {
                    _activeStrategyLevels[type]++;
                }
                else
                {
                    // 满了就不再抽取该策略
                    weights[strategyIndex] = 0;
                }
            }
        }

        private int WeightedRandom(int[] weights)
        {
            int total = weights.Sum();
            if (total <= 0) return -1;
            int r = Random.Range(0, total);
            for (int i = 0; i < weights.Length; i++)
            {
                if (r < weights[i]) return i;
                r -= weights[i];
            }

            return weights.Length - 1;
        }

        /// <summary>
        /// 动态修改关卡掉落概率
        /// </summary>
        public void ModifyLevelDropRate(ref int[] dropColors)
        {
            _activeStrategyLevels.TryGetValue(DifficultyStrategyType.ColorRegularity, out int level);
            if (level == 0) return;

            // 获取当前关卡的颜色数量
            int colorCount = dropColors.Length;
            if (colorCount < 2) return;

            // 提高"2个棋子颜色相同"的概率
            float targetProb = GetTargetSameColorProb(colorCount, level);

            // 修改 dropColorRate 数组，使得 sum(p_i^2) ≈ targetProb
            AdjustProbabilityForSameColorTarget(ref dropColors, targetProb);
        }

        private float GetTargetSameColorProb(int n, int level)
        {
            // (Base): 1/n
            // 硬编码常见颜色数的调优值
            if (n == 2) // Base 50%
            {
                if (level == 1) return 0.60f; // +10%
                if (level >= 2) return 0.70f;
            }

            if (n == 3) // Base 33%
            {
                if (level == 1) return 0.50f;
                if (level >= 2) return 0.60f;
            }

            if (n == 4) // Base 25%
            {
                if (level == 1) return 0.40f;
                if (level >= 2) return 0.50f;
            }

            if (n == 5) // Base 20%
            {
                if (level == 1) return 0.25f; // -> 4色概率
                if (level >= 2) return 0.33f; // -> 3色概率
            }

            // 默认公式: 1/n -> 1/(n-1) -> 1/(n-2)
            float baseN = Mathf.Max(1, n - level);
            return 1.0f / baseN;
        }

        private void AdjustProbabilityForSameColorTarget(ref int[] rates, float targetProb)
        {
            // 算法：增大其中一个颜色的权重，减少其他颜色的权重，直到 sum(rate^2) 接近 targetProb
            if (rates == null || rates.Length == 0) return;

            // 迭代调整：增加最大概率项，减少其他项
            // 先就直接让第一个元素成为“优势颜色”
            // 设优势颜色概率为 x，其余 n-1 个颜色平分 (1-x)
            // 目标方程: x^2 + (n-1) * ((1-x)/(n-1))^2 = targetProb
            // x^2 + (1-x)^2 / (n-1) = targetProb
            // 解这个方程求 x

            int n = rates.Length;
            // 如果只有1种颜色，概率必然是1，无需调整
            if (n <= 1) return;

            // 解一元二次方程 ax^2 + bx + c = 0
            // (n-1)x^2 + (1 - 2x + x^2) = targetProb * (n-1)
            // n*x^2 - 2x + (1 - targetProb*(n-1)) = 0

            float a = n;
            float b = -2;
            float c = 1 - targetProb * (n - 1);

            float delta = b * b - 4 * a * c;
            if (delta >= 0)
            {
                float x1 = (-b + Mathf.Sqrt(delta)) / (2 * a);

                float dominantProb = Mathf.Clamp01(x1);
                float otherProb = (1 - dominantProb) / (n - 1);

                // 应用回 rates
                // 保持原有的相对大小顺序，增强最大的那个
                int maxIndex = 0;
                int maxVal = -1;
                for (int i = 0; i < rates.Length; i++)
                {
                    if (rates[i] > maxVal)
                    {
                        maxVal = rates[i];
                        maxIndex = i;
                    }
                }

                int newTotal = 0;
                for (int i = 0; i < rates.Length; i++)
                {
                    if (i == maxIndex)
                        rates[i] = Mathf.RoundToInt(dominantProb * 10000);
                    else
                        rates[i] = Mathf.RoundToInt(otherProb * 10000);
                    newTotal += rates[i];
                }

                // 修正总和误差 万分比
                int diff = 10000 - newTotal;
                rates[maxIndex] += diff;
            }
        }

        /// <summary>
        /// 初始独立棋子占比
        /// </summary>
        /// <returns>返回独立棋子比例的修正系数 (+0.15, +0.30 等)</returns>
        public float GetIndependentRatioModifier()
        {
            _activeStrategyLevels.TryGetValue(DifficultyStrategyType.IndependentRatio, out int level);
            if (level == 0) return 0f;

            //  调易 -> 降低15%
            return level * -0.15f;
        }

        public void ApplyAdvancedDropStrategies(GridSystem gridSystem, List<ElementItemData> newDrops,
            HashSet<Vector2Int> destroyedIndices)
        {
            int rows = gridSystem.LevelData.gridRow;
            int cols = gridSystem.LevelData.gridCol;

            EnsureBuffers(rows, cols);

            // 将新掉落按列分类
            for (int i = 0; i < newDrops.Count; i++)
            {
                var ele = newDrops[i];
                int c = ele.GridPos.x; // 注意这里只能用使用x坐标，x坐标是正确的，y由于并未掉落到实际格子上，所以y坐标是不能用的
                if (c >= 0 && c < cols)
                {
                    _cachedColDrops[c].Add(ele);
                }
            }

            // 模拟掉落
            SimulateGravity(gridSystem, rows, cols, destroyedIndices);

            // 执行策略
            ApplyVirtualStrategies(gridSystem, rows, cols);

            // 数据回写
            // 此时 _cachedVBoard 里存储的是最终颜色，同步回 _genElements
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var node = _cachedVBoard[x, y];
                    if (node.HasData && node.IsNew && node.Ref != null && node.Ref.ConfigId != node.ConfigId)
                    {
                        // Logger.Debug($"被改了:{node.Ref.GridPos} {node.Ref.ConfigId}=>{node.ConfigId}");
                        node.Ref.ConfigId = node.ConfigId;
                    }
                }
            }
        }

        private void SimulateGravity(GridSystem gridSystem, int rows, int cols, HashSet<Vector2Int> destroyedIndices)
        {
            bool IsFixedObstacleInGrid(int x, int y)
            {
                var elements = ElementSystem.Instance.GetGridElements(new Vector2Int(x, y), false);
                if (elements == null) return false;
                bool bResult = false;
                foreach (var element in elements)
                {
                    if (!element.Data.IsMovable)
                    {
                        bResult = true; //有固定的障碍物
                        break;
                    }
                }

                return bResult;
            }

            for (int x = 0; x < cols; x++)
            {
                int writeY = rows - 1; // 写入指针，从底向上

                // 处理旧棋子
                for (int y = rows - 1; y >= 0; y--)
                {
                    // 快速检查：跳过即将消除的
                    if (destroyedIndices.Contains(new Vector2Int(x, y))) continue;

                    // 检查固定障碍物
                    bool isFixed = IsFixedObstacleInGrid(x, y);

                    if (isFixed)
                    {
                        // 固定障碍物：原位不动，重置 writeY 到它上方
                        if (gridSystem.IsValidPosition(x, y))
                        {
                            var grid = gridSystem.GetGridByCoord(x, y);
                            if (grid != null && grid.Data.GetTopElement() != null)
                            {
                                _cachedVBoard[x, y].SetOld(grid.Data.GetTopElement().ConfigId);
                                writeY = y - 1;
                            }
                        }

                        continue;
                    }

                    // 普通旧棋子：掉落到 writeY
                    if (gridSystem.IsValidPosition(x, y))
                    {
                        // 寻找下一个有效空位 (跳过已经被占用的，虽然逻辑上writeY是递减的，双重保险)
                        while (writeY >= 0 && _cachedVBoard[x, writeY].HasData) writeY--;

                        if (writeY >= 0)
                        {
                            var grid = gridSystem.GetGridByCoord(x, y);
                            if (grid != null && grid.Data.GetTopElement() != null)
                            {
                                _cachedVBoard[x, writeY].SetOld(grid.Data.GetTopElement().ConfigId);
                                writeY--;
                            }
                        }
                    }
                }

                // 填入新棋子
                var colList = _cachedColDrops[x];
                // 这里的列表顺序通常是生成的顺序(可能是从下到上)，需要根据实际生成逻辑确认
                for (int i = 0; i < colList.Count; i++)
                {
                    while (writeY >= 0 && _cachedVBoard[x, writeY].HasData) writeY--;

                    if (writeY >= 0)
                    {
                        _cachedVBoard[x, writeY].SetNew(colList[i]);
                        writeY--;
                    }
                }
            }
        }

        private void ApplyVirtualStrategies(GridSystem gridSystem, int rows, int cols)
        {
            int colorCount = gridSystem.LevelData.dropColor.Length;

            bool IsCanDropCondition(int elementId)
            {
                if (gridSystem.LevelData == null || gridSystem.LevelData.dropColor == null) return false;
                for (int i = 0; i < gridSystem.LevelData.dropColor.Length; i++)
                {
                    if (gridSystem.LevelData.dropColor[i] == elementId) return true;
                }

                return false;
            }

            // --- 策略2：相邻同色 ---
            int[] dirX = { 0, 0, -1, 1 }; // 上 下 左 右
            int[] dirY = { -1, 1, 0, 0 };

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var node = _cachedVBoard[x, y];
                    // 只修改新生成的棋子
                    if (!node.HasData || !node.IsNew) continue;

                    _cachedNeighborColors.Clear();

                    // 检查4个方向
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dirX[d];
                        int ny = y + dirY[d];

                        if (nx >= 0 && nx < cols && ny >= 0 && ny < rows)
                        {
                            var neighbor = _cachedVBoard[nx, ny];
                            if (neighbor.HasData && IsCanDropCondition(neighbor.ConfigId))
                            {
                                _cachedNeighborColors.Add(neighbor.ConfigId);
                            }
                        }
                    }

                    if (_cachedNeighborColors.Count == 0) continue;

                    // 获取概率
                    float prob = GetNeighborMatchProbability(colorCount, _cachedNeighborColors.Count - 1);

                    if (Random.value < prob)
                    {
                        // 随机取色
                        int randIdx = Random.Range(0, _cachedNeighborColors.Count);
                        // 这里修改了 ConfigId，后续遍历到的节点会看到这个新颜色，形成连带效应
                        // Logger.Debug($"被改了:{node.ConfigId}");
                        node.ConfigId = _cachedNeighborColors[randIdx];
                    }
                }
            }

            // --- 策略4：四方格 ---
            if (IsSquareFormationActive())
            {
                ApplySquareStrategyOptimized(rows, cols, IsCanDropCondition);
            }
        }

        private void ApplySquareStrategyOptimized(int rows, int cols, Func<int, bool> condition)
        {
            // 边界检查：如果棋盘太小，无法形成2x2，直接返回
            if (cols < 2 || rows < 2) return;

            // 1. 随机起点
            int rangeX = cols - 1;
            int rangeY = rows - 1;
            int startX = Random.Range(0, rangeX);
            int startY = Random.Range(0, rangeY);

            bool CheckImmutable(VirtualNode node, ref int tgtColor)
            {
                if (node.IsImmutable)
                {
                    if (!condition(node.ConfigId)) return false;

                    if (tgtColor == -1) tgtColor = node.ConfigId;
                    else if (tgtColor != node.ConfigId) return false;
                }

                return true;
            }

            // 2. 遍历棋盘
            for (int i = 0; i < rangeX; i++)
            {
                for (int j = 0; j < rangeY; j++)
                {
                    // 计算当前检查的锚点坐标
                    int x = (startX + i) % rangeX;
                    int y = (startY + j) % rangeY;

                    var n1 = _cachedVBoard[x, y];
                    var n2 = _cachedVBoard[x + 1, y];
                    var n3 = _cachedVBoard[x, y + 1];
                    var n4 = _cachedVBoard[x + 1, y + 1];

                    // 基础检查：必须都有数据
                    if (!n1.HasData || !n2.HasData || !n3.HasData || !n4.HasData) continue;

                    // 策略核心：至少有一个是新棋子
                    if (!n1.IsNew && !n2.IsNew && !n3.IsNew && !n4.IsNew) continue;
                    // 3. 检查旧棋子
                    int targetColor = -1;
                    bool conflict = !CheckImmutable(n1, ref targetColor);
                    if (!conflict && !CheckImmutable(n2, ref targetColor)) conflict = true;
                    if (!conflict && !CheckImmutable(n3, ref targetColor)) conflict = true;
                    if (!conflict && !CheckImmutable(n4, ref targetColor)) conflict = true;

                    if (conflict) continue; // 存在障碍物或颜色冲突，跳过此区域

                    // --- 4. 确定目标颜色 ---
                    if (targetColor == -1)
                    {
                        // 双重保险检查
                        if (condition(n1.ConfigId))
                        {
                            targetColor = n1.ConfigId;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    // --- 5. 执行修改 ---
                    bool isModified = false;

                    if (n1.IsNew && n1.ConfigId != targetColor)
                    {
                        n1.ConfigId = targetColor;
                        isModified = true;
                    }

                    if (n2.IsNew && n2.ConfigId != targetColor)
                    {
                        n2.ConfigId = targetColor;
                        isModified = true;
                    }

                    if (n3.IsNew && n3.ConfigId != targetColor)
                    {
                        n3.ConfigId = targetColor;
                        isModified = true;
                    }

                    if (n4.IsNew && n4.ConfigId != targetColor)
                    {
                        n4.ConfigId = targetColor;
                        isModified = true;
                    }

                    // 只要成功处理完一个合法的 2x2 区域，就退出
                    return;
                }
            }
        }

        /// <summary>
        /// 策略2：获取相邻棋子同色概率的目标值
        /// </summary>
        /// <param name="colorCount">当前关卡颜色总数</param>
        /// <param name="neighborCount">待填充格子的邻居数量(1, 2, 3)</param>
        /// <returns>返回同色生成的概率 (0.0 - 1.0)</returns>
        private float GetNeighborMatchProbability(int colorCount, int neighborCount)
        {
            _activeStrategyLevels.TryGetValue(DifficultyStrategyType.NeighborConsistency, out var level);
            if (level == 0) return 0;

            // 基础概率计算
            // 看同色概率表的话同 公式应该是 P = neighborCount / n.
            float baseProb = (float)neighborCount / colorCount;
            // 难度调节 (调易):
            // 调易 Level 1: Prob = (neighborCount + 1) / n
            // 调易 Level 2: Prob = (neighborCount + 2) / n
            float adjustedProb = (neighborCount + level) * 1.0f / colorCount;

            // 如果 adjustedProb > 1.0 或 接近，处理溢出
            if (adjustedProb >= 1.0f)
            {
                // 如果基础已经是 max，则 + 10%
                adjustedProb = baseProb + (level * 0.1f);
            }

            return Mathf.Clamp(adjustedProb, 0.1f, 0.95f); // 留一点余地
        }

        /// <summary>
        /// 策略4：是否尝试强制生成四方格
        /// </summary>
        private bool IsSquareFormationActive()
        {
            return _activeStrategyLevels.ContainsKey(DifficultyStrategyType.SquareFormation) &&
            _activeStrategyLevels[DifficultyStrategyType.SquareFormation] > 0;
        }

        private void EnsureBuffers(int rows, int cols)
        {
            // 初始化虚拟棋盘数组
            if (_cachedVBoard == null || _cachedVBoard.GetLength(0) != cols || _cachedVBoard.GetLength(1) != rows)
            {
                _cachedVBoard = new VirtualNode[cols, rows];
                for (int x = 0; x < cols; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        _cachedVBoard[x, y] = new VirtualNode();
                    }
                }
            }
            else
            {
                for (int x = 0; x < cols; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        _cachedVBoard[x, y].Clear();
                    }
                }
            }

            // 初始化列缓存列表
            if (_cachedColDrops == null || _cachedColDrops.Length != cols)
            {
                _cachedColDrops = new List<ElementItemData>[cols];
                for (int i = 0; i < cols; i++)
                {
                    _cachedColDrops[i] = new List<ElementItemData>(12); // 预分配容量
                }
            }
            else
            {
                for (int i = 0; i < _cachedColDrops.Length; i++)
                {
                    _cachedColDrops[i].Clear();
                }
            }

            // 初始化辅助列表
            if (_cachedNeighborColors == null) _cachedNeighborColors = new List<int>(4);
            _cachedNeighborColors.Clear();
        }
    }
}