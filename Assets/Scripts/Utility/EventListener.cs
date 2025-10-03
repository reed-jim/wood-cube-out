using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.GamePlay;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    
    public class EventListener : MonoBehaviour
    {
        #region Private Serializable Fields
        
        [SerializeField] private bool isActive = true;

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            if (!isActive) return;
            // Subscribe to all static event Actions in the project
            BasicObjectPartSubscribe();
            BasePopupSubscribe();
            BaseScrewSubscribe();
            BoosterUISubscribe();
            BottomBarSubscribe();
            BuyBoosterPopupSubscribe();
            DebugPopupSubscribe();
            GameLeaderboardManagerSubscribe();
            GameVariableInitializerSubscribe();
            LevelLoaderSubscribe();
            LoadLevelErrorSceneSubscribe();
            LockScrewBoxManagerSubscribe();
            LockScrewColorLevelManagerSubscribe();
            MultiPhaseScrewSubscribe();
            // RemoteAddressablesLoaderSubscribe();
            // RemoteConfigControllerSubscribe();
            ReplayPopupSubscribe();
            ReplenishLifeManagerSubscribe();
            RevivePopupSubscribe();
            RustyScrewSpecialLevelManagerSubscribe();
            // SalePackIAPManagerSubscribe();
            ScrewBoxSubscribe();
            ScrewBoxCameraManagerSubscribe();
            ScrewBoxManagerSubscribe();
            ScrewManagerSubscribe();
            ScrewsDataManagerSubscribe();
            // SwipeGestureSubscribe();
            // SwipingScreenSubscribe();
            // TopBarSubscribe();
            // UserLivesUISubscribe();
            
            
        }

        private void LockScrewColorLevelManagerSubscribe()
        {
            LockScrewColorLevelManager.lockScrewColorManagerInitEvent += (manager) => DebugLogger.LogWithFrame($"LockScrewColorLevelManager.lockScrewColorManagerInitEvent: {manager}", Color.yellow);
        }

        private void RustyScrewSpecialLevelManagerSubscribe()
        {
            RustyScrewSpecialLevelManager.rustyScrewManagerInitEvent += (manager) => DebugLogger.LogWithFrame($"RustyScrewSpecialLevelManager.rustyScrewManagerInitEvent: {manager}", Color.yellow);
        }

        #endregion

        #region Public Methods
        
        public static void BasicObjectPartSubscribe()
        {
            BasicObjectPart.loosenScrewOnObjectBrokenEvent += (screw) => DebugLogger.LogWithFrame("BasicObjectPart.loosenScrewOnObjectBrokenEvent", Color.yellow);
        }
        public static void BasePopupSubscribe()
        {
            BasePopup.popupShowEvent += () => DebugLogger.LogWithFrame("BasePopup.popupShowEvent", Color.yellow);
            BasePopup.popupHideEvent += () => DebugLogger.LogWithFrame("BasePopup.popupHideEvent", Color.yellow);
            BasePopup.enableSwipingScreenEvent += (b) => DebugLogger.LogWithFrame($"BasePopup.enableSwipingScreenEvent: {b}", Color.yellow);
            BasePopup.showAdNotLoadedPopupEvent += () => DebugLogger.LogWithFrame("BasePopup.showAdNotLoadedPopupEvent", Color.yellow);
            BasePopup.switchPopupRouteEvent += (route) => DebugLogger.LogWithFrame($"BasePopup.switchPopupRouteEvent: {route}", Color.yellow);
            BasePopup.showNotificationEvent += (a, b) => DebugLogger.LogWithFrame($"BasePopup.showNotificationEvent: {a}, {b}", Color.yellow);
            BasePopup.showResourcesEarnPopupEvent += (a, b, c) => DebugLogger.LogWithFrame($"BasePopup.showResourcesEarnPopupEvent: {a}, {b}, {c}", Color.yellow);
        }
        public static void BaseScrewSubscribe()
        {
            BaseScrew.autoSelectScrewEvent += (screw, faction) => DebugLogger.LogWithFrame($"BaseScrew.autoSelectScrewEvent: {screw}, {faction}", Color.yellow);
            BaseScrew.selectScrewEvent += (screw, faction) => DebugLogger.LogWithFrame($"BaseScrew.selectScrewEvent: {screw}, {faction}", Color.yellow);
            BaseScrew.screwMovedCompletedEvent += (screw) => DebugLogger.LogWithFrame($"BaseScrew.screwMovedCompletedEvent: {screw}", Color.yellow);
        }
        public static void BoosterUISubscribe()
        {
            BoosterUI.addMoreScrewPortEvent += () => DebugLogger.LogWithFrame("BoosterUI.addMoreScrewPortEvent", Color.yellow);
            BoosterUI.clearAllScrewPortsEvent += () => DebugLogger.LogWithFrame("BoosterUI.clearAllScrewPortsEvent", Color.yellow);
            BoosterUI.shakeScrewPortsEvent += () => DebugLogger.LogWithFrame("BoosterUI.shakeScrewPortsEvent", Color.yellow);
        }
        public static void BottomBarSubscribe()
        {
            BottomBar.setHighlightPositionEvent += (f) => DebugLogger.LogWithFrame($"BottomBar.setHighlightPositionEvent: {f}", Color.yellow);
            BottomBarItem.moveBottomBarHighlightEvent += (f) => DebugLogger.LogWithFrame("BottomBarItem.moveBottomBarHighlightEvent", Color.yellow);
            BottomBarItem.switchRouteEvent += (route) => DebugLogger.LogWithFrame($"BottomBarItem.switchRouteEvent: {route}", Color.yellow);
        }
        public static void BuyBoosterPopupSubscribe()
        {
            BuyBoosterPopup.useBoosterEvent += (type) => DebugLogger.LogWithFrame($"BuyBoosterPopup.useBoosterEvent: {type}", Color.yellow);
            BuyBoosterPopup.unlockScrewBoxEvent += () => DebugLogger.LogWithFrame("BuyBoosterPopup.unlockScrewBoxEvent", Color.yellow);
            BuyBoosterPopup.openIAPShopPopupEvent += (route) => DebugLogger.LogWithFrame($"BuyBoosterPopup.openIAPShopPopupEvent: {route}", Color.yellow);
            BuyBoosterPopup.switchRouteEvent += (route) => DebugLogger.LogWithFrame($"BuyBoosterPopup.switchRouteEvent: {route}", Color.yellow);
            BuyBoosterPopup.updateBoosterQuantityEvent += (q) => DebugLogger.LogWithFrame($"BuyBoosterPopup.updateBoosterQuantityEvent: {q}", Color.yellow);
        }
        public static void DebugPopupSubscribe()
        {
            DebugPopup.toLevelEvent += (level) => DebugLogger.LogWithFrame($"DebugPopup.toLevelEvent: {level}", Color.yellow);
        }
        public static void GameLeaderboardManagerSubscribe()
        {
            GameLeaderboardManager.fetchLeaderboardDataCompletedEvent += () => DebugLogger.LogWithFrame("GameLeaderboardManager.fetchLeaderboardDataCompletedEvent", Color.yellow);
        }
        public static void GameVariableInitializerSubscribe()
        {
            GameVariableInitializer.currentLevelFetchedEvent += () => DebugLogger.LogWithFrame("GameVariableInitializer.currentLevelFetchedEvent", Color.yellow);
            GameVariableInitializer.gameSettingLoadedEvent += () => DebugLogger.LogWithFrame("GameVariableInitializer.gameSettingLoadedEvent", Color.yellow);
        }
        public static void LevelLoaderSubscribe()
        {
            LevelLoader.startLevelEvent += () => DebugLogger.LogWithFrame("LevelLoader.startLevelEvent", Color.yellow);
            LevelLoader.setLevelNumberEvent += () => DebugLogger.LogWithFrame("LevelLoader.setLevelNumberEvent", Color.yellow);
            LevelLoader.setLevelScrewNumberEvent += (num) => DebugLogger.LogWithFrame($"LevelLoader.setLevelScrewNumberEvent: {num}", Color.yellow);
            LevelLoader.setMultiPhaseLevelScrewNumberEvent += (num) => DebugLogger.LogWithFrame($"LevelLoader.setMultiPhaseLevelScrewNumberEvent: {num}", Color.yellow);
            LevelLoader.showSceneTransitionEvent += () => DebugLogger.LogWithFrame("LevelLoader.showSceneTransitionEvent", Color.yellow);
            LevelLoader.switchRouteEvent += (route) => DebugLogger.LogWithFrame($"LevelLoader.switchRouteEvent: {route}", Color.yellow);
            LevelLoader.setIsLevelLoadedEvent += () => DebugLogger.LogWithFrame("LevelLoader.setIsLevelLoadedEvent", Color.yellow);
            LevelLoader.enableSwipingEvent += (enabled) => DebugLogger.LogWithFrame($"LevelLoader.enableSwipingEvent: {enabled}", Color.yellow);
            LevelLoader.setTargetOrthographicSizeEvent += (size) => DebugLogger.LogWithFrame($"LevelLoader.setTargetOrthographicSizeEvent: {size}", Color.yellow);
            LevelLoader.showLoadLevelErrorEvent += (show) => DebugLogger.LogWithFrame($"LevelLoader.showLoadLevelErrorEvent: {show}", Color.yellow);
        }
        public static void LoadLevelErrorSceneSubscribe()
        {
            LoadLevelErrorScene.retryLoadLevelEvent += () => DebugLogger.LogWithFrame("LoadLevelErrorScene.retryLoadLevelEvent", Color.yellow);
            LoadLevelErrorScene.retryLoadRemoteContentEvent += () => DebugLogger.LogWithFrame("LoadLevelErrorScene.retryLoadRemoteContentEvent", Color.yellow);
        }
        public static void LockScrewBoxManagerSubscribe()
        {
            LockScrewBoxManager.requestLockedScrewBox += (screwBox, record) => DebugLogger.LogWithFrame($"Locked Screw Box requested: {screwBox}, Record: {record}", Color.yellow);
            LockScrewBoxManager.lockScrewBoxManagerInitEvent += (manager) => DebugLogger.LogWithFrame($"LockScrewBoxManager.lockScrewBoxManagerInitEvent: {manager}", Color.yellow);
        }
        public static void MultiPhaseScrewSubscribe()
        {
            MultiPhaseScrew.manageScrewEvent += (screw) => DebugLogger.LogWithFrame($"MultiPhaseScrew.manageScrewEvent: {screw}", Color.yellow);
        }
        public static void RemoteAddressablesLoaderSubscribe()
        {
            RemoteAddressablesLoader.updateDownloadProgressEvent += (progress) => DebugLogger.LogWithFrame($"RemoteAddressablesLoader.updateDownloadProgressEvent: {progress}", Color.yellow);
            RemoteAddressablesLoader.retryLoadLevelEvent += () => DebugLogger.LogWithFrame("RemoteAddressablesLoader.retryLoadLevelEvent", Color.yellow);
            RemoteAddressablesLoader.downloadRemoteContentCompletedEvent += () => DebugLogger.LogWithFrame("RemoteAddressablesLoader.downloadRemoteContentCompletedEvent", Color.yellow);
        }
        public static void RemoteConfigControllerSubscribe()
        {
            RemoteConfigController.remoteConfigReadyEvent += () => DebugLogger.LogWithFrame("RemoteConfigController.remoteConfigReadyEvent", Color.yellow);
        }
        public static void ReplayPopupSubscribe()
        {
            ReplayPopup.replayLevelEvent += () => DebugLogger.LogWithFrame("ReplayPopup.replayLevelEvent", Color.yellow);
            ReplayPopup.changeLivesNumberEvent += (num) => DebugLogger.LogWithFrame($"ReplayPopup.changeLivesNumberEvent: {num}", Color.yellow);
        }
        public static void ReplenishLifeManagerSubscribe()
        {
            // ReplenishLifeManager.updateLivesNumberEvent += (num) => DebugLogger.LogWithFrame("ReplenishLifeManager.updateLivesNumberEvent", Color.yellow);
        }
        public static void RevivePopupSubscribe()
        {
            RevivePopup.reviveEvent += (type) => DebugLogger.LogWithFrame("RevivePopup.reviveEvent", Color.yellow);
        }
        public static void SalePackIAPManagerSubscribe()
        {
            SalePackIAPManager.suggestSalePackEvent += (type) => DebugLogger.LogWithFrame("SalePackIAPManager.suggestSalePackEvent", Color.yellow);
            SalePackIAPManager.updateSalePackVisibilityEvent += () => DebugLogger.LogWithFrame("SalePackIAPManager.updateSalePackVisibilityEvent", Color.yellow);
        }
        public static void ScrewBoxSubscribe()
        {
            ScrewBox.screwBoxCompletedEvent += (box) => DebugLogger.LogWithFrame("ScrewBox.screwBoxCompletedEvent", Color.yellow);
            ScrewBox.spawnNewScrewBoxEvent += () => DebugLogger.LogWithFrame("ScrewBox.spawnNewScrewBoxEvent", Color.yellow);
            ScrewBox.setFactionForScrewBoxEvent += (box) => DebugLogger.LogWithFrame("ScrewBox.setFactionForScrewBoxEvent", Color.yellow);
            ScrewBox.screwBoxUnlockedEvent += (box) => DebugLogger.LogWithFrame("ScrewBox.screwBoxUnlockedEvent", Color.yellow);
            ScrewBox.shakeCameraEvent += () => DebugLogger.LogWithFrame("ScrewBox.shakeCameraEvent", Color.yellow);
            ScrewBox.playFullscreenEffectEvent += (color) => DebugLogger.LogWithFrame("ScrewBox.playFullscreenEffectEvent", Color.yellow);
        }
        public static void ScrewBoxCameraManagerSubscribe()
        {
            ScrewBoxCameraManager.setCameraEvent += (cam) => DebugLogger.LogWithFrame("ScrewBoxCameraManager.setCameraEvent", Color.yellow);
        }
        public static void ScrewBoxManagerSubscribe()
        {
            ScrewBoxManager.looseScrewEvent += (id, faction, slot) => DebugLogger.LogWithFrame("ScrewBoxManager.looseScrewEvent", Color.yellow);
            ScrewBoxManager.screwBoxSpawnedEvent += (screw) => DebugLogger.LogWithFrame($"ScrewBoxManager.screwBoxSpawnedEvent: {screw}", Color.yellow);
            ScrewBoxManager.screwBoxLockSpawnedEvent += (screw) => DebugLogger.LogWithFrame($"ScrewBoxManager.screwBoxLockSpawnedEvent: {screw}", Color.yellow);
            ScrewBoxManager.screwPortsClearedEvent += (num) => DebugLogger.LogWithFrame($"ScrewBoxManager.screwPortsClearedEvent: {num}", Color.yellow);
            ScrewBoxManager.enableBoosterEvent += (enabled) => DebugLogger.LogWithFrame($"ScrewBoxManager.screwBoxCompletedEvent: {enabled}", Color.yellow);
        }
        public static void ScrewManagerSubscribe()
        {
            ScrewManager.spawnScrewBoxEvent += (faction) => DebugLogger.LogWithFrame("ScrewManager.spawnScrewBoxEvent", Color.yellow);
            ScrewManager.spawnAdsScrewBoxesEvent += () => DebugLogger.LogWithFrame("ScrewManager.spawnAdsScrewBoxesEvent", Color.yellow);
            ScrewManager.winLevelEvent += () => DebugLogger.LogWithFrame("ScrewManager.winLevelEvent", Color.yellow);
            ScrewManager.setScrewsEvent += (screws) => DebugLogger.LogWithFrame("ScrewManager.setScrewsEvent", Color.yellow);
            ScrewManager.updateRemainingScrewEvent += (a, b) => DebugLogger.LogWithFrame($"ScrewManager.updateRemainingScrewEvent: {a}, {b}", Color.yellow);
            ScrewManager.enableBoosterEvent += (enabled) => DebugLogger.LogWithFrame($"ScrewManager.enableBoosterEvent: {enabled}", Color.yellow);
            ScrewManager.levelSetupCompletedEvent += () => DebugLogger.LogWithFrame("ScrewManager.levelSetupCompletedEvent", Color.yellow);
            ScrewManager.updateOutsideScrewNumberEvent += () => DebugLogger.LogWithFrame("ScrewManager.updateOutsideScrewNumberEvent", Color.yellow);
            ScrewManager.switchScrewsToSpecialTypeCompleteEvent += (screws) => DebugLogger.LogWithFrame("ScrewManager.switchScrewsToSpecialTypeCompleteEvent", Color.yellow);
        }
        public static void ScrewsDataManagerSubscribe()
        {
            ScrewsDataManager.spawnFreshLevelScrewBoxesEvent += () => DebugLogger.LogWithFrame("ScrewsDataManager.spawnFreshLevelScrewBoxesEvent", Color.yellow);
            ScrewsDataManager.updateRemainingScrewEvent += (a, b) => DebugLogger.LogWithFrame("ScrewsDataManager.updateRemainingScrewEvent", Color.yellow);
            ScrewsDataManager.updateOutsideScrewNumberEvent += () => DebugLogger.LogWithFrame("ScrewsDataManager.updateOutsideScrewNumberEvent", Color.yellow);
            ScrewsDataManager.endTutorialLevel1Event += () => DebugLogger.LogWithFrame("ScrewsDataManager.endTutorialLevel1Event", Color.yellow);
        }
        public static void SwipeGestureSubscribe()
        {
            SwipeGesture.swipeGestureEvent += (vec) => DebugLogger.LogWithFrame("SwipeGesture.swipeGestureEvent", Color.yellow);
            SwipeGesture.stopSwipeGestureEvent += () => DebugLogger.LogWithFrame("SwipeGesture.stopSwipeGestureEvent", Color.yellow);
        }
        public static void SwipingScreenSubscribe()
        {
            SwipingScreen.hideOutsideScreenEvent += (f) => DebugLogger.LogWithFrame("SwipingScreen.hideOutsideScreenEvent", Color.yellow);
        }
        public static void TopBarSubscribe()
        {
            TopBar.showPopupEvent += (route) => DebugLogger.LogWithFrame("TopBar.showPopupEvent", Color.yellow);
        }
        public static void UserLivesUISubscribe()
        {
            UserLivesUI.collectLivesEvent += (type, pos, a, b) => DebugLogger.LogWithFrame("UserLivesUI.collectLivesEvent", Color.yellow);
        }

       
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }
        
        public void DebugEmpty()
        {
            DebugLogger.LogWithFrame("--------------------------------", Color.yellow);
        }

        #endregion
    }
    
    [System.Flags]
    public enum EventClass
    {
        None = 0,
        BasicObjectPart = 1 << 0,
        BasePopup = 1 << 1,
        BaseScrew = 1 << 2,
        BoosterUI = 1 << 3,
        BottomBar = 1 << 4,
        BuyBoosterPopup = 1 << 5,
        DebugPopup = 1 << 6,
        GameLeaderboardManager = 1 << 7,
        GameVariableInitializer = 1 << 8,
        LevelLoader = 1 << 9,
        LoadLevelErrorScene = 1 << 10,
        LockScrewBoxManager = 1 << 11,
        MultiPhaseScrew = 1 << 12,
        ReplayPopup = 1 << 13,
        ReplenishLifeManager = 1 << 14,
        RevivePopup = 1 << 15,
        ScrewBox = 1 << 16,
        ScrewBoxCameraManager = 1 << 17,
        ScrewBoxManager = 1 << 18,
        ScrewManager = 1 << 19,
        ScrewsDataManager = 1 << 20,
        All = ~0
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(EventListener))]
    [CanEditMultipleObjects]
    public class EventListenerEditor : Editor
    {
        private EventListener script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (EventListener)target;

            ButtonResetValues();
        }

        private void ButtonResetValues()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Debug Empty", frogIcon), GUILayout.Width(InspectorConst.BUTTON_WIDTH_MEDIUM)))
            {
                script.DebugEmpty();
                EditorUtility.SetDirty(script); // Mark the object as dirty
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endif
}

