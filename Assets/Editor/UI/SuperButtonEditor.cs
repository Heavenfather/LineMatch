using System;
using UnityEditor;
using UnityEngine;

namespace GameEditor.UI
{
    [CustomEditor(typeof(SuperButton), true)]
    [CanEditMultipleObjects]
    public class SuperButtonEditor : UnityEditor.UI.ButtonEditor
    {
        protected const int DEFAULT_LABEL_WIDTH = 115;

        private static readonly string[] ButtonTypeClick =
            { "OnlySingleClick", "OnlyDoubleClick", "LongClick", "Delayed", "Hold" };

        private SerializedProperty _isMotion;
        private SerializedProperty _clickType;
        private SerializedProperty _onLongClick;
        private SerializedProperty _onDoubleClick;
        private SerializedProperty _onPointerUp;
        private SerializedProperty _onHold;
        private SerializedProperty _isAffectToSelf;
        private SerializedProperty _allowMultipleClick;
        private SerializedProperty _timeDisableButton;
        private SerializedProperty _longClickInterval;
        private SerializedProperty _delayDetectHold;
        private SerializedProperty _doubleClickInterval;
        private SerializedProperty _ignoreTimeScale;
        private SerializedProperty _affectObject;
        private SerializedProperty _scale;
        private SerializedProperty _duration;
        private SerializedProperty _easeDown;
        private SerializedProperty _easeUp;

        protected override void OnEnable()
        {
            base.OnEnable();
            _clickType = serializedObject.FindProperty("clickType");
            _onLongClick = serializedObject.FindProperty("onLongClick");
            _onDoubleClick = serializedObject.FindProperty("onDoubleClick");
            _onPointerUp = serializedObject.FindProperty("onPointerUp");
            _onHold = serializedObject.FindProperty("onHold");
            _allowMultipleClick = serializedObject.FindProperty("allowMultipleClick");
            _timeDisableButton = serializedObject.FindProperty("timeDisableButton");
            _longClickInterval = serializedObject.FindProperty("longClickInterval");
            _delayDetectHold = serializedObject.FindProperty("delayDetectHold");
            _doubleClickInterval = serializedObject.FindProperty("doubleClickInterval");
            _ignoreTimeScale = serializedObject.FindProperty("ignoreTimeScale");
            _isAffectToSelf = serializedObject.FindProperty("isAffectToSelf");
            _affectObject = serializedObject.FindProperty("affectObject");
            _isMotion = serializedObject.FindProperty("isMotion");
            _scale = serializedObject.FindProperty("motionData").FindPropertyRelative("scale");
            _duration = serializedObject.FindProperty("motionData").FindPropertyRelative("duration");
            _easeDown = serializedObject.FindProperty("motionData").FindPropertyRelative("easeDown");
            _easeUp = serializedObject.FindProperty("motionData").FindPropertyRelative("easeUp");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawInspector();
        }

        protected virtual void DrawInspector()
        {
            Draw();
        }

        protected void Draw(Action callback = null)
        {
            serializedObject.Update();

            if (callback != null)
            {
                callback.Invoke();
                GUILayout.Space(2);
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(
                new GUIContent("Type Click",
                    "点击类型:\nOnlySingleClick(普通单击)\nOnlyDoubleClick(双击)\nLongClick(长按)\nDelayed(双击之前不执行单个点击操作)\nHold(点击或按住)"),
                GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _clickType.enumValueIndex = EditorGUILayout.Popup(_clickType.enumValueIndex, ButtonTypeClick);
            EditorGUILayout.EndHorizontal();

            switch ((EButtonClickType)_clickType.enumValueIndex)
            {
                case EButtonClickType.OnlySingleClick:
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Multiple Click", "允许连续快速点击"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _allowMultipleClick.boolValue = GUILayout.Toggle(_allowMultipleClick.boolValue, "");
                    EditorGUILayout.EndHorizontal();

                    if (!_allowMultipleClick.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Duration(s)", "不允许连续快速点击时禁用间隔"),
                            GUILayout.Width(DEFAULT_LABEL_WIDTH));
                        _timeDisableButton.floatValue =
                            EditorGUILayout.Slider(_timeDisableButton.floatValue, 0.01f, 1f);
                        EditorGUILayout.EndHorizontal();
                    }

                    break;
                }
                case EButtonClickType.LongClick:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Duration(s)", "延迟多长时间触发长按回调"),
                        GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _longClickInterval.floatValue = EditorGUILayout.Slider(_longClickInterval.floatValue, 0.01f, 5f);
                    EditorGUILayout.EndHorizontal();
                    break;
                case EButtonClickType.OnlyDoubleClick:
                case EButtonClickType.Delayed:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Duration(s)", "触发双击间隔"),
                        GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _doubleClickInterval.floatValue =
                        EditorGUILayout.Slider(_doubleClickInterval.floatValue, 0.01f, 1f);
                    EditorGUILayout.EndHorizontal();
                    break;
                case EButtonClickType.Hold:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Delay Detect(s)", "连续触发Hold回调间隔"),
                        GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _delayDetectHold.floatValue = EditorGUILayout.Slider(_delayDetectHold.floatValue, 0.01f, 1f);
                    EditorGUILayout.EndHorizontal();
                    break;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Use Tween", "是否使用补间动画"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _isMotion.boolValue = GUILayout.Toggle(_isMotion.boolValue, "");
            EditorGUILayout.EndHorizontal();

            if (_isMotion.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Ignore Time Scale", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _ignoreTimeScale.boolValue = GUILayout.Toggle(_ignoreTimeScale.boolValue, "");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Affect To Self", "补间动画是否影响的自身"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _isAffectToSelf.boolValue = GUILayout.Toggle(_isAffectToSelf.boolValue, "");
                if (!_isAffectToSelf.boolValue)
                    _affectObject.objectReferenceValue =
                        EditorGUILayout.ObjectField("", _affectObject.objectReferenceValue, typeof(Transform), true) as
                            Transform;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Scale", GUILayout.Width(DEFAULT_LABEL_WIDTH - 50));
                GUILayout.FlexibleSpace();
                _scale.vector2Value = EditorGUILayout.Vector2Field("", _scale.vector2Value);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Duration","补间动画执行时长"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _duration.floatValue = EditorGUILayout.Slider(_duration.floatValue, 0.01f, 5f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Ease Down"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.PropertyField(_easeDown, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Ease Up"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.PropertyField(_easeUp, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
            }

            switch ((EButtonClickType)_clickType.enumValueIndex)
            {
                case EButtonClickType.LongClick:
                    EditorGUILayout.PropertyField(_onLongClick);
                    break;
                case EButtonClickType.Hold:
                    EditorGUILayout.PropertyField(_onHold);
                    break;
                case EButtonClickType.OnlyDoubleClick:
                case EButtonClickType.Delayed:
                    EditorGUILayout.PropertyField(_onDoubleClick);
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "In case you move the mouse pointer away from the button while holding down the OnPointerUp event is still called. So be careful when using it",
                MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_onPointerUp);

            EditorGUILayout.Space();
            Repaint();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}