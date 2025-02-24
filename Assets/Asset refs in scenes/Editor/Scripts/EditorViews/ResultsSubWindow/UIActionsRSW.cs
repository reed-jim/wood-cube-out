#if UNITY_EDITOR

using System.IO;
using System.Linq;
using SearchEngine.EditorViews.Tables;
using SearchEngine.Additions;
using SearchEngine.EditorViews.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SearchEngine.EditorViews.ResultsSubWindow
{
    public class UIActionsRSW : IEditorView
    {
        public static int TitleObjWidth = 186;
        public static int ContentObjWidth = 150;
        WarningMsgEV warnMsg = new WarningMsgEV();

        public void ShowGUI()
        {
            warnMsg.ShowGUI();
        }

        public bool TrySelectGOOnScene(DataGOPath g)
        {
            var rootGO =
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                    .GetRootGameObjects()
                    .Where(x => x.transform.GetSiblingIndex() == g.IndexPaths[0]);

            if (!rootGO.Any())
                return false;

            Transform foundObj = rootGO.First().transform;
            for (int i = 1; i < g.IndexPaths.Count(); i++)
            {
                if (g.IndexPaths[i] >= foundObj.childCount)
                    return false;
                foundObj = foundObj.GetChild(g.IndexPaths[i]);
            }

            if(!g.Name.Equals(foundObj.name))
                return false;
            Selection.activeTransform = foundObj;
            return true;
        }
        
        public void ShowGOButton(DataGOPath dataGoPath, string scenePath, int rowH)
        {
            if (!GUILayout.Button(dataGoPath.Name, GUILayout.ExpandWidth(true), GUILayout.Height(rowH)))
                return;

            if (!EditorSceneManager.GetActiveScene().path.Equals(scenePath)
                && EditorSceneManager.GetActiveScene().isDirty)
            {
                warnMsg.SetWarningMsg("Current scene is dirty.\nPlease save scene before.");
            }
            else if (HelperGeneral.OpenScene(scenePath, false))
            {
                if (!TrySelectGOOnScene(dataGoPath))
                {
                    warnMsg.SetWarningMsg("Game object not found");
                }
            }
            else
            {
                warnMsg.SetWarningMsg("Can not open scene " + Path.GetFileName(scenePath));                    
            }
        }
    }
}

#endif