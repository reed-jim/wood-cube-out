#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.iOS.Xcode;
public class CustomPostprocessScript : ScriptableObject
{
    public DefaultAsset m_entitlementsFile;
/// <summary>
    /// Description for IDFA request notification 
    /// [sets NSUserTrackingUsageDescription]
    /// </summary>
    const string TrackingDescription =
        "Your data will be used to deliver personalized advertisements and provide a better in-game rewarded ad experience.";

    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // Get plist
            var plistPath = pathToBuiltProject + "/Info.plist";
            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            // Get root
            var rootDict = plist.root;

            // add key
            rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");
           // rootDict.SetString("SKAdNetworkIdentifier", "su67r6k2v3.skadnetwork");            
            rootDict.SetString("NSLocationAlwaysUsageDescription", "This allows us to continuously provide region-specific game events and localized content even when the app is in the background.");
            rootDict.SetString("NSLocationWhenInUseUsageDescription", "This identifier is used to customize in-game events, content, and offers based on your geographic region.");
            rootDict.SetString("NSCalendarsUsageDescription", "This allows you to add reminders for upcoming in-game events, tournaments, and special reward claims to your calendar.");
            rootDict.SetString("NSUserTrackingUsageDescription", TrackingDescription);

            var appID = "ca-app-pub-9457878244675693~3008927510";
            rootDict.SetString("GADApplicationIdentifier", appID);
            var buildKey = "UIBackgroundModes";
            rootDict.CreateArray(buildKey).AddString("remote-notification");
            if (rootDict.values.ContainsKey("UIApplicationExitsOnSuspend"))
            {
                rootDict.values.Remove("UIApplicationExitsOnSuspend");
            }


