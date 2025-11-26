using System;
using System.Collections.Generic;
using UnityEngine;

namespace HotfixLogic.Match
{
    /// <summary>
    /// 障碍物元素占据的格子信息
    /// </summary>
    public struct GridHoldInfo : IEquatable<GridHoldInfo>
    {
        public int ElementId;

        public int TargetElementId;

        public int TargetElementNum;

        public int ElementWidth;

        public int ElementHeight;
        
        public Vector2Int StartCoord;

        public int Order;
        
        public HashSet<Vector2Int> AllHoldGridPos;

        public List<int> LinkElementIds;

        public void RemoveHoldGridPos(Vector2Int pos)
        {
            if (AllHoldGridPos.Contains(pos))
            {
                AllHoldGridPos.Remove(pos);
            }
        }

        public bool Equals(GridHoldInfo other)
        {
            return ElementId == other.ElementId && StartCoord.Equals(other.StartCoord);
        }

        public override bool Equals(object obj)
        {
            return obj is GridHoldInfo other && Equals(other);
        }

        public override int GetHashCode()
        { 
            return HashCode.Combine(ElementId, StartCoord);
        }
    }
}