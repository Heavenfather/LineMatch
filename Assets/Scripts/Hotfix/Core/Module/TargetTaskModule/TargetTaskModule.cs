using GameConfig;
using Hotfix.Utils;
using HotfixLogic.Match;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    public class TargetTaskModule : IModuleAwake, IModuleDestroy
    {
        private TargetTask _targetTask;
        public TargetTask TargetTask => _targetTask;

        private int _targetItemID = 0;
        public int TargetItemID => _targetItemID;

        public void Awake(object parameter)
        {
        }

        public void Destroy()
        {
        }

        public void SetTargetTask(TargetTask targetTask)
        {
            _targetTask = targetTask;
            switch (_targetTask.objective_type) {
                case (int)TargetTaskType.Normal:
                    _targetItemID = 1;
                    break;
                case (int)TargetTaskType.Rocket:
                    _targetItemID = 8;
                    break;
                case (int)TargetTaskType.Bomb:
                    _targetItemID = 9;
                    break;
                case (int)TargetTaskType.LightBall:
                    _targetItemID = 10;
                    break;
                case (int)TargetTaskType.Square:
                    _targetItemID = -1;
                    break;
            }
        }

        public bool TargetTaskIsOpen() {
            if (_targetTask == null) return false;

            long timestamp = CommonUtil.GetNowTime();
            return timestamp < _targetTask.end_time && TargetTaskLvOpen();
        }

        public void GetTargetTaskRewardFinish() {
            _targetTask.wait_reward_ids.Clear();
        }

        public bool TargetTaskLvOpen() {
			var openLv = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("TargetTaskOpenLv");
			return MatchManager.Instance.MaxLevel >= openLv;
        }

        public int GetTargetNum(int rewardID, int rewardType) {
            Logger.Debug("GetTargetNum rewardID: " + rewardID + " rewardType: " + rewardType);
            return ConfigMemoryPool.Get<ObjectiveRewardDB>().GetObjectiveRewardTargetCount(rewardID, rewardType);
        }

    }
}
