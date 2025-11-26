using System.Collections.Generic;

namespace GameConfig
{
    public partial class PuzzleUnlockDB
    {
        public List<int> CheckIsUnlockLv(int levelID) {
            List<int> unlockMap = new List<int>();
            foreach (var unlock in _data) {
                if (unlock.unlockLevel <= levelID) {
                    unlockMap.Add(unlock.mapId);
                }
            }
            return unlockMap;
        }

        public Dictionary<int, int> UnlcockLvDict => _idToIdx;
    }
}