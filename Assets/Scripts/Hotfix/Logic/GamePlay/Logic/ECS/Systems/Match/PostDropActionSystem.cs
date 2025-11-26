namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 掉落后检测的执行系统，负责棋盘稳定后检测是否有可消除的元素，并执行消除逻辑
    /// </summary>
    public class PostDropActionSystem : IEcsInitSystem, IEcsRunSystem
    {
        public void Init(IEcsSystems systems)
        {
        }

        public void Run(IEcsSystems systems)
        {
        }
    }
}