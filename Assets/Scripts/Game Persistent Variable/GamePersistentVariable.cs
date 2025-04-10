using System.Collections.Generic;
using UnityEngine;

public enum CloudStorageRequest
{
    Upload,
    Download
}

public static class GamePersistentVariable
{
    public static Vector2 canvasSize;
    public static Vector2 canvasSizeAfterCanvasScaler;
    public static Vector2 canvasScale;
    public static string iapInitializeFailReason;

    public static Vector2 screenSizeWorld;

    #region LEVEL OBSERVER
    public static int currentLevel;
    public static int levelPrefab;
    public static bool isFreshLevel;
    public static bool isLoadingOldLevelState;
    public static bool isPreventLoadLevelData;
    public static int numScrewBoxUnlocked;
    public static int numOutsideScrew;
    public static int numHolesAdded;
    public static bool isAllowedSkipLevel;
    public static bool isPassedStepOneTutorial;
    public static Dictionary<int, List<Transform>> pieceDependencies;
    public static Vector3 levelOffset;
    public static Vector3 boxForOutsideScrewScreenPosition;
    public static RaycastHit lastScrewSelectedSurfaceHit;
    #endregion

    #region LEVEL IN PROGRESS
    public static bool isEnableLoadingLevelInProgress;
    public static bool isLoadingLevelInProgress;
    #endregion

    #region AUTHENTICATION
    public static string FirebaseUserId;
    #endregion

    #region BACKUP
    public static CloudStorageRequest cloudStorageRequest;
    public static bool isCloudStorageOperationCompleted;
    #endregion

    #region AUTO TEST
    public static bool isAutoTestingLevel;
    #endregion

    #region SETTING
    public static bool IsVibrate;
    #endregion

    #region SOUND
    public static bool isSoundTightenScrewPlaying;
    #endregion

    #region FIREBASE
    public static bool isFirebaseAvailable;
    public static bool isFirebaseRemoteConfigLoaded;
    #endregion

    #region ADS
    public static bool remoteConfigIsShowMREC;
    #endregion

    #region IAP
    public static string IAPLocation;
    public static int IAPAddition;
    public static bool isUnlockedAllScrewBoxes;
    #endregion
}
