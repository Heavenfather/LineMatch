using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace HotfixCore.Utils
{
    /// <summary>
    /// 常用Unity接口管理
    /// </summary>
    public static class UnityUtil
    {
        private static GameObject _entity;
        private static MainBehaviour _mainBehaviour;

        #region Coroutine

        public static Coroutine StartCoroutine(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return null;
            }

            MakeEntity();
            return _mainBehaviour.StartCoroutine(methodName);
        }

        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            if (routine == null)
            {
                return null;
            }

            MakeEntity();
            return _mainBehaviour.StartCoroutine(routine);
        }

        public static void StopCoroutine(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return;
            }

            if (_entity != null)
            {
                _mainBehaviour.StopCoroutine(methodName);
            }
        }

        public static void StopCoroutine(IEnumerator routine)
        {
            if (routine == null)
            {
                return;
            }

            if (_entity != null)
            {
                _mainBehaviour.StopCoroutine(routine);
            }
        }

        public static void StopCoroutine(Coroutine routine)
        {
            if (routine == null)
                return;

            if (_entity != null)
            {
                _mainBehaviour.StopCoroutine(routine);
                routine = null;
            }
        }

        public static void StopAllCoroutines()
        {
            if (_entity != null)
            {
                _mainBehaviour.StopAllCoroutines();
            }
        }

        #endregion

        #region Update Event

        /// <summary>
        /// 为给外部提供的 添加帧更新事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void AddUpdateListener(UnityAction fun)
        {
            MakeEntity();
            AddUpdateListenerImp(fun);
        }

        private static void AddUpdateListenerImp(UnityAction fun)
        {
            _mainBehaviour.AddUpdateListener(fun);
        }

        /// <summary>
        /// 为给外部提供的 添加物理帧更新事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void AddFixedUpdateListener(UnityAction fun)
        {
            MakeEntity();
            AddFixedUpdateListenerImp(fun);
        }

        private static void AddFixedUpdateListenerImp(UnityAction fun)
        {
            _mainBehaviour.AddFixedUpdateListener(fun);
        }

        /// <summary>
        /// 为给外部提供的 添加Late帧更新事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void AddLateUpdateListener(UnityAction fun)
        {
            MakeEntity();
            AddLateUpdateListenerImp(fun);
        }

        private static void AddLateUpdateListenerImp(UnityAction fun)
        {
            _mainBehaviour.AddLateUpdateListener(fun);
        }

        /// <summary>
        /// 移除帧更新事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void RemoveUpdateListener(UnityAction fun)
        {
            MakeEntity();
            _mainBehaviour.RemoveUpdateListener(fun);
        }

        /// <summary>
        /// 移除物理帧更新事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void RemoveFixedUpdateListener(UnityAction fun)
        {
            MakeEntity();
            _mainBehaviour.RemoveFixedUpdateListener(fun);
        }

        /// <summary>
        /// 移除Late帧更新事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void RemoveLateUpdateListener(UnityAction fun)
        {
            MakeEntity();
            _mainBehaviour.RemoveLateUpdateListener(fun);
        }

        #endregion

        #region Unity Events 注入

        /// <summary>
        /// 为给外部提供的Destroy注册事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void AddDestroyListener(UnityAction fun)
        {
            MakeEntity();
            _mainBehaviour.AddDestroyListener(fun);
        }

        /// <summary>
        /// 为给外部提供的Destroy反注册事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void RemoveDestroyListener(UnityAction fun)
        {
            MakeEntity();
            _mainBehaviour.RemoveDestroyListener(fun);
        }

        /// <summary>
        /// 为给外部提供的OnDrawGizmos注册事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void AddOnDrawGizmosListener(UnityAction fun)
        {
            MakeEntity();
            _mainBehaviour.AddOnDrawGizmosListener(fun);
        }

        /// <summary>
        /// 为给外部提供的OnDrawGizmos反注册事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void RemoveOnDrawGizmosListener(UnityAction fun)
        {
            MakeEntity();
            _mainBehaviour.RemoveOnDrawGizmosListener(fun);
        }

        /// <summary>
        /// 为给外部提供的OnApplicationPause注册事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void AddOnApplicationPauseListener(UnityAction<bool> fun)
        {
            MakeEntity();
            _mainBehaviour.AddOnApplicationPauseListener(fun);
        }

        /// <summary>
        /// 为给外部提供的OnApplicationFocus注册事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void AddOnApplicationFocusListener(UnityAction<bool> fun)
        {
            MakeEntity();
            _mainBehaviour.AddOnApplicationFocusListener(fun);
        }
        
        /// <summary>
        /// 为给外部提供的OnApplicationQuit注册事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void AddApplicationQuitListener(UnityAction fun)
        { 
            MakeEntity();
            _mainBehaviour.AddApplicationQuitListener(fun);
        }

        /// <summary>
        /// 为给外部提供的OnApplicationPause反注册事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void RemoveOnApplicationPauseListener(UnityAction<bool> fun)
        {
            MakeEntity();
            _mainBehaviour.RemoveOnApplicationPauseListener(fun);
        }

        /// <summary>
        /// 为给外部提供的OnApplicationFocus反注册事件。
        /// </summary>
        /// <param name="fun"></param>
        public static void RemoveOnApplicationFocusListener(UnityAction<bool> fun)
        {
            MakeEntity();
            _mainBehaviour.RemoveOnApplicationFocusListener(fun);
        }
        #endregion

        /// <summary>
        /// 释放Behaviour生命周期。
        /// </summary>
        public static void Shutdown()
        {
            if (_mainBehaviour != null)
            {
                _mainBehaviour.Release();
            }

            if (_entity != null)
            {
                Object.Destroy(_entity);
            }

            _entity = null;
        }

        private static void MakeEntity()
        {
            if (_entity != null)
                return;

            _entity = new GameObject("[EngineMain]");
            _entity.SetActive(true);
            Object.DontDestroyOnLoad(_entity);
            _mainBehaviour = _entity.AddComponent<MainBehaviour>();
            
            GameCore.Log.Logger.Info("Unity Util Make Engine Entity");
        }

        private class MainBehaviour : MonoBehaviour
        {
            private event UnityAction UpdateEvent;
            private event UnityAction FixedUpdateEvent;
            private event UnityAction LateUpdateEvent;
            private event UnityAction DestroyEvent;
            private event UnityAction OnDrawGizmosEvent;
            private event UnityAction OnApplicationQuitEvent;
            private event UnityAction<bool> OnApplicationFocusEvent;
            private event UnityAction<bool> OnApplicationPauseEvent;

            void Update()
            {
                if (UpdateEvent != null)
                {
                    UpdateEvent();
                }
            }

            void FixedUpdate()
            {
                if (FixedUpdateEvent != null)
                {
                    FixedUpdateEvent();
                }
            }

            void LateUpdate()
            {
                if (LateUpdateEvent != null)
                {
                    LateUpdateEvent();
                }
            }

            private void OnDestroy()
            {
                if (DestroyEvent != null)
                {
                    DestroyEvent();
                }
            }

            private void OnDrawGizmos()
            {
                if (OnDrawGizmosEvent != null)
                {
                    OnDrawGizmosEvent();
                }
            }

            private void OnApplicationPause(bool pauseStatus)
            {
                if (OnApplicationPauseEvent != null)
                {
                    OnApplicationPauseEvent(pauseStatus);
                }
            }

            private void OnApplicationQuit()
            {
                GameCore.Log.Logger.Info("Engine mono application quit");
                if (OnApplicationQuitEvent != null)
                {
                    OnApplicationQuitEvent();
                }
                
            }

            private void OnApplicationFocus(bool hasFocus)
            {
                // GameCore.Log.Logger.Debug("Engine mono application focus");
                if (OnApplicationFocusEvent != null)
                {
                    OnApplicationFocusEvent(hasFocus);
                }
            }

            public void AddLateUpdateListener(UnityAction fun)
            {
                LateUpdateEvent += fun;
            }

            public void RemoveLateUpdateListener(UnityAction fun)
            {
                LateUpdateEvent -= fun;
            }

            public void AddFixedUpdateListener(UnityAction fun)
            {
                FixedUpdateEvent += fun;
            }

            public void RemoveFixedUpdateListener(UnityAction fun)
            {
                FixedUpdateEvent -= fun;
            }

            public void AddUpdateListener(UnityAction fun)
            {
                UpdateEvent += fun;
            }

            public void RemoveUpdateListener(UnityAction fun)
            {
                UpdateEvent -= fun;
            }

            public void AddDestroyListener(UnityAction fun)
            {
                DestroyEvent += fun;
            }

            public void RemoveDestroyListener(UnityAction fun)
            {
                DestroyEvent -= fun;
            }

            public void AddOnDrawGizmosListener(UnityAction fun)
            {
                OnDrawGizmosEvent += fun;
            }

            public void RemoveOnDrawGizmosListener(UnityAction fun)
            {
                OnDrawGizmosEvent -= fun;
            }

            public void AddOnApplicationPauseListener(UnityAction<bool> fun)
            {
                OnApplicationPauseEvent += fun;
            }

            public void RemoveOnApplicationPauseListener(UnityAction<bool> fun)
            {
                OnApplicationPauseEvent -= fun;
            }


            public void AddOnApplicationFocusListener(UnityAction<bool> fun)
            {
                OnApplicationFocusEvent += fun;
            }

            public void RemoveOnApplicationFocusListener(UnityAction<bool> fun)
            {
                OnApplicationFocusEvent -= fun;
            }
            
            public void AddApplicationQuitListener(UnityAction fun)
            {
                OnApplicationQuitEvent += fun;
            }
            
            public void Release()
            {
                UpdateEvent = null;
                FixedUpdateEvent = null;
                LateUpdateEvent = null;
                OnDrawGizmosEvent = null;
                DestroyEvent = null;
                OnApplicationPauseEvent = null;
                OnApplicationQuitEvent = null;
            }
        }
    }
}