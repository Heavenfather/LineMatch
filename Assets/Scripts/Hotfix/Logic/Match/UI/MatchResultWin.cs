using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;
using GameConfig;
using GameCore.Localization;
using System.Collections.Generic;
using System.Drawing;
using Cysharp.Threading.Tasks;
using System;
using GameCore.Utils;
using Hotfix.Define;
using Logger = GameCore.Log.Logger;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using Hotfix.Utils;
using DG.Tweening;
using TMPro;
using Spine;
using HotfixCore.MVC;
using System.Linq;

namespace HotfixLogic
{
    [Window(UILayer.UI, "uiprefab/match/matchresultwin")]
    public partial class MatchResultWin : UIWindow
    {
        int _levelID;
        int _score = 888;
        int _starCount = 0;

        int _lastStepCount = 10;
        int _coinNum = 0;

        List<UnityEngine.UI.Image> _starList = new List<UnityEngine.UI.Image>();
        List<GameObject> _goStarEffList = new List<GameObject>();

        const int _titleCount = 8;

        string _matchType = "";

		DG.Tweening.Sequence _shineTween;
        DG.Tweening.Sequence _awaitMultiplyTween;
        DG.Tweening.Sequence _selectMultiplyTween;
        DG.Tweening.Sequence _fingerTween;

        List<TextMeshProUGUI> _multiTextList = new List<TextMeshProUGUI>();

        Animator _animator;

        bool _rotationRight = true;
        float _awaitRotateSpeed = 40f;
        float _selectSpeed = 300f;

        bool _isFirstPlay = true;

        LevelData _levelData;

        List<int> _multipleNums = new List<int>();
        List<int> _multipleProbabity = new List<int>();



        public override void OnCreate()
        {
            // AudioUtil.PlayMatchWin();

            go_winStreak.SetActive(false);

            btn_close.gameObject.SetActive(false);
            btn_close.AddClick(OnClickClose);
            btn_continue.AddClick(OnClickContinue);
            btn_continue1.AddClick(OnClickContinue);
            btn_continue2.AddClick(OnClickContinue);
            btn_adv.AddClick(OnClickAdv);
            // btn_quick.AddClick(PlayResultAnim);

            text_coin.gameObject.SetActive(false);

            _score = MatchManager.Instance.TotalScore;
            _levelData = (LevelData)UserDatas[0];
            _lastStepCount = (int)userDatas[1];
            _isFirstPlay = (bool)userDatas[2];
            _coinNum = (int)userDatas[3];

            _animator = transform.GetComponent<Animator>();

            InitStarList();
            InitTitle();
            SetResultScore(_score);
            InitMultiple();

            SetLevelID(_levelData.id);
            UpdateGoals(_levelData);
            FingerAwait();
        }

        public override void OnDestroy()
        {
            if (_shineTween!= null) {
				_shineTween.Kill();
				_shineTween = null;
			}

            if (_fingerTween != null) {
                _fingerTween.Kill();
				_fingerTween = null;
            }

            KillRotateTween();
        }

        private void KillRotateTween() {
            if (_awaitMultiplyTween!= null) {
                _awaitMultiplyTween.Kill();
                _awaitMultiplyTween = null;
            }

            if (_selectMultiplyTween!= null) {
                _selectMultiplyTween.Kill();
                _selectMultiplyTween = null;
            }
        }
        public void InitMultiple() {
            _multiTextList.Add(text_multi2);
            _multiTextList.Add(text_multi3);
            _multiTextList.Add(text_multi4);
            _multiTextList.Add(text_multi5);


            var multipleCfg = ConfigMemoryPool.Get<ConstConfigDB>().GetConfigStrVal("AdResultMulti");
            var multipleArr = multipleCfg.Split('|');

            var totalWeight = 0;
            for (int i = 0; i < multipleArr.Length; i++){
                var item = multipleArr[i];
                var arr = item.Split('_');
                var multi = int.Parse(arr[0]);
                var weight = int.Parse(arr[1]);

                _multipleNums.Add(multi);
                _multiTextList[i].text = multi / 10f + "倍";

                totalWeight += weight;
                _multipleProbabity.Add(totalWeight);
            }
        }

