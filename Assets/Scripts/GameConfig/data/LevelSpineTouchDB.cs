/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 大厅关卡图.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;
    using UnityEngine;


    public partial class LevelSpineTouchDB : ConfigBase
    {
        private LevelSpineTouch[] _data;
        private Dictionary<string, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new LevelSpineTouch[]
            {
                new(Id: "pdlxq_2", btnSize: new Vector2(x: 700.0f, y: 500.0f), btnPos: new Vector2(x: 0.0f, y: 354.0f)),
                new(Id: "pdlxq_3", btnSize: new Vector2(x: 500.0f, y: 500.0f), btnPos: new Vector2(x: -47.0f, y: 827.0f)),
                new(Id: "pdlxq_6", btnSize: new Vector2(x: 300.0f, y: 450.0f), btnPos: new Vector2(x: -148.0f, y: -87.0f)),
                new(Id: "pdlxq_9", btnSize: new Vector2(x: 450.0f, y: 700.0f), btnPos: new Vector2(x: -2.0f, y: -22.0f)),
                new(Id: "pdlxq_10", btnSize: new Vector2(x: 400.0f, y: 250.0f), btnPos: new Vector2(x: 61.0f, y: 265.0f)),
                new(Id: "fwrj_9", btnSize: new Vector2(x: 700.0f, y: 700.0f), btnPos: new Vector2(x: 0.0f, y: 200.0f)),
                new(Id: "fwrj_7", btnSize: new Vector2(x: 700.0f, y: 700.0f), btnPos: new Vector2(x: 0.0f, y: -236.0f)),
                new(Id: "fwrj_6", btnSize: new Vector2(x: 700.0f, y: 700.0f), btnPos: new Vector2(x: -5.0f, y: 12.0f)),
                new(Id: "fwrj_5", btnSize: new Vector2(x: 700.0f, y: 600.0f), btnPos: new Vector2(x: -103.0f, y: 600.0f)),
                new(Id: "fwrj_3", btnSize: new Vector2(x: 400.0f, y: 400.0f), btnPos: new Vector2(x: -159.0f, y: 617.0f)),
                new(Id: "fwrj_2", btnSize: new Vector2(x: 500.0f, y: 700.0f), btnPos: new Vector2(x: 5.0f, y: 99.0f)),
                new(Id: "fwrj_1", btnSize: new Vector2(x: 600.0f, y: 600.0f), btnPos: new Vector2(x: 0.0f, y: 0.0f)),
                new(Id: "jlls_2", btnSize: new Vector2(x: 700.0f, y: 800.0f), btnPos: new Vector2(x: 0.0f, y: 330.0f)),
                new(Id: "jlls_4", btnSize: new Vector2(x: 450.0f, y: 420.0f), btnPos: new Vector2(x: 127.0f, y: 120.0f)),
                new(Id: "jlls_6", btnSize: new Vector2(x: 300.0f, y: 400.0f), btnPos: new Vector2(x: 243.0f, y: -339.0f)),
                new(Id: "tmgf_9", btnSize: new Vector2(x: 600.0f, y: 400.0f), btnPos: new Vector2(x: 0.0f, y: -25.0f)),
                new(Id: "tmgf_7", btnSize: new Vector2(x: 700.0f, y: 700.0f), btnPos: new Vector2(x: 0.0f, y: 138.0f)),
                new(Id: "tmgf_5", btnSize: new Vector2(x: 550.0f, y: 800.0f), btnPos: new Vector2(x: 0.0f, y: 263.0f)),
                new(Id: "tmgf_2", btnSize: new Vector2(x: 650.0f, y: 650.0f), btnPos: new Vector2(x: -17.0f, y: 379.0f)),
                new(Id: "jsds_1", btnSize: new Vector2(x: 700.0f, y: 700.0f), btnPos: new Vector2(x: 0.0f, y: 50.0f)),
                new(Id: "jsds_2", btnSize: new Vector2(x: 300.0f, y: 400.0f), btnPos: new Vector2(x: -206.0f, y: 445.0f)),
                new(Id: "jsds_3", btnSize: new Vector2(x: 500.0f, y: 700.0f), btnPos: new Vector2(x: -5.0f, y: 124.0f)),
                new(Id: "jsds_5", btnSize: new Vector2(x: 450.0f, y: 450.0f), btnPos: new Vector2(x: 33.0f, y: 24.0f)),
                new(Id: "jsds_7", btnSize: new Vector2(x: 300.0f, y: 300.0f), btnPos: new Vector2(x: 256.0f, y: 414.0f)),
                new(Id: "yyjn_5", btnSize: new Vector2(x: 700.0f, y: 700.0f), btnPos: new Vector2(x: 77.0f, y: -95.0f)),
                new(Id: "yyjn_6", btnSize: new Vector2(x: 500.0f, y: 500.0f), btnPos: new Vector2(x: -131.0f, y: -51.0f)),
                new(Id: "yyjn_3", btnSize: new Vector2(x: 400.0f, y: 600.0f), btnPos: new Vector2(x: 184.0f, y: -446.0f)),
                new(Id: "yyjn_2", btnSize: new Vector2(x: 500.0f, y: 600.0f), btnPos: new Vector2(x: -132.0f, y: 310.0f)),
                new(Id: "yyjn_4", btnSize: new Vector2(x: 550.0f, y: 650.0f), btnPos: new Vector2(x: -245.0f, y: -246.0f)),
                new(Id: "gwds_2", btnSize: new Vector2(x: 400.0f, y: 400.0f), btnPos: new Vector2(x: 115.0f, y: 4.0f)),
                new(Id: "gwds_3", btnSize: new Vector2(x: 500.0f, y: 500.0f), btnPos: new Vector2(x: -4.0f, y: -40.0f)),
                new(Id: "gwds_5", btnSize: new Vector2(x: 400.0f, y: 500.0f), btnPos: new Vector2(x: -176.0f, y: 331.0f)),
                new(Id: "gwds_7", btnSize: new Vector2(x: 300.0f, y: 300.0f), btnPos: new Vector2(x: -53.0f, y: 257.0f)),
                new(Id: "gwds_8", btnSize: new Vector2(x: 600.0f, y: 500.0f), btnPos: new Vector2(x: 270.0f, y: -83.0f)),
                new(Id: "dtxy_3", btnSize: new Vector2(x: 600.0f, y: 600.0f), btnPos: new Vector2(x: 17.0f, y: 124.0f)),
                new(Id: "dtxy_5", btnSize: new Vector2(x: 300.0f, y: 300.0f), btnPos: new Vector2(x: -60.0f, y: 248.0f)),
                new(Id: "dtxy_7", btnSize: new Vector2(x: 300.0f, y: 700.0f), btnPos: new Vector2(x: -39.0f, y: -475.0f)),
                new(Id: "nhsbzz_3", btnSize: new Vector2(x: 400.0f, y: 200.0f), btnPos: new Vector2(x: -21.0f, y: -18.0f)),
                new(Id: "nhsbzz_4", btnSize: new Vector2(x: 800.0f, y: 700.0f), btnPos: new Vector2(x: 104.0f, y: 45.0f)),
                new(Id: "nhsbzz_6", btnSize: new Vector2(x: 800.0f, y: 1000.0f), btnPos: new Vector2(x: -89.0f, y: -420.0f)),
                new(Id: "nhsbzz_10", btnSize: new Vector2(x: 800.0f, y: 700.0f), btnPos: new Vector2(x: 57.0f, y: 89.0f)),
                new(Id: "dhft_3", btnSize: new Vector2(x: 600.0f, y: 600.0f), btnPos: new Vector2(x: -21.0f, y: 46.0f)),
                new(Id: "dhft_4", btnSize: new Vector2(x: 600.0f, y: 600.0f), btnPos: new Vector2(x: 47.0f, y: 310.0f)),
                new(Id: "dhft_7", btnSize: new Vector2(x: 400.0f, y: 600.0f), btnPos: new Vector2(x: 73.0f, y: -193.0f)),
                new(Id: "dhft_9", btnSize: new Vector2(x: 600.0f, y: 400.0f), btnPos: new Vector2(x: 108.0f, y: -162.0f)),
                new(Id: "sscltt_1", btnSize: new Vector2(x: 600.0f, y: 600.0f), btnPos: new Vector2(x: 20.0f, y: -222.0f)),
                new(Id: "sscltt_3", btnSize: new Vector2(x: 700.0f, y: 1200.0f), btnPos: new Vector2(x: 2.0f, y: -121.0f)),
                new(Id: "jqryrgw_07", btnSize: new Vector2(x: 500.0f, y: 700.0f), btnPos: new Vector2(x: -80.0f, y: 152.0f)),
                new(Id: "jqryrgw_06", btnSize: new Vector2(x: 600.0f, y: 700.0f), btnPos: new Vector2(x: 146.0f, y: 200.0f)),
                new(Id: "jqryrgw_03", btnSize: new Vector2(x: 500.0f, y: 1000.0f), btnPos: new Vector2(x: 97.0f, y: 59.0f)),
                new(Id: "jqryrgw_02", btnSize: new Vector2(x: 500.0f, y: 900.0f), btnPos: new Vector2(x: 2.0f, y: 476.0f))
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly LevelSpineTouch this[string Id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(Id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[LevelSpineTouch] Id: {Id} not found");
                return ref _data[idx];
            }
        }
        
        public LevelSpineTouch[] All => _data;
        
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
                _idToIdx[_data[i].Id] = i;
            }
        }
    }
}