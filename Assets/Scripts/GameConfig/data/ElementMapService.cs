using System.Collections.Generic;

namespace GameConfig
{
    public partial class ElementMapDB
    {
        private int _findCount = 0;

        public void FindAfterReduceNextId(int elementId, int reduceCount, bool resetFind, ref int nextId,
            ref List<int> attachIds)
        {
            if (attachIds == null)
                return;
            if (resetFind)
                _findCount = 0;
            ref readonly ElementMap config = ref this[elementId];
            if (config.nextBlock == 0)
            {
                nextId = 0;
                return;
            }

            if (config.elementType == ElementType.SpreadGround)
            {
                if (!attachIds.Contains(elementId))
                    attachIds.Add(elementId);
                nextId = elementId;
                return;
            }

            if (attachIds.Contains(config.nextBlock))
                return;
            nextId = config.nextBlock;
            attachIds.Add(config.nextBlock);
            ++_findCount;
            if (_findCount < reduceCount)
            {
                FindAfterReduceNextId(nextId, reduceCount, false, ref nextId, ref attachIds);
            }
        }

        public bool IsContain(int elementId)
        {
            return _idToIdx.ContainsKey(elementId);
        }

        /// <summary>
        /// 该元素是否是循环元素
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public bool IsCircleElement(int elementId)
        {
            List<int> eleIds = new List<int>();
            RefElementNextList(elementId, ref eleIds);
            if (eleIds.Count <= 0)
                return false;
            bool bResult = true;
            for (int i = 0; i < eleIds.Count; i++)
            {
                var next = this[eleIds[i]].nextBlock;
                if (!eleIds.Contains(next))
                {
                    bResult = false;
                }
            }
            
            return bResult;
        }
        
        public void RefElementNextList(int elementId, ref List<int> nextIds)
        {
            ref readonly ElementMap config = ref this[elementId];
            if (config.nextBlock <= 0)
                return;
            if (nextIds.Contains(config.nextBlock))
                return;
            nextIds.Add(config.nextBlock);
            RefElementNextList(config.nextBlock, ref nextIds);
        }

        /// <summary>
        /// 计算元素总的消除次数
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        public int CalculateTotalEliminateCount(int configId)
        {
            int count = 0;
            List<int> nextIds = new List<int>() { configId };
            RefElementNextList(configId, ref nextIds);
            for (int i = 0; i < nextIds.Count; i++)
            {
                count += this[nextIds[i]].eliminateCount;
            }
            return count;
        }

        /// <summary>
        /// 是否是金币类型的元素
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public bool IsCoinTypeElement(int elementId)
        {
            ref readonly ElementMap config = ref this[elementId];
            return config.elementType == ElementType.Coin;
        }

        /// <summary>
        /// 是否是收集元素
        /// </summary>
        public bool IsCollectElement(int elementId)
        {
            ref readonly ElementMap config = ref this[elementId];
            if (config.eliminateCount == 0)
                return true;
            if (config.elementType == ElementType.JumpCollect)
                return true;
            if (config.elementType == ElementType.Collect)
                return true;
            if (config.elementType == ElementType.Coin)
                return true;
            return false;
        }

        /// <summary>
        /// 是否需要消除多次的彩色元素
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public bool IsMultipleColorBlock(int elementId)
        {
            ref readonly ElementMap config = ref this[elementId];
            return config is { elementType: ElementType.ColorBlock, eliminateCount: > 1 };
        }
    }
}