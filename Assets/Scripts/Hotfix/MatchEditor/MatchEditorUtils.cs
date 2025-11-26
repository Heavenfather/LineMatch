#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using GameCore.FileHelper;
using Hotfix.Define;
using HotfixCore.Module;
using HotfixLogic.Match;
using LitJson;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic
{
    public enum ElementFillState
    {
        Scroll,
        Selected,
        Delete
    }

    public static class MatchEditorUtils
    {
        public static async UniTask<Sprite> GetElementIcon(int elementId)
        {
            ElementMapDB mapDB = ConfigMemoryPool.Get<ElementMapDB>();
            var config = mapDB[elementId];
            return await LoadElementIcon(config);
        }

        private static async UniTask<Sprite> LoadElementIcon(ElementMap config)
        {
            string iconAddress = config.address_icon;
            if (string.IsNullOrEmpty(iconAddress))
                iconAddress = config.address;
            string location = $"{MatchConst.SpritesAddressBase}/{iconAddress}";
            return await G.ResourceModule.LoadAssetAsync<Sprite>(location);
        }

        public static bool IsFillElement(int elementId)
        {
            if (elementId == MatchEditorConst.WhiteElementId || elementId == MatchEditorConst.RectangleElementId)
                return false;
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[elementId];
            return config.elementType == ElementType.TargetBlock ||
                   config.elementType == ElementType.VerticalExpand ||
                   config.elementType == ElementType.FixPosExpand ||
                   config.elementType == ElementType.FixedGridTargetBlock;
        }

        public static List<Vector2Int> GetFillElementCoords(int x, int y, int width, int height,
            ElementDirection direction)
        {
            List<Vector2Int> coords = new List<Vector2Int>();
            if (direction == ElementDirection.None)
            {
                //默认是从左上角开始 从左到右 从上到下
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int posX = x + i;
                        int posY = y + j;
                        coords.Add(new Vector2Int(posX, posY));
                    }
                }
            }
            else if (direction == ElementDirection.Up || direction == ElementDirection.Down)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int posX = x + i;
                        int posY = y + j;
                        if (direction == ElementDirection.Up)
                            posY = y - j;
                        coords.Add(new Vector2Int(posX, posY));
                    }
                }
            }
            else
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int posX = x + j;
                        if (direction == ElementDirection.Left)
                            posX = x - j;
                        int posY = y + i;
                        coords.Add(new Vector2Int(posX, posY));
                    }
                }
            }

            return coords;
        }

        public static int GetOrSetLastEditLevel(int setLevel)
        {
            if (setLevel > 0)
            {
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetInt(MatchEditorConst.LastEditLevelId, setLevel);
#else
                PlayerPrefs.SetInt(MatchEditorConst.LastEditLevelId, setLevel);

#endif
                return setLevel;
            }
#if UNITY_EDITOR
            return UnityEditor.EditorPrefs.GetInt(MatchEditorConst.LastEditLevelId, 1);
#else
            return PlayerPrefs.GetInt(MatchEditorConst.LastEditLevelId, 1);
#endif
        }

        public static void SaveLevelData(string fileName, LevelData levelData)
        {
            if (!levelData.Validate())
            {
                return;
            }

            string json = JsonMapper.ToJson(levelData);
            FileHelper.WriteString(fileName, json);
            Logger.Info("保存成功!");
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public static void ResizeLevelGrid(ref LevelData levelData, int newRows, int newColumns)
        {
            // 参数校验
            newColumns = Math.Max(newColumns, 0);
            newRows = Math.Max(newRows, 0);

            // 特殊情况处理：清空网格
            if (newColumns == 0 || newRows == 0)
            {
                levelData.grid = new LevelElement[0][];
                return;
            }

            // 创建新列容器
            LevelElement[][] newGrid = new LevelElement[newColumns][];

            // 计算需要复制的列数
            int columnsToCopy = (levelData.grid != null) ? Math.Min(levelData.grid.Length, newColumns) : 0;

            // 处理现有列
            for (int col = 0; col < columnsToCopy; col++)
            {
                LevelElement[] oldColumn = levelData.grid[col];
                newGrid[col] = new LevelElement[newRows];

                // 复制旧行数据
                if (oldColumn != null)
                {
                    int rowsToCopy = Math.Min(oldColumn.Length, newRows);
                    Array.Copy(oldColumn, newGrid[col], rowsToCopy);

                    // 初始化新增行
                    if (newRows > oldColumn.Length)
                    {
                        for (int row = oldColumn.Length; row < newRows; row++)
                        {
                            newGrid[col][row] = new LevelElement()
                                { elements = new List<GridElement>(), isWhite = false }; // 初始化新元素
                        }
                    }
                }
                else
                {
                    for (int row = 0; row < newRows; row++)
                    {
                        newGrid[col][row] = new LevelElement()
                            { elements = new List<GridElement>(), isWhite = false };
                    }
                }
            }

            // 处理新增列（如果新列数 > 原列数）
            for (int col = columnsToCopy; col < newColumns; col++)
            {
                newGrid[col] = new LevelElement[newRows];

                for (int row = 0; row < newRows; row++)
                {
                    newGrid[col][row] = new LevelElement()
                        { elements = new List<GridElement>(), isWhite = false };
                }
            }

            levelData.grid = newGrid;
        }
    }
}
#endif