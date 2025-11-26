/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 每日任务.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;

    public readonly struct Tasks
    {
        
        /// <summary>
        /// 配置索引
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 对应的任务数量要求 登录对应的是天数
        /// </summary>
        public int num { get; }
        
        /// <summary>
        /// 任务类型： 1-登录天数 2-每天登录 3-通关 4-通关后免费获得金币（包括看广告翻倍） 5-获得卡牌 6-使用道具 7-付费金额 8-付费次数 9-寻宝收集物品 10-消耗金币 11-看广告 12-消耗体力 13-生成火箭 14-生成炸弹 15-生成彩球 16-使用炸弹*火箭 17-使用炸弹*炸弹 18-使用彩球*火箭 19-使用火箭*火箭 20-使用彩球*炸弹 21-使用彩球*彩球 22-使用局内道具 23-分享 24-获得星星
        /// </summary>
        public int tag { get; }
        
        /// <summary>
        /// 任务描述
        /// </summary>
        public string desc { get; }
        
        /// <summary>
        /// 奖励
        /// </summary>
        public List<string> reward { get; }
        
        internal Tasks(int id, int num, int tag, string desc, List<string> reward)
        {
            this.id = id;
            this.num = num;
            this.tag = tag;
            this.desc = desc;
            this.reward = reward;
        }
    }
}