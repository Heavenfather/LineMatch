using Cysharp.Threading.Tasks;
using GameConfig;
using HotfixCore.MemoryPool;
using HotfixLogic;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 引导工具类
    /// 提供引导相关的辅助方法
    /// </summary>
    public static class GuideUtil
    {
        /// <summary>
        /// 检查是否需要处理下一个引导
        /// </summary>
        public static bool ProcessNextGuide(string parameter, EcsWorld world)
        {
            if (string.IsNullOrEmpty(parameter))
                return true;
            
            string[] args = parameter.Split("|");
            if (args.Length <= 1) return true;
            
            if (!int.TryParse(args[1], out int nextId)) return true;
            
            GuideConfigDB db = ConfigMemoryPool.Get<GuideConfigDB>();
            ref readonly GuideConfig config = ref db[nextId];
            
            // 检查是否是触摸道具引导
            if (string.IsNullOrEmpty(config.guideParameters) && !string.IsNullOrEmpty(config.guideParameters2))
            {
                if (CheckTouchItem(config.guideParameters2, world, out var _))
                {
                    GuideManager.Instance.StartGuide(nextId).Forget();
                    return false;
                }
                return true;
            }
            
            // 检查路径引导
            string[] coords = config.guideParameters.Split("|");
            Vector2Int[] gridCoords = new Vector2Int[coords.Length];
            for (int i = 0; i < coords.Length; i++)
            {
                string[] pos = coords[i].Split(",");
                gridCoords[i] = new Vector2Int(int.Parse(pos[0]), int.Parse(pos[1]));
            }
            
            // 检查第一个格子的元素
            var elements = ElementSystem.Instance.GetGridElements(gridCoords[0], true);
            if (!ElementSystem.Instance.TryGetBaseElement(elements, out int index, true))
                return true;
            
            var firstElement = elements[index];
            bool isSpecial = config.guideParameters2 == "CheckItem";
            bool shouldGuide = true;
            
            for (int i = 0; i < gridCoords.Length - 1; i++)
            {
                var coordElements = ElementSystem.Instance.GetGridElements(gridCoords[i], true);
                if (!isSpecial)
                {
                    if (ElementSystem.Instance.TryGetBaseElement(coordElements, out int index2))
                    {
                        if (coordElements[index2].Data.ConfigId != firstElement.Data.ConfigId)
                        {
                            shouldGuide = false;
                            break;
                        }
                    }
                }
                else
                {
                    if (ElementSystem.Instance.TryGetBaseElement(coordElements, out int index2, true))
                    {
                        if (!ElementSystem.Instance.IsSpecialElement(coordElements[index2].Data.ElementType))
                        {
                            shouldGuide = false;
                            break;
                        }
                    }
                }
            }
            
            if (shouldGuide)
            {
                GuideManager.Instance.StartGuide(nextId).Forget();
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查触摸道具引导
        /// </summary>
        public static bool CheckTouchItem(string parameter2, EcsWorld world, out Vector2Int coord)
        {
            coord = new Vector2Int(-1, -1);
            if (string.IsNullOrEmpty(parameter2))
                return false;
            
            if (!int.TryParse(parameter2, out int elementId))
                return false;
            
            var allGridElements = ElementSystem.Instance.GridElements.Values;
            foreach (var elements in allGridElements)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Data.ConfigId == elementId)
                    {
                        coord = elements[i].Data.GridPos;
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取引导关卡的手指路径坐标
        /// </summary>
        public static string[] GetLevelGuideFingerCoords(int levelId)
        {
            return levelId switch
            {
                1 => new[] { "0,0", "1,0" },
                2 => new[] { "0,0", "1,0", "1,1", "2,1", "2,2", "3,2", "3,3" },
                4 => new[] { "1,1", "2,1" },
                5 => new[] { "2,0", "3,0", "3,1", "2,1", "2,0" },
                6 => new[] { "1,0", "1,1", "0,1" },
                _ => null
            };
        }
    }
}
