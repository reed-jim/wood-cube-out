using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPlayerPrefs
{
    public GPlayerPrefs() { }


    public static bool isCanChangeVersion = true;

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    public static float GetFloat(string key, float defaultValue)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static float GetFloat(string key)
    {
        return PlayerPrefs.GetFloat(key);
    }

    public static int GetInt(string key, int defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public static int GetInt(string key)
    {
        return PlayerPrefs.GetInt(key);
    }

    public static string GetString(string key, string defaultValue)
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public static string GetString(string key)
    {
        return PlayerPrefs.GetString(key);
    }

    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void SetSyncTime()
    {
        PlayerPrefs.SetString(StringHelper.LAST_TIME_SYNC, DateTime.Now.Ticks.ToString());
        Save();
    }

    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        if (isCanChangeVersion)
            AddVersionData(1);
    }

    public static void SetInt(string key, int value)
    {
        if (key == StringHelper.VERSION_DATA)
            Debug.Log("value " + value);
        PlayerPrefs.SetInt(key, value);
        if (isCanChangeVersion)
            AddVersionData(1);
    }

    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        if (isCanChangeVersion)
            AddVersionData(1);
    }

    private static void AddVersionData(int add)
    {
        long verTemp = UseProfile.VersionData;
        UseProfile.VersionData = verTemp + add;
    }    
}
