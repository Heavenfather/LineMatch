#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;
using UnityEngine.UI;

namespace HotfixLogic
{
	public partial class widget_targetCell : UIWidget
	{
		private TargetElement _target;
		private int _index;
		
		public override void OnCreate()
		{
			btn_del.AddClick(() =>
			{
				DispatchEvent(MatchEditorConst.OnDelTargetElement);
			});
			Button iconBtn = img_icon.GetComponent<Button>();
			iconBtn.AddClick(() =>
			{
				DispatchEvent(MatchEditorConst.OnTargetElementIconClick);
			});
			
			input_num.AddEndEditListener(OnTargetNumChanged);
		}

		public void SetView(TargetElement target,int index)
		{
			_target = target;
			_index = index;
			input_num.SetTextWithoutNotify(target.targetNum.ToString());
			UpdateIcon(target.targetId).Forget();
		}

		private async UniTask UpdateIcon(int id)
		{
			img_icon.sprite = await MatchEditorUtils.GetElementIcon(id);
			var db = ConfigMemoryPool.Get<ElementMapDB>();
			ElementMap config = db[id];
			if (config.elementType == ElementType.Normal)
			{
				img_icon.color = ElementSystem.Instance.GetElementColor(id);
			}
			else
			{
				img_icon.color = Color.white;
			}
		}

		private void OnTargetNumChanged(string value)
		{
			if (int.TryParse(value, out int num))
			{
				_target.targetNum = num;
				DispatchEvent(MatchEditorConst.OnTargetElementChanged);
			}
			
		}
		
		private void DispatchEvent(int eventId)
		{
			G.EventModule.DispatchEvent(eventId,EventTwoParam<int,TargetElement>.Create(_index,_target));
		}
	}
}
#endif