/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 商城配置表.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class CoinShopItemsDB : ConfigBase
    {
        private CoinShopItems[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new CoinShopItems[]
            {
                new(Id: 103, name: "eArrow1", alias: LocalizationPool.Get("CoinShopItems/6560"), coin_price: 1850, reward: "eliminateArrow*1", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 104, name: "eArrow3", alias: LocalizationPool.Get("CoinShopItems/6297"), coin_price: 5500, reward: "eliminateArrow*3", label: CoinExchangeLabType.More, type: 1, limit: ""),
                new(Id: 105, name: "eArrow10", alias: LocalizationPool.Get("CoinShopItems/c7b9"), coin_price: 17500, reward: "eliminateArrow*10", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 106, name: "eBullet1", alias: LocalizationPool.Get("CoinShopItems/857e"), coin_price: 1850, reward: "eliminateBullet*1", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 107, name: "eBullet3", alias: LocalizationPool.Get("CoinShopItems/3fe2"), coin_price: 5500, reward: "eliminateBullet*3", label: CoinExchangeLabType.More, type: 1, limit: ""),
                new(Id: 108, name: "eBullet10", alias: LocalizationPool.Get("CoinShopItems/2507"), coin_price: 17500, reward: "eliminateBullet*10", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 109, name: "eColored1", alias: LocalizationPool.Get("CoinShopItems/5b99"), coin_price: 1680, reward: "eliminateColored*1", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 110, name: "eColored3", alias: LocalizationPool.Get("CoinShopItems/d150"), coin_price: 5000, reward: "eliminateColored*3", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 111, name: "eColored10", alias: LocalizationPool.Get("CoinShopItems/5ffa"), coin_price: 15800, reward: "eliminateColored*10", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 112, name: "eHammer1", alias: LocalizationPool.Get("CoinShopItems/6551"), coin_price: 1280, reward: "eliminateHammer*1", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 113, name: "eHammer3", alias: LocalizationPool.Get("CoinShopItems/c94d"), coin_price: 3800, reward: "eliminateHammer*3", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 114, name: "eHammer10", alias: LocalizationPool.Get("CoinShopItems/5e89"), coin_price: 12000, reward: "eliminateHammer*10", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 115, name: "eDice1", alias: LocalizationPool.Get("CoinShopItems/ea48"), coin_price: 1280, reward: "eliminateDice*1", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 116, name: "eDice3", alias: LocalizationPool.Get("CoinShopItems/45b8"), coin_price: 3800, reward: "eliminateDice*3", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 117, name: "eDice10", alias: LocalizationPool.Get("CoinShopItems/fce0"), coin_price: 11800, reward: "eliminateDice*10", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 118, name: "eLive1", alias: LocalizationPool.Get("CoinShopItems/d6ce"), coin_price: 500, reward: "live*5", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 119, name: "eLive3", alias: LocalizationPool.Get("CoinShopItems/b5d8"), coin_price: 1500, reward: "live*15", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 120, name: "eLive10", alias: LocalizationPool.Get("CoinShopItems/5d9e"), coin_price: 4500, reward: "live*50", label: CoinExchangeLabType.Discount, type: 1, limit: ""),
                new(Id: 121, name: "eRock1", alias: LocalizationPool.Get("CoinShopItems/5c9d"), coin_price: 600, reward: "eliminateRock*1", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 122, name: "eRock3", alias: LocalizationPool.Get("CoinShopItems/e9cb"), coin_price: 1750, reward: "eliminateRock*3", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 123, name: "eRock10", alias: LocalizationPool.Get("CoinShopItems/1828"), coin_price: 5500, reward: "eliminateRock*10", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 124, name: "eBomb1", alias: LocalizationPool.Get("CoinShopItems/3fc6"), coin_price: 990, reward: "eliminateBoom*1", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 125, name: "eBomb3", alias: LocalizationPool.Get("CoinShopItems/9ab7"), coin_price: 2900, reward: "eliminateBoom*3", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 126, name: "eBomb10", alias: LocalizationPool.Get("CoinShopItems/4700"), coin_price: 9500, reward: "eliminateBoom*10", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 127, name: "eBall1", alias: LocalizationPool.Get("CoinShopItems/653e"), coin_price: 1260, reward: "eliminateBall*1", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 128, name: "eBall3", alias: LocalizationPool.Get("CoinShopItems/609a"), coin_price: 3700, reward: "eliminateBall*2", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 129, name: "eBall10", alias: LocalizationPool.Get("CoinShopItems/89f5"), coin_price: 11600, reward: "eliminateBall*5", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 130, name: "eLoupe1", alias: LocalizationPool.Get("CoinShopItems/4688"), coin_price: 1080, reward: "puzzleLoupe*1", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 131, name: "eLoupe3", alias: LocalizationPool.Get("CoinShopItems/220f"), coin_price: 3200, reward: "puzzleLoupe*3", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 132, name: "eLoupe10", alias: LocalizationPool.Get("CoinShopItems/d199"), coin_price: 9800, reward: "puzzleLoupe*10", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 133, name: "ePosition1", alias: LocalizationPool.Get("CoinShopItems/3616"), coin_price: 990, reward: "puzzlePosition*1", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 134, name: "ePosition3", alias: LocalizationPool.Get("CoinShopItems/adce"), coin_price: 2900, reward: "puzzlePosition*3", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 135, name: "ePosition10", alias: LocalizationPool.Get("CoinShopItems/0134"), coin_price: 9500, reward: "puzzlePosition*10", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 136, name: "eVacuum1", alias: LocalizationPool.Get("CoinShopItems/b560"), coin_price: 1280, reward: "puzzleVacuum*1", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 137, name: "eVacuum3", alias: LocalizationPool.Get("CoinShopItems/a9a4"), coin_price: 3800, reward: "puzzleVacuum*3", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 138, name: "eVacuum0", alias: LocalizationPool.Get("CoinShopItems/864c"), coin_price: 11800, reward: "puzzleVacuum*10", label: CoinExchangeLabType.None, type: 1, limit: ""),
                new(Id: 139, name: "eHead3", alias: LocalizationPool.Get("CoinShopItems/9bfb"), coin_price: 10000, reward: "Head03*1", label: CoinExchangeLabType.None, type: 2, limit: ""),
                new(Id: 140, name: "eHead4", alias: LocalizationPool.Get("CoinShopItems/2c36"), coin_price: 10000, reward: "Head04*1", label: CoinExchangeLabType.None, type: 2, limit: ""),
                new(Id: 141, name: "eFrame5", alias: LocalizationPool.Get("CoinShopItems/f82a"), coin_price: 10000, reward: "Frame05*1", label: CoinExchangeLabType.None, type: 2, limit: ""),
                new(Id: 142, name: "eFrame6", alias: LocalizationPool.Get("CoinShopItems/be7e"), coin_price: 10000, reward: "Frame06*1", label: CoinExchangeLabType.None, type: 2, limit: "")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly CoinShopItems this[int Id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(Id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[CoinShopItems] Id: {Id} not found");
                return ref _data[idx];
            }
        }
        
        public CoinShopItems[] All => _data;
        
        public int Count => _data.Length;
        
        public override void Dispose()
        {
            _data = null;
            OnDispose();
        }
        
        private void MakeIdToIdx()
        {
            _idToIdx = new Dictionary<int,int>(_data.Length);
            for (int i = 0; i < _data.Length; i++)
            {
                _idToIdx[_data[i].Id] = i;
            }
        }
    }
}