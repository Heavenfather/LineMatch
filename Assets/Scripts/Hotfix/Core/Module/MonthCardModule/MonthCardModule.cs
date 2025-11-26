
using System.Collections.Generic;
using Hotfix.Define;
using HotfixLogic;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    public class MonthCardModule : IModuleAwake, IModuleDestroy
    {
        private List<ServerMonthCardData> _monthCardList = new List<ServerMonthCardData>();
        public List<ServerMonthCardData> MonthCardList => _monthCardList;

        public void Awake(object parameter)
        {
        }

        public void Destroy()
        {
        }

        public void UpdateMonthCardList(List<ServerMonthCardData> monthCardList)
        {
            _monthCardList = monthCardList;
            UpdateMonthCardReddot();
            G.EventModule.DispatchEvent(GameEventDefine.OnMonthCardUpdateData);
        }

        public void UpdateMonthCardState(int rewardId, int state) {
            foreach (var monthCard in _monthCardList) {
                if (monthCard.reward_id == rewardId) {
                    monthCard.state = state;
                    break;
                }
            }

            UpdateMonthCardReddot();
            G.EventModule.DispatchEvent(GameEventDefine.OnMonthCardUpdateData);
        }

        private void UpdateMonthCardReddot() {
            var dotCount1 = _monthCardList[0].state == 1 ? 1 : 0;
            var dotCount2 = _monthCardList[1].state == 1 ? 1 : 0;

            Logger.Debug("UpdateMonthCardReddot dotCount1:" + dotCount1 + " dotCount2:" + dotCount2);

            G.RedDotModule.SetRedDotCount(RedDotDefine.MonthCardNormal, dotCount1);
            G.RedDotModule.SetRedDotCount(RedDotDefine.MonthCardSuper, dotCount2);
        }

    }
}
