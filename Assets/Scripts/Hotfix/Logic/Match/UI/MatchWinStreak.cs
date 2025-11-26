using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;
using DG.Tweening;
using HotfixCore.Extensions;

namespace HotfixLogic
{
	public partial class MatchWinStreak : UIWidget
	{
		Tween _tween;

		public override void OnCreate()
		{
			(btn_descClose.transform as RectTransform).sizeDelta = new Vector2(Screen.width * 2, Screen.height * 2);

			btn_winStreakDesc.AddClick(OnClickWinStreakDesc);
			btn_descClose.AddClick(OnClickWinStreakDesc);

			UpdateWinStreakProgress();
		}

        public override void OnDestroy()
        {
            KillTween();
        }

		private void UpdateWinStreakProgress()
		{
			var curStreak = MatchManager.Instance.GetWinStreakBox();
			
			text_winStreakPro.text = $"{curStreak}/3";
			fillImg_winStreakPro.fillAmount = (float)curStreak / 3;
		}

		private void KillTween() {
			if (_tween!= null) {
				_tween.Kill();
				_tween = null;
			}
		}

		private void OnClickWinStreakDesc() {
			if (_tween!= null) return;

			bool descIsShow = img_arrow.gameObject.activeSelf;
			btn_descClose.gameObject.SetActive(!descIsShow);

			int beginScale = descIsShow ? 1 : 0;
			img_arrow.transform.localScale = new Vector3(beginScale, beginScale, beginScale);

			int toScale = descIsShow ? 0 : 1;
			img_arrow.gameObject.SetActive(true);
			_tween = img_arrow.transform.DOScale(new Vector3(toScale, toScale, toScale), 0.3f).OnComplete(() => {
				img_arrow.gameObject.SetActive(toScale == 1);
				_tween = null;

				btn_descClose.gameObject.SetActive(toScale == 1);
			});

			if (toScale == 1) _tween.SetEase(Ease.OutBack);
			
		}

		public void SetBtnDescVisible(bool visible) {
			btn_winStreakDesc.gameObject.SetActive(visible);
		}
	}
}
