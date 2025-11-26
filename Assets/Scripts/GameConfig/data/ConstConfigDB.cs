/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 通用配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class ConstConfigDB : ConfigBase
    {
        private ConstConfig[] _data;
        private Dictionary<string, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new ConstConfig[]
            {
                new(id: "None", intValue: 0, strValue: ""),
                new(id: "GridWidth", intValue: 4, strValue: ""),
                new(id: "MatchBeginItem", intValue: 0, strValue: "eliminateRock|eliminateBoom|eliminateBall"),
                new(id: "MBeginBoosterUnlock", intValue: 7, strValue: ""),
                new(id: "MBeginBoosterUnlockReward", intValue: 0, strValue: "eliminateRock*3|eliminateBoom*3|eliminateBall*3"),
                new(id: "WinStreakLV", intValue: 8, strValue: ""),
                new(id: "MatchStarPercent", intValue: 0, strValue: "30|60|100"),
                new(id: "MatchMaxLevel", intValue: 250, strValue: ""),
                new(id: "LiveRestorTime", intValue: 6, strValue: ""),
                new(id: "LiveConsum", intValue: 5, strValue: ""),
                new(id: "VibrationForce", intValue: 3, strValue: ""),
                new(id: "VibrationForceIOS", intValue: 1, strValue: ""),
                new(id: "CardPackCount", intValue: 9, strValue: ""),
                new(id: "MatchBoosterNeighbor", intValue: 0, strValue: "50|30|20"),
                new(id: "AdReviveCount", intValue: 3, strValue: ""),
                new(id: "AdReviveStep", intValue: 2, strValue: ""),
                new(id: "AdResultMulti", intValue: 0, strValue: "12_80|15_15|20_4|30_1"),
                new(id: "OneYuanDay", intValue: 3, strValue: ""),
                new(id: "OneYuanPopLv", intValue: 0, strValue: "15|200"),
                new(id: "OneYuanShopID", intValue: 10001, strValue: ""),
                new(id: "ShouChongPopLv", intValue: 0, strValue: "20|500"),
                new(id: "ShouChongShopID", intValue: 0, strValue: "10002_10003_10004|10002_10003_10005"),
                new(id: "TrainMasterOpenLv", intValue: 40, strValue: ""),
                new(id: "InviteOpenLv", intValue: 16, strValue: ""),
                new(id: "InviteDurationDay", intValue: 3, strValue: ""),
                new(id: "InvitePopMaxLv", intValue: 100, strValue: ""),
                new(id: "TargetTaskOpenLv", intValue: 5, strValue: ""),
                new(id: "MatchBoosterUnlockLv", intValue: 0, strValue: "eliminateDice_8|eliminateHammer_15|eliminateArrow_19|eliminateBullet_13|eliminateColored_17"),
                new(id: "MBeginPanelTips", intValue: 0, strValue: "30|5"),
                new(id: "TreasureOpenLv", intValue: 40, strValue: ""),
                new(id: "MatchLastStepPromptLV", intValue: 20, strValue: ""),
                new(id: "FeedbackOpenLV", intValue: 10, strValue: ""),
                new(id: "MatchLevelTips", intValue: 50, strValue: ""),
                new(id: "DailyFeedbackCount", intValue: 3, strValue: ""),
                new(id: "MatchLineCCount", intValue: 13, strValue: ""),
                new(id: "ContinueLevel", intValue: 5, strValue: ""),
                new(id: "NewPlayerLoadingLV", intValue: 7, strValue: ""),
                new(id: "NewPlayerLoadingIds", intValue: 0, strValue: "1")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly ConstConfig this[string id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[ConstConfig] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public ConstConfig[] All => _data;
        
        public int Count => _data.Length;
        
        public override void Dispose()
        {
            _data = null;
            OnDispose();
        }
        
        private void MakeIdToIdx()
        {
            _idToIdx = new Dictionary<string,int>(_data.Length);
            for (int i = 0; i < _data.Length; i++)
            {
                _idToIdx[_data[i].id] = i;
            }
        }
    }
}