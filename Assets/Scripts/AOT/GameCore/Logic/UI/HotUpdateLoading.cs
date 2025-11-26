using DG.Tweening;
using GameCore.Localization;
using TMPro;
using UnityEngine;

namespace GameCore.Logic
{
    public class HotUpdateLoading : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text_tip;
        [SerializeField] private TextMeshProUGUI text_version;
        [SerializeField] private RectTransform slider_progress;
        [SerializeField] private TextMeshProUGUI slider_progress_text;

        private Tweener _progressTween;
        private Vector2 _progressSize;
        private EUpdateState _lastUpdateState = EUpdateState.Unknown;
        private UpdateStateDesc _lastStateDesc;

        private void Awake()
        {
            _progressSize.x = 0;
            _progressSize.y = slider_progress.sizeDelta.y;
            slider_progress.sizeDelta = _progressSize;
        }

        public void UpdateVersion()
        {
            text_version.text = $"{Application.version}-{HotUpdateManager.Instance.PackageVersion}";
        }

        public void SetProgress(EUpdateState state, UpdateStateDesc desc, float updatePercent = -1)
        {
            if(desc == null)
                return;
            float percent = 0f;
            if (updatePercent < 0)
            {
                if (state == _lastUpdateState)
                    return;
                _lastUpdateState = state;
                percent = _lastStateDesc?.ExpectProgress ?? 0;
                _progressTween?.Kill();
                _progressTween = DOTween
                    .To(() => percent, x => percent = x, desc.ExpectProgress, 0.5f).OnUpdate(
                        () =>
                        {
                            _progressSize.x = Mathf.Max(46, 640f * percent);
                            slider_progress.sizeDelta = _progressSize;
                            slider_progress_text.text = $"{(int)(percent * 100)}%";
                        }).OnComplete(() =>
                    {
                        _lastStateDesc = desc;
                    });
            }
            else
            {
                //资源下载更新进度
                percent = updatePercent;
                _progressSize.x = Mathf.Max(46, 640f * percent);
                slider_progress.sizeDelta = _progressSize;
                slider_progress_text.text = $"{(int)(percent * 100)}%";
            }
            text_tip.text = LocalizationPool.Get(desc.LocalKey);
        }

        private void OnDestroy()
        {
            _progressTween?.Kill();
        }
    }
}