using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 按钮点击类型
/// </summary>
public enum EButtonClickType
{
    /// <summary>
    /// 单次点击
    /// </summary>
    OnlySingleClick = 0,

    /// <summary>
    /// 双击
    /// </summary>
    OnlyDoubleClick = 1,

    /// <summary>
    /// 长按
    /// </summary>
    LongClick = 2,

    /// <summary>
    /// 如果是双击，则不会执行单个点击
    /// 如果想确保在双击之前不执行单个点击操作，使用这个方法
    /// 缺点是在执行单次点击时有延迟（延迟时间是双击的间隔）
    /// </summary>
    Delayed = 3,

    /// <summary>
    /// 点击或按住
    /// 正常执行按钮onClick事件
    /// 如果hold已经被调用，那么松开手时，onClick将不会再被调用
    /// 删除双击执行
    /// </summary>
    Hold = 4
}

public interface IButton
{
}

public interface ILabel
{
    TextMeshProUGUI Label { get; }
}

public interface IButtonAffect
{
    /// <summary>
    /// 默认Scale值
    /// </summary>
    Vector3 DefaultScale { get; set; }

    /// <summary>
    /// 是否只影响自己
    /// </summary>
    bool IsAffectToSelf { get; }

    /// <summary>
    /// 如果IsAffectToSelf为false，还影响哪个对象
    /// </summary>
    Transform AffectObject { get; }
}

public class SuperButton : Button, IButton, IButtonAffect
{
    /// <summary>
    /// 检定双击间隔
    /// </summary>
    private const float DOUBLE_CLICK_TIME_INTERVAL = 0.2f;

    /// <summary>
    /// 检定长按间隔
    /// </summary>
    private const float LONG_CLICK_TIME_INTERVAL = 1.5f;

    [Serializable]
    public class ButtonHoldEvent : UnityEngine.Events.UnityEvent<float>
    {
    }

    [Serializable]
    public class TweenData
    {
        public Vector2 scale;
        public float duration = 0.1f;
        public Ease easeDown = Ease.OutQuad;
        public Ease easeUp = Ease.OutBack;
    }

    [SerializeField] private EButtonClickType clickType = EButtonClickType.OnlySingleClick;
    [SerializeField] private bool allowMultipleClick;
    [SerializeField] private float timeDisableButton = DOUBLE_CLICK_TIME_INTERVAL;
    [SerializeField] private float doubleClickInterval = DOUBLE_CLICK_TIME_INTERVAL;
    [SerializeField] private float longClickInterval = LONG_CLICK_TIME_INTERVAL;
    [SerializeField] private float delayDetectHold = DOUBLE_CLICK_TIME_INTERVAL;
    [SerializeField] private ButtonClickedEvent onDoubleClick = new();
    [SerializeField] private ButtonClickedEvent onLongClick = new();
    [SerializeField] private ButtonClickedEvent onPointerUp = new();
    [SerializeField] private ButtonClickedEvent onPointerDown = new();
    [SerializeField] private ButtonHoldEvent onHold = new();
    [SerializeField] private bool isMotion = true;
    [SerializeField] private bool ignoreTimeScale;
    [SerializeField] private bool isAffectToSelf = true;
    [SerializeField] private Transform affectObject;

    [SerializeField] private TweenData motionData = new()
        { scale = new Vector2(1.2f, 1.2f) };

    public Action<float> onHoldEvent;
    public Action onHoldStoppedEvent;
    public Action onDoubleClickEvent;
    public Action onLongClickEvent;
    public Action onPointerUpEvent;
    public Action onPointerDownEvent;

    private Coroutine _routineLongClick;
    private Coroutine _routineHold;
    private bool _clickedOnce;
    private bool _longClickDone;
    private bool _holdDone;
    private bool _holding;
    private float _doubleClickTimer;
    private float _longClickTimer;
    private float _holdTimer;
    private Vector3 _endValue;
    private bool _isCompletePhaseDown;
    private readonly WaitForEndOfFrame _waitForEndOfFrame = new();
    private CancellationToken _destroyToken;

#if UNITY_EDITOR
    /// <summary>
    /// Editor only
    /// </summary>
    public bool IsMotion
    {
        get => isMotion;
        set => isMotion = value;
    }
#endif

    /// <summary>
    /// OnPointerUp调用时为true
    /// </summary>
    private bool IsRelease { get; set; } = true;

    /// <summary>
    /// 确保OnPointerClick在IsRelease的条件下被调用，只有当OnPointerExit被调用时才设置为true
    /// </summary>
    private bool IsPrevent { get; set; }


