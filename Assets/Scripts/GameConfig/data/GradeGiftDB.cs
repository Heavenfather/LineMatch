/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 商城配置表.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class GradeGiftDB : ConfigBase
    {
        private GradeGift[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new GradeGift[]
            {
                new(Id: 1, giftType: GradeGiftType.ResultCoin, shopID: 44),
                new(Id: 2, giftType: GradeGiftType.ResultCoin, shopID: 42),
                new(Id: 3, giftType: GradeGiftType.ResultCoin, shopID: 43),
                new(Id: 4, giftType: GradeGiftType.ResultCoin, shopID: 45),
                new(Id: 5, giftType: GradeGiftType.ResultCoin, shopID: 48),
                new(Id: 6, giftType: GradeGiftType.LiveLack, shopID: 46),
                new(Id: 7, giftType: GradeGiftType.LiveLack, shopID: 39),
                new(Id: 8, giftType: GradeGiftType.LiveLack, shopID: 40),
                new(Id: 9, giftType: GradeGiftType.LiveLack, shopID: 47),
                new(Id: 10, giftType: GradeGiftType.LiveLack, shopID: 49)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly GradeGift this[int Id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(Id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[GradeGift] Id: {Id} not found");
                return ref _data[idx];
            }
        }
        
        public GradeGift[] All => _data;
        
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