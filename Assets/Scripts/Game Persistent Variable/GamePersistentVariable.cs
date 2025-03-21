using System.Collections.Generic;
using UnityEngine;

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
    public static int numScrewBoxUnlocked; 
    public static int numOutsideScrew;
    public static bool isAllowedSkipLevel;
    public static bool isPassedStepOneTutorial; 
    public static Dictionary<int, List<Transform>> pieceDependencies;
    public static Vector3 levelOffset;
    public static Vector3 boxForOutsideScrewScreenPosition;
    public static RaycastHit lastScrewSelectedSurfaceHit;
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

    #region IAP
    public static string IAPLocation;
    public static int IAPAddition;
    #endregion
}
