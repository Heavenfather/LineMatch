using System.Collections.Generic;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Module;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 消除逻辑产生的数据记录
    /// </summary>
    public class MatchStateContext
    {
        private int _remainStep;
        /// <summary>
        /// 关卡剩余步数
        /// </summary>
        public int RemainStep
        {
            get
            {
                return _remainStep;
            }
            private set
            {
                _remainStep = value;
                // 通知UI
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchStepModify, EventOneParam<int>.Create(_remainStep));
            }
        }

        /// <summary>
        /// 记录掉落物的已生成数量 (Key: ConfigId, Value: Count)
        /// </summary>
        public Dictionary<int, int> DropElementMapCounts = new Dictionary<int, int>();

        /// <summary>
        /// 全局掉落配额 (Key: ConfigId, Value: 剩余允许掉落的最大数量)
        /// 如果 Key 不存在，代表该元素没有全局数量限制（或者不是目标）
        /// </summary>
        public Dictionary<int, int> GlobalDropQuotas = new Dictionary<int, int>();

        /// <summary>
        /// 目标棋子的合法掉落列索引集合 (Key: TargetId, Value: List of Column Index)
        /// 只要棋盘初始配置里该列出现了目标（或其变体），该列就记录在案
        /// </summary>
        public Dictionary<int, List<int>> TargetValidColumns = new Dictionary<int, List<int>>();

        /// <summary>
        /// 增加掉落计数
        /// </summary>
        public void AddDropCount(int configId)
        {
            if (DropElementMapCounts.ContainsKey(configId))
            {
                DropElementMapCounts[configId]++;
            }
            else
            {
                DropElementMapCounts[configId] = 1;
            }
        }

        /// <summary>
        /// 获取当前掉落计数
        /// </summary>
        public int GetDropCount(int configId)
        {
            if (DropElementMapCounts.TryGetValue(configId, out int count))
            {
                return count;
            }

            return 0;
        }

        /// <summary>
        /// 消耗一个配额
        /// </summary>
        public void ConsumeDropQuota(int configId)
        {
            if (GlobalDropQuotas.ContainsKey(configId))
            {
                if (GlobalDropQuotas[configId] > 0)
                {
                    GlobalDropQuotas[configId]--;
                }
            }
        }

        /// <summary>
        /// 检查是否还有配额
        /// </summary>
        public bool HasDropQuota(int configId)
        {
            if (GlobalDropQuotas.TryGetValue(configId, out int left))
            {
                return left > 0;
            }

            // 如果没有记录配额，说明不是受限的目标物品，默认允许
            return true;
        }

        /// <summary>
        /// 注册合法列
        /// </summary>
        public void RegisterTargetColumn(int targetId, int col)
        {
            if (!TargetValidColumns.TryGetValue(targetId, out var list))
            {
                list = new List<int>();
                TargetValidColumns.Add(targetId, list);
            }

            if (!list.Contains(col))
            {
                list.Add(col);
            }
        }

        /// <summary>
        /// 设置关卡步数
        /// </summary>
        /// <param name="total"></param>
        public void SetStep(int total)
        {
            this.RemainStep = total;
        }

        /// <summary>
        /// 添加步数
        /// </summary>
        /// <param name="count"></param>
        public void AddStep(int count)
        {
            RemainStep += count;
        }

        /// <summary>
        /// 扣除步数
        /// </summary>
        public void ResumeStep(int count = 1)
        {
            RemainStep -= count;
        }

        /// <summary>
        /// 每次新输入清理
        /// </summary>
        public void RoundClear()
        {
        }

        /// <summary>
        /// 关卡退出清理
        /// </summary>
        public void Clear()
        {
            RoundClear();

            DropElementMapCounts.Clear();
            GlobalDropQuotas.Clear();
            TargetValidColumns.Clear();
        }
    }
}