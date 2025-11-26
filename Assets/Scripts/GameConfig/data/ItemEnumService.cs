using System.Collections.Generic;
using System.Linq;

namespace GameConfig
{
    public partial class ItemEnumDB
    {
        Dictionary<string, int> _strKeyDict = new Dictionary<string, int>();

        private ItemEnum _none;
        public ref readonly ItemEnum None => ref _none;


        protected override void OnInitialized()
        {
            foreach (var item in _data)
            {
                _strKeyDict.Add(item.name, item.Id);
            }
            _none = _data[_idToIdx[1000]];
        }

        protected override void OnDispose()
        {
            _strKeyDict.Clear();
        }

        public ref readonly ItemEnum this[string name]
        {
            get
            {
                TackUsage();
                var ok = _strKeyDict.TryGetValue(name, out int id);
                if (!ok) {
                    UnityEngine.Debug.LogError($"[ItemEnum] name: {name} not found");
                    return ref _data[_idToIdx[1000]];
                }
                
                return ref _data[_idToIdx[id]];
            }
        }

        public bool IsNoneKey(int id)
        {
            return !_idToIdx.TryGetValue(id, out int idx);
        }

        public bool IsNoneKey(string name)
        {
            return !_strKeyDict.TryGetValue(name, out int id) && !_idToIdx.TryGetValue(id, out int idx);
        }

        public List<ItemEnum> GetItemsByTag(ItemEnumType tag) {
            var items = _data.Where(item => item.tags == tag);
            return items.ToList();
        }

        public int GetIdByName(string name)
        {
            TackUsage();
            var ok = _strKeyDict.TryGetValue(name, out int id);
            if (!ok) {
                UnityEngine.Debug.LogError($"[ItemEnum] name: {name} not found");
                return 1000;
            }
            return id;
        }
    }
}