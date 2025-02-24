#if UNITY_EDITOR

using System;
using SearchEngine.EditorViews.Data;
using SearchEngine.Memento;

namespace SearchEngine.EditorViews.ResultsSubWindow
{
    public class RswSimpleFacade : ResultsSWFacade, IOriginator<RswSimpleFacade.Memento>
    {
        #region subsystems
        private RSWSimpleAlg swAlg;
        #endregion
        
        #region creating subsystems
        public void CreateEmpty()
        {
            ReCreate(
                string.Empty,
                new SceneGOData[0]);
        }

        public void ReCreate(string title, SceneGOData[] data)
        {
            uiActions = new UIActionsRSW();
            swAlgoritm = swAlg = new RSWSimpleAlg(data, uiActions);
            swAlgoritm.CloseButtonClicked += OnCloseButtonClicked;
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
            public RSWSimpleAlg.Memento swAlg;
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