        private void InitStarList()
        {
            _starList.Add(img_star1);
            _starList.Add(img_star2);
            _starList.Add(img_star3);

            _goStarEffList.Add(go_effStar1);
            _goStarEffList.Add(go_effStar2);
            _goStarEffList.Add(go_effStar3);

            img_star1.gameObject.SetActive(false);
            img_star2.gameObject.SetActive(false);
            img_star3.gameObject.SetActive(false);
        }

        private void InitTitle()
        {
            System.Random random = new System.Random();
            int randomNumber = random.Next(1, _titleCount);
            string key = "Match/Result/Title" + randomNumber.ToString();
            text_title.text = LocalizationPool.Get(key);
        }

        private void InitWinStreak()
        {
            bool isShow = MatchManager.Instance.MaxLevel >=
                          ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("WinStreakLV");
            go_winStreak.gameObject.SetActive(isShow);

            if (!isShow) return;
            var winStreak = CreateWidget<MatchWinStreak>(go_matchWinStreak);
            winStreak.SetBtnDescVisible(false);
        }


        public void UpdateStarCount(int fullScore)
        {
            _starCount = MatchManager.Instance.CalStarCountByScore(_score, fullScore);
            Logger.Debug("star count = " + _starCount + "  _score = " + _score + "  fullScore = " + fullScore);
        }

        public void SetResultScore(int score)
        {
            text_score.text = LocalizationPool.Get("Common/Score") + ":" + score.ToString();

            Logger.Debug("结算分数 _score = " + _score);
        }

        public void SetLevelID(int levelID)
        {
            _levelID = levelID;
        }

        private void UpdateGoals(LevelData levelData)
        {
            CheckShowGoal();

            if (levelData == null)
                return;

            UpdateStarCount(levelData.fullScore);

            text_coin.text = _coinNum.ToString();
            text_coin.gameObject.SetActive(true);

            var maxMultiple = _multipleNums[_multipleNums.Count - 1] / 10f;
            text_advCoin.text = (_coinNum *  maxMultiple).ToString();
            text_maxTIps.text = string.Format(LocalizationPool.Get("Match/Result/AdvTips"), maxMultiple);

            var matchDiff = MatchManager.Instance.GetMatchDifficulty(levelData.difficulty);
            var animName = "idle_lv";
            if (matchDiff == MatchDifficulty.Hard) {
                animName = "idle_zi";
            } else if (matchDiff == MatchDifficulty.Crazy) {
                animName = "idle_h";
            }
            spine_bg.AnimationState.SetAnimation(0, animName, true);

            PlayResultAnim();
        }

        private void OnclickCloseBtn()
        {
            CloseSelf();
            EventDispatcher.DispatchEvent(GameEventDefine.OnMatchCloseClick); 
        }

        private void CheckShowGoal() {
            go_coin.gameObject.SetActive(_isFirstPlay);
            go_continue.gameObject.SetActive(!_isFirstPlay);
        }

        private bool CheckMustNextLevel() {
            return MatchManager.Instance.MaxLevel <= ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("ContinueLevel");
        }

        private void OnClickClose() {
            CheckClose(false);
        }

        private void CheckClose(bool isContinue) {
            if (MatchManager.Instance.IsEnterByEditor())
            {
                OnclickCloseBtn();
                return;
            }

            if (CheckMustNextLevel() && CommonUtil.CanBeginGame()) {
                ReqBeginNextLevel();
            } else {
                if (isContinue) {
                    G.EventModule.DispatchEvent(GameEventDefine.OnMatchContinueBack);
                }
                OnclickCloseBtn();
            }
        }

        private void OnClickContinue() {
            CheckClose(true);
        }

        private void ReqBeginNextLevel() {
			G.HttpModule.ReportLevelGameBegin(MatchManager.Instance.CurLevelID, (result, code) =>
			{
				Debug.Log("ReportLevelGameBegin result:" + result + " code:" + code);
				if (code == 0) {
                    // 在游戏中继续下一关，在loading中关闭界面，在重新进入刷新数据
                    CommonLoading.ShowLoading(LoadingEnum.Match, 0f);
                    EventDispatcher.DispatchEvent(GameEventDefine.OnMatchCloseContinue); 
				} else {
					Debug.LogError("ReportLevelGameBegin failed, code:" + code);
				}
			});
        }

        
        private void DelayShowCloseBtn() {
            _shineTween = DOTween.Sequence();
            _shineTween.AppendInterval(0.5f);
            _shineTween.AppendCallback(() => {
                _shineTween = null;
                btn_close.gameObject.SetActive(true);
                ShineAnim();
            });
        }

