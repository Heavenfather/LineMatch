using System;
using System.Collections.Generic;
using GameConfig;
using GameCore.Localization;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.Utils;
using HotfixLogic;
using HotfixLogic.Match;

namespace HotfixCore.Module
{
    public class InviteModule : IModuleAwake, IModuleDestroy
    {
        private int _inviteCount;
        public int InviteCount => _inviteCount;

        private List<ServerInviteReward> _inviteRewards;

        private bool _isInviteFinished;

        private bool _hasInviteFailed;
        public bool HasInviteFailed => _hasInviteFailed;

        public void Awake(object parameter)
        {
        }

        public void Destroy()
        {
        }

        public void SetInviteData(int inviteNum, List<ServerInviteReward> inviteRewards) {
            _inviteCount = inviteNum;
            _inviteRewards = inviteRewards;

            UpdateState();
        }

        private void UpdateState() {
            _isInviteFinished = true;
            for (int i = 0; i < _inviteRewards.Count; i++) {
                var data = _inviteRewards[i];
                if (data.state != 2) {
                    _isInviteFinished = false;
                }

                if (data.state == 1) {
                    G.RedDotModule.SetRedDotCount(RedDotDefine.Invite, 1);
                }
            }

            G.EventModule.DispatchEvent(GameEventDefine.OnMainUpdateInviteFriend);
        }

        public void QueryInviteData(Action<bool> callback = null) {
            G.HttpModule.QueryInviteReward(data => {
                if (data != null) {
                    _hasInviteFailed = false;
                    if (data.invite_fail_list != null && data.invite_fail_list.Count > 0) {
                        foreach (var player in data.invite_fail_list) {
                            if (player.show_cnt == 1) {
                                _hasInviteFailed = true;
                                break;
                            }
                        }
                    }
                    
                    SetInviteData(data.invite_cnt, data.reward_list);
                }
                callback?.Invoke(data != null);
            });
        }

        public void GetReward(int rewardID, Action<bool> callback = null) {
			G.HttpModule.GetnviteReward(rewardID.ToString(), (succ) => {
				if (succ) {
					foreach (var reward in _inviteRewards) {
						if (reward.id == rewardID) {
							reward.state = 2;
							break;
						}
					}

					var cfg = ConfigMemoryPool.Get<InviteFriendCfgDB>()[rewardID];
					var rewardStr = cfg.rewards;
					var itemList = CommonUtil.GetItemDatasByStr(rewardStr);
					CommonUtil.ShowRewardWindow(itemList, LocalizationPool.Get("Reward/Tips/Invite"));
					G.GameItemModule.AddItemCount(itemList);

                    UpdateState();
				}
                callback?.Invoke(succ);
			});
        }

        public bool IsOpenInvite() {
            if (_isInviteFinished) return false;

            var openLv = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("InviteOpenLv");
            return MatchManager.Instance.MaxLevel >= openLv;
        }

		public int GetCanRewardID() {
			for (int i = 0; i < _inviteRewards.Count; i++) {
				var reward = _inviteRewards[i];
				if (reward.state == 1) {
					return reward.id;
				}
			}

			return 0;
		}

        public bool CanPop() {
            if (!IsOpenInvite() || MatchManager.Instance.MaxLevel >= ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("InvitePopMaxLv")) return false;

            var key1 = "InviteBeginPopTime";
            var key2 = "InviteDailyPopTime";

            var nowTime = CommonUtil.GetNowTime().ToString();

            var beginTimeStr = PlayerPrefsUtil.GetString(key1, "");
            long beginPopTime = beginTimeStr == "" ? 0 : long.Parse(beginTimeStr);
            if (beginPopTime == 0) {
                PlayerPrefsUtil.SetString(key1, nowTime.ToString());
                PlayerPrefsUtil.SetString(key2, nowTime.ToString());
                return true;
            }

            var beginDate = CommonUtil.UnixToLocalDateTime(beginPopTime);
            var nowDate = CommonUtil.GetNowDateTime();
            var beginDiff = nowDate - beginDate;
            // 只有在一定时间内才弹出
            if (beginDiff.Days >= ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("InviteDurationDay")) {
                return false;
            }

            // 判断每日弹出
            var dailyTimeStr = PlayerPrefsUtil.GetString(key2, "");
            long dailyPopTime = dailyTimeStr == "" ? 0 : long.Parse(dailyTimeStr);
            if (dailyPopTime == 0) {
                PlayerPrefsUtil.SetString(key2, nowTime.ToString());
                return true;
            }

            var dailyDate = CommonUtil.UnixToLocalDateTime(dailyPopTime);
            var dailyDiff = nowDate - dailyDate;
            if (dailyDiff.Days >= 1) {
                PlayerPrefsUtil.SetString(key2, nowTime.ToString());
                return true;
            } else {
                return false;
            }
        }
    }
}
