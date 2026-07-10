using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

public class EmbedXCFrameworks
{
    [PostProcessBuild(1000)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS) return;

        string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        PBXProject project = new PBXProject();
        project.ReadFromFile(projectPath);
        string mainTargetGuid = project.GetUnityMainTargetGuid();

        // List<string> frameworks = new List<string> {
        //     "AppLovinSDK",
        //     "ATOM",
        //     "FBAEMKit",
        //     "FBAudienceNetwork",
        //     "FBSDKCoreKit_Basics",
        //     "FBSDKCoreKit",
        //     "FBSDKGamingServicesKit",
        //     "FBSDKLoginKit",
        //     "FBSDKShareKit",
        //     "InMobiSDK",
        //     "MolocoSDK",
        //     "OMSDK_Pubnative",
        //     "OMSDK_Smaato",
        //     "SmaatoSDKBanner",
        //     "SmaatoSDKCore",
        //     "SmaatoSDKInAppBidding",
        //     "SmaatoSDKInterstitial",
        //     "SmaatoSDKNative",
        //     "SmaatoSDKOpenMeasurement",
        //     "SmaatoSDKOutstream",
        //     "SmaatoSDKRewardedAds",
        //     "SmaatoSDKRichMedia",
        //     "SmaatoSDKVideo"
        // };

        List<string> frameworks = new List<string> {
            "FBAEMKit",
            "FBAudienceNetwork",
            "FBSDKCoreKit_Basics",
            "FBSDKCoreKit",
            "FBSDKGamingServicesKit",
            "FBSDKLoginKit",
            "FBSDKShareKit",
        };

        foreach (string framework in frameworks)
        {
            string frameworkName = framework + ".xcframework";
            var src = Path.Combine("Pods", framework, "XCFrameworks", frameworkName);
            var frameworkPath = project.AddFile(src, src);
            project.AddFileToBuild(mainTargetGuid, frameworkPath);
            project.AddFileToEmbedFrameworks(mainTargetGuid, frameworkPath);
        }
        // Write.
        project.WriteToFile(projectPath);
    }
}
