/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 道具配置表.xlsx
*/

namespace GameConfig
{
    using GameCore.Localization;
    using System.Collections.Generic;


    public partial class ItemEnumDB : ConfigBase
    {
        private ItemEnum[] _data;
        private Dictionary<int, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new ItemEnum[]
            {
                new(Id: 1000, name: "none", alias: LocalizationPool.Get("ItemEnum/ff0e"), comment: "", tags: ItemEnumType.Normal, iconColor: "", iconDes: ""),
                new(Id: 1001, name: "coin", alias: LocalizationPool.Get("ItemEnum/ed29"), comment: "", tags: ItemEnumType.Normal, iconColor: "#d6c2a7", iconDes: LocalizationPool.Get("ItemEnum/bd61")),
                new(Id: 1002, name: "live", alias: LocalizationPool.Get("ItemEnum/2c78"), comment: "", tags: ItemEnumType.Normal, iconColor: "#e8caca", iconDes: LocalizationPool.Get("ItemEnum/749e")),
                new(Id: 1003, name: "liveBuff", alias: LocalizationPool.Get("ItemEnum/f0c8"), comment: LocalizationPool.Get("ItemEnum/3132"), tags: ItemEnumType.Normal, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/8356")),
                new(Id: 1004, name: "star", alias: LocalizationPool.Get("ItemEnum/26cc"), comment: "", tags: ItemEnumType.Normal, iconColor: "#e3b9a1", iconDes: LocalizationPool.Get("ItemEnum/7706")),
                new(Id: 1005, name: "health3Buff", alias: LocalizationPool.Get("ItemEnum/baae"), comment: "", tags: ItemEnumType.Normal, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/7491")),
                new(Id: 1006, name: "freeRevive", alias: LocalizationPool.Get("ItemEnum/4ee0"), comment: "", tags: ItemEnumType.Normal, iconColor: "", iconDes: ""),
                new(Id: 1101, name: "eliminateRock", alias: LocalizationPool.Get("ItemEnum/65a5"), comment: "", tags: ItemEnumType.Match, iconColor: "#e5bfa8", iconDes: LocalizationPool.Get("ItemEnum/850e")),
                new(Id: 1102, name: "eliminateBoom", alias: LocalizationPool.Get("ItemEnum/44d8"), comment: "", tags: ItemEnumType.Match, iconColor: "#e4b4b4", iconDes: LocalizationPool.Get("ItemEnum/19ab")),
                new(Id: 1103, name: "eliminateBall", alias: LocalizationPool.Get("ItemEnum/0f9c"), comment: "", tags: ItemEnumType.Match, iconColor: "#c8b9d0", iconDes: LocalizationPool.Get("ItemEnum/4319")),
                new(Id: 1104, name: "eliminateRockBuff", alias: LocalizationPool.Get("ItemEnum/bd03"), comment: LocalizationPool.Get("ItemEnum/3cee"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/a9c6")),
                new(Id: 1105, name: "eliminateBoomBuff", alias: LocalizationPool.Get("ItemEnum/6eca"), comment: LocalizationPool.Get("ItemEnum/3cee"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/a9c6")),
                new(Id: 1106, name: "eliminateBallBuff", alias: LocalizationPool.Get("ItemEnum/1193"), comment: LocalizationPool.Get("ItemEnum/3cee"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/a9c6")),
                new(Id: 1107, name: "eliminateDice", alias: LocalizationPool.Get("ItemEnum/73c6"), comment: "", tags: ItemEnumType.Match, iconColor: "#acafe6", iconDes: LocalizationPool.Get("ItemEnum/96b8")),
                new(Id: 1108, name: "eliminateHammer", alias: LocalizationPool.Get("ItemEnum/3c06"), comment: "", tags: ItemEnumType.Match, iconColor: "#a7beb6", iconDes: LocalizationPool.Get("ItemEnum/e3b4")),
                new(Id: 1109, name: "eliminateArrow", alias: LocalizationPool.Get("ItemEnum/1e75"), comment: "", tags: ItemEnumType.Match, iconColor: "#e1b3b3", iconDes: LocalizationPool.Get("ItemEnum/0bb5")),
                new(Id: 1110, name: "eliminateBullet", alias: LocalizationPool.Get("ItemEnum/32cb"), comment: "", tags: ItemEnumType.Match, iconColor: "#b8c4d4", iconDes: LocalizationPool.Get("ItemEnum/0822")),
                new(Id: 1111, name: "eliminateColored", alias: LocalizationPool.Get("ItemEnum/8dea"), comment: "", tags: ItemEnumType.Match, iconColor: "#bea6bc", iconDes: LocalizationPool.Get("ItemEnum/1b31")),
                new(Id: 1112, name: "eliminateDye", alias: LocalizationPool.Get("ItemEnum/ff04"), comment: "", tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/2e8d")),
                new(Id: 1113, name: "eliminateDiceBuff", alias: LocalizationPool.Get("ItemEnum/b996"), comment: LocalizationPool.Get("ItemEnum/3cee"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/a9c6")),
                new(Id: 1114, name: "eliminateHammerBuff", alias: LocalizationPool.Get("ItemEnum/72c5"), comment: LocalizationPool.Get("ItemEnum/3cee"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/a9c6")),
                new(Id: 1115, name: "eliminateArrowBuff", alias: LocalizationPool.Get("ItemEnum/9a35"), comment: LocalizationPool.Get("ItemEnum/3cee"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/a9c6")),
                new(Id: 1116, name: "eliminateBulletBuff", alias: LocalizationPool.Get("ItemEnum/e956"), comment: LocalizationPool.Get("ItemEnum/3cee"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/a9c6")),
                new(Id: 1117, name: "eliminateColoredBuff", alias: LocalizationPool.Get("ItemEnum/79ba"), comment: LocalizationPool.Get("ItemEnum/3cee"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/a9c6")),
                new(Id: 1118, name: "eliminateDyeBuff", alias: LocalizationPool.Get("ItemEnum/c89f"), comment: LocalizationPool.Get("ItemEnum/3cee"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/a9c6")),
                new(Id: 1120, name: "doubleCollectBuff", alias: LocalizationPool.Get("ItemEnum/3c25"), comment: "", tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/a5ff")),
                new(Id: 1121, name: "winStreakRock", alias: LocalizationPool.Get("ItemEnum/f9b4"), comment: LocalizationPool.Get("ItemEnum/9275"), tags: ItemEnumType.Match, iconColor: "", iconDes: ""),
                new(Id: 1122, name: "winStreakBoom", alias: LocalizationPool.Get("ItemEnum/8cb6"), comment: LocalizationPool.Get("ItemEnum/9275"), tags: ItemEnumType.Match, iconColor: "", iconDes: ""),
                new(Id: 1123, name: "winStreakBall", alias: LocalizationPool.Get("ItemEnum/bb47"), comment: LocalizationPool.Get("ItemEnum/9275"), tags: ItemEnumType.Match, iconColor: "", iconDes: ""),
                new(Id: 1124, name: "eliminateBox", alias: LocalizationPool.Get("ItemEnum/1125"), comment: LocalizationPool.Get("ItemEnum/9300"), tags: ItemEnumType.Match, iconColor: "", iconDes: ""),
                new(Id: 1125, name: "eliminateStarBombDots", alias: LocalizationPool.Get("ItemEnum/9584"), comment: LocalizationPool.Get("ItemEnum/9275"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/9828")),
                new(Id: 1126, name: "eliminateSearchDots", alias: LocalizationPool.Get("ItemEnum/63c4"), comment: LocalizationPool.Get("ItemEnum/9275"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/7590")),
                new(Id: 1127, name: "eliminateHorizontalDots", alias: LocalizationPool.Get("ItemEnum/455c"), comment: LocalizationPool.Get("ItemEnum/c013"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/daa1")),
                new(Id: 1128, name: "eliminateBombDots", alias: LocalizationPool.Get("ItemEnum/db5a"), comment: LocalizationPool.Get("ItemEnum/c013"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/fd97")),
                new(Id: 1129, name: "eliminateColoredDots", alias: LocalizationPool.Get("ItemEnum/88ce"), comment: LocalizationPool.Get("ItemEnum/c013"), tags: ItemEnumType.Match, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/c1eb")),
                new(Id: 1201, name: "puzzleLoupe", alias: LocalizationPool.Get("ItemEnum/d86d"), comment: "", tags: ItemEnumType.Puzzle, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/90fc")),
                new(Id: 1202, name: "puzzlePosition", alias: LocalizationPool.Get("ItemEnum/3ee3"), comment: "", tags: ItemEnumType.Puzzle, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/5ee5")),
                new(Id: 1203, name: "puzzleVacuum", alias: LocalizationPool.Get("ItemEnum/c77d"), comment: "", tags: ItemEnumType.Puzzle, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/9bce")),
                new(Id: 1204, name: "puzzleLoupeAD", alias: LocalizationPool.Get("ItemEnum/78bc"), comment: LocalizationPool.Get("ItemEnum/0adf"), tags: ItemEnumType.Puzzle, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/90fc")),
                new(Id: 1205, name: "puzzlePositionAD", alias: LocalizationPool.Get("ItemEnum/3756"), comment: LocalizationPool.Get("ItemEnum/0adf"), tags: ItemEnumType.Puzzle, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/5ee5")),
                new(Id: 1206, name: "puzzleVacuumAD", alias: LocalizationPool.Get("ItemEnum/2d76"), comment: LocalizationPool.Get("ItemEnum/0adf"), tags: ItemEnumType.Puzzle, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/9bce")),
                new(Id: 1301, name: "BrownPack", alias: LocalizationPool.Get("ItemEnum/6dc0"), comment: LocalizationPool.Get("ItemEnum/f942"), tags: ItemEnumType.Pack, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/ae40")),
                new(Id: 1302, name: "GreenPack", alias: LocalizationPool.Get("ItemEnum/cf65"), comment: LocalizationPool.Get("ItemEnum/ad83"), tags: ItemEnumType.Pack, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/5e28")),
                new(Id: 1303, name: "BluePack", alias: LocalizationPool.Get("ItemEnum/eac3"), comment: LocalizationPool.Get("ItemEnum/792d"), tags: ItemEnumType.Pack, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/2ab1")),
                new(Id: 1304, name: "PurplePack", alias: LocalizationPool.Get("ItemEnum/98ae"), comment: LocalizationPool.Get("ItemEnum/5431"), tags: ItemEnumType.Pack, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/afe0")),
                new(Id: 1305, name: "RedPack", alias: LocalizationPool.Get("ItemEnum/fdeb"), comment: LocalizationPool.Get("ItemEnum/5431"), tags: ItemEnumType.Pack, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/80b5")),
                new(Id: 1401, name: "Frame01", alias: LocalizationPool.Get("ItemEnum/1694"), comment: LocalizationPool.Get("ItemEnum/3233"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1402, name: "Frame02", alias: LocalizationPool.Get("ItemEnum/4fd7"), comment: LocalizationPool.Get("ItemEnum/6be0"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1403, name: "Frame03", alias: LocalizationPool.Get("ItemEnum/100d"), comment: LocalizationPool.Get("ItemEnum/6bdd"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1404, name: "Frame04", alias: LocalizationPool.Get("ItemEnum/4bdf"), comment: LocalizationPool.Get("ItemEnum/d628"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1405, name: "Frame05", alias: LocalizationPool.Get("ItemEnum/2028"), comment: LocalizationPool.Get("ItemEnum/6653"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1406, name: "Frame06", alias: LocalizationPool.Get("ItemEnum/fc81"), comment: LocalizationPool.Get("ItemEnum/69d7"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1407, name: "Frame07", alias: LocalizationPool.Get("ItemEnum/b00a"), comment: LocalizationPool.Get("ItemEnum/60ba"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1408, name: "Frame08", alias: LocalizationPool.Get("ItemEnum/1af9"), comment: LocalizationPool.Get("ItemEnum/4d33"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1409, name: "Frame09", alias: LocalizationPool.Get("ItemEnum/9f35"), comment: LocalizationPool.Get("ItemEnum/2b8b"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1410, name: "Frame10", alias: LocalizationPool.Get("ItemEnum/c7e9"), comment: LocalizationPool.Get("ItemEnum/b4fb"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1411, name: "Frame11", alias: LocalizationPool.Get("ItemEnum/c191"), comment: LocalizationPool.Get("ItemEnum/abc3"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1412, name: "Frame12", alias: LocalizationPool.Get("ItemEnum/641a"), comment: LocalizationPool.Get("ItemEnum/4c69"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1413, name: "Frame13", alias: LocalizationPool.Get("ItemEnum/04b5"), comment: LocalizationPool.Get("ItemEnum/6925"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1414, name: "Frame14", alias: LocalizationPool.Get("ItemEnum/20fa"), comment: LocalizationPool.Get("ItemEnum/37de"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1415, name: "Frame15", alias: LocalizationPool.Get("ItemEnum/43eb"), comment: LocalizationPool.Get("ItemEnum/bed5"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1416, name: "Frame16", alias: LocalizationPool.Get("ItemEnum/2aff"), comment: LocalizationPool.Get("ItemEnum/765c"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1417, name: "Frame17", alias: LocalizationPool.Get("ItemEnum/b451"), comment: LocalizationPool.Get("ItemEnum/a92a"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1418, name: "Frame18", alias: LocalizationPool.Get("ItemEnum/f5d2"), comment: LocalizationPool.Get("ItemEnum/10f6"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1419, name: "Frame19", alias: LocalizationPool.Get("ItemEnum/e929"), comment: LocalizationPool.Get("ItemEnum/5525"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1420, name: "Frame20", alias: LocalizationPool.Get("ItemEnum/4803"), comment: LocalizationPool.Get("ItemEnum/98bc"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1421, name: "Frame21", alias: LocalizationPool.Get("ItemEnum/9fcc"), comment: LocalizationPool.Get("ItemEnum/909b"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1422, name: "Frame22", alias: LocalizationPool.Get("ItemEnum/c96d"), comment: LocalizationPool.Get("ItemEnum/00b2"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1423, name: "Frame23", alias: LocalizationPool.Get("ItemEnum/adac"), comment: LocalizationPool.Get("ItemEnum/0f66"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1424, name: "Frame24", alias: LocalizationPool.Get("ItemEnum/ecb0"), comment: LocalizationPool.Get("ItemEnum/d882"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1425, name: "Frame25", alias: LocalizationPool.Get("ItemEnum/d36f"), comment: LocalizationPool.Get("ItemEnum/3c3e"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1426, name: "Frame26", alias: LocalizationPool.Get("ItemEnum/ff8e"), comment: LocalizationPool.Get("ItemEnum/35bd"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1427, name: "Frame27", alias: LocalizationPool.Get("ItemEnum/cc66"), comment: LocalizationPool.Get("ItemEnum/97ce"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1428, name: "Frame28", alias: LocalizationPool.Get("ItemEnum/99db"), comment: LocalizationPool.Get("ItemEnum/0824"), tags: ItemEnumType.HeadFrame, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/d6d9")),
                new(Id: 1501, name: "NameColor01", alias: LocalizationPool.Get("ItemEnum/19da"), comment: LocalizationPool.Get("ItemEnum/0637"), tags: ItemEnumType.Name, iconColor: "", iconDes: ""),
                new(Id: 1502, name: "NameColor02", alias: LocalizationPool.Get("ItemEnum/3246"), comment: LocalizationPool.Get("ItemEnum/d94a"), tags: ItemEnumType.Name, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/2e66")),
                new(Id: 1503, name: "NameColor03", alias: LocalizationPool.Get("ItemEnum/3e9c"), comment: LocalizationPool.Get("ItemEnum/5a2b"), tags: ItemEnumType.Name, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/c378")),
                new(Id: 1504, name: "NameColor04", alias: LocalizationPool.Get("ItemEnum/4622"), comment: LocalizationPool.Get("ItemEnum/36aa"), tags: ItemEnumType.Name, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/aa17")),
                new(Id: 1601, name: "Medal01", alias: LocalizationPool.Get("ItemEnum/f88d"), comment: LocalizationPool.Get("ItemEnum/d878"), tags: ItemEnumType.Medal, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/3b6b")),
                new(Id: 1701, name: "Head01", alias: LocalizationPool.Get("ItemEnum/ae90"), comment: LocalizationPool.Get("ItemEnum/2e20"), tags: ItemEnumType.Head, iconColor: "", iconDes: ""),
                new(Id: 1702, name: "Head02", alias: LocalizationPool.Get("ItemEnum/d768"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: ""),
                new(Id: 1703, name: "Head03", alias: LocalizationPool.Get("ItemEnum/ac60"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1704, name: "Head04", alias: LocalizationPool.Get("ItemEnum/bdf6"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1705, name: "Head05", alias: LocalizationPool.Get("ItemEnum/9093"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1706, name: "Head06", alias: LocalizationPool.Get("ItemEnum/83c8"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1707, name: "Head07", alias: LocalizationPool.Get("ItemEnum/27a9"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1708, name: "Head08", alias: LocalizationPool.Get("ItemEnum/31a9"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1709, name: "Head09", alias: LocalizationPool.Get("ItemEnum/b339"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1710, name: "Head10", alias: LocalizationPool.Get("ItemEnum/19ae"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1711, name: "Head11", alias: LocalizationPool.Get("ItemEnum/9ca5"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1712, name: "Head12", alias: LocalizationPool.Get("ItemEnum/6feb"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1713, name: "Head13", alias: LocalizationPool.Get("ItemEnum/779e"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1714, name: "Head14", alias: LocalizationPool.Get("ItemEnum/8418"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1715, name: "Head15", alias: LocalizationPool.Get("ItemEnum/aa39"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1716, name: "Head16", alias: LocalizationPool.Get("ItemEnum/8ae9"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1717, name: "Head17", alias: LocalizationPool.Get("ItemEnum/a7fb"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1718, name: "Head18", alias: LocalizationPool.Get("ItemEnum/8d01"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1719, name: "Head19", alias: LocalizationPool.Get("ItemEnum/7ba4"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1720, name: "Head20", alias: LocalizationPool.Get("ItemEnum/e2fa"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1721, name: "Head21", alias: LocalizationPool.Get("ItemEnum/cbb2"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1722, name: "Head22", alias: LocalizationPool.Get("ItemEnum/506f"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1723, name: "Head23", alias: LocalizationPool.Get("ItemEnum/8c80"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1724, name: "Head24", alias: LocalizationPool.Get("ItemEnum/64d8"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1725, name: "Head25", alias: LocalizationPool.Get("ItemEnum/b329"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1726, name: "Head26", alias: LocalizationPool.Get("ItemEnum/8fc1"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1727, name: "Head27", alias: LocalizationPool.Get("ItemEnum/c9ac"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 1728, name: "Head28", alias: LocalizationPool.Get("ItemEnum/1eee"), comment: "", tags: ItemEnumType.Head, iconColor: "", iconDes: LocalizationPool.Get("ItemEnum/b535")),
                new(Id: 2001, name: "dailyNewEg", alias: LocalizationPool.Get("ItemEnum/774f"), comment: LocalizationPool.Get("ItemEnum/5e73"), tags: ItemEnumType.Normal, iconColor: "#d6c2a7", iconDes: LocalizationPool.Get("ItemEnum/7eb2")),
                new(Id: 2002, name: "dailyEg", alias: LocalizationPool.Get("ItemEnum/774f"), comment: LocalizationPool.Get("ItemEnum/252b"), tags: ItemEnumType.Normal, iconColor: "#e8caca", iconDes: LocalizationPool.Get("ItemEnum/7eb2"))
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly ItemEnum this[int Id]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(Id, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[ItemEnum] Id: {Id} not found");
                return ref _data[idx];
            }
        }
        
        public ItemEnum[] All => _data;
        
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
                _idToIdx[_data[i].Id] = i;
            }
        }
    }
}