using System;
using UnityEngine;

namespace HotfixCore.Adapter
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Background2DScaler : MonoBehaviour
    {
        private Camera _mainCamera;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _mainCamera = Camera.main;
            
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void ScaleBackground(Camera adapterCamera = null)
        {
            if (adapterCamera != null)
                _mainCamera = adapterCamera;
            // 获取屏幕分辨率
            float screenHeight = _mainCamera.orthographicSize * 2;
            float screenWidth = screenHeight * _mainCamera.aspect;

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if(_spriteRenderer == null)
                return;
            // 获取Sprite原始尺寸
            Sprite sprite = _spriteRenderer.sprite;
            float spriteWidth = sprite.bounds.size.x;
            float spriteHeight = sprite.bounds.size.y;

            // 计算缩放比例
            float scaleX = screenWidth / spriteWidth;
            float scaleY = screenHeight / spriteHeight;
            float finalScale = Mathf.Max(scaleX, scaleY);

            transform.localScale = new Vector3(finalScale, finalScale, 1);
            transform.position = new Vector3(_mainCamera.transform.position.x, _mainCamera.transform.position.y, 0);
        }
    }
}