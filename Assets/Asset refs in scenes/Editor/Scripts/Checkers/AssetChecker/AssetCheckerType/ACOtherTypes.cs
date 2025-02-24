#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using SearchEngine.Additions;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SearchEngine.Checkers
{
    //Animation component is legacy and Animation.Animations are not available by code
    public class ACOtherType : IAssetCheckerType
    {
        [SerializeField] private HashSet<string> assets;
        [SerializeField] private ACFieldsAndPropsAlgoritm algoritm;
        private HashSet<string> assetsFound = new HashSet<string>();

        public ACOtherType(IEnumerable<string> assets)
        {
            var validTypes = new HashSet<Type>();
            var validParentTypes = new HashSet<Type>();
            validParentTypes.Add(typeof(Object));

            this.assets = new HashSet<string>(assets);
            algoritm = new ACFieldsAndPropsAlgoritm(this, validTypes, validParentTypes);
            this.CheckSerializeFields();
        }
        
        public IEnumerable<string> CheckGO(GameObject go)
        {
            assetsFound.Clear();

            algoritm.CheckComponentsVariables(go);
            return assetsFound;
        }
        
        public void CheckSceneObject(GameObject go){}

        public void CheckFieldObject(object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset as Object);
            if (assets.Contains(path))
            {
                assetsFound.Add(path);
            }
        }
    }
}

#endif