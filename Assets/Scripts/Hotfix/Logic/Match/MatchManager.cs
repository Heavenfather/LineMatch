using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using GameCore.Singleton;
using Hotfix.Define;
using HotfixCore.Module;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hotfix.EventParameter;
using Logger = GameCore.Log.Logger;
using Hotfix.Utils;
using DG.Tweening;
using GameCore.SDK;
using GameCore.Utils;

namespace HotfixLogic.Match
{
    public class MatchManager : LazySingleton<MatchManager>
    {
        private int _matchElementId = 1;
        private const string MatchSceneLocation = "scenes/matchscene";

        private List<GameObject> _sceneAllObjects = new List<GameObject>();

        private GridSystem _gridSystem;

        private ElementMapDB _configDB = ConfigMemoryPool.Get<ElementMapDB>();

        private List<int> _beginUseElements = new List<int>();
        private List<int> _winStreakElements = new List<int>();

        private int _curLevelID = 1;
        public int CurLevelID => _curLevelID;

        private int _maxLevel = 1;
        public int MaxLevel => _maxLevel;

        // 连胜的数据
        private int _winStreak = 0;

        // 失败的次数
        private int _lostCount = 0;
        public int LostCount => _lostCount;

        private bool _hadAdvRevive = false;
        public bool HadAdvRevive => _hadAdvRevive;

        private int _currentCalScore = 0;

        private int _totalScore = 0;
        public int TotalScore => _totalScore;

        // 最大连胜，用于连胜排行榜
        private int _maxWinStreak = 0;
        public int MaxWinStreak => _maxWinStreak;

        private float _rokerAudioTime = 0;
        private float _eliminateAudioTime = 0;
        private float _obstacleAudioTime = 0;
        private float _bombAudioTime = 0;

        private DateTime _maxLvBeginTime = default;
        private int _maxLvLostCount = 0;
        
        private MatchLevelType _currentMatchLevelType = MatchLevelType.A;
        
        /// <summary>
        /// 当前消除关卡类型
        /// </summary>
        public MatchLevelType CurrentMatchLevelType => _currentMatchLevelType;
        
        private MatchGameType _currentMatchGameType = MatchGameType.NormalMatch;

        public MatchGameType CurrentMatchGameType
        {
            get
            {
                if (_currentMatchLevelType == MatchLevelType.Editor && CurrentMatchLevelType == MatchLevelType.C)
                    return MatchGameType.TowDots; //C关就是towdots模式
                if (CurrentMatchLevelType == MatchLevelType.Editor)
                {
                    _currentMatchGameType = (MatchGameType)PlayerPrefsUtil.GetInt("EditorMatchGameType", 0);
                }
                return _currentMatchGameType;
            }
        }
        
        public bool HasFreeTimes {
            get
            {
                int freeItemCount = G.GameItemModule.GetItemCount((int)ItemDef.FreeRevive);
                return freeItemCount > 0;
            }
        }

        private Dictionary<int, int> _levelState = new Dictionary<int, int>();

        protected override void OnInitialized()
        {
        }

        /// <summary>
        /// 元素生成自增id
        /// </summary>
        /// <returns></returns>
        public int GenerateElementId()
        {
            return ++_matchElementId;
        }
        
        public bool IsEnterByEditor()
        {
            return _currentMatchLevelType == MatchLevelType.Editor;
        }

        public void SetLevelType(MatchLevelType levelType)
        {
            _currentMatchLevelType = levelType;
            if(levelType == MatchLevelType.Editor)
            {
                return;
            }
            if(_curLevelID > 100 && levelType != MatchLevelType.B)
                _currentMatchLevelType = MatchLevelType.B;
        }

        public void SetMatchGameType(MatchGameType gameType)
        {
            _currentMatchGameType = gameType;
        }

        /// <summary>
        /// 获取最大关卡数
        /// </summary>
        /// <returns></returns>
        public int GetMaxLevelConst()
        {
            var infos = G.ResourceModule.GetAssetInfos("MatchLevel", "");
            // Logger.Debug($"最大关卡数:{infos.Length}");
            return infos.Length;
        }
        