    public Vector3 DefaultScale { get; set; }
    public bool IsAffectToSelf => isAffectToSelf;
    public Transform AffectObject => IsAffectToSelf ? targetGraphic.rectTransform : affectObject;


    protected override void Awake()
    {
        base.Awake();
        if (!Application.isPlaying) return;
        DefaultScale = AffectObject.localScale;
        _destroyToken = destroyCancellationToken;
    }

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();
        _doubleClickTimer = 0;
        doubleClickInterval = DOUBLE_CLICK_TIME_INTERVAL;
        _longClickTimer = 0;
        longClickInterval = LONG_CLICK_TIME_INTERVAL;
        delayDetectHold = DOUBLE_CLICK_TIME_INTERVAL;
        _clickedOnce = false;
        _longClickDone = false;
        _holdDone = false;
    }
#endif

    protected override void OnDestroy()
    {
        base.OnDestroy();

        onHoldEvent = null;
        onHoldStoppedEvent = null;
        onDoubleClickEvent = null;
        onLongClickEvent = null;
        onPointerUpEvent = null;
        onPointerDownEvent = null;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (!Application.isPlaying) return; // not execute awake when not playing

        if (_routineLongClick != null) StopCoroutine(_routineLongClick);
        if (_routineHold != null) StopCoroutine(_routineHold);
        interactable = true;
        _clickedOnce = false;
        _longClickDone = false;
        _holdDone = false;
        _doubleClickTimer = 0;
        _longClickTimer = 0;
        if (AffectObject != null) AffectObject.localScale = DefaultScale;
    }

    #region Button Override

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        InternalInvokePointerDownEvent();
        IsRelease = false;
        IsPrevent = false;
        if (IsDetectLongCLick && interactable) RegisterLongClick();
        if (IsDetectHold && interactable) RegisterHold();

        RunMotionPointerDown();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (IsRelease) return;
        base.OnPointerUp(eventData);
        IsRelease = true;
        InternalInvokePointerUpEvent();
        if (IsDetectLongCLick) ResetLongClick();
        if (IsDetectHold)
        {
            if (_holding)
            {
                onHoldStoppedEvent?.Invoke();
                _holding = false;
                _holdDone = true;
            }

            ResetHold();
        }

        RunMotionPointerUp();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (IsRelease && IsPrevent || !interactable) return;

        StartClick(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (IsRelease) return;
        base.OnPointerExit(eventData);
        IsPrevent = true;
        if (IsDetectLongCLick) ResetLongClick();
        if (IsDetectHold)
        {
            if (_holding)
            {
                onHoldStoppedEvent?.Invoke();
                _holding = false;
                _holdDone = true;
            }

            ResetHold();
        }

        OnPointerUp(eventData);
    }

    private void RunMotionPointerDown()
    {
        if (!isMotion) return;
        if (!interactable)
        {
            return;
        }

        TweenDown(motionData);
    }

    private void RunMotionPointerUp()
    {
        if (!isMotion) return;
        if (!interactable)
        {
            return;
        }

        TweenUp(motionData);
    }

    private async UniTask DisableButtonAsync(float duration, CancellationToken token)
    {
        interactable = false;
        if (!token.IsCancellationRequested)
            await UniTask.WaitForSeconds(duration, ignoreTimeScale, cancellationToken: token);
        interactable = true;
    }

    private void InternalInvokePointerDownEvent()
    {
        onPointerDownEvent?.Invoke();
        onPointerDown?.Invoke();
    }

    private void InternalInvokePointerUpEvent()
    {
        onPointerUpEvent?.Invoke();
        onPointerUp?.Invoke();
    }

    private void StartClick(PointerEventData eventData)
    {
        if (IsDetectLongCLick && _longClickDone)
        {
            ResetLongClick();
            return;
        }

        if (IsDetectHold && _holdDone)
        {
            ResetHold();
            return;
        }

        Execute(eventData).Forget();
    }

    private async UniTask Execute(PointerEventData eventData)
    {
        if (IsDetectSingleClick) base.OnPointerClick(eventData);

        if (!allowMultipleClick && clickType == EButtonClickType.OnlySingleClick)
        {
            if (!interactable) return;

            await DisableButtonAsync(timeDisableButton, _destroyToken);
            return;
        }

        if (clickType == EButtonClickType.OnlySingleClick || clickType == EButtonClickType.LongClick ||
            clickType == EButtonClickType.Hold) return;

        if (!_clickedOnce && _doubleClickTimer < doubleClickInterval)
        {
            _clickedOnce = true;
        }
        else
        {
            _clickedOnce = false;
            return;
        }

        await UniTask.Yield();

        while (_doubleClickTimer < doubleClickInterval)
        {
            if (!_clickedOnce)
            {
                ExecuteDoubleClick();
                _doubleClickTimer = 0;
                _clickedOnce = false;
                return;
            }

            if (ignoreTimeScale) _doubleClickTimer += Time.unscaledDeltaTime;
            else _doubleClickTimer += Time.deltaTime;
            await UniTask.Yield();
        }

        if (clickType == EButtonClickType.Delayed) base.OnPointerClick(eventData);

        _doubleClickTimer = 0;
        _clickedOnce = false;
    }

    #region single click

    private bool IsDetectSingleClick =>
        clickType is EButtonClickType.OnlySingleClick || clickType == EButtonClickType.LongClick ||
        clickType == EButtonClickType.Hold;

    #endregion

    #region double click

    private bool IsDetectDoubleClick =>
        clickType == EButtonClickType.OnlyDoubleClick || clickType == EButtonClickType.Delayed;

    private void ExecuteDoubleClick()
    {
        if (!IsActive() || !IsInteractable() || !IsDetectDoubleClick) return;
        InternalInvokeDoubleClickEvent();
    }

    private void InternalInvokeDoubleClickEvent()
    {
        onDoubleClickEvent?.Invoke();
        onDoubleClick?.Invoke();
    }

    #endregion

    #region long click

    private bool IsDetectLongCLick => clickType == EButtonClickType.LongClick;

    private IEnumerator IsExecuteLongClick()
    {
        while (_longClickTimer < longClickInterval)
        {
            if (ignoreTimeScale) _longClickTimer += Time.unscaledDeltaTime;
            else _longClickTimer += Time.deltaTime;
            yield return _waitForEndOfFrame;
        }

        ExecuteLongClick();
        _longClickDone = true;
    }

    private void ExecuteLongClick()
    {
        if (!IsActive() || !IsInteractable() || !IsDetectLongCLick) return;
        InternalInvokeLongClickEvent();
    }

    private void ResetLongClick()
    {
        if (!IsDetectLongCLick) return;
        _longClickDone = false;
        _longClickTimer = 0;
        if (_routineLongClick != null) StopCoroutine(_routineLongClick);
    }

    private void RegisterLongClick()
    {
        if (_longClickDone || !IsDetectLongCLick) return;
        ResetLongClick();
        _routineLongClick = StartCoroutine(IsExecuteLongClick());
    }

    private void InternalInvokeLongClickEvent()
    {
        onLongClickEvent?.Invoke();
        onLongClick?.Invoke();
    }

    #endregion

    #region hold click

    private bool IsDetectHold => clickType == EButtonClickType.Hold;

    private IEnumerator IeExecuteHold()
    {
        _holding = false;
        if (ignoreTimeScale) yield return new WaitForSecondsRealtime(delayDetectHold);
        else yield return new WaitForSeconds(delayDetectHold);
        _holding = true;
        while (true)
        {
            if (ignoreTimeScale) _holdTimer += Time.unscaledDeltaTime;
            else _holdTimer += Time.deltaTime;
            ExecuteHold(_holdTimer);
            yield return _waitForEndOfFrame;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private void ExecuteHold(float time)
    {
        if (!IsActive() || !IsInteractable() || !IsDetectHold) return;
        InternalInvokeHoldEvent(time);
    }

    private void ResetHold()
    {
        if (!IsDetectHold) return;
        _holdDone = false;
        _holdTimer = 0;
        if (_routineHold != null) StopCoroutine(_routineHold);
    }

    private void RegisterHold()
    {
        if (_holdDone || !IsDetectHold) return;
        ResetHold();
        _routineHold = StartCoroutine(IeExecuteHold());
    }

    private void InternalInvokeHoldEvent(float time)
    {
        onHoldEvent?.Invoke(time);
        onHold?.Invoke(time);
    }

    #endregion

    #endregion

    #region Tween

    private void TweenUp(TweenData data)
    {
        _endValue = DefaultScale;
        DOTween.Kill(AffectObject);
        DOTween.To(() => AffectObject.localScale, x => { AffectObject.localScale = x; }, _endValue,
            motionData.duration).SetEase(motionData.easeUp).SetAutoKill(true);
    }

    private void TweenDown(TweenData data)
    {
        _endValue = data.scale;
        DOTween.Kill(AffectObject);
        DOTween.To(() => DefaultScale, x => { AffectObject.localScale = x; }, _endValue, motionData.duration)
            .SetEase(motionData.easeDown).SetAutoKill(true);
    }

    #endregion
}