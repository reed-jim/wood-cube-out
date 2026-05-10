// // Copyright (c) 2024 GplayJSC (gplayjsc.com)
// // 
// // Author: Axolotl (lamanh.w@gmail.com)
// // 
// // Created: 04/06/2024
// // 
// // File: IdfaPostProcess.cs
// // 
// // Note:

// using System.IO;
// using AppleAuth.Editor;
// using UnityEditor;
// using UnityEditor.Callbacks;
// using UnityEditor.iOS.Xcode;

// namespace GplayAppFlyer.Editor
// {
//     public static class IdfaPostProcess
//     {
//         [PostProcessBuild(1000)]
//         public static void UpdatePlist(BuildTarget buildTarget, string pathToBuiltProject)
//         {
//             if (buildTarget != BuildTarget.iOS) return;
//             var plistPath = pathToBuiltProject + "/Info.plist";
//             var plist = new PlistDocument();
//             plist.ReadFromString(File.ReadAllText(plistPath));
//             var rootDict = plist.root;
//             rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");
//             rootDict.SetString("NSUserTrackingUsageDescription", "Your data will be used to provide you a better and personalized ad experience.");
//             File.WriteAllText(plistPath, plist.WriteToString());

//             // Sign In With Apple
//             // var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
//             // var project = new PBXProject();
//             // project.ReadFromString(System.IO.File.ReadAllText(projectPath));
//             // var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null, project.GetUnityMainTargetGuid());
//             // manager.AddSignInWithAppleWithCompatibility();
//             // manager.WriteToFile();

//             // // Framework
//             // var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
//             // var project = new PBXProject();
//             // project.ReadFromFile(projectPath);
//             // var targetName = project.GetUnityFrameworkTargetGuid(); // note, not "project." ...
//             // var targetGUID = project.TargetGuidByName(targetName);
//             // project.AddFrameworkToProject(targetGUID, "AppTrackingTransparency.framework", true);
//         }
//     }
// }