using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using HotfixCore.MemoryPool;
using UnityEngine;

namespace HotfixLogic.Match
{
    public enum ElementState
    {
        Using,
        Destroying,
        CanRecycle,
        Recycle
    }
    
    public interface IElementItem : IMemory
    {
        /// <summary>
        /// 元素状态
        /// </summary>
        ElementState State { get; set; }
        
        /// <summary>
        /// 元素持有的实体对象
        /// </summary>
        GameObject GameObject { get; set; }
        
        /// <summary>
        /// 元素持有数据
        /// </summary>
        ElementItemData Data { get; set; }
        
        /// <summary>
        /// 只会在创建实体对象时创建一次
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        void Initialize(Transform parent,ElementItemData data);
        
        /// <summary>
        /// 设置元素父对象
        /// </summary>
        /// <param name="parent"></param>
        void SetParent(Transform parent);

        /// <summary>
        /// 执行选中元素操作
        /// </summary>
        void DoSelect();
        
        /// <summary>
        /// 执行释放选中元素
        /// </summary>
        void DoDeselect();

        /// <summary>
        /// 执行移动
        /// </summary>
        void DoMove(float delayTime = 0, Ease ease = Ease.OutBounce);

        /// <summary>
        /// 游戏对象销毁逻辑处理
        /// </summary>
        /// <returns></returns>
        bool DoDestroy(ElementDestroyContext context);

        /// <summary>
        /// 销毁逻辑处理完成
        /// </summary>
        UniTask AfterDestroy(ElementDestroyContext context,bool immediate,Action callback);
    }
}