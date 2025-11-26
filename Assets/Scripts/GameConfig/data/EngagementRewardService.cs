using System.Collections.Generic;

namespace GameConfig
{
    public partial class EngagementRewardDB
    {
        private List<EngagementReward> _sevenEngagementRewards;
        public List<EngagementReward> SevenEngagementRewards
        {
            get
            {
                if (_sevenEngagementRewards == null)
                {
                    _sevenEngagementRewards = new List<EngagementReward>();
                    for (int i = 0; i < Count; i++)
                    {
                        if (All[i].taskType == 1)
                        {
                            _sevenEngagementRewards.Add(All[i]);
                        }
                    }
                }
                return _sevenEngagementRewards;
            }
        }
        private List<EngagementReward> _dailyEngagementRewards;
        public List<EngagementReward> DailyEngagementRewards
        {
            get
            {
                if (_dailyEngagementRewards == null)
                {
                    _dailyEngagementRewards = new List<EngagementReward>();
                    for (int i = 0; i < Count; i++)
                    {
                        if (All[i].taskType == 8)
                        {
                            _dailyEngagementRewards.Add(All[i]);
                        }
                    }
                }
                return _dailyEngagementRewards;
            }
        }

        public EngagementReward DailyLastReward
        {
            get
            {
                return DailyEngagementRewards[^1];
            }
        }

        public EngagementReward SevenLastReward
        {
            get
            {
                return SevenEngagementRewards[^1];
            }
        }

        protected override void OnInitialized()
        {
            
        }

        protected override void OnDispose()
        {
            _sevenEngagementRewards?.Clear();
            _sevenEngagementRewards = null;
            _dailyEngagementRewards?.Clear();
            _dailyEngagementRewards = null;
        }
    }
}