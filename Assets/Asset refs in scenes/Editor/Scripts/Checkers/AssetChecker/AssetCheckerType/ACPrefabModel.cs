#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using SearchEngine.Additions;
using Object = UnityEngine.Object;

namespace SearchEngine.Checkers
{
    public class ACPrefabModel : IAssetCheckerType
    {
        [SerializeField] private HashSet<string> assets;
        [SerializeField] private ACFieldsAndPropsAlgoritm algoritm;
        private HashSet<string> assetsFound = new HashSet<string>();
        private Dictionary<Mesh, string> validMeshes = new Dictionary<Mesh, string>();

        public ACPrefabModel(IEnumerable<string> assets)
        {
            var validTypes = new HashSet<Type>()
            {
                typeof(Object),
                typeof(GameObject),
                typeof(Component),
                typeof(Transform),
                typeof(Mesh)
            };
            var validParentTypes = new HashSet<Type>()
            {
                typeof(MonoBehaviour)
            };
            this.assets = new HashSet<string>(assets);
            algoritm = new ACFieldsAndPropsAlgoritm(this, validTypes, validParentTypes);
            this.CheckSerializeFields();
        }

        public IEnumerable<string> CheckGO(GameObject go)
        {
            assetsFound.Clear();

            CheckSceneObject(go);
            algoritm.CheckComponentsVariables(go);
            
            return assetsFound;
        }

        public void CheckSceneObject(GameObject go)
        {
            var v1 = PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);//work with disconnected prefabs
            //var v1 = PrefabUtility.FindPrefabRoot(go);//bad
            if (v1 != go)
                return;

            CheckAssetContains(
                AssetDatabase.GetAssetPath(
                    PrefabUtility.GetCorrespondingObjectFromSource(go)));

        }

        //model mesh catch too
        public void CheckFieldObject(object assetField)
        {
            if (assetField as Mesh)
            {
                CheckMesh((Mesh) assetField);
            }
            else
            {
                CheckAssetContains(AssetDatabase.GetAssetPath(assetField as Object));
            }
        }

        private void CheckMesh(Mesh m)
        {
            if (validMeshes.ContainsKey(m))
            {
                if(validMeshes[m] != null)
                    assetsFound.Add(validMeshes[m]);
            }
            else
            {
                var v = AssetDatabase.GetAssetPath(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(m)));
                if (assets.Contains(v))
                {
                    validMeshes.Add(m, v);
                    assetsFound.Add(v);                
                }
                else
                    validMeshes.Add(m, null);
            }
        }
         
        private void CheckAssetContains(string asset)
        {
            if (assets.Contains(asset))
                assetsFound.Add(asset);
        }
    }
}

#endif