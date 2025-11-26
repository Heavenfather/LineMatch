using System.Collections.Generic;
using System.Linq;

namespace GameConfig
{
    public partial class DressEnumDB
    {
        public List<DressEnum> GetItemsByTag(ItemEnumType tag) {
            var items = _data.Where(item => item.tags == tag);
            return items.ToList();
        }

        public int GetDressIDByItemName(string itemName) {
            var item = _data.FirstOrDefault(item => item.itemName == itemName);
            return item.Id;
        }
    }


}