using System.IO;
using UnityEditor;
using UnityEngine;

namespace Falcon.FalconCore.Editor.Views
{
    public class FalconScriptSymbol : EditorWindow
    {
        [MenuItem("Falcon/Falcon Analytic/FixScriptingDefine")]
        public static void FixSymbol()
        {
            string appsflyer = "USE_APPSFLYER";
            string adjust = "USE_ADJUST";
#if UNITY_ANDROID
            string a = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
#elif UNITY_IOS
            string a = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
#endif
            if (a.Length != 0 && !a.EndsWith(";")) a += ";";
            //remove all
            a = a.Replace(appsflyer, "");
            a = a.Replace(adjust, "");
            a = FormatTag(a);
            //check and add
            //appsflyer
            if (File.Exists(Application.dataPath + "/Appsflyer/AppsFlyer.cs"))
            {
                a += appsflyer + ";";
            }

            //adjust
            if (File.Exists(Application.dataPath + "/Adjust/Unity/Adjust.cs"))
            {
                a += adjust + ";";
            }

            //format
            a = FormatTag(a);
            if (a.Length != 0 && !a.EndsWith(";")) a += ";";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, a);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, a);
        }

        static string FormatTag(string a)
        {
            a = a.Replace(";;", ";");
            if (a.StartsWith(";"))
            {
                a = a.Remove(0, 1);
            }

            return a;
        }
    }
}