using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.LitJson;
using GameCore.Singleton;
using Hotfix.Define;
using HotfixCore.Module;
using UnityEngine;
using YooAsset;
using Logger = GameCore.Log.Logger;
using Random = UnityEngine.Random;

namespace HotfixLogic.Match
{
    public class LevelManager : LazySingleton<LevelManager>
    {
        //用户行为系数
        private int _behaviourValue;
        public int BehaviourValue => _behaviourValue;

        //用户分群系数
        private int _groupValue;
        public int GroupValue => _groupValue;

        //当前关卡开启次数
        private int _stagePlayCount;
        public int StagePlayCount => _stagePlayCount;
        
        private int _curLevelId = 0;

        private bool _currentIsCoinLevel = false;
        /// <summary>
        /// 当前关卡是否金币关
        /// </summary>
        public bool IsCoinLevel
        {
            get
            {
                return _currentIsCoinLevel;
            }
        }
        
        private LevelData _curLevelData = null; //外部不要使用这个关卡数据，这里的数据暂时会因为弹出结算弹窗后，而被错误的设置了别的关卡数据
        private LevelDifficulty _levelDifficulty;

        /// <summary>
        /// 关卡难度对应调整比例
        /// </summary>
        private Dictionary<LevelDifficultyType, float> _levelDifficultyValue =
            new Dictionary<LevelDifficultyType, float>()
            {
                { LevelDifficultyType.DropRate, 1.0f / 5.0f },
                { LevelDifficultyType.FillSpecialRate, 1.0f / 2.0f },
                { LevelDifficultyType.ElementLayout, 1.0f / 2.0f }
            };

        public void SetCurrentLevelData(LevelData levelData)
        {
            _curLevelData = levelData;
        }

        public void SetIsCoinLevelState(bool isCoinLevel)
        {
            _currentIsCoinLevel = isCoinLevel;
        }
        
        public void ResetLevelDifficultyValue(int behaviourVal, int groupVal)
        {
            ResetBehaviourValue(behaviourVal);
            ResetGroupValue(groupVal);
        }

        public void ResetBehaviourValue(int behaviourVal)
        {
            _behaviourValue = behaviourVal;
        }

        public void ResetGroupValue(int groupVal)
        {
            _groupValue = groupVal;
        }

        public async UniTask PrepareLevelDifficulty()
        {
            var handle = G.ResourceModule.LoadAssetAsyncHandle<TextAsset>("config/leveldifficulty");
            await handle.ToUniTask();
            var ta = handle.AssetObject as TextAsset;
            if (ta != null)
            {
                _levelDifficulty = JsonMapper.ToObject<LevelDifficulty>(ta.text);
                handle.Release();
            }
        }

        public int GetLevelDifficulty(int levelId, MatchLevelType levelType = MatchLevelType.A)
        {
            if (_levelDifficulty != null)
            {
                if (levelId > 100) levelType = MatchLevelType.B;
                return _levelDifficulty.GetLevelDifficulty(levelId, levelType);
            }

            return 0;
        }

        public void SetCurrentStagePlayCount(int count)
        {
            _stagePlayCount = count;
        }
        
        public async UniTask<LevelData> GetLevel(int levelId)
        {
            if (levelId == _curLevelId && _curLevelData != null) return _curLevelData;

            var location = GetLevelLocation(MatchManager.Instance.CurrentMatchLevelType, levelId);
            var handle = G.ResourceModule.LoadAssetAsyncHandle<TextAsset>(location);

            await handle.ToUniTask();

            var ta = handle.AssetObject as TextAsset;
            if (ta != null)
            {
                var data = JsonMapper.ToObject<LevelData>(ta.text);
                handle.Release();
                return data;
            }

            return null;
        }

        public async UniTask<List<LevelData>> GetGuideLevels(List<int> levelIds)
        {
            if (levelIds == null || levelIds.Count <= 0)
                return null;
            List<LevelData> levels = new List<LevelData>(levelIds.Count + 1);
            string location = "match/levelb/level{0}_guide";
            for (int i = 0; i < levelIds.Count; i++)
            {
                int index = i;
                var levelId = levelIds[index];
                var handle = G.ResourceModule.LoadAssetAsyncHandle<TextAsset>(string.Format(location, levelId));
                await handle.ToUniTask();
                var ta = handle.AssetObject as TextAsset;
                if (ta != null)
                {
                    var data = JsonMapper.ToObject<LevelData>(ta.text);
                    levels.Add(data);
                    handle.Release();
                }
            }
            levels.Sort((a, b) => a.id - b.id);
            return levels;
        }
        
