using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 元素视图容器
    /// 不包含任何业务逻辑，只负责暴露组件引用、提供外部设置视图的接口
    /// </summary>
    public class ElementView : MonoBehaviour
    {
        public SpriteRenderer Icon { get; private set; }
        public SkeletonAnimation Spine { get; private set; }

        private Dictionary<string, GameObject> _partDict;

        private void Awake()
        {
            _partDict = new Dictionary<string, GameObject>(10);
            // 直接添加所有的根部子节点，兼容旧的预制体模式，外部直接通过名称查找和获取想要的节点
            // 这样就不用为旧的预制体加上拖拽引用组件了
            if (this.transform != null)
            {
                foreach (Transform rootChild in this.transform)
                {
                    if (!_partDict.ContainsKey(rootChild.gameObject.name))
                        _partDict.Add(rootChild.gameObject.name, rootChild.gameObject);
                    if (rootChild.gameObject.name == "Icon")
                    {
                        if (rootChild.gameObject.GetComponent<SpriteRenderer>() != null)
                            Icon = rootChild.gameObject.GetComponent<SpriteRenderer>();
                        else if (rootChild.gameObject.GetComponent<SkeletonAnimation>() != null)
                            Spine = rootChild.gameObject.GetComponent<SkeletonAnimation>();
                    }
                }
            }
        }

        /// <summary>
        /// 获取特定部件
        /// </summary>
        /// <param name="childName">子节点根名称</param>
        /// <param name="nodePath">子节点路径（可选）</param>
        /// <returns></returns>
        public GameObject GetPart(string childName, string nodePath = "")
        {
            if (_partDict != null && _partDict.TryGetValue(childName, out var obj))
            {
                if (!string.IsNullOrEmpty(nodePath))
                {
                    return obj.transform.Find(nodePath)?.gameObject;
                }

                return obj;
            }

            return null;
        }

        /// <summary>
        /// 设置层级排序
        /// </summary>
        /// <param name="order"></param>
        /// <param name="layerName"></param>
        public void SetSortingOrder(int order, string layerName = "")
        {
            if (string.IsNullOrEmpty(layerName))
                layerName = "Item";
            Renderer icon = null;
            if (Icon != null)
                icon = Icon;
            else if (Spine != null)
                icon = Spine.GetComponent<Renderer>();

            if (icon != null)
            {
                icon.sortingLayerName = layerName;
                icon.sortingOrder = order;
            }
        }

        /// <summary>
        /// 设置Spine动画
        /// </summary>
        /// <param name="animationName"></param>
        /// <param name="isLoop"></param>
        /// <param name="isEmpty">是否清空其他轨道动画</param>
        /// <param name="trackIndex"></param>
        public void SetSpineAnimation(string animationName, bool isLoop = false,bool isEmpty = false, int trackIndex = 0)
        {
            if (Spine != null)
            {
                if (isEmpty)
                    Spine.AnimationState.SetEmptyAnimation(0, 0);
                else
                    Spine.AnimationState.SetAnimation(trackIndex, animationName, isLoop);
            }
        }
    }
}