        /// <summary>
        /// 开始关卡玩法
        /// </summary>
        public UniTask Start(LevelData level,List<LevelData> guideLevels = null)
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            _sceneAllObjects.Clear();
            G.SceneModule.LoadScene(MatchSceneLocation, LoadSceneMode.Additive,
                callBack: async (scene) =>
                {
                    scene.GetRootGameObjects(_sceneAllObjects);
                    // for (int i = 0; i < _sceneAllObjects.Count; i++)
                    // {
                    //     Logger.Debug($"场景物体:{_sceneAllObjects[i].name}");
                    // }
                    TaskManager.Instance.SetCurrentPlayLevel(IsEnterByEditor() ? -1 : level.id);
                    for (int i = 0; i < _sceneAllObjects.Count; i++)
                    {
                        if (_sceneAllObjects[i].GetComponentInChildren<GridSystem>(true) != null)
                        {
                            _gridSystem = _sceneAllObjects[i].GetComponentInChildren<GridSystem>(true);
                            await _gridSystem.StartMatch(level,false,guideLevels);
                            tcs.TrySetResult();
                            break;
                        }
                    }
                },progressCallBack: (progress) =>
                {
                    CommonLoading.ShowLoading(LoadingEnum.Match, progress * 0.6f);
                });
            return tcs.Task;
        }

        public async UniTask Restart(bool isRestart = true)
        {
            await ClearData(isRestart);
        }

        /// <summary>
        /// 退出关卡
        /// </summary>
        public void Quit()
        {
            ClearData(false).Forget();
            G.SceneModule.UnloadAsync(MatchSceneLocation, () =>
            {
                // G.UIModule.SetSceneCamera(null);
            });
        }

        /// <summary>
        /// 单轮分数增加
        /// </summary>
        /// <param name="score"></param>
        public void AddScore(int score)
        {
            _currentCalScore += score;
            _totalScore += score;
        }

        /// <summary>
        /// 通知界面分数变更
        /// </summary>
        public void TickScoreChange()
        {
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchScoreChanged,
                EventOneParam<int>.Create(_currentCalScore));
                