        public async UniTask<LevelData> GetLevel(MatchLevelType levelType, int level)
        {
            var location = GetLevelLocation(levelType, level);
            var handle = G.ResourceModule.LoadAssetAsyncHandle<TextAsset>(location);
            await handle.ToUniTask();
            if (handle.Status == EOperationStatus.Succeed)
            {
                var ta = handle.AssetObject as TextAsset;
                if (ta != null)
                {
                    var data = JsonMapper.ToObject<LevelData>(ta.text);
                    handle.Dispose();
                    return data;
                }
            }

            return null;
        }

        private string GetLevelLocation(MatchLevelType levelType, int level)
        {
            if(level > 100 || levelType == MatchLevelType.Editor)
                levelType = MatchLevelType.B;
            if (level == MatchManager.Instance.MaxLevel)
            {
                //金币关
                bool isCoinLevel = _currentIsCoinLevel;
                if (isCoinLevel)
                    return $"match/levelb/level{level}_coin";
            }

            return $"match/level{levelType.ToString().ToLower()}/level{level}";
        }
        
        /// <summary>
        /// 修改关卡掉落概率
        /// </summary>
        /// <param name="data"></param>
        /// <param name="modifyData"></param>
        public void ModifyLevelDropRate(ref LevelData data, LevelDifficultyModifyData modifyData)
        {
            if (modifyData.DifficultyType != LevelDifficultyType.DropRate)
                return;
            //true为增加难度
            bool isHarder = modifyData.IsHarder;
            //转换成万分比
            int modifyRate = Mathf.FloorToInt(modifyData.ModifyRate * 10000);

            AdjustProbabilityArray(ref data.initColorRate, isHarder, modifyRate);
            AdjustProbabilityArray(ref data.dropColorRate, isHarder, modifyRate);
        }

        /// <summary>
        /// 修改关卡生成功能棋子相邻的概率
        /// </summary>
        /// <param name="rates"></param>
        /// <param name="modifyData"></param>
        public void ModifyGenerateItemRate(ref int[] rates, LevelDifficultyModifyData modifyData)
        {
            if (rates == null || rates.Length != 3) return;
            if (modifyData.DifficultyType != LevelDifficultyType.FillSpecialRate)
                return;

            bool isHarder = modifyData.IsHarder;
            // 转为百分比
            int modifyRate = Mathf.FloorToInt(modifyData.ModifyRate * 100);

            // 验证概率总和（应为100%）
            int total = rates[0] + rates[1] + rates[2];
            if (total != 100)
            {
                NormalizeRates(ref rates);
            }

            if (isHarder)
            {
                // 增加难度：减少后两个元素的值，加到第一个元素
                IncreaseDifficulty(ref rates, modifyRate);
            }
            else
            {
                // 降低难度：减少第一个元素的值，平均加到后两个元素
                DecreaseDifficulty(ref rates, modifyRate);
            }

            // 最终验证并修正概率
            NormalizeRates(ref rates);
        }

        private void IncreaseDifficulty(ref int[] rates, int modifyRate)
        {
            // 计算后两个元素总共可减少的最大值（不能小于0）
            int availableReduction = rates[1] + rates[2];
            if (availableReduction <= 0) return;

            // 确定实际减少量（不超过可减少量）
            int actualReduction = Mathf.Min(modifyRate, availableReduction);

            // 按比例减少后两个元素
            float reductionRatio = (float)actualReduction / availableReduction;
            int reduction1 = Mathf.RoundToInt(rates[1] * reductionRatio);
            int reduction2 = actualReduction - reduction1; // 确保总和正确

            // 应用减少（确保不会变成负数）
            reduction1 = Mathf.Min(reduction1, rates[1]);
            reduction2 = Mathf.Min(reduction2, rates[2]);

            rates[1] -= reduction1;
            rates[2] -= reduction2;

            // 将减少的值加到第一个元素
            rates[0] += (reduction1 + reduction2);
        }

