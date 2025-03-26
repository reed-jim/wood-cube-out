#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using SearchEngine.Additions;
using Object = UnityEngine.Object;

namespace SearchEngine.Checkers
{
    public class ACTexture : IAssetCheckerType
    {
        [SerializeField] private HashSet<string> assets;
        [SerializeField] private ACFieldsAndPropsAlgoritm algoritm;
        private HashSet<string> assetsFound = new HashSet<string>();
        private Dictionary<Material, string[]> validMats = new Dictionary<Material, string[]>();

        public ACTexture(IEnumerable<string> assets)
        {
            var validTypes = new HashSet<Type>()
            {
                typeof(Object),
                typeof(Texture),
                typeof(Sprite),
                typeof(Texture2D),
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
                CheckMaterial((Material) assetField);
            }
            else if(assetField as Sprite)
            {
                CheckAssetContains(((Sprite) assetField).texture);
            }
            else
            {
                CheckAssetContains(assetField as Object);
            }
        }

        private void CheckMaterial(Material m)
        {
            if (validMats.ContainsKey(m))
            {
                if(validMats[m] != null)
                    foreach (var v in validMats[m]) assetsFound.Add(v);
            }
            else
            {
                List<Object> textures = new List<Object>();
                
                Shader s = m.shader;
                int propertyCount = ShaderUtil.GetPropertyCount(s);
                m.shaderKeywords.Count();
                for (int i = 0; i < propertyCount; i++)
                {
                    if (ShaderUtil.GetPropertyType(s, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        var v = m.GetTexture(ShaderUtil.GetPropertyName(s, i));
                        if (assets.Contains(AssetDatabase.GetAssetPath(v)))
                            textures.Add(v);
                    }
                }

                if (textures.Any())
                {
                    validMats.Add(m, textures.Select(v=>AssetDatabase.GetAssetPath(v)).ToArray());
                    foreach (var v in validMats[m]) assetsFound.Add(v);
                }
                else
                {
                    validMats.Add(m, null);                    
                }
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