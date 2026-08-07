using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config
{
    public static string settingProductName = "Ant Out: Pixel Block";

    public const string settingKeyStore = "antout";
    public static string keyaliasPass = "12345678two";
    public static string keystorePass = "12345678two";
    public static string settingAliasName = "antout";

    public const string settingLogo = "GAME_ICON";

    public static int versionCode = 2026080701;//sua
    public static string versionName = "1.0.6";//sua
    public static int settingVersionCode = 2026080701;//sua
    public static string settingVersionName = "1.0.6";//sua

    public static string productNameBuild = "Ant Out: Pixel Block";

    public static int VersionCodeAll
    {
        get
        {
            return versionCode / 100;
        }
    }

    public static int VersionFirstInstall
    {
        get
        {
            int data = PlayerPrefs.GetInt(StringHelper.VERSION_FIRST_INSTALL, 0);
            if (data == 0)
            {
                PlayerPrefs.SetInt(StringHelper.VERSION_FIRST_INSTALL, versionCode);
                data = versionCode;
            }

            return data;
        }
    }

    public static string inappAndroidKeyHash
        = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApwxZOgYqdcadqBJEbNm6muPQqqzcYk5L/GtdCijQ2zG3jitW2yyvaDC/DrRUD4Pykv7KLNHWKjww8flY957nXVRLTtKAk6d+qj2e6DvW1nKo3Yb1IX44DpBVono5RAaJ/m8Phkgtk2ibr+yybmBw3a6ayxJ6XOvTmGshP/3K4DkjEHar5BP95erdjnVFfqPrl9QlFdJJaUN5tvldpl1SX3u0mMDbmo1E8YOb1V8e4aH2+Jf89y+HZJni7vRI1Mspf2nNI4pvF5CZXwDJP+ZkOrbYYV52lhBDeq2BYwfOojLSjeSOpP3GCcfhqkqHzobgM61m2edOa0mTPIW6HsZHAQIDAQAB";
#if UNITY_ANDROID
    public const string package_name = "com.fc.p.gpl.ant.out.pixel.color";
#else
    public const string package_name = "com.fc.p.gpl.ant.out.pixel.color";
#endif



#if UNITY_ANDROID
    public static string OPEN_LINK_RATE = "market://details?id=" + package_name;
#else
    public static string OPEN_LINK_RATE = "itms-apps://itunes.apple.com/app/id6689494714";
#endif

    public static string FanpageLinkWeb = "https://www.facebook.com/groups/402513540729752/";
    public static string FanpageLinkApp = "https://www.facebook.com/groups/402513540729752/";

    public static string LinkFeedback = "https://funchip.net/";
    public static string LinkPolicy = "https://funchip.net/privacy-policy.html";
    public static string IdFanPage = "100086685782219";
    public static string IdGroup = "1525203137994927";
    public static string LinkTermOfSerice = "https://sites.google.com/view/global-play-terms-of-use";
    public static string LinkMoreGame = "https://gplayjsc.com/";
    public static string LinkTerm = "https://sites.google.com/view/mini-game-puzzle-fun-policy/";

#if UNITY_ANDROID
    public const string IRONSOURCE_DEV_KEY = "1b0dc277d";
#else
 public const string IRONSOURCE_DEV_KEY = "1b2a3e8dd";
#endif


#if UNITY_ANDROID
    public const string ADJUST_APP_TOKEN = "dkyd08wh4000";
#else
    public const string ADJUST_APP_TOKEN = "dkyd08wh4000";
#endif
    public const string Link_Avatar_Save = "BirdSort/avatar";
    public const string FLAG_API = "https://raw.githubusercontent.com/caothecuong91/countries_flag/master/png100px/{0}.png";

    public const string StoreIDPlayFab = "Config";

#if UNITY_ANDROID
    public const string PLAYFAB_TITLE_ID = "71D80";
#else
 public const string PLAYFAB_TITLE_ID = "71D80";
#endif
#if UNITY_ANDROID
    public const string AD_PLATFORM = "ironSource";
#else
    public const string AD_PLATFORM = "ironSource";
#endif
}