        private void DecreaseDifficulty(ref int[] rates, int modifyRate)
        {
            // 计算第一个元素可减少的最大值（不能小于0）
            int availableReduction = rates[0];
            if (availableReduction <= 0) return;

            // 确定实际减少量（不超过可减少量）
            int actualReduction = Mathf.Min(modifyRate, availableReduction);

            // 减少第一个元素
            rates[0] -= actualReduction;

            // 将减少的值平均分配到后两个元素
            int add1 = actualReduction / 2; // 第一个元素分得的值
            int add2 = actualReduction - add1; // 第二个元素分得的值（处理奇数情况）

            rates[1] += add1;
            rates[2] += add2;
        }

        // 确保概率总和为100%
        private void NormalizeRates(ref int[] rates)
        {
            int total = rates[0] + rates[1] + rates[2];
            int diff = 100 - total;

            if (diff == 0) return;

            // 根据难度方向决定如何分配差异
            if (diff > 0)
            {
                // 总和不足100，需要增加
                int addIndex = (rates[0] > rates[1] && rates[0] > rates[2]) ? 0 : (rates[1] > rates[2] ? 1 : 2);
                rates[addIndex] += diff;
            }
            else
            {
                // 总和超过100，需要减少
                int reduceIndex = (rates[0] < rates[1] && rates[0] < rates[2]) ? 0 : (rates[1] < rates[2] ? 1 : 2);
                rates[reduceIndex] = Mathf.Max(0, rates[reduceIndex] + diff);
            }

            // 递归调用直到总和为100（最多递归3次）
            total = rates[0] + rates[1] + rates[2];
            if (total != 100 && Mathf.Abs(total - 100) < 10)
            {
                NormalizeRates(ref rates);
            }
        }

        private void AdjustProbabilityArray(ref int[] rates, bool isHarder, int modifyRate)
        {
            if (rates == null || rates.Length == 0) return;

            // 计算当前概率总和（确保是万分比）
            int total = 0;
            foreach (int rate in rates) total += rate;

            // 创建索引数组用于跟踪原始位置
            int[] indices = new int[rates.Length];
            for (int i = 0; i < indices.Length; i++) indices[i] = i;

            // 根据调整方向选择排序方式
            if (isHarder)
            {
                // 增加难度：按概率升序排序（小概率在前）
                Array.Sort(rates, indices);
            }
            else
            {
                // 降低难度：按概率降序排序（大概率在前）
                Array.Sort(rates, (a, b) => b.CompareTo(a));
                Array.Reverse(indices); // 保持索引对应关系
            }

            // 计算最大可调整量
            int availableAdjustment = 0;
            if (isHarder)
            {
                // 增加难度：取最高概率和最低概率差值的一半
                availableAdjustment = Mathf.Min(modifyRate, (rates[rates.Length - 1] - rates[0]) / 2);
            }
            else
            {
                // 降低难度：取最低概率值（不能减到负数）
                availableAdjustment = Mathf.Min(modifyRate, rates[rates.Length - 1]);
            }

            // 计算调整步长
            int adjustmentStep = Mathf.Max(1, availableAdjustment / rates.Length);

            // 执行调整
            int remainingAdjustment = availableAdjustment;
            while (remainingAdjustment > 0)
            {
                // 计算本次实际调整量（不能超过剩余量）
                int currentStep = Mathf.Min(adjustmentStep, remainingAdjustment);

                if (isHarder)
                {
                    // 增加难度：从高概率向低概率转移
                    rates[^1] -= currentStep; // 减少最高概率
                    rates[0] += currentStep; // 增加最低概率
                }
                else
                {
                    // 降低难度：从低概率向高概率转移
                    rates[^1] -= currentStep; // 减少最低概率
                    rates[0] += currentStep; // 增加最高概率
                }

                remainingAdjustment -= currentStep;

                // 重新排序以更新最高/最低概率位置
                if (isHarder)
                {
                    Array.Sort(rates, indices);
                }
                else
                {
                    Array.Sort(rates, (a, b) => b.CompareTo(a));
                }
            }

            // 恢复原始顺序
            int[] temp = new int[rates.Length];
            Array.Copy(rates, temp, rates.Length);
            for (int i = 0; i < indices.Length; i++)
            {
                rates[indices[i]] = temp[i];
            }

            // 验证并修正总和
            int newTotal = 0;
            foreach (int rate in rates) newTotal += rate;

            // 如果有微小差异，均匀分配到所有概率上
            if (newTotal != total)
            {
                int diff = total - newTotal;
                int perElementAdjust = diff / rates.Length;
                int remainder = diff % rates.Length;

                for (int i = 0; i < rates.Length; i++)
                {
                    rates[i] += perElementAdjust;
                    if (i < remainder) rates[i] += 1;
                }
            }
        }

