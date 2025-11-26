namespace GameConfig
{
    public partial class consecutiveCheckinDB
    {

        public consecutiveCheckin GetConfigByDay(string day)
        {
            for (int i = 0; i < All.Length; i++)
            {
                if(All[i].dayNum == day)
                    return All[i];
            }
            return default;
        }
        
        public int GetMaxDay()
        {
            int max = 0;
            int.TryParse(All[Count - 1].dayNum, out max);
            return max;
        }
    }
}