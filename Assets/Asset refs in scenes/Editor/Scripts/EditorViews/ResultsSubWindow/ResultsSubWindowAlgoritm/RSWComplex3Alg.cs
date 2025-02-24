#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.Data;
using SearchEngine.Memento;
using SearchEngine.Memento.KeyValueMementoClasses;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.ResultsSubWindow
{
    public class RSWComplex3Alg : RSWComplexAlg<string, string>, IOriginator<RSWComplex3Alg.Memento>
    {
        public RSWComplex3Alg(string headerData, List<KeyValuePair<string, DataGOPath[]>> contentData, bool[] dataFoldout, UIActionsRSW uiActions) 
            : base(headerData, contentData, dataFoldout, uiActions)
        {
        }
        
        protected override void DisplayHeaderAsset()
        {
            GUILayout.BeginHorizontal(HelperGUI.GUIStyleObjectField2());
            GUILayout.Label(headerData, GUILayout.Width(UIActionsRSW.TitleObjWidth));
            GUILayout.EndHorizontal();
        }
        protected override void DisplayRowHeaderAsset(int rowNum)
        {
            HelperGUI.AssetField(contentData.ElementAt(rowNum).Key, UIActionsRSW.ContentObjWidth);
        }
        protected override string GetScenePath(int rowNum)
        {
            return contentData.ElementAt(rowNum).Key;
        }


        #region  memento part
        public Memento GetMemento()
        {
            return new Memento
            {
                headerData = this.headerData,
                contentData = CopyAbleHelper.Copy01(this.contentData, new StringDataGOPathM()),
                dataFoldout = this.dataFoldout.ToArray(),
            };
        }

        public void SetMemento(Memento mem)
        {
            if (mem == null || !mem.Validate())
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }

            this.headerData = mem.headerData;
            this.contentData = CopyAbleHelper.Copy01(mem.contentData).ToList();
            this.dataFoldout = mem.dataFoldout.ToArray();
        }

        [Serializable]
        public class Memento : IValidatable
        {
            public string headerData;
            public StringDataGOPathM[] contentData;
            public bool[] dataFoldout;

            public bool Validate()
            {
                return
                    !string.IsNullOrEmpty(headerData)
                    && dataFoldout != null
                    && contentData != null
                    && dataFoldout.Length == contentData.Length
                    && contentData.All(v => v != null && v.Validate());
            }
        }
        #endregion
    }
}

#endif