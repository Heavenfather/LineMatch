using System;

namespace GameCore.Singleton
{
    public class LazySingleton<T> where T : LazySingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Activator.CreateInstance<T>();
                    _instance.OnInitialized();
                }

                return _instance;
            }
        }

        protected virtual void OnInitialized()
        {
        }
    }
}