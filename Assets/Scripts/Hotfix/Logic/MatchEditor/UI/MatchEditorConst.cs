#if UNITY_EDITOR
using HotfixCore.Module;

namespace HotfixLogic
{
    public class MatchEditorConst
    {
        public const int WhiteElementId = -999;
        public const int RectangleElementId = -998;
        
        #region Key

        public const string LastEditLevelId = "LastEditLevelId";

        public const string LevelPathKey = "MatchEditorLevel";

        public const string EditorMatchGameType = "EditorMatchGameType";
        #endregion
        
        #region Event

        public static readonly int OnLevelFileFolderChanged = RuntimeId.ToRuntimeId("OnLevelFileFolderChanged");

        public static readonly int OnClickNextLevel = RuntimeId.ToRuntimeId("OnClickNextLevel");
        
        public static readonly int OnClickPreviousLevel = RuntimeId.ToRuntimeId("OnClickPreviousLevel");
        
        public static readonly int OnClickSave = RuntimeId.ToRuntimeId("OnClickSave");
        
        public static readonly int OnClickSaveAndNew = RuntimeId.ToRuntimeId("OnClickSaveAndNew");
        
        public static readonly int OnEditLevelChanged = RuntimeId.ToRuntimeId("OnEditLevelChanged");
        
        public static readonly int OnCosLevelChanged = RuntimeId.ToRuntimeId("OnCosLevelChanged");
        
        public static readonly int OnLevelStepChanged = RuntimeId.ToRuntimeId("OnLevelStepChanged");
        
        public static readonly int OnDifficultyChanged = RuntimeId.ToRuntimeId("OnDifficultyChanged");
        
        public static readonly int OnMatchTypeChanged = RuntimeId.ToRuntimeId("OnMatchTypeChanged");
        
        public static readonly int OnRandomDifficulty = RuntimeId.ToRuntimeId("OnRandomDifficulty");
        
        public static readonly int OnLevelGridCountChanged = RuntimeId.ToRuntimeId("OnLevelGridCountChanged");
        
        public static readonly int OnFullScoreChanged = RuntimeId.ToRuntimeId("OnFullScoreChanged");
        
        public static readonly int OnTargetElementChanged = RuntimeId.ToRuntimeId("OnTargetElementChanged");
        
        public static readonly int OnDelTargetElement = RuntimeId.ToRuntimeId("OnDelTargetElement");
        
        public static readonly int OnAddTargetElement = RuntimeId.ToRuntimeId("OnAddTargetElement");
        
        public static readonly int OnTargetElementIconClick = RuntimeId.ToRuntimeId("OnTargetElementIconClick");
        
        public static readonly int OnElementFillStateChanged = RuntimeId.ToRuntimeId("OnElementFillStateChanged");
        
        public static readonly int OnFillElementChanged = RuntimeId.ToRuntimeId("OnFillElementChanged");
        
        public static readonly int OnAddInitElement = RuntimeId.ToRuntimeId("OnAddInitElement");
        
        public static readonly int OnDeleteInitElement = RuntimeId.ToRuntimeId("OnDeleteInitElement");
        
        public static readonly int OnInitElementValueChanged = RuntimeId.ToRuntimeId("OnInitElementValueChanged");
        
        public static readonly int OnAddDropElement = RuntimeId.ToRuntimeId("OnAddDropElement");
        
        public static readonly int OnDeleteDropElement = RuntimeId.ToRuntimeId("OnDeleteDropElement");
        
        public static readonly int OnDropElementValueChanged = RuntimeId.ToRuntimeId("OnDropElementValueChanged");
        
        public static readonly int OnClearGridElement = RuntimeId.ToRuntimeId("OnClearGridElement");
        
        public static readonly int OnGridNotWhite = RuntimeId.ToRuntimeId("OnGridNotWhite");
        
        public static readonly int OnFillElementToGrid = RuntimeId.ToRuntimeId("OnFillElementToGrid");
        
        public static readonly int OnDelTopElement = RuntimeId.ToRuntimeId("OnDelTopElement");
        
        public static readonly int OnClearAllElement = RuntimeId.ToRuntimeId("OnClearAllElement");
        
        public static readonly int OnPreStepClick = RuntimeId.ToRuntimeId("OnPreStepClick");
        
        public static readonly int OnNextStepClick = RuntimeId.ToRuntimeId("OnNextStepClick");
        
        public static readonly int OnLevelPreviewClick = RuntimeId.ToRuntimeId("OnLevelPreviewClick");

        public static readonly int OnEditFillBlockOkClick = RuntimeId.ToRuntimeId("OnEditFillBlockOkClick");
        
        public static readonly int OnFirstAddFillElementToGrid = RuntimeId.ToRuntimeId("OnFirstAddFillElementToGrid");

        public static readonly int OnEditDropFlagOk = RuntimeId.ToRuntimeId("OnEditDropFlagOk");

        public static readonly int OnOpenEditDropFlag = RuntimeId.ToRuntimeId("OnOpenEditDropFlag");

        #endregion
    }

    public struct FillElementData
    {
        public int X;

        public int Y;

        public int ElementId;
        
        public int TargetId;

        public int TargetNum;

        public int Width;
        
        public int Height;
    }

    public struct DropElementFlagConfig
    {
        public int Index;
        
        public int ElementId;
        
        public int Rate;

        public int Limit;
    }
}
#endif