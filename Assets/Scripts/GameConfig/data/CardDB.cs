/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 集卡配置表.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class CardDB : ConfigBase
    {
        private Card[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new Card[]
            {
                new(cardId: 10101, star: 1, isGold: false, packId: 101, cardName_cn: LocalizationPool.Get("Card/c8bf"), themeId: 10000, desc_cn: ""),
                new(cardId: 10102, star: 1, isGold: false, packId: 101, cardName_cn: LocalizationPool.Get("Card/a442"), themeId: 10000, desc_cn: ""),
                new(cardId: 10103, star: 1, isGold: false, packId: 101, cardName_cn: LocalizationPool.Get("Card/c1ac"), themeId: 10000, desc_cn: ""),
                new(cardId: 10104, star: 1, isGold: false, packId: 101, cardName_cn: LocalizationPool.Get("Card/e040"), themeId: 10000, desc_cn: ""),
                new(cardId: 10105, star: 1, isGold: false, packId: 101, cardName_cn: LocalizationPool.Get("Card/3051"), themeId: 10000, desc_cn: ""),
                new(cardId: 10106, star: 1, isGold: false, packId: 101, cardName_cn: LocalizationPool.Get("Card/a39f"), themeId: 10000, desc_cn: ""),
                new(cardId: 10107, star: 1, isGold: false, packId: 101, cardName_cn: LocalizationPool.Get("Card/e585"), themeId: 10000, desc_cn: ""),
                new(cardId: 10108, star: 1, isGold: false, packId: 101, cardName_cn: LocalizationPool.Get("Card/caf2"), themeId: 10000, desc_cn: ""),
                new(cardId: 10109, star: 2, isGold: false, packId: 101, cardName_cn: LocalizationPool.Get("Card/8e20"), themeId: 10000, desc_cn: ""),
                new(cardId: 10201, star: 1, isGold: false, packId: 102, cardName_cn: LocalizationPool.Get("Card/002a"), themeId: 10000, desc_cn: ""),
                new(cardId: 10202, star: 1, isGold: false, packId: 102, cardName_cn: LocalizationPool.Get("Card/4c2a"), themeId: 10000, desc_cn: ""),
                new(cardId: 10203, star: 1, isGold: false, packId: 102, cardName_cn: LocalizationPool.Get("Card/ece6"), themeId: 10000, desc_cn: ""),
                new(cardId: 10204, star: 1, isGold: false, packId: 102, cardName_cn: LocalizationPool.Get("Card/5472"), themeId: 10000, desc_cn: ""),
                new(cardId: 10205, star: 1, isGold: false, packId: 102, cardName_cn: LocalizationPool.Get("Card/9971"), themeId: 10000, desc_cn: ""),
                new(cardId: 10206, star: 1, isGold: false, packId: 102, cardName_cn: LocalizationPool.Get("Card/a534"), themeId: 10000, desc_cn: ""),
                new(cardId: 10207, star: 1, isGold: false, packId: 102, cardName_cn: LocalizationPool.Get("Card/837b"), themeId: 10000, desc_cn: ""),
                new(cardId: 10208, star: 2, isGold: false, packId: 102, cardName_cn: LocalizationPool.Get("Card/6bb2"), themeId: 10000, desc_cn: ""),
                new(cardId: 10209, star: 2, isGold: false, packId: 102, cardName_cn: LocalizationPool.Get("Card/0aba"), themeId: 10000, desc_cn: ""),
                new(cardId: 10301, star: 1, isGold: false, packId: 103, cardName_cn: LocalizationPool.Get("Card/4627"), themeId: 10000, desc_cn: ""),
                new(cardId: 10302, star: 1, isGold: false, packId: 103, cardName_cn: LocalizationPool.Get("Card/f7e7"), themeId: 10000, desc_cn: ""),
                new(cardId: 10303, star: 1, isGold: false, packId: 103, cardName_cn: LocalizationPool.Get("Card/7959"), themeId: 10000, desc_cn: ""),
                new(cardId: 10304, star: 1, isGold: false, packId: 103, cardName_cn: LocalizationPool.Get("Card/8d0f"), themeId: 10000, desc_cn: ""),
                new(cardId: 10305, star: 1, isGold: false, packId: 103, cardName_cn: LocalizationPool.Get("Card/3048"), themeId: 10000, desc_cn: ""),
                new(cardId: 10306, star: 1, isGold: false, packId: 103, cardName_cn: LocalizationPool.Get("Card/e766"), themeId: 10000, desc_cn: ""),
                new(cardId: 10307, star: 1, isGold: false, packId: 103, cardName_cn: LocalizationPool.Get("Card/c17b"), themeId: 10000, desc_cn: ""),
                new(cardId: 10308, star: 2, isGold: false, packId: 103, cardName_cn: LocalizationPool.Get("Card/a3e8"), themeId: 10000, desc_cn: ""),
                new(cardId: 10309, star: 3, isGold: false, packId: 103, cardName_cn: LocalizationPool.Get("Card/21d9"), themeId: 10000, desc_cn: ""),
                new(cardId: 10401, star: 1, isGold: false, packId: 104, cardName_cn: LocalizationPool.Get("Card/e337"), themeId: 10000, desc_cn: ""),
                new(cardId: 10402, star: 1, isGold: false, packId: 104, cardName_cn: LocalizationPool.Get("Card/ac3f"), themeId: 10000, desc_cn: ""),
                new(cardId: 10403, star: 1, isGold: false, packId: 104, cardName_cn: LocalizationPool.Get("Card/0611"), themeId: 10000, desc_cn: ""),
                new(cardId: 10404, star: 1, isGold: false, packId: 104, cardName_cn: LocalizationPool.Get("Card/e8b0"), themeId: 10000, desc_cn: ""),
                new(cardId: 10405, star: 1, isGold: false, packId: 104, cardName_cn: LocalizationPool.Get("Card/9047"), themeId: 10000, desc_cn: ""),
                new(cardId: 10406, star: 2, isGold: false, packId: 104, cardName_cn: LocalizationPool.Get("Card/80d5"), themeId: 10000, desc_cn: ""),
                new(cardId: 10407, star: 2, isGold: false, packId: 104, cardName_cn: LocalizationPool.Get("Card/ffff"), themeId: 10000, desc_cn: ""),
                new(cardId: 10408, star: 2, isGold: false, packId: 104, cardName_cn: LocalizationPool.Get("Card/6b8a"), themeId: 10000, desc_cn: ""),
                new(cardId: 10409, star: 3, isGold: false, packId: 104, cardName_cn: LocalizationPool.Get("Card/300b"), themeId: 10000, desc_cn: ""),
                new(cardId: 10501, star: 1, isGold: false, packId: 105, cardName_cn: LocalizationPool.Get("Card/7e74"), themeId: 10000, desc_cn: ""),
                new(cardId: 10502, star: 1, isGold: false, packId: 105, cardName_cn: LocalizationPool.Get("Card/8b8c"), themeId: 10000, desc_cn: ""),
                new(cardId: 10503, star: 1, isGold: false, packId: 105, cardName_cn: LocalizationPool.Get("Card/3162"), themeId: 10000, desc_cn: ""),
                new(cardId: 10504, star: 2, isGold: false, packId: 105, cardName_cn: LocalizationPool.Get("Card/5dc1"), themeId: 10000, desc_cn: ""),
                new(cardId: 10505, star: 2, isGold: false, packId: 105, cardName_cn: LocalizationPool.Get("Card/19f2"), themeId: 10000, desc_cn: ""),
                new(cardId: 10506, star: 2, isGold: false, packId: 105, cardName_cn: LocalizationPool.Get("Card/3e33"), themeId: 10000, desc_cn: ""),
                new(cardId: 10507, star: 2, isGold: false, packId: 105, cardName_cn: LocalizationPool.Get("Card/1e57"), themeId: 10000, desc_cn: ""),
                new(cardId: 10508, star: 3, isGold: false, packId: 105, cardName_cn: LocalizationPool.Get("Card/cfa5"), themeId: 10000, desc_cn: ""),
                new(cardId: 10509, star: 4, isGold: false, packId: 105, cardName_cn: LocalizationPool.Get("Card/b48b"), themeId: 10000, desc_cn: ""),
                new(cardId: 10601, star: 1, isGold: false, packId: 106, cardName_cn: LocalizationPool.Get("Card/4f4e"), themeId: 10000, desc_cn: ""),
                new(cardId: 10602, star: 1, isGold: false, packId: 106, cardName_cn: LocalizationPool.Get("Card/d0ea"), themeId: 10000, desc_cn: ""),
                new(cardId: 10603, star: 2, isGold: false, packId: 106, cardName_cn: LocalizationPool.Get("Card/cd16"), themeId: 10000, desc_cn: ""),
                new(cardId: 10604, star: 2, isGold: false, packId: 106, cardName_cn: LocalizationPool.Get("Card/b6c7"), themeId: 10000, desc_cn: ""),
                new(cardId: 10605, star: 2, isGold: false, packId: 106, cardName_cn: LocalizationPool.Get("Card/bcb1"), themeId: 10000, desc_cn: ""),
                new(cardId: 10606, star: 2, isGold: false, packId: 106, cardName_cn: LocalizationPool.Get("Card/a36f"), themeId: 10000, desc_cn: ""),
                new(cardId: 10607, star: 3, isGold: false, packId: 106, cardName_cn: LocalizationPool.Get("Card/0040"), themeId: 10000, desc_cn: ""),
                new(cardId: 10608, star: 4, isGold: false, packId: 106, cardName_cn: LocalizationPool.Get("Card/4a50"), themeId: 10000, desc_cn: ""),
                new(cardId: 10609, star: 5, isGold: false, packId: 106, cardName_cn: LocalizationPool.Get("Card/a7c8"), themeId: 10000, desc_cn: ""),
                new(cardId: 10701, star: 1, isGold: false, packId: 107, cardName_cn: LocalizationPool.Get("Card/128f"), themeId: 10000, desc_cn: ""),
                new(cardId: 10702, star: 2, isGold: false, packId: 107, cardName_cn: LocalizationPool.Get("Card/b063"), themeId: 10000, desc_cn: ""),
                new(cardId: 10703, star: 2, isGold: false, packId: 107, cardName_cn: LocalizationPool.Get("Card/b1b1"), themeId: 10000, desc_cn: ""),
                new(cardId: 10704, star: 2, isGold: false, packId: 107, cardName_cn: LocalizationPool.Get("Card/ca33"), themeId: 10000, desc_cn: ""),
                new(cardId: 10705, star: 3, isGold: false, packId: 107, cardName_cn: LocalizationPool.Get("Card/e209"), themeId: 10000, desc_cn: ""),
                new(cardId: 10706, star: 3, isGold: false, packId: 107, cardName_cn: LocalizationPool.Get("Card/ead4"), themeId: 10000, desc_cn: ""),
                new(cardId: 10707, star: 4, isGold: false, packId: 107, cardName_cn: LocalizationPool.Get("Card/3eeb"), themeId: 10000, desc_cn: ""),
                new(cardId: 10708, star: 5, isGold: false, packId: 107, cardName_cn: LocalizationPool.Get("Card/2ade"), themeId: 10000, desc_cn: ""),
                new(cardId: 10709, star: 5, isGold: true, packId: 107, cardName_cn: LocalizationPool.Get("Card/3d95"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/118b")),
                new(cardId: 10801, star: 2, isGold: false, packId: 108, cardName_cn: LocalizationPool.Get("Card/508e"), themeId: 10000, desc_cn: ""),
                new(cardId: 10802, star: 2, isGold: false, packId: 108, cardName_cn: LocalizationPool.Get("Card/bde3"), themeId: 10000, desc_cn: ""),
                new(cardId: 10803, star: 2, isGold: false, packId: 108, cardName_cn: LocalizationPool.Get("Card/4110"), themeId: 10000, desc_cn: ""),
                new(cardId: 10804, star: 2, isGold: false, packId: 108, cardName_cn: LocalizationPool.Get("Card/0a32"), themeId: 10000, desc_cn: ""),
                new(cardId: 10805, star: 3, isGold: false, packId: 108, cardName_cn: LocalizationPool.Get("Card/9948"), themeId: 10000, desc_cn: ""),
                new(cardId: 10806, star: 3, isGold: false, packId: 108, cardName_cn: LocalizationPool.Get("Card/96db"), themeId: 10000, desc_cn: ""),
                new(cardId: 10807, star: 4, isGold: false, packId: 108, cardName_cn: LocalizationPool.Get("Card/7fed"), themeId: 10000, desc_cn: ""),
                new(cardId: 10808, star: 5, isGold: true, packId: 108, cardName_cn: LocalizationPool.Get("Card/acde"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/9cfe")),
                new(cardId: 10809, star: 5, isGold: true, packId: 108, cardName_cn: LocalizationPool.Get("Card/bc1c"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/3ca8")),
                new(cardId: 10901, star: 2, isGold: false, packId: 109, cardName_cn: LocalizationPool.Get("Card/7e50"), themeId: 10000, desc_cn: ""),
                new(cardId: 10902, star: 2, isGold: false, packId: 109, cardName_cn: LocalizationPool.Get("Card/ccd5"), themeId: 10000, desc_cn: ""),
                new(cardId: 10903, star: 2, isGold: false, packId: 109, cardName_cn: LocalizationPool.Get("Card/8019"), themeId: 10000, desc_cn: ""),
                new(cardId: 10904, star: 3, isGold: false, packId: 109, cardName_cn: LocalizationPool.Get("Card/4a8b"), themeId: 10000, desc_cn: ""),
                new(cardId: 10905, star: 3, isGold: false, packId: 109, cardName_cn: LocalizationPool.Get("Card/13a1"), themeId: 10000, desc_cn: ""),
                new(cardId: 10906, star: 4, isGold: false, packId: 109, cardName_cn: LocalizationPool.Get("Card/3571"), themeId: 10000, desc_cn: ""),
                new(cardId: 10907, star: 5, isGold: false, packId: 109, cardName_cn: LocalizationPool.Get("Card/c8fa"), themeId: 10000, desc_cn: ""),
                new(cardId: 10908, star: 5, isGold: false, packId: 109, cardName_cn: LocalizationPool.Get("Card/0beb"), themeId: 10000, desc_cn: ""),
                new(cardId: 10909, star: 5, isGold: true, packId: 109, cardName_cn: LocalizationPool.Get("Card/b68c"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/f324")),
                new(cardId: 11001, star: 2, isGold: false, packId: 110, cardName_cn: LocalizationPool.Get("Card/47f7"), themeId: 10000, desc_cn: ""),
                new(cardId: 11002, star: 2, isGold: false, packId: 110, cardName_cn: LocalizationPool.Get("Card/3d72"), themeId: 10000, desc_cn: ""),
                new(cardId: 11003, star: 3, isGold: false, packId: 110, cardName_cn: LocalizationPool.Get("Card/6f1a"), themeId: 10000, desc_cn: ""),
                new(cardId: 11004, star: 3, isGold: false, packId: 110, cardName_cn: LocalizationPool.Get("Card/f4de"), themeId: 10000, desc_cn: ""),
                new(cardId: 11005, star: 4, isGold: false, packId: 110, cardName_cn: LocalizationPool.Get("Card/34c5"), themeId: 10000, desc_cn: ""),
                new(cardId: 11006, star: 4, isGold: false, packId: 110, cardName_cn: LocalizationPool.Get("Card/210a"), themeId: 10000, desc_cn: ""),
                new(cardId: 11007, star: 5, isGold: false, packId: 110, cardName_cn: LocalizationPool.Get("Card/bb59"), themeId: 10000, desc_cn: ""),
                new(cardId: 11008, star: 5, isGold: true, packId: 110, cardName_cn: LocalizationPool.Get("Card/4e63"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/d883")),
                new(cardId: 11009, star: 5, isGold: true, packId: 110, cardName_cn: LocalizationPool.Get("Card/2dc8"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/b89b")),
                new(cardId: 11101, star: 2, isGold: false, packId: 111, cardName_cn: LocalizationPool.Get("Card/b85c"), themeId: 10000, desc_cn: ""),
                new(cardId: 11102, star: 3, isGold: false, packId: 111, cardName_cn: LocalizationPool.Get("Card/d080"), themeId: 10000, desc_cn: ""),
                new(cardId: 11103, star: 3, isGold: false, packId: 111, cardName_cn: LocalizationPool.Get("Card/b5f2"), themeId: 10000, desc_cn: ""),
                new(cardId: 11104, star: 3, isGold: false, packId: 111, cardName_cn: LocalizationPool.Get("Card/0c00"), themeId: 10000, desc_cn: ""),
                new(cardId: 11105, star: 4, isGold: false, packId: 111, cardName_cn: LocalizationPool.Get("Card/b69c"), themeId: 10000, desc_cn: ""),
                new(cardId: 11106, star: 5, isGold: false, packId: 111, cardName_cn: LocalizationPool.Get("Card/6493"), themeId: 10000, desc_cn: ""),
                new(cardId: 11107, star: 5, isGold: false, packId: 111, cardName_cn: LocalizationPool.Get("Card/ff8f"), themeId: 10000, desc_cn: ""),
                new(cardId: 11108, star: 5, isGold: true, packId: 111, cardName_cn: LocalizationPool.Get("Card/49d6"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/75be")),
                new(cardId: 11109, star: 5, isGold: true, packId: 111, cardName_cn: LocalizationPool.Get("Card/0a67"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/5aee")),
                new(cardId: 11201, star: 3, isGold: false, packId: 112, cardName_cn: LocalizationPool.Get("Card/0ad3"), themeId: 10000, desc_cn: ""),
                new(cardId: 11202, star: 3, isGold: false, packId: 112, cardName_cn: LocalizationPool.Get("Card/f3e9"), themeId: 10000, desc_cn: ""),
                new(cardId: 11203, star: 3, isGold: false, packId: 112, cardName_cn: LocalizationPool.Get("Card/9b38"), themeId: 10000, desc_cn: ""),
                new(cardId: 11204, star: 4, isGold: false, packId: 112, cardName_cn: LocalizationPool.Get("Card/001c"), themeId: 10000, desc_cn: ""),
                new(cardId: 11205, star: 4, isGold: false, packId: 112, cardName_cn: LocalizationPool.Get("Card/f95b"), themeId: 10000, desc_cn: ""),
                new(cardId: 11206, star: 5, isGold: false, packId: 112, cardName_cn: LocalizationPool.Get("Card/411c"), themeId: 10000, desc_cn: ""),
                new(cardId: 11207, star: 5, isGold: false, packId: 112, cardName_cn: LocalizationPool.Get("Card/0c8b"), themeId: 10000, desc_cn: ""),
                new(cardId: 11208, star: 5, isGold: true, packId: 112, cardName_cn: LocalizationPool.Get("Card/19d2"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/c34f")),
                new(cardId: 11209, star: 5, isGold: true, packId: 112, cardName_cn: LocalizationPool.Get("Card/3bd7"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/8792")),
                new(cardId: 11301, star: 3, isGold: false, packId: 113, cardName_cn: LocalizationPool.Get("Card/4b06"), themeId: 10000, desc_cn: ""),
                new(cardId: 11302, star: 3, isGold: false, packId: 113, cardName_cn: LocalizationPool.Get("Card/bc7a"), themeId: 10000, desc_cn: ""),
                new(cardId: 11303, star: 3, isGold: false, packId: 113, cardName_cn: LocalizationPool.Get("Card/9db2"), themeId: 10000, desc_cn: ""),
                new(cardId: 11304, star: 4, isGold: false, packId: 113, cardName_cn: LocalizationPool.Get("Card/dab7"), themeId: 10000, desc_cn: ""),
                new(cardId: 11305, star: 4, isGold: false, packId: 113, cardName_cn: LocalizationPool.Get("Card/75f0"), themeId: 10000, desc_cn: ""),
                new(cardId: 11306, star: 5, isGold: false, packId: 113, cardName_cn: LocalizationPool.Get("Card/bc9c"), themeId: 10000, desc_cn: ""),
                new(cardId: 11307, star: 5, isGold: true, packId: 113, cardName_cn: LocalizationPool.Get("Card/c01d"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/3987")),
                new(cardId: 11308, star: 5, isGold: true, packId: 113, cardName_cn: LocalizationPool.Get("Card/5226"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/4558")),
                new(cardId: 11309, star: 5, isGold: true, packId: 113, cardName_cn: LocalizationPool.Get("Card/5353"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/c192")),
                new(cardId: 11401, star: 3, isGold: false, packId: 114, cardName_cn: LocalizationPool.Get("Card/4ff4"), themeId: 10000, desc_cn: ""),
                new(cardId: 11402, star: 3, isGold: false, packId: 114, cardName_cn: LocalizationPool.Get("Card/d2ea"), themeId: 10000, desc_cn: ""),
                new(cardId: 11403, star: 4, isGold: false, packId: 114, cardName_cn: LocalizationPool.Get("Card/d875"), themeId: 10000, desc_cn: ""),
                new(cardId: 11404, star: 4, isGold: false, packId: 114, cardName_cn: LocalizationPool.Get("Card/64a7"), themeId: 10000, desc_cn: ""),
                new(cardId: 11405, star: 5, isGold: false, packId: 114, cardName_cn: LocalizationPool.Get("Card/afb6"), themeId: 10000, desc_cn: ""),
                new(cardId: 11406, star: 5, isGold: false, packId: 114, cardName_cn: LocalizationPool.Get("Card/dbaf"), themeId: 10000, desc_cn: ""),
                new(cardId: 11407, star: 5, isGold: true, packId: 114, cardName_cn: LocalizationPool.Get("Card/8065"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/113a")),
                new(cardId: 11408, star: 5, isGold: true, packId: 114, cardName_cn: LocalizationPool.Get("Card/47f7"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/857c")),
                new(cardId: 11409, star: 5, isGold: true, packId: 114, cardName_cn: LocalizationPool.Get("Card/8684"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/6c6f")),
                new(cardId: 11501, star: 4, isGold: false, packId: 115, cardName_cn: LocalizationPool.Get("Card/5a78"), themeId: 10000, desc_cn: ""),
                new(cardId: 11502, star: 4, isGold: false, packId: 115, cardName_cn: LocalizationPool.Get("Card/e353"), themeId: 10000, desc_cn: ""),
                new(cardId: 11503, star: 4, isGold: false, packId: 115, cardName_cn: LocalizationPool.Get("Card/e1d9"), themeId: 10000, desc_cn: ""),
                new(cardId: 11504, star: 4, isGold: false, packId: 115, cardName_cn: LocalizationPool.Get("Card/163e"), themeId: 10000, desc_cn: ""),
                new(cardId: 11505, star: 5, isGold: false, packId: 115, cardName_cn: LocalizationPool.Get("Card/43a6"), themeId: 10000, desc_cn: ""),
                new(cardId: 11506, star: 5, isGold: false, packId: 115, cardName_cn: LocalizationPool.Get("Card/6954"), themeId: 10000, desc_cn: ""),
                new(cardId: 11507, star: 5, isGold: true, packId: 115, cardName_cn: LocalizationPool.Get("Card/3109"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/cc54")),
                new(cardId: 11508, star: 5, isGold: true, packId: 115, cardName_cn: LocalizationPool.Get("Card/1a11"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/15ce")),
                new(cardId: 11509, star: 5, isGold: true, packId: 115, cardName_cn: LocalizationPool.Get("Card/a4bc"), themeId: 10000, desc_cn: LocalizationPool.Get("Card/c43d"))
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly Card this[int cardId]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(cardId, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[Card] cardId: {cardId} not found");
                return ref _data[idx];
            }
        }
        
        public Card[] All => _data;
        
        public int Count => _data.Length;
        
        public override void Dispose()
        {
            _data = null;
            OnDispose();
        }
        
        private void MakeIdToIdx()
        {
            _idToIdx = new Dictionary<int,int>(_data.Length);
            for (int i = 0; i < _data.Length; i++)
            {
                _idToIdx[_data[i].cardId] = i;
            }
        }
    }
}