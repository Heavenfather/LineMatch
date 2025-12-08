using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// TowDots模式下的爆炸点系统，负责处理爆炸点的表现效果
    /// 特性：
    /// 1. 初始为白色，可与任意Normal棋子相连
    /// 2. 连接后变成对应颜色，断开连接后恢复白色
    /// 3. 消除效果：以最后连接点为中心的3x3范围爆炸
    /// </summary>
    public class TowDotsBombDotSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<EliminatedTag> _eliminatePool;
        private IElementFactoryService _elementService;
        private EcsPool<VariableColorComponent> _variableColorPool;

        public void Init(IEcsSystems systems)
        {
            _elementService = MatchBoot.Container.Resolve<IElementFactoryService>();

            _world = systems.GetWorld();
            _filter = _world.Filter<ElementRenderComponent>().Include<TowDotsBombDotComponent>().End();

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
                if (_eliminatePool.Has(entity))
                {
                    // _eliminatePool.Del(entity);

                    ref var variableColorCom = ref _variableColorPool.Get(entity);
                    if (variableColorCom.CurrentColorId == 0)
                    {
                        //直接消掉，无效果
                        DelayDestroySelf(entity);
                    }
                    else
                    {
                        // ref var positionCom = ref _positionPool.Get(entity);
                        // 播放直线消除点效果
                        PlayBombDotEffect(entity);
                    }
                }
            }
        }

        /// <summary>
        /// 播放爆炸点效果
        /// </summary>
        private void PlayBombDotEffect(int entity)
        {
            // 可以在这里添加爆炸效果、粒子效果、音效等
            
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
