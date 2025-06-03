using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager Instance;

    [SerializeField] private IntVariable currentLevel;

    public static event Action suggestCoinChallengeEvent;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        SuggestCoinChallenge();
    }

    public bool IsSpaceRaceUnlocked()
    {
        return currentLevel.Value >= 40;
    }

    public bool IsCoinChallengeUnlocked()
    {
        // DayOfWeek dayOfWeek = System.DateTime.Now.DayOfWeek;

        // if (dayOfWeek != DayOfWeek.Monday && dayOfWeek != DayOfWeek.Tuesday)
        // {
        //     return false;
        // }

        return currentLevel.Value >= 30;
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

    private void SuggestCoinChallenge()
    {
        currentLevel.Load();

        Debug.Log("SAFERIO " + (currentLevel.Value) + "/" + DataUtility.Load("Is Coin Challenge Suggested", false));

        if (currentLevel.Value < 30)
        {
            return;
        }

        bool isSuggested = DataUtility.Load("Is Coin Challenge Suggested", false);

        if (!isSuggested)
        {
            Tween.Delay(1).OnComplete(() =>
            {
                suggestCoinChallengeEvent?.Invoke();

                DataUtility.Save("Is Coin Challenge Suggested", true);
            });
        }
    }
}