        // 配置转移缓存结构
        private struct ConfigTransfer
        {
            public int fromConfigId;
            public int toConfigId;
            public int count;
        }

        /// <summary>
        /// 调整掉落元素布局，使其更分散或更集中
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="modifyData"></param>
        public void AdjustElementDropLayout(ref List<ElementItemData> elements, LevelDifficultyModifyData modifyData)
        {
            if (elements == null || elements.Count == 0) return;
            if (modifyData.DifficultyType != LevelDifficultyType.ElementLayout)
                return;

            bool isHarder = modifyData.IsHarder;
            float adjustRate = modifyData.ModifyRate;

            // 创建配置组字典
            Dictionary<int, List<ElementItemData>> configGroups = CreateConfigGroups(elements);

            // 计算需要调整的元素数量
            int totalAdjustments = Mathf.CeilToInt(elements.Count * adjustRate);

            if (isHarder)
            {
                // 增加难度：使配置分布更平均
                MakeConfigDistributionMoreUniform(configGroups, totalAdjustments);
            }
            else
            {
                // 降低难度：使配置分布更集中
                MakeConfigDistributionMoreConcentrated(configGroups, totalAdjustments);
            }

            AdjustPositionsBasedOnNewConfig(elements, isHarder, totalAdjustments);
        }

        // 创建配置组字典
        private Dictionary<int, List<ElementItemData>> CreateConfigGroups(List<ElementItemData> elements)
        {
            var configGroups = new Dictionary<int, List<ElementItemData>>();

            for (int i = 0; i < elements.Count; i++)
            {
                ElementItemData element = elements[i];
                int configId = element.ConfigId;

                if (!configGroups.TryGetValue(configId, out var group))
                {
                    group = new List<ElementItemData>();
                    configGroups[configId] = group;
                }

                group.Add(element);
            }

            return configGroups;
        }

