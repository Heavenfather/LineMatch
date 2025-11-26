namespace GameConfig
{
    public partial class PuzzleCollectAudioDB
    {
        public string GetAudioName(int id)
        {
            foreach (var audio in _data) {
                if (audio.collectIds != null && audio.collectIds.Contains(id)) {
                    return audio.audioName;
                }
            }
            return "";
        }
    }
}