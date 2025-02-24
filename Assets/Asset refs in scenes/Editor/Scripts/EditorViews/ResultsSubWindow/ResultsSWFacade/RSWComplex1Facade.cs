#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using SearchEngine.EditorViews.Data;
using SearchEngine.Memento;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.ResultsSubWindow
{
    public class RSWComplex1Facade : ResultsSWFacade, IOriginator<RSWComplex1Facade.Memento>
    {      
        #region subsystems
        private RSWComplex1Alg swAlg;
        #endregion

        #region creating subsystems
        public void CreateEmpty()
        {
            ReCreate(
                string.Empty,
                null,
                new List<string>(),
                new List<DataGOPath[]>());
        }

        public void ReCreate(string title, string headerData, List<string> contentTitles, List<DataGOPath[]> contentData)
        {
            List<KeyValuePair<string, DataGOPath[]>> contentData2 = new List<KeyValuePair<string, DataGOPath[]>>();
            for (int i = 0; i < contentTitles.Count; i++)
            {
                contentData2.Add(new KeyValuePair<string, DataGOPath[]>(contentTitles[i], contentData[i]));
            }

            uiActions = new UIActionsRSW();
            bool[] dataFoldout = new bool[contentTitles.Count];
            swAlgoritm = swAlg = new RSWComplex1Alg(headerData, contentData2, dataFoldout, uiActions);
            swAlg.CloseButtonClicked += OnCloseButtonClicked;
            this.title = title;
        }
        #endregion

        #region  memento part
        public new Memento GetMemento()
        {
            return new Memento
            {
                results = base.GetMemento(),
                swAlg = this.swAlg.GetMemento(),
            };
        }
        public void SetMemento(Memento mem)
        {
            if (mem == null || !mem.Validate())
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }

            this.swAlg.SetMemento(mem.swAlg);
            this.SetMemento(mem.results);
        }

        [Serializable]
        public new class Memento : IValidatable
        {
            public RSWComplex1Alg.Memento swAlg;
            public ResultsSWFacade.Memento results;

            public bool Validate()
            {
                return
                    swAlg != null
                    && results != null
                    && swAlg.Validate()
                    && results.Validate();
            }
        }
        #endregion
    }
}

#endif