		private void ShineAnim() {
            text_close.alpha = 0f;
            _shineTween = CommonUtil.TextShine(text_close);

            if (CheckMustNextLevel()) {
                text_close.text = LocalizationPool.Get("Match/Result/TouchContinue");
            }
		}

        private void OnClickAdv() {
            G.AdvModule.PlayAdvVideo(GiftIDDefine.Adv_MatchWinGoldDouble, succ => {
                if (succ) {
                    GetAdvCoin();
                }
            }, false, _levelData.id);
        }

        private void GetAdvCoin() {
            var weightRange = _multipleProbabity[_multipleProbabity.Count - 1];
            int randomNumber = UnityEngine.Random.Range(0, weightRange);

            for (int i = 0; i < _multipleProbabity.Count; i++) {
                if (randomNumber < _multipleProbabity[i]) {
                    int multiple = _multipleNums[i];
                    var curCoinNum = int.Parse(text_coin.text);
                    G.HttpModule.GameEndCoinMulti(curCoinNum, multiple, (succ) => {
                        if (succ) {
                            btn_close.interactable = false;
                            PlaySelectAnim(i, multiple);

                            float mul = (float)multiple / 10;
                            int addCoin = (int)(curCoinNum * mul) - curCoinNum;
                            var param = EventTwoParam<int, int>.Create(addCoin, 0);
                            G.EventModule.DispatchEvent(GameEventDefine.OnMatchResultAddCoinAndStar, param);
                        }
                    });
                    break;
                }
            }
        }

        private void PlayAwaitAnim() {
            var time = 160f / _awaitRotateSpeed;

            _awaitMultiplyTween = DOTween.Sequence();
            _awaitMultiplyTween.AppendCallback(() => { _rotationRight = true; });
            _awaitMultiplyTween.Append(go_multiple.transform.DOLocalRotate(new Vector3(0, 0, 10), time));
            _awaitMultiplyTween.AppendCallback(() => { _rotationRight = false; });
            _awaitMultiplyTween.Append(go_multiple.transform.DOLocalRotate(new Vector3(0, 0, 170), time));
            _awaitMultiplyTween.SetLoops(-1);
        }

        private void PlaySelectAnim(int multipleIdx, int multiple) {
            var multipleAngle = new List<int>();
            multipleAngle.Add(137);
            multipleAngle.Add(90);
            multipleAngle.Add(45);
            multipleAngle.Add(10);

            KillRotateTween();

            btn_adv.gameObject.SetActive(false);
            btn_continue.gameObject.SetActive(false);

            var curDir = _rotationRight;
            var targetRotation = curDir ? 10 : 170;
            var time = Math.Abs(go_multiple.transform.localRotation.z - targetRotation) / _selectSpeed;

            _selectMultiplyTween = DOTween.Sequence();
            _selectMultiplyTween.Append(go_multiple.transform.DOLocalRotate(new Vector3(0, 0, targetRotation), time).SetEase(Ease.Linear));
            _selectMultiplyTween.AppendCallback(() => { _rotationRight = !_rotationRight; });
            curDir = !curDir;

            for (int i = 0; i < 5; i++) {
                targetRotation = curDir ? 10 : 170;
                _selectMultiplyTween.Append(go_multiple.transform.DOLocalRotate(new Vector3(0, 0, targetRotation), 160f / _selectSpeed).SetEase(Ease.Linear));
                _selectMultiplyTween.AppendCallback(() => { _rotationRight = !_rotationRight; });
                curDir = !curDir;
            }

            go_jinbiEff.SetActive(false);

            var endRotation = multipleAngle[multipleIdx];
            var endTime = (endRotation - targetRotation) / _selectSpeed;
            _selectMultiplyTween.Append(go_multiple.transform.DOLocalRotate(new Vector3(0, 0, endRotation), endTime).SetEase(Ease.Linear));
            _selectMultiplyTween.AppendCallback(() => { 
                go_multiple.transform.eulerAngles = new Vector3(0, 0, endRotation);


                var curCoinNum = int.Parse(text_coin.text);
                var targetNum = curCoinNum * multiple / 10;

                text_coin.transform.DOScale(1.3f, 0.2f);

                go_jinbiEff.SetActive(true);
                
                DOTween.To(() => curCoinNum, x => curCoinNum = x, targetNum, 0.7f)
                            .OnUpdate(() => {
                                text_coin.text = curCoinNum.ToString();
                            }).OnComplete(() => {
                                btn_close.interactable = true;

                                btn_continue2.gameObject.SetActive(true);
                                btn_continue2.transform.localScale = Vector3.zero;
                                btn_continue2.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);

                                text_coin.transform.DOScale(1f, 0.1f);
                            }); // 更新文本
            });
        }

