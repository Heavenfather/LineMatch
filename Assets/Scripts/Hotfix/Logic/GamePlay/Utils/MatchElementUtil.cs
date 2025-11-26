using System.Collections.Generic;
using GameConfig;
using Hotfix.Define;
using HotfixCore.Module;
using HotfixLogic;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public static class MatchElementUtil
    {
        private static Dictionary<int, Color> _elementColorMap = null;

        private static string _matchBgColor = "";

        /// <summary>
        /// 消除关卡背景颜色
        /// </summary>
        public static string MatchBgColor
        {
            get
            {
                if(_elementColorMap == null)
                {
                    _elementColorMap = new Dictionary<int, Color>();
                    BuildElementColorMap();
                }
                return _matchBgColor;
            }
            private set
            {
                _matchBgColor = value;
            }
        }

        /// <summary>
        /// 元素单网格大小
        /// </summary>
        public static readonly Vector2 GridSize = new Vector2(0.8f, 0.8f);

        /// <summary>
        /// 判断元素是否可匹配
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        public static bool IsMatchable(ElementType elementType)
        {
            return elementType == ElementType.Rocket ||
                   elementType == ElementType.RocketHorizontal ||
                   elementType == ElementType.Bomb ||
                   elementType == ElementType.ColorBall ||
                   elementType == ElementType.StarBomb ||
                   elementType == ElementType.SearchDot ||
                   elementType == ElementType.HorizontalDot ||
                   elementType == ElementType.TowDotsBombDot ||
                   elementType == ElementType.TowDotsColoredDot;
        }

        public static void DynamicModifyColorMap(BoardColorStruck data)
        {
            MatchBgColor = $"#{ColorUtility.ToHtmlStringRGB(data.BgColor).ToLower()}";
            _elementColorMap[1] = data.Green;
            _elementColorMap[2] = data.Blue;
            _elementColorMap[3] = data.Yellow;
            _elementColorMap[4] = data.Red;
            _elementColorMap[5] = data.Purple;
            _elementColorMap[6] = data.Orange;
            _elementColorMap[7] = data.Cycan;
            // foreach (var elements in _gridElements.Values)
            // {
            //     for (int i = 0; i < elements.Count; i++)
            //     {
            //         if (elements[i] is BaseElementItem elementItem)
            //         {
            //             elementItem.SetElementColor();
            //         }
            //     }
            // }

            // 发送GameEventDefine.OnOkChangeBoardColor事件 实时刷新棋盘上棋子的颜色 TODO....
        }

        public static Color GetElementColor(int elementId)
        {
            if (_elementColorMap == null)
            {
                _elementColorMap = new Dictionary<int, Color>();
                BuildElementColorMap();
            }

            Color color = Color.white;
            if (_elementColorMap.TryGetValue(elementId, out var value))
                color = value;
            return color;
        }

        private static void BuildElementColorMap()
        {
            LevelMapImageDB db = ConfigMemoryPool.Get<LevelMapImageDB>();
            int id = Mathf.Max(1, db.GetLevelInPage(MatchManager.Instance.CurLevelID, MatchManager.Instance.MaxLevel));
            LevelMapImage config = db[id + 1];
            MatchBgColor = config.matchBgColor;
            if (ColorUtility.TryParseHtmlString(config.lineColorMap[1], out var color1))
                _elementColorMap[1] = color1;
            if (ColorUtility.TryParseHtmlString(config.lineColorMap[2], out var color2))
                _elementColorMap[2] = color2;
            if (ColorUtility.TryParseHtmlString(config.lineColorMap[3], out var color3))
                _elementColorMap[3] = color3;
            if (ColorUtility.TryParseHtmlString(config.lineColorMap[4], out var color4))
                _elementColorMap[4] = color4;
            if (ColorUtility.TryParseHtmlString(config.lineColorMap[5], out var color5))
                _elementColorMap[5] = color5;
            if (ColorUtility.TryParseHtmlString(config.lineColorMap[6], out var color6))
                _elementColorMap[6] = color6;
            if (ColorUtility.TryParseHtmlString(config.lineColorMap[7], out var color7))
                _elementColorMap[7] = color7;
        }
    }
}