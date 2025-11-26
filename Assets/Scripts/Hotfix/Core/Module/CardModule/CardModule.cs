using System.Collections.Generic;
using System.Linq;
using GameConfig;
using GameCore.LitJson;
using GameCore.SDK;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixLogic;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    public class CardModule : IModuleAwake, IModuleDestroy
    {
        
        // 全收集奖励领奖状态
        private int _totalRewardState = 0;
        public int TotalRewardState => _totalRewardState;

        // 全收集奖励ID
        private int _totalRewardID = 1001;
        public int TotalRewardID => _totalRewardID;

        // 拥有的星星数量
        private int _startCount;
        public int StartCount => _startCount;

        // 主题ID
        private int _themeID;
        public int ThemeID => _themeID;

        // 结束时间
        private int _endTime;
        public int EndTime => _endTime;

        // 剩余分享次数
        private int _leftShareCount = 0;
        public int LeftShareCount => _leftShareCount;

        // 每日分享上线
        private int _shareLimitCount = 3;
        public int ShareLimitCount => _shareLimitCount;

        // 我拥有的卡片
        private Dictionary<int, int> _myCardDatas;
        public Dictionary<int, int> MyCardDatas => _myCardDatas;

        // 卡包列表
        private List<PackItemsReward> _carPackList;
        public List<PackItemsReward> CarPackList => _carPackList;

        // 打包对应的卡片字典
        private Dictionary<int, List<Card>> _carDict;
        public Dictionary<int, List<Card>> CarDict => _carDict;

        // 卡包奖励领奖状态
        private Dictionary<int, int> _packStateDict;

        // 星星奖励领奖状态
        private Dictionary<int, int> _starStateDict;

        // 收集的卡片进度
        private Dictionary<int, int> _curPackProgress;

        // 收藏的卡片列表
        private Dictionary<int, List<int>> _collectCardDict;
        public Dictionary<int, List<int>> CollectCardDict => _collectCardDict;

        // 本地金卡记录
        private List<int> _localGoldCard;
        public List<int> LocalGoldCard => _localGoldCard;

        private bool _hasNewGoldCard = false;
        public bool HasNewGoldCard => _hasNewGoldCard;

        private int _timerID;

        private CardDB _cardConfig;


        public void Awake(object parameter)
        {
            _cardConfig = ConfigMemoryPool.Get<CardDB>();

            _starStateDict = new Dictionary<int, int>();
            _packStateDict = new Dictionary<int, int>();
            _myCardDatas = new Dictionary<int, int>();
            _curPackProgress = new Dictionary<int, int>();
            _carPackList = new List<PackItemsReward>();
            _carDict = new Dictionary<int, List<Card>>();
            _collectCardDict = new Dictionary<int, List<int>>();

            InitLocalGoalCard();
        }

        public void Destroy()
        {
        }

        public void InitLocalGoalCard() {
            var goalCardStr = PlayerPrefsUtil.GetString("GoalCard", "");
            if (goalCardStr != "") {
                _localGoldCard = JsonMapper.ToObject<List<int>>(goalCardStr);
            }
        }

        public void SetThemeID(int themeID, int endTime)
        {
            _themeID = themeID;
            _endTime = endTime;

            var cardArr = _cardConfig.All;
            foreach (var card in cardArr) {
                if (card.themeId == _themeID) {
                    if (!_carDict.ContainsKey(card.packId)) {
                        _carDict.Add(card.packId, new List<Card>());
                    }
                    _carDict[card.packId].Add(card);

                    if (_carPackList.FindIndex(x => x.packId == card.packId) == -1) {
                        _carPackList.Add(ConfigMemoryPool.Get<PackItemsRewardDB>()[card.packId]);
                    }
                }
            }

            UpdateTimeOut();
        }

        public void SetTotalRewardID(int totalRewardID) {            
            _totalRewardID = totalRewardID;
        }

        public void SetTotalRewardState(int totalRewardState) {
            _totalRewardState = totalRewardState;
        }
        public bool IsCompleteTotalReward() {
            return _totalRewardState == 2;
        }

        public void SetPackRewardState(int packId, int rewardState) {
            if (!_packStateDict.ContainsKey(packId)) {
                _packStateDict.Add(packId, rewardState);
            } else if (_packStateDict[packId] != rewardState) {
                _packStateDict[packId] = rewardState;
            }
        }

        public void SetStarRewardState(int starId, int rewardState) {
            if (!_starStateDict.ContainsKey(starId)) {
                _starStateDict.Add(starId, rewardState);
            } else if (_starStateDict[starId] != rewardState) {
                _starStateDict[starId] = rewardState;
            }
        }

        public void SetStartCount(int startCount) {
            _startCount = startCount;
        }

        public bool IsCompletePackReward(int packId) {
            if (!_packStateDict.ContainsKey(packId)) return false;

            return _packStateDict[packId] == 2;
        }

        public bool IsCompleteStarReward(int starId) {
            if (!_starStateDict.ContainsKey(starId)) return false;
            return _starStateDict[starId] == 2;
        }

        public void SetStarRewardComplete(int starId) {
            if (!_starStateDict.ContainsKey(starId)) {
                _starStateDict.Add(starId, 2);
            }
            _starStateDict[starId] = 2;
        }

        public void AddCardData(int cardID, int cardNum, bool isFire = true) {
            var cardCfg = ConfigMemoryPool.Get<CardDB>()[cardID];

            if (!_myCardDatas.ContainsKey(cardID)) {
                _myCardDatas.Add(cardID, 0);
                AddPackProgress(cardCfg.packId);

                AddCollectCard(cardID);
            }

            _myCardDatas[cardID] += cardNum;
            SetStartCount(_startCount + cardCfg.star * cardNum);

            if (isFire) G.EventModule.DispatchEvent(GameEventDefine.OnCardUpdate);
        }

        private void AddPackProgress(int packId) {
            if (_curPackProgress.ContainsKey(packId)) {
                _curPackProgress[packId]++;
            } else {
                _curPackProgress.Add(packId, 1);
            }

            var packCount = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("CardPackCount");
            if (_curPackProgress[packId] > packCount) {
                _curPackProgress[packId] = packCount;
            }
        }

        public int GetCurPackProgress(int packId) {
            if (_curPackProgress.ContainsKey(packId)) {
                return _curPackProgress[packId];
            }
            
            return 0;
        }

        public int GetTotalProgress() {
            var totalProgress = 0;
            foreach (var packProgress in _curPackProgress) {
                totalProgress += packProgress.Value;
            }

            return totalProgress;
        }

        private void UpdateTimeOut() {
            if (_timerID != 0) return;

            if (CommonUtil.GetNowTime() > _endTime) return;

            _timerID = G.TimerModule.AddTimer(()=> {
                int leftTime = (int)(_endTime - CommonUtil.GetNowTime());
                G.EventModule.DispatchEvent(GameEventDefine.OnCardTimeOut, EventOneParam<int>.Create(leftTime));

                if (leftTime <= 0) {
                    G.TimerModule.RemoveTimer(_timerID);
                    _timerID = 0;
                    return;
                }
            }, 1.0f, true);
        }

        public int GetCardCount(int cardID) {
            if (!_myCardDatas.ContainsKey(cardID)) {
                return 0;
            }

            return _myCardDatas[cardID];
        }

        public void AddShareCount(int count = -1) {
            _leftShareCount += count;
        }

        public void SetCollectCardList(List<int> collectCardList) {
            if (collectCardList == null) return;

            // 本地没有记录金卡，则以服务器的为准
            if (_localGoldCard == null) {
                _localGoldCard = collectCardList;
                RecordGoalCard();
            }

            foreach (var cardID in collectCardList) {
                var themeID = _cardConfig[cardID].themeId;
                if (!_collectCardDict.ContainsKey(themeID)) {
                    _collectCardDict.Add(themeID, new List<int>());
                }
                _collectCardDict[themeID].Add(cardID);

                CheckNewGoldCard(cardID);
            }
        }

        public void AddCollectCard(int cardID) {
            var cardData = _cardConfig[cardID];
            if (cardData.star < 5 || !cardData.isGold) return;

            if (!_collectCardDict.ContainsKey(cardData.themeId)) {
                _collectCardDict.Add(cardData.themeId, new List<int>());
            }

            if (!_collectCardDict[cardData.themeId].Contains(cardID)) {
                _collectCardDict[cardData.themeId].Add(cardID);

                CheckNewGoldCard(cardID);
            }
        }

        public bool HasCollectCard(int cardID) {
            var cardData = _cardConfig[cardID];
            if (cardData.star < 5) return false;
            if (!_collectCardDict.ContainsKey(cardData.themeId)) return false;
            return _collectCardDict[cardData.themeId].Contains(cardID);
        }



        public void CheckNewPackReward() {
            foreach (var pack in _carPackList) {
                var packId = pack.packId;
                if (_packStateDict.ContainsKey(packId) && _packStateDict[packId] != 0) return ;

                var packTotalCount = CarDict[packId].Count;
                var curCardCount = GetCurPackProgress(packId);

                if (curCardCount >= packTotalCount) {
                    SetPackRewardState(packId, 1);
                    G.RedDotModule.AddRedDotCount(RedDotDefine.CardPackReward, 1);
                }
            }
        }

        private void CheckNewStarReward() {
            var rewardCfg = ConfigMemoryPool.Get<PackTotalRewardDB>();
            var keys = _starStateDict.Keys.ToArray();

            foreach (var key in keys) {
                if (_starStateDict.ContainsKey(key) && _starStateDict[key] != 0) continue;
                if (rewardCfg[key].StarNumber <= _startCount) {
                    SetStarRewardState(key, 1);
                    G.RedDotModule.AddRedDotCount(RedDotDefine.CardStar, 1);
                }
            }
        }

        private void CheckNewTotalReward() {
			if (G.CardModule.IsCompleteTotalReward()) return;
			bool collectFinish = GetTotalProgress() >= G.CardModule.CarPackList.Count * ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("CardPackCount");
            if (collectFinish) {
                SetTotalRewardState(1);
                G.RedDotModule.AddRedDotCount(RedDotDefine.CardTotalReward, 1);
            }
        }

        public void UpdateAllRewardState() {
            CheckNewPackReward();
            CheckNewStarReward();
            CheckNewTotalReward();
        }

        private void RecordGoalCard() {
            if (!_hasNewGoldCard) return;

            var goalCardStr = JsonMapper.ToJson(_localGoldCard);
            PlayerPrefsUtil.SetString("GoalCard", goalCardStr);
        }

        public void UpdateLocalGoldCard() {
            foreach (var cardDict in _collectCardDict) {
                foreach (var cardID in cardDict.Value) {
                    if (!_localGoldCard.Contains(cardID)) {
                        _localGoldCard.Add(cardID);
                        _hasNewGoldCard = true;
                    }
                }
            }

            RecordGoalCard();

            _hasNewGoldCard = false;
            G.RedDotModule.SetRedDotCount(RedDotDefine.CardGoalCard, 0);
        }

        private void CheckNewGoldCard(int cardID) {
            if (_hasNewGoldCard) return;

            if (!_localGoldCard.Contains(cardID)) {
                _hasNewGoldCard = true;
                G.RedDotModule.AddRedDotCount(RedDotDefine.CardGoalCard);
            }
        }

        public bool IsNewGoalCard(int cardID) {
            return !_localGoldCard.Contains(cardID);
        }

        public void CheckGetShareCard() {
            Logger.Debug("CheckGetShareCard");

            SDKMgr.Instance.CallSDKMethod("getLaunchParams", "", "", (returnData) => {
                var param = returnData.Param;
                Logger.Debug("CheckGetShareCard param = " + param);

                if (string.IsNullOrEmpty(param) || param == "") return;
                
                var dict = JsonMapper.ToObject<Dictionary<string, string>>(param);
                if (dict == null) return;

                if (dict.ContainsKey("shareCode")) {
                    Logger.Debug("shareCode = " + dict["shareCode"]);
                    G.HttpModule.ReceiveShareCard(dict["shareCode"]);
                }
            });
        }
    }
}