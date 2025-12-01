using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using HotfixLogic;
using HotfixLogic.Match;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 引导控制器
    /// 负责引导的整体流程控制和与旧系统的桥接
    /// </summary>
    public class GuideController
    {
        private GuideVisualSystem _visualSystem;
        private GuideInputFilterSystem _inputFilterSystem;
        private GameStateContext _context;
        
        // 引导关卡映射
        private Dictionary<int, List<int>> _levelAGuideLevelMap = new Dictionary<int, List<int>>()
        {
            [1] = new List<int>() { 1, 2, 4 },
        };
        
        private Dictionary<int, List<int>> _levelBGuideLevelMap = new Dictionary<int, List<int>>()
        {
            [1] = new List<int> { 1, 2, 4 },
            [3] = new List<int> { 5 },
            [6] = new List<int> { 6 },
        };
        
        private Queue<LevelData> _guideLevels = new Queue<LevelData>();
        private int _guideLevelAttemptCount = 0;
        
        public GuideController(GameStateContext context)
        {
            _context = context;
        }
        
        public void SetSystems(GuideVisualSystem visualSystem, GuideInputFilterSystem inputFilterSystem)
        {
            _visualSystem = visualSystem;
            _inputFilterSystem = inputFilterSystem;
        }
        
        /// <summary>
        /// 播放消除引导
        /// </summary>
        public void PlayMatchGuide(LevelData levelData, MatchLevelType levelType)
        {
            // 检查是否需要引导
            if (!GuideManager.Instance.IsGuiding())
            {
                return;
            }
            
            int guideId = GuideManager.Instance.CurrentGuideId;
            GuideConfigDB db = ConfigMemoryPool.Get<GuideConfigDB>();
            ref readonly GuideConfig config = ref db[guideId];
            
            // 触发引导
            TriggerGuide(guideId, config);
        }
        
        /// <summary>
        /// 触发引导
        /// </summary>
        public void TriggerGuide(int guideId, GuideConfig config)
        {
            Logger.Debug($"触发引导: ID={guideId}, Type={config.guideType}");
            
            GuideType guideType = config.guideType == GameConfig.GuideType.Weak ? GuideType.Weak : GuideType.Force;
            
            _visualSystem?.StartGuide(guideId, guideType, config.guideParameters, config.guideParameters2);
        }
        
        /// <summary>
        /// 完成当前引导
        /// </summary>
        public void FinishCurrentGuide()
        {
            _visualSystem?.StopGuide();
            
            // 触发引导完成事件
            G.EventModule.DispatchEvent(GameEventDefine.OnGuideFinish, 
                EventOneParam<int>.Create(GuideManager.Instance.CurrentGuideId));
        }
        
        /// <summary>
        /// 检查是否需要引导关卡
        /// </summary>
        public bool TryGetGuideLevels(LevelData currentLevel, MatchLevelType levelType, out List<int> guideLevelIds)
        {
            guideLevelIds = null;
            
            int maxLevel = MatchManager.Instance.MaxLevel;
            if (maxLevel > currentLevel.id)
            {
                return false;
            }
            
            Dictionary<int, List<int>> guideLevelMap = levelType == MatchLevelType.A 
                ? _levelAGuideLevelMap 
                : _levelBGuideLevelMap;
            
            if (guideLevelMap.TryGetValue(currentLevel.id, out guideLevelIds))
            {
                string key = levelType == MatchLevelType.A
                    ? GamePrefsKey.MatchALevelGuideFinish
                    : GamePrefsKey.MatchBLevelGuideFinish;
                
                int saveValue = PlayerPrefsUtil.GetInt($"{key}_{currentLevel.id}", 0);
                return saveValue == 0;
            }
            
            return false;
        }
        
        /// <summary>
        /// 加载引导关卡
        /// </summary>
        public async UniTask<List<LevelData>> LoadGuideLevels(List<int> guideLevelIds)
        {
            List<LevelData> guideLevels = new List<LevelData>();
            
            foreach (var levelId in guideLevelIds)
            {
                var levelData = await LevelManager.Instance.GetLevel(MatchLevelType.A, levelId);
                if (levelData != null)
                {
                    guideLevels.Add(levelData);
                }
            }
            
            return guideLevels;
        }
        
        /// <summary>
        /// 开始引导关卡
        /// </summary>
        public void StartGuideLevel(LevelData levelData)
        {
            _guideLevels.Enqueue(levelData);
            _guideLevelAttemptCount = 0;
            
            // 触发引导关卡开始事件
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchGuideLevelStartFinish, 
                EventOneParam<LevelData>.Create(levelData));
        }
        
        /// <summary>
        /// 引导关卡步骤完成
        /// </summary>
        public void OnGuideLevelStepComplete()
        {
            if (_guideLevels.Count == 0) return;
            
            var currentLevel = _guideLevels.Dequeue();
            
            // 触发步骤完成事件
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchStepComplete, 
                EventOneParam<int>.Create(currentLevel.id));
            
            _guideLevelAttemptCount = 0;
            
            // 如果还有引导关卡，继续下一个
            if (_guideLevels.Count > 0)
            {
                var nextLevel = _guideLevels.Peek();
                StartGuideLevel(nextLevel);
            }
            else
            {
                // 所有引导关卡完成
                OnAllGuideLevelsFinish();
            }
        }
        
        /// <summary>
        /// 所有引导关卡完成
        /// </summary>
        private void OnAllGuideLevelsFinish()
        {
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchGuideLevelAllFinish);
        }
        
        /// <summary>
        /// 记录引导关卡尝试次数
        /// </summary>
        public void RecordGuideLevelAttempt(int levelId, int step)
        {
            _guideLevelAttemptCount++;
            
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchGuideLevelAttemptStep, 
                EventTwoParam<int, int>.Create(levelId, step));
        }
        
        /// <summary>
        /// 检查坐标是否在引导路径中
        /// </summary>
        public bool IsCoordInGuidePath(Vector2Int coord)
        {
            return _inputFilterSystem?.IsCoordInGuidePath(coord) ?? true;
        }
        
        /// <summary>
        /// 获取引导关卡ID对应的引导ID
        /// </summary>
        public int GetGuideIdByGuideLevelId(int levelId)
        {
            if (levelId >= 1 && levelId <= 6)
            {
                // 这里可以根据实际配置映射
                return levelId + 100; // 示例映射
            }
            return 0;
        }
    }
}
