using System;
using System.Collections.Generic;
using Hotfix.Tools.Random;
using HotfixLogic.Match;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 保底掉落策略：基于配置权重的纯随机，也就是基于关卡配置的掉落权重
    /// </summary>
    public class WeightedRandomDropStrategy : IDropStrategy
    {
        private int _cachedLevelId = -1;
        private int[] _cachedDropIds; // 掉落物ID列表
        private int[] _cachedRates;
        private int _totalWeight; // 总权重
        private EcsFilter _inputFilter;
        private List<int> _banList = new List<int>();

        public int GetDropElementId(int col, int row, GameStateContext context,List<DropAnalysisComponent> dropAnalysisComponents)
        {
            // 1. 获取当前关卡数据
            LevelData levelData = context.CurrentLevel;

            // 2. 检查缓存是否有效 (如果是新的一关，或者数据变了，重新构建缓存)
            if (levelData == null || levelData.id != _cachedLevelId)
            {
                RebuildCache(levelData);
            }

            if (_totalWeight <= 0 || _cachedDropIds == null || _cachedDropIds.Length == 0)
            {
                return 0;
            }

            _banList.Clear();
            
            int currentTotalWeight = _totalWeight;
            if (dropAnalysisComponents != null && dropAnalysisComponents.Count > 0)
            {
                
                for (int i = 0; i < dropAnalysisComponents.Count; i++)
                {
                    var component = dropAnalysisComponents[i];
                    if (component.BanDropElementId > 0)
                    {
                        _banList.Add(component.BanDropElementId);
                    }
                }
            }

            bool hasBan = _banList != null && _banList.Count > 0;
            if (hasBan)
            { 
                for (int i = 0; i < _cachedDropIds.Length; i++)
                {
                    if (IsBannedId(_cachedDropIds[i], _banList))
                    {
                        currentTotalWeight -= _cachedRates[i];
                    }
                }
            }
            if (currentTotalWeight <= 0) 
            {
                currentTotalWeight = _totalWeight;
                hasBan = false; 
            }
            
            // 3. 生成随机数
            int rnd = RandomTools.RandomRange(0, currentTotalWeight);

            // 4. 线性遍历寻找
            for (int i = 0; i < _cachedDropIds.Length; i++)
            {
                // 如果被Ban了，直接跳过，不参与权重扣除
                if (hasBan && IsBannedId(_cachedDropIds[i], _banList))
                    continue;

                int rate = _cachedRates[i];
                if (rnd < rate)
                {
                    return _cachedDropIds[i];
                }
                rnd -= rate;
            }

            return _cachedDropIds[^1]; //保底，按理说不会走到这里
        }

        private bool IsBannedId(int id, List<int> banList)
        {
            if(banList == null )
                return false;
            for (int i = 0; i < banList.Count; i++)
            {
                if(banList[i] == id)
                    return true;
            }
            return false;
        }

        private void RebuildCache(LevelData levelData)
        {
            if (levelData == null) return;
            _cachedLevelId = levelData.id;
            _cachedDropIds = levelData.dropColor;
            _cachedRates = levelData.dropColorRate;

            _totalWeight = 0;
            if (_cachedRates != null)
            {
                for (int i = 0; i < _cachedRates.Length; i++)
                {
                    _totalWeight += _cachedRates[i];
                }
            }
        }
    }
}