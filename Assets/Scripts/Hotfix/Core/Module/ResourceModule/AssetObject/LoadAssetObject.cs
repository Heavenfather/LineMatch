using System;

namespace HotfixCore.Module
{
    [Serializable]
    public class LoadAssetObject
    {
        public ISetAssetObject AssetObject { get; }
        public UnityEngine.Object AssetTarget { get; }

        public LoadAssetObject(ISetAssetObject obj, UnityEngine.Object assetTarget)
        {
            AssetObject = obj;
            AssetTarget = assetTarget;
        }
    }
}