using System;
using System.Collections.Generic;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 游戏依赖注入容器
    /// </summary>
    public class GameDIContainer
    {
        private readonly Dictionary<Type, object> _singletons = new();
        private readonly Dictionary<Type, Func<object>> _factories = new();
        private readonly Dictionary<Type, Type> _mappings = new();

        public TImplementation RegisterSingleton<TInterface, TImplementation>() where TImplementation : TInterface
        {
            var type = typeof(TInterface);
            if (_singletons.TryGetValue(type, out var singleton))
                return (TImplementation)singleton;
            var instance = Activator.CreateInstance<TImplementation>();
            _singletons[typeof(TInterface)] = instance;
            return instance;
        }

        public void RegisterSingleton<TInterface>(TInterface instance)
        {
            _singletons[typeof(TInterface)] = instance;
        }

        public void RegisterTransient<TInterface, TImplementation>() where TImplementation : TInterface
        {
            _mappings[typeof(TInterface)] = typeof(TImplementation);
        }

        public void RegisterFactory<TInterface>(Func<TInterface> factory)
        {
            _factories[typeof(TInterface)] = () => factory();
        }

        public T Resolve<T>()
        {
            var type = typeof(T);

            // 1. 检查单例
            if (_singletons.TryGetValue(type, out var singleton))
                return (T)singleton;

            // 2. 检查工厂
            if (_factories.TryGetValue(type, out var factory))
                return (T)factory();

            // 3. 检查映射关系
            if (_mappings.TryGetValue(type, out var implType))
                return (T)Activator.CreateInstance(implType);

            throw new InvalidOperationException($"未注册的类型: {type.Name}");
        }
    }
}