using System;
using System.Collections.Generic;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixLogic;
using HotfixLogic.Match;

namespace HotfixCore.Module
{
    public class RankModule : IModuleAwake, IModuleDestroy
    {
        private int _worldSelfRank;
        private List<WorldRankData> _worldDataList;

        private int _winStreakSelfRank;
        private List<WinStreakRankData> _winStreakDataList;
        public List<WinStreakRankData> WinStreakDataList => _winStreakDataList;

        int _lastRank;
        public int LastRank => _lastRank;
        int _rewardState;
        public int RewardState => _rewardState;

		long _worldRankLoadTime = 0;
		long _winStreakLoadTime = 0;
        int _endTime;

        int _timerID;

        public void Awake(object parameter)
        {
        }

        public void Destroy()
        {
        }

        public void SetWorldRankData(int worldSelfRank, List<WorldRankData> worldDataList)
        {
            _worldDataList = worldDataList;
            SetWorldSelfRank(worldSelfRank);
            if (_worldSelfRank > 50) _worldSelfRank = 0;
        }

        public void SetWorldSelfRank(int rankNum) {
            _worldSelfRank = rankNum;
            if (_worldSelfRank != 0 && _worldSelfRank < G.UserInfoModule.MaxRank) {
                G.UserInfoModule.SetMaxRank(_worldSelfRank);
            }
        }

        public void SetLastSreakState(int rewardState, int lastRank) {
            _lastRank = lastRank;
            SetWinStreakRewardState(rewardState);
        }

        public void SetWinStreakRewardState(int rewardState) {
            _rewardState = rewardState;
            G.RedDotModule.SetRedDotCount(RedDotDefine.Rank, _rewardState == 1? 1 : 0);
        }

        public void SetWinStreakRankData(int winStreakSelfRank, List<WinStreakRankData> winStreakDataList, int endTime)
        {
            _winStreakSelfRank = winStreakSelfRank;
            _winStreakDataList = winStreakDataList;
            _endTime = endTime;

            if (_winStreakSelfRank > 50) _winStreakSelfRank = 0;
            UpdateTimeOut();
        }

        public void GetWorldRankData(Action<int, List<WorldRankData>> callback)
        {
            if (_worldSelfRank == 0 || _worldDataList == null || CommonUtil.GetNowTime() - _worldRankLoadTime > 600) {
                G.HttpModule.GetRankOfWorld(success => {
                    _worldRankLoadTime = CommonUtil.GetNowTime();
                    UpdateSelfWorldRankData();
                    callback?.Invoke(_worldSelfRank, _worldDataList);
                });
            } else {
                UpdateSelfWorldRankData();
                callback?.Invoke(_worldSelfRank, _worldDataList);
            }
        }

        private void UpdateSelfWorldRankData() {
            if (_worldDataList == null) _worldDataList = new List<WorldRankData>();

            var selfLevel = MatchManager.Instance.MaxLevel - 1;
            var selfCurIdx = _worldDataList.FindIndex(data => data.user_id == G.UserInfoModule.UserId);

            if (selfCurIdx != -1) {
                // 查找自己是否在排行榜中
                var selfRankData = _worldDataList[selfCurIdx];
                if (selfRankData.stage_id == selfLevel) return;


                SetSelfWorldRankData(selfRankData);

                // 设置关卡
                selfRankData.stage_id = selfLevel;

                _worldDataList.RemoveAt(selfCurIdx);

                var randIdx = _worldDataList.FindLastIndex(data => data.stage_id >= selfLevel);

                _worldSelfRank = randIdx + 2;
                _worldDataList.Insert(randIdx + 1, selfRankData);
            } else {
                var randIdx = _worldDataList.FindLastIndex(data => data.stage_id >= selfLevel);
                if (randIdx >= 49) return;

                var selfRankData = new WorldRankData();
                SetSelfWorldRankData(selfRankData);
                selfRankData.stage_id = selfLevel;

                _worldDataList.Insert(randIdx + 1, selfRankData);
                _worldSelfRank = randIdx + 2;

                if (_worldDataList.Count > 50) _worldDataList.RemoveAt(50);
            }
        }

