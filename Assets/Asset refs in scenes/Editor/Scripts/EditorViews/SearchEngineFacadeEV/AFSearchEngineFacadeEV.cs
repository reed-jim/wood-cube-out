#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using SearchEngine.Checkers;
using SearchEngine.EditorViews.AssetTypes;
using SearchEngine.EditorViews.Data;
using SearchEngine.EditorViews.FinderEngine;
using SearchEngine.EditorViews.Results;
using SearchEngine.EditorViews.Tables;
using SearchEngine.Memento;

namespace SearchEngine.EditorViews.GCModeFacade
{
    public class AFSearchEngineFacadeEV : SearchEngineFacadeEV, IOriginator<AFSearchEngineFacadeEV.Memento>
    {
        #region subsystems
        protected AssetsFilterEV assetsFilter;
        protected WarningMsgEV assetsWarnMsg;
        protected AssetSortingEV assetsSorting;
        protected IList<AssetInfoData> assetsData;
        protected AssetInfoDataManagerAsList assetsDataManager;
        
        protected ResultsAssetsFinderEV resultsAF;

        protected UIAssetsFinderMediator mediatorAF;

        protected AssetsFinderEngineState activeFEStateAF;
        #endregion

        #region creating subsystems
        public override void ReCreate()
        {
            //assets
            assetsFilter = new AssetsFilterEV();
            assetsSorting = new AssetSortingEV(AssetSortingTypes.Folder, AssetSortingTypes.Name, AssetSortingTypes.Type);
            assetsData = assetsDataManager = new AssetInfoDataManagerAsList(assetsFilter, assetsSorting);
            assetsFilter.ChangeEnable += assetsDataManager.OnFilterChangeEnable;
            assetsFilter.ChangeFilter += assetsDataManager.OnFilterChangeFilter;
            assetsSorting.ChangeEnable += assetsDataManager.OnSortingChangeEnable;
            assetsSorting.ChangeSorting += assetsDataManager.OnSortingChangeSorting;

            var tableAlg = new AssetInfoAlg(assetsData, assetsFilter, assetsSorting, "Asset", "");
            assetsFilter.ChangeEnable += tableAlg.UpdateActiveToggle;
            assetsFilter.ChangeFilter += tableAlg.UpdateActiveToggle;
            assetsWarnMsg = new WarningMsgEV();
            assetsWarnMsg.Msg = "Please add at least one asset here...";
            assetsWarnMsg.FontSize = 16;
            var table = new DataTableEV<AssetInfoData>(assetsData, tableAlg, assetsWarnMsg);
            table.TableDragAndDrop += tableAlg.OnTableDragAndDrop;
            table.DragAndDropEnabled = true;
            table.Items = "assets";
            assets = table;

            CreateSceneTable();

            resultsAF = new ResultsAssetsFinderEV(scenesData);

            activeFEStateAF = new AssetsFinderEngineState();

            mediatorAF = new UIAssetsFinderMediator(assetsData,  resultsAF, activeFEStateAF);
            uiEngineMediator = mediatorAF;
            mediatorAF.AssetProcFactory= new AssetCheckerFactory();

            string[] toolbarTitles = { "Assets for search", "Scenes for search", "Found assets" };
            wcAssets = new WarningCheckerAssets(assetsData, "asset", toolbarTitles[0]);
            wcScenes = new WarningCheckerAssets(scenesData, "scene", toolbarTitles[1]);
            warningChecker = new WarningCheckerManager(wcAssets, wcScenes);
            
            results = resultsAF;
            activeFEState = activeFEStateAF;

            CreateUIShower(toolbarTitles);
            
            TuneObservers();

            lockedState.AssetsWarning += assetsWarnMsg.Activate;
            lockedState.ScenesWarning += scenesWarnMsg.Activate;
        }
        #endregion

        #region  mementoAF part
        public Memento GetMemento()
        {
            return new Memento
            {
                uiShower = this.uiShower.GetMemento(),

                assetsFilter = this.assetsFilter.GetMemento(),
                assetsSorting = this.assetsSorting.GetMemento(),
                assetsDataManager = this.assetsDataManager.GetMemento(),

                scenesSorting = this.scenesSorting.GetMemento(),
                scenesDataManager = this.scenesDataManager.GetMemento(),

                results = this.resultsAF.GetMemento(),
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

            this.assetsFilter.SetMemento(mem.assetsFilter);
            this.assetsSorting.SetMemento(mem.assetsSorting);
            this.assetsDataManager.SetMemento(mem.assetsDataManager);

            this.scenesSorting.SetMemento(mem.scenesSorting);
            this.scenesDataManager.SetMemento(mem.scenesDataManager);

            this.resultsAF.SetMemento(mem.results);
        }

        [Serializable]
        public class Memento : IValidatable
        {
            public GCModeUIShowerEV.Memento uiShower;

            public AssetsFilterEV.Memento assetsFilter;
            public AssetSortingEV.Memento assetsSorting;
            public AssetInfoDataManagerAsList.Memento assetsDataManager;

            public AssetSortingEV.Memento scenesSorting;
            public AssetInfoDataManagerAsList.Memento scenesDataManager;

            public ResultsAssetsFinderEV.Memento results;

            public bool Validate()
            {
                return
                    uiShower != null
                    && assetsFilter != null
                    && assetsSorting != null
                    && assetsDataManager != null
                    && scenesSorting != null
                    && scenesDataManager != null
                    && results != null;
            }
        }       
        #endregion        
    }
}

#endif