/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 集卡配置表.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class PackThemeDB : ConfigBase
    {
        private PackTheme[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new PackTheme[]
            {
                new(themeId: 10000, themeName: "gujiDream", themeName_cn: LocalizationPool.Get("PackTheme/59c0"), themeMedalID: "1601", themeMedalName: "Medal01", themeDateBegin: "2025-05-01 00:00", themeDateEnd: "2025-08-01 00:00")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly PackTheme this[int themeId]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(themeId, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[PackTheme] themeId: {themeId} not found");
                return ref _data[idx];
            }
        }
        
        public PackTheme[] All => _data;
        
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
                _idToIdx[_data[i].themeId] = i;
            }
        }
    }
}