        private void SetSelfWorldRankData(WorldRankData data) {
            data.user_id = G.UserInfoModule.UserId;
            data.nickname = G.UserInfoModule.NickName;
            data.avatar = G.UserInfoModule.Avatar;
            data.avatar_frame = G.UserInfoModule.AvatarFrame;
            data.medal = G.UserInfoModule.UsingMedal;
            data.nickname_color = G.UserInfoModule.NameColor;
        }

        private void SetSelfWinStreakRankData(WinStreakRankData data) {
            data.user_id = G.UserInfoModule.UserId;
            data.nickname = G.UserInfoModule.NickName;
            data.avatar = G.UserInfoModule.Avatar;
            data.avatar_frame = G.UserInfoModule.AvatarFrame;
            data.medal = G.UserInfoModule.UsingMedal;
            data.nickname_color = G.UserInfoModule.NameColor;
        }

        private void UpdateSelfStreakRankData() {
            if (_winStreakDataList == null) _winStreakDataList = new List<WinStreakRankData>();

            var selfWinStreak = MatchManager.Instance.MaxWinStreak;

            // 至少五连胜才能上榜
            if (selfWinStreak < 5) return;

            var selfCurIdx = _winStreakDataList.FindIndex(data => data.user_id == G.UserInfoModule.UserId);
            if (selfCurIdx != -1) {
                // 查找自己是否在排行榜中
                var selfWinStreakData = _winStreakDataList[selfCurIdx];
                if (selfWinStreakData.win_streak == selfWinStreak) return;

                SetSelfWinStreakRankData(selfWinStreakData);

                // 设置关卡
                selfWinStreakData.win_streak = selfWinStreak;

                _winStreakDataList.RemoveAt(selfCurIdx);

                var randIdx = _winStreakDataList.FindLastIndex(data => data.win_streak >= selfWinStreak);

                _winStreakSelfRank = randIdx + 2;
                _winStreakDataList.Insert(randIdx + 1, selfWinStreakData);
            } else {
                var randIdx = _winStreakDataList.FindLastIndex(data => data.win_streak >= selfWinStreak);
                if (randIdx >= 49) return;

                var selfWinStreakData = new WinStreakRankData();
                SetSelfWinStreakRankData(selfWinStreakData);

                selfWinStreakData.win_streak = selfWinStreak;

                _winStreakDataList.Insert(randIdx + 1, selfWinStreakData);
                _winStreakSelfRank = randIdx + 2;

                if (_winStreakDataList.Count > 50) _winStreakDataList.RemoveAt(50);
            }
        }

        public void GetWinStreakRankData(Action<int, List<WinStreakRankData>> callback = null)
        {
            if (_winStreakSelfRank == 0 || _winStreakDataList == null || CommonUtil.GetNowTime() - _winStreakLoadTime > 600) {
                G.HttpModule.GetRankOfWinStreak(success => {
                    _winStreakLoadTime = CommonUtil.GetNowTime();
                    UpdateSelfStreakRankData();
                    callback?.Invoke(_winStreakSelfRank, _winStreakDataList);
                });
            } else {
                UpdateSelfStreakRankData();
                callback?.Invoke(_winStreakSelfRank, _winStreakDataList);
            }
        }

        private void UpdateTimeOut() {
            if (_timerID != 0) return;

            if (CommonUtil.GetNowTime() > _endTime) return;

            _timerID = G.TimerModule.AddTimer(()=> {
                int leftTime = (int)(_endTime - CommonUtil.GetNowTime());
                G.EventModule.DispatchEvent(GameEventDefine.OnRankTimeout, EventOneParam<int>.Create(leftTime));

                if (leftTime <= 0) {
                    G.TimerModule.RemoveTimer(_timerID);
                    _timerID = 0;

                    // 重置连胜
                    MatchManager.Instance.SetMaxWinStreak(0);
                    GetWinStreakRankData();
                    return;
                }
            }, 1.0f, true);
        }
    }
}