            _currentCalScore = 0;
        }

        public async UniTask<Sprite> GetMatchElementSprite(int elementId)
        {
            return await G.ResourceModule.LoadAssetAsync<Sprite>(
                $"{MatchConst.SpritesAddressBase}/{_configDB[elementId].address}");
        }

        public void GetMatchElementSprite(int elementId, Action<Sprite> callback)
        {
            string address = _configDB[elementId].address_icon;
            if (string.IsNullOrEmpty(address))
            {
                address = _configDB[elementId].address;
            }
            G.ResourceModule
                .LoadAssetAsync<Sprite>($"{MatchConst.SpritesAddressBase}/{address}", callback)
                .Forget();
        }

        public void SetBeginUseElements(List<int> beginUseElements)
        {
            if (beginUseElements == null) return;
            _beginUseElements = beginUseElements;
        }

        public void SetWinStreakElements(List<int> winStreakElements)
        {
            if (winStreakElements == null) return;
            _winStreakElements = winStreakElements;
        }

        public string GetElementIconLocation(int elementId)
        {
            ElementMapDB mapDB = ConfigMemoryPool.Get<ElementMapDB>();
            // ElementSystem.Instance.IsNeedParseTarget(elementId, out elementId);
            ref readonly ElementMap config = ref mapDB[elementId];
            string location = config.address_icon;
            if (string.IsNullOrEmpty(location))
                location = config.address;
            return $"{MatchConst.SpritesAddressBase}/{location}";
        }

        public void GameBeginUseElements()
        {
            if (_beginUseElements.Count <= 0 && _winStreakElements.Count <= 0)
            {
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchUpdateSpecialElements,EventThreeParam<List<int>, List<int>, bool>.Create(null, null, false));
                return;
            }

            var useBooster = new List<int>();
            useBooster.AddRange(_beginUseElements);

            var winstreakBooster = new List<int>();
            winstreakBooster.AddRange(_winStreakElements);

            var param = EventThreeParam<List<int>, List<int>, bool>.Create(useBooster, winstreakBooster, false);
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchUpdateSpecialElements,param);
        }

        public void ClearBeginUseElements()
        {
            _beginUseElements.Clear();
            _winStreakElements.Clear();
        }

        public List<int> GetBeginUseElements()
        {
            return _beginUseElements;
        }

        public List<int> GetWinStreakElements()
        {
            return _winStreakElements;
        }

        public void SetWinStreak(int winStreak)
        {
            _winStreak = winStreak;

            if (_winStreak > _maxWinStreak) {
                SetMaxWinStreak(_winStreak);
            }

            G.UserInfoModule.CheckMaxWinStreak(_winStreak);
        }

        public int GetWinStreakBox() {
            var openLv = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("WinStreakLV");
            var boxLv = 0;
            if (MaxLevel < openLv) {
                return boxLv;
            } else if (_winStreak > MaxLevel - openLv + 1) {
                // 连胜宝箱从第八关开始算
                boxLv = MaxLevel - openLv + 1;
            } else {
                boxLv = _winStreak;
            }

            if (boxLv > 3) return 3;
            return boxLv;
        }

        public void SetMaxWinStreak(int maxWinStreak)
        {
            _maxWinStreak = maxWinStreak;
        }

        public void AddWinStreak()
        {
            SetWinStreak(++_winStreak);
        }
        
        public void SetMatchLevelID(int levelID)
        {
            _curLevelID = levelID;
            SetMaxLevelID(levelID);
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchUpdateLevel);
        }

        public void SetMaxLevelID(int levelID) {
            if (levelID > _maxLevel)
            {
                _maxLevel = levelID;
                SDKMgr.Instance.CallSDKMethod("setUserRecord",(_maxLevel - 1).ToString(), "", null);
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchUpdateMaxLevel);
            }
        }

        public MatchDifficulty GetMatchDifficulty(int difficulty)
        {
            if (difficulty <= 2)
            {
                return MatchDifficulty.Normal;
            }
            if (difficulty <= 4)
            {
                return MatchDifficulty.Hard;
            }

            return MatchDifficulty.Crazy;
        }

        public void AddLostCount()
        {
            _lostCount++;
        }

        public void SetHadAdvRevive(bool hadAdvRevive)
        {
            _hadAdvRevive = hadAdvRevive;
        }
        
        public async UniTask ClearData(bool isRestart)
        {
            _lostCount = 0;
            _totalScore = 0;
            _hadAdvRevive = false;
            if (_gridSystem != null)
            {
                if(isRestart)
                    await _gridSystem.Restart();
                else
                {
                    //清空池子
                    await ElementObjectPool.Instance.ClearAllPool();
                    await _gridSystem.Clear();
                }
            }
            _matchElementId = 1;
            TaskManager.Instance.ClearTaskCalculate();
        }

        public void PlayRockerAudio() {
            if (Time.time - _rokerAudioTime > 0.3f) {
                AudioUtil.PlayRocker();
                _rokerAudioTime = Time.time;
            } else {
                System.Random random = new System.Random();
                float randomNumber = (float)random.NextDouble() * 0.3f;
                var seq = DOTween.Sequence();
                seq.AppendInterval(randomNumber);
                seq.AppendCallback(() => {

                    
                    AudioUtil.PlayRocker();
                });
            }
        }

        public void PlayBombAudio() {
            if (Time.time - _bombAudioTime > 0.3f) {
                AudioUtil.PlayBomb();
                _bombAudioTime = Time.time;
            } else {
                System.Random random = new System.Random();
                float randomNumber = (float)random.NextDouble() * 0.3f;
                var seq = DOTween.Sequence();
                seq.AppendInterval(randomNumber);
                seq.AppendCallback(() => {
                    AudioUtil.PlayBomb();
                });
            }
        }

        public void PlayEliminateAudio() {
            if (Time.time - _eliminateAudioTime < 0.1f) return;
            AudioUtil.PlayMatchEliminate();
            _eliminateAudioTime = Time.time;
        }

        public int CalStarCountByScore(int score,int fullScore)
        {
            var cfg = ConfigMemoryPool.Get<ConstConfigDB>();
            var cfgStr = cfg.GetConfigStrVal("MatchStarPercent");
            var starPercent = cfgStr.Split('|');
            int star = 0;
            for (int i = 0; i < starPercent.Length; i++)
            {
                float percent = float.Parse(starPercent[i]);
                float percentScore = percent / 100f * fullScore;


                if (score >= percentScore)
                {
                    star = i + 1;
                }
            }

            Logger.Debug($"分数:{score},满分:{fullScore},星级:{star}");

            return star;
        }

        public int GetLevelCoinCount(int difficulty, int starCount)
        {
            int baseCoin = GetBaseCoin(difficulty);
            float multi = 1f + StarFactor(starCount);
            float coin = baseCoin * multi;

            Logger.Debug($"难度:{difficulty},星级:{starCount},基础金币:{baseCoin},星星系数:{multi},金币:{coin}");
            return (int)coin;
        }
        
        private float StarFactor(int star)
        {
            float multi = 0.5f + 0.2f * star;
            return multi * multi;
        }
        
        public int GetBaseCoin(int difficulty)
        {
            var matchDiff = GetMatchDifficulty(difficulty);
            return ConfigMemoryPool.Get<matchBaseCoinDB>().GetBaseCoin(matchDiff);
        }

        public void SetLevelState(List<LevelItem> levelState) {
            foreach (var item in levelState) {
                _levelState[item.stage_id] = item.star_num;
            }
            
        }

        public void SetLevelState(int stageId, int stageNum) {
            if (!_levelState.ContainsKey(stageId)) {
                _levelState.Add(stageId, stageNum);
            }

            if (_levelState[stageId] < stageNum) {
                _levelState[stageId] = stageNum;
            }
        }

        public int GetLevelStar(int stageId) {
            if (_levelState.ContainsKey(stageId)) {
                return _levelState[stageId];
            }

            return 0;
        }

        public async UniTask ReqLevelState() {
			// 获取历史关卡数据
            await UniTask.NextFrame();
			G.HttpModule.GetLevelData(1, MaxLevel);
        }

        public bool IsOpenWinStreak(int levelID) {
			var winStreakOpenLv = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("WinStreakLV");
			return MaxLevel >= winStreakOpenLv && levelID >= MaxLevel;
        }

        public bool IsShowBeginTips(int levelID) {
            if (MaxLevel < ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("MBeginBoosterUnlock")) return false;

            // 不是最大关卡，没有开始时间，失败次数为0，则不显示
            if (levelID != MaxLevel || _maxLvBeginTime == default || _maxLvLostCount == 0) return false;

            var cfgStr = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigStrVal("MBeginPanelTips").Split('|');
            var minute = int.Parse(cfgStr[0]);
            var count = int.Parse(cfgStr[1]);

            // 每关只会显示一次，既失败次数和配置次数一致的时候
            if (_maxLvLostCount != count) return false;

            var now = CommonUtil.GetNowDateTime();
            var diff = now - _maxLvBeginTime;
            var diffMinute = (int)diff.TotalMinutes;

            RecordBeginTipsData();

            return diffMinute <= minute;
        }

        // 记录提示数据
        private void RecordBeginTipsData() {
            var recordStr = MaxLevel + "|" + _maxLvBeginTime.ToString() + "|" + _maxLvLostCount;
            PlayerPrefsUtil.SetString("MatchBeginTipsLv", recordStr);

            ResetBeginTipsData();
        }

        public void InitBeginTipsData() {
            var str = PlayerPrefsUtil.GetString("MatchBeginTipsLv", "");
            if (str == "") return;

            var arr = str.Split('|');
            var lv = int.Parse(arr[0]);
            if (lv == MaxLevel) {
                _maxLvBeginTime = DateTime.Parse(arr[1]);
                _maxLvLostCount = int.Parse(arr[2]);
            }
        }

        public void SetBeginTipsData(int levelID = 0) {     
            if (levelID != MaxLevel) {
                ResetBeginTipsData();
                return;
            }
            
            if (_maxLvBeginTime == default) {
                _maxLvBeginTime = CommonUtil.GetNowDateTime();
            }
        }

        public void AddMaxLvLostCount(int levelID) {
            // 添加开始次数
            if (levelID == MaxLevel) {
                _maxLvLostCount++;
            } else {
                ResetBeginTipsData();
            }
        }

        private void ResetBeginTipsData() {
            _maxLvBeginTime = default;
            _maxLvLostCount = 0;
        }
    }
}