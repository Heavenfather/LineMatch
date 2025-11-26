using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HotfixCore.Module;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;
using Logger = GameCore.Log.Logger;
using Object = UnityEngine.Object;

namespace HotfixCore.Extensions
{
    [RequireComponent(typeof(TMP_Text))]
    public class TMPInlineImageTag : MonoBehaviour
    {
        // 图片信息结构
        private struct ImageSegment
        {
            public int charIndex;
            public string spritePath;
            public Vector2 size;
            public float yOffset;
            public float xOffset;
            public Color tint;
        }

        private TMP_Text _textComponent;
        private const string _imgTag = "<img";
        private const string _imgEndTag = "/>";
        private Regex _imgRegex;
        private bool _isUpdating = false;
        private Coroutine _updateCoroutine;
        private bool _isInternalTextUpdate = false;

        // 图片对象池
        private readonly Queue<GameObject> _imagePool = new Queue<GameObject>();
        private readonly List<GameObject> _activeImages = new List<GameObject>();

        private string _processText = string.Empty;

#if UNITY_EDITOR
        private string _testOriText = string.Empty;
#endif

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
            // 图片标签正则: <img src="path" width=50 height=50 yOffset=0 tint="#FFFFFF">
            string imgPattern =
                $@"{Regex.Escape(_imgTag)}\s+src=""([^""]+)""\s*(?:width=""([0-9.]+)""\s*)?(?:height=""([0-9.]+)""\s*)?(?:yOffset=""([-0-9.]+)""\s*)?(?:xOffset=""([-0-9.]+)""\s*)?(?:space=""([-0-9.]+)""\s*)?(?:tint=""#?([0-9a-fA-F]{{6,8}})""\s*)?{Regex.Escape(_imgEndTag)}";
            _imgRegex = new Regex(imgPattern, RegexOptions.Singleline);
        }


        private void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);

            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }

            ClearAllElements();
        }

        private void OnTextChanged(Object obj)
        {
            if (obj == _textComponent && !_isUpdating && !_isInternalTextUpdate)
            {
                _processText = _textComponent.text;
#if UNITY_EDITOR
                _testOriText = _textComponent.text;
#endif
                UpdateImage();
            }
        }
#if UNITY_EDITOR
        [Button("重刷文本")]
        public void RebuildText()
        {
            _processText = _testOriText;
            UpdateImage();
        }
