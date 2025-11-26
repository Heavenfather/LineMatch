using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Extension
{
    [RequireComponent(typeof(Animator))]
    public class UIAnimationHelper : MonoBehaviour
    {
        [SerializeField]
        private bool _playOnEnable = false;
        
        private Animator _animator;
        private Dictionary<string, AnimationClip> _clipsMap;
        private Coroutine _currentCoroutine;

        private void Awake()
        {
            _animator = this.GetComponent<Animator>();
            //初始化动画片段映射
            _clipsMap = new Dictionary<string, AnimationClip>(10);
            var controller = _animator.runtimeAnimatorController;
            if (controller != null)
            {
                foreach (var clip in controller.animationClips)
                {
                    _clipsMap.TryAdd(clip.name, clip);
                }
            }
        }

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                PlayAnimation(_animator.GetCurrentAnimatorStateInfo(0).shortNameHash.ToString());
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public void PlayAnimation(string animationName, Action<bool> onComplete = null)
        {
            //先停止之前的协程
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }

            _animator.Play(animationName);
            if (onComplete != null && _clipsMap.TryGetValue(animationName, out var clip))
            {
                if (clip.wrapMode == WrapMode.Once || clip.wrapMode == WrapMode.Default)
                {
                    _currentCoroutine = StartCoroutine(WaitForAnimationComplete(animationName, onComplete));
                }
            }
        }

        private IEnumerator WaitForAnimationComplete(string animationName, Action<bool> onComplete)
        {
            // 等待一帧，确保动画状态已经切换
            yield return null;

            // 获取当前状态
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName(animationName))
            {
                // 如果当前状态不是目标动画，可能已经被切换，直接退出
                onComplete?.Invoke(false);
                yield break;
            }

            // 等待直到动画播放完成
            // 注意：循环动画的normalizedTime会一直增加，所以我们需要判断该动画是否循环
            // 但是我们无法从状态信息中获取是否循环，所以我们需要通过动画片段来判断
            // 因此，还是需要动画片段

            // 所以，我们只能要求用户确保传入的animationName是动画片段名，并且我们能够从_clipsMap中获取到
            if (!_clipsMap.TryGetValue(animationName, out var clip))
            {
                // 找不到片段，直接退出
                onComplete?.Invoke(false);
                yield break;
            }

            // 如果是循环动画，我们不需要等待完成
            if (clip.wrapMode == WrapMode.Loop || clip.wrapMode == WrapMode.PingPong)
            {
                onComplete?.Invoke(false);
                yield break;
            }

            // 等待动画长度
            float speed = _animator.speed;
            float waitTime = clip.length / speed;
            float timer = 0f;

            while (timer < waitTime)
            {
                stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName(animationName))
                {
                    yield break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            // 最后再检查一次状态
            stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(animationName))
            {
                onComplete?.Invoke(true);
            }
        }
    }
}