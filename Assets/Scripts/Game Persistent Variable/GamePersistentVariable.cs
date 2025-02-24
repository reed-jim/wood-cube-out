using UnityEngine;

public static class GamePersistentVariable
{
    public static Vector2 canvasSize;
    public static Vector2 canvasScale;
    public static string iapInitializeFailReason;

    public static Vector2 screenSizeWorld;

    #region LEVEL OBSERVER
    public static int levelPrefab; 
    public static int numScrewBoxUnlocked; 
    #endregion

    #region SETTING
    public static bool IsVibrate;
    #endregion
}
