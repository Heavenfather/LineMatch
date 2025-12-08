using HotfixCore.Extensions;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// TowDots模式下的直线消除点系统，负责处理直线消除点的表现效果
    /// 特性：
    /// 1. 初始为白色，可与任意Normal棋子相连
    /// 2. 连接后变成对应颜色，断开连接后恢复白色
    /// 3. 消除效果：消除所在行的所有棋子（行号跟随连线位置变化）
    /// </summary>
    public class HorizontalDotSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<VariableColorComponent> _variableColorPool;
        private EcsPool<EliminatedTag> _eliminatePool;
        private IElementFactoryService _elementService;

        public void Init(IEcsSystems systems)
        {
            _elementService = MatchBoot.Container.Resolve<IElementFactoryService>();

            _world = systems.GetWorld();
            _filter = _world.Filter<ElementRenderComponent>().Include<HorizontalDotComponent>().End();

            _eliminatePool = _world.GetPool<EliminatedTag>();
            _variableColorPool = _world.GetPool<VariableColorComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                if (!_world.IsEntityAliveInternal(entity))
                    continue;

                // 处理消除标签
                if (_eliminatePool.Has(entity) && _variableColorPool.Has(entity))
                {
                    ref var variableColorCom = ref _variableColorPool.Get(entity);
                    // _eliminatePool.Del(entity);
                    if (variableColorCom.CurrentColorId == 0)
                    {
                        //直接消掉，无效果
                        DelayDestroySelf(entity);
                    }
                    else
                    {
                        // ref var positionCom = ref _positionPool.Get(entity);
                        // 播放直线消除点效果
                        PlayHorizontalDotEffect(entity);
                    }
                }
            }
        }

        /// <summary>
        /// 播放直线消除点效果
        /// </summary>
        private void PlayHorizontalDotEffect(int entity)
        {
            // ref var renderComponent = ref _renderPool.Get(entity);

            // 可以在这里添加横向扫描效果、粒子效果、音效等

            // 延迟销毁自身
            DelayDestroySelf(entity);
        }

        private void DelayDestroySelf(int entity)
        {
            // 延迟销毁，给动画播放时间
            _elementService.AddDestroyElementTag2Entity(_world, entity);
        }
    }
}