using UnityEngine;

namespace GameCore.Singleton
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = GameObject.Find($"[{typeof(T).Name}]");
                    if (go == null)
                    {
                        go = new GameObject($"[{typeof(T).Name}]");
                        go.transform.SetParent(GameObject.Find("MonoSingleton").transform);
                    }

                    _instance = go.AddComponent<T>();
                }
                return _instance;
            }
        }

        private bool _isDestroyed;
        
        public void Awake()
        {
            _isDestroyed = false;
            OnAwake();
        }

        void Start()
        {
            if (null != Instance)
                OnStart();
        }

        private void Update()
        {
            if (Instance != null)
                OnUpdate();
        }

        void OnDestroy()
        {
            if (null != Instance)
            {
                OnQuit();
                _instance = null;
                _isDestroyed = true;
            }
        }

        void OnApplicationQuit()
        {
            if (null != Instance)
            {
                if (this.gameObject != null)
                {
                    GameObject.DestroyImmediate(this.gameObject);
                }
                OnQuit();
                _instance = null;
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (null != Instance)
                OnPause(pauseStatus);
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (null != Instance)
                OnFocus(hasFocus);
        }

        public bool IsDestroyed => _isDestroyed;
        
        protected virtual void OnAwake()
        {
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnPause(bool pauseStatus)
        {
        }

        protected virtual void OnQuit()
        {
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnFocus(bool hasFocus)
        {
        }
    }
}