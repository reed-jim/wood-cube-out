#if UNITY_EDITOR

using System.Linq;
using SearchEngine.EditorViews;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SearchEngine.EditorWindows
{
    public class MainEW : EditorWindow
    {
        public const string Version = "1.0";
        public const string PluginName = "Asset refs in scenes";
        
        private static MainEW instance = null;

        private SearchEnginesEV engines = new SearchEnginesEV();

        public static MainEW Instance
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
            var windows = Resources.FindObjectsOfTypeAll<MainEW>();
            if (windows.Any())
            {
                instance = windows.First();
            }
            else
            {
                instance = CreateInstance<MainEW>();
                instance.titleContent = new GUIContent(PluginName);
            }
        }
        
        [MenuItem("Window/" + PluginName+" v"+Version)]
        private static void ShowInstance()
        { 
            Instance.Show();
        }
        
        private void OnGUI()
        { 
            engines.ShowGUI(); 
        }    
            
        private void OnEnable()
        {
            engines.ReloadEngines();
        }

        private void OnInspectorUpdate()  
        {
            if(engines.IsSearching)
                Repaint();
        }

        private void OnDisable()
        {
            engines.Dispose();
        }
    }
}

#endif