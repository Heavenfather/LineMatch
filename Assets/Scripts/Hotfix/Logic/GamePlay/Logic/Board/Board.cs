using System;
using System.Collections.Generic;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 棋盘实现
    /// 作为数据中转站，存储棋盘数据，提供访问接口。不做逻辑处理
    /// </summary>
    public class Board : IBoard
    {
        private int[] _boardEntities;
        private GameObject[] _gameObjects;
        private Dictionary<int, bool> _idx2HaveEntity = new Dictionary<int, bool>(); //索引映射该实体是否为空格子，true=空格子，false=非空格子

        public bool IsBoardDirty { get; set; }
        
        public LevelData LevelData { get; private set; }

        public int this[int x, int y]
        {
            get => this[ParseIndex(x, y)];
            private set => this[ParseIndex(x, y)] = value;
        }

        public int this[Vector2Int position]
        {
            get => this[position.x, position.y];
            private set => this[position.x, position.y] = value;
        }

        public int this[int index]
        {
            get => _boardEntities[index];
            private set => _boardEntities[index] = value;
        }

        public bool IsValid(int x, int y)
        {
            int index = ParseIndex(x, y);
            if (_idx2HaveEntity.ContainsKey(index))
                return _idx2HaveEntity[index] == false;
            return false;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public void Initialize(LevelData levelData)
        {
            Reset(levelData);
        }

        public void ForeachBoard(Action<int> func)
        {
            for (int i = 0; i < _boardEntities.Length; i++)
            {
                func(_boardEntities[i]);
            }
        }

        public bool TryGetGridEntity(int x, int y, out int entity, bool includeBlank = false)
        {
            entity = -1;
            int index = ParseIndex(x, y);
            if (_idx2HaveEntity.TryGetValue(index, out var isBlank))
            {
                if (!includeBlank && isBlank)
                {
                    return false;
                }

                entity = this[index];
                return true;
            }

            return false;
        }

        public void RegisterGridEntity(int x, int y, int entity, bool isBlank)
        {
            int index = ParseIndex(x, y);
            this[index] = entity;
            _idx2HaveEntity[index] = isBlank;
        }

        public void RegisterViewInstance(int x, int y, GameObject instance)
        {
            _gameObjects[ParseIndex(x, y)] = instance;
        }

        public GameObject GetGridInstance(int x, int y)
        {
            return _gameObjects[ParseIndex(x, y)];
        }

        public void Reset(LevelData levelData)
        {
            this.LevelData = levelData;
            this.Width = levelData.gridCol;
            this.Height = levelData.gridRow;
            _boardEntities = new int[Width * Height];
            _gameObjects = new GameObject[Width * Height];
        }

        public void Clear()
        {
            _boardEntities = null;
            _gameObjects = null;
            _idx2HaveEntity.Clear();
        }

        public int ParseIndex(int x, int y)
        {
            return x * Height + y;
        }

        public Vector2Int ParsePosition(int index)
        {
            return new Vector2Int(index / Height, index % Height);
        }
    }
}