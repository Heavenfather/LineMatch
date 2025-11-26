using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Module;
using HotfixCore.MVC;
using HotfixLogic.Match;

namespace HotfixLogic
{
    public class MatchData : BaseModel
    {
        private Dictionary<int, List<int>> _levelAGuideLevelMap = new Dictionary<int, List<int>>()
        {
            [1] = new List<int>() { 1, 2, 4},
            [3] = new List<int>() { 5, }
        };

        private Dictionary<int, List<int>> _levelBGuideLevelMap = new Dictionary<int, List<int>>()
        {
            [1] = new List<int> { 1, 2, 4},
            [3] = new List<int>() { 5 },
            [6] = new List<int>() { 6 },
        };

        private bool _isMatchDone;

        public bool IsMatchDone
        {
            get => _isMatchDone;
            set
            {
                _isMatchDone = value;
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchDoneStateChanged, EventOneParam<bool>.Create(value));
            }
        }

        public MatchLevelType EnterLevelType { get; private set; }

        /// <summary>
        /// 剩余移动步数
        /// </summary>
        public int MoveStep { get; private set; }

        /// <summary>
        /// 当前关卡数据
        /// </summary>
        public LevelData CurrentLevelData { get; private set; }

        protected override async UniTask OnInitialized()
        {
            IsMatchDone = false;
            if (UserData[0] is LevelData levelData)
            {
                EnterLevelType = MatchLevelType.Editor;
                CurrentLevelData = levelData;
            }
            else if (UserData[0] is MatchLevelType matchType)
            {
                EnterLevelType = matchType;
                int levelId = MatchManager.Instance.CurLevelID;
                CurrentLevelData = await LevelManager.Instance.GetLevel(matchType, levelId);
            }

            InitData();
            LevelTargetSystem.Instance.ResetLevelTarget(CurrentLevelData);
        }

        public void Restart(LevelData levelData)
        {
            IsMatchDone = false;
            CurrentLevelData = levelData;
            InitData();
            LevelTargetSystem.Instance.ResetLevelTarget(CurrentLevelData, true);
        }

        public ECheckMatchResult CheckMatchState()
        {
            bool isFinish = true;
            bool haveCoinTarget = false;
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            foreach (var kp in LevelTargetSystem.Instance.TargetElements)
            {
                if (db.IsCoinTypeElement(kp.Key))
                {
                    haveCoinTarget = true;
                    break;
                }
            }

            foreach (var element in LevelTargetSystem.Instance.TargetElements)
            {
                if (element.Value > 0 && !db.IsCoinTypeElement(element.Key))
                {
                    isFinish = false;
                    break;
                }
            }

            if (haveCoinTarget && isFinish)
            {
                //如果有金币元素，将剩余步数全部用完才认为成功
                if (MoveStep > 0)
                {
                    return ECheckMatchResult.Actioning;
                }
                else
                {
                    return ECheckMatchResult.Success;
                }
            }

            if (MoveStep <= 0 && !isFinish)
            {
                return ECheckMatchResult.Failure;
            }

            if (isFinish)
            {
                return ECheckMatchResult.Success;
            }

            return ECheckMatchResult.Actioning;
        }

        public bool IsEnterByEditor()
        {
            return UserData[0] is LevelData;
        }

        public void ReduceMoveStep(int step)
        {
            MoveStep -= step;
        }

        public bool TryGetGuideLevel(out List<int> guideLevels)
        {
            int maxLevel = MatchManager.Instance.MaxLevel;
            if (maxLevel > CurrentLevelData.id)
            {
                guideLevels = null;
                return false;
            }

            if (EnterLevelType == MatchLevelType.A)
            {
                if (_levelAGuideLevelMap.TryGetValue(CurrentLevelData.id, out guideLevels))
                {
                    int saveValue =
                        PlayerPrefsUtil.GetInt($"{GamePrefsKey.MatchALevelGuideFinish}_{CurrentLevelData.id}");
                    if (saveValue == 0)
                    {
                        return true;
                    }
                }
            }

            if (EnterLevelType == MatchLevelType.B)
            {
                if (_levelBGuideLevelMap.TryGetValue(CurrentLevelData.id, out guideLevels))
                {
                    int saveValue =
                        PlayerPrefsUtil.GetInt($"{GamePrefsKey.MatchBLevelGuideFinish}_{CurrentLevelData.id}");
                    if (saveValue == 0)
                    {
                        return true;
                    }
                }
            }

            guideLevels = null;
            return false;
        }

        /// <summary>
        /// 获取引导id
        /// </summary>
        /// <param name="levelId"></param>
        /// <returns></returns>
        public int GetGuideIdByGuideLevelId(int levelId)
        {
            if(levelId >= 1 && levelId <= 6)
                return 2000 + levelId;
            return -1;
        }

        private void InitData()
        {
            MoveStep = CurrentLevelData.stepLimit;
        }

        protected override void OnDestroy()
        {
            MoveStep = 0;
            CurrentLevelData = null;
        }

        public void AddStep(int step)
        {
            IsMatchDone = false;
            MoveStep += step;
        }
    }
}