#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SearchEngine.EditorWindows;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace SearchEngine.Additions
{
    public static class HelperGeneral
    {
        public static bool OpenScene(string path, bool reLoadActiveScene)
        {
            if (!reLoadActiveScene)
            {
                if (EditorSceneManager.GetActiveScene().path.Equals(path))
                    return true;
            }

            try
            {
                EditorSceneManager.OpenScene(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat(MainEW.PluginName +": can not open scene" + path, e);
                return false;
            }
        }

        public static IEnumerable<string> GetAssets(string path, string assetsFormat="")
        {
            if (!Directory.Exists(path))
                return new string[0];
            if (string.IsNullOrEmpty(assetsFormat))
                assetsFormat = "*";

#if UNITY_WEBPLAYER
            var assets = Directory.GetFiles(
                path,
                string.Format("*.{0}", assetsFormat));
#else
            var assets = Directory.GetFiles(path, 
                string.Format("*.{0}", assetsFormat), 
                SearchOption.AllDirectories);
#endif
            return assets.Where(f => !f.EndsWith(".meta"));
        }

        public static int[] GetTransformIndexPaths(Transform tr)
        {
            List<int> result = new List<int>();
            while (tr != null)
            {
                result.Insert(0, tr.GetSiblingIndex());
                tr = tr.parent;
            }
            return result.ToArray();
        }

        public static bool MouseDownInRect(Rect rect)
        {
            return 
                Event.current.type == EventType.MouseDown
                && rect.Contains(Event.current.mousePosition);
        }
        public static void MouseDownInRect(Rect rect, ref bool toggleState)
        {
            if (MouseDownInRect(rect))
            {
                toggleState = !toggleState;
            }
        }

        public static string GetGCResDir()
        {
            return GetDir("Assets", MainEW.PluginName, "Editor", "Resources");
        }

        public static string GetDir(string beginPath, string firstFolder, params string[] folders)
        {
            if (string.IsNullOrEmpty(beginPath)
                || string.IsNullOrEmpty(firstFolder))
                return null;

#if UNITY_WEBPLAYER
            var gbDirs = Directory.GetDirectories(beginPath, firstFolder);
#else
            var gbDirs = Directory.GetDirectories(beginPath, firstFolder, SearchOption.AllDirectories);
#endif

            List<string> dirs = new List<string>(gbDirs);
            List<string> subdirs = new List<string>();
            foreach (var f in folders)
            {
                foreach (var dir in dirs)
                {
                    var resDirs = Directory.GetDirectories(dir, f);
                    if (resDirs.Any())
                        subdirs.Add(resDirs[0]);
                }
                if(!subdirs.Any())
                    return null;
                dirs = subdirs;
                subdirs = new List<string>();
            }
            
            if(dirs.Count>1)
                return null;
            return dirs[0];
        }
    }
}

#endif