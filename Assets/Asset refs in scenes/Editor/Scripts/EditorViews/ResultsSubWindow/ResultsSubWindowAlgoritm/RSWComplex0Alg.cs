#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using SearchEngine.EditorViews.Data;
using SearchEngine.Memento;
using SearchEngine.Memento.KeyValueMementoClasses;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.ResultsSubWindow
{
    public class RSWComplex0Alg : RSWComplex1Alg, IOriginator<RSWComplex1Alg.Memento>
    {
        public RSWComplex0Alg(string headerData, List<KeyValuePair<string, DataGOPath[]>> contentData, bool[] dataFoldout, UIActionsRSW uiActions) 
            : base(headerData, contentData, dataFoldout, uiActions)
        {
        }
        
        protected override string GetScenePath(int rowNum)
        {
            return contentData.ElementAt(rowNum).Key;
        }

        
        #region  memento part
        public new RSWComplex1Alg.Memento GetMemento()
        {
            return new RSWComplex1Alg.Memento
            {
                headerData = this.headerData,
                contentData = CopyAbleHelper.Copy01(this.contentData, new StringDataGOPathM()),
                dataFoldout = this.dataFoldout.ToArray(),
            };
        }

        public new void SetMemento(RSWComplex1Alg.Memento mem)
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
        #endregion
    }
}

#endif