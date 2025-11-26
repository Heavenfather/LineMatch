using UnityEngine;
using HotfixCore.Module;
using GameConfig;
using System.Collections.Generic;
using System.Linq;

namespace HotfixLogic
{
	public partial class MatchBoosterSelect : UIWidget
	{

		List<MatchBeginBoosterItem> _boosterItemList = new List<MatchBeginBoosterItem>();

		public override void OnCreate()
		{
			_boosterItemList.Add(widget_boosterItem1);
			_boosterItemList.Add(widget_boosterItem2);
			_boosterItemList.Add(widget_boosterItem3);

			InitBoosterItem();
		}

		private void InitBoosterItem() {
			var constConfig = ConfigMemoryPool.Get<ConstConfigDB>();
			var itemCfg = constConfig.GetConfigStrVal("MatchBeginItem");
			var itemID = itemCfg.Split('|');
			for (int i = 0; i < itemID.Length; i++) {
				var id = itemID[i];
				_boosterItemList[i].SetBoosterID(id);
			}
		}

		public void SetBoosterUseBuff(bool bl) {
			foreach (var item in _boosterItemList) {
				item.SetUseBuff(bl);
			}
		}

		public List<string> GetSelectBoosterID() {
			List<string> selectID = new List<string>();
			foreach (var item in _boosterItemList) {
				if (item.IsUse()) {
					selectID.Add(item.GetBoosterID());
				}
			}
			return selectID;
		}

		public void ShowBoosterTips() {
			for (int i = 0; i < _boosterItemList.Count; i++) {
				_boosterItemList[i].SetBtnEffectShow(i * 0.53f);
			}
		}
	}
}
