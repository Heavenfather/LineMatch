using System.Collections.Generic;

namespace GameConfig
{
    public partial class GradeGiftDB
    {
        public List<GradeGift> GetGradeGiftsByType(GradeGiftType type) {
            List<GradeGift> result = new List<GradeGift>();
            foreach (GradeGift gift in _data) {
                if (gift.giftType == type) {
                    result.Add(gift);
                }
            }
            return result;
        }
    }
}