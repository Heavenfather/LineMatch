using System;
using System.Collections.Generic;
using System.Reflection;
using GameCore.Log;
using HotfixCore.Utils;

namespace HotfixCore.MVC
{
    public class MVCConfig
    {
        private static Dictionary<string, MVCDefine> _metas = new Dictionary<string, MVCDefine>(200);

        public static void BuildMVCConfigMap()
        {
            _metas.Clear();
            Assembly hotfixAssembly = null;
            hotfixAssembly = AssemblyUtil.GetAssembly("GameHotfix");
            if (hotfixAssembly == null)
            {
                Logger.Error("GameHotfix assembly not found!");
                return;
            }

            foreach (Type type in hotfixAssembly.GetTypes())
            {
                var attr = type.GetCustomAttribute<MVCDefine>(false);
                if (attr == null) continue;

                var mvcName = attr.MVCName;
                if (string.IsNullOrEmpty(mvcName))
                {
                    Logger.Error($"MVCName不能为空: {type.FullName}");
                    continue;
                }

                if (_metas.ContainsKey(mvcName))
                {
                    Logger.Error($"重复的MVCName定义: {mvcName} (类型: {type.FullName})");
                    continue;
                }

                _metas.Add(mvcName, attr);
            }

            foreach (var pair in _metas)
            {
                if (!string.IsNullOrEmpty(pair.Value.Parent) &&
                    !_metas.ContainsKey(pair.Value.Parent))
                {
                    Logger.Error($"无效的Parent定义: {pair.Value.Parent} (模块: {pair.Key})");
                }
            }
        }

        public static MVCDefine GetMVCDefine(string mvcName)
        {
            if (_metas.TryGetValue(mvcName, out var meta))
                return meta;
            Logger.Error($"Can't find MVCDefine for {mvcName}");
            return null;
        }
    }
}