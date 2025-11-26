using System.Collections.Generic;

namespace GameConfig
{
    public partial class dailyRewardDB
    {
        public List<string> GetDailyReward(int rewardId)
        {
            List<string> rewards = new List<string>(7);
            ref readonly dailyReward config = ref this[rewardId];
            
            for (int i = 1; i <= 7; i++)
            {
                string dayReward = (string)config.GetType().GetProperty($"day{i}").GetValue(config);
                if (!string.IsNullOrEmpty(dayReward))
                    rewards.Add(dayReward);
            }
            return rewards;
        }
    }
}