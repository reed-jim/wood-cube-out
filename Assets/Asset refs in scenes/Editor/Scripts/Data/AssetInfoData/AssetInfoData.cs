#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SearchEngine.EditorViews.AssetTypes;
using SearchEngine.Memento;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.Data
{
    [Serializable]
    public class AssetInfoData : ICopyAble<AssetInfoData>, IValidatable
    {
        [SerializeField] private string assetPath;
        [SerializeField] private bool active;
        private int assetType;

        public string AssetPath
        {
            get { return assetPath; }
        }

        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        public int AssetType
        {
            get
            {
                return assetType;                
            }
        }

        public AssetInfoData(string assetPath, bool active)
        {
            this.assetPath = assetPath;
            this.active = active;
            assetType = (int)AssetTypesHelper.GetType(assetPath);
        }

        public bool Validate()
        {
            return
                File.Exists(assetPath);
        }
        
        #region static
        public static void AddNewAssets(IList<AssetInfoData> data, IEnumerable<string> newAssetPaths)
        {
            foreach (var path in newAssetPaths)
            {
                var correctPath = path.Replace('\\', '/');
                if (data.Any(v => v.AssetPath == correctPath))
                    continue;
                if (new Asset(correctPath).isFolder)
                    continue;
                data.Add(new AssetInfoData(correctPath, true));
            }
        }

        public static string[] GetActiveAssetPaths(IEnumerable<AssetInfoData> data)
        {
            return
            (from v in data
                where v.Active
                select v.AssetPath).ToArray();
        }
        #endregion

        #region Copy
        public AssetInfoData Copy()
        {
            return new AssetInfoData(assetPath, active);
        }

        object ICopyAble.Copy()
        {
            return Copy();
        }
        #endregion
    }
}

#endif