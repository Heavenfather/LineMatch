using System.Runtime.CompilerServices;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 实体扩展方法类
    /// </summary>
    public static class EcsEntityExtensions
    {
        /// <summary>
        /// 将实体打包为EcsPackedEntity
        /// </summary>
        /// <param name="world">实体所属的世界</param>
        /// <param name="entity">实体ID</param>
        /// <returns>打包后的实体</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsPackedEntity PackEntity(this EcsWorld world, int entity)
        {
            EcsPackedEntity packed;
            packed.Id = entity;
            packed.Gen = world.GetEntityGen(entity);
            return packed;
        }

        /// <summary>
        /// 解包EcsPackedEntity为实体ID
        /// </summary>
        /// <param name="packed">打包的实体</param>
        /// <param name="world">实体所属的世界</param>
        /// <param name="entity">解包后的实体ID</param>
        /// <returns>如果解包成功返回true，否则false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unpack(this in EcsPackedEntity packed, EcsWorld world, out int entity)
        {
            entity = packed.Id;
            return
                world != null
                && world.IsAlive()
                && world.IsEntityAliveInternal(packed.Id)
                && world.GetEntityGen(packed.Id) == packed.Gen;
        }

        /// <summary>
        /// 比较两个EcsPackedEntity是否相等
        /// </summary>
        /// <param name="a">第一个打包实体</param>
        /// <param name="b">第二个打包实体</param>
        /// <returns>如果相等返回true，否则false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsTo(this in EcsPackedEntity a, in EcsPackedEntity b)
        {
            return a.Id == b.Id && a.Gen == b.Gen;
        }

        /// <summary>
        /// 将实体打包为EcsPackedEntityWithWorld
        /// </summary>
        /// <param name="world">实体所属的世界</param>
        /// <param name="entity">实体ID</param>
        /// <returns>打包后的实体与世界</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsPackedEntityWithWorld PackEntityWithWorld(this EcsWorld world, int entity)
        {
            EcsPackedEntityWithWorld packedEntity;
            packedEntity.World = world;
            packedEntity.Id = entity;
            packedEntity.Gen = world.GetEntityGen(entity);
            return packedEntity;
        }

        /// <summary>
        /// 解包EcsPackedEntityWithWorld为世界和实体ID
        /// </summary>
        /// <param name="packedEntity">打包的实体与世界</param>
        /// <param name="world">解包后的世界</param>
        /// <param name="entity">解包后的实体ID</param>
        /// <returns>如果解包成功返回true，否则false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unpack(this in EcsPackedEntityWithWorld packedEntity, out EcsWorld world, out int entity)
        {
            world = packedEntity.World;
            entity = packedEntity.Id;
            return
                world != null
                && world.IsAlive()
                && world.IsEntityAliveInternal(packedEntity.Id)
                && world.GetEntityGen(packedEntity.Id) == packedEntity.Gen;
        }

        /// <summary>
        /// 比较两个EcsPackedEntityWithWorld是否相等
        /// </summary>
        /// <param name="a">第一个打包实体与世界</param>
        /// <param name="b">第二个打包实体与世界</param>
        /// <returns>如果相等返回true，否则false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsTo(this in EcsPackedEntityWithWorld a, in EcsPackedEntityWithWorld b)
        {
            return a.Id == b.Id && a.Gen == b.Gen && a.World == b.World;
        }
    }
}