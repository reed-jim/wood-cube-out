using UnityEngine;

public static class DebugUtil
{
    public static void Log(string message, Color color)
    {
        string hex = ColorUtility.ToHtmlStringRGB(color);
        Debug.Log($"<color=#{hex}>{message}</color>");
    }

    public static void Log(string message, string hexColor)
    {
        Debug.Log($"<color={hexColor}>{message}</color>");
    }
}
