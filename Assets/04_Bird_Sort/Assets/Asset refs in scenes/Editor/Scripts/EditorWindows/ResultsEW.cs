#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using SearchEngine.Additions;
using SearchEngine.EditorViews.ResultsSubWindow;
using SearchEngine.Memento;

namespace SearchEngine.EditorWindows
{
    public class ResultsEW : EditorWindow, IOriginator<ResultsEW.Memento>
    {
        private Vector2 scrollPos;
        private readonly Vector2 windowSize = new Vector2(300, 400);
        private List<ResultsSWFacade> subWindows = new List<ResultsSWFacade>();

        private static ResultsEW instance;

        public static ResultsEW Instance
        {
            get
            {
                if (instance == null)
                    SetInstanceValue();
                return instance;
            }
        }
        private static void SetInstanceValue()
        {
            var windows = Resources.FindObjectsOfTypeAll<ResultsEW>();
            if (windows.Any())
            {
                instance = windows[0];
            }
            else
            {
                instance = CreateInstance<ResultsEW>();
                instance.titleContent = new GUIContent("Result details");
            }
        }

        public static void CloseCurrWindow()
        {
            var windows = Resources.FindObjectsOfTypeAll<ResultsEW>();
            if (windows.Any())
                windows[0].Close();
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal(HelperGUI.GUIStyleSkinBox1, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Set default window's position", GUILayout.ExpandWidth(false)))
                    Reposition();
                if (GUILayout.Button("Close all windows", GUILayout.ExpandWidth(false)))
                    ClearData();
            GUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                float scropllSizeX = 100;
                float scropllSizeY = 100;
                BeginWindows();
                for (int i = 0; i < subWindows.Count; i++)
                {
                    subWindows[i].ShowGUI(i);
                    if (subWindows[i].WindowPos.xMax > scropllSizeX)
                        scropllSizeX = subWindows[i].WindowPos.xMax;
                    if (subWindows[i].WindowPos.yMax > scropllSizeY)
                        scropllSizeY = subWindows[i].WindowPos.yMax;
                }
                EndWindows();
                GUILayout.Label("", GUILayout.Height(scropllSizeY+200), GUILayout.Width(scropllSizeX+200));
            GUILayout.EndScrollView();
        }
        
        public void AddData(ResultsSWFacade newSubWindow)
        {
            newSubWindow.WindowSize = windowSize;
            subWindows.Add(newSubWindow);
            subWindows.Last().CloseButtonClicked += CloseSubWindowButtonClicked;
            Reposition();
            Repaint();
        }

        private void CloseSubWindowButtonClicked(ResultsSWFacade subWindow)
        {
            subWindows.Remove(subWindow);
        }

        private void ClearData()
        {
            subWindows.Clear();
        }

        private void Reposition()
        {
            int margin = 4;
            int scenesCount = subWindows.Count;
            float s = scenesCount*windowSize.x*windowSize.y;
            float side = Mathf.Sqrt(s);
            int width = Mathf.CeilToInt(side / windowSize.x);
            int height = Mathf.CeilToInt(scenesCount / (float)width);
            int counter = 0;
            int heightShift = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var v = new Rect(j*windowSize.x+ margin*(j+1), heightShift + i * windowSize.y+ margin*(i+1), windowSize.x, windowSize.y);
                    subWindows[counter].WindowPos=v; 

                    if(++counter >= scenesCount)
                        return;
                }                    
            }
        }

        
        #region  memento part
        public Memento GetMemento()
        {
            var newSubWindows = new RSW[this.subWindows.Count];
            int i = -1;
            foreach (var sw in this.subWindows)
            {
                i++;
                newSubWindows[i] = new RSW();
                if (sw is RswSimpleFacade)
                {
                    newSubWindows[i].type = ResultsSWFactoryTypes.Simple;
                    newSubWindows[i].simple = ((RswSimpleFacade) sw).GetMemento(); 
                }
                else if (sw is RSWComplex0Facade)
                {
                    newSubWindows[i].type = ResultsSWFactoryTypes.Complex0;
                    newSubWindows[i].complex0 = ((RSWComplex0Facade)sw).GetMemento();
                }
                else if (sw is RSWComplex1Facade)
                {
                    newSubWindows[i].type = ResultsSWFactoryTypes.Complex1;
                    newSubWindows[i].complex1 = ((RSWComplex1Facade)sw).GetMemento();
                }
                else if (sw is RSWComplex2Facade)
                {
                    newSubWindows[i].type = ResultsSWFactoryTypes.Complex2;
                    newSubWindows[i].complex2 = ((RSWComplex2Facade)sw).GetMemento();
                }
                else if (sw is RSWComplex3Facade)
                {
                    newSubWindows[i].type = ResultsSWFactoryTypes.Complex3;
                    newSubWindows[i].complex3 = ((RSWComplex3Facade)sw).GetMemento();
                }
            }

            return new Memento
            {
                subWindows = newSubWindows,
            };
        }

        public void SetMemento(Memento mem)
        {
            if (mem == null || !mem.Validate())
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }

            ClearData();
            foreach (var window in mem.subWindows)
            {
                if(window == null || !window.Validate())
                    continue;

                if (window.type == ResultsSWFactoryTypes.Simple)
                {
                    var rsw = new RswSimpleFacade();
                    rsw.CreateEmpty();
                    rsw.SetMemento(window.simple);
                    AddData(rsw);     
                }
                else if (window.type == ResultsSWFactoryTypes.Complex0)
                {
                    var rsw = new RSWComplex0Facade();
                    rsw.CreateEmpty();
                    rsw.SetMemento(window.complex0);
                    AddData(rsw);
                }
                else if (window.type == ResultsSWFactoryTypes.Complex1)
                {
                    var rsw = new RSWComplex1Facade();
                    rsw.CreateEmpty();
                    rsw.SetMemento(window.complex1);
                    AddData(rsw);
                }
                else if (window.type == ResultsSWFactoryTypes.Complex2)
                {
                    var rsw = new RSWComplex2Facade();
                    rsw.CreateEmpty();
                    rsw.SetMemento(window.complex2);
                    AddData(rsw);
                }
                else if (window.type == ResultsSWFactoryTypes.Complex3)
                {
                    var rsw = new RSWComplex3Facade();
                    rsw.CreateEmpty();
                    rsw.SetMemento(window.complex3);
                    AddData(rsw);
                }
            }
        }

        [Serializable]
        public class Memento : IValidatable
        {
            public RSW[] subWindows;

            public bool Validate()
            {
                return
                    subWindows != null;
            }
        }
        #endregion
    }
}

#endif