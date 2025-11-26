using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using HotfixCore.Extensions;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;

/// <summary>
/// 元素池子本身不会缓存游戏对象，只负责管理，池子对象在ResourceModule中缓存并释放
/// </summary>
public class ElementObjectPool : MonoBehaviour
{
    private class PoolData
    {
        public string location;
        public int maxSize;
        public bool allowExpand;
        public Queue<GameObject> pool = new Queue<GameObject>();
        public int activeCount;
        public Transform poolRoot; // 专用父节点
    }


    private static ElementObjectPool _instance;

    public static ElementObjectPool Instance
    {
        get { return _instance; }
    }

    private Dictionary<string, PoolData> _pools = new Dictionary<string, PoolData>();
    private Dictionary<GameObject, string> _instanceToKeyMap = new Dictionary<GameObject, string>();

    void Awake()
    {
        _instance = this;
    }

    /// <summary>
    /// 动态创建对象池
    /// </summary>
    /// <param name="poolKey">自定义唯一标识</param>
    /// <param name="location">资源预加载地址</param>
    /// <param name="callback">创建池子成功回调</param>
    /// <param name="prewarmCount">预热数量</param>
    /// <param name="maxSize">最大容量</param>
    public async UniTask CreatePool(string poolKey, string location, Action callback,
        int prewarmCount = 20,
        int maxSize = 100)
    {
        if (_pools.TryGetValue(poolKey, out var existPool))
        {
            callback?.Invoke();
            return;
        }

        var poolData = new PoolData
        {
            location = location,
            maxSize = maxSize,
            allowExpand = true,
            poolRoot = new GameObject($"[Pool-{poolKey}]").transform
        };

        poolData.poolRoot.SetParent(transform);
        poolData.poolRoot.transform.localPosition = Vector3.zero;

        var firstObject = await G.ResourceModule.LoadGameObjectAsync(location, poolData.poolRoot);
        if (firstObject == null)
        {
            callback?.Invoke();
            return;
        }

        firstObject.SetVisible(false);
        firstObject.transform.localPosition = Vector3.zero;
        poolData.pool.Enqueue(firstObject);
        // 立即预热
        for (int i = 0; i < prewarmCount - 1; i++)
        {
            var obj = CreatePooledObject(poolData);
            poolData.pool.Enqueue(obj);
        }

        _pools.Add(poolKey, poolData);
        callback?.Invoke();
    }

    public GameObject Spawn(string poolKey)
    {
        if (!_pools.TryGetValue(poolKey, out PoolData poolData))
        {
            Logger.Error($"对象池 {poolKey} 未注册");
            return null;
        }

        GameObject obj = GetAvailableObject(poolData);
        if (obj == null) return null;
        // Logger.Debug($"对象池 {poolKey} 获取对象: {obj.name}");

        ResetObjectState(obj);
        obj.SetVisible(true);
        poolData.activeCount++;

        _instanceToKeyMap.Add(obj, poolKey);
        return obj;
    }

    public void Recycle(GameObject obj)
    {
        if (obj == null || obj.transform == null)
            return;
        if (!_instanceToKeyMap.TryGetValue(obj, out string poolKey))
        {
            Logger.Warning("尝试回收未池化的对象: " + obj.name);
            Destroy(obj);
            return;
        }

        if (!_pools.TryGetValue(poolKey, out PoolData poolData))
        {
            Destroy(obj);
            return;
        }

        ResetObjectState(obj);
        obj.transform.SetParent(poolData.poolRoot);
        obj.SetVisible(false);
        poolData.pool.Enqueue(obj);
        poolData.activeCount--;
        _instanceToKeyMap.Remove(obj);
    }

    private GameObject GetAvailableObject(PoolData poolData)
    {
        // 优先从池中获取
        while (poolData.pool.Count > 0)
        {
            var obj = poolData.pool.Dequeue();
            if (obj != null) return obj;
        }

        // 动态扩容
        if (poolData.allowExpand)
        {
            if (TotalCount(poolData) >= poolData.maxSize)
            {
                poolData.maxSize *= 2;
                Logger.Warning($"对象池 {poolData.poolRoot.name} 已紧急扩容至 {poolData.maxSize}");
            }

            return CreatePooledObject(poolData);
        }

        Logger.Warning($"对象池 {poolData.poolRoot.name} 已达最大容量且不允许扩容");
        return null;
    }

    private GameObject CreatePooledObject(PoolData poolData)
    {
        var obj = G.ResourceModule.LoadGameObjectByPool(poolData.location, poolData.poolRoot);
        obj.SetVisible(false);
        obj.transform.localPosition = Vector3.zero;
        return obj;
    }

    private int TotalCount(PoolData poolData) =>
        poolData.activeCount + poolData.pool.Count;

    private void ResetObjectState(GameObject obj)
    {
        // 状态重置逻辑
        obj.transform.localScale = Vector3.one;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.DOKill();
    }

    public async UniTask ClearAllPool(bool immediate = true)
    {
        var poolsToClear = new List<string>(_pools.Keys);
        // ParticleSimulationBudgetManager.Instance.ClearMemory();

        int processed = 0;
        foreach (var poolKey in poolsToClear)
        {
            if (!_pools.TryGetValue(poolKey, out PoolData pool))
                continue;
            // 销毁闲置对象
            while (pool.pool.Count > 0)
            {
                var obj = pool.pool.Dequeue();
                if (obj != null)
                {
                    if (immediate)
                    {
                        GameObject.DestroyImmediate(obj);
                        processed++;

                        if (processed % 20 == 0)
                        {
                            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
                        }
                    }
                    else
                    {
                        GameObject.Destroy(obj);
                    }

                }
            }

            // 销毁活跃对象
            {
                var activeObjects = new List<GameObject>();
                foreach (var kvp in _instanceToKeyMap)
                {
                    if (kvp.Value == poolKey)
                    {
                        activeObjects.Add(kvp.Key);
                    }
                }

                foreach (var obj in activeObjects)
                {
                    if (obj != null)
                    {
                        if (immediate)
                        {
                            GameObject.DestroyImmediate(obj);
                            processed++;

                            if (processed % 20 == 0)
                            {
                                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
                            }
                        }
                        else
                        {
                            GameObject.Destroy(obj);
                        }
                    }
                }
            }

            // 销毁父节点
            if (pool.poolRoot != null)
            {
                if (immediate)
                {
                    GameObject.DestroyImmediate(pool.poolRoot.gameObject);
                }
                else
                {
                    GameObject.Destroy(pool.poolRoot.gameObject);
                }
            }
        }

        _pools.Clear();
        _instanceToKeyMap.Clear();
    }

    /// <summary>
    /// 清空指定对象池
    /// </summary>
    public void ClearPool(string poolKey, bool destroyActive = false)
    {
        if (!_pools.TryGetValue(poolKey, out PoolData poolData)) return;

        foreach (var obj in poolData.pool)
        {
            GameObject.Destroy(obj);
        }

        poolData.pool.Clear();

        if (destroyActive)
        {
            var toRemove = new List<GameObject>();
            foreach (var kvp in _instanceToKeyMap)
            {
                if (kvp.Value == poolKey)
                {
                    toRemove.Add(kvp.Key);
                    GameObject.Destroy(kvp.Key);
                }
            }

            foreach (var obj in toRemove)
            {
                _instanceToKeyMap.Remove(obj);
            }

            poolData.activeCount = 0;
        }
    }
}