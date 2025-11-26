using System;
using System.Collections.Generic;

namespace HotfixCore.Utils
{
    public static class AssemblyUtil
    {
        private static readonly System.Reflection.Assembly[] _assemblies = null;

        private static readonly Dictionary<string, Type> _cachedTypes =
            new Dictionary<string, Type>(StringComparer.Ordinal);

        static AssemblyUtil()
        {
            _assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// 获取已加载的程序集。
        /// </summary>
        /// <returns>已加载的程序集。</returns>
        public static System.Reflection.Assembly[] GetAssemblies()
        {
            return _assemblies;
        }

        /// <summary>
        /// 获取已加载程序集
        /// </summary>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns></returns>
        public static System.Reflection.Assembly GetAssembly(string assemblyName)
        {
            for (int i = 0; i < _assemblies.Length; i++)
            {
                if (assemblyName == _assemblies[i].GetName().Name)
                    return _assemblies[i];
            }

            return null;
        }

        /// <summary>
        /// 获取已加载的程序集中的所有类型。
        /// </summary>
        /// <returns>已加载的程序集中的所有类型。</returns>
        public static Type[] GetTypes()
        {
            List<Type> results = new List<Type>();
            foreach (System.Reflection.Assembly assembly in _assemblies)
            {
                results.AddRange(assembly.GetTypes());
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取已加载的程序集中的所有类型。
        /// </summary>
        /// <param name="results">已加载的程序集中的所有类型。</param>
        public static void GetTypes(List<Type> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (System.Reflection.Assembly assembly in _assemblies)
            {
                results.AddRange(assembly.GetTypes());
            }
        }

        /// <summary>
        /// 获取已加载的程序集中的指定类型。
        /// </summary>
        /// <param name="typeName">要获取的类型名。</param>
        /// <returns>已加载的程序集中的指定类型。</returns>
        public static Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new Exception("Type name is invalid.");
            }

            Type type = null;
            if (_cachedTypes.TryGetValue(typeName, out type))
            {
                return type;
            }

            type = Type.GetType(typeName);
            if (type != null)
            {
                _cachedTypes.Add(typeName, type);
                return type;
            }

            foreach (System.Reflection.Assembly assembly in _assemblies)
            {
                type = Type.GetType(string.Format("{0}, {1}", typeName, assembly.FullName));
                if (type != null)
                {
                    _cachedTypes.Add(typeName, type);
                    return type;
                }
            }

            return null;
        }
    }
}