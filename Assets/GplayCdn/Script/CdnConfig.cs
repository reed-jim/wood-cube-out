using UnityEngine;
using System.IO;

namespace GplayCdn
{
    /// <summary>
    /// CDN configuration - server URLs and API key management.
    /// </summary>
    public static class CdnConfig
    {
        /// <summary>
        /// CDN download URL format. Use string.Format with fileId.
        /// </summary>
        public const string CdnServerUrl = "https://cdn.gplaysdk.com/{0}";

        /// <summary>
        /// CDN upload API endpoint.
        /// </summary>
        public const string CdnUploadEndpoint = "https://api.gplaysdk.com/cdn/file";

        /// <summary>
        /// Path to API key file on local machine (editor only).
        /// </summary>
        public const string ApiKeyPath = "C:/GPlaySDK/ApiKey.txt";

        /// <summary>
        /// Local storage path for downloaded CDN resources.
        /// </summary>
        public static string LocalStoragePath
        {
            get
            {
                string path = Path.Combine(Application.persistentDataPath, "CdnCache");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        /// <summary>
        /// Get the local file path for a given resource id.
        /// </summary>
        public static string GetLocalPath(string resourceId)
        {
            return Path.Combine(LocalStoragePath, resourceId);
        }

        /// <summary>
        /// Get the CDN download URL for a given resource id.
        /// </summary>
        public static string GetDownloadUrl(string resourceId)
        {
            return string.Format(CdnServerUrl, resourceId);
        }

#if UNITY_EDITOR
        private static string _cachedApiKey;

        /// <summary>
        /// Read API key from local file (editor only).
        /// </summary>
        public static string GetApiKey()
        {
            if (!string.IsNullOrEmpty(_cachedApiKey))
                return _cachedApiKey;

            if (File.Exists(ApiKeyPath))
            {
                _cachedApiKey = File.ReadAllText(ApiKeyPath).Trim();
            }
            else
            {
                Debug.LogError($"[CdnConfig] API key file not found: {ApiKeyPath}");
                _cachedApiKey = string.Empty;
            }

            return _cachedApiKey;
        }
#endif
    }
}
