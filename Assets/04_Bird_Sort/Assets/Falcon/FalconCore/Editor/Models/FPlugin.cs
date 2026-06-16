using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Falcon.FalconCore.Editor.Payloads;
using Falcon.FalconCore.Editor.Repositories;
using Falcon.FalconCore.Editor.Services;
using Falcon.FalconCore.Editor.Utils;
using Falcon.FalconCore.Scripts.Logs;
using Falcon.FalconCore.Scripts.Utils;
using Falcon.FalconCore.Scripts.Utils.FActions.Variances.Starts;
using UnityEditor;
using UnityEngine;

namespace Falcon.FalconCore.Editor.Models
{
    public class FPlugin
    {
        private const string UnityPackageExtension = ".unitypackage";

        public FPlugin(BitBucObj obj)
        {
            var tokens = obj.Path.Split('/');
            PluginName = tokens[tokens.Length - 1];
            string[] tokens1 = PluginName.Split(' ');
            PluginShortName = tokens1[tokens1.Length - 1];
            if (PluginShortName.EndsWith("Android") || PluginShortName.EndsWith("IOS"))
            {
                PluginShortName = PluginShortName.Replace("Android", "");
                PluginShortName = PluginShortName.Replace("IOS", "");
            }

            BitBucObj remoteConfigLink = null;
            var pluginVersions = new HashSet<BitBucObj>();

            foreach (var value in BitBucCall.OfUrl(obj.Links.Self.HRef))
            {
                var href = value.Links.Self.HRef;
                if (href != null && href.EndsWith("config.txt"))
                    remoteConfigLink = value;
                else if (href != null && !href.EndsWith(".meta")) pluginVersions.Add(value);
            }

            RemoteConfig = JsonUtil.FromJson<FPluginMeta>(new HttpRequest
            {
                RequestType = HttpMethod.Get,
                URL = remoteConfigLink.Links.Self.HRef
            }.InvokeAndGet());
            UpdateRemoteConfigFromBigBucObjs(pluginVersions);

            UpdateInstalledConfig();
        }

        private void UpdateRemoteConfigFromBigBucObjs(HashSet<BitBucObj> bitBucObjs)
        {
            foreach (var url in bitBucObjs)
            {
                if (url.Path.EndsWith(PluginShortName + "-" + RemoteConfig.version +
                                      UnityPackageExtension)) PluginUrl = url.Links.Self.HRef;
            }

            if (PluginUrl == null) PluginUrl = bitBucObjs.First().Links.Self.HRef;
        }

        private void UpdateInstalledConfig()
        {
            var directory =
                Directory.GetDirectories(FalconCoreFileUtils.ApplicationDataPath, PluginShortName,
                    SearchOption.AllDirectories);

            if (directory.Length == 0) Installed = false;
            else
                try
                {
                    Installed = true;
                    InstalledDirectory = directory[0].Contains("Release") ? directory[1] : directory[0];
                    InstalledConfig = JsonUtil.FromJson<FPluginMeta>(File.ReadAllText(
                        InstalledDirectory + Path.DirectorySeparatorChar + "config.txt"));
                }
                catch (Exception)
                {
                    Installed = false;
                }
        }

        string urlAppsFlyer =
            "https://bitbucket.org/falcongame/falcon-unity-sdk/raw/a2812cdd3bad12e0e9a5ca08dd2f53ad4500032f/Assets/Falcon/Release/FalconAnalytics/Appsflyer-16-12-2024.unitypackage";

        string urlAdjust =
            "https://bitbucket.org/falcongame/falcon-unity-sdk/raw/b1bb75bf26521f70c38a115fbb32f499affb7267/Assets/Falcon/Release/FalconAnalytics/Adjust-04-05-2024.unitypackage";

        public FPluginMeta InstalledConfig { get; private set; }

        public string PluginName { get; }
        public string PluginShortName { get; private set; }
        public FPluginMeta RemoteConfig { get; private set; }

        public string PluginUrl { get; private set; }
        public bool Installed { get; private set; }
        public bool IsDownloading { get; private set; }
        public bool IsDownloadingExternal { get; private set; }
        public string InstalledDirectory { get; private set; }
        public int progress;

        public IEnumerator Install()
        {
            var requireJson = RemoteConfig.Require;
            var requirePlugins = new List<FPlugin>();
            if (requireJson != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in requireJson)
                {
                    FPlugin plugin;
                    while (!FPluginRepo.TryGet(keyValuePair.Key, out plugin))
                    {
                        yield return null;
                    }

                    if (plugin.InstalledConfig == null ||
                        string.CompareOrdinal(plugin.InstalledConfig.version, keyValuePair.Value) < 0)
                    {
                        requirePlugins.Add(plugin);
                    }
                }
            }

            if (requirePlugins.Count == 0)
            {
                yield return UnsafeInstall();
                yield break;
            }

            var requirePluginString = new StringBuilder();
            requirePluginString
                .Append("The following plugins are required for" + PluginShortName + ":").AppendLine();
            foreach (var plugin in requirePlugins)
                requirePluginString.Append("  - ").Append(plugin.PluginShortName).AppendLine();
            requirePluginString.Append("Please install/update them first!");
            new EditorMainThreadAction(() =>
                {
                    EditorUtility.DisplayDialog("Additional plugin require!!!", requirePluginString.ToString(), "Ok");
                }
            ).Schedule();
        }

        void RemoveAppsFlyer()
        {
            DeleteFileOrFolder(Path.Combine(Application.dataPath, "AppsFlyer"));
            DeleteFileOrFolder(Path.Combine(Application.dataPath, "PlayServicesResolver"));
            DeleteFileOrFolder(Path.Combine(Application.dataPath, "Falcon/FalconAnalytics/Editor/AppsFlyer"));
            DeleteFileOrFolder(Path.Combine(Application.dataPath, "Falcon/FalconAnalytics/Scripts/AppsFlyer"));
        }

        void DeleteFileOrFolder(string path)
        {
            FileUtil.DeleteFileOrDirectory(path);
            FileUtil.DeleteFileOrDirectory(path + ".meta");
        }

        public IEnumerator InstallAppsFlyer()
        {
            RemoveAdjust();
            var tempFolder = Application.dataPath + "/../Temp/AppsFlyer" + UnityPackageExtension;
            var fileGetRequest = new FileGetRequest(urlAppsFlyer, tempFolder);
            new Thread(fileGetRequest.Invoke).Start();
            CoreLogger.Instance.Info("Downloading AppsFlyer");
            IsDownloadingExternal = true;
            while (!fileGetRequest.Done)
            {
                progress = fileGetRequest.progress;
                yield return null;
            }

            progress = 100;
            yield return new WaitForSecondsRealtime(0.3f);
            IsDownloadingExternal = false;
            CoreLogger.Instance.Info("Downloading completed, preparing to import");
            AssetDatabase.ImportPackage(tempFolder, true);
        }

        void RemoveAdjust()
        {
            DeleteFileOrFolder(Path.Combine(Application.dataPath, "Adjust"));
            DeleteFileOrFolder(Path.Combine(Application.dataPath, "Falcon/FalconAnalytics/Editor/Adjust"));
            DeleteFileOrFolder(Path.Combine(Application.dataPath, "Falcon/FalconAnalytics/Scripts/Adjust"));
        }

        public IEnumerator InstallAdjust()
        {
            RemoveAppsFlyer();
            var tempFolder = Application.dataPath + "/../Temp/Adjust" + UnityPackageExtension;
            var fileGetRequest = new FileGetRequest(urlAdjust, tempFolder);
            new Thread(fileGetRequest.Invoke).Start();
            CoreLogger.Instance.Info("Downloading Adjust");
            IsDownloadingExternal = true;
            while (!fileGetRequest.Done)
            {
                progress = fileGetRequest.progress;
                yield return null;
            }

            progress = 100;
            yield return new WaitForSecondsRealtime(0.3f);
            IsDownloadingExternal = false;
            CoreLogger.Instance.Info("Downloading completed, preparing to import");
            AssetDatabase.ImportPackage(tempFolder, true);
        }

        private IEnumerator UnsafeInstall()
        {
            var tempFolder = FalconCoreFileUtils.ApplicationDataPath + "/../Temp/" + PluginShortName +
                             UnityPackageExtension;

            var fileGetRequest = new FileGetRequest(PluginUrl, tempFolder);
            new Thread(fileGetRequest.Invoke).Start();
            CoreLogger.Instance.Info("Downloading " + PluginShortName);
            IsDownloading = true;
            while (!fileGetRequest.Done)
            {
                progress = fileGetRequest.progress;
                yield return null;
            }

            progress = 100;
            yield return new WaitForSecondsRealtime(0.3f);
            InstalledConfig = RemoteConfig;
            Installed = true;
            InstalledDirectory = Path.Combine(FalconCoreFileUtils.GetFalconPluginFolder(), PluginName);
            IsDownloading = false;
            CoreLogger.Instance.Info("Downloading complete, preparing to import");
            new EditorMainThreadAction(() => AssetDatabase.ImportPackage(tempFolder, true)).Schedule();
        }

        public void UnInstall()
        {
            CoreLogger.Instance.Info("Start uninstallation");
            if (IsFalconAnalytics())
            {
                RemoveAppsFlyer();
                RemoveAdjust();
            }

            DeleteFileOrFolder(InstalledDirectory);

            Installed = false;
            InstalledConfig = null;
            InstalledDirectory = null;
            AssetDatabase.Refresh();
        }

        public bool IsFalconCore()
        {
            return string.CompareOrdinal(PluginShortName, "FalconCore") == 0;
        }

        public bool IsFalconAnalytics()
        {
            return string.CompareOrdinal(PluginShortName, "FalconAnalytics") == 0;
        }


        public bool IsUseAppsFlyer()
        {
            if (!Directory.Exists(Application.dataPath + "/AppsFlyer")) return false;
            return true;
        }

        public bool IsUseAdjust()
        {
            if (!Directory.Exists(Application.dataPath + "/Adjust")) return false;
            return true;
        }


        public bool InstalledNewest()
        {
            if (string.CompareOrdinal(InstalledConfig.version, RemoteConfig.version) < 0) return false;
            return true;
        }
    }
}