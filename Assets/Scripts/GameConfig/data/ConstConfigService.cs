using System.Collections.Generic;

namespace GameConfig
{
    public partial class ConstConfigDB
    {
        private Dictionary<string, int> _matchBoosterDict;

        public int GetConfigIntVal(string key)
        {
            return this[key].intValue;
        }

        public string GetConfigStrVal(string key)
        {
            return this[key].strValue;
        }

        public bool IsMatchBoosterUnlocked(string key, int level)
        {
            if (_matchBoosterDict == null)
            {
                InitMatchBoosterDict();
            }
            return _matchBoosterDict.ContainsKey(key) && level >= _matchBoosterDict[key];
        }

        private void InitMatchBoosterDict()
        {
            _matchBoosterDict = new Dictionary<string, int>();

            var strValue = GetConfigStrVal("MatchBoosterUnlockLv");
            var strArr = strValue.Split('|');
            for (int i = 0; i < strArr.Length; i++)
            {
                var keyVal = strArr[i].Split('_');
                _matchBoosterDict.Add(keyVal[0], int.Parse(keyVal[1]));
            }
        }

        public int GetMatchBoosterUnlockLv(string key)
        {
            if (_matchBoosterDict == null)
            {
                InitMatchBoosterDict();
            }
            if (!_matchBoosterDict.ContainsKey(key))
            {
                return 0;
            }

            return _matchBoosterDict[key];
        }

        public List<int> GetNewPlayerLoadingID() {
            var idsStr = GetConfigStrVal("NewPlayerLoadingIds");
            var idList = new List<int>();
            if (idsStr.Contains("|")) {
                var starArr = GetConfigStrVal("NewPlayerLoadingIds").Split('|');
                for (int i = 0; i < starArr.Length; i++)
                {
                    var id = int.TryParse(starArr[i], out int idNum) ? idNum : 0;
                    if (id != 0) idList.Add(id);
                }
            } else {
                var id = int.TryParse(idsStr, out int idNum) ? idNum : 0;
                if (id != 0) idList.Add(id);
            }
            return idList;
        }
    }
}