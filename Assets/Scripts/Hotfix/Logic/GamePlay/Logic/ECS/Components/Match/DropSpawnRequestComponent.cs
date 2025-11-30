using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 掉落生成请求：标记哪一列的哪些行需要生成新元素
    /// </summary>
    public struct DropSpawnRequestComponent
    {
        public int Column;
        public int DropBanConfigId; // 掉落算法在检测时需要排除掉的棋子Id
        public List<int> TargetRows; // 明确指定需要填充的行号列表
    }
}