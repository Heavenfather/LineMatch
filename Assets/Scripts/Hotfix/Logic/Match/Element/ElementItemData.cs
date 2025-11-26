using System.Collections.Generic;
using GameConfig;
using HotfixCore.MemoryPool;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class ElementItemData : IMemory
    {
        /// <summary>
        /// 唯一id
        /// </summary>
        public int UId;

        /// <summary>
        /// 配置id
        /// </summary>
        public int ConfigId;

        /// <summary>
        /// 排序优先级
        /// </summary>
        public int Priority;

        /// <summary>
        /// 占格方式
        /// </summary>
        public float HoldGrid;

        /// <summary>
        /// 当前元素类型
        /// </summary>
        public ElementType ElementType;

        /// <summary>
        /// 当前元素朝向
        /// </summary>
        public ElementDirection Direction;

        /// <summary>
        /// 元素所处的格子坐标
        /// </summary>
        public Vector2Int GridPos { get; private set; }

        /// <summary>
        /// 是否可以移动类型的
        /// </summary>
        public bool IsMovable;
        
        /// <summary>
        /// 消除次数
        /// </summary>
        /// <returns></returns>
        public int EliminateCount;

        private int _nextId;

        /// <summary>
        /// 消除后的下一个元素id
        /// </summary>
        public int NextBlockId
        {
            get=>_nextId;
            set
            {
                _nextId = value;
            }
        }

        /// <summary>
        /// 消除方式
        /// </summary>
        public EliminateStyle EliminateStyle;

        /// <summary>
        /// 排序
        /// </summary>
        public int SortOrder;

        /// <summary>
        /// 彩色球形成的元素id
        /// </summary>
        public int ColorBallDestroyId;
        
        /// <summary>
        /// 消除指定颜色才影响障碍物的颜色Id列表
        /// </summary>
        public List<int> EffElementIds;

        /// <summary>
        /// 额外数据
        /// </summary>
        public string Extra = "";

        /// <summary>
        /// 更新当前所处坐标
        /// </summary>
        public void UpdatePos(int x, int y)
        {
            GridPos = new Vector2Int(x, y);
        }

        /// <summary>
        /// 更新当前所处坐标
        /// </summary>
        public void UpdatePos(Vector2Int pos)
        {
            GridPos = pos;
        }

        public void Clear()
        {
            Direction = ElementDirection.None;
            Priority = 0;
            EliminateCount = 0;
            // AttachIds?.Clear();
            EffElementIds?.Clear();
            ColorBallDestroyId = -1;
        }
    }
}