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

        public int Width { get; private set; }

        public int Height { get; private set; }

        public void Initialize(LevelData levelData)
        {
            Reset(levelData);
        }
        
        public void RegisterGridEntity(int x, int y, int entity)
        {
            this[x, y] = entity;
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