#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SearchEngine.Additions;
using SearchEngine.Checkers;
using SearchEngine.EditorViews.Data;
using SearchEngine.EditorViews.FinderEngine;
using SearchEngine.EditorViews.Results;

namespace SearchEngine.EditorViews.GCModeFacade
{
    public class UIAssetsFinderMediator : IUIEngineMediator
    {
        [SerializeField] private IList<AssetInfoData> assetsData;
        [SerializeField] private ResultsAssetsFinderEV results;
        [SerializeField] private AssetsFinderEngineState engineState;
        public AssetProcessorFactory AssetProcFactory { get; set; }

        public UIAssetsFinderMediator(IList<AssetInfoData> assetsData,  ResultsAssetsFinderEV results, AssetsFinderEngineState engineState)
        {
            this.assetsData = assetsData;
            this.results = results;
            this.engineState = engineState;
            this.CheckSerializeFields();
        }
        
        public void ExecuteStart()
        {
            if(AssetProcFactory == null)
                return;

            var assets = AssetInfoData.GetActiveAssetPaths(assetsData.Where(v => v.Validate())).Where(v => v != null);//".*", "*.h", etc.
            var checker = AssetProcFactory.Create(assets);
            if (checker != null)
            {
                results.UpdateData(new Dictionary<string, List<SceneGOData>>(), new List<string>());
                engineState.StartExecuting(checker, assets);
            }
        }

        public void OnExecuteStop()
        {
            results.UpdateData(engineState.FoundAssets, engineState.AllScenes);
        }
    }
}

#endif