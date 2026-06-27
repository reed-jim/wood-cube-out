using System;
using System.Collections;
using System.Collections.Generic;
using Falcon.FalconCore.Editor.Models;
using Falcon.FalconCore.Editor.Repositories;
using Falcon.FalconCore.Editor.Services;
using Falcon.FalconCore.Editor.Utils;
using Falcon.FalconCore.Scripts.Repositories;
using Falcon.FalconCore.Scripts.Utils;
using Unity.CodeEditor;
using UnityEditor;

namespace Falcon.FalconCore.Editor.Views
{
    public class FalconCoreWindow : EditorWindow
    {
        [MenuItem("Falcon/FalconCore/Refresh")]
        public static void ShowWindow()
        {
            new EditorSequence(Refresh()).Start();
        }
        
        [MenuItem("Falcon/FalconCore/DebugLog/Enable")]
        public static void EnableDebugLog()
        {
            DefineSymbols.Add("FALCON_LOG_DEBUG");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
                
            EditorUtility.RequestScriptReload();
            CodeEditor.CurrentEditor.Initialize(CodeEditor.CurrentEditorInstallation);
        }
        
        [MenuItem("Falcon/FalconCore/DebugLog/Disable")]
        public static void DisableDebugLog()
        {
            DefineSymbols.Remove("FALCON_LOG_DEBUG");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
                
            EditorUtility.RequestScriptReload();
            CodeEditor.CurrentEditor.Initialize(CodeEditor.CurrentEditorInstallation);
        }
        
        [MenuItem("Falcon/FalconCore/ClearData")]
        public static void ClearData()
        {
            new FFile(FDataPool.DataFilePath).Save(new Dictionary<String, String>());
        }

        private static IEnumerator Refresh()
        {
            var a = new FalconCoreInstallResponder();
            
            FPlugin plugin;
            while (!FPluginRepo.TryGet("FalconCore", out plugin))
            {
                yield return null;
            }
            yield return a.OnPluginInstalled(plugin.InstalledDirectory);
        }
    }
}