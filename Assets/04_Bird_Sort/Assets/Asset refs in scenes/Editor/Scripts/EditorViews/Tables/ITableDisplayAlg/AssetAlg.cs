#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using SearchEngine.Additions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.Tables
{
    [Serializable]
    public class AssetAlg : ITableDisplayAlg
    {
        [SerializeField] private List<string> assets;
        [SerializeField] private string titleTemplate;

        public AssetAlg(List<string> assets, string titleTemplate = "Asset ({0})")
        {
            this.assets = assets;
            this.titleTemplate = titleTemplate;
            this.CheckSerializeFields();
        }
        
        public void ShowRow(int rowNum)
        {
            HelperGUI.AssetField(assets[rowNum]);
        }

        public void ShowPreTitle() { }

        public void ShowTitle()
        {
            GUILayout.Label(string.Format(titleTemplate, assets.Count));
        }

        public void AfterTable(){}
    }
}

#endif