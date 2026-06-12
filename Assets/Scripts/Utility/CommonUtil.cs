using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Dreamteck.Utilities;
using PrimeTween;
using UnityEngine;
using static GameEnum;

public static class CommonUtil
{

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
    #endregion

    public static void StopTween(Tween tween)
    {
        if (tween.isAlive)
        {
            tween.Stop();
        }
    }

    public static void StopAllTweens(List<Tween> tweens)
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

    public static void StopAllSequences(List<Sequence> tweens)
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
        List<Tween> tweens
    )
    {
        Tween tween = Tween.Custom(
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
