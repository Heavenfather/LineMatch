using System.Collections.Generic;
using UnityEngine;

namespace HotfixLogic.Match
{
    public struct GridElementInfo
    {
        public int ElementId;
        public int Count;
        public int PreLinkId;
        public int ElementWidth;
        public int ElementHeight;
        public Vector2Int Coord;
        public bool IsConfigElement;
        public List<int> NextElementIds;
    }
}