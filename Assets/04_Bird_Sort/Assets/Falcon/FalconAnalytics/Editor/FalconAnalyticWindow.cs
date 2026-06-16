using System.Collections;
using Falcon.FalconCore.Editor.Models;
using Falcon.FalconCore.Editor.Repositories;
using Falcon.FalconCore.Editor.Services;
using UnityEditor;

namespace Falcon.FalconAnalytics.Editor
{
    public class FalconAnalyticWindow : EditorWindow
    {
        [MenuItem("Falcon/Falcon Analytic/Refresh")]
        public static void ShowWindow()
        {
            new EditorSequence(Refresh()).Start();
        }

        private static IEnumerator Refresh()
        {
            var a = new FalconAnalyticInstallResponder();
            FPlugin plugin;
            while (!FPluginRepo.TryGet("FalconAnalytics", out plugin))
            {
                yield return null;
            }
            yield return a.OnPluginInstalled(plugin.InstalledDirectory);
        }
    }
}