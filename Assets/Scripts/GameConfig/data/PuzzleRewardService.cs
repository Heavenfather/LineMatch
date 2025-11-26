using System.Collections.Generic;

namespace GameConfig
{
    public partial class PuzzleRewardDB
    {

        public int GetOpenStar(int mapID, int levelID) {
            List<PuzzleReward> rewards = new List<PuzzleReward>();
            foreach (PuzzleReward reward in _data) {
                if (reward.mapId == mapID && reward.levelId == levelID) {
                    return reward.openStar;
                }
            }

            return -1;
        }

    }
}