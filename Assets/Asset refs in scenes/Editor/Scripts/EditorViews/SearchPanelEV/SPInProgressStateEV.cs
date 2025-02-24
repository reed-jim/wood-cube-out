#if UNITY_EDITOR 

using System;
using UnityEngine;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.Tables;
using UnityEditor;

namespace SearchEngine.EditorViews
{
    public class SPInProgressStateEV : ISPState
    {
        [SerializeField] private ScenesRunner scenesRunner;
        [SerializeField] private IEditorView timer;
        public event Action StopBtnClicked;

        public SPInProgressStateEV(IEditorView timer, ScenesRunner scenesRunner)
        {
            this.timer = timer;
            this.scenesRunner = scenesRunner;
            this.CheckSerializeFields();
        }

        public void ShowGUI()
        {
            GUILayoutOption glo = GUILayout.Height(SearchPanelEV.ButtonHeight);
            GUILayoutOption glo2 = GUILayout.Width(SearchPanelEV.StartButtonWidth);
            GUILayout.BeginHorizontal();
                ShowProgress();

                GUILayout.BeginVertical();
                    if (GUILayout.Button("Stop searching (Alt+Q)", glo, glo2)
                        || Event.current.keyCode == KeyCode.AltGr && Event.current.keyCode == KeyCode.Q)
                        EventSender.SendEvent(StopBtnClicked);
                    timer.ShowGUI();
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void ShowProgress()
        {
            GUILayout.BeginVertical(HelperGUI.GUIStyleFrame2());
                ShowProgressBar();

                GUILayout.Space(4);

                GUILayout.BeginHorizontal();
                    ShowSceneInProgress();
                    GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
         
        private void ShowSceneInProgress()
        { 
            var curScene = scenesRunner.CurrScene;
            var scenes = scenesRunner.Scenes;
            if (scenes == null)
                return;
            if (curScene >= 0 && curScene < scenes.Count())
            {
                GUILayout.Box(scenes[curScene], EditorStyles.helpBox);
            }
        }

        private void ShowProgressBar()
        {
            var sceneInProgress = scenesRunner.CurrScene;
            var scenesInProgres = scenesRunner.Scenes;
            var sceneObjs = scenesRunner.SceneObjs;
            var currExecuteObj = scenesRunner.CurrSceneObj;
            if(scenesInProgres == null || scenesInProgres.Count()==0)
                return;
            float progress = 100 * sceneInProgress / (float)scenesInProgres.Count();
            float progressGUI = (float)Math.Round(progress, 1);

            float progress2;
            if (sceneObjs == null || sceneObjs.Count() == 0 || currExecuteObj < 0)
                progress2 = 0;
            else
                progress2 = 100 * currExecuteObj / (float)sceneObjs.Count();

            Rect rect = EditorGUILayout.BeginVertical();
                rect.x += 4;
                rect.width -= 8;
                EditorGUI.ProgressBar(rect, (progressGUI + progress2 / scenesInProgres.Count()) / 100, 
                    string.Format("{0}({2}%)/{1}", (sceneInProgress + 1), scenesInProgres.Count(), 
                    (int)progress2));
                GUILayout.Space(20);
            GUILayout.EndVertical();
        }
    }
}

#endif