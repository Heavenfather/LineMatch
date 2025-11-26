using Cysharp.Threading.Tasks;
using Hotfix.Define;
using HotfixCore.MVC;

namespace Hotfix.Logic
{
    public partial class GameLaunch
    {
        /// <summary>
        /// 登录
        /// </summary>
        public async UniTask OpenLoginModule()
        {
            await MVCManager.Instance.ActiveModule(MVCEnum.Login.ToString());
        }
    }
}