using Cysharp.Threading.Tasks;
using GameConfig;
using UnityEngine;

namespace HotfixLogic.Match
{
    /// <summary>
    /// 消除引导的特殊处理
    /// </summary>
    public static class MatchGuideUtil
    {
        public static bool ProcessNextGuide(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
                return true;
            string[] args = parameter.Split("|");
            if (args.Length > 1)
            {
                if (int.TryParse(args[1], out int nextId))
                {
                    GuideConfigDB db = ConfigMemoryPool.Get<GuideConfigDB>();
                    ref readonly GuideConfig config = ref db[nextId];
                    if (string.IsNullOrEmpty(config.guideParameters) && !string.IsNullOrEmpty(config.guideParameters2))
                    {
                        if (CheckTouchItem(config.guideParameters2,out var _))
                        {
                            GuideManager.Instance.StartGuide(nextId).Forget();
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    string[] coords = config.guideParameters.Split("|");
                    Vector2Int[] gridCoords = new Vector2Int[coords.Length];
                    for (int i = 0; i < coords.Length; i++)
                    {
                        string[] pos = coords[i].Split(",");
                        Vector2Int coord = new Vector2Int(int.Parse(pos[0]), int.Parse(pos[1]));
                        gridCoords[i] = coord;
                    }

                    var elements = ElementSystem.Instance.GetGridElements(gridCoords[0], true);
                    bool shouldGuide = true;
                    if (ElementSystem.Instance.TryGetBaseElement(elements, out int index, true))
                    {
                        var firstElement = elements[index];
                        bool isSpecial = config.guideParameters2 == "CheckItem";
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
                                if (ElementSystem.Instance.TryGetBaseElement(coordElements, out int index2,true))
                                {
                                    if(!ElementSystem.Instance.IsSpecialElement(coordElements[index2].Data.ElementType))
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
                    }
                }
            }
            return true;
        }

        public static bool CheckTouchItem(string parameter2,out Vector2Int coord)
        {
            coord = new Vector2Int(-1, -1);
            if (string.IsNullOrEmpty(parameter2))
                return false;
            if (int.TryParse(parameter2, out int elementId))
            {
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
            }
            return false;
        }

        public static string[] GetLevelGuideFingerCoords(int levelId)
        {
            if (levelId == 1)
            {
                return new[] { "0,0", "1,0" };
            }

            if (levelId == 2)
            {
                return new[] { "0,0", "1,0", "1,1", "2,1", "2,2", "3,2", "3,3" };
            }

            if (levelId == 4)
            {
                return new[] { "1,1", "2,1" };
            }

            if (levelId == 5)
            {
                return new[] { "2,0", "3,0", "3,1", "2,1", "2,0" };
            }
            
            if (levelId == 6)
            {
                return new[] { "1,0", "1,1", "0,1" };
            }

            return null;
        }
    }
}