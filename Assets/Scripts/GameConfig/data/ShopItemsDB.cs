/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 商城配置表.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class ShopItemsDB : ConfigBase
    {
        private ShopItems[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new ShopItems[]
            {
                new(Id: -1, name: "adgift", alias: LocalizationPool.Get("ShopItems/ac67"), price: 0, tt_diamond: 0, reward: "coin*50", discount: 0, discountType: 0, giftType: 0, limit: "2-5", original_price: 0),
                new(Id: 1, name: "halfGift", alias: LocalizationPool.Get("ShopItems/aed9"), price: 3, tt_diamond: 30, reward: "coin*6000", discount: 50, discountType: 1, giftType: 2, limit: "2-1", original_price: 0),
                new(Id: 3, name: "CompanionGift", alias: LocalizationPool.Get("ShopItems/61eb"), price: 6, tt_diamond: 60, reward: "coin*2999|eliminateArrow*1|eliminateColored*1|liveBuff*10", discount: 80, discountType: 4, giftType: 2, limit: "", original_price: 0),
                new(Id: 4, name: "ExplorerGift", alias: LocalizationPool.Get("ShopItems/848c"), price: 12, tt_diamond: 120, reward: "coin*8888|eliminateBullet*1|eliminateColored*1|eliminateDice*1|eliminateHammer*1|liveBuff*10", discount: 75, discountType: 4, giftType: 2, limit: "", original_price: 0),
                new(Id: 5, name: "SustainmentGift", alias: LocalizationPool.Get("ShopItems/c454"), price: 30, tt_diamond: 300, reward: "coin*25999|eliminateArrow*2|eliminateHammer*2|eliminateColored*2|eliminateBullet*2|liveBuff*20", discount: 72, discountType: 4, giftType: 2, limit: "", original_price: 0),
                new(Id: 6, name: "StellarGift", alias: LocalizationPool.Get("ShopItems/5221"), price: 50, tt_diamond: 500, reward: "coin*49999|eliminateArrow*6|eliminateHammer*6|eliminateColored*6|eliminateBullet*6|liveBuff*40", discount: 72, discountType: 4, giftType: 2, limit: "", original_price: 0),
                new(Id: 7, name: "LegendaryGift", alias: LocalizationPool.Get("ShopItems/58ce"), price: 128, tt_diamond: 1280, reward: "coin*99999|eliminateArrow*12|eliminateHammer*12|eliminateColored*12|eliminateBullet*12|liveBuff*40", discount: 69, discountType: 4, giftType: 2, limit: "", original_price: 0),
                new(Id: 11, name: "coingift1", alias: LocalizationPool.Get("ShopItems/a322"), price: 3, tt_diamond: 30, reward: "coin*3000", discount: 0, discountType: 0, giftType: 1, limit: "", original_price: 0),
                new(Id: 12, name: "coingift2", alias: LocalizationPool.Get("ShopItems/a322"), price: 6, tt_diamond: 60, reward: "coin*6000", discount: 0, discountType: 0, giftType: 1, limit: "", original_price: 0),
                new(Id: 13, name: "coingift3", alias: LocalizationPool.Get("ShopItems/a322"), price: 12, tt_diamond: 120, reward: "coin*12000", discount: 0, discountType: 0, giftType: 1, limit: "", original_price: 0),
                new(Id: 14, name: "coingift4", alias: LocalizationPool.Get("ShopItems/a322"), price: 30, tt_diamond: 300, reward: "coin*30000", discount: 0, discountType: 0, giftType: 1, limit: "", original_price: 0),
                new(Id: 15, name: "coingift5", alias: LocalizationPool.Get("ShopItems/a322"), price: 68, tt_diamond: 680, reward: "coin*68000", discount: 0, discountType: 0, giftType: 1, limit: "", original_price: 0),
                new(Id: 16, name: "coingift6", alias: LocalizationPool.Get("ShopItems/a322"), price: 128, tt_diamond: 1280, reward: "coin*128000", discount: 0, discountType: 0, giftType: 1, limit: "", original_price: 0),
                new(Id: 17, name: "RockGift1", alias: LocalizationPool.Get("ShopItems/8a86"), price: 3, tt_diamond: 30, reward: "coin*2599|eliminateRock*1", discount: 0, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 18, name: "RockGift2", alias: LocalizationPool.Get("ShopItems/8a86"), price: 6, tt_diamond: 60, reward: "coin*4699|eliminateRock*3|liveBuff*10", discount: 70, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 19, name: "BoomGift1", alias: LocalizationPool.Get("ShopItems/5296"), price: 3, tt_diamond: 30, reward: "coin*2160|eliminateBoom*1", discount: 0, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 20, name: "BoomGift2", alias: LocalizationPool.Get("ShopItems/5296"), price: 6, tt_diamond: 60, reward: "coin*3399|eliminateBoom*3|liveBuff*10", discount: 70, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 21, name: "BallGift1", alias: LocalizationPool.Get("ShopItems/78ba"), price: 3, tt_diamond: 30, reward: "coin*1899|eliminateBall*1", discount: 0, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 22, name: "BallGift2", alias: LocalizationPool.Get("ShopItems/78ba"), price: 6, tt_diamond: 60, reward: "coin*2499|eliminateBall*3|liveBuff*10", discount: 70, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 23, name: "DiceGift1", alias: LocalizationPool.Get("ShopItems/213d"), price: 3, tt_diamond: 30, reward: "coin*1999|eliminateDice*1", discount: 0, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 24, name: "DiceGift2", alias: LocalizationPool.Get("ShopItems/213d"), price: 6, tt_diamond: 60, reward: "coin*2999|eliminateDice*3|liveBuff*10", discount: 70, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 25, name: "HammerGift1", alias: LocalizationPool.Get("ShopItems/3dc8"), price: 3, tt_diamond: 30, reward: "coin*1999|eliminateHammer*1", discount: 0, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 26, name: "HammerGift2", alias: LocalizationPool.Get("ShopItems/3dc8"), price: 6, tt_diamond: 60, reward: "coin*2999|eliminateHammer*3|liveBuff*10", discount: 70, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 27, name: "ArrowGift1", alias: LocalizationPool.Get("ShopItems/ecec"), price: 3, tt_diamond: 30, reward: "coin*1399|eliminateArrow*1", discount: 0, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 28, name: "ArrowGift2", alias: LocalizationPool.Get("ShopItems/ecec"), price: 6, tt_diamond: 60, reward: "coin*1899|eliminateArrow*3|liveBuff*10", discount: 70, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 29, name: "BulletGift1", alias: LocalizationPool.Get("ShopItems/b991"), price: 3, tt_diamond: 30, reward: "coin*1399|eliminateBullet*1", discount: 0, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 30, name: "BulletGift2", alias: LocalizationPool.Get("ShopItems/b991"), price: 6, tt_diamond: 60, reward: "coin*1899|eliminateBullet*3|liveBuff*10", discount: 70, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 31, name: "ColoredGift1", alias: LocalizationPool.Get("ShopItems/0c94"), price: 3, tt_diamond: 30, reward: "coin*1599|eliminateColored*1", discount: 0, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 32, name: "ColoredGift2", alias: LocalizationPool.Get("ShopItems/0c94"), price: 6, tt_diamond: 60, reward: "coin*1999|eliminateColored*3|liveBuff*10", discount: 70, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 33, name: "LoupeGift1", alias: LocalizationPool.Get("ShopItems/c233"), price: 3, tt_diamond: 30, reward: "coin*2199|puzzleLoupe*1", discount: 0, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 34, name: "LoupeGift2", alias: LocalizationPool.Get("ShopItems/c233"), price: 6, tt_diamond: 60, reward: "coin*3499|puzzleLoupe*3|liveBuff*10", discount: 70, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 35, name: "PositionGift1", alias: LocalizationPool.Get("ShopItems/44d2"), price: 3, tt_diamond: 30, reward: "coin*2299|puzzlePosition*1", discount: 0, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 36, name: "PositionGift2", alias: LocalizationPool.Get("ShopItems/44d2"), price: 6, tt_diamond: 60, reward: "coin*3699|puzzlePosition*3|liveBuff*10", discount: 70, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 37, name: "VacuumGift1", alias: LocalizationPool.Get("ShopItems/72e9"), price: 3, tt_diamond: 30, reward: "coin*1999|puzzleVacuum*1", discount: 0, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 38, name: "VacuumGift2", alias: LocalizationPool.Get("ShopItems/72e9"), price: 6, tt_diamond: 60, reward: "coin*2999|puzzleVacuum*3|liveBuff*10", discount: 70, discountType: 0, giftType: 3, limit: "", original_price: 0),
                new(Id: 39, name: "lifeGift1", alias: LocalizationPool.Get("ShopItems/f2c6"), price: 6, tt_diamond: 60, reward: "coin*1999|live*45", discount: 90, discountType: 0, giftType: 6, limit: "", original_price: 0),
                new(Id: 40, name: "lifeGift2", alias: LocalizationPool.Get("ShopItems/f2c6"), price: 12, tt_diamond: 120, reward: "coin*4999|live*100|liveBuff*20", discount: 80, discountType: 0, giftType: 6, limit: "", original_price: 0),
                new(Id: 41, name: "bankruptgift", alias: LocalizationPool.Get("ShopItems/ac92"), price: 12, tt_diamond: 120, reward: "coin*15000|liveBuff*20|eliminateHammerBuff*10", discount: 50, discountType: 0, giftType: 4, limit: "45689", original_price: 0),
                new(Id: 42, name: "insufficientcoin1", alias: LocalizationPool.Get("ShopItems/0168"), price: 6, tt_diamond: 60, reward: "coin*5999|eliminateHammer*3", discount: 90, discountType: 0, giftType: 6, limit: "", original_price: 0),
                new(Id: 43, name: "insufficientcoin2", alias: LocalizationPool.Get("ShopItems/0168"), price: 12, tt_diamond: 120, reward: "coin*12999|eliminateHammer*5|eliminateColored*5", discount: 80, discountType: 0, giftType: 6, limit: "", original_price: 0),
                new(Id: 44, name: "insufficientcoin3", alias: LocalizationPool.Get("ShopItems/0168"), price: 3, tt_diamond: 30, reward: "coin*2500|eliminateHammer*1", discount: 0, discountType: 0, giftType: 6, limit: "", original_price: 0),
                new(Id: 45, name: "insufficientcoin4", alias: LocalizationPool.Get("ShopItems/0168"), price: 30, tt_diamond: 300, reward: "coin*32999|eliminateHammer*15|eliminateColored*15", discount: 70, discountType: 0, giftType: 6, limit: "", original_price: 0),
                new(Id: 46, name: "lifeGift3", alias: LocalizationPool.Get("ShopItems/0168"), price: 3, tt_diamond: 30, reward: "coin*999|live*20", discount: 0, discountType: 0, giftType: 6, limit: "", original_price: 0),
                new(Id: 47, name: "lifeGift4", alias: LocalizationPool.Get("ShopItems/0168"), price: 30, tt_diamond: 300, reward: "coin*9999|live*320|liveBuff*60", discount: 70, discountType: 0, giftType: 6, limit: "", original_price: 0),
                new(Id: 48, name: "insufficientcoin5", alias: LocalizationPool.Get("ShopItems/0168"), price: 68, tt_diamond: 680, reward: "coin*73999|eliminateHammer*25|eliminateBoom*25", discount: 60, discountType: 0, giftType: 6, limit: "", original_price: 0),
                new(Id: 49, name: "lifeGift5", alias: LocalizationPool.Get("ShopItems/0168"), price: 68, tt_diamond: 680, reward: "coin*21999|live*700|liveBuff*180", discount: 60, discountType: 0, giftType: 6, limit: "", original_price: 0),
                new(Id: 1001, name: "spuerValueCard", alias: LocalizationPool.Get("ShopItems/15a4"), price: 30, tt_diamond: 0, reward: "coin*18000|liveBuff*10", discount: 0, discountType: 0, giftType: 5, limit: "", original_price: 0),
                new(Id: 1002, name: "supremeCard", alias: LocalizationPool.Get("ShopItems/a620"), price: 68, tt_diamond: 0, reward: "coin*45000|liveBuff*30", discount: 0, discountType: 0, giftType: 5, limit: "", original_price: 0),
                new(Id: 10001, name: "1yuangift", alias: LocalizationPool.Get("ShopItems/b8a0"), price: 1, tt_diamond: 10, reward: "coin*3999|liveBuff*20|eliminateBoom*1", discount: 5, discountType: 0, giftType: 7, limit: "1-1", original_price: 20),
                new(Id: 10002, name: "scgift1", alias: LocalizationPool.Get("ShopItems/9f5e"), price: 3, tt_diamond: 30, reward: "coin*4199|liveBuff*10", discount: 60, discountType: 0, giftType: 7, limit: "1-1", original_price: 0),
                new(Id: 10003, name: "scgift2", alias: LocalizationPool.Get("ShopItems/9f5e"), price: 6, tt_diamond: 60, reward: "coin*8999|liveBuff*20", discount: 50, discountType: 0, giftType: 7, limit: "1-1", original_price: 0),
                new(Id: 10004, name: "scgift3", alias: LocalizationPool.Get("ShopItems/9f5e"), price: 12, tt_diamond: 120, reward: "coin*21599|liveBuff*30|eliminateDice*1", discount: 40, discountType: 0, giftType: 7, limit: "1-1", original_price: 0),
                new(Id: 10005, name: "scgift4", alias: LocalizationPool.Get("ShopItems/9f5e"), price: 18, tt_diamond: 180, reward: "coin*35999|liveBuff*40|eliminateDice*1", discount: 35, discountType: 0, giftType: 7, limit: "1-1", original_price: 0),
                new(Id: 11001, name: LocalizationPool.Get("ShopItems/7b27"), alias: LocalizationPool.Get("ShopItems/3fa6"), price: 3, tt_diamond: 30, reward: "liveBuff*10", discount: 0, discountType: 0, giftType: 7, limit: "", original_price: 0),
                new(Id: 11002, name: LocalizationPool.Get("ShopItems/9851"), alias: LocalizationPool.Get("ShopItems/3fa6"), price: 6, tt_diamond: 60, reward: "coin*4499", discount: 0, discountType: 0, giftType: 7, limit: "", original_price: 0),
                new(Id: 11003, name: LocalizationPool.Get("ShopItems/15d6"), alias: LocalizationPool.Get("ShopItems/3fa6"), price: 12, tt_diamond: 120, reward: "liveBuff*20", discount: 0, discountType: 0, giftType: 7, limit: "", original_price: 0),
                new(Id: 11004, name: LocalizationPool.Get("ShopItems/3b1f"), alias: LocalizationPool.Get("ShopItems/3fa6"), price: 30, tt_diamond: 300, reward: "liveBuff*20|eliminateHammer*5", discount: 0, discountType: 0, giftType: 7, limit: "", original_price: 0),
                new(Id: 11005, name: LocalizationPool.Get("ShopItems/1cc8"), alias: LocalizationPool.Get("ShopItems/3fa6"), price: 68, tt_diamond: 680, reward: "coin*79999|eliminateBullet*12", discount: 0, discountType: 0, giftType: 7, limit: "", original_price: 0),
                new(Id: 11006, name: LocalizationPool.Get("ShopItems/1ade"), alias: LocalizationPool.Get("ShopItems/3fa6"), price: 128, tt_diamond: 1280, reward: "coin*129999|eliminateBall*20", discount: 0, discountType: 0, giftType: 7, limit: "", original_price: 0)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly ShopItems this[int Id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(Id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[ShopItems] Id: {Id} not found");
                return ref _data[idx];
            }
        }
        
        public ShopItems[] All => _data;
        
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