        // 使配置分布更平均（增加难度）
        private void MakeConfigDistributionMoreUniform(
            Dictionary<int, List<ElementItemData>> configGroups,
            int maxAdjustments)
        {
            // 计算当前配置分布和平均值
            int totalCount = 0;
            var configCounts = new List<KeyValuePair<int, int>>(configGroups.Count);

            foreach (var kvp in configGroups)
            {
                int count = kvp.Value.Count;
                configCounts.Add(new KeyValuePair<int, int>(kvp.Key, count));
                totalCount += count;
            }

            float averageCount = (float)totalCount / configCounts.Count;

            // 排序（从大到小）
            configCounts.Sort((a, b) => b.Value.CompareTo(a.Value));

            // 识别大组和小组
            var largeGroups = new List<KeyValuePair<int, int>>();
            var smallGroups = new List<KeyValuePair<int, int>>();

            for (int i = 0; i < configCounts.Count; i++)
            {
                var config = configCounts[i];
                if (config.Value > averageCount)
                {
                    largeGroups.Add(config);
                }
                else if (config.Value < averageCount)
                {
                    smallGroups.Add(config);
                }
            }

            // 如果没有可调整的组，直接返回
            if (largeGroups.Count == 0 || smallGroups.Count == 0) return;

            // 计算转移计划
            var transfers = new List<ConfigTransfer>();
            int adjustments = 0;

            while (adjustments < maxAdjustments && largeGroups.Count > 0 && smallGroups.Count > 0)
            {
                // 选择最大的组和最小的组
                var largestGroup = largeGroups[0];
                var smallestGroup = smallGroups[^1];

                // 计算从大组到小组的转移量
                int maxTransfer = largestGroup.Value - (int)averageCount; // 大组可减少量
                maxTransfer = Mathf.Min(maxTransfer, (int)averageCount - smallestGroup.Value); // 小组可增加量
                maxTransfer = Mathf.Min(maxTransfer, maxAdjustments - adjustments); // 剩余调整量

                if (maxTransfer <= 0)
                {
                    // 移除无法再调整的组
                    largeGroups.RemoveAt(0);
                    continue;
                }

                // 实际转移量（至少1个）
                int transferCount = Mathf.Max(1, maxTransfer);

                // 记录转移计划
                transfers.Add(new ConfigTransfer
                {
                    fromConfigId = largestGroup.Key,
                    toConfigId = smallestGroup.Key,
                    count = transferCount
                });

                adjustments += transferCount;

                // 更新组信息（避免创建新对象）
                for (int i = 0; i < largeGroups.Count; i++)
                {
                    if (largeGroups[i].Key == largestGroup.Key)
                    {
                        var updated = new KeyValuePair<int, int>(
                            largestGroup.Key,
                            largestGroup.Value - transferCount);
                        largeGroups[i] = updated;
                        break;
                    }
                }

                for (int i = 0; i < smallGroups.Count; i++)
                {
                    if (smallGroups[i].Key == smallestGroup.Key)
                    {
                        var updated = new KeyValuePair<int, int>(
                            smallestGroup.Key,
                            smallestGroup.Value + transferCount);
                        smallGroups[i] = updated;
                        break;
                    }
                }

                // 重新排序（仅当必要）
                if (largeGroups.Count > 1 && largeGroups[0].Value < largeGroups[1].Value)
                {
                    largeGroups.Sort((a, b) => b.Value.CompareTo(a.Value));
                }

                if (smallGroups.Count > 1 &&
                    smallGroups[smallGroups.Count - 1].Value > smallGroups[smallGroups.Count - 2].Value)
                {
                    smallGroups.Sort((a, b) => b.Value.CompareTo(a.Value));
                }
            }

            // 执行所有转移
            ExecuteTransfers(configGroups, transfers);
        }

        // 使配置分布更集中（降低难度）
        private void MakeConfigDistributionMoreConcentrated(
            Dictionary<int, List<ElementItemData>> configGroups,
            int maxAdjustments)
        {
            // 找到最大的组
            int targetConfigId = -1;
            int maxCount = 0;

            foreach (var kvp in configGroups)
            {
                if (kvp.Value.Count > maxCount)
                {
                    maxCount = kvp.Value.Count;
                    targetConfigId = kvp.Key;
                }
            }

            if (targetConfigId == -1) return;

            // 识别其他配置组
            var transfers = new List<ConfigTransfer>();
            int adjustments = 0;

            foreach (var kvp in configGroups)
            {
                if (adjustments >= maxAdjustments) break;
                if (kvp.Key == targetConfigId) continue;

                int groupCount = kvp.Value.Count;
                int maxTransfer = Mathf.Min(groupCount, maxAdjustments - adjustments);

                if (maxTransfer <= 0) continue;

                // 记录转移计划
                transfers.Add(new ConfigTransfer
                {
                    fromConfigId = kvp.Key,
                    toConfigId = targetConfigId,
                    count = maxTransfer
                });

                adjustments += maxTransfer;
            }

            // 执行所有转移
            ExecuteTransfers(configGroups, transfers);
        }

        // 执行批量转移
        private void ExecuteTransfers(
            Dictionary<int, List<ElementItemData>> configGroups,
            List<ConfigTransfer> transfers)
        {
            for (int i = 0; i < transfers.Count; i++)
            {
                ConfigTransfer transfer = transfers[i];

                if (!configGroups.TryGetValue(transfer.fromConfigId, out var fromGroup)) continue;
                if (!configGroups.TryGetValue(transfer.toConfigId, out var toGroup)) continue;

                int transferCount = Mathf.Min(transfer.count, fromGroup.Count);

                // 随机选择要转移的元素
                for (int j = 0; j < transferCount; j++)
                {
                    if (fromGroup.Count == 0) break;

                    int randomIndex = Random.Range(0, fromGroup.Count);
                    ElementItemData element = fromGroup[randomIndex];

                    // 更新元素的配置ID
                    element.ConfigId = transfer.toConfigId;

                    // 移动到新组
                    toGroup.Add(element);
                    fromGroup.RemoveAt(randomIndex);
                }
            }
        }

