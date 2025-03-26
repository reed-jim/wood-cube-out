#if UNITY_EDITOR 

using System;
using UnityEngine;
using SearchEngine.Additions;
using SearchEngine.EditorViews.GCModeFacade;
using SearchEngine.EditorViews.Tables;
using UnityEditor.SceneManagement;

namespace SearchEngine.EditorViews
{
    public class SPDefaultStateEV : ISPState
    {
        public event Action StartBtnClicked;
        public event Action SetLockedState;
        [SerializeField] private WarningCheckerManager warningChecker;

        private Texture SaveIcon;
        private Texture DontSaveIcon;
        private GUIStyle redButton;

        public GCModeUIShowerEV UiShower { private get; set; }

        public SPDefaultStateEV(WarningCheckerManager warningChecker)
        {
            this.warningChecker = warningChecker;
            this.CheckSerializeFields();
            SaveIcon = (Texture)Resources.Load("UI/SaveIcon");
            DontSaveIcon = (Texture)Resources.Load("UI/DontSaveIcon");
        } 

        public void ShowGUI()
        {
            string warningMsg = warningChecker.Check();
            if (!string.IsNullOrEmpty(warningMsg))
            {
                if (Event.current.type == EventType.Repaint)
                    EventSender.SendEvent(SetLockedState);
            }

            GUILayout.BeginHorizontal();
            if (UiShower != null && UiShower.CurrToolbar == 2)
            {
                GUILayout.FlexibleSpace();
                ShowStartButtons();
            }
            else
            {
                ShowBackNextButton();                
            }
            GUILayout.EndHorizontal();
        }

        private void ShowStartButtons()
        {
            GUILayoutOption glo = GUILayout.Height(SearchPanelEV.ButtonHeight);
            GUILayoutOption glo2 = GUILayout.Width(SearchPanelEV.StartButtonWidth);
            if (redButton == null)
            {
                redButton = (GUIStyle)HelperGUI.CopyObject(GUI.skin.button, 1);
                redButton.normal.textColor = Color.red;                
            }

            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUIContent content;

                content = new GUIContent("Start search without saving scene", DontSaveIcon);
                if (GUILayout.Button(content, redButton, glo, glo2))
                    EventSender.SendEvent(StartBtnClicked);

                content = new GUIContent("Save scene and Start search", SaveIcon);
                if (GUILayout.Button(content, redButton, glo, glo2))
                {
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                    EventSender.SendEvent(StartBtnClicked);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Start search", glo, glo2))
                    EventSender.SendEvent(StartBtnClicked);
            }
        }
        
        private void ShowBackNextButton()
        {
            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayoutOption glo = GUILayout.Height(SearchPanelEV.ButtonHeight);
                GUILayoutOption glo2 = GUILayout.Width(80);
                if (UiShower != null)
                {
                    GUI.enabled = UiShower.CurrToolbar < 2;
                    if (GUILayout.Button("Next", glo, glo2))
                    {
                        UiShower.SetNextToolbarItem();
                    }
                    GUI.enabled = true;
                }
            GUILayout.EndVertical();
        }
    }
}

#endif