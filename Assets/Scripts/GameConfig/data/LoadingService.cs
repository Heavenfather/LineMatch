namespace GameConfig
{
    public partial class LoadingDB
    {
        public ref Loading RandomGetLoading()
        {
            return ref All[UnityEngine.Random.Range(0, Count)];
        }

        public int RandomGetLoadingId() {
            return All[UnityEngine.Random.Range(0, Count)].id;
        }
    }
}