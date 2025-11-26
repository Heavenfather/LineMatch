using UnityEngine;
using HotfixCore.Module;
using Cysharp.Threading.Tasks;
using HotfixLogic.Match;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using GameConfig;

namespace HotfixLogic
{
	public partial class MatchBeginTargetWidget : UIWidget
	{
		List<Image> _imgList = new List<Image>();
		List<TextMeshProUGUI> _textList = new List<TextMeshProUGUI>();
		List<Image> _finishImgList = new List<Image>();
		List<Image> _errorImgList = new List<Image>();

		LevelData _levelData;

		public override void OnCreate()
		{
			_imgList.Add(img_icon1);
			_imgList.Add(img_icon2);
			_imgList.Add(img_icon3);
			_imgList.Add(img_icon4);

			_finishImgList.Add(img_finish1);
			_finishImgList.Add(img_finish2);
			_finishImgList.Add(img_finish3);
			_finishImgList.Add(img_finish4);

			_errorImgList.Add(img_error1);
			_errorImgList.Add(img_error2);
			_errorImgList.Add(img_error3);
			_errorImgList.Add(img_error4);

			_textList.Add(text_count1);
			_textList.Add(text_count2);
			_textList.Add(text_count3);
			_textList.Add(text_count4);
		}
		
		public void InitTarget(LevelData levelData, Dictionary<int, int> targetData = null)
		{
			_levelData = levelData;

			var targetIDList = new List<TargetElement>();
			for (int i = 0; i < levelData.target.Length; i++) {
				targetIDList.Add(levelData.target[i]);
			}

			for (int i = 0; i < _imgList.Count; i++) {
				_imgList[i].gameObject.SetActive(false);
				_textList[i].gameObject.SetActive(false);
			}


			for (int i = 0; i < targetIDList.Count; i++) {
				var img = _imgList[i];
				var text = _textList[i];

				img.gameObject.SetActive(true);
				if (targetData == null) {
					text.gameObject.SetActive(true);
					text.text = targetIDList[i].targetNum.ToString();
				} else {
					if (targetData.ContainsKey(targetIDList[i].targetId)) {
						_errorImgList[i].gameObject.SetActive(targetData[targetIDList[i].targetId] > 0);
						_finishImgList[i].gameObject.SetActive(targetData[targetIDList[i].targetId] <= 0);
					} else {
						_errorImgList[i].gameObject.SetActive(true);
						_finishImgList[i].gameObject.SetActive(false);
					}


				}

				SetTargetImg(img, targetIDList[i].targetId);
			}
		}

		public void SetTargetImg(Image icon, int targetID) {
			MatchManager.Instance.GetMatchElementSprite(targetID, (sp) => {
				if(this.gameObject == null || this.gameObject.transform == null)
					return;
				icon.sprite = sp;
				ElementMapDB elementDB = ConfigMemoryPool.Get<ElementMapDB>();
				ref readonly ElementMap config = ref elementDB[targetID];
				if (config.elementType == ElementType.Normal)
				{
					icon.color = ConfigMemoryPool.Get<LevelMapImageDB>()
						.GetBaseElementColor(_levelData.id, _levelData.id, targetID);
				}
				else
				{
					icon.color = Color.white;
				}
			});
		}
	}
}
