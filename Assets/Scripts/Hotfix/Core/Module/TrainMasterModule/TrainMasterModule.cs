using System.Collections.Generic;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.Utils;
using HotfixLogic;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    public class TrainMasterModule : IModuleAwake, IModuleDestroy
    {
        private ServerTrainMasterData _serverTrainMasterData;
        private List<int> _lvPlayerCount = new List<int>();
        private bool _hasLvChange = false;

        private int _failWinStreakNum = 0;

        public void Awake(object parameter)
        {
        }

        public void Destroy()
        {
        }

        public void SetTrainMasterInfo(ServerTrainMasterData data)
        {
            _serverTrainMasterData = data;

            if (_serverTrainMasterData != null) {
                G.RedDotModule.SetRedDotCount(RedDotDefine.TrainMaster, GetState() == 1 ? 1 : 0);
                InitLvPlayerCount();
                _failWinStreakNum = _serverTrainMasterData.win_streak_num;
            }

            G.EventModule.DispatchEvent(GameEventDefine.OnMainUpdateTrainMaster);
        }

        public void InitLvPlayerCount() {
            if (_lvPlayerCount.Count != 0 || !InTrainning()) return;

            var strArr = PlayerPrefsUtil.GetString("TrainMasterPlayerCount", "");
            if (strArr != "") {
                var recordList = strArr.Split(',');
                for (int i = 0; i < recordList.Length; i++) {
                    _lvPlayerCount.Add(int.TryParse(recordList[i], out int count) ? count : 0);
                }
                return;
            }


            var lastPlayerCount = _serverTrainMasterData.total_coin / GetCoinReward();
            var averageCount = (100 - lastPlayerCount) / 6;

            var lastCount = 100;
            var random = new System.Random();
            var offsetCount = 0;
            var recordStr = "";
            
            // 分配前6关的人数
            for (int i = 0; i < 6; i++) {

                if (i % 2 == 0) offsetCount = random.Next(-5, 6);

                var curLvCount = i % 2 == 0 ? averageCount + offsetCount : averageCount - offsetCount;
                lastCount -= curLvCount;
                _lvPlayerCount.Add(lastCount);

                recordStr += curLvCount + ",";
            }

            _lvPlayerCount.Add(lastPlayerCount);
            recordStr += lastPlayerCount;

            Logger.Debug("_lvPlayerCount to string" + _lvPlayerCount.ToString());


            PlayerPrefsUtil.SetString("TrainMasterPlayerCount", recordStr);
        }

        public int GetState() {
            return _serverTrainMasterData.state;
        }

        private void SetState(int state) {
            _serverTrainMasterData.state = state;
        }

        public long GetEndTime() {
            return _serverTrainMasterData.end_time;
        }

        public int GetTotalCoin() {
            return _serverTrainMasterData.total_coin;
        }

        public long GetStartTime() {
            return _serverTrainMasterData.start_time;
        }

        public int GetCoinReward() {
            return _serverTrainMasterData.coin_reward;
        }

        public int GetWinStreakNum() {
            if (_serverTrainMasterData == null) return 0;
            return _serverTrainMasterData.win_streak_num > 7 ? 7 : _serverTrainMasterData.win_streak_num;
        }

        public int GetRemainingPlayer() {
            if (_serverTrainMasterData.win_streak_num > 0) {
                return _lvPlayerCount[GetWinStreakNum() - 1];
            } else {
                return 100;
            }
        }

        public int RandomRewardCoin() {
            var random = new System.Random();

            // 随机奖励玩家数量
            var rewardPlayerCount = random.Next(7, 12);

            // 奖励金币
            int rewardCoin = _serverTrainMasterData.total_coin / rewardPlayerCount;

            return rewardCoin;
        }

        public bool InTrainning() {
            if (_serverTrainMasterData == null) return false;
            if (GetState() != 2) {
                return IsInTrainningTime();
            }

            return false;
        }

        public bool IsInTrainningTime() {
            var nowTime = CommonUtil.GetNowTime();
            return nowTime >= GetStartTime() && nowTime < GetEndTime();
        }

        public void AddWinStreak() {
            if (InTrainning() && _serverTrainMasterData.win_streak_num < 7) {
                _serverTrainMasterData.win_streak_num++;
                _failWinStreakNum = _serverTrainMasterData.win_streak_num;

                if (_serverTrainMasterData.win_streak_num >= 7 && GetState() == 0) {
                    SetState(1);
                    G.RedDotModule.SetRedDotCount(RedDotDefine.TrainMaster, 1);
                }

                _hasLvChange = true;
            }
        }
        

        public void ResetWinStreak() {
            if (_serverTrainMasterData == null) return;
            if (_serverTrainMasterData.win_streak_num >= 7) return;

            if (_serverTrainMasterData.win_streak_num != 0) {
                _hasLvChange = true;
            }
            _serverTrainMasterData.win_streak_num = 0;
        }

        public void FinishTrainning() {
            SetState(2);
            ClearPlayerCount();
            G.RedDotModule.SetRedDotCount(RedDotDefine.TrainMaster, 0);

            G.EventModule.DispatchEvent(GameEventDefine.OnMainUpdateTrainMaster);
        }



        public bool TrainningIsFinish() {
            if (_serverTrainMasterData == null) return false;
            return GetState() == 2;
        }

        private void ClearPlayerCount() {
            _lvPlayerCount.Clear();
            PlayerPrefsUtil.SetString("TrainMasterPlayerCount", "");
        }

        public bool HasLvChange() {
            return _hasLvChange;
        }

        public int GetFailWinStreakNum() {
            return _failWinStreakNum;
        }

        public void ResetChangeState() {
            _hasLvChange = false;
        }

        public void ClearFailWinStreak() {
            _failWinStreakNum = 0;
        }
    }
}
