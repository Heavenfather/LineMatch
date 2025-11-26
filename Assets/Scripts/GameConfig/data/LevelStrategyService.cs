namespace GameConfig
{
    public partial class LevelStrategyDB
    {
        public bool TryGetValue(int levelId, out int value)
        {
            if (_idToIdx.ContainsKey(levelId))
            {
                value = this[levelId].value;
                return true;
            }
            value = 0;
            return false;
        }
    }
}