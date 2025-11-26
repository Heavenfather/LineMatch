using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 挂载在格子上，用于射线检测时反向查找 Entity ID
    /// 替代掉旧的MatchTouchItem,这个后面不会再用了
    /// </summary>
    public class EntityLink : MonoBehaviour
    {
        public int EntityId;
        public EcsWorld World; // 引用 World 方便调试或直接获取数据
        
        /// <summary>
        /// 链接 EntityId 和 World
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="world"></param>
        public void Link(int entityId, EcsWorld world)
        {
            this.EntityId = entityId;
            this.World = world;
        }
    }
}