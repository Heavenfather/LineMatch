using System.Collections.Generic;
using System.Linq;
using GameCore.Singleton;
using UnityEngine;

namespace HotfixTools.DynamicAtlas
{
    public class DynamicAtlasManager : MonoSingleton<DynamicAtlasManager>
    {
        [Header("基础设置")] [SerializeField] private int _atlasSize = 2048; // 图集尺寸
        [SerializeField] private TextureFormat _textureFormat = TextureFormat.RGBA32;
        [SerializeField] private int _padding = 2; // 元素间距

        [SerializeField] private FilterMode _filterMode = FilterMode.Bilinear;
        [SerializeField] private bool _useMipMap = false;

        private class AtlasTextureData
        {
            public Texture2D texture;
            public List<Rect> usedAreas = new List<Rect>();
            public int currentYOffset;
        }

        private List<AtlasTextureData> _atlases = new List<AtlasTextureData>();

        private Dictionary<Texture2D, (AtlasTextureData atlas, Rect uvRect)> _textureMapping =
            new Dictionary<Texture2D, (AtlasTextureData, Rect)>();

        // 动态添加纹理到图集
        public (Texture2D atlasTexture, Vector2[] uv) AddTexture(Texture2D source)
        {
            if (_textureMapping.TryGetValue(source, out var existingData))
                return (existingData.atlas.texture, RectToUV(existingData.uvRect));

            foreach (var atlasData in _atlases)
            {
                if (TryPackTexture(atlasData, source, out var rect))
                {
                    UpdateAtlasTexture(atlasData, source, rect);
                    return (atlasData.texture, RectToUV(rect));
                }
            }

            // 创建新图集
            var newAtlas = CreateNewAtlas();
            if (!TryPackTexture(newAtlas, source, out var newRect))
            {
                Debug.LogError($"Texture {source.name} is too large for atlas!");
                return (null, null);
            }

            UpdateAtlasTexture(newAtlas, source, newRect);
            return (newAtlas.texture, RectToUV(newRect));
        }

        private bool TryPackTexture(AtlasTextureData atlas, Texture2D tex, out Rect rect)
        {
            int requiredWidth = tex.width + _padding * 2;
            int requiredHeight = tex.height + _padding * 2;
            rect = default;
            
            // 当前行剩余空间检测
            if (atlas.currentYOffset + requiredHeight > _atlasSize)
            {
                return false;
            }

            // 横向空间检测
            if (requiredWidth > _atlasSize)
                return false;

            rect = new Rect(
                x: atlas.usedAreas.Count > 0 ? atlas.usedAreas.Last().xMax + _padding : _padding,
                y: atlas.currentYOffset + _padding,
                width: tex.width,
                height: tex.height
            );

            atlas.usedAreas.Add(rect);
            atlas.currentYOffset += requiredHeight;
            return true;
        }

        private void UpdateAtlasTexture(AtlasTextureData atlas, Texture2D source, Rect targetRect)
        {
            // 复制像素数据
            Color[] pixels = source.GetPixels();
            atlas.texture.SetPixels(
                (int)targetRect.x,
                (int)targetRect.y,
                source.width,
                source.height,
                pixels
            );
            atlas.texture.Apply();

            // 建立映射关系
            _textureMapping.Add(source, (atlas, targetRect));
        }

        #region 工具方法

        private AtlasTextureData CreateNewAtlas()
        {
            var newAtlas = new AtlasTextureData
            {
                texture = new Texture2D(_atlasSize, _atlasSize, _textureFormat, _useMipMap)
                {
                    filterMode = _filterMode,
                    wrapMode = TextureWrapMode.Clamp
                }
            };

            _atlases.Add(newAtlas);
            return newAtlas;
        }

        private Vector2[] RectToUV(Rect rect)
        {
            return new Vector2[]
            {
                new Vector2(rect.xMin / _atlasSize, rect.yMin / _atlasSize),
                new Vector2(rect.xMax / _atlasSize, rect.yMin / _atlasSize),
                new Vector2(rect.xMax / _atlasSize, rect.yMax / _atlasSize),
                new Vector2(rect.xMin / _atlasSize, rect.yMax / _atlasSize)
            };
        }

        #endregion

        #region 调试工具

        public void PrintAtlasInfo()
        {
            Debug.Log($"当前图集数量: {_atlases.Count}");
            foreach (var atlas in _atlases)
            {
                Debug.Log($"图集尺寸: {atlas.texture.width}x{atlas.texture.height} 已使用高度: {atlas.currentYOffset}");
            }
        }

        public Texture2D GetDebugAtlas(int index)
        {
            if (index < 0 || index >= _atlases.Count) return null;
            return _atlases[index].texture;
        }

        #endregion

        #region 内存管理

        public void UnloadUnusedTextures()
        {
            var usedTextures = new HashSet<Texture2D>(_textureMapping.Keys);
            var texturesToRemove = new List<Texture2D>();

            foreach (var kvp in _textureMapping)
            {
                if (!IsTextureInUse(kvp.Key))
                    texturesToRemove.Add(kvp.Key);
            }

            foreach (var tex in texturesToRemove)
            {
                _textureMapping.Remove(tex);
            }

            // 重建有空余的图集
            RebuildAtlases();
        }

        private bool IsTextureInUse(Texture2D texture)
        {
            // 此处需要根据项目具体实现引用检测
            return true;
        }

        private void RebuildAtlases()
        {
            // 重建算法（略）
        }

        #endregion
    }
}