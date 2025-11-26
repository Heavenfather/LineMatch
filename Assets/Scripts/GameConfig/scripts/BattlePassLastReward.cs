/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 通行证.xlsx
*/

namespace GameConfig
{
    
    public readonly struct BattlePassLastReward
    {
        
        /// <summary>
        /// 唯一id
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 升级所需资源量
        /// </summary>
        public int key { get; }
        
        /// <summary>
        /// 普通奖励
        /// </summary>
        public string reward1 { get; }
        
        /// <summary>
        /// 付费奖励
        /// </summary>
        public string reward2 { get; }
        
        internal BattlePassLastReward(int id, int key, string reward1, string reward2)
        {
            this.id = id;
            this.key = key;
            this.reward1 = reward1;
            this.reward2 = reward2;
        }
    }
}