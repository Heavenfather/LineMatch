using System;
using HotfixCore.MVC;

namespace HotfixLogic
{
	[MVCDefine("Guide")]
	public class GuideController : BaseController
	{
		public override Type MainView { get; } = typeof(GuideMainWindow);
	}
}
