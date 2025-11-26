namespace GameConfig
{
    public partial class LevelSpineTouchDB
    {
        public bool CanTouch(string Id) {
            return _idToIdx.ContainsKey(Id);
        }
    }
}