using System;

namespace GameConfig
{
    public abstract class ConfigBase : IDisposable
    {
        private volatile bool _isInitialized;
        private readonly object _initLock = new object();

        public ConfigBase()
        {
            EnsureInitialized();
        }
        
        public abstract void Dispose();

        protected abstract void ConstructConfig();

        protected virtual void OnDispose()
        {
        }

        protected virtual void OnInitialized()
        {
        }

        protected void TackUsage()
        {
            ConfigMemoryPool.MarkUsage(this);
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;

            lock (_initLock)
            {
                if (_isInitialized) return;

                ConstructConfig();
                OnInitialized();
                _isInitialized = true;
            }
        }
    }
}