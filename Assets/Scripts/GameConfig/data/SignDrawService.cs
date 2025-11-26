namespace GameConfig
{
    public partial class SignDrawDB
    {
        public int GetRandomSignId()
        {
            return All[UnityEngine.Random.Range(0, Count)].id;
        }
    }
}