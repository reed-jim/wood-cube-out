using System;
using SearchEngine.Additions;
using SearchEngine.Memento;
using UnityEngine;

#if UNITY_EDITOR

namespace SearchEngine.EditorViews.ResultsSubWindow
{
    public abstract class ResultsSWFacade : IOriginator<ResultsSWFacade.Memento>
    {
        #region subsystems
        protected UIActionsRSW uiActions;
        #endregion

        #region facade interface
        public event Action<ResultsSWFacade> CloseButtonClicked;

        protected string title;
        protected IResultsSubWindowAlgoritm swAlgoritm;
        public Rect WindowPos { get; set; }

        public Vector2 windowSize = new Vector2(300, 400);
        public Vector2 WindowSize
        {
            set
            {
                windowSize = value;
                swAlgoritm.WindowSize = value;
            }
        }

        public void ShowGUI(int windowID)
        {
            WindowPos = GUILayout.Window(windowID, WindowPos, DoWindow, title);
        }
        private void DoWindow(int id)
        {
            GUILayout.BeginHorizontal(HelperGUI.GUIStyleTableHeader());
            swAlgoritm.ShowHeaderContent();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(HelperGUI.GUIStyleTableContent());
            GUILayout.Space(8);
            swAlgoritm.ShowMiddleContent();
            GUILayout.EndVertical();

            uiActions.ShowGUI();

            GUI.DragWindow();
        }
        public void OnCloseButtonClicked()
        {
            EventSender.SendEvent(CloseButtonClicked, this);
        }
        #endregion
        
        #region  memento part
        public Memento GetMemento()
        {
            return new Memento
            {
                title = this.title,
                windowPos = this.WindowPos,
            };
        }

        public void SetMemento(Memento mem)
        {
            if (mem == null && mem.Validate())
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }

            title = mem.title;
            WindowPos = mem.windowPos;
        }

        [Serializable]
        public class Memento : IValidatable
        {
            public string title;
            public Rect windowPos;

            public bool Validate()
            {
                return true;
            }
        }
        #endregion
    }
}

#endif