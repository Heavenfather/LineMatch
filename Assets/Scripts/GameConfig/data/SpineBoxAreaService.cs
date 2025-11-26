namespace GameConfig
{
    public partial class SpineBoxAreaDB
    {
        public bool HasSpineBoxArea(string spineID)
        {
            return _idToIdx.ContainsKey(spineID);
        }
    }
}