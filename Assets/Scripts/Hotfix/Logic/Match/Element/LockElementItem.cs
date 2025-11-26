
using System.Collections.Generic;
using GameConfig;
using Hotfix.Logic.Match;
using UnityEngine;

namespace HotfixLogic.Match
{
    /// <summary>
    /// 藤蔓类型的元素
    /// </summary>
    public class LockElementItem : BlockElementItem
    {
        private GridSnowLine _snowLine;
        private int _snowID = 150;

        protected override void OnInitialized()
        {
            if (Data.ConfigId == _snowID) {
                _snowLine = GameObject.transform.Find("SnowLine").GetComponent<GridSnowLine>();
                _snowLine.SetGridPos(Data.GridPos);
                _snowLine.HideAllLines();
            }
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            if (Data.ConfigId == _snowID) {
                DrawSnowLineUtil.DestroyLockElement(Data.GridPos);
            }

            bool bResult = base.OnDestroy(context);
            if (bResult)
            {
                if (_snowLine != null)
                    _snowLine.HideAllLines();
            }
            return bResult;
        }

        public void UpdateLockLine(List<Vector2Int> neighborSnows)
        {
            if (_snowLine != null)
                _snowLine.UpdateLine(neighborSnows);
        }
    }

    public class DrawSnowLineUtil
    {
        private static Dictionary<Vector2Int, LockElementItem> _snowDict =
            new Dictionary<Vector2Int, LockElementItem>();

        public static void InitSnowDict()
        {
            _snowDict.Clear();
            
            var elementDB = ConfigMemoryPool.Get<ElementMapDB>();
            var lockElements = ElementSystem.Instance.GetAllTargetElements(ElementType.Lock);
            foreach (var item in lockElements)
            {
                if (elementDB[item.Data.ConfigId].nameFlag == "snow") {
                    _snowDict.TryAdd(item.Data.GridPos, (LockElementItem)item);
                }
            }

            UpdateLockElementLine(new List<Vector2Int>(_snowDict.Keys));
        }

        public static void UpdateLockElementLine(List<Vector2Int> lockElementPos) {
            foreach (var pos in lockElementPos)
            {
                LockElementItem item = null;
                if (_snowDict.ContainsKey(pos))
                {
                    item = _snowDict[pos];
                }

                if (item == null) continue;

                var neighborPos = MatchTweenUtil.GetEightNeighborPos(pos);
                var neighborSnows = new List<Vector2Int>();
                foreach (var neighbor in neighborPos)
                {
                    if (_snowDict.ContainsKey(neighbor))
                    {
                        neighborSnows.Add(neighbor);
                    }
                }
                item.UpdateLockLine(neighborSnows);
            }
        }

        public static void DestroyLockElement(Vector2Int gridPos)
        {
            if (_snowDict.ContainsKey(gridPos))
            {
                _snowDict.Remove(gridPos);

                var neighborPos = MatchTweenUtil.GetEightNeighborPos(gridPos);
                UpdateLockElementLine(neighborPos);
            }
        }
    }


}