#if UNITY_EDITOR 

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.Data;
using SearchEngine.EditorWindows;
using UnityEditor;

namespace SearchEngine.EditorViews
{
    public class ScenesRunner
    {
        public event Action Started;
        public event Action Complete;
        public event Action<string> SceneComplete;


        [SerializeField] private IList<AssetInfoData> scenes;
        [SerializeField] private FinderEngine.FinderEngine finderEngine;

        public bool InProgress { get; set; }

        const float minTimePerExecute = 0.1f;
        private const int goPerExecuteDefault = 10;
        private int goPerExecute = goPerExecuteDefault;

        public int CurrScene { get; protected set; }
        public string[] Scenes { get; protected set; }
        public int CurrSceneObj { get; protected set; }
        public GameObject[] SceneObjs { get; protected set; }

        public ScenesRunner(IList<AssetInfoData> scenes, FinderEngine.FinderEngine finderEngine)
        {
            this.scenes = scenes;
            this.finderEngine = finderEngine;
            this.CheckSerializeFields();
        }

        public void Start()
        {
            if (InProgress)
                return;

            Scenes = AssetInfoData.GetActiveAssetPaths(scenes.Where(v=>v.Validate())) ?? new string[0];
            SceneObjs = new GameObject[0];
            CurrScene = -1;
            CurrSceneObj = 0;

            goPerExecute = goPerExecuteDefault;
            InProgress = true;
            CloseResultsEW();
            LoadGOFromNextScene();
            EventSender.SendEvent(Started);
        }

        public void Stop()
        {
            if (!InProgress)
                return;
            SceneObjs = new GameObject[0];
            Scenes = new string[0];
            InProgress = false;
            EventSender.SendEvent(Complete);
        }

        public void Break()
        {
            if (!InProgress)
                return;
            EventSender.SendEvent(SceneComplete, Scenes[CurrScene]);
            Stop();
        }

        public void ContinueExecuting()
        {
            double endTime = EditorApplication.timeSinceStartup;

            if (CurrScene < Scenes.Count())
            {
                if (CurrSceneObj >= SceneObjs.Count())
                {
                    EventSender.SendEvent(SceneComplete, Scenes[CurrScene]);
                    LoadGOFromNextScene();
                    return;
                }

                ExecuteCurrentScene();
            }
            else
            {
                Stop();
                return;
            }

            CorrectGOPerExecute(EditorApplication.timeSinceStartup - endTime);
        }
        private void ExecuteCurrentScene()
        {
            int startPos = CurrSceneObj;
            CurrSceneObj = Mathf.Min(SceneObjs.Length, startPos + goPerExecute);

            for (int i = startPos; i < CurrSceneObj; i++)
            {
                if (SceneObjs[i] != null)
                    finderEngine.ExecuteCurrGO(SceneObjs[i]);
            }
        }
        private void LoadGOFromNextScene()
        {
            while (++CurrScene < Scenes.Count())
            {
                if (HelperGeneral.OpenScene(Scenes[CurrScene], true))
                {
                    SceneObjs = new GameObjectsCollector().FindAllObjectsOnScene();
                    if (!SceneObjs.Any())
                        continue;
                    CurrSceneObj = 0;
                    return;
                }
            }
        }

        private void CloseResultsEW()
        {
            ResultsEW.CloseCurrWindow();
        }

        private void CorrectGOPerExecute(double allTime)
        {
            if (
                (allTime > minTimePerExecute && goPerExecute > 1)
                || allTime < minTimePerExecute * 0.95f
            )
            {
                goPerExecute = (int)(goPerExecute * minTimePerExecute * 0.975f / allTime);
                goPerExecute = Mathf.Min(10000, Mathf.Max(1, goPerExecute));
            }
        }

    }
}

#endif
