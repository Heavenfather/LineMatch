/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: Loading配置.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class LoadingDB : ConfigBase
    {
        private Loading[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new Loading[]
            {
                new(id: 1, loadingConst: new LoadingConst(startX: 0.0f, startY: 0.0f, vCount: 0, hCount: 0, size: 0.0f, htmlColor: "#ffffff", imgName: "none"), desc: LocalizationPool.Get("Loading/591a"), effName: "CommonLoading_eff_sgtsqz"),
                new(id: 2, loadingConst: new LoadingConst(startX: 0.0f, startY: 0.0f, vCount: 0, hCount: 0, size: 0.0f, htmlColor: "#ffffff", imgName: "none"), desc: LocalizationPool.Get("Loading/301b"), effName: "CommonLoading_eff_xchhj"),
                new(id: 3, loadingConst: new LoadingConst(startX: 0.0f, startY: 0.0f, vCount: 0, hCount: 0, size: 0.0f, htmlColor: "#ffffff", imgName: "none"), desc: LocalizationPool.Get("Loading/bf65"), effName: "CommonLoading_eff_xcshj"),
                new(id: 4, loadingConst: new LoadingConst(startX: 0.0f, startY: 0.0f, vCount: 0, hCount: 0, size: 0.0f, htmlColor: "#ffffff", imgName: "none"), desc: LocalizationPool.Get("Loading/41c5"), effName: "CommonLoading_eff_xczy"),
                new(id: 5, loadingConst: new LoadingConst(startX: 0.0f, startY: 0.0f, vCount: 0, hCount: 0, size: 0.0f, htmlColor: "#ffffff", imgName: "none"), desc: LocalizationPool.Get("Loading/2f6a"), effName: "CommonLoading_eff_xczy"),
                new(id: 6, loadingConst: new LoadingConst(startX: 0.0f, startY: 0.0f, vCount: 0, hCount: 0, size: 0.0f, htmlColor: "#ffffff", imgName: "none"), desc: LocalizationPool.Get("Loading/2706"), effName: "CommonLoading_eff_xccq"),
                new(id: 7, loadingConst: new LoadingConst(startX: 0.0f, startY: 0.0f, vCount: 0, hCount: 0, size: 0.0f, htmlColor: "#ffffff", imgName: "none"), desc: LocalizationPool.Get("Loading/f07f"), effName: "CommonLoading_eff_xccq"),
                new(id: 8, loadingConst: new LoadingConst(startX: 178.0f, startY: -177.0f, vCount: 4, hCount: 4, size: 60.0f, htmlColor: "#9b6add", imgName: "none"), desc: LocalizationPool.Get("Loading/6c4e"), effName: "CommonLoading_eff_gnjfxg")
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly Loading this[int id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[Loading] id: {id} not found");
                return ref _data[idx];
            }
        }
        
        public Loading[] All => _data;
        
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
                _idToIdx[_data[i].id] = i;
            }
        }
    }
}