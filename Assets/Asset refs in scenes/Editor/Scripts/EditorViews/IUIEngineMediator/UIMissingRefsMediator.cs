#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using SearchEngine.Additions;
using SearchEngine.EditorViews.Data;
using SearchEngine.EditorViews.FinderEngine;
using SearchEngine.EditorViews.Results;

namespace SearchEngine.EditorViews.GCModeFacade
{
    public class UIMissingRefsMediator : IUIEngineMediator
    {
        [SerializeField] private MissingRefTypesEV assets;
        [SerializeField] private ResultsInspPropsMRFEV resultsInspPropsMRF;
        [SerializeField] private MissingRefsFinderState engine;

        public UIMissingRefsMediator(MissingRefTypesEV assets, ResultsInspPropsMRFEV resultsInspPropsMRF, MissingRefsFinderState engine)
        {
            this.assets = assets;
            this.resultsInspPropsMRF = resultsInspPropsMRF;
            this.engine = engine;
            this.CheckSerializeFields();
        }
        
        public void ExecuteStart()
        {
            if (!(assets.Components || assets.InspectorProps || assets.GameObjects))
            {
                Debug.LogWarning("No one asset to work with!");
                return;
            }

            resultsInspPropsMRF.UpdateData(new Dictionary<string, List<SceneGOData>>(), new List<string>());
            engine.StartExecuting(assets.GameObjects, assets.Components, assets.InspectorProps);
        }

        public void OnExecuteStop()
        {
            resultsInspPropsMRF.UpdateData(engine.FoundMissingInspectorProps, engine.AllScenes);
        }
    }
}

#endif