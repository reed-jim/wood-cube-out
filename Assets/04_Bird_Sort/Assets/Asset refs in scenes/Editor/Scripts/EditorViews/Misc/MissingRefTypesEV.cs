#if UNITY_EDITOR

using System;
using SearchEngine.Additions;
using SearchEngine.Memento;
using UnityEngine;

namespace SearchEngine.EditorViews
{
    public class MissingRefTypesEV : IEditorView, IOriginator<MissingRefTypesEV.Memento>
    {
        [SerializeField] private IEditorView warnMsg;
        private bool gameObjects = true;
        private bool components = true;
        private bool inspectorProps = true;

        public bool GameObjects
        {
            get
            {
                return gameObjects;
            }

            private set
            {
                gameObjects = value;
            }
        }
        public bool Components
        {
            get
            {
                return components;
            }

            private set
            {
                components = value;
            }
        }
        public bool InspectorProps
        {
            get
            {
                return inspectorProps;
            }

            private set
            {
                inspectorProps = value;
            }
        }

        public MissingRefTypesEV(IEditorView warnMsg)
        {
            this.warnMsg = warnMsg;
            this.CheckSerializeFields();
        }

        public void ShowGUI()
        {
            GameObjects = GUILayout.Toggle(GameObjects, 
                "GameObjects: missing refs to prefabs and models");

            Components = GUILayout.Toggle(Components,
                "GameObject components: missing refs to mono scripts");

            InspectorProps = GUILayout.Toggle(InspectorProps,
                "Component`s inspector properties:");
            GUILayout.BeginHorizontal();
                GUILayout.Label(string.Empty, GUILayout.Width(16));
                GUI.enabled = false;
                    GUILayout.Toggle(InspectorProps,
                        "missing refs to assets");
                GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
                GUILayout.Label(string.Empty, GUILayout.Width(16));
                GUI.enabled = false;
                    GUILayout.Toggle(InspectorProps, 
                        "missing refs to scene game objects and their components");
                GUI.enabled = true;
            GUILayout.EndHorizontal();

            warnMsg.ShowGUI();
        }

        #region  memento part
        public Memento GetMemento()
        {
            return new Memento
            {
                gameObjects = this.gameObjects,
                components = this.components,
                inspectorProps = this.inspectorProps,
            };
        }

        public void SetMemento(Memento mem)
        {
            if (mem == null)
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }

            gameObjects = mem.gameObjects;
            components = mem.components;
            inspectorProps = mem.inspectorProps;
        }

        [Serializable]
        public class Memento
        {
            public bool gameObjects;
            public bool components;
            public bool inspectorProps;
        }
        #endregion
    }
}

#endif
