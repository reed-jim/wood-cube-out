#if UNITY_EDITOR

using UnityEngine;

namespace SearchEngine.Checkers
{
    public interface IAssetCheckerType : IAssetProcessor
    {
        void CheckSceneObject(GameObject go);
        void CheckFieldObject(object assetField);
    }
}

#endif