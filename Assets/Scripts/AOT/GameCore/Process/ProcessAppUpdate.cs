using GameCore.Logic;

namespace GameCore.Process
{
    /// <summary>
    /// 检查App版本是否需要更新
    /// </summary>
    public class ProcessAppUpdate : IProcess
    {
        public void Init()
        {
        }

        public void Enter()
        {
            HotUpdateManager.Instance.UpdateProgress(EUpdateState.CheckAppUpdate);
            if (CheckAppUpdate())
            {
                ProcessManager.Instance.Enter<ProcessInitPackage>();
            }
        }

        public void Leave()
        {
        }

        public void Update()
        {
        }

        private bool CheckAppUpdate()
        {
#if UNITY_IOS || UNITY_ANDROID
            //TODO...
            return true;
#else
            return true;
#endif
        }

        private void JumpToUpdate()
        {
        }
    }
}