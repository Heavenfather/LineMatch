using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using HotfixCore.Extensions;
using Logger = GameCore.Log.Logger;

namespace HotfixCore.Module
{
    public class ScreenLocker : MonoBehaviour
    {
        // 锁屏状态管理核心数据结构
        private class LockState
        {
            public string reason;
            public Coroutine timeoutCoroutine;
            public float lockTime;
            public bool isActive;
        }

        private GameObject _lockMask;
        private readonly Dictionary<string, LockState> _activeLocks = new Dictionary<string, LockState>();
        private readonly HashSet<string> _pendingRemoval = new HashSet<string>();
        private bool _isScreenLocked = false;

        private void Awake()
        {
            _lockMask = this.transform.Find("LockMask").gameObject;
            _lockMask.transform.localScale = new Vector3(0, 1, 1);
            // _lockMask.SetVisible(false);
        }

        // 主锁屏接口方法
        public void ScreenLock(string reason, bool visible, float lockTime = 3.0f)
        {
            // 空值保护
            if (string.IsNullOrEmpty(reason))
            {
                Logger.Error("必须要传入锁住屏幕的原因!");
                return;
            }

            // 加锁逻辑
            if (visible)
            {
                if (lockTime <= 0)
                    return;
                AddOrUpdateLock(reason, lockTime);
            }
            // 解锁逻辑
            else
            {
                RemoveLock(reason);
            }
        }

        private void AddOrUpdateLock(string reason, float lockTime)
        {
            // 已存在的锁：刷新超时时间
            if (_activeLocks.TryGetValue(reason, out LockState existingLock))
            {
                // 停止旧协程
                if (existingLock.timeoutCoroutine != null)
                {
                    StopCoroutine(existingLock.timeoutCoroutine);
                }

                // 启动新超时计时
                existingLock.timeoutCoroutine = StartCoroutine(TimeoutLock(reason, lockTime));
                existingLock.lockTime = lockTime;
                existingLock.isActive = true;
            }
            // 新锁：创建状态
            else
            {
                var newLock = new LockState
                {
                    reason = reason,
                    lockTime = lockTime,
                    isActive = true
                };

                newLock.timeoutCoroutine = StartCoroutine(TimeoutLock(reason, lockTime));
                _activeLocks.Add(reason, newLock);
            }

            // 激活遮罩
            UpdateScreenLockState();
        }

        private void RemoveLock(string reason)
        {
            if (_activeLocks.TryGetValue(reason, out LockState lockState))
            {
                // 标记待移除，避免在迭代中修改集合
                _pendingRemoval.Add(reason);
                lockState.isActive = false;

                // 延迟到帧结束处理确保线程安全
                StartCoroutine(ProcessRemovalsNextFrame());
            }
        }

        private IEnumerator ProcessRemovalsNextFrame()
        {
            yield return new WaitForEndOfFrame();

            foreach (var reason in _pendingRemoval)
            {
                if (_activeLocks.TryGetValue(reason, out LockState lockState))
                {
                    // 停止关联协程
                    if (lockState.timeoutCoroutine != null)
                    {
                        StopCoroutine(lockState.timeoutCoroutine);
                    }

                    _activeLocks.Remove(reason);
                    // Logger.Debug($"[{reason}] 主动解除!");
                }
            }

            _pendingRemoval.Clear();
            UpdateScreenLockState();
        }

        private IEnumerator TimeoutLock(string reason, float lockTime)
        {
            yield return new WaitForSecondsRealtime(lockTime);

            if (_activeLocks.TryGetValue(reason, out LockState lockState) && lockState.isActive)
            {
                Logger.Warning($"[{reason}] 因超时自动解除,请检查锁屏的逻辑是否存在问题!");
                RemoveLock(reason);
            }
        }

        private void UpdateScreenLockState()
        {
            bool shouldLock = _activeLocks.Count > 0;

            if (_isScreenLocked != shouldLock)
            {
                _isScreenLocked = shouldLock;

                this._lockMask.gameObject.transform.localScale = new Vector3(shouldLock ? 1 : 0, 1, 1);
                if (shouldLock)
                {
                    this.transform.SetAsLastSibling();
                }
            }
        }
    }
}