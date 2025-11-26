namespace GameConfig
{
    public partial class ObjectiveRewardDB
    {
        public int GetObjectiveRewardTargetCount(int id, int num) {
            if (_idToIdx.TryGetValue(id, out int idx)) {
                switch (num) {
                    case 1:
                        return _data[idx].Num1;
                    case 2:
                        return _data[idx].Num2;
                    case 3:
                        return _data[idx].Num3;
                    case 4:
                        return _data[idx].Num4;
                    case 5:
                        return _data[idx].Num5;
                }
            }
            return 0;
        }
    }
}