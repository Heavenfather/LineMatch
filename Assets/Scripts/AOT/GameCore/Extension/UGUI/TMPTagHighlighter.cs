using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;
using System.Text;

/// <summary>
/// 文本高亮标签
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TMPTagHighlighter : MonoBehaviour
{
    [System.Serializable]
    public class HighlightPreset
    {
        [Tooltip("预设名称（用于标签引用）")] public string presetName = "default";

        [Tooltip("背景颜色")] public Color backgroundColor = new Color(1, 0.8f, 0.4f, 0.5f);

        [Tooltip("背景图片（可选）")] public Sprite backgroundSprite;

        [Tooltip("背景边距（左，上，右，下）")] public Vector4 padding = new Vector4(2, 1, 2, 1);

        [Tooltip("是否使用拉伸模式")] public bool useStretch = true;

        [Tooltip("Y轴偏移（调整垂直位置）")] public float yOffset = 0f;
    }

    //开始标签格式，例如：<highlight preset="default">
    private const string _startTag = "<highlight";

    //结束标签格式
    private const string _endTag = "</highlight>";

    [Header("预设配置")] public List<HighlightPreset> highlightPresets = new List<HighlightPreset>()
    {
        new HighlightPreset() { presetName = "default" }
    };

    private TMP_Text _textComponent;
    private float _lastCheckTime;
    private readonly List<GameObject> _activeHighlights = new List<GameObject>();
    private bool _isUpdating = false;
    private bool _isInternalTextUpdate = false; // 新增：标记内部文本更新

    // 对象池
    private readonly Queue<GameObject> _highlightPool = new Queue<GameObject>();

    // 标签解析正则表达式
    private Regex _tagRegex;

    private Coroutine _updateCoroutine;

    // 新增：存储标签位置信息
    private struct HighlightSegment
    {
        public int startIndex;
        public int endIndex;
        public HighlightPreset preset;
    }

    private void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();

        string pattern = $"{Regex.Escape(_startTag)}(.*?)>(.*?){Regex.Escape(_endTag)}";
        _tagRegex = new Regex(pattern, RegexOptions.Singleline);
    }

    private void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        StartCoroutine(DelayedUpdate(0.1f));
    }

    private IEnumerator DelayedUpdate(float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateHighlights();
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        // ClearAllHighlights();

        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }
    }

    private void OnTextChanged(Object obj)
    {
        if (obj == _textComponent && !_isUpdating && !_isInternalTextUpdate)
        {
            UpdateHighlights();
        }
    }

    /// <summary>
    /// 更新所有高亮背景
    /// </summary>
    public void UpdateHighlights()
    {
        if (_isUpdating) return;

        _isUpdating = true;

        // 使用协程避免在渲染循环中操作
        if (_updateCoroutine != null) StopCoroutine(_updateCoroutine);
        _updateCoroutine = StartCoroutine(UpdateHighlightsRoutine());
    }

    private IEnumerator UpdateHighlightsRoutine()
    {
        // 等待渲染循环结束
        yield return new WaitForEndOfFrame();

        try
        {
            // ClearAllHighlights();

            if (string.IsNullOrEmpty(_textComponent.text))
            {
                _isUpdating = false;
                yield break;
            }

            // 强制更新文本网格
            if (!_textComponent.havePropertiesChanged)
            {
                _textComponent.ForceMeshUpdate(true);
            }

            // 解析并移除标签
            string processedText;
            List<HighlightSegment> segments = ParseAndRemoveTags(out processedText);

            // 更新文本内容（移除标签）
            if (!string.Equals(_textComponent.text, processedText))
            {
                _isInternalTextUpdate = true;
                _textComponent.text = processedText;
                _textComponent.ForceMeshUpdate(true);
                _isInternalTextUpdate = false;
            }

            // 创建高亮
            foreach (var segment in segments)
            {
                AddHighlightForTextRange(segment.startIndex, segment.endIndex, segment.preset);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating highlights: {e.Message}\n{e.StackTrace}");
        }
        finally
        {
            _isUpdating = false;
            _updateCoroutine = null;
        }
    }

    /// <summary>
    /// 解析并移除标签，返回处理后的文本和标签位置
    /// </summary>
    private List<HighlightSegment> ParseAndRemoveTags(out string processedText)
    {
        List<HighlightSegment> segments = new List<HighlightSegment>();
        StringBuilder sb = new StringBuilder();
        int currentIndex = 0;
        int offset = 0;

        MatchCollection matches = _tagRegex.Matches(_textComponent.text);
        foreach (Match match in matches)
        {
            if (match.Groups.Count < 3) continue;

            // 添加标签前的文本
            sb.Append(_textComponent.text.Substring(currentIndex, match.Index - currentIndex));

            string attributes = match.Groups[1].Value.Trim();
            string content = match.Groups[2].Value;

            // 记录内容位置（新文本中的索引）
            int contentStart = sb.Length;
            sb.Append(content);
            int contentEnd = sb.Length - 1;

            // 解析预设
            var preset = ParseTagAttributes(attributes);

            segments.Add(new HighlightSegment
            {
                startIndex = contentStart,
                endIndex = contentEnd,
                preset = preset
            });

            currentIndex = match.Index + match.Length;
        }

        // 添加剩余文本
        sb.Append(_textComponent.text.Substring(currentIndex));
        processedText = sb.ToString();

        return segments;
    }

    /// <summary>
    /// 解析标签属性
    /// </summary>
    private HighlightPreset ParseTagAttributes(string attributes)
    {
        HighlightPreset preset = new HighlightPreset()
        {
            presetName = "default",
            backgroundColor = new Color(1, 0.8f, 0.4f, 0.5f),
            padding = new Vector4(2, 1, 2, 1)
        };

        var presetMatch = Regex.Match(attributes, "preset=\"([^\"]*)\"");
        if (presetMatch.Success && presetMatch.Groups.Count > 1)
        {
            string presetName = presetMatch.Groups[1].Value;
            HighlightPreset foundPreset = highlightPresets.Find(p => p.presetName == presetName);
            if (foundPreset != null) preset = foundPreset;
        }

        var colorMatch = Regex.Match(attributes, "color=\"#?([0-9a-fA-F]{6,8})\"");
        if (colorMatch.Success && colorMatch.Groups.Count > 1)
        {
            string colorHex = colorMatch.Groups[1].Value;
            if (colorHex.Length == 6) colorHex += "FF";
            if (ColorUtility.TryParseHtmlString("#" + colorHex, out Color color))
            {
                preset.backgroundColor = color;
            }
        }

        var paddingMatch = Regex.Match(attributes, "padding=\"([0-9,\\. ]+)\"");
        if (paddingMatch.Success && paddingMatch.Groups.Count > 1)
        {
            string[] values = paddingMatch.Groups[1].Value.Split(',');
            if (values.Length >= 4)
            {
                if (float.TryParse(values[0], out float left)) preset.padding.x = left;
                if (float.TryParse(values[1], out float top)) preset.padding.y = top;
                if (float.TryParse(values[2], out float right)) preset.padding.z = right;
                if (float.TryParse(values[3], out float bottom)) preset.padding.w = bottom;
            }
            else if (values.Length == 1)
            {
                if (float.TryParse(values[0], out float uniform))
                    preset.padding = new Vector4(uniform, uniform, uniform, uniform);
            }
        }

        var yOffsetMatch = Regex.Match(attributes, "yOffset=\"([-0-9\\.]+)\"");
        if (yOffsetMatch.Success && yOffsetMatch.Groups.Count > 1)
        {
            if (float.TryParse(yOffsetMatch.Groups[1].Value, out float yOffset))
            {
                preset.yOffset = yOffset;
            }
        }

        return preset;
    }

    /// <summary>
    /// 为指定文本范围添加高亮背景
    /// </summary>
    private void AddHighlightForTextRange(int startIndex, int endIndex, HighlightPreset preset)
    {
        TMP_TextInfo textInfo = _textComponent.textInfo;

        if (startIndex < 0 || endIndex >= textInfo.characterInfo.Length || startIndex > endIndex)
        {
            return;
        }

        endIndex = Mathf.Min(endIndex, textInfo.characterCount - 1);
        int currentLine = textInfo.characterInfo[startIndex].lineNumber;
        int lineStart = startIndex;

        for (int i = startIndex; i <= endIndex; i++)
        {
            if (i >= textInfo.characterInfo.Length) break;

            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            if (charInfo.lineNumber != currentLine)
            {
                CreateHighlightForLine(lineStart, i - 1, currentLine, preset);
                lineStart = i;
                currentLine = charInfo.lineNumber;
            }
        }

        CreateHighlightForLine(lineStart, endIndex, currentLine, preset);
    }

    /// <summary>
    /// 为单行文本创建高亮背景
    /// </summary>
    private void CreateHighlightForLine(int startIndex, int endIndex, int lineNumber, HighlightPreset preset)
    {
        TMP_TextInfo textInfo = _textComponent.textInfo;
        if (lineNumber < 0 || lineNumber >= textInfo.lineCount) return;

        TMP_LineInfo lineInfo = textInfo.lineInfo[lineNumber];

        int firstVisibleIndex = -1;
        int lastVisibleIndex = -1;

        for (int i = lineInfo.firstCharacterIndex; i <= lineInfo.lastCharacterIndex; i++)
        {
            if (i >= textInfo.characterInfo.Length) break;
            if (textInfo.characterInfo[i].isVisible)
            {
                if (firstVisibleIndex < 0) firstVisibleIndex = i;
                lastVisibleIndex = i;
            }
        }

        if (firstVisibleIndex < 0 || lastVisibleIndex < 0) return;

        float startX = textInfo.characterInfo[startIndex].bottomLeft.x;
        float endX = textInfo.characterInfo[endIndex].topRight.x;

        Vector3 highlightMin = new Vector3(startX, lineInfo.descender, 0);
        Vector3 highlightMax = new Vector3(endX, lineInfo.ascender, 0);

        highlightMin.x -= preset.padding.x;
        highlightMin.y -= preset.padding.w + preset.yOffset;
        highlightMax.x += preset.padding.z;
        highlightMax.y += preset.padding.y + preset.yOffset;

        CreateHighlight(highlightMin, highlightMax, preset);
    }

    /// <summary>
    /// 创建高亮背景对象
    /// </summary>
    private void CreateHighlight(Vector3 min, Vector3 max, HighlightPreset preset)
    {
        GameObject highlight = GetHighlightFromPool();
        if (highlight == null) return;

        RectTransform rt = highlight.GetComponent<RectTransform>();
        Image img = highlight.GetComponent<Image>();

        highlight.transform.SetParent(_textComponent.transform, false);
        // 确保高亮在文本下方（修改层级为文本之后）
        highlight.transform.SetSiblingIndex(_textComponent.transform.childCount - 1);

        Vector3 center = (min + max) / 2f;
        Vector3 size = max - min;

        rt.localPosition = center;
        rt.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));

        highlight.transform.SetParent(_textComponent.transform.parent, true);
        int index = _textComponent.transform.GetSiblingIndex();
        highlight.transform.SetSiblingIndex(index - 1);

        img.color = preset.backgroundColor;

        if (preset.backgroundSprite != null)
        {
            img.sprite = preset.backgroundSprite;
            img.type = preset.useStretch ? Image.Type.Sliced : Image.Type.Simple;
        }
        else
        {
            img.sprite = null;
        }

        highlight.SetActive(true);
        _activeHighlights.Add(highlight);
    }

    /// <summary>
    /// 从对象池获取高亮对象
    /// </summary>
    private GameObject GetHighlightFromPool()
    {
        while (_highlightPool.Count > 0 && _highlightPool.Peek() == null)
        {
            _highlightPool.Dequeue();
        }

        if (_highlightPool.Count > 0) return _highlightPool.Dequeue();

        GameObject highlight = new GameObject("TextHighlight", typeof(RectTransform), typeof(Image));
        RectTransform rt = highlight.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);

        highlight.GetComponent<Image>().raycastTarget = false;
        return highlight;
    }

    /// <summary>
    /// 回收高亮对象到对象池
    /// </summary>
    private void ReturnHighlightToPool(GameObject highlight)
    {
        if (highlight == null) return;
        highlight.SetActive(false);
        _highlightPool.Enqueue(highlight);
    }

    /// <summary>
    /// 清除所有高亮
    /// </summary>
    private void ClearAllHighlights()
    {
        foreach (GameObject highlight in _activeHighlights)
        {
            if (highlight != null) ReturnHighlightToPool(highlight);
        }

        _activeHighlights.Clear();
    }
}