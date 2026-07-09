using System;
using UnityEngine.Events;

public class AppOpenAdManager
{
    private static AppOpenAdManager instance;

    public static AppOpenAdManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AppOpenAdManager();
            }
            return instance;
        }
    }

    public bool IsAdAvailable => false;

    public void LoadAd(UnityAction actionLoadDone = null)
    {
        actionLoadDone?.Invoke();
    }

    public void ShowAdIfAvailable()
    {
    }
}