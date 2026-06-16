using UnityEngine;

public class GUtils : MonoBehaviour
{
    public static string GetDeviceId()
    {

        if(UseProfile.LinkedId != "")
        {
            return UseProfile.LinkedId;
        }

#if UNITY_EDITOR
        return SystemInfo.deviceUniqueIdentifier + 0935141004;
       //return "969921e6812715bf";
#elif UNITY_ANDROID
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
		AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
		string android_id = secure.CallStatic<string>("getString", contentResolver, "android_id");
		return android_id + 1 ;
#elif UNITY_IOS
return SystemInfo.deviceUniqueIdentifier;

#endif
    }
}