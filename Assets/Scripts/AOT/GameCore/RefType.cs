using System;
using GameCore.Resource;
using UnityEngine;

namespace AOT.GameCore
{
    /// <summary>
    /// HybridCLR补充AOT泛型函数
    /// </summary>
    public class RefType : MonoBehaviour
    {
        private void Awake()
        {
            this.GetComponentInChildren<ResourceModuleDriver>();
        }
    }
}