#if UNITY_EDITOR 

using System;
using UnityEngine;
using SearchEngine.Additions;
using SearchEngine.EditorViews.GCModeFacade;
using SearchEngine.EditorViews.Tables;
using UnityEditor;

namespace SearchEngine.EditorViews
{
    public class SPLockedStateEV : ISPState
    {
        public event Action SetDefaultState;
        public event Action AssetsWarning;
        public event Action ScenesWarning;
        [SerializeField] private WarningCheckerManager warnChecker;
        [SerializeField] private IWarningChecker assetsWarnChecker;
        [SerializeField] private IWarningChecker scenesWarnChecker;

        public GCModeUIShowerEV UiShower { private get; set; }

        public SPLockedStateEV(WarningCheckerManager warnChecker, IWarningChecker assetsWarnChecker, IWarningChecker scenesWarnChecker)
        {
            this.warnChecker = warnChecker;
            this.assetsWarnChecker = assetsWarnChecker;
            this.scenesWarnChecker = scenesWarnChecker;
            this.CheckSerializeFields();
        }

        public void ShowGUI()
        {
            GUILayoutOption glo = GUILayout.Height(SearchPanelEV.ButtonHeight);
            EditorGUILayout.BeginHorizontal();
                ShowWarningMsg();

                if (UiShower != null)
                {
                    if(UiShower.CurrToolbar < 2)
                    {
                        GUILayoutOption glo2 = GUILayout.Width(SearchPanelEV.StartButtonWidth / 3);
                        if (GUILayout.Button("Next", glo, glo2))
                        {
                            if (UiShower.CurrToolbar == 0)
                            {
                                if(assetsWarnChecker.Check())
                                    UiShower.SetNextToolbarItem();
                                else
                                    EventSender.SendEvent(AssetsWarning);
                            }
                            else
                            {
                                if (scenesWarnChecker.Check())
                                    UiShower.SetPrevToolbarItem();
                                else
                                    EventSender.SendEvent(ScenesWarning);
                            }
                        }
                    }
                    else
                    {
                        GUI.enabled = false;
                        GUILayoutOption glo2 = GUILayout.Width(SearchPanelEV.StartButtonWidth);
                        GUILayout.Button("Start search", glo, glo2);
                        GUI.enabled = true;
                    }
                }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowWarningMsg()
        {
            string warningMsg = warnChecker.Check();
            if (string.IsNullOrEmpty(warningMsg))
            {
                if(Event.current.type == EventType.Repaint)
                    EventSender.SendEvent(SetDefaultState);
            }
            
            EditorGUILayout.HelpBox(warningMsg, MessageType.Warning, true);
        }
    }
}

#endif