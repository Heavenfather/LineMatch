using System.Collections.Generic;
using GameConfig;
using GameCore.Singleton;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Module;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class LevelTargetSystem : LazySingleton<LevelTargetSystem>
    {
        /// <summary>
        /// 剩余消除目标数量 id=>num
        /// </summary>
        private Dictionary<int, int> _targetElements;

        private Dictionary<int, int> _originalTargets;
        
        public Dictionary<int, int> TargetElements => _targetElements;

        public Dictionary<int, int> TempTargetElements;

        private Dictionary<int, int> _randomPopElementCounting;
        
        /// <summary>
        /// 初始化关卡目标
        /// </summary>
        public void ResetLevelTarget(LevelData levelData,bool notifyWindow = false)
        {
            _targetElements ??= new Dictionary<int, int>();
            _targetElements.Clear();
            _randomPopElementCounting ??= new Dictionary<int, int>();
            _randomPopElementCounting.Clear();
            
            for (int i = 0; i < levelData.target.Length; i++)
            {
                if (_targetElements.ContainsKey(levelData.target[i].targetId))
                    _targetElements[levelData.target[i].targetId] += levelData.target[i].targetNum;
                else
                    _targetElements.TryAdd(levelData.target[i].targetId, levelData.target[i].targetNum);
            }

            TempTargetElements = new Dictionary<int, int>(_targetElements);
            _originalTargets = new Dictionary<int, int>(_targetElements);

            if (notifyWindow)
                TickTargetChangedNum(true);
        }

        /// <summary>
        /// 添加目标
        /// </summary>
        public void AddTarget(int targetId, int count)
        {
            if (!_targetElements.ContainsKey(targetId))
            {
                _targetElements.Add(targetId, count);
            }
        }

        /// <summary>
        /// 统计消除目标
        /// </summary>
        /// <param name="result"></param>
        public void CalculateTarget(Dictionary<int, int> result)
        {
            if (result == null || result.Count <= 0)
            {
                return;
            }

            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            foreach (var kp in result)
            {
                if (_targetElements.ContainsKey(kp.Key))
                {
                    if (db.IsCoinTypeElement(kp.Key))
                    {
                        //金币类型需要往上加
                        _targetElements[kp.Key] += kp.Value;
                    }
                    else
                    {
                        _targetElements[kp.Key] -= kp.Value;
                    }
                }
            }

            FixTempTargetNum();
            TickTargetChangedNum();
        }

        /// <summary>
        /// 动态添加目标数量
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="count"></param>
        public void AddTargetNum(int targetId, int count)
        {
            if (_targetElements.ContainsKey(targetId))
                _targetElements[targetId] += count;
            else
                _targetElements.TryAdd(targetId, count);
            TickTargetChangedNum();
        }

        /// <summary>
        /// 目标是否已完成
        /// </summary>
        /// <param name="targetId"></param>
        /// <returns></returns>
        public bool IsTargetFinish(int targetId)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            if (db.IsCoinTypeElement(targetId))
            {
                return false;
            }
            if (TryGetTargetByOtherEvolution(targetId, out var id))
            {
                if (_targetElements.TryGetValue(id, out var num))
                {
                    return _targetElements[targetId] <= 0 && num <= 0;
                }
            }
            return _targetElements.ContainsKey(targetId) && _targetElements[targetId] <= 0;
        }

        public int GetTargetRemainNum(int elementId)
        {
            if (TryGetTargetByOtherEvolution(elementId, out var id))
            {
                if (_targetElements.TryGetValue(id, out var num))
                {
                    return Mathf.Max(0, num);
                }
            }

            return Mathf.Max(0, _targetElements.GetValueOrDefault(elementId, 0));
        }

        public bool CheckTargetComplete(int elementId)
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[elementId];
            if (config.checkTargetId <= 0)
                return false;
            bool isFinish = IsTargetFinish(config.checkTargetId);
            return isFinish;
        }

        public void AddRandomDiffuseCount(int randomElementId, int count)
        {
            if (!_randomPopElementCounting.TryAdd(randomElementId, count))
            {
                _randomPopElementCounting[randomElementId] += count;
            }
        }

        public bool IsRandomDiffuseCanPop(int randomElementId)
        {
            if (_randomPopElementCounting.TryGetValue(randomElementId, out var popCount))
            {
                ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
                int checkTargetId = -1;
                if(_originalTargets.ContainsKey(randomElementId))
                    checkTargetId = randomElementId;
                else
                {
                    List<int> nextIds = new();
                    db.RefElementNextList(randomElementId, ref nextIds);
                    for (int i = 0; i < nextIds.Count; i++)
                    {
                        if (_originalTargets.ContainsKey(nextIds[i]))
                        {
                            checkTargetId = nextIds[i];
                            break;
                        }
                    }
                }

                if (checkTargetId != -1)
                {
                    int targetNum = _originalTargets[checkTargetId];
                    return popCount < targetNum;
                }
            }

            return true;
        }

        private void FixTempTargetNum()
        {
            foreach (var kp in _targetElements)
            {
                TempTargetElements[kp.Key] = kp.Value;
            }
        }
        
        /// <summary>
        /// 目标是否由同是目标的其它元素演变而成的
        /// </summary>
        /// <returns></returns>
        private bool TryGetTargetByOtherEvolution(int elementId,out int evolutionId)
        {
            evolutionId = -1;
            if (_targetElements == null)
                return false;

            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            if (db.IsCircleElement(elementId))
                return false;
            
            if (!_targetElements.ContainsKey(elementId))
                return false;
            
            foreach (var targetId in _targetElements.Keys)
            {
                if(elementId == targetId)
                    continue;
                
                List<int> nextIds = new();
                db.RefElementNextList(targetId, ref nextIds);
                if (nextIds.Count > 0)
                {
                    if (nextIds.Contains(elementId))
                    {
                        evolutionId = targetId;
                        return true;
                    }
                }
            }
            return false;
        }

        private void TickTargetChangedNum(bool reset = false)
        {
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchTargetChangedNum, EventOneParam<bool>.Create(reset));
        }
    }
}