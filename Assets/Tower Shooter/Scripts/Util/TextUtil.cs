using System;
using UnityEngine;

public static class TextUtil
{
    private const double KmPerLightYear = 9_460_730_472_580.8;
    private const double LightSpeedKmPerSecond = 299_792.458;
    private const double OnePercentLightSpeedKmPerSecond = 2_792.458;

    public static string FormatFloat(float value)
    {
        // Adjust the number of '#' characters to set your maximum decimal limit
        return value.ToString("0.#####", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string FormatDistance(double km)
    {
        if (km < KmPerLightYear)
            return $"{km:0} km";

        return $"{km / KmPerLightYear:0.#####} ly";
    }

    public static string FormatSpeed(double kmPerSecond, double InversedLightSpeedKmPerSecond)
    {
        // if (kmPerSecond < OnePercentLightSpeedKmPerSecond)
        // {
        //     return $"{kmPerSecond:0.#####}km/h";
        // }

        // return $"{kmPerSecond * InversedLightSpeedKmPerSecond:0.#####}c";

        return $"{kmPerSecond * InversedLightSpeedKmPerSecond:0.####}";
    }

    public static string GetDurationString(double kmPerFrame, double totalKm, float fps = 60f)
    {
        if (kmPerFrame <= 0.0 || totalKm <= 0.0 || fps <= 0.0)
            return "00:00";

        double totalSeconds = totalKm / kmPerFrame / fps;

        TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

        return time.TotalHours >= 1
            ? time.ToString(@"hh\:mm\:ss")
            : time.ToString(@"mm\:ss");
    }
}
