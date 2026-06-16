#if UNITY_EDITOR

using System.Collections.Generic;
using SearchEngine.EditorViews.AssetTypes;
using UnityEditor;
using UnityEngine;

namespace SearchEngine.Checkers
{
    public abstract class AssetProcessorFactory
    {
        protected List<string> PrefabModels;
        protected List<string> Shaders;
        protected List<string> Textures;
        protected List<string> Scripts;
        protected List<string> OtherTypes;

        protected void SetupAssetLists(IEnumerable<string> assets)
        {
            PrefabModels = new List<string>();
            Shaders = new List<string>();
            Textures = new List<string>();
            Scripts = new List<string>();
            OtherTypes = new List<string>();

            foreach (var v in assets)
            {
                if (v == null)
                    continue;

                AssetTypes type = AssetTypesHelper.GetType(v);
                if (type == AssetTypes.Prefab || type == AssetTypes.Model)
                {
                    PrefabModels.Add(v);
                }
                else if (type == AssetTypes.Shader)
                {
                    Shaders.Add(v);
                }
                else if (type == AssetTypes.Texture)
                {
                    Textures.Add(v);
                }
                else if (type == AssetTypes.Script)
                {
                    Scripts.Add(v);
                }
                else
                {
                    OtherTypes.Add(v);
                }
            }
        }

        public abstract AssetProcessor Create(IEnumerable<string> assets);
    }
}

#endif
