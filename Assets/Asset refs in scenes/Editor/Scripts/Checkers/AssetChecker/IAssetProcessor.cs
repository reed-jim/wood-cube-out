#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;

namespace SearchEngine.Checkers
{
    public interface IAssetProcessor
    {
        IEnumerable<string> CheckGO(GameObject go);
    }
}

#endif