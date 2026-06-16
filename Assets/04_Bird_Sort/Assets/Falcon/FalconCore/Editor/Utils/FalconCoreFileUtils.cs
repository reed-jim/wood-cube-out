using System.IO;
using UnityEditor;
using UnityEngine;

namespace Falcon.FalconCore.Editor.Utils
{
    [InitializeOnLoad]
    public static class FalconCoreFileUtils
    {
        public static string ApplicationDataPath { get; private set; }
        
        static FalconCoreFileUtils()
        {
            ApplicationDataPath = Application.dataPath;
        }
        
        public static string GetFalconPluginFolder()
        {
            //will use FalconCore's parent folder as milestone for other plugins
            string[] directory =
                Directory.GetDirectories(ApplicationDataPath, "FalconCore", SearchOption.AllDirectories);
            
            if (directory.Length == 0) return null;
            
            DirectoryInfo result = Directory.GetParent(directory[0].Contains("Release") ? directory[1] : directory[0]);
            return result?.FullName ;
        }

        public static void RewriteLineInFile(string filePath, string oldLine, string newLine)
        {
            string[] arrLine = File.ReadAllLines(filePath);

            for (int i = 0; i < arrLine.Length; i++)
            {
                if (arrLine[i].StartsWith(oldLine))
                {
                    arrLine[i] = newLine;
                    File.WriteAllLines(filePath, arrLine);
                    return;
                }
            }
        }

    }
}