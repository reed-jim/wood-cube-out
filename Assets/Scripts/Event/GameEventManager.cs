using System;
using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager Instance;

    [SerializeField] private IntVariable currentLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);

            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public bool IsSpaceRaceUnlocked()
    {
        return currentLevel.Value >= 40;
    }

    public bool IsSpaceRaceAvailable()
    {
        DayOfWeek dayOfWeek = System.DateTime.Now.DayOfWeek;

        if (!IsSpaceRaceUnlocked())
        {
            return false;
        }

        if (dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
