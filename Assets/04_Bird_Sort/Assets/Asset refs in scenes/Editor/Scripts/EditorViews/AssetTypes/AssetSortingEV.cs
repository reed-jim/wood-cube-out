#if UNITY_EDITOR

using System;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.Data;
using SearchEngine.Memento;
using UnityEngine;

namespace SearchEngine.EditorViews.AssetTypes
{
    public class AssetSortingEV : IEditorView, IOriginator<AssetSortingEV.Memento>
    {
        public event Action ChangeEnable;
        public event Action ChangeSorting;
        public bool Enabled { get; set; }
        private AssetSortingTypes[] options;
        
        public AssetSortingEV(params AssetSortingTypes[] types)
        {
            options = types;
        }

        public void ShowGUI()
        {
            bool entryEnabled = Enabled;
            int changePos = -1;

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                Enabled = GUILayout.Toggle(Enabled, "Sorting", GUILayout.Width(65));
                GUI.enabled = Enabled;
                if (options.Any())
                {
                    GUILayout.BeginHorizontal();
                        GUILayout.BeginHorizontal(HelperGUI.GUIStyleFrame1(), GUILayout.Width(50));
                            GUILayout.Label(options[0].ToString());
                        GUILayout.EndHorizontal();
                        for (int i = 1; i < options.Length; i++)
                        {
                            if (GUILayout.Button(HelperGUI.RowsButton(), HelperGUI.GUIStyleButtonWithoutPadding, GUILayout.Height(16), GUILayout.Width(16)))
                            {
                                changePos = i;
                            }

                            GUILayout.BeginHorizontal(HelperGUI.GUIStyleFrame1(), GUILayout.Width(50));
                                GUILayout.Label(options[i].ToString());
                            GUILayout.EndHorizontal();
                }
                    GUILayout.EndHorizontal();
                }
                GUI.enabled = true;
            GUILayout.EndHorizontal();

            if (entryEnabled != Enabled)
            {
                EventSender.SendEvent(ChangeEnable);
            }
            if (changePos >= 0)
            {
                var temp = options[changePos];
                options[changePos] = options[changePos - 1];
                options[changePos - 1] = temp;
                EventSender.SendEvent(ChangeSorting);
            }
        }
        
        public Comparison<AssetInfoData> GetComparer()
        {
            return SortingHelper.GetComparer(options);
        }


        #region  memento part
        public Memento GetMemento()
        {
            return new Memento
            {
                options = this.options.ToArray(),
                Enabled = this.Enabled,
            };
        }
        public void SetMemento(Memento mem)
        {
            if (mem == null || !mem.Validate())
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }

            this.options = mem.options.ToArray();
            this.Enabled = mem.Enabled;
        }

        [Serializable]
        public class Memento : IValidatable
        {
            public AssetSortingTypes[] options;
            public bool Enabled;

            public bool Validate()
            {
                return
                    options != null;
            }
        }
        #endregion
    }
}

#endif