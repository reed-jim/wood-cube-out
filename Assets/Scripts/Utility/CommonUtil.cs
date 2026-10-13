using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using Dreamteck.Utilities;
using PrimeTween;
using UnityEngine;
using static GameEnum;

public static class CommonUtil
{
    public static void EnableUIGlobally()
    {
        // UnityEngine.EventSystems.EventSystem.current.enabled = true;
    }

    public static void DisableUIGlobally()
    {
        // UnityEngine.EventSystems.EventSystem.current.enabled = false;
    }

    public static string ToPercent(float chance)
    {
        return $"{FormatNumber((int)(chance * 100))}%";
    }

    public static void CopyList<T>(List<T> source, List<T> destination)
    {
        destination.Clear();
        destination.AddRange(source);
    }

    private static readonly string[] Suffixes =
    {
        "", "K", "M", "B", "T",
        "Qa", "Qi", "Sx", "Sp", "Oc",
        "No", "Dc", "Ud", "Dd", "Td",
        "Qad", "Qid", "Sxd", "Spd", "Ocd",
        "Nod", "Vg"
    };

    public static string FormatNumber(double value)
    {
        if (double.IsNaN(value))
            return "NaN";

        if (double.IsInfinity(value))
            return "Infinity";

        if (value < 1000)
            return value.ToString("0.##");

        int suffixIndex = 0;

        while (value >= 1000 && suffixIndex < Suffixes.Length - 1)
        {
            value /= 1000;
            suffixIndex++;
        }

        return value.ToString("0.##") + Suffixes[suffixIndex];
    }

    public static string GetOrdinal(int number)
    {
        if (number % 100 >= 11 && number % 100 <= 13)
            return number + "th";

        switch (number % 10)
        {
            case 1: return number + "st";
            case 2: return number + "nd";
            case 3: return number + "rd";
            default: return number + "th";
        }
    }

    public static void SetParentAndResetScale(Transform target, Transform parent)
    {
        target.SetParent(parent);
        target.localScale = Vector3.one;
    }

    public static void ReturnListToPool<T>(List<T> items, Func<T, GameObject> getGameObject)
    {
        for (int i = 0; i < items.Count; i++)
        {
            T item = items[i];

            ObjectPoolingEverything.ReturnToPool(GameConstants.node, getGameObject(item).gameObject);
        }

        items.Clear();
    }

    public static void ResetRectTransform(RectTransform rt)
    {
        // Anchors
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);

        // Pivot
        rt.pivot = new Vector2(0.5f, 0.5f);

        // Position
        rt.anchoredPosition = Vector2.zero;
        rt.localPosition = Vector3.zero;

        // Size
        rt.sizeDelta = new Vector2(100f, 100f);

        // Offsets
        // rt.offsetMin = Vector2.zero;
        // rt.offsetMax = Vector2.zero;

        // Rotation
        rt.localRotation = Quaternion.identity;