#endif
        
        public void UpdateImage()
        {
            if (_isUpdating) return;

            _isUpdating = true;

            // 使用协程避免在渲染循环中操作
            if (_updateCoroutine != null) StopCoroutine(_updateCoroutine);
            _updateCoroutine = StartCoroutine(UpdateImageRoutine());
        }

        private IEnumerator UpdateImageRoutine()
        {
            // 等待渲染循环结束
            yield return new WaitForEndOfFrame();

            try
            {
                if (string.IsNullOrEmpty(_processText))
                {
                    _isUpdating = false;
                    yield break;
                }

                // 强制更新文本网格
                if (!_textComponent.havePropertiesChanged)
                {
                    _textComponent.ForceMeshUpdate(true);
                }

                ParseAndRemoveTags(out var processedText, out List<ImageSegment> imageSegments);

                // 更新文本内容（移除标签）
                if (!string.Equals(_processText, processedText))
                {
                    _isInternalTextUpdate = true;
                    _textComponent.text = processedText;
                    _processText = "";
                    _textComponent.ForceMeshUpdate(true);
                    _isInternalTextUpdate = false;
                }

                ClearAllElements();
                if (imageSegments.Count > 0)
                {
                    foreach (var segment in imageSegments)
                    {
                        StartCoroutine(CreateImageForSegment(segment));
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.Error($"Error updating inline images: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                _isUpdating = false;
                _updateCoroutine = null;
            }
        }

        /// <summary>
        /// 解析并移除所有标
        /// </summary>
        private void ParseAndRemoveTags(
            out string processedText,
            out List<ImageSegment> imageSegments)
        {
            imageSegments = new List<ImageSegment>();
            StringBuilder sb = new StringBuilder();
            int currentIndex = 0;

            MatchCollection matches = _imgRegex.Matches(_processText);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    sb.Append(_processText.Substring(currentIndex, match.Index - currentIndex));
                    // 图片标签解析
                    string spritePath = match.Groups[1].Value;

                    // 解析尺寸参数（带默认值）
                    float width = 40f, height = 40f, yOffset = 0f, xOffset = 0f;
                    int space = 3;
                    Color tint = Color.white;

                    if (float.TryParse(match.Groups[2].Value, out float w)) width = w;
                    if (float.TryParse(match.Groups[3].Value, out float h)) height = h;
                    if (float.TryParse(match.Groups[4].Value, out float yoff)) yOffset = yoff;
                    if (float.TryParse(match.Groups[5].Value, out float xoff)) xOffset = xoff;
                    if (int.TryParse(match.Groups[6].Value, out int spaceCount)) space = spaceCount;
                    if (ColorUtility.TryParseHtmlString("#" + match.Groups[7].Value, out Color c)) tint = c;

                    sb.Append(' ', space);
                    imageSegments.Add(new ImageSegment
                    {
                        charIndex = sb.Length - 1,
                        spritePath = spritePath,
                        size = new Vector2(width, height),
                        yOffset = yOffset,
                        xOffset = xOffset,
                        tint = tint
                    });

                    currentIndex = match.Index + match.Length;
                }
            }

            // 添加剩余文本
            sb.Append(_processText.Substring(currentIndex));
            processedText = sb.ToString();
        }

        /// <summary>
        /// 为图片占位符创建图片对象（协程异步加载）
        /// </summary>
        private IEnumerator CreateImageForSegment(ImageSegment segment)
        {
            TMP_CharacterInfo charInfo;
            try
            {
                charInfo = _textComponent.textInfo.characterInfo[segment.charIndex];
            }
            catch
            {
                yield break; // 索引越界保护
            }

            // 获取字符中心位置
            Vector3 charCenter = (charInfo.bottomLeft + charInfo.topRight) / 2f;


            G.ResourceModule.LoadAssetAsync<Sprite>(segment.spritePath, sp =>
            {
                if (sp == null)
                {
                    // ReturnImageToPool(imgObj);
                    return;
                }

                // 获取图片对象
                GameObject imgObj = GetImageFromPool();
                RectTransform rt = imgObj.GetComponent<RectTransform>();

                // 设置初始位置
                rt.SetParent(_textComponent.transform, false);
                rt.localPosition = new Vector3(
                    charCenter.x + segment.xOffset,
                    charInfo.baseLine + segment.yOffset,
                    0
                );
                rt.sizeDelta = segment.size;
                Image img = imgObj.GetComponent<Image>();
                img.color = segment.tint;
                img.sprite = sp;
                imgObj.SetActive(true);
                _activeImages.Add(imgObj);
            }).Forget();
        }

        /// <summary>
        /// 从对象池获取图片对象
        /// </summary>
        private GameObject GetImageFromPool()
        {
            while (_imagePool.Count > 0 && _imagePool.Peek() == null)
            {
                _imagePool.Dequeue();
            }

            if (_imagePool.Count > 0)
            {
                return _imagePool.Dequeue();
            }

            // 创建新图片对象
            GameObject imgObj = new GameObject("InlineImage", typeof(RectTransform), typeof(Image));
            RectTransform rt = imgObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            Image img = imgObj.GetComponent<Image>();
            img.raycastTarget = false;
            imgObj.SetActive(false);

            return imgObj;
        }

        private void ClearAllElements()
        {
            // 清理图片
            foreach (var img in _activeImages)
            {
                if (img != null) ReturnImageToPool(img);
            }

            _activeImages.Clear();
        }

        /// <summary>
        /// 图片对象回池
        /// </summary>
        private void ReturnImageToPool(GameObject imgObj)
        {
            if (imgObj == null) return;

            imgObj.SetActive(false);
            _imagePool.Enqueue(imgObj);
        }
    }
}