#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.EditorViews.AssetTypes;
using SearchEngine.EditorViews.Data;
using SearchEngine.EditorViews.FinderEngine;
using SearchEngine.EditorViews.Results;
using SearchEngine.EditorViews.Tables;
using SearchEngine.Memento;

namespace SearchEngine.EditorViews.GCModeFacade
{
    public class MRFSearchEngineFacadeEV : SearchEngineFacadeEV, IOriginator<MRFSearchEngineFacadeEV.Memento>
    {
        #region subsystems
        protected WarningMsgEV assetsWarnMsg;
        protected MissingRefTypesEV assetsMRF;
        protected ResultsMissingRefsEV resultsMRF;
        protected ResultsInspPropsMRFEV resultsInspPropsMRF;
        protected MissingRefsFinderState activeFEStateMRF;
        protected List<SceneGOData> missingGOData;
        protected List<SceneGOData> missingCompsData;
        #endregion

        #region creating subsystems
        public override void ReCreate()
        {
            assetsWarnMsg = new WarningMsgEV();
            assetsWarnMsg.Msg = "Please activate at least one missing ref toggle here...";
            assetsWarnMsg.FontSize = 16;
            assetsMRF = new MissingRefTypesEV(assetsWarnMsg);

            CreateSceneTable();

            resultsInspPropsMRF = new ResultsInspPropsMRFEV(scenesData);
            missingGOData = new List<SceneGOData>();
            missingCompsData = new List<SceneGOData>();
            resultsMRF = CreateResultsMissingRefs();
            activeFEStateMRF = new MissingRefsFinderState(missingGOData, missingCompsData);

            uiEngineMediator = new UIMissingRefsMediator(assetsMRF, resultsInspPropsMRF, activeFEStateMRF);

            string[] toolbarTitles = { "Refs for search", "Scenes for search", "Found missing refs" };
            wcAssets = new WarningCheckerMissingRefs(assetsMRF, "missing refs toggle ", toolbarTitles[0]);
            wcScenes = new WarningCheckerAssets(scenesData, "scene", toolbarTitles[1]);
            warningChecker = new WarningCheckerManager(wcAssets, wcScenes);
            
            assets = assetsMRF;
            results = resultsMRF;
            activeFEState = activeFEStateMRF;

            CreateUIShower(toolbarTitles);
            TuneObservers();

            lockedState.AssetsWarning += assetsWarnMsg.Activate;
            lockedState.ScenesWarning += scenesWarnMsg.Activate;
        }

        private ResultsMissingRefsEV CreateResultsMissingRefs()
        {
            var tableAlg1 = new SceneGOAlg(missingGOData);
            var table1 = new DataTableEV<SceneGOData>(missingGOData, tableAlg1);
            tableAlg1.SubWindowTitle = "Found missing prefabs and models in scene";
            table1.TableHeight = 300; 

            var tableAlg2 = new SceneGOAlg(missingCompsData);
            var table2 = new DataTableEV<SceneGOData>(missingCompsData, tableAlg2);
            tableAlg2.SubWindowTitle = "Found missing mono scripts in scene";
            table2.TableHeight = 300;

            return new ResultsMissingRefsEV(table1, table2, resultsInspPropsMRF, "No results");
        }
        #endregion

        #region  memento part        
        public Memento GetMemento()
        {
            return new Memento
            {
                uiShower = this.uiShower.GetMemento(),

                assets = ((MissingRefTypesEV) this.assets).GetMemento(),

                scenesSorting = this.scenesSorting.GetMemento(),
                scenesDataManager = this.scenesDataManager.GetMemento(),

                results = resultsMRF.GetMemento(),
                missingGOData = CopyAbleHelper.ToList(this.missingGOData),
                missingCompsData = CopyAbleHelper.ToList(this.missingCompsData),
                resultsInspPropsMRF = resultsInspPropsMRF.GetMemento(),
            };
        }

        public void SetMemento(Memento mem)
        {
            if (mem == null || !mem.Validate())
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }
            
            this.uiShower.SetMemento(mem.uiShower);

            ((MissingRefTypesEV) this.assets).SetMemento(mem.assets);

            this.scenesSorting.SetMemento(mem.scenesSorting);
            this.scenesDataManager.SetMemento(mem.scenesDataManager);

            this.missingGOData.AddRange(CopyAbleHelper.ToEnum(mem.missingGOData));
            this.missingCompsData.AddRange(CopyAbleHelper.ToEnum(mem.missingCompsData));
            this.resultsInspPropsMRF.SetMemento(mem.resultsInspPropsMRF);
            this.resultsMRF.SetMemento(mem.results);
        }
        [Serializable]
        public class Memento : IValidatable
        {
            public GCModeUIShowerEV.Memento uiShower;
            public MissingRefTypesEV.Memento assets;

            public AssetSortingEV.Memento scenesSorting;
            public AssetInfoDataManagerAsList.Memento scenesDataManager;

            public ResultsMissingRefsEV.Memento results;
            public List<SceneGOData> missingGOData;
            public List<SceneGOData> missingCompsData;
            public ResultsInspPropsMRFEV.Memento resultsInspPropsMRF;

            public bool Validate()
            {
                return
                    uiShower != null
                    && assets != null
                    && scenesSorting != null
                    && scenesDataManager != null
                    && results != null
                    && missingGOData != null
                    && missingCompsData != null
                    && resultsInspPropsMRF != null
                    && missingGOData.All(v => v != null && v.Validate())
                    && missingCompsData.All(v => v != null && v.Validate());
            }
        }
        #endregion
    }
}

#endif