        // Scale
        rt.localScale = Vector3.one;
    }

    public static void SetParentAndResetScale(Transform target, Transform parent, Vector3 localPosition)
    {
        target.SetParent(parent);
        target.localScale = Vector3.one;
        target.localPosition = localPosition;
    }

    #region Round
    public static float RoundToDecimalPlaces(float value, int decimalPlaces)
    {
        return (float)Math.Round(value, decimalPlaces, System.MidpointRounding.AwayFromZero);
    }
    #endregion

    public static Color GetMostCommonColor(List<Color> colors, out int count)
    {
        count = 0;

        if (colors == null || colors.Count == 0)
            return default;

        Dictionary<Color, int> counts = new();
        Color mostCommon = colors[0];

        foreach (Color color in colors)
        {
            counts.TryGetValue(color, out int currentCount);
            currentCount++;
            counts[color] = currentCount;

            if (currentCount > count)
            {
                count = currentCount;
                mostCommon = color;
            }
        }

        return mostCommon;
    }

    public static List<T> GetRandomItems<T>(List<T> list, int count)
    {
        count = Mathf.Min(count, list.Count);

        List<T> shuffled = new List<T>(list);

        // Fisher-Yates shuffle
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        return shuffled.GetRange(0, count);
    }

    public static Vector3 RectTransformToWorldPosition(
    RectTransform rectTransform,
    Camera worldCamera,
    float worldZ)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);

        Vector3 worldPoint = worldCamera.ScreenToWorldPoint(
            new Vector3(
                screenPoint.x,
                screenPoint.y,
                Mathf.Abs(worldZ - worldCamera.transform.position.z)));

        worldPoint.z = worldZ;
        return worldPoint;
    }

    public static void MoveInCurve(Transform target, Vector3 from, Vector3 to, float height, float duration, Action onComplete)
    {
        Vector3 mid = Vector3.Lerp(from, to, 0.5f);
        mid.y += height;

        Vector3[] path =
        {
            from,
            mid,
            to
        };

        target.DOPath(path, duration, DG.Tweening.PathType.CatmullRom)
        .SetEase(DG.Tweening.Ease.Linear)
        .OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    #region Component
    public static TDerived ReplaceAndCopy<TBase, TDerived>(GameObject obj)
        where TBase : Component
        where TDerived : TBase
    {
        TBase original = obj.GetComponent<TBase>();
        if (original == null)
        {
            Debug.LogError($"Missing component of type {typeof(TBase).Name} on {obj.name}");
            return null;
        }

        // Cache field info for performance
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var fields = typeof(TBase).GetFields(bindingFlags);

        // Save data
        object[] values = new object[fields.Length];
        for (int i = 0; i < fields.Length; i++)
            values[i] = fields[i].GetValue(original);

        // Remove original component
        UnityEngine.Object.Destroy(original);

        // Add new component
        TDerived replacement = obj.AddComponent<TDerived>();

        // Apply saved values
        for (int i = 0; i < fields.Length; i++)
        {
            fields[i].SetValue(replacement, values[i]);
        }

        return replacement;
    }
    #endregion

    #region POSITION
    public static Vector2Int GetVector2Int(Vector3 position)
    {
        return new Vector2Int(Mathf.CeilToInt(position.x), Mathf.CeilToInt(position.y));
    }

    public static Vector3Int GetVector3Int(Vector3 position)
    {
        return new Vector3Int(Mathf.CeilToInt(position.x), Mathf.CeilToInt(position.y), Mathf.CeilToInt(position.z));
    }

    public static List<Vector3> GetListVector3(List<Vector2> input, float z)
    {
        List<Vector3> output = new List<Vector3>();

        for (int i = 0; i < input.Count; i++)
        {
            output.Add(new Vector3(input[i].x, input[i].y, z));
        }

        return output;
    }

    public static Vector3 GetAveragePosition(List<Vector3> positions)
    {
        Vector3 result = Vector3.zero;

        foreach (var position in positions)
        {
            result += position;
        }

        result /= positions.Count;

        return result;
    }
    #endregion

    public static void StopTween(PrimeTween.Tween tween)
    {
        if (tween.isAlive)
        {
            tween.Stop();
        }
    }

    public static void StopAllTweens(List<PrimeTween.Tween> tweens)
    {
        if (tweens == null)
        {
            return;
        }

        for (int i = 0; i < tweens.Count; i++)
        {
            if (tweens[i].isAlive)
            {
                tweens[i].Stop();
            }
        }
    }

    public static void StopAllSequences(List<PrimeTween.Sequence> tweens)
    {
        if (tweens == null)
        {
            return;
        }

        for (int i = 0; i < tweens.Count; i++)
        {
            if (tweens[i].isAlive)
            {
                tweens[i].Stop();
            }
        }
    }

    public static void OnHitColorEffect(
        Renderer meshRenderer,
        MaterialPropertyBlock materialPropertyBlock,
        Color startColor,
        Color endColor,
        float duration,
        List<PrimeTween.Tween> tweens
    )
    {
        PrimeTween.Tween tween = PrimeTween.Tween.Custom(
            startColor, endColor,
            duration: duration,
            onValueChange: newVal =>
            {
                materialPropertyBlock.SetColor("_Albedo", newVal);
                meshRenderer.SetPropertyBlock(materialPropertyBlock);
            },
            cycles: 2,
            cycleMode: CycleMode.Yoyo
            );

        tweens.Add(tween);
    }

    public static bool IsNull(object testObject)
    {
        return testObject == null;
    }

    public static bool IsNotNull(object testObject)
    {
        return testObject != null;
    }

    public static GameObject GetParentGameObject(GameObject child)
    {
        return child.transform.parent.gameObject;
    }

    public static int GetParentActiveChildCount(Transform parent)
    {
        int count = 0;

        foreach (Transform child in parent)
        {
            if (child.gameObject.activeInHierarchy)
            {
                count++;
            }
        }

        return count;
    }

    #region CONSOLE
    public static void ClearLog()
    {
#if UNITY_EDITOR
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
#endif
    }
    #endregion

    #region COLOR
    public static Color ChangeAlpha(this Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
    #endregion

    private static IEnumerator WaitFor(float amount, Action onCompletedAction)
    {
        float deltaTime = Time.deltaTime;

        WaitForSeconds waitForSeconds = new WaitForSeconds(deltaTime);

        float time = 0;

        while (time < amount)
        {
            time += deltaTime;

            yield return waitForSeconds;
        }

        onCompletedAction?.Invoke();
    }

    public static Vector2 GetScreenSizeWorld(Camera camera)
    {
        float orthoSize = camera.orthographicSize;
        float screenHeight = orthoSize * 2;
        float screenWidth = screenHeight * camera.aspect;

        return new Vector2(screenWidth, screenHeight);
    }

    public static Vector2 GetScreenSizeWorld(Camera camera, float orthographicSize)
    {
        float orthoSize = orthographicSize;
        float screenHeight = orthoSize * 2;
        float screenWidth = screenHeight * camera.aspect;

        return new Vector2(screenWidth, screenHeight);
    }

    #region LEVEL
    public static bool IsLevelInProgress(int level)
    {
        LevelProgressData levelProgressData = DataUtility.Load($"{GameConstants.SCREWS_DATA}_{level}", new LevelProgressData());

        return levelProgressData.ScrewsData != null;
    }
    #endregion

    #region TIME
    public static string ConvertTimeFormat(double totalSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

        return string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}",
                        (int)time.TotalDays,
                        time.Hours,
                        time.Minutes,
                        time.Seconds);
    }

    public static string ToHHMMSS(double totalSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

        return string.Format("{0:D2}:{1:D2}:{2:D2}",
                        time.Hours,
                        time.Minutes,
                        time.Seconds);
    }

    public static DateTime TimestampToDateTime(long unixTimestampSeconds)
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return epoch.AddSeconds(unixTimestampSeconds).ToLocalTime();
    }

    public static long GetCurrentTimestamp()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public static bool IsNewDay(long timestamp)
    {
        // Convert the long timestamp to a DateTime object
        DateTime timestampDate = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;

        DateTime now = DateTime.Now;

        // Compare the year, month, and day only (ignoring time)
        if (now.Year != timestampDate.Year || now.Month != timestampDate.Month || now.Day != timestampDate.Day)
        {
            return true; // It's a new day compared to the timestamp
        }

        return false;
    }

    public static bool IsNewDay(DateTime timestamp)
    {
        DateTime now = DateTime.Now;

        // Compare the year, month, and day only (ignoring time)
        if (now.Year != timestamp.Year || now.Month != timestamp.Month || now.Day != timestamp.Day)
        {
            return true; // It's a new day compared to the timestamp
        }

        return false;
    }

    public static bool IsNewWeek(long timestamp)
    {
        return IsNewWeek(TimestampToDateTime(timestamp));
    }

    public static bool IsNewWeek(DateTime timestamp)
    {
        DateTime now = DateTime.Now;

        Calendar calendar = CultureInfo.CurrentCulture.Calendar;
        CalendarWeekRule weekRule = CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule;
        DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

        int nowWeek = calendar.GetWeekOfYear(now, weekRule, firstDayOfWeek);
        int timestampWeek = calendar.GetWeekOfYear(timestamp, weekRule, firstDayOfWeek);

        // Also check the year to handle week 1 of new year
        if (now.Year != timestamp.Year || nowWeek != timestampWeek)
        {
            return true; // It's a new week compared to the timestamp
        }
        return false;
    }

    public static DateTime RandomBirthday(int startYear, int endYear)
    {
        int year = UnityEngine.Random.Range(startYear, endYear + 1);
        int month = UnityEngine.Random.Range(1, 13);

        int daysInMonth = DateTime.DaysInMonth(year, month);
        int day = UnityEngine.Random.Range(1, daysInMonth + 1);

        return new DateTime(year, month, day);
    }

    public static int GetAge(DateTime birthday, DateTime currentDate)
    {
        int age = currentDate.Year - birthday.Year;

        if (currentDate < birthday.AddYears(age))
            age--;

        return age;
    }
    #endregion

    #region RANDOMIZE
    public static bool RandomizeWithProbability(float probability)
    {
        probability = Mathf.Clamp(probability, 0f, 1f);

        return UnityEngine.Random.Range(0f, 1f) < probability;
    }

    public static GameFaction RandomizeFaction()
    {
        return GameConstants.SCREW_FACTION[UnityEngine.Random.Range(0, GameConstants.SCREW_FACTION.Length)];
    }

    public static T GetRandomValueFromArray<T>(T[] array)
    {
        return array[UnityEngine.Random.Range(0, array.Length)];
    }

    public static T GetRandomEnumValue<T>(int numLastValuesToSkip = 0)
    {
        T[] values = (T[])System.Enum.GetValues(typeof(T));
        return values[UnityEngine.Random.Range(0, values.Length - numLastValuesToSkip)];
    }

    public static List<T> GetRandomEnumValuePool<T>(int maxUniqueValueCount, int numLastValuesToSkip = 0)
    {
        T[] values = (T[])System.Enum.GetValues(typeof(T));

        int availableCount = values.Length - numLastValuesToSkip;

        maxUniqueValueCount = Mathf.Clamp(
            maxUniqueValueCount,
            0,
            availableCount);

        // Copy allowed values
        List<T> pool = values.Take(availableCount).ToList();

        pool.Shuffle();

        return pool.Take(maxUniqueValueCount).ToList();
    }
    #endregion

    public static void SumValueOfDictionary<T>(Dictionary<T, int> dictionary, T key, int value)
    {
        if (dictionary == null)
        {
            dictionary = new Dictionary<T, int>();
        }

        if (dictionary.ContainsKey(key))
        {
            dictionary[key] += value;
        }
        else
        {
            dictionary.Add(key, value);
        }
    }

    public static void AddValueOfDictionary<T1, T2>(Dictionary<T1, List<T2>> dictionary, T1 key, T2 value)
    {
        if (dictionary == null)
        {
            dictionary = new Dictionary<T1, List<T2>>();
        }

        if (dictionary.ContainsKey(key))
        {
            dictionary[key].Add(value);
        }
        else
        {
            dictionary.Add(key, new List<T2>() { value });
        }
    }

    public static Color GetHighContrastComplementary(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);

        // Opposite hue
        h = (h + 0.5f) % 1f;

        // Increase contrast
        s = Mathf.Clamp01(s < 0.5f ? 1f : s);
        v = v > 0.5f ? 0.15f : 1f;

        Color result = Color.HSVToRGB(h, s, v);
        result.a = color.a;

        return result;
    }

    #region Guassian
    // Returns a normally distributed float
    public static float NextGaussian(float mean = 0f, float standardDeviation = 1f)
    {
        float u1 = 1.0f - UnityEngine.Random.value; // avoid 0
        float u2 = 1.0f - UnityEngine.Random.value;

        float randStdNormal =
            Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
            Mathf.Sin(2.0f * Mathf.PI * u2);

        return mean + standardDeviation * randStdNormal;
    }

    // Returns a clamped int between min and max
    public static int GaussianRange(int min, int max)
    {
        float mean = (min + max) / 2f;

        // controls spread
        float stdDev = (max - min) / 6f;

        float value = NextGaussian(mean, stdDev);

        value = Mathf.Clamp(value, min, max);

        return Mathf.RoundToInt(value);
    }
    #endregion
}
