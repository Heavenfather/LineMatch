/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 道具对应棋子字典.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class ItemElementDictDB : ConfigBase
    {
        private ItemElementDict[] _data;
        private Dictionary<string, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new ItemElementDict[]
            {
                new(itemId: "eliminateRock", elementId: new List<int>() { 8, 11 }),
                new(itemId: "eliminateBoom", elementId: new List<int>() { 9 }),
                new(itemId: "eliminateBall", elementId: new List<int>() { 10 }),
                new(itemId: "eliminateRockBuff", elementId: new List<int>() { 8, 11 }),
                new(itemId: "eliminateBoomBuff", elementId: new List<int>() { 9 }),
                new(itemId: "eliminateBallBuff", elementId: new List<int>() { 10 }),
                new(itemId: "eliminateStarBombDots", elementId: new List<int>() { 12 }),
                new(itemId: "eliminateSearchDots", elementId: new List<int>() { 13 }),
                new(itemId: "eliminateHorizontalDots", elementId: new List<int>() { 14 }),
                new(itemId: "eliminateBombDots", elementId: new List<int>() { 15 }),
                new(itemId: "eliminateColoredDots", elementId: new List<int>() { 16 })
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly ItemElementDict this[string itemId]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(itemId, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[ItemElementDict] itemId: {itemId} not found");
                return ref _data[idx];
            }
        }
        
        public ItemElementDict[] All => _data;
        
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
                _idToIdx[_data[i].itemId] = i;
            }
        }
    }
}