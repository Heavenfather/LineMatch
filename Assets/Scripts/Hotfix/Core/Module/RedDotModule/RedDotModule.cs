using System;
using System.Collections.Generic;
using GameConfig;
using GameCore.Log;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Utils;
using HotfixLogic;

namespace HotfixCore.Module
{
    public class RedDotModule : IModuleAwake, IModuleDestroy
    {
        /// <summary>
        /// 红点树的的 Root节点
        /// </summary>
        private RedDotNode _rootNode;

        public void Awake(object parameter)
        {
            InitRedDotTreeNode();
        }

        public void Destroy()
        {
        }

        /// <summary>
        /// 红点数变化通知委托
        /// </summary>
        /// <param name="node"></param>
        public delegate void OnRdCountChange(RedDotNode node);

        /// <summary>
        /// 初始化红点树
        /// </summary>
        private void InitRedDotTreeNode()
        {
            /*
            * 结构层：根据红点是否显示或显示数，自定义红点的表现方式
            */

            _rootNode = new RedDotNode {rdName = RedDotDefine.Root};

            foreach (string path in RedDotDefine.RedDotsTree)
            {
                string[] treeNodeAy = path.Split('/');
                int nodeCount = treeNodeAy.Length;
                RedDotNode curNode = _rootNode;

                if (treeNodeAy[0] != _rootNode.rdName)
                {
                    Logger.Warning("根节点必须为Root，检查 " + treeNodeAy[0]);
                    continue;
                }

                if (nodeCount > 1)
                {
                    for (int i = 1; i < nodeCount; i++)
                    {
                        if (!curNode.rdChildrenDic.ContainsKey(treeNodeAy[i]))
                        {
                            curNode.rdChildrenDic.Add(treeNodeAy[i], new RedDotNode());
                        }

                        curNode.rdChildrenDic[treeNodeAy[i]].rdName = treeNodeAy[i];
                        curNode.rdChildrenDic[treeNodeAy[i]].parent = curNode;

                        curNode = curNode.rdChildrenDic[treeNodeAy[i]];
                    }
                }
            }
        }

        /// <summary>
        /// 设置红点数变化的回调
        /// </summary>
        /// <param name="strNode">红点路径，必须是 RedDotDefine </param>
        /// <param name="callBack">回调函数</param>
        public void SetRedDotNodeCallBack(string strNode, OnRdCountChange callBack)
        {
            var nodeList = strNode.Split('/');

            if (nodeList.Length == 1)
            {
                if (nodeList[0] != RedDotDefine.Root)
                {
                    Logger.Warning("Get Wrong Root Node! current is " + nodeList[0]);
                    return;
                }
            }

            var node = _rootNode;
            for (int i = 1; i < nodeList.Length; i++)
            {
                if (!node.rdChildrenDic.ContainsKey(nodeList[i]))
                {
                    Logger.Warning("Does Not Contain child Node: " + nodeList[i]);
                    return;
                }

                node = node.rdChildrenDic[nodeList[i]];

                if (i == nodeList.Length - 1)
                {
                    node.countChangeFunc += callBack;
                    return;
                }
            }
        }

        public void RemoveRedDotNode(string strNode, OnRdCountChange callBack = null) {
            if (callBack == null) return;

            var nodeList = strNode.Split('/');

            if (nodeList.Length == 1)
            {
                if (nodeList[0] != RedDotDefine.Root)
                {
                    Logger.Warning("Get Wrong Root Node! current is " + nodeList[0]);
                    return;
                }
            }

            var node = _rootNode;
            for (int i = 1; i < nodeList.Length; i++)
            {
                if (!node.rdChildrenDic.ContainsKey(nodeList[i]))
                {
                    Logger.Warning("Does Not Contain child Node: " + nodeList[i]);
                    return;
                }


                var childNode = node.rdChildrenDic[nodeList[i]];
                if (i == nodeList.Length - 1)
                {
                    childNode.countChangeFunc -= callBack;
                    return;
                }

                node = childNode;
            }
        }

        /// <summary>
        /// 设置红点参数
        /// </summary>
        /// <param name="nodePath">红点路径，必须走 RedDotDefine </param>
        /// <param name="rdCount">红点计数</param>
        public void SetRedDotCount(string nodePath, int rdCount = 1)
        {
            string[] nodeList = nodePath.Split('/');

            if (nodeList.Length == 1)
            {
                if (nodeList[0] != RedDotDefine.Root)
                {
                    Logger.Debug("Get Wrong RootNod！ current is " + nodeList[0]);
                    return;
                }
            }

            //遍历子红点
            RedDotNode node = _rootNode;
            for (int i = 1; i < nodeList.Length; i++)
            {
                //父红点的 子红点字典表 内，必须包含
                if (node.rdChildrenDic.ContainsKey(nodeList[i]))
                {
                    node = node.rdChildrenDic[nodeList[i]];

                    //设置叶子红点的红点数
                    if (i == nodeList.Length - 1)
                    {
                        node.SetRedDotCount(Math.Max(0, rdCount));
                    }
                }
                else
                {
                    Logger.Warning($"{node.rdName}的子红点字典内无 Key={nodeList[i]}, 检查 RedDotSystem.InitRedDotTreeNode()");
                    return;
                }
            }
        }

        public void AddRedDotCount(string nodePath, int rdCount = 1)
        {
            var curCount = GetRedDotCount(nodePath);
            SetRedDotCount(nodePath, curCount + rdCount);
        }

        /// <summary>
        /// 获取红点的计数
        /// </summary>
        /// <param name="nodePath"></param>
        /// <returns></returns>
        public int GetRedDotCount(string nodePath)
        {
            string[] nodeList = nodePath.Split('/');

            int count = 0;
            if (nodeList.Length >= 1)
            {
                //遍历子红点
                RedDotNode node = _rootNode;
                for (int i = 1; i < nodeList.Length; i++)
                {
                    //父红点的 子红点字典表 内，必须包含
                    if (node.rdChildrenDic.ContainsKey(nodeList[i]))
                    {
                        node = node.rdChildrenDic[nodeList[i]];

                        if (i == nodeList.Length - 1)
                        {
                            count = node.rdCount;
                            break;
                        }
                    }
                }
            }

            return count;
        }
    }
}