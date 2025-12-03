#if UNITY_EDITOR
using HotfixCore.Command;
using HotfixLogic;
using HotfixLogic.Match;

namespace EditorMatch.Scripts
{
    public class LevelDataChangeCommand : ICommand
    {
        private LevelData _oriLevelData;
        private LevelData _newLevelData;
        private MatchEditorController _matchEditorController;
        
        public LevelDataChangeCommand(LevelData levelData,MatchEditorController controller)
        {
            _oriLevelData = new LevelData();
            levelData.DeepClone(_oriLevelData);
            _oriLevelData.referenceId = levelData.referenceId;
            _matchEditorController = controller;
        }

        public void SetNewLevelData(LevelData newLevelData)
        {
            _newLevelData = newLevelData;
        }

        public void Execute()
        {
            // _matchEditorController.LoadLevelData(_newLevelData, true);
        }

        public void Undo()
        {
            _matchEditorController.LoadLevelData(_oriLevelData,true);
        }

        public void Redo()
        {
            _matchEditorController.LoadLevelData(_newLevelData,true);
        }
    }
}
#endif