using System;
using DG.Tweening;
using UnityEngine;

namespace GameCore.Logic
{
    public class AssetLoadingWait : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed = 180; //每秒旋转度数
        [SerializeField] private RectTransform rotate;

        private Tweener tweener;

        private void OnEnable()
        {
            float duration = Mathf.Abs(360.0f / rotateSpeed);
            tweener = rotate.DORotate(new Vector3(0, 0, 360 * Mathf.Sign(rotateSpeed)), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(UpdateType.Normal, true);
        }

        private void OnDisable()
        {
            tweener?.Kill();
        }

        public void SetVisible(bool visible)
        {
            if (this.gameObject.activeSelf != visible)
                this.gameObject.SetActive(visible);
        }
    }
}