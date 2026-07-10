// using UnityEditor;
// using UnityEditor.Callbacks;
// using UnityEditor.iOS.Xcode;
// using UnityEditor.iOS.Xcode.Extensions;

// public class EmbedXCFrameworks
// {
//     [PostProcessBuild(999)]
//     public static void OnPostProcessBuild(BuildTarget target, string path)
//     {
//         if (target != BuildTarget.iOS)
//             return;

//         string pbxPath = PBXProject.GetPBXProjectPath(path);

//         PBXProject project = new PBXProject();
//         project.ReadFromFile(pbxPath);

// #if UNITY_2019_3_OR_NEWER
//         string mainTarget = project.GetUnityMainTargetGuid();
//         string frameworkTarget = project.GetUnityFrameworkTargetGuid();
// #else
//         string mainTarget = project.TargetGuidByName("Unity-iPhone");
//         string frameworkTarget = mainTarget;
// #endif

//         string[] frameworks =
//         {
//             "AppLovinSDK.xcframework",
//             "ATOM.xcframework",
//             "FBAEMKit.xcframework",
//             "FBAudienceNetwork.xcframework",
//             "FBSDKCoreKit_Basics.xcframework",
//             "FBSDKCoreKit.xcframework",
//             "FBSDKGamingServicesKit.xcframework",
//             "FBSDKLoginKit.xcframework",
//             "FBSDKShareKit.xcframework",
//             "InMobiSDK.xcframework",
//             "MolocoSDK.xcframework",
//             "OMSDK_Pubnative.xcframework",
//             "OMSDK_Smaato.xcframework",
//             "SmaatoSDKBanner.xcframework",
//             "SmaatoSDKCore.xcframework",
//             "SmaatoSDKInAppBidding.xcframework",
//             "SmaatoSDKInterstitial.xcframework",
//             "SmaatoSDKNative.xcframework",
//             "SmaatoSDKOpenMeasurement.xcframework",
//             "SmaatoSDKOutstream.xcframework",
//             "SmaatoSDKRewardedAds.xcframework",
//             "SmaatoSDKRichMedia.xcframework",
//             "SmaatoSDKVideo.xcframework"
//         };

//         foreach (var framework in frameworks)
//         {
//             string projectPath = "Frameworks/" + framework;

//             string fileGuid = project.FindFileGuidByProjectPath(projectPath);

//             if (string.IsNullOrEmpty(fileGuid))
//             {
//                 fileGuid = project.AddFile(projectPath, projectPath, PBXSourceTree.Source);
//             }

//             project.AddFileToBuild(frameworkTarget, fileGuid);
//             project.AddFileToEmbedFrameworks(mainTarget, fileGuid);
//         }

//         project.WriteToFile(pbxPath);
//     }
// }
