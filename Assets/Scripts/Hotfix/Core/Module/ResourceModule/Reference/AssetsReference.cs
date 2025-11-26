using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HotfixCore.Module
{
    [Serializable]
    public struct AssetsRefInfo
    {
        public int instanceId;

        public Object refAsset;

        public AssetsRefInfo(Object refAsset)
        {
            this.refAsset = refAsset;
            instanceId = this.refAsset.GetInstanceID();
        }
    }

    /// <summary>
    /// 资源加载自动引用自动释放
    /// </summary>
    public sealed class AssetsReference : MonoBehaviour
    {
        [SerializeField] private GameObject sourceGameObject;

        [SerializeField] private List<AssetsRefInfo> refAssetInfoList;

        private void OnDestroy()
        {
            if (sourceGameObject != null)
            {
                G.ResourceModule.UnloadAsset(sourceGameObject);
            }

            ReleaseRefAssetInfoList();
        }

        private void ReleaseRefAssetInfoList()
        {
            if (refAssetInfoList != null)
            {
                foreach (var refInfo in refAssetInfoList)
                {
                    G.ResourceModule.UnloadAsset(refInfo.refAsset);
                }

                refAssetInfoList.Clear();
            }
        }

        public AssetsReference Ref(GameObject source)
        {
            if (source == null)
            {
                throw new Exception($"Source gameObject is null.");
            }

            if (source.scene.name != null)
            {
                throw new Exception($"Source gameObject is in scene.");
            }

            sourceGameObject = source;
            return this;
        }

        public AssetsReference Ref<T>(T source) where T : UnityEngine.Object
        {
            if (source == null)
            {
                throw new Exception($"Source gameObject is null.");
            }

            if (refAssetInfoList == null)
            {
                refAssetInfoList = new List<AssetsRefInfo>();
            }

            refAssetInfoList.Add(new AssetsRefInfo(source));
            return this;
        }

        public static AssetsReference Instantiate(GameObject source, Transform parent = null)
        {
            if (source == null)
            {
                throw new Exception($"Source gameObject is null.");
            }

            if (source.scene.name != null)
            {
                throw new Exception($"Source gameObject is in scene.");
            }

            GameObject instance = Object.Instantiate(source, parent);
            return instance.AddComponent<AssetsReference>().Ref(source);
        }

        public static AssetsReference Ref(GameObject source, GameObject instance)
        {
            if (source == null)
            {
                throw new Exception($"Source gameObject is null.");
            }

            if (source.scene.name != null)
            {
                throw new Exception($"Source gameObject is in scene.");
            }

            var comp = instance.GetComponent<AssetsReference>();
            return comp ? comp : instance.AddComponent<AssetsReference>().Ref(source);
        }

        public static AssetsReference Ref<T>(T source, GameObject instance)
            where T : UnityEngine.Object
        {
            if (source == null)
            {
                throw new Exception($"Source gameObject is null.");
            }

            var comp = instance.GetComponent<AssetsReference>();
            return comp ? comp : instance.AddComponent<AssetsReference>().Ref(source);
        }
    }
}