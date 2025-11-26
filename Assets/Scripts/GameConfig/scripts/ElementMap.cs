/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: Y 元素映射表.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;
    using UnityEngine;

    public readonly struct ElementMap
    {
        
        /// <summary>
        /// 唯一Id
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// 指定的目标完成后，消除该棋子将无效
        /// </summary>
        public int checkTargetId { get; }
        
        /// <summary>
        /// 初始化时延迟显示时长
        /// </summary>
        public float delayShow { get; }
        
        /// <summary>
        /// 播受击动画时延迟多久播放消除动画
        /// </summary>
        public float delayTimePlay { get; }
        
        /// <summary>
        /// 消除次数: -1表示无法被消除
        /// </summary>
        public int eliminateCount { get; }
        
        /// <summary>
        /// 占格数量，默认1格
        /// </summary>
        public float holdGrid { get; }
        
        /// <summary>
        /// 是否可移动
        /// </summary>
        public bool isMovable { get; }
        
        /// <summary>
        /// 消除后的下一个障碍物
        /// </summary>
        public int nextBlock { get; }
        
        /// <summary>
        /// 分数基础值
        /// </summary>
        public int score { get; }
        
        /// <summary>
        /// 元素排序 区间规划： [-5--1]置于最底，如樱花草地等 0:连线的层级，请勿占用 [1-10]可被覆盖的元素，如普通障碍物，基础棋子等 [11-15]覆盖在消除元素上方，如雪地、藤蔓、冰块等类型 [16-20]最高层的覆盖元素，如卷帘
        /// </summary>
        public int sortOrder { get; }
        
        /// <summary>
        /// 对应元素的实体
        /// </summary>
        public string address { get; }
        
        /// <summary>
        /// 对应元素的实体
        /// </summary>
        public string address_icon { get; }
        
        /// <summary>
        /// 音效名称
        /// </summary>
        public string audio { get; }
        public string collectAudio { get; }
        
        /// <summary>
        /// 特殊元素定制参数
        /// </summary>
        public string extra { get; }
        
        /// <summary>
        /// 受击动画
        /// </summary>
        public string hitSpine { get; }
        
        /// <summary>
        /// 待机动画
        /// </summary>
        public string idleSpine { get; }
        
        /// <summary>
        /// 加载特效名称
        /// </summary>
        public string loadEffect { get; }
        
        /// <summary>
        /// 元素名称标识
        /// </summary>
        public string nameFlag { get; }
        
        /// <summary>
        /// 元素默认朝向： None Up Down Left Right
        /// </summary>
        public ElementDirection direction { get; }
        
        /// <summary>
        /// 元素类型： Normal：普通消除点 Rocket：火箭类型元素 Bomb：炸弹类型元素 ColorBall：彩色球类型 Block:障碍物，不可直接点击消除 Collect:收集元素 Lock:锁住基础元素的障碍物 Background:底板，半透明或底板类型元素，不会阻挡基础元素的 DropBlock:可掉落类型障碍物 ColorBlock:同色才消除的障碍物 SpreadGround：消除后向外一圈扩散 RandomDiffuse:消除后散开成其它元素 TargetBlock:指定目标的障碍物 VerticalExpand:垂直方向拓展障碍物
        /// </summary>
        public ElementType elementType { get; }
        
        /// <summary>
        /// 消除方式 Match:连线 Side：旁消 Drop：掉落 Bomb:爆炸 Target：指定目标
        /// </summary>
        public EliminateStyle eliminateStyle { get; }
        
        /// <summary>
        /// 特效偏移
        /// </summary>
        public Vector2 effectOffset { get; }
        
        /// <summary>
        /// 消除指定颜色才影响障碍物的颜色Id列表
        /// </summary>
        public List<int> effEleIds { get; }
        
        internal ElementMap(int Id, int checkTargetId, float delayShow, float delayTimePlay, int eliminateCount, float holdGrid, bool isMovable, int nextBlock, int score, int sortOrder, string address, string address_icon, string audio, string collectAudio, string extra, string hitSpine, string idleSpine, string loadEffect, string nameFlag, ElementDirection direction, ElementType elementType, EliminateStyle eliminateStyle, Vector2 effectOffset, List<int> effEleIds)
        {
            this.Id = Id;
            this.checkTargetId = checkTargetId;
            this.delayShow = delayShow;
            this.delayTimePlay = delayTimePlay;
            this.eliminateCount = eliminateCount;
            this.holdGrid = holdGrid;
            this.isMovable = isMovable;
            this.nextBlock = nextBlock;
            this.score = score;
            this.sortOrder = sortOrder;
            this.address = address;
            this.address_icon = address_icon;
            this.audio = audio;
            this.collectAudio = collectAudio;
            this.extra = extra;
            this.hitSpine = hitSpine;
            this.idleSpine = idleSpine;
            this.loadEffect = loadEffect;
            this.nameFlag = nameFlag;
            this.direction = direction;
            this.elementType = elementType;
            this.eliminateStyle = eliminateStyle;
            this.effectOffset = effectOffset;
            this.effEleIds = effEleIds;
        }
    }
}