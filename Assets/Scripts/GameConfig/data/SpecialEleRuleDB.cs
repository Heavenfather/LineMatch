/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 功能棋子形成规则表.xlsx
*/

namespace GameConfig
{
    

    public partial class SpecialEleRuleDB : ConfigBase
    {
        private SpecialEleRule[] _data;
        
        
        protected override void ConstructConfig()
        {
            _data = new SpecialEleRule[]
            {
                new(priority: 1, resultElement: ElementType.ColorBall, scoreType: OneTakeScoreType.ColorBallRect, taskTag: 15, minWidth: 4, minHeight: 4, maxWidth: 0, maxHeight: 0),
                new(priority: 2, resultElement: ElementType.Bomb, scoreType: OneTakeScoreType.BombRect, taskTag: 14, minWidth: 3, minHeight: 3, maxWidth: 3, maxHeight: 3),
                new(priority: 3, resultElement: ElementType.Rocket, scoreType: OneTakeScoreType.RocketRect, taskTag: 13, minWidth: 2, minHeight: 3, maxWidth: 0, maxHeight: 0),
                new(priority: 3, resultElement: ElementType.RocketHorizontal, scoreType: OneTakeScoreType.RocketRect, taskTag: 13, minWidth: 3, minHeight: 2, maxWidth: 0, maxHeight: 0)
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly SpecialEleRule this[int idx]
        {
            get
            {
                TackUsage();
                if(idx < 0 || idx >= _data.Length)
                    UnityEngine.Debug.LogError($"[SpecialEleRule] {idx} out of bounds");
                return ref _data[idx];
            }
        }
        
        public SpecialEleRule[] All => _data;
        
        public int Count => _data.Length;
        
        public override void Dispose()
        {
            _data = null;
            OnDispose();
        }
        
        private void MakeIdToIdx()
        {
            
        }
    }
}