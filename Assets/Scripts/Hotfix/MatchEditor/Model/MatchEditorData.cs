#if UNITY_EDITOR
using System.Collections.Generic;
using HotfixCore.MVC;
using HotfixLogic.Match;
using UnityEngine;

namespace HotfixLogic
{
    public class MatchEditorData : BaseModel
    {
#if UNITY_EDITOR
        public string LevelPath => UnityEditor.EditorPrefs.GetString(MatchEditorConst.LevelPathKey,
            $"{Application.dataPath}/ArtLoad/Match/Level");
#else
        public string LevelPath => PlayerPrefs.GetString(MatchEditorConst.LevelPathKey, $"{Application.dataPath}/MatchLevel");
#endif

        public int CurrentLevelId = 0;

        public ElementFillState FillState = ElementFillState.Scroll;

        public int CurFillElementId = -1;

        public List<LevelData> AllLevelData = new List<LevelData>();
    }
}
#endif