        private void PlayResultAnim() {
            btn_close.interactable = false;
            btn_continue.interactable = false;
            btn_continue1.interactable = false;
            btn_continue2.interactable = false;
            btn_adv.interactable =  false;


            _animator.Play("ani_yr_00005_xx");


            var seq = DOTween.Sequence();
            var beginTime = 0.1f;
            var disTime = 0.1f;
            for (int i = 0; i < _starCount; i++) {
                var idx = i;
                var star = _starList[idx];
                var goEff = _goStarEffList[idx];

                seq.AppendInterval(beginTime + idx * disTime);
                
                seq.AppendCallback(() => {
                    AudioUtil.PlaySound("audio/match/match_win_star");
                    star.gameObject.SetActive(true);
                    goEff.SetActive(true);

                    if (idx == _starCount - 1) {
                        PlayAnimFinish();
                        InitWinStreak();
                    }
                });
            }

            if (_starCount == 0) {
                PlayAnimFinish();
                InitWinStreak();
            }
        }

        private void PlayAnimFinish() {
            bool hasTrainMasterChange = G.TrainMasterModule.InTrainning() && G.TrainMasterModule.HasLvChange();
            var delayTime = hasTrainMasterChange ? 1f : 0.1f;

            var seq = DOTween.Sequence();
            seq.AppendInterval(delayTime);
            seq.AppendCallback(() => {
                DelayShowCloseBtn();

                PlayAwaitAnim();

                if (hasTrainMasterChange) {
                    MVCManager.Instance.ActiveModule(MVCEnum.TrainMaster.ToString()).Forget();
                } else {
                    btn_close.interactable = true;
                    btn_continue.interactable = true;
                    btn_continue1.interactable = true;
                    btn_continue2.interactable = true;
                    btn_adv.interactable =  true;
                }
            });

            if (hasTrainMasterChange) {
                seq.AppendInterval(0.3f);
                seq.AppendCallback(() => {
                    btn_close.interactable = true;
                    btn_continue.interactable = true;
                    btn_continue1.interactable = true;
                    btn_continue2.interactable = true;
                    btn_adv.interactable =  true;
                });
            }
        }

		private void FingerAwait() {
			spine_touch.color = new UnityEngine.Color(1, 1, 1, 0);
			spine_touch.gameObject.SetActive(true);

			var seq = DOTween.Sequence();
			seq.AppendInterval(5);
			seq.AppendCallback(() => {
				ShowFingerTouch();
				_fingerTween = null;
			});

			_fingerTween = seq;
		}

		private void ShowFingerTouch() {
			var seq = DOTween.Sequence();
			seq.Append(spine_touch.DOFade(1, 0.5f));
			seq.AppendCallback(() => {
				spine_touch.AnimationState.SetAnimation(0, "dianji", false);
			});
			seq.AppendInterval(1f);
			seq.Append(spine_touch.DOFade(0, 0.5f));
			seq.AppendInterval(1f);
			seq.AppendCallback(() => {
				spine_touch.gameObject.SetActive(true);
			});
			seq.SetLoops(-1);

			_fingerTween = seq;
		}
    }
}