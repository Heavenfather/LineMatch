using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 场景视图接口
    /// 定义场景视图的基本操作，比如加载场景，卸载场景，获取场景元素等
    /// </summary>
    public interface ISceneView
    {
        /// <summary>
        /// 场景资源寻址地址
        /// </summary>
        string SceneLocation { get; }

        /// <summary>
        /// 获取当前场景
        /// </summary>
        Scene Scene { get; }

        /// <summary>
        /// 获取当前场景根节点对象物体
        /// </summary>
        List<GameObject> SceneAllObjects { get; }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <returns></returns>
        UniTask LoadScene();

        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="callback">卸载完成回调</param>
        /// <returns></returns>
        void UnloadScene(Action callback = null);

        /// <summary>
        /// 获取场景根节点对象物体
        /// </summary>
        /// <returns></returns>
        GameObject GetSceneRoot(string nodeName);
        
        /// <summary>
        /// 获取场景根节点对象物体
        /// </summary>
        /// <returns></returns>
        Transform GetSceneRootTransform(string nodeName);
        
        /// <summary>
        /// 获取场景指定根节点下子对象物体
        /// </summary>
        /// <param name="rootName"></param>
        /// <param name="nodePath"></param>
        /// <returns></returns>
        Transform GetSceneRootTransform(string rootName,string nodePath);
        
        /// <summary>
        /// 获取场景根节点对象物体组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns></returns>
        T GetSceneRootComponent<T>(string rootName, string nodePath) where T : Component;
    }
}