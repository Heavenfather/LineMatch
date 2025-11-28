using GameConfig;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 掉落收集类的元素
    /// </summary>
    public class DropElementBuilder : IElementBuilder
    {
        public ElementType TargetType => ElementType.Collect;
        
        public void Build(GameStateContext context, int entity, in ElementMap config)
        {
            var dropCom = context.World.GetPool<DropStyleElementComponent>();
            dropCom.Add(entity);

            if (config.elementType == ElementType.JumpCollect)
            {
                var jumpCom = context.World.GetPool<JumpCollectComponent>();
                jumpCom.Add(entity);
            }
        }

        public bool IsElementCanSelected(EcsWorld world, int entity)
        {
            // 掉落收集类的元素，本身不会受任何爆炸和消除影响，只监听棋盘上空位变化
            return false;
        }
    }
}