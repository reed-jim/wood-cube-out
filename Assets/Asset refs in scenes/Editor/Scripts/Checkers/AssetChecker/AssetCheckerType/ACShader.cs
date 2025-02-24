#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using SearchEngine.Additions;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SearchEngine.Checkers
{
    public class ACShader : IAssetCheckerType
    {
        [SerializeField] private HashSet<string> assets;
        [SerializeField] private ACFieldsAndPropsAlgoritm algoritm;
        private HashSet<string> assetsFound = new HashSet<string>();

        public ACShader(IEnumerable<string> assets)
        {
            var validTypes = new HashSet<Type>()
            {
                typeof(Object),
                typeof(Shader),
                typeof(Material)
            };
            this.assets = new HashSet<string>(assets);
            algoritm = new ACFieldsAndPropsAlgoritm(this, validTypes, null);
            this.CheckSerializeFields();
        }

        public IEnumerable<string> CheckGO(GameObject go)
        {
            assetsFound.Clear();

            algoritm.CheckComponentsVariables(go);
            return assetsFound;
        }
        
        public void CheckSceneObject(GameObject go){}
        
        public void CheckFieldObject(object assetField)
        {
            if (assetField as Material)
            {
                CheckAssetContains(((Material)assetField).shader);   
            }
            else
            {
                CheckAssetContains(assetField as Object);
            }
        }

        private void CheckAssetContains(Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            if (assets.Contains(path))
            {
                assetsFound.Add(path);
            }
        }
    }
}

#endif