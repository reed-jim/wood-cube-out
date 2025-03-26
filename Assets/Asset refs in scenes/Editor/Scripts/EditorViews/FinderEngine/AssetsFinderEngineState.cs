#if UNITY_EDITOR

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.Checkers;
using SearchEngine.EditorViews.Data;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.FinderEngine
{
    public class AssetsFinderEngineState : IFinderEngineStateActive
    {
        public event Action Started;
        
        public Dictionary<string, List<SceneGOData>> FoundAssets { get; private set; }// assets scenes GOs
        public List<string> AllScenes { get; private set; }
        private AssetProcessor checker;
        private Dictionary<string, List<GameObject>> sceneObjsMatch = new Dictionary<string, List<GameObject>>();// assets GOs
        
        public AssetsFinderEngineState()
        {
            this.CheckSerializeFields();

            AllScenes = new List<string>();
            FoundAssets = new Dictionary<string, List<SceneGOData>>();
        }
         
        public void StartExecuting(AssetProcessor checker, IEnumerable<string> assets)
        {
            if (checker == null)
                return;
            
            this.checker = checker;
            FoundAssets.Clear();
            AllScenes.Clear();
            sceneObjsMatch.Clear();
            foreach (var v in assets)
            {
                sceneObjsMatch.Add(v, new List<GameObject>());
                FoundAssets.Add(v, new List<SceneGOData>());
            }

            EventSender.SendEvent(Started);
        }

        public void ExecuteCurrGO(GameObject go)
        {
            foreach (var v in checker.CheckGO(go))
            {
                foreach (var asset in v)
                {
                    sceneObjsMatch[asset].Add(go);                       
                }
            }
        } 

        public void OnSceneComplete(string currScene)
        {
            CollectSceneData(currScene);
            foreach (var v in sceneObjsMatch)
                v.Value.Clear();
            AllScenes.Add(currScene);
        }
        
        public void StopExecuting()
        {
            checker = null;
        }

        private void CollectSceneData(string currScene)
        {
            foreach (var v in sceneObjsMatch)
            {
                if (v.Value.Any())
                {
                     DataGOPath[] sceneDataGoPath =
                    (from o in v.Value
                     select new DataGOPath(o.name, HelperGeneral.GetTransformIndexPaths(o.transform))).ToArray();

                    FoundAssets[v.Key].Add(new SceneGOData(currScene, sceneDataGoPath));
                }
            }
        }
    }
}

#endif
