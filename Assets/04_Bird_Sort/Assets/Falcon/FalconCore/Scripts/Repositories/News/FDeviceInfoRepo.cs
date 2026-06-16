#if ! UNITY_EDITOR
using System;
using System.Security.Cryptography;
using System.Text;
#endif
using Falcon.FalconCore.Scripts.Logs;
using UnityEngine;
#if UNITY_IOS
using System;
using System.Globalization;
using Falcon;
using Newtonsoft.Json;
using UnityEngine.iOS;
#endif

namespace Falcon.FalconCore.Scripts.Repositories.News
{
    public static class FDeviceInfoRepo
    {
        static FDeviceInfoRepo()
        {
            Init();
        }

        public static string PackageName { get; private set; }
        public static string GameName { get; private set; }
        public static string Platform { get; private set; }
        public static string AppVersion { get; private set; }
        public static string SdkCoreVersion { get; private set; }
        public static string DeviceName { get; private set; }
        public static string DeviceId { get; private set; }
        public static string DeviceOs { get; private set; }
        public static string DeviceModel { get; private set; }
        public static int ScreenWidth { get; private set; }
        public static int ScreenHeight { get; private set; }
        public static float ScreenDpi { get; private set; }
        public static string DeviceGpu { get; private set; }
        public static string DeviceCpu { get; private set; }
        public static string Language { get; private set; }
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        public static string IDFV { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Init()
        {
            PackageName = Application.identifier.ToLower();
            GameName = Application.productName.ToLower();
#if UNITY_IOS
            Platform = "ios";
            IDFV = Device.vendorIdentifier;
#elif UNITY_ANDROID
            Platform = "android";
            IDFV = SystemInfo.deviceUniqueIdentifier;
#else
            Platform = Application.platform.ToString().ToLower();
#endif
            AppVersion = Application.version.ToLower();
            SdkCoreVersion = "2.2.4";
            DeviceName = SystemInfo.deviceName.ToLower();
            DeviceId = FDataPool.Instance.ComputeIfAbsent("FDeviceInfoRepo_Device_Id", GetDeviceId).Value;
            DeviceOs = SystemInfo.operatingSystem.ToLower();
            DeviceModel = SystemInfo.deviceModel.ToLower();
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;
            ScreenDpi = Screen.dpi;
            DeviceGpu = SystemInfo.graphicsDeviceName.ToLower();
            DeviceCpu = SystemInfo.processorType.ToLower();
            Language = Application.systemLanguage.ToString();
            CoreLogger.Instance.Info("FDeviceInfoRepo init complete");
        }

        private static string GetDeviceId()
        {
            // ReSharper disable once RedundantAssignment
            string deviceId = "";
#if UNITY_EDITOR
            deviceId = SystemInfo.deviceUniqueIdentifier + "-editor";
#elif UNITY_ANDROID
            try
            {
                var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
                var contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
                var secure = new AndroidJavaClass("android.provider.Settings$Secure");
                deviceId = secure.CallStatic<string>("getString", contentResolver, "android_id");


                if (string.IsNullOrEmpty(deviceId) || deviceId.ToLower().Equals("unknown") || deviceId.Length < 4)
                {
                    var client =
                        new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
                    var adInfo =
                        client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);
                    deviceId = adInfo.Call<string>("getId");
                }

                if (string.IsNullOrEmpty(deviceId) || deviceId.ToLower().Equals("unknown") || deviceId.Length < 4)
                    deviceId = CreateUniqueString();

                if (string.IsNullOrEmpty(deviceId) || deviceId.ToLower().Equals("unknown") || deviceId.Length < 4)
                    deviceId = SystemInfo.deviceUniqueIdentifier;
            }
            catch (Exception e)
            {
                if (string.IsNullOrEmpty(deviceId) || deviceId.ToLower().Equals("unknown") || deviceId.Length < 4)
                    deviceId = CreateUniqueString();

                if (string.IsNullOrEmpty(deviceId) || deviceId.ToLower().Equals("unknown") || deviceId.Length < 4)
                    deviceId = SystemInfo.deviceUniqueIdentifier;
            }

#elif UNITY_WEBGL
            deviceId = FDataPool.Instance.GetOrSet("UniqueIdentifier", Guid.NewGuid().ToString());
#elif UNITY_IOS
            var key = "falcon_" + Application.identifier + "_uuid";
            deviceId = UUIDiOS.GetKeyChainValue(key);

            if (string.IsNullOrEmpty(deviceId) || deviceId.ToLower().Equals("unknown") || deviceId.Length < 4)
            {
                deviceId = Guid.NewGuid().ToString();
                UUIDiOS.SaveKeyChainValue(key, deviceId);
            }

            if (string.IsNullOrEmpty(deviceId) || deviceId.ToLower().Equals("unknown") || deviceId.Length < 4)
            {
                deviceId = Device.vendorIdentifier;
            }

#else
            deviceId = SystemInfo.deviceUniqueIdentifier;
#endif

            return deviceId;
        }

#if !UNITY_EDITOR && UNITY_ANDROID
        private static string CreateUniqueString()
        {
            // Get device Info to create an String
            var deviceInfo = SystemInfo.deviceModel + SystemInfo.deviceType + SystemInfo.graphicsDeviceName +
                             SystemInfo.graphicsDeviceType;

            // encode that string so create an unique String
            using (var sha1 = SHA1.Create())
            {
                var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(deviceInfo));
                var stringBuilder = new StringBuilder();
                foreach (var b in hashBytes) stringBuilder.Append(b.ToString("x2"));
                return stringBuilder.ToString();
            }
        }
#endif
    }
}