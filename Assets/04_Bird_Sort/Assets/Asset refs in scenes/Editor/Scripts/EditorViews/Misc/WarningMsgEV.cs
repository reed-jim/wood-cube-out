#if UNITY_EDITOR

using UnityEditor;
using SearchEngine.Additions;
using UnityEngine;

namespace SearchEngine.EditorViews
{
    public class WarningMsgEV : IEditorView
    {
        private double timeEnd;
        private float duration=3;
        private int fontSize = 0;

        public int FontSize
        {
            get { return fontSize; }
            set
            {
                if (value >= 0)
                    fontSize = value;                
            }
        }

        public float Duration
        {
            get { return duration; }
            set
            {
                if (value >= 0.1f)
                    duration = value;
            }
        }

        public string Msg { get; set; }
        
        public void SetWarningMsg(string msg)
        {
            if (string.IsNullOrEmpty(msg))
                return;
            Activate();
            Msg = msg;
        }

        public void Activate()
        {
            timeEnd = EditorApplication.timeSinceStartup + duration;
        }

        public void ShowGUI()
        {
            if (timeEnd > EditorApplication.timeSinceStartup
                && !string.IsNullOrEmpty(Msg))
            {
                GUILayout.Label(Msg, HelperGUI.RedTxtStyle(fontSize));
            }
        }
    }
}

#endif