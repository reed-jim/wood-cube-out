using System.Collections.Generic;
using System.Threading;
using Falcon.FalconCore.Editor.Models;
using Falcon.FalconCore.Editor.Services;
using Falcon.FalconCore.Scripts.Utils.Entities;

namespace Falcon.FalconCore.Editor.Repositories
{
    public static class FPluginRepo
    {
        #region Static

        private static ExecState PullState { get; set; } = ExecState.NotStarted;

        public static int RemotePluginCount { get; private set; }

        private static readonly Dictionary<string, FPlugin> NameToPlugins = new Dictionary<string, FPlugin>();

        public static bool TryGet(string pluginName, out FPlugin plugin)
        {
            if (ExecStates.CanStart(PullState)) new Thread(Init).Start();

            return NameToPlugins.TryGetValue(pluginName, out plugin);
        }

        public static bool TryGetAll(out ICollection<FPlugin> plugins)
        {
            if (ExecStates.CanStart(PullState)) new Thread(Init).Start();

            if (PullState == ExecState.Succeed)
            {
                plugins = NameToPlugins.Values;
                return true;
            }

            plugins = null;
            return false;
        }
        
        public static FPlugin Get(string pluginName)
        {
            FPlugin result;
            while (!TryGet(pluginName, out result))
            {
                Thread.Yield();
            }

            return result;
        }

        public static ICollection<FPlugin> GetAll()
        {
            ICollection<FPlugin> result;
            while (!TryGetAll(out result))
            {
                Thread.Yield();
            }

            return result;
        }

        #endregion

        #region NetPull

        private const string SdkUrl =
            "https://api.bitbucket.org/2.0/repositories/falcongame/falcon-unity-sdk/src/master/Assets/Falcon/Release/";

        public static void Init()
        {
            if (!ExecStates.CanStart(PullState)) return;
            PullState = ExecState.Processing;

            RemotePluginCount = 0;

            foreach (var value in BitBucCall.OfUrl(SdkUrl))
                if (value.Path != null && !value.Path.EndsWith(".meta"))
                {
                    RemotePluginCount++;
                    var plugin = new FPlugin(value);
                    NameToPlugins[plugin.PluginShortName] = plugin;
                }

            PullState = ExecState.Succeed;
        }

        public static void ClearCache()
        {
            PullState = ExecState.NotStarted;

            RemotePluginCount = 0;

            NameToPlugins.Clear();
        }

        #endregion
    }
}