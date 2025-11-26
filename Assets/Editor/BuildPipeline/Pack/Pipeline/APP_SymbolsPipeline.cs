using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameEditor.BuildPipeline
{
    public class APP_SymbolsPipeline : IBuildPipeline
    {
        public EPipeLine PipeLine => EPipeLine.APP_Symbols;

        public void Execute(BuildContext context)
        {
            string sym = MergeDefines(context);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(context.TargetGroup, sym);
        }

        private string MergeDefines(BuildContext context)
        {
            string retVal = context.OriginalSymbols;

            // 添加 EnableSymbols 中不存在于 OriginalSymbols 的宏定义
            if (context.EnableSymbols.Length > 0)
            {
                var originalSymbolsList = new HashSet<string>(retVal.Split(';'));
                var symbolsToAdd = new List<string>();

                foreach (var symbol in context.EnableSymbols)
                {
                    if (!originalSymbolsList.Contains(symbol))
                    {
                        symbolsToAdd.Add(symbol);
                    }
                }

                if (symbolsToAdd.Count > 0)
                {
                    if (!string.IsNullOrEmpty(retVal))
                    {
                        retVal = retVal + ";" + string.Join(";", symbolsToAdd);
                    }
                    else
                    {
                        retVal = string.Join(";", symbolsToAdd);
                    }
                }
            }

            // 移除 DisableSymbols 中指定的宏定义
            if (context.DisableSymbols.Length > 0)
            {
                var symbolsList = new List<string>(retVal.Split(';'));

                foreach (var symbol in context.DisableSymbols)
                {
                    symbolsList.RemoveAll(s => s == symbol);
                }

                retVal = string.Join(";", symbolsList);
            }
            Debug.Log($"{context.Target} 宏定义处理完成：{retVal}");

            return retVal;
        }
    }
}