 PlistElementArray SKAdNetworkDict = null;
            if(plist.root.values.ContainsKey("SKAdNetworkItems"))
            {
            SKAdNetworkDict = plist.root.values["SKAdNetworkItems"].AsArray();
            }
            else
            {
            SKAdNetworkDict = plist.root.CreateArray("SKAdNetworkItems");
            }
            var skd_1 = SKAdNetworkDict.AddDict();
skd_1.SetString("SKAdNetworkIdentifier", "97r2b46745.skadnetwork");
var skd_2 = SKAdNetworkDict.AddDict();
skd_2.SetString("SKAdNetworkIdentifier", "3rd42ekr43.skadnetwork");
var skd_3 = SKAdNetworkDict.AddDict();
skd_3.SetString("SKAdNetworkIdentifier", "22mmun2rn5.skadnetwork");
var skd_4 = SKAdNetworkDict.AddDict();
skd_4.SetString("SKAdNetworkIdentifier", "a8cz6cu7e5.skadnetwork");
var skd_5 = SKAdNetworkDict.AddDict();
skd_5.SetString("SKAdNetworkIdentifier", "252b5q8x7y.skadnetwork");
var skd_6 = SKAdNetworkDict.AddDict();
skd_6.SetString("SKAdNetworkIdentifier", "9nlqeag3gk.skadnetwork");
var skd_7 = SKAdNetworkDict.AddDict();
skd_7.SetString("SKAdNetworkIdentifier", "f7s53z58qe.skadnetwork");
var skd_8 = SKAdNetworkDict.AddDict();
skd_8.SetString("SKAdNetworkIdentifier", "2u9pt9hc89.skadnetwork");
var skd_9 = SKAdNetworkDict.AddDict();
skd_9.SetString("SKAdNetworkIdentifier", "prcb7njmu6.skadnetwork");
var skd_10 = SKAdNetworkDict.AddDict();
skd_10.SetString("SKAdNetworkIdentifier", "4fzdc2evr5.skadnetwork");
var skd_11 = SKAdNetworkDict.AddDict();
skd_11.SetString("SKAdNetworkIdentifier", "s39g8k73mm.skadnetwork");
var skd_12 = SKAdNetworkDict.AddDict();
skd_12.SetString("SKAdNetworkIdentifier", "t38b2kh725.skadnetwork");
var skd_13 = SKAdNetworkDict.AddDict();
skd_13.SetString("SKAdNetworkIdentifier", "5l3tpt7t6e.skadnetwork");
var skd_14 = SKAdNetworkDict.AddDict();
skd_14.SetString("SKAdNetworkIdentifier", "6yxyv74ff7.skadnetwork");
var skd_15 = SKAdNetworkDict.AddDict();
skd_15.SetString("SKAdNetworkIdentifier", "578prtvx9j.skadnetwork");
var skd_16 = SKAdNetworkDict.AddDict();
skd_16.SetString("SKAdNetworkIdentifier", "32z4fx6l9h.skadnetwork");
var skd_17 = SKAdNetworkDict.AddDict();
skd_17.SetString("SKAdNetworkIdentifier", "8s468mfl3y.skadnetwork");
var skd_18 = SKAdNetworkDict.AddDict();
skd_18.SetString("SKAdNetworkIdentifier", "5tjdwbrq8w.skadnetwork");
var skd_19 = SKAdNetworkDict.AddDict();
skd_19.SetString("SKAdNetworkIdentifier", "su67r6k2v3.skadnetwork");
var skd_20 = SKAdNetworkDict.AddDict();
skd_20.SetString("SKAdNetworkIdentifier", "kbmxgpxpgc.skadnetwork");
var skd_21 = SKAdNetworkDict.AddDict();
skd_21.SetString("SKAdNetworkIdentifier", "7fmhfwg9en.skadnetwork");
var skd_22 = SKAdNetworkDict.AddDict();
skd_22.SetString("SKAdNetworkIdentifier", "mqn7fxpca7.skadnetwork");
var skd_23 = SKAdNetworkDict.AddDict();
skd_23.SetString("SKAdNetworkIdentifier", "44jx6755aq.skadnetwork");
var skd_24 = SKAdNetworkDict.AddDict();
skd_24.SetString("SKAdNetworkIdentifier", "hs6bdukanm.skadnetwork");
var skd_25 = SKAdNetworkDict.AddDict();
skd_25.SetString("SKAdNetworkIdentifier", "zq492l623r.skadnetwork");
var skd_26 = SKAdNetworkDict.AddDict();
skd_26.SetString("SKAdNetworkIdentifier", "av6w8kgt66.skadnetwork");
var skd_27 = SKAdNetworkDict.AddDict();
skd_27.SetString("SKAdNetworkIdentifier", "k674qkevps.skadnetwork");
var skd_28 = SKAdNetworkDict.AddDict();
skd_28.SetString("SKAdNetworkIdentifier", "yclnxrl5pm.skadnetwork");
var skd_29 = SKAdNetworkDict.AddDict();
skd_29.SetString("SKAdNetworkIdentifier", "424m5254lk.skadnetwork");
var skd_30 = SKAdNetworkDict.AddDict();
skd_30.SetString("SKAdNetworkIdentifier", "v72qych5uu.skadnetwork");
var skd_31 = SKAdNetworkDict.AddDict();
skd_31.SetString("SKAdNetworkIdentifier", "f38h382jlk.skadnetwork");
var skd_32 = SKAdNetworkDict.AddDict();
skd_32.SetString("SKAdNetworkIdentifier", "cstr6suwn9.skadnetwork");
var skd_33 = SKAdNetworkDict.AddDict();
skd_33.SetString("SKAdNetworkIdentifier", "9t245vhmpl.skadnetwork");
var skd_34 = SKAdNetworkDict.AddDict();
skd_34.SetString("SKAdNetworkIdentifier", "7ug5zh24hu.skadnetwork");
var skd_35 = SKAdNetworkDict.AddDict();
skd_35.SetString("SKAdNetworkIdentifier", "kbd757ywx3.skadnetwork");
var skd_36 = SKAdNetworkDict.AddDict();
skd_36.SetString("SKAdNetworkIdentifier", "4468km3ulz.skadnetwork");
var skd_37 = SKAdNetworkDict.AddDict();
skd_37.SetString("SKAdNetworkIdentifier", "e5fvkxwrpn.skadnetwork");
var skd_38 = SKAdNetworkDict.AddDict();
skd_38.SetString("SKAdNetworkIdentifier", "3qy4746246.skadnetwork");
var skd_39 = SKAdNetworkDict.AddDict();
skd_39.SetString("SKAdNetworkIdentifier", "wzmmz9fp6w.skadnetwork");
var skd_40 = SKAdNetworkDict.AddDict();
skd_40.SetString("SKAdNetworkIdentifier", "m8dbw4sv7c.skadnetwork");
var skd_41 = SKAdNetworkDict.AddDict();
skd_41.SetString("SKAdNetworkIdentifier", "ppxm28t8ap.skadnetwork");
var skd_42 = SKAdNetworkDict.AddDict();
skd_42.SetString("SKAdNetworkIdentifier", "qqp299437r.skadnetwork");
var skd_43 = SKAdNetworkDict.AddDict();
skd_43.SetString("SKAdNetworkIdentifier", "4pfyvq9l8r.skadnetwork");
var skd_44 = SKAdNetworkDict.AddDict();
skd_44.SetString("SKAdNetworkIdentifier", "tl55sbb4fm.skadnetwork");
var skd_45 = SKAdNetworkDict.AddDict();
skd_45.SetString("SKAdNetworkIdentifier", "mlmmfzh3r3.skadnetwork");
var skd_46 = SKAdNetworkDict.AddDict();
skd_46.SetString("SKAdNetworkIdentifier", "klf5c3l5u5.skadnetwork");
var skd_47 = SKAdNetworkDict.AddDict();
skd_47.SetString("SKAdNetworkIdentifier", "c6k4g5qg8m.skadnetwork");
var skd_48 = SKAdNetworkDict.AddDict();
skd_48.SetString("SKAdNetworkIdentifier", "9rd848q2bz.skadnetwork");
var skd_49 = SKAdNetworkDict.AddDict();
skd_49.SetString("SKAdNetworkIdentifier", "7rz58n8ntl.skadnetwork");
var skd_50 = SKAdNetworkDict.AddDict();
skd_50.SetString("SKAdNetworkIdentifier", "ejvt5qm6ak.skadnetwork");
var skd_51 = SKAdNetworkDict.AddDict();
skd_51.SetString("SKAdNetworkIdentifier", "5lm9lj6jb7.skadnetwork");
var skd_52 = SKAdNetworkDict.AddDict();
skd_52.SetString("SKAdNetworkIdentifier", "mtkv5xtk9e.skadnetwork");
var skd_53 = SKAdNetworkDict.AddDict();
skd_53.SetString("SKAdNetworkIdentifier", "6g9af3uyq4.skadnetwork");
var skd_54 = SKAdNetworkDict.AddDict();
skd_54.SetString("SKAdNetworkIdentifier", "uw77j35x4d.skadnetwork");
var skd_55 = SKAdNetworkDict.AddDict();
skd_55.SetString("SKAdNetworkIdentifier", "u679fj5vs4.skadnetwork");
var skd_56 = SKAdNetworkDict.AddDict();
skd_56.SetString("SKAdNetworkIdentifier", "rx5hdcabgc.skadnetwork");
var skd_57 = SKAdNetworkDict.AddDict();
skd_57.SetString("SKAdNetworkIdentifier", "g28c52eehv.skadnetwork");
var skd_58 = SKAdNetworkDict.AddDict();
skd_58.SetString("SKAdNetworkIdentifier", "cg4yq2srnc.skadnetwork");
var skd_59 = SKAdNetworkDict.AddDict();
skd_59.SetString("SKAdNetworkIdentifier", "275upjj5gd.skadnetwork");
var skd_60 = SKAdNetworkDict.AddDict();
skd_60.SetString("SKAdNetworkIdentifier", "wg4vff78zm.skadnetwork");
var skd_61 = SKAdNetworkDict.AddDict();
skd_61.SetString("SKAdNetworkIdentifier", "294l99pt4k.skadnetwork");
var skd_62 = SKAdNetworkDict.AddDict();
skd_62.SetString("SKAdNetworkIdentifier", "2fnua5tdw4.skadnetwork");
var skd_63 = SKAdNetworkDict.AddDict();
skd_63.SetString("SKAdNetworkIdentifier", "ydx93a7ass.skadnetwork");
var skd_64 = SKAdNetworkDict.AddDict();
skd_64.SetString("SKAdNetworkIdentifier", "523jb4fst2.skadnetwork");
var skd_65 = SKAdNetworkDict.AddDict();
skd_65.SetString("SKAdNetworkIdentifier", "cj5566h2ga.skadnetwork");
var skd_66 = SKAdNetworkDict.AddDict();
skd_66.SetString("SKAdNetworkIdentifier", "r45fhb6rf7.skadnetwork");
var skd_67 = SKAdNetworkDict.AddDict();
skd_67.SetString("SKAdNetworkIdentifier", "g2y4y55b64.skadnetwork");
var skd_68 = SKAdNetworkDict.AddDict();
skd_68.SetString("SKAdNetworkIdentifier", "n6fk4nfna4.skadnetwork");
var skd_69 = SKAdNetworkDict.AddDict();
skd_69.SetString("SKAdNetworkIdentifier", "n9x2a789qt.skadnetwork");
var skd_70 = SKAdNetworkDict.AddDict();
skd_70.SetString("SKAdNetworkIdentifier", "pwa73g5rt2.skadnetwork");
var skd_71 = SKAdNetworkDict.AddDict();
skd_71.SetString("SKAdNetworkIdentifier", "74b6s63p6l.skadnetwork");
var skd_72 = SKAdNetworkDict.AddDict();
skd_72.SetString("SKAdNetworkIdentifier", "44n7hlldy6.skadnetwork");
var skd_73 = SKAdNetworkDict.AddDict();
skd_73.SetString("SKAdNetworkIdentifier", "gta9lk7p23.skadnetwork");
var skd_74 = SKAdNetworkDict.AddDict();
skd_74.SetString("SKAdNetworkIdentifier", "84993kbrcf.skadnetwork");
var skd_75 = SKAdNetworkDict.AddDict();
skd_75.SetString("SKAdNetworkIdentifier", "pwdxu55a5a.skadnetwork");
var skd_76 = SKAdNetworkDict.AddDict();
skd_76.SetString("SKAdNetworkIdentifier", "6964rsfnh4.skadnetwork");
var skd_77 = SKAdNetworkDict.AddDict();
skd_77.SetString("SKAdNetworkIdentifier", "a7xqa6mtl2.skadnetwork");
var skd_78 = SKAdNetworkDict.AddDict();
skd_78.SetString("SKAdNetworkIdentifier", "c3frkrj4fj.skadnetwork");
var skd_79 = SKAdNetworkDict.AddDict();
skd_79.SetString("SKAdNetworkIdentifier", "3qcr597p9d.skadnetwork");
var skd_80 = SKAdNetworkDict.AddDict();
skd_80.SetString("SKAdNetworkIdentifier", "3sh42y64q3.skadnetwork");
var skd_81 = SKAdNetworkDict.AddDict();
skd_81.SetString("SKAdNetworkIdentifier", "5a6flpkh64.skadnetwork");
var skd_82 = SKAdNetworkDict.AddDict();
skd_82.SetString("SKAdNetworkIdentifier", "p78axxw29g.skadnetwork");
var skd_83 = SKAdNetworkDict.AddDict();
skd_83.SetString("SKAdNetworkIdentifier", "24t9a8vw3c.skadnetwork");
var skd_84 = SKAdNetworkDict.AddDict();
skd_84.SetString("SKAdNetworkIdentifier", "54nzkqm89y.skadnetwork");
var skd_85 = SKAdNetworkDict.AddDict();
skd_85.SetString("SKAdNetworkIdentifier", "6xzpu9s2p8.skadnetwork");
var skd_86 = SKAdNetworkDict.AddDict();
skd_86.SetString("SKAdNetworkIdentifier", "79pbpufp6p.skadnetwork");
var skd_87 = SKAdNetworkDict.AddDict();
skd_87.SetString("SKAdNetworkIdentifier", "9b89h5y424.skadnetwork");
var skd_88 = SKAdNetworkDict.AddDict();
skd_88.SetString("SKAdNetworkIdentifier", "feyaarzu9v.skadnetwork");
var skd_89 = SKAdNetworkDict.AddDict();
skd_89.SetString("SKAdNetworkIdentifier", "ggvn48r87g.skadnetwork");
var skd_90 = SKAdNetworkDict.AddDict();
skd_90.SetString("SKAdNetworkIdentifier", "glqzh8vgby.skadnetwork");
var skd_91 = SKAdNetworkDict.AddDict();
skd_91.SetString("SKAdNetworkIdentifier", "ludvb6z3bs.skadnetwork");
var skd_92 = SKAdNetworkDict.AddDict();
skd_92.SetString("SKAdNetworkIdentifier", "xy9t38ct57.skadnetwork");
var skd_93 = SKAdNetworkDict.AddDict();
skd_93.SetString("SKAdNetworkIdentifier", "zmvfpc5aq8.skadnetwork");
var skd_94 = SKAdNetworkDict.AddDict();
skd_94.SetString("SKAdNetworkIdentifier", "737z793b9f.skadnetwork");
var skd_95 = SKAdNetworkDict.AddDict();
skd_95.SetString("SKAdNetworkIdentifier", "mls7yz5dvl.skadnetwork");
var skd_96 = SKAdNetworkDict.AddDict();
skd_96.SetString("SKAdNetworkIdentifier", "w9q455wk68.skadnetwork");
var skd_97 = SKAdNetworkDict.AddDict();
skd_97.SetString("SKAdNetworkIdentifier", "lr83yxwka7.skadnetwork");
var skd_98 = SKAdNetworkDict.AddDict();
skd_98.SetString("SKAdNetworkIdentifier", "4w7y6s5ca2.skadnetwork");
var skd_99 = SKAdNetworkDict.AddDict();
skd_99.SetString("SKAdNetworkIdentifier", "dzg6xy7pwj.skadnetwork");
var skd_100 = SKAdNetworkDict.AddDict();
skd_100.SetString("SKAdNetworkIdentifier", "y45688jllp.skadnetwork");
var skd_101 = SKAdNetworkDict.AddDict();
skd_101.SetString("SKAdNetworkIdentifier", "hdw39hrw9y.skadnetwork");
var skd_102 = SKAdNetworkDict.AddDict();
skd_102.SetString("SKAdNetworkIdentifier", "4dzt52r2t5.skadnetwork");
var skd_103 = SKAdNetworkDict.AddDict();
skd_103.SetString("SKAdNetworkIdentifier", "s69wq72ugq.skadnetwork");
var skd_104 = SKAdNetworkDict.AddDict();
skd_104.SetString("SKAdNetworkIdentifier", "f73kdq92p3.skadnetwork");
var skd_105 = SKAdNetworkDict.AddDict();
skd_105.SetString("SKAdNetworkIdentifier", "x44k69ngh6.skadnetwork");
var skd_106 = SKAdNetworkDict.AddDict();
skd_106.SetString("SKAdNetworkIdentifier", "mp6xlyr22a.skadnetwork");
var skd_107 = SKAdNetworkDict.AddDict();
skd_107.SetString("SKAdNetworkIdentifier", "6p4ks3rnbw.skadnetwork");
var skd_108 = SKAdNetworkDict.AddDict();
skd_108.SetString("SKAdNetworkIdentifier", "rvh3l7un93.skadnetwork");
var skd_109 = SKAdNetworkDict.AddDict();
skd_109.SetString("SKAdNetworkIdentifier", "24zw6aqk47.skadnetwork");
var skd_110 = SKAdNetworkDict.AddDict();
skd_110.SetString("SKAdNetworkIdentifier", "cs644xg564.skadnetwork");
var skd_111 = SKAdNetworkDict.AddDict();
skd_111.SetString("SKAdNetworkIdentifier", "9vvzujtq5s.skadnetwork");
var skd_112 = SKAdNetworkDict.AddDict();
skd_112.SetString("SKAdNetworkIdentifier", "8r8llnkz5a.skadnetwork");
var skd_113 = SKAdNetworkDict.AddDict();
skd_113.SetString("SKAdNetworkIdentifier", "b9bk5wbcq9.skadnetwork");
var skd_114 = SKAdNetworkDict.AddDict();
skd_114.SetString("SKAdNetworkIdentifier", "bxvub5ada5.skadnetwork");
var skd_115 = SKAdNetworkDict.AddDict();
skd_115.SetString("SKAdNetworkIdentifier", "x8uqf25wch.skadnetwork");
var skd_116 = SKAdNetworkDict.AddDict();
skd_116.SetString("SKAdNetworkIdentifier", "238da6jt44.skadnetwork");
var skd_117 = SKAdNetworkDict.AddDict();
skd_117.SetString("SKAdNetworkIdentifier", "x2jnk7ly8j.skadnetwork");
var skd_118 = SKAdNetworkDict.AddDict();
skd_118.SetString("SKAdNetworkIdentifier", "9g2aggbj52.skadnetwork");
var skd_119 = SKAdNetworkDict.AddDict();
skd_119.SetString("SKAdNetworkIdentifier", "wzmmZ9fp6w.skadnetwork");
var skd_120 = SKAdNetworkDict.AddDict();
skd_120.SetString("SKAdNetworkIdentifier", "nu4557a4je.skadnetwork");
var skd_121 = SKAdNetworkDict.AddDict();
skd_121.SetString("SKAdNetworkIdentifier", "7953jerfzd.skadnetwork");
var skd_122 = SKAdNetworkDict.AddDict();
skd_122.SetString("SKAdNetworkIdentifier", "9yg77x724h.skadnetwork");
var skd_123 = SKAdNetworkDict.AddDict();
skd_123.SetString("SKAdNetworkIdentifier", "n66cz3y3bx.skadnetwork");
var skd_124 = SKAdNetworkDict.AddDict();
skd_124.SetString("SKAdNetworkIdentifier", "v4nxqhlyqp.skadnetwork");
var skd_125 = SKAdNetworkDict.AddDict();
skd_125.SetString("SKAdNetworkIdentifier", "v9wttpbfk9.skadnetwork");
var skd_126 = SKAdNetworkDict.AddDict();
skd_126.SetString("SKAdNetworkIdentifier", "n38lu8286q.skadnetwork");
var skd_127 = SKAdNetworkDict.AddDict();
skd_127.SetString("SKAdNetworkIdentifier", "r26jy69rpl.skadnetwork");
var skd_128 = SKAdNetworkDict.AddDict();
skd_128.SetString("SKAdNetworkIdentifier", "dbu4b84rxf.skadnetwork");
var skd_129 = SKAdNetworkDict.AddDict();
skd_129.SetString("SKAdNetworkIdentifier", "3l6bd9hu43.skadnetwork");
var skd_130 = SKAdNetworkDict.AddDict();
skd_130.SetString("SKAdNetworkIdentifier", "488r3q3dtq.skadnetwork");
var skd_131 = SKAdNetworkDict.AddDict();
skd_131.SetString("SKAdNetworkIdentifier", "52fl2v3hgk.skadnetwork");
var skd_132 = SKAdNetworkDict.AddDict();
skd_132.SetString("SKAdNetworkIdentifier", "6v7lgmsu45.skadnetwork");
var skd_133 = SKAdNetworkDict.AddDict();
skd_133.SetString("SKAdNetworkIdentifier", "89z7zv988g.skadnetwork");
var skd_134 = SKAdNetworkDict.AddDict();
skd_134.SetString("SKAdNetworkIdentifier", "8m87ys6875.skadnetwork");
var skd_135 = SKAdNetworkDict.AddDict();
skd_135.SetString("SKAdNetworkIdentifier", "hb56zgv37p.skadnetwork");
var skd_136 = SKAdNetworkDict.AddDict();
skd_136.SetString("SKAdNetworkIdentifier", "m297p6643m.skadnetwork");
var skd_137 = SKAdNetworkDict.AddDict();
skd_137.SetString("SKAdNetworkIdentifier", "m5mvw97r93.skadnetwork");
var skd_138 = SKAdNetworkDict.AddDict();
skd_138.SetString("SKAdNetworkIdentifier", "vcra2ehyfk.skadnetwork");
var skd_139 = SKAdNetworkDict.AddDict();
skd_139.SetString("SKAdNetworkIdentifier", "v79kvwwj4g.skadnetwork");
var skd_140 = SKAdNetworkDict.AddDict();
skd_140.SetString("SKAdNetworkIdentifier", "a2p9lx4jpn.skadnetwork");


            //write http protoco
            PlistElementDict allowsDict = plist.root.CreateDict("NSAppTransportSecurity");

            allowsDict.SetBoolean("NSAllowsArbitraryLoads", true);
            allowsDict.SetBoolean("NSAllowsArbitraryLoadsInWebContent", true);

            var exceptionsDict = allowsDict.CreateDict("NSExceptionDomains");

            var domainDict = exceptionsDict.CreateDict("ip-api.com");
            var domainDictRocket = exceptionsDict.CreateDict("rocketstudio.com.vn");
            var domainDictHerokuapp = exceptionsDict.CreateDict("space-shooter-server-colyseus.herokuapp.com");
            var domainDictLoveMoney = exceptionsDict.CreateDict("lovemoney.vn");
            var domainDictIP = exceptionsDict.CreateDict("127.0.0.1");
            var domainDictCountryFlag = exceptionsDict.CreateDict("countryflags.io");

            domainDict.SetBoolean("NSExceptionAllowsInsecureHTTPLoads", true);
            domainDict.SetBoolean("NSIncludesSubdomains", true);

            domainDictRocket.SetBoolean("NSExceptionAllowsInsecureHTTPLoads", true);
            domainDictRocket.SetBoolean("NSIncludesSubdomains", true);

            domainDictLoveMoney.SetBoolean("NSExceptionAllowsInsecureHTTPLoads", true);
            domainDictLoveMoney.SetBoolean("NSIncludesSubdomains", true);

            domainDictIP.SetBoolean("NSExceptionAllowsInsecureHTTPLoads", true);
            domainDictIP.SetBoolean("NSIncludesSubdomains", true);

            domainDictCountryFlag.SetBoolean("NSExceptionAllowsInsecureHTTPLoads", true);
            domainDictCountryFlag.SetBoolean("NSIncludesSubdomains", true);

            domainDictHerokuapp.SetBoolean("NSExceptionAllowsInsecureHTTPLoads", true);
            domainDictHerokuapp.SetBoolean("NSIncludesSubdomains", true);

            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }

    [PostProcessBuild]
    public static void OnPostProcessAssociedDomains(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget != BuildTarget.iOS) return;

        var dummy = CreateInstance<CustomPostprocessScript>();
        var file = dummy.m_entitlementsFile;

        DestroyImmediate(dummy);
        if (file == null) return;

        var proj_path = PBXProject.GetPBXProjectPath(buildPath);
        var proj = new PBXProject();
        proj.ReadFromFile(proj_path);

        // target_name = "Unity-iPhone"
        var target_name = proj.GetUnityFrameworkTargetGuid();
        var target_guid = proj.TargetGuidByName(target_name);
        var src = AssetDatabase.GetAssetPath(file);
        var file_name = Path.GetFileName(src);
        var dst = buildPath + "/" + target_name + "/" + file_name;
        FileUtil.CopyFileOrDirectory(src, dst);
        proj.AddFile(target_name + "/" + file_name, file_name);
        proj.AddBuildProperty(target_guid, "CODE_SIGN_ENTITLEMENTS", target_name + "/" + file_name);
        proj.WriteToFile(proj_path);
    }

    [PostProcessBuild(1)]
    public static void OnPostProcessBuildFramework(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            //using (StreamWriter sw = File.AppendText(path + "/Podfile"))
            //{
// E.g. add an app extension
              //  sw.WriteLine("post_install do |installer|\n problematic_targets = ['AppMetricaLibraryAdapter']\n installer.pods_project.targets.each do |target|\n target.build_configurations.each do |config|\n if problematic_targets.include?(target.name)\n config.build_settings['BUILD_LIBRARY_FOR_DISTRIBUTION'] = 'NO'\n end\n end\n end\nend");
           // }
            
            
            // Read.
            var projectPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            var targetName = project.GetUnityFrameworkTargetGuid(); // note, not "project." ...
            var targetGUID = project.TargetGuidByName(targetName);
            project.AddCapability(targetGUID, PBXCapabilityType.InAppPurchase);
            project.AddCapability(targetGUID, PBXCapabilityType.PushNotifications);
            AddFrameworks(project, targetGUID);
            // Write.
            File.WriteAllText(projectPath, project.WriteToString());
            
            
        }
    }

    [PostProcessBuild(999)]
    public static void OnPostProcessBuildDisableBitCode(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            var projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
            var pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);
            var target = pbxProject.GetUnityMainTargetGuid();
            pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            pbxProject.SetBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
            pbxProject.WriteToFile(projectPath);
        }
    }

    private static void AddFrameworks(PBXProject project, string targetGUID)
    {
        project.AddFrameworkToProject(targetGUID, "UserNotificationsUI.framework", true);
        project.AddFrameworkToProject(targetGUID, "UserNotifications.framework", true);
        project.AddFrameworkToProject(targetGUID, "iAd.framework", true);
        project.AddFrameworkToProject(targetGUID, "StoreKit.framework", true);
    }
}
#endif