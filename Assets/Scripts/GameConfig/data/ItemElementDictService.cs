using UnityEngine;

namespace GameConfig
{
    public partial class ItemElementDictDB
    {
        public int GetElementId(string itemID)
        {
            var config = this[itemID];
            if (config.elementId == null || config.elementId.Count <= 0)
                return -1;
            if(config.elementId.Count <= 1)
                return config.elementId[0];
            return config.elementId[Random.Range(0, config.elementId.Count)];
        }
    }
}