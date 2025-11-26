/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 装扮配置.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class DressEnumDB : ConfigBase
    {
        private DressEnum[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new DressEnum[]
            {
                new(Id: 1001, tags: ItemEnumType.Head, buyType: 0, itemName: "Head01"),
                new(Id: 1002, tags: ItemEnumType.Head, buyType: 0, itemName: "Head02"),
                new(Id: 1003, tags: ItemEnumType.Head, buyType: 1, itemName: "Head03"),
                new(Id: 1004, tags: ItemEnumType.Head, buyType: 1, itemName: "Head04"),
                new(Id: 1005, tags: ItemEnumType.Head, buyType: 2, itemName: "Head05"),
                new(Id: 1006, tags: ItemEnumType.Head, buyType: 2, itemName: "Head06"),
                new(Id: 1007, tags: ItemEnumType.Head, buyType: 2, itemName: "Head07"),
                new(Id: 1008, tags: ItemEnumType.Head, buyType: 2, itemName: "Head08"),
                new(Id: 1009, tags: ItemEnumType.Head, buyType: 2, itemName: "Head09"),
                new(Id: 1010, tags: ItemEnumType.Head, buyType: 2, itemName: "Head10"),
                new(Id: 1011, tags: ItemEnumType.Head, buyType: 2, itemName: "Head11"),
                new(Id: 1012, tags: ItemEnumType.Head, buyType: 2, itemName: "Head12"),
                new(Id: 1013, tags: ItemEnumType.Head, buyType: 2, itemName: "Head13"),
                new(Id: 1014, tags: ItemEnumType.Head, buyType: 2, itemName: "Head14"),
                new(Id: 1015, tags: ItemEnumType.Head, buyType: 2, itemName: "Head15"),
                new(Id: 1016, tags: ItemEnumType.Head, buyType: 2, itemName: "Head16"),
                new(Id: 1017, tags: ItemEnumType.Head, buyType: 2, itemName: "Head17"),
                new(Id: 1018, tags: ItemEnumType.Head, buyType: 2, itemName: "Head18"),
                new(Id: 1019, tags: ItemEnumType.Head, buyType: 2, itemName: "Head19"),
                new(Id: 1020, tags: ItemEnumType.Head, buyType: 2, itemName: "Head20"),
                new(Id: 1021, tags: ItemEnumType.Head, buyType: 2, itemName: "Head21"),
                new(Id: 1022, tags: ItemEnumType.Head, buyType: 2, itemName: "Head22"),
                new(Id: 1023, tags: ItemEnumType.Head, buyType: 2, itemName: "Head23"),
                new(Id: 1024, tags: ItemEnumType.Head, buyType: 2, itemName: "Head24"),
                new(Id: 1025, tags: ItemEnumType.Head, buyType: 2, itemName: "Head25"),
                new(Id: 1026, tags: ItemEnumType.Head, buyType: 2, itemName: "Head26"),
                new(Id: 1027, tags: ItemEnumType.Head, buyType: 2, itemName: "Head27"),
                new(Id: 1028, tags: ItemEnumType.Head, buyType: 2, itemName: "Head28"),
                new(Id: 1101, tags: ItemEnumType.HeadFrame, buyType: 0, itemName: "Frame01"),
                new(Id: 1102, tags: ItemEnumType.HeadFrame, buyType: 0, itemName: "Frame02"),
                new(Id: 1105, tags: ItemEnumType.HeadFrame, buyType: 1, itemName: "Frame05"),
                new(Id: 1106, tags: ItemEnumType.HeadFrame, buyType: 1, itemName: "Frame06"),
                new(Id: 1107, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame07"),
                new(Id: 1108, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame08"),
                new(Id: 1109, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame09"),
                new(Id: 1110, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame10"),
                new(Id: 1111, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame11"),
                new(Id: 1112, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame12"),
                new(Id: 1115, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame15"),
                new(Id: 1116, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame16"),
                new(Id: 1117, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame17"),
                new(Id: 1118, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame18"),
                new(Id: 1121, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame21"),
                new(Id: 1122, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame22"),
                new(Id: 1123, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame23"),
                new(Id: 1124, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame24"),
                new(Id: 1125, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame25"),
                new(Id: 1126, tags: ItemEnumType.HeadFrame, buyType: 2, itemName: "Frame26"),
                new(Id: 1201, tags: ItemEnumType.Name, buyType: 0, itemName: "NameColor01"),
                new(Id: 1202, tags: ItemEnumType.Name, buyType: 2, itemName: "NameColor02"),
                new(Id: 1203, tags: ItemEnumType.Name, buyType: 2, itemName: "NameColor03"),
                new(Id: 1204, tags: ItemEnumType.Name, buyType: 2, itemName: "NameColor04"),
                new(Id: 1301, tags: ItemEnumType.Medal, buyType: 2, itemName: "Medal01")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly DressEnum this[int Id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(Id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[DressEnum] Id: {Id} not found");
                return ref _data[idx];
            }
        }
        
        public DressEnum[] All => _data;
        
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