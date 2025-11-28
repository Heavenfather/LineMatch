using System;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public interface IBoard
    {
        /// <summary>
        /// 关卡数据
        /// </summary>
        LevelData LevelData { get; }

        /// <summary>
        /// 获取棋盘单元格实体
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        int this[int x, int y] { get; }

        /// <summary>
        /// 获取棋盘单元格
        /// </summary>
        /// <param name="index">单元格索引</param>
        /// <returns>单元格</returns>
        int this[int index] { get; }

        int this[Vector2Int position] { get; }

        /// <summary>
        /// 获取棋盘单元格是否合法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool IsValid(int x, int y);

        /// <summary>
        /// 棋盘宽度
        /// </summary>
        int Width { get; }

        /// <summary>
        /// 棋盘高度
        /// </summary>
        int Height { get; }

        /// <summary>
        /// 初始化棋盘
        /// </summary>
        void Initialize(LevelData levelData);

        /// <summary>
        /// 遍历棋盘实体
        /// </summary>
        /// <param name="func"></param>
        void ForeachBoard(Action<int> func);

        /// <summary>
        /// 尝试获取棋盘单元格元素实体
        /// </summary>
        /// <returns></returns>
        bool TryGetGridEntity(int x, int y, out int entity,bool includeBlank = false);

        /// <summary>
        /// 重置棋盘
        /// 重刷整个棋盘数据
        /// </summary>
        /// <param name="levelData">关卡数据</param>
        void Reset(LevelData levelData);

        /// <summary>
        /// 添加棋盘单元格元素实体
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="entity"></param>
        /// <param name="isBlank">是否空格子</param>
        void RegisterGridEntity(int x, int y, int entity,bool isBlank);

        /// <summary>
        /// 注册棋盘单元格视图实例
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="instance"></param>
        void RegisterViewInstance(int x, int y, GameObject instance);

        /// <summary>
        /// 获取棋盘单元格视图实例
        /// 提供快速获取实例的接口
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        GameObject GetGridInstance(int x, int y);

        /// <summary>
        /// 清空棋盘
        /// </summary>
        void Clear();
    }
}