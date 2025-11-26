using UnityEngine;

namespace HotfixTools.DynamicAtlas
{
    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteAlways]
    public class DynamicSprite : MonoBehaviour
    {
        [SerializeField] private Texture2D _sourceTexture;

        private MaterialPropertyBlock _propBlock;
        private SpriteRenderer _renderer;
        private Vector2[] _currentUVs;


        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _propBlock = new MaterialPropertyBlock();
            UpdateSpriteData();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            UpdateSpriteData();
        }

        private void Update()
        {
            if (_renderer.sprite == null)
                UpdateSpriteData();
        }

        private void UpdateSpriteData()
        {
            if (_sourceTexture == null) return;

            (Texture2D atlasTexture, Vector2[] uv) atlasData = DynamicAtlasManager.Instance.AddTexture(_sourceTexture);
            if (atlasData.atlasTexture == null) return;

            // 创建伪Sprite
            // var sprite = Sprite.Create(
            //     atlasData.atlasTexture,
            //     new Rect(0, 0, atlasData.atlasTexture.width, atlasData.atlasTexture.height),
            //     new Vector2(0.5f, 0.5f)
            // );

            // 设置材质属性
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetTexture("_MainTex", atlasData.atlasTexture);
            Vector4[] uvs = new Vector4[4];
            uvs[0] = new Vector4(atlasData.uv[0].x, atlasData.uv[0].y, 0, 0);
            uvs[1] = new Vector4(atlasData.uv[1].x, atlasData.uv[1].y, 0, 0);
            uvs[2] = new Vector4(atlasData.uv[2].x, atlasData.uv[2].y, 0, 0);
            uvs[3] = new Vector4(atlasData.uv[3].x, atlasData.uv[3].y, 0, 0);
            _propBlock.SetVectorArray("_CustomUVs", uvs);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}