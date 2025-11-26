namespace GameConfig
{
    public partial class matchBaseCoinDB
    {

        public int GetBaseCoin(MatchDifficulty difficulty) {
            var baseCoin = 0;

            if (difficulty == MatchDifficulty.Normal)
            {
                baseCoin = _data[0].ease;
            }
            else if (difficulty == MatchDifficulty.Hard)
            {
                baseCoin = _data[0].hard;
            }
            else if (difficulty == MatchDifficulty.Crazy)
            {
                baseCoin = _data[0].extreme;
            }

            return baseCoin;
        }
    }
}