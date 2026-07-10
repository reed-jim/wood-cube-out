using UnityEngine;
using System;

public class UnbiasedTime : MonoBehaviour
{
    private static UnbiasedTime instance;

    public static UnbiasedTime Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject g = new GameObject("UnbiasedTimeSingleton");
                instance = g.AddComponent<UnbiasedTime>();
                DontDestroyOnLoad(g);
            }
            return instance;
        }
    }

    // Kept for compatibility with existing code.
    [HideInInspector]
    public long timeOffset = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        timeOffset = 0;
    }

    /// <summary>
    /// Returns the current device time.
    /// </summary>
    public DateTime Now()
    {
#if TESTER
        return DateTime.Now;
#else
        return DateTime.Now;
#endif
    }

    /// <summary>
    /// No-op. Kept for API compatibility.
    /// </summary>
    public void UpdateTimeOffset()
    {
        timeOffset = 0;
    }

    /// <summary>
    /// Always returns true because only system time is used.
    /// </summary>
    public bool IsUsingSystemTime()
    {
        return true;
    }
}