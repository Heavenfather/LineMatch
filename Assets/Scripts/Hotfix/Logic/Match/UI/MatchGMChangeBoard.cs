using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;

namespace HotfixLogic
{
    public struct BoardColorStruck
    {
        public Color BgColor;

        public Color Green;

        public Color Blue;

        public Color Yellow;

        public Color Red;

        public Color Purple;

        public Color Orange;

        public Color Cycan;
    }


    [Window(UILayer.UI, "uiprefab/match/matchgmchangeboard")]
    public partial class MatchGMChangeBoard : UIWindow
    {
        public override void OnCreate()
        {
            btn_close.AddClick(CloseSelf);

            btn_ok.AddClick(() =>
            {
                if (!Valid(out var data))
                    return;
                G.EventModule.DispatchEvent(GameEventDefine.OnOkChangeBoardColor,
                    EventOneParam<BoardColorStruck>.Create(data));
                CloseSelf();
            });
            btn_reset.AddClick(ResetToConfig);
        }

        public void SetInit(string bgColor)
        {
            var colorMap = ElementSystem.Instance.ElementColorMap;
            input_bgColor.text = bgColor;
            input_green.text = $"#{ColorUtility.ToHtmlStringRGB(colorMap[1]).ToLower()}";
            input_blue.text = $"#{ColorUtility.ToHtmlStringRGB(colorMap[2]).ToLower()}";
            input_yellow.text = $"#{ColorUtility.ToHtmlStringRGB(colorMap[3]).ToLower()}";
            input_red.text = $"#{ColorUtility.ToHtmlStringRGB(colorMap[4]).ToLower()}";
            input_purple.text = $"#{ColorUtility.ToHtmlStringRGB(colorMap[5]).ToLower()}";
            input_orange.text = $"#{ColorUtility.ToHtmlStringRGB(colorMap[6]).ToLower()}";
            input_cycan.text = $"#{ColorUtility.ToHtmlStringRGB(colorMap[7]).ToLower()}";
        }

        private void ResetToConfig()
        {
            //还原回配置的色值
            LevelMapImageDB db = ConfigMemoryPool.Get<LevelMapImageDB>();
            input_bgColor.text = db.GetMatchBgColor(MatchManager.Instance.CurLevelID, MatchManager.Instance.MaxLevel);

            var lineColorMap = db.GetLineColors(MatchManager.Instance.CurLevelID, MatchManager.Instance.MaxLevel);
            input_green.text = lineColorMap[1];
            input_blue.text = lineColorMap[2];
            input_yellow.text = lineColorMap[3];
            input_red.text = lineColorMap[4];
            input_purple.text = lineColorMap[5];
            input_orange.text = lineColorMap[6];
            input_cycan.text = lineColorMap[7];
        }

        private bool Valid(out BoardColorStruck data)
        {
            data = new BoardColorStruck();
            if (!ColorUtility.TryParseHtmlString(input_bgColor.text, out data.BgColor))
            {
                CommonUtil.ShowCommonTips("背景颜色格式错误");
                return false;
            }

            if (!ColorUtility.TryParseHtmlString(input_green.text, out data.Green))
            {
                CommonUtil.ShowCommonTips("绿色颜色格式错误");
                return false;
            }

            if (!ColorUtility.TryParseHtmlString(input_blue.text, out data.Blue))
            {
                CommonUtil.ShowCommonTips("蓝色颜色格式错误");
                return false;
            }

            if (!ColorUtility.TryParseHtmlString(input_yellow.text, out data.Yellow))
            {
                CommonUtil.ShowCommonTips("黄色颜色格式错误");
                return false;
            }

            if (!ColorUtility.TryParseHtmlString(input_red.text, out data.Red))
            {
                CommonUtil.ShowCommonTips("红色颜色格式错误");
                return false;
            }

            if (!ColorUtility.TryParseHtmlString(input_purple.text, out data.Purple))
            {
                CommonUtil.ShowCommonTips("紫色颜色格式错误");
                return false;
            }

            if (!ColorUtility.TryParseHtmlString(input_orange.text, out data.Orange))
            {
                CommonUtil.ShowCommonTips("橙色颜色格式错误");
                return false;
            }

            if (!ColorUtility.TryParseHtmlString(input_cycan.text, out data.Cycan))
            {
                CommonUtil.ShowCommonTips("青色颜色格式错误");
                return false;
            }

            return true;
        }
    }
}