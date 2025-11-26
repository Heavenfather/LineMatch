/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 寻宝关卡奖励.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;


    public partial class PuzzleCollectAudioDB : ConfigBase
    {
        private PuzzleCollectAudio[] _data;
        private Dictionary<string, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new PuzzleCollectAudio[]
            {
                new(audioName: "puzzle_chicken_chick", collectIds: new List<int>() { 26, 154, 187, 85, 53, 115 }),
                new(audioName: "puzzle_chicken_box", collectIds: new List<int>() { 6, 90, 447, 182, 238, 18, 411 }),
                new(audioName: "puzzle_chicken_1", collectIds: new List<int>() { 363, 212, 3, 72, 199, 374, 461, 525, 614 }),
                new(audioName: "puzzle_chicken_book", collectIds: new List<int>() { 22, 68, 163, 214, 378, 420, 400, 465 }),
                new(audioName: "puzzle_chicken_bottle", collectIds: new List<int>() { 24, 188, 444, 611, 643, 131, 192 }),
                new(audioName: "puzzle_chicken_camera", collectIds: new List<int>() { 151, 391, 421, 511, 574 }),
                new(audioName: "puzzle_chicken_cat", collectIds: new List<int>() { 193, 390, 407, 455, 516, 521, 629 }),
                new(audioName: "puzzle_chicken_clock", collectIds: new List<int>() { 690 }),
                new(audioName: "puzzle_chicken_coin", collectIds: new List<int>() { 59, 609 }),
                new(audioName: "puzzle_chicken_cookie", collectIds: new List<int>() { 494, 639, 216, 537, 568, 473, 44, 121, 369, 427, 623, 41 }),
                new(audioName: "puzzle_chicken_diamon", collectIds: new List<int>() { 38, 86, 559, 582, 616, 19, 100 }),
                new(audioName: "puzzle_chicken_drum", collectIds: new List<int>() { 398, 7, 228 }),
                new(audioName: "puzzle_chicken_fruit", collectIds: new List<int>() { 37, 108, 220, 375, 476, 510, 526, 539, 549, 576, 633, 97, 456, 624 }),
                new(audioName: "puzzle_chicken_gita", collectIds: new List<int>() { 128, 385, 422 }),
                new(audioName: "puzzle_chicken_huhu", collectIds: new List<int>() { 139, 437, 415 }),
                new(audioName: "puzzle_chicken_phone", collectIds: new List<int>() { 412 }),
                new(audioName: "puzzle_chicken_shake", collectIds: new List<int>() { 4, 63, 141, 210, 235, 367, 413, 458, 523, 575 }),
                new(audioName: "puzzle_chicken_sheep", collectIds: new List<int>() { 160 }),
                new(audioName: "puzzle_chicken_star", collectIds: new List<int>() { 50, 113, 194, 376, 403, 425, 450, 517, 558, 600 })
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly PuzzleCollectAudio this[string audioName]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(audioName, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[PuzzleCollectAudio] audioName: {audioName} not found");
                return ref _data[idx];
            }
        }
        
        public PuzzleCollectAudio[] All => _data;
        
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
                _idToIdx[_data[i].audioName] = i;
            }
        }
    }
}