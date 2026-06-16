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
    public class MissingRefsFinderState : IFinderEngineStateActive
    {
        public event Action Started;
        public List<string> AllScenes { get; private set; }
        private IMissingRefsChecker checker = new MissingRefsChecker();
        private bool checkGameObjects;
        private bool checkComponents;
        private bool checkFieldsAndProps;
        
        protected List<GameObject> sceneObjsMatchMissingGameObjects = new List<GameObject>();
        private List<SceneGOData> missingGOData;
        
        protected List<GameObject> sceneObjsMatchMissingComps = new List<GameObject>();
        private List<SceneGOData> missingCompsData;
        
        protected Dictionary<string, List<GameObject>> sceneObjsMatchMissingInspectorProps = new Dictionary<string, List<GameObject>>();
        public Dictionary<string, List<SceneGOData>> FoundMissingInspectorProps { get; private set; }

        public MissingRefsFinderState(List<SceneGOData> missingGOData, List<SceneGOData> missingCompsData)
        {
            this.missingGOData = missingGOData;
            this.missingCompsData = missingCompsData;
            this.CheckSerializeFields();
            FoundMissingInspectorProps = new Dictionary<string, List<SceneGOData>>();
            AllScenes = new List<string>();
        }
        
        public void StartExecuting (bool checkGameObjects, bool checkComponents, bool checkFieldsAndProps)
        {
            if (!(checkGameObjects || checkComponents || checkFieldsAndProps))
                return;

            this.checkGameObjects = checkGameObjects;
            this.checkComponents = checkComponents;
            this.checkFieldsAndProps = checkFieldsAndProps;

            AllScenes.Clear();

            missingCompsData.Clear();
            sceneObjsMatchMissingComps.Clear();

            missingGOData.Clear();
            sceneObjsMatchMissingGameObjects.Clear();

            FoundMissingInspectorProps.Clear();
            sceneObjsMatchMissingInspectorProps.Clear();
            EventSender.SendEvent(Started);
        }

        public void ExecuteCurrGO(GameObject go)
        {
            if (checkGameObjects && checker.CheckGO(go))
                sceneObjsMatchMissingGameObjects.Add(go);

            var comps = go.GetComponents<Component>();

            if(checkComponents && comps.Any(c => checker.CheckComponent(c)))
                sceneObjsMatchMissingComps.Add(go);

            if (checkFieldsAndProps)
            {
                foreach (var propType in checker.CheckFieldsAndProps(comps))
                {
                    var correctPropType = 
                        propType[0] != '$' ? 
                        propType :
                        propType.Substring(1, propType.Length - 1);

                    if (sceneObjsMatchMissingInspectorProps.ContainsKey(correctPropType))
                    {
                        sceneObjsMatchMissingInspectorProps[correctPropType].Add(go);
                    }
                    else
                    {
                        sceneObjsMatchMissingInspectorProps[correctPropType] = new List<GameObject> {go};
                    }
                }                
            }
        }

        public void OnSceneComplete(string currScene)
        {
            CollectSceneData(currScene);
            sceneObjsMatchMissingComps.Clear();
            sceneObjsMatchMissingGameObjects.Clear();
            sceneObjsMatchMissingInspectorProps.Clear();
            AllScenes.Add(currScene);
        }

        public void StopExecuting()
        {
            
        }

        protected void CollectSceneData(string currScene)
        {
            if (sceneObjsMatchMissingComps.Any())
            {
                DataGOPath[] sceneDataGoPath =
                (from o in sceneObjsMatchMissingComps
                 select new DataGOPath(o.name, HelperGeneral.GetTransformIndexPaths(o.transform))).ToArray();
                missingCompsData.Add(new SceneGOData(currScene, sceneDataGoPath));                 
            }
            
            if (sceneObjsMatchMissingGameObjects.Any())
            {
                DataGOPath[] sceneDataGoPath =
                (from o in sceneObjsMatchMissingGameObjects
                 select new DataGOPath(o.name, HelperGeneral.GetTransformIndexPaths(o.transform))).ToArray();
                missingGOData.Add(new SceneGOData(currScene, sceneDataGoPath));                 
            }

            if (sceneObjsMatchMissingInspectorProps.Any())
            {
                foreach (var propType in sceneObjsMatchMissingInspectorProps.Keys)
                {
                    if (!sceneObjsMatchMissingInspectorProps[propType].Any())
                        continue;
                    
                    DataGOPath[] sceneDataGoPath =
                    (from o in sceneObjsMatchMissingInspectorProps[propType]
                     select new DataGOPath(o.name, HelperGeneral.GetTransformIndexPaths(o.transform))).ToArray();
                    
                    if(!FoundMissingInspectorProps.ContainsKey(propType))
                        FoundMissingInspectorProps[propType] = new List<SceneGOData>();
                    var sgd = new SceneGOData(currScene, sceneDataGoPath);
                    FoundMissingInspectorProps[propType].Add(sgd);
                }
            }
        }

        public void SetMissingRefsChecker(IMissingRefsChecker mrChecker)
        {
            if (mrChecker != null)
                checker = mrChecker;
        }


    }
}

#endif