        // 可选的位置调整算法
        private void AdjustPositionsBasedOnNewConfig(
            List<ElementItemData> elements,
            bool isHarder,
            int maxAdjustments)
        {
            // 创建位置字典
            Dictionary<Vector2Int, ElementItemData> positionMap = new Dictionary<Vector2Int, ElementItemData>();
            for (int i = 0; i < elements.Count; i++)
            {
                positionMap[elements[i].GridPos] = elements[i];
            }

            // 创建配置组
            Dictionary<int, List<ElementItemData>> configGroups = CreateConfigGroups(elements);

            if (isHarder)
            {
                // 增加难度：使元素更加分散
                MakeElementsMoreDispersed(positionMap, configGroups, maxAdjustments);
            }
            else
            {
                // 降低难度：使元素更加聚集
                MakeElementsMoreClustered(positionMap, configGroups, maxAdjustments);
            }
        }

        // 分散算法
        private void MakeElementsMoreDispersed(
            Dictionary<Vector2Int, ElementItemData> positionMap,
            Dictionary<int, List<ElementItemData>> configGroups,
            int maxAdjustments)
        {
            // 识别所有需要分散的元素（按聚集程度排序）
            var allElementsToDisperse = new List<ElementItemData>();

            // 遍历所有配置组
            foreach (var group in configGroups.Values)
            {
                if (group.Count < 2) continue;

                // 遍历组内所有元素
                for (int i = 0; i < group.Count; i++)
                {
                    ElementItemData element = group[i];
                    int clusterScore = CountAdjacentSameConfig(element, element.ConfigId, positionMap);
                    if (clusterScore > 0)
                    {
                        allElementsToDisperse.Add(element);
                    }
                }
            }

            // 按聚集程度从高到低排序
            allElementsToDisperse.Sort((a, b) =>
                CountAdjacentSameConfig(b, b.ConfigId, positionMap).CompareTo(
                    CountAdjacentSameConfig(a, a.ConfigId, positionMap)));

            // 分散策略
            int adjustments = 0;
            for (int i = 0; i < allElementsToDisperse.Count; i++)
            {
                if (adjustments >= maxAdjustments) break;

                ElementItemData element = allElementsToDisperse[i];

                // 寻找最佳分散位置
                Vector2Int? bestDispersePos = FindBestDispersePosition(
                    element.GridPos,
                    element.ConfigId,
                    positionMap);

                if (bestDispersePos.HasValue)
                {
                    // 交换位置
                    SwapElementPos(positionMap[element.GridPos], positionMap[bestDispersePos.Value]);
                    adjustments++;
                }
            }
        }

        // 聚集算法
        private void MakeElementsMoreClustered(
            Dictionary<Vector2Int, ElementItemData> positionMap,
            Dictionary<int, List<ElementItemData>> configGroups,
            int maxAdjustments)
        {
            int adjustments = 0;

            // 优先处理元素数量多的组
            var configIds = new List<int>(configGroups.Keys);
            configIds.Sort((a, b) => configGroups[b].Count.CompareTo(configGroups[a].Count));

            foreach (int configId in configIds)
            {
                if (adjustments >= maxAdjustments) break;

                List<ElementItemData> group = configGroups[configId];
                if (group.Count < 2) continue;

                // 计算组的中心位置
                Vector2Int center = Vector2Int.zero;
                for (int i = 0; i < group.Count; i++)
                {
                    center += group[i].GridPos;
                }

                center /= group.Count;

                // 按距离中心远近排序（最远的先处理）
                group.Sort((a, b) =>
                    Vector2Int.Distance(b.GridPos, center).CompareTo(Vector2Int.Distance(a.GridPos, center)));

                // 处理每个元素
                for (int i = 0; i < group.Count; i++)
                {
                    if (adjustments >= maxAdjustments) break;

                    ElementItemData element = group[i];

                    // 如果已经有相邻的同组元素，跳过
                    if (HasAdjacentSameConfig(element, configId, positionMap)) continue;

                    // 寻找最佳聚集位置
                    Vector2Int? bestClusterPos = FindBestClusterPosition(
                        element.GridPos,
                        configId,
                        positionMap,
                        center);

                    if (bestClusterPos.HasValue)
                    {
                        SwapElementPos(positionMap[element.GridPos], positionMap[bestClusterPos.Value]);
                        adjustments++;
                    }
                }
            }
        }

        // 寻找最佳分散位置
        private Vector2Int? FindBestDispersePosition(
            Vector2Int currentPos,
            int configId,
            Dictionary<Vector2Int, ElementItemData> positionMap)
        {
            float bestScore = float.MinValue;
            Vector2Int? bestPos = null;

            // 遍历所有位置
            foreach (var candidate in positionMap)
            {
                Vector2Int pos = candidate.Key;
                ElementItemData element = candidate.Value;

                // 跳过自身位置
                if (pos == currentPos) continue;

                // 跳过同配置元素的位置
                if (element.ConfigId == configId) continue;

                // 计算分散分数
                float disperseScore = 0f;

                // 检查候选位置是否有相邻的同配置元素
                bool hasAdjacentSame = false;
                int adjacentSameCount = 0;

                for (int i = 0; i < ValidateManager.Instance.NeighborDirs.Length; i++)
                {
                    Vector2Int neighborPos = pos + ValidateManager.Instance.NeighborDirs[i];
                    if (positionMap.TryGetValue(neighborPos, out ElementItemData neighbor) &&
                        neighbor.ConfigId == configId)
                    {
                        hasAdjacentSame = true;
                        adjacentSameCount++;
                    }
                }

                // 如果没有相邻同配置元素，给高分
                if (!hasAdjacentSame)
                {
                    disperseScore += 100f;
                }
                else
                {
                    // 如果有相邻，给负分
                    disperseScore -= adjacentSameCount * 50f;
                }

                // 远离当前元素（避免交换到附近位置）
                float distanceToCurrent = Vector2.Distance(pos, currentPos);
                disperseScore += distanceToCurrent * 2f;

                // 确保不会形成新的聚集
                bool createsNewCluster = false;
                for (int i = 0; i < ValidateManager.Instance.NeighborDirs.Length; i++)
                {
                    Vector2Int neighborPos = pos + ValidateManager.Instance.NeighborDirs[i];
                    if (positionMap.TryGetValue(neighborPos, out ElementItemData neighbor) &&
                        neighbor.ConfigId == configId)
                    {
                        createsNewCluster = true;
                        break;
                    }
                }

                if (createsNewCluster)
                {
                    disperseScore -= 200f;
                }

                // 更新最佳位置
                if (disperseScore > bestScore)
                {
                    bestScore = disperseScore;
                    bestPos = pos;
                }
            }

            return bestScore > 0 ? bestPos : null;
        }

        // 寻找最佳聚集位置
        private Vector2Int? FindBestClusterPosition(
            Vector2Int currentPos,
            int configId,
            Dictionary<Vector2Int, ElementItemData> positionMap,
            Vector2Int center)
        {
            float bestScore = float.MinValue;
            Vector2Int? bestPos = null;

            // 遍历所有位置
            foreach (var candidate in positionMap)
            {
                Vector2Int pos = candidate.Key;
                ElementItemData element = candidate.Value;

                // 跳过自身位置
                if (pos == currentPos) continue;

                // 跳过同配置元素的位置
                if (element.ConfigId == configId) continue;

                // 计算聚集分数
                float clusterScore = 0f;

                // 检查候选位置是否有相邻的同配置元素
                bool hasAdjacentSame = false;
                for (int i = 0; i < ValidateManager.Instance.NeighborDirs.Length; i++)
                {
                    Vector2Int neighborPos = pos + ValidateManager.Instance.NeighborDirs[i];
                    if (positionMap.TryGetValue(neighborPos, out ElementItemData neighbor) &&
                        neighbor.ConfigId == configId)
                    {
                        clusterScore += 5f;
                        hasAdjacentSame = true;
                    }
                }

                // 靠近中心位置加分
                float distanceToCenter = Vector2.Distance(pos, center);
                clusterScore += 10f / (distanceToCenter + 1f);

                // 如果当前元素移动到此位置后会有相邻同配置元素，额外加分
                if (hasAdjacentSame)
                {
                    clusterScore += 3f;
                }

                // 更新最佳位置
                if (clusterScore > bestScore)
                {
                    bestScore = clusterScore;
                    bestPos = pos;
                }
            }

            return bestPos;
        }

        // 检查元素是否有相邻的同配置元素
        private bool HasAdjacentSameConfig(ElementItemData element, int configId,
            Dictionary<Vector2Int, ElementItemData> positionMap)
        {
            for (int i = 0; i < ValidateManager.Instance.NeighborDirs.Length; i++)
            {
                Vector2Int neighborPos = element.GridPos + ValidateManager.Instance.NeighborDirs[i];
                if (positionMap.TryGetValue(neighborPos, out ElementItemData neighbor) &&
                    neighbor.ConfigId == configId)
                {
                    return true;
                }
            }

            return false;
        }

        // 计算相邻的同配置元素数量
        private int CountAdjacentSameConfig(ElementItemData element, int configId,
            Dictionary<Vector2Int, ElementItemData> positionMap)
        {
            int count = 0;
            for (int i = 0; i < ValidateManager.Instance.NeighborDirs.Length; i++)
            {
                Vector2Int neighborPos = element.GridPos + ValidateManager.Instance.NeighborDirs[i];
                if (positionMap.TryGetValue(neighborPos, out ElementItemData neighbor) &&
                    neighbor.ConfigId == configId)
                {
                    count++;
                }
            }

            return count;
        }

        // 交换两个元素的ConfigId
        private void SwapElementPos(ElementItemData a, ElementItemData b)
        {
            Vector2Int temp = new Vector2Int(a.GridPos.x, a.GridPos.y);
            a.UpdatePos(b.GridPos);
            b.UpdatePos(temp);
        }

        /// <summary>
        /// 计算动态难度变化概率
        /// </summary>
        /// <returns></returns>
        public LevelDifficultyModifyData CalDifficultyChangeRate()
        {
            float value = CalDynamicDifficultyValue();
            value -= 1;
            if (value == 0.0f)
            {
                return new LevelDifficultyModifyData()
                    { DifficultyType = LevelDifficultyType.None, IsHarder = false, ModifyRate = 0 };
            }

            LevelDifficultyType type = GetRandomDifficultyType(_stagePlayCount);
            //随机一个难度变更类型
            float rate = _levelDifficultyValue[type];
            float percent = Math.Abs(value); //转换为百分比
            return new LevelDifficultyModifyData()
            {
                DifficultyType = type,
                IsHarder = value > 0,
                ModifyRate = percent * rate
            };
        }
        
        /// <summary>
        /// 根据次数值随机获取关卡难度类型
        /// </summary>
        /// <returns>随机到的关卡难度类型</returns>
        private LevelDifficultyType GetRandomDifficultyType(int count)
        {
            // 根据次数值确定ElementLayout的概率
            float elementLayoutProbability = 0f;
    
            if (count <= 3)
            {
                // 纯随机，每个类型概率相等
                int random = Random.Range((int)LevelDifficultyType.DropRate, (int)LevelDifficultyType.Last);
                return (LevelDifficultyType)random;
            }

            if (count == 4)
            {
                elementLayoutProbability = 0.3f;
            }
            if (count == 5)
            {
                elementLayoutProbability = 0.45f;
            }

            if (count >= 6)
            {
                elementLayoutProbability = 0.6f;
            }

            // 对于4和5次的情况，使用权重随机
            float rand = Random.value;
    
            if (rand < elementLayoutProbability)
            {
                return LevelDifficultyType.ElementLayout;
            }
            else
            {
                // 剩余概率平均分配给DropRate和FillSpecialRate
                float remainingProbability = 1f - elementLayoutProbability;
                float halfRemaining = remainingProbability / 2f;
        
                if (rand < elementLayoutProbability + halfRemaining)
                {
                    return LevelDifficultyType.DropRate;
                }
                else
                {
                    return LevelDifficultyType.FillSpecialRate;
                }
            }
        }
        
        /// <summary>
        /// 关卡最终难度系数
        /// </summary>
        /// <returns></returns>
        private float CalDynamicDifficultyValue()
        {
            //最终难度系数=(1+用户行为系数)*(1+用户分群系数)
            float value = (1 + _behaviourValue / 100.0f) * (1 + _groupValue / 100.0f);
            return value;
        }
    }
}