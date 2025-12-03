#if UNITY_EDITOR
using System;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using HotfixCore.Module;
using HotfixLogic.Match;

namespace HotfixLogic
{
	public partial class EditDropFlagCell : UIWidget
	{
		private int _index;
		private int _id;
		private int _rate;
		private int _limit;
		private Action<int> _delClickCallback;
		private Action<DropElementFlagConfig> _editChangedCallback;
		
		public override void OnCreate()
		{
			btn_del.AddClick(() =>
			{
				_delClickCallback?.Invoke(_index);
			});
			
			input_id.AddEndEditListener((v) =>
			{
				if (int.TryParse(v, out _id))
				{
					DispatchEditChanged();
				}
			});
			
			input_limit.AddEndEditListener((v) =>
			{
				if (int.TryParse(v, out _limit))
				{
					DispatchEditChanged();
				}
			});
			
			input_rate.AddEndEditListener((v) =>
			{
				if (int.TryParse(v, out _rate))
				{
					DispatchEditChanged();
				}
			});
			//概率的暂时不支持
			input_rate.interactable = false;
			input_rate.SetTextWithoutNotify("100");
		}

		public void SetData(int idx,DropFlagElement element,Action<int> delCallback,Action<DropElementFlagConfig> editChangedCallback)
		{
			_index = idx;
			_id = element.elementId;
			_rate = element.dropRate;
			_limit = element.dropLimitMax;
			_delClickCallback = delCallback;
			_editChangedCallback = editChangedCallback;
			InitInputValue();
		}
		
		private void InitInputValue()
		{
			if (_id > 0)
			{
				input_id.SetTextWithoutNotify(_id.ToString());
			}
			if (_rate > 0)
			{
				input_rate.SetTextWithoutNotify($"{_rate / 100}");
			}
			if (_limit > 0)
			{
				input_limit.SetTextWithoutNotify(_limit.ToString());
			}
		}

		private void DispatchEditChanged()
		{
			DropElementFlagConfig config = new DropElementFlagConfig()
			{
				Index = _index,
				ElementId = _id,
				Rate = (_rate / 100) * 10000,
				Limit = _limit
			};
			_editChangedCallback?.Invoke(config);
		}
	}
}
#endif