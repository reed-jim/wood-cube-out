using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace LA.PostBuild
{
    public static class PostBuild
    {
        [PostProcessBuild(1000)]
        public static void OnBuildFinish(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.Android)
            {
                return;
            }
            Debug.Log(pathToBuiltProject);
            if (!File.Exists(PostBuildEditor.PathToConfig))
                return;
            string jsonText = File.ReadAllText(PostBuildEditor.PathToConfig);
            var data = JsonUtility.FromJson<PostBuildData>(jsonText);
            if(!data.IsOverrideName)
                return;
            var time = DateTime.Now;
            string afterName = data.ProjectName + "_" + data.FeatureName + "_" + time.ToString("HH") + "h" + time.ToString("mm") + "_" +
                               time.ToString("ddMMyyyy")+".apk";
            string afterPath = Directory.GetParent(pathToBuiltProject)!.ToString();
            afterPath = Path.Combine(afterPath, afterName);
            Debug.Log(afterPath);
            DelayAction(pathToBuiltProject,afterPath);
        }
        private static async void DelayAction(string oldPath,string newPath)
        {
            await WaitUntil((() => File.Exists(oldPath)));
            File.Move(oldPath, newPath);
        }
        private static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (!condition()) await Task.Delay(frequency);
            });

            if (waitTask != await Task.WhenAny(waitTask, 
                    Task.Delay(timeout))) 
                throw new TimeoutException();
        }
    }
}

