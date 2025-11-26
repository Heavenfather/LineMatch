#if UNITY_EDITOR

using GameConfig;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic
{
    [Window(UILayer.UI, "uiprefab/matcheditor/matcheditoreditfillblock")]
    public partial class MatchEditorEditFillBlock : UIWindow
    {
        private int _eleId;
        private int _blockX;
        private int _blockY;
        private LevelData _levelData;

        public override void OnCreate()
        {
            btn_cancel.AddClick(CloseSelf);
            btn_ok.AddClick(() =>
            {
                if (!IsValid())
                {
                    return;
                }

                int.TryParse(input_wid.text, out int wid);
                int.TryParse(input_height.text, out int hei);
                int.TryParse(input_target.text, out int targetId);
                int.TryParse(input_targetNum.text, out int targetNum);
                FillElementData data = new FillElementData()
                {
                    X = _blockX,
                    Y = _blockY,
                    TargetId = targetId,
                    TargetNum = targetNum,
                    Width = wid,
                    Height = hei,
                    ElementId = _eleId,
                };
                G.EventModule.DispatchEvent(MatchEditorConst.OnEditFillBlockOkClick,
                    EventOneParam<FillElementData>.Create(data));
                CloseSelf();
            });

            _eleId = (int)userDatas[0];
            _blockX = (int)userDatas[1];
            _blockY = (int)userDatas[2];
            _levelData = (LevelData)userDatas[3];
            InitFillData();
        }

        private bool IsValid()
        {
            int.TryParse(input_wid.text, out int wid);
            int.TryParse(input_height.text, out int hei);
            int.TryParse(input_target.text, out int targetId);
            int.TryParse(input_targetNum.text, out int targetNum);
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            bool isTargetElement = db[_eleId].elementType == ElementType.TargetBlock || db[_eleId].elementType == ElementType.FixedGridTargetBlock;
            if (isTargetElement && !db.IsContain(targetId))
            {
                Logger.Error($"目标元素 [{targetId}] 不存在，请检查配置表是否已配置");
                return false;
            }

            if (hei <= 0)
            {
                if (isTargetElement)
                    Logger.Error("高不能小于等于0");
                else
                    Logger.Error("长度不能小于等于0");
                return false;
            }

            if (isTargetElement && wid <= 0)
            {
                Logger.Error("宽不能小于等于0");
                return false;
            }

            if (isTargetElement && targetNum <= 0)
            {
                Logger.Error("目标数量不能小于等于0");
                return false;
            }

            return true;
        }

        private void InitFillData()
        {
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[_eleId];
            input_target.SetVisible(config.eliminateStyle == EliminateStyle.Target);
            input_targetNum.SetVisible(config.eliminateStyle == EliminateStyle.Target);

            if (config.elementType == ElementType.FixedGridTargetBlock ||
                config.elementType == ElementType.VerticalExpand ||
                config.elementType == ElementType.FixPosExpand)
            {
                if (config.direction == ElementDirection.Down || config.direction == ElementDirection.Up)
                {
                    input_wid.SetTextWithoutNotify("1");
                    input_wid.interactable = false;
                    input_height.SetTextWithoutNotify(Mathf.FloorToInt(config.holdGrid).ToString());
                    if (config.elementType == ElementType.FixedGridTargetBlock)
                        input_height.interactable = false;
                }
                else if (config.direction == ElementDirection.Right || config.direction == ElementDirection.Left)
                {
                    input_height.SetTextWithoutNotify("1");
                    input_height.interactable = false;
                    input_wid.SetTextWithoutNotify(Mathf.FloorToInt(config.holdGrid).ToString());
                    if (config.elementType == ElementType.FixedGridTargetBlock)
                        input_wid.interactable = false;
                }
            }
            
            var gridInfos = _levelData.FindCoordHoldGridInfo(_blockX, _blockY, true);
            if (gridInfos != null)
            {
                int fillElementIndex = gridInfos.FindIndex(t => t.ElementId == _eleId);
                if (fillElementIndex >= 0)
                {
                    var info = gridInfos[fillElementIndex];
                    input_wid.SetTextWithoutNotify(info.ElementWidth.ToString());
                    input_height.SetTextWithoutNotify(info.ElementHeight.ToString());
                    input_target.SetTextWithoutNotify(info.TargetElementId.ToString());
                    input_targetNum.SetTextWithoutNotify(info.TargetElementNum.ToString());
                }
            }
        }
    }
}

#endif