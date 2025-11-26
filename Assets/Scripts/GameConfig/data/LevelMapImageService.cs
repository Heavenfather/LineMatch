using System;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace GameConfig
{
    public partial class LevelMapImageDB
    {
        public int GetLevelInPage(int level, int maxLevel)
        {
            if (maxLevel <= 12) return 0;
            return (int)Math.Ceiling((double)(level - 12) / 15);
        }

        public (int, int) GetLevelRangeByPage(int page)
        {
            if (page <= 0) return (1, 13);

            var begin = 13 + (page - 1) * 15;
            var end = begin + 15;
            return (begin, end);
        }

        public Color GetBaseElementColor(int level, int maxLevel,int elementId)
        {
            int id = Mathf.Max(1, GetLevelInPage(level, maxLevel));
            ref readonly LevelMapImage config = ref this[id + 1];
            if(!config.lineColorMap.ContainsKey(elementId))
                return Color.white;
            string colorHtml = config.lineColorMap[elementId];
            return ColorUtility.TryParseHtmlString(colorHtml, out Color color) ? color : Color.white;
        }
    }
}