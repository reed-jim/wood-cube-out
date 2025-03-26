#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using SearchEngine.EditorViews;
using SearchEngine.Additions;
using SearchEngine.EditorViews.AssetTypes;
using SearchEngine.EditorViews.Data;
using SearchEngine.EditorViews.FinderEngine;
using SearchEngine.EditorViews.Tables;

namespace SearchEngine.EditorViews.GCModeFacade
{
    public abstract class SearchEngineFacadeEV : IEditorViewDisposable
    {
        #region subsystems
        protected IEditorView assets;

        protected IEditorView scenes;
        protected WarningMsgEV scenesWarnMsg;
        protected IList<AssetInfoData> scenesData;
        protected AssetSortingEV scenesSorting;
        protected AssetInfoDataManagerAsList scenesDataManager;

        protected IEditorView results;
        protected IFinderEngineStateActive activeFEState;
        protected InactiveFinderEngineState inactiveEState;
        protected IUIEngineMediator uiEngineMediator;
        protected OperationTimerEV timer;

        protected WarningCheckerManager warningChecker;
        protected IWarningChecker wcAssets;
        protected IWarningChecker wcScenes;

        protected FinderEngine.FinderEngine finderEngine;
        protected ScenesRunner scenesRunner;

        protected SearchPanelEV searchPanel;
        protected SPLockedStateEV lockedState;
        private SPDefaultStateEV defaultState;
        private SPInProgressStateEV inProgressState;

        protected GCModeUIShowerEV uiShower;
        protected List<AssetInfoData> assetsList;
        #endregion
        
        #region creating subsystems
        public abstract void ReCreate();
        
        protected void CreateUIShower(string[] toolbarTitles)
        {
            inactiveEState = new InactiveFinderEngineState();
            finderEngine = new FinderEngine.FinderEngine(inactiveEState, activeFEState);
            activeFEState.Started += finderEngine.SeActiveState; 

            scenesRunner = new ScenesRunner(scenesData, finderEngine);
            timer = new OperationTimerEV();
            CreateSearchPanel();
            uiShower = new GCModeUIShowerEV(scenesRunner, assets, scenes, results, searchPanel, toolbarTitles);
            defaultState.UiShower = uiShower;
            lockedState.UiShower = uiShower;
        }

        protected void CreateSceneTable()
        {
            scenesSorting = new AssetSortingEV(AssetSortingTypes.Folder, AssetSortingTypes.Name);
            scenesData = scenesDataManager = new AssetInfoDataManagerAsList(new AssetsFilterEV(), scenesSorting);
            scenesSorting.ChangeEnable += scenesDataManager.OnSortingChangeEnable;
            scenesSorting.ChangeSorting += scenesDataManager.OnSortingChangeSorting;
            var tableAlg = new AssetInfoAlg(scenesData, null, scenesSorting, "Scene", "unity");
            tableAlg.AddCurrSceneButton = true;
            scenesWarnMsg = new WarningMsgEV();
            scenesWarnMsg.Msg = "Please add at least one scene here...";
            scenesWarnMsg.FontSize = 16;
            var table = new DataTableEV<AssetInfoData>(scenesData, tableAlg, scenesWarnMsg);
            table.TableDragAndDrop += tableAlg.OnTableDragAndDrop;
            table.DragAndDropEnabled = true;
            table.Items = "scenes";
            scenes = table;
        }

        protected void TuneObservers()
        {
            inactiveEState.EngineFinderInactive += scenesRunner.Stop;
            activeFEState.Started += scenesRunner.Start;
            scenesRunner.Complete += uiEngineMediator.OnExecuteStop;
            scenesRunner.Started += searchPanel.SetInProgressState;
            scenesRunner.Started += timer.ReStart;
            scenesRunner.Complete += timer.Stop;
            scenesRunner.Complete += searchPanel.SetDefaultState;
            scenesRunner.Complete += finderEngine.StopExecuting;
            scenesRunner.SceneComplete += finderEngine.OnSceneComplete;
            uiShower.ClearAllTables += () =>
            {
                EventSender.SendEvent(ClearAllTables);
            };
        }
        
        private void CreateSearchPanel()
        {
            lockedState = new SPLockedStateEV(warningChecker, wcAssets, wcScenes);
            defaultState = new SPDefaultStateEV(warningChecker);
            inProgressState = new SPInProgressStateEV(timer, scenesRunner);

            searchPanel = new EditorViews.SearchPanelEV(defaultState, lockedState, inProgressState);

            defaultState.StartBtnClicked += uiEngineMediator.ExecuteStart;
            defaultState.SetLockedState += searchPanel.SetLockedState;
            lockedState.SetDefaultState += searchPanel.SetDefaultState;
            inProgressState.StopBtnClicked += scenesRunner.Break;
        }
        #endregion

        #region facade interface
        public event Action ClearAllTables;

        public bool IsSearching
        {
            get { return scenesRunner.InProgress; }
        }
        public void ShowGUI()
        {
            uiShower.ShowGUI();
        }
        public void Dispose()
        {
            scenesRunner.Stop();
        }
        #endregion
    }
}

#endif