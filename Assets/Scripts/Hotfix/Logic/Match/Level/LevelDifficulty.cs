using System;
using System.Collections.Generic;
using Hotfix.Define;

namespace HotfixLogic.Match
{
    /// <summary>
    /// 关卡难度变化类型
    /// </summary>
    public enum LevelDifficultyType : int
    {
        /// <summary>
        /// 不调整难度
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 基础棋子概率掉落重新调整，以便更容易形成方格
        /// </summary>
        DropRate,
        
        /// <summary>
        /// 调整进入时功能棋子相连的概率
        /// </summary>
        FillSpecialRate,
        
        /// <summary>
        /// 掉落形成功能棋子布局
        /// </summary>
        ElementLayout,
        
        Last
    }

    public struct LevelDifficultyModifyData
    {
        /// <summary>
        /// 降低难度或者增加难度
        /// </summary>
        public bool IsHarder;

        /// <summary>
        /// 调整难度类型
        /// </summary>
        public LevelDifficultyType DifficultyType;
        
        /// <summary>
        /// 难度调整比例(百分比小数)
        /// </summary>
        public float ModifyRate;
    }
    
    [Serializable]
    public class DifficultyData
    {
        public int levelId;
        public int difficulty;
    }

    [Serializable]
    public class LevelDifficulty
    {
        public List<DifficultyData> levelA = new();

        public List<DifficultyData> levelB = new();
        
        public List<DifficultyData> levelC = new();

        public int GetLevelDifficulty(int levelId,MatchLevelType levelType = MatchLevelType.A)
        {
            if (levelId > 100) levelType = MatchLevelType.B;

            if(levelType == MatchLevelType.A)
            {
                if(levelA == null)
                    return 1;
                int index = levelA.FindIndex(x => x.levelId == levelId);
                if(index == -1)
                    return 1;
                return levelA[index].difficulty;
            }

            if (levelType == MatchLevelType.C)
            {
                if(levelC == null)
                    return 1;
                int index = levelC.FindIndex(x => x.levelId == levelId);
                if (index == -1)
                {
                    return 1;
                }
                return levelC[index].difficulty;
            }
            
            if(levelB == null)
                return 1;
            int indexB = levelB.FindIndex(x => x.levelId == levelId);
            if(indexB == -1)
                return 1;
            return levelB[indexB].difficulty;
        }

        public void Clear()
        {
            levelA.Clear();
            levelB.Clear();
            levelC.Clear();
        }
    }
}