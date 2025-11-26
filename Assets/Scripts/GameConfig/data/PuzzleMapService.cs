namespace GameConfig
{
    public partial class PuzzleMapDB
    {
        public (string, string) GetMapName(int mapId, int levelId) {
            var map = this[mapId];
            switch (levelId) {
                case 1:
                    return (map.mapCNName, map.LeveName1);
                case 2:
                    return (map.mapCNName, map.LeveName2);
                case 3:
                    return (map.mapCNName, map.LeveName3);
                case 4:
                    return (map.mapCNName, map.LeveName4);
                case 5:
                    return (map.mapCNName, map.LeveName5);
                case 6:
                    return (map.mapCNName, map.LeveName6);
                case 7:
                    return (map.mapCNName, map.LeveName7);
                case 8:
                    return (map.mapCNName, map.LeveName8);
                default:
                    return ("", "");
            }
        }
    }
}