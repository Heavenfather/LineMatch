/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: X 新手引导.xlsx
*/

namespace GameConfig
{
    using UnityEngine;

    public readonly struct GuideConfig
    {
        
        /// <summary>
        /// 延迟多少秒触发下一个引导
        /// </summary>
        public float delayNext { get; }
        
        /// <summary>
        /// 完成时锁屏时间
        /// </summary>
        public float finishLock { get; }
        
        /// <summary>
        /// 引导唯一ID
        /// </summary>
        public int id { get; }
        
        /// <summary>
        /// 引导下一个ID
        /// </summary>
        public int nextId { get; }
        
        /// <summary>
        /// 引导内容Y轴是否自动粘附到目标附近 为True时，contentPos的值只做偏移
        /// </summary>
        public bool stickYTarget { get; }
        
        /// <summary>
        /// 引导描述
        /// </summary>
        public string content { get; }
        
        /// <summary>
        /// 结束点节点，用于适配不同分辨率在世界坐标系中自动识别宽高
        /// </summary>
        public string endNodePath { get; }
        
        /// <summary>
        /// 引导额外参数
        /// </summary>
        public string guideParameters { get; }
        
        /// <summary>
        /// 引导额外参数2
        /// </summary>
        public string guideParameters2 { get; }
        
        /// <summary>
        /// 挖孔起始节点，挖孔数据里的位置只做偏移
        /// </summary>
        public string nodePath { get; }
        
        /// <summary>
        /// 引导完成后获得奖励id， 这个是和后端约定好的id
        /// </summary>
        public string rewardId { get; }
        
        /// <summary>
        /// 触发引导模块
        /// </summary>
        public string triggerModule { get; }
        
        /// <summary>
        /// 触发引导参数
        /// </summary>
        public string triggerParameters { get; }
        
        /// <summary>
        /// 手指引导类型： None:默认不填就不显示手指 Touch:缓动引导点击
        /// </summary>
        public GuideFingerType fingerType { get; }
        
        /// <summary>
        /// 引导类型: Force=强引导 Weak=弱引导
        /// </summary>
        public GuideType guideType { get; }
        
        /// <summary>
        /// 挖孔类型: Rectangle:圆角矩形 Circle：圆形
        /// </summary>
        public GuideHoleShape holeShape { get; }
        
        /// <summary>
        /// 引导内容位置
        /// </summary>
        public Vector2 contentPos { get; }
        
        /// <summary>
        /// 挖孔数据
        /// </summary>
        public Rect holeData { get; }
        
        internal GuideConfig(float delayNext, float finishLock, int id, int nextId, bool stickYTarget, string content, string endNodePath, string guideParameters, string guideParameters2, string nodePath, string rewardId, string triggerModule, string triggerParameters, GuideFingerType fingerType, GuideType guideType, GuideHoleShape holeShape, Vector2 contentPos, Rect holeData)
        {
            this.delayNext = delayNext;
            this.finishLock = finishLock;
            this.id = id;
            this.nextId = nextId;
            this.stickYTarget = stickYTarget;
            this.content = content;
            this.endNodePath = endNodePath;
            this.guideParameters = guideParameters;
            this.guideParameters2 = guideParameters2;
            this.nodePath = nodePath;
            this.rewardId = rewardId;
            this.triggerModule = triggerModule;
            this.triggerParameters = triggerParameters;
            this.fingerType = fingerType;
            this.guideType = guideType;
            this.holeShape = holeShape;
            this.contentPos = contentPos;
            this.holeData = holeData;
        }
    }
}