using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using PrimeTween;
using UnityEngine;

public static class CommonUtil
{
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
    public static Color ChangeAlpha(Color color, float alpha)
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
    #endregion
}
