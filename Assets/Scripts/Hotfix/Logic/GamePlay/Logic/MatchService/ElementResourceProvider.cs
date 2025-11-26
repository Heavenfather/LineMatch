using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Define;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public class ElementResourceProvider : IElementResourceProvider
    {
        private ElementMapDB _db;
        private IMatchService _matchService;
        private HashSet<int> _currentLevelRequiredElementIds;
        private Dictionary<int, IEcsSystem> _providedEcsSystems = new Dictionary<int, IEcsSystem>(50);

        public HashSet<int> CurrentLevelRequiredElementIds => _currentLevelRequiredElementIds;

        public HashSet<int> AnalyzeLevelRequiredElements(LevelData levelData, IMatchService matchService)
        {
            _matchService = matchService;
            _db = ConfigMemoryPool.Get<ElementMapDB>();
            _currentLevelRequiredElementIds ??= new HashSet<int>();
            _currentLevelRequiredElementIds.Clear();
            // 1. 添加基础通用元素 (根据玩法类型可能不同)
            AddCommonElements(_currentLevelRequiredElementIds);

            // 2. 扫描关卡配置的初始颜色 (InitColor)
            if (levelData.initColor != null)
            {
                foreach (var id in levelData.initColor)
                    CollectRecursive(id, _currentLevelRequiredElementIds);
            }

            // 3. 扫描掉落颜色 (DropColor)
            if (levelData.dropColor != null)
            {
                foreach (var id in levelData.dropColor)
                    CollectRecursive(id, _currentLevelRequiredElementIds);
            }

            // 4. 扫描关卡目标 (Target)
            if (levelData.target != null)
            {
                foreach (var target in levelData.target)
                    CollectRecursive(target.targetId, _currentLevelRequiredElementIds);
            }

            // 5. 扫描棋盘布局
            var holdInfos = levelData.BuildElementHoldGridMap();
            foreach (var info in holdInfos)
            {
                CollectRecursive(info.ElementId, _currentLevelRequiredElementIds);
            }

            // 6. 扫描固定掉落配置 (DropFlags)
            if (levelData.dropFlags != null)
            {
                foreach (var flag in levelData.dropFlags)
                {
                    if (flag.dropElements != null)
                    {
                        foreach (var dropEle in flag.dropElements)
                            CollectRecursive(dropEle.elementId, _currentLevelRequiredElementIds);
                    }
                }
            }

            return _currentLevelRequiredElementIds;
        }

        /// <summary>
        /// 递归收集元素及其衍生物（变身、生成物）
        /// 对应旧代码中的 BuildLevelUsingElement
        /// </summary>
        private void CollectRecursive(int elementId, HashSet<int> collectedIds)
        {
            if (elementId <= 0) return;
            if (collectedIds.Contains(elementId)) return;

            // 添加自身
            collectedIds.Add(elementId);

            if (!_db.IsContain(elementId)) return;
            ref readonly ElementMap config = ref _db[elementId];

            // A. 递归查找 NextBlock (消除后变成的元素)
            if (config.nextBlock > 0)
            {
                CollectRecursive(config.nextBlock, collectedIds);
            }

            // B. 查找特殊生成物 (RandomDiffuse 等)
            // 旧代码逻辑：RandomDiffuse 在 extra 字段配置了生成的元素 ID
            if (config.elementType == ElementType.RandomDiffuse)
            {
                if (!string.IsNullOrEmpty(config.extra))
                {
                    var args = config.extra.Split('|');
                    if (args.Length > 0 && int.TryParse(args[0], out int generateId))
                    {
                        CollectRecursive(generateId, collectedIds);
                    }
                }
            }

            // C. 查找其他关联
            // 如果有需要在 extra 中解析的其他关联ID，也在这里处理 TODO....
        }

        private void AddCommonElements(HashSet<int> ids)
        {
            int[] commonSpecials = _matchService.SpecialElements;

            foreach (var id in commonSpecials)
            {
                CollectRecursive(id, ids);
            }
        }

        public async UniTask PreloadResources(HashSet<int> requiredElementIds, Transform poolRoot)
        {
            MatchEffectManager.Instance.ClearCacheKey();
            
            var tasks = new List<UniTask>();
            // 预加载元素
            foreach (var id in requiredElementIds)
            {
                if (!_db.IsContain(id)) continue;
                // 确定池子大小策略 ---后续再走配置？
                int warmCount = MatchConst.BlockElementWarmCount;
                int poolMax = MatchConst.BlockElementMaxCount;
                if (_db[id].elementType == ElementType.Normal)
                {
                    warmCount = MatchConst.NormalElementWarmCount;
                    poolMax = MatchConst.NormalElementMaxCount;
                }

                if (_matchService.IsSpecialElement(id))
                {
                    warmCount = MatchConst.SpecialElementWarmCount;
                    poolMax = MatchConst.SpecialElementMaxCount;
                }

                // 启动创建池子任务
                tasks.Add(ElementObjectPool.Instance.CreatePool(
                    $"Element-{id}",
                    $"{MatchConst.ElementAddressBase}/{_db[id].address}",
                    null,
                    warmCount,poolMax));
                tasks.Add(MatchEffectManager.Instance.PrewarmElementEffects(id, warmCount, poolMax));
            }
            //特效相关的也放到这一起处理了
            tasks.Add(MatchEffectManager.Instance.PrewarmEffects());

            await UniTask.WhenAll(tasks);
        }
    }
}