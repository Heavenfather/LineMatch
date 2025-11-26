namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 打包实体结构体，包含实体ID和生成号，用于安全地传递实体引用
    /// </summary>
    public struct EcsPackedEntity
    {
        /// <summary>
        /// 实体ID
        /// </summary>
        public int Id;

        /// <summary>
        /// 实体生成号，用于检测实体是否已被回收
        /// </summary>
        public int Gen;

        /// <summary>
        /// 获取哈希码，用于字典等集合
        /// </summary>
        /// <returns>哈希码值</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (23 * 31 + Id) * 31 + Gen;
            }
        }
    }

    /// <summary>
    /// 打包实体与世界结构体，包含实体ID、生成号和所属世界
    /// </summary>
    public struct EcsPackedEntityWithWorld
    {
        /// <summary>
        /// 实体ID
        /// </summary>
        public int Id;

        /// <summary>
        /// 实体生成号，用于检测实体是否已被回收
        /// </summary>
        public int Gen;

        /// <summary>
        /// 实体所属的世界
        /// </summary>
        public EcsWorld World;

        public override int GetHashCode()
        {
            unchecked
            {
                return ((23 * 31 + Id) * 31 + Gen) * 31 + (World?.GetHashCode() ?? 0);
            }
        }
        
#if DEBUG
        /// <summary>
        /// 调试用组件视图
        /// </summary>
        internal object[] DebugComponentsView
        {
            get
            {
                object[] list = null;
                if (World != null && World.IsAlive() && World.IsEntityAliveInternal(Id) &&
                    World.GetEntityGen(Id) == Gen)
                {
                    World.GetComponents(Id, ref list);
                }

                return list;
            }
        }

        /// <summary>
        /// 调试用组件数量
        /// </summary>
        internal int DebugComponentsCount
        {
            get
            {
                if (World != null && World.IsAlive() && World.IsEntityAliveInternal(Id) &&
                    World.GetEntityGen(Id) == Gen)
                {
                    return World.GetComponentsCount(Id);
                }

                return 0;
            }
        }

        /// <summary>
        /// 转换为字符串，用于调试显示实体信息
        /// </summary>
        /// <returns>实体信息字符串</returns>
        public override string ToString()
        {
            if (Id == 0 && Gen == 0)
            {
                return "Entity-Null";
            }

            if (World == null || !World.IsAlive() || !World.IsEntityAliveInternal(Id) || World.GetEntityGen(Id) != Gen)
            {
                return "Entity-NonAlive";
            }

            System.Type[] types = null;
            var count = World.GetComponentTypes(Id, ref types);
            System.Text.StringBuilder sb = null;
            if (count > 0)
            {
                sb = new System.Text.StringBuilder(512);
                for (var i = 0; i < count; i++)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append(types[i].Name);
                }
            }

            return $"Entity-{Id}:{Gen} [{sb}]";
        }
#endif
    }
}