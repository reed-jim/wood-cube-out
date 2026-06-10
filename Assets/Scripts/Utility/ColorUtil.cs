using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class ColorUtil
{
    public static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    public static Color GetGradientColor(Color color1, Color color2, float percent)
    {
        return Color.Lerp(color1, color2, percent);
    }

    public static Color GetRandomGradientColor(Color color1, Color color2)
    {
        int priority = Random.Range(0, 10);

        int randomNumber = Random.Range(0, 100);

        if (priority < 1)
        {
            randomNumber = Random.Range(20, 100);
        }
        else
        {
            randomNumber = Random.Range(0, 20);
        }

        return Color.Lerp(color1, color2, randomNumber / 100f);
    }

    public static int GetRandomNumber()
    {
        int priority = Random.Range(0, 10);

        int randomNumber = Random.Range(0, 100);

        if (priority < 1)
        {
            randomNumber = Random.Range(20, 100);
        }
        else
        {
            randomNumber = Random.Range(0, 20);
        }

        return randomNumber;
    }

    public static Color HexToColor(string hex)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

    public static string ColorToHex(Color color)
    {
        string hex = "#" + ColorUtility.ToHtmlStringRGB(color);
        
        return hex;
    }
    
    public static Color GetColorFromHex(string hex)
    {
        string validatedHex = hex.Replace("#", "");

        if (validatedHex.Length < 6)
        {
            throw new System.FormatException("Needs a string with a length of at least 6");
        }

        var r = validatedHex.Substring(0, 2);
        var g = validatedHex.Substring(2, 2);
        var b = validatedHex.Substring(4, 2);

        string alpha;

        if (validatedHex.Length >= 8)
        {
            alpha = validatedHex.Substring(6, 2);
        }
        else
        {
            alpha = "FF";
        }

        return
        new Color
        (
            int.Parse(r, NumberStyles.HexNumber) / 255f,
            int.Parse(g, NumberStyles.HexNumber) / 255f,
            int.Parse(b, NumberStyles.HexNumber) / 255f,
            int.Parse(alpha, NumberStyles.HexNumber) / 255f
        );
    }

    public static Color ToGrayscale(Color color)
    {
        float grayValue = (color.r + color.g + color.b) / 3f;

        return new Color(grayValue, grayValue, grayValue);
    }

    public static Color GetBrighterColor(Color color, float increase)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);

        v = Mathf.Clamp01(v + increase);

        return Color.HSVToRGB(h, s, v);
    }

    public static Color GetNearWhiteColor(Color color, float saturationAmount = 0.2f, float brightness = 1f)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);

        s = Mathf.Clamp01(saturationAmount);
        v = Mathf.Clamp01(brightness);

        return Color.HSVToRGB(h, s, v);
    }

    #region MATH
    public static Color Multiply(this Color color, float multiplier)
    {
        Color finalColor = color * multiplier;

        finalColor.a = color.a;

        return finalColor;
    }
    #endregion
}
