using System.Collections.Generic;
using GameConfig;
using UnityEngine;

namespace HotfixLogic.Match
{
    public struct LevelDropElementInfo
    {
        public int ElementId;
        
        public int DropCount;
        
        public ElementType ElementType;
        
        public List<Vector2Int> DropPosition;
    }
}