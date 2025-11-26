using Cysharp.Threading.Tasks;
using GameCore.Logic;

namespace GameCore.Process
{
    public class ProcessStartGame : IProcess
    {
        public void Init()
        {
            
        }

        public void Enter()
        {
            HotUpdateManager.Instance.UpdateProgress(EUpdateState.DoneSuccess);
            StartGame().Forget();
        }

        public void Leave()
        {
        }

        public void Update()
        {
        }
        
        private async UniTaskVoid StartGame()
        {
            await UniTask.Yield();
            //热更流程结束
            // HotUpdateManager.Instance.Hide(); //关闭UI由热更代码调用
            ProcessManager.Instance.StopProcess();
        }
    }
}