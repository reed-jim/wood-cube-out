using UnityEngine;

public static class TextUtil
{
    private const double KmPerLightYear = 9_460_730_472_580.8;
    private const double LightSpeedKmPerSecond = 299_792.458;

    public static string FormatDistance(double km)
    {
        if (km < KmPerLightYear)
            return $"{km:0.#####} km";

        return $"{km / KmPerLightYear:0.#####} ly";
    }

    public static string FormatSpeed(double kmPerSecond)
    {
        return $"{kmPerSecond / LightSpeedKmPerSecond:0.#####}c";
    }

    // public static string FormatDistance(double km)
    // {
    //     const double KmPerLightYear = 9_460_730_472_580.8;

    //     if (km < 1_000_000)
    //         return $"{km:0} km";

    //     if (km < 1_000_000_000)
    //         return $"{km / 1_000_000:0.#}M km";

    //     if (km < 1_000_000_000_000)
    //         return $"{km / 1_000_000_000:0.##}B km";

    //     return $"{km / KmPerLightYear:0.##} ly";
    // }

    // public static string FormatSpeed(double kmPerSecond)
    // {
    //     if (kmPerSecond < 1_000)
    //         return $"{kmPerSecond:0.#} km/s";

    //     if (kmPerSecond < 1_000_000)
    //         return $"{kmPerSecond / 1_000:0.#}K km/s";

    //     if (kmPerSecond < 1_000_000_000)
    //         return $"{kmPerSecond / 1_000_000:0.##}M km/s";

    //     return $"{kmPerSecond / 1_000_000_000:0.##}B km/s";
    // }
}
