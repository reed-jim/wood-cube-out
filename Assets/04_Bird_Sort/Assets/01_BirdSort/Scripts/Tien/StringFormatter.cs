using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringFormatter : MonoBehaviour
{
    public static string FormatNumberSIF(int number)
    {
        if (number >= 10000000)
            return (number / 1000000d).ToString("#,0M");

        if (number >= 1000000)
            return (number / 1000000d).ToString("0.#") + "M";

        if (number >= 100000)
            return (number / 1000d).ToString("#,0K");

        if (number >= 10000)
            return (number / 1000d).ToString("0.#") + "K";

        return number.ToString("#,0");
    }
}
