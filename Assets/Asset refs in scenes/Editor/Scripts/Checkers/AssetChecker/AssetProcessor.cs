#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using SearchEngine.Additions;
using Object = UnityEngine.Object;

namespace SearchEngine.Checkers
{
    public class AssetProcessor
    {
        [SerializeField] private IAssetProcessor[] procs;
        
        public AssetProcessor(IAssetProcessor[] procs)
        {
            this.procs = procs;
            this.CheckSerializeFields();
        }
        
        public IEnumerable<IEnumerable<string>> CheckGO(GameObject go)
        {
            foreach (var v in procs) yield return v.CheckGO(go);
        }
    }
}

#endif
