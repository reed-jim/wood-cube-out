using System;
using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager Instance;

    [SerializeField] private IntVariable currentLevel;

    private Coroutine _openingChestsCoroutine;

    public static event Action suggestCoinChallengeEvent;
    public static event Action openStarChestPopupEvent;
    public static event Action openLevelChestPopupEvent;
    public static event Action openCoinChallengePopupEvent;

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

        StarChestProgressBar.openChestEvent += StartOpenChests;
        LevelChestProgressBar.openChestEvent += StartOpenChests;
        CoinChallengeProgressBar.openChestEvent += StartOpenChests;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        StarChestProgressBar.openChestEvent -= StartOpenChests;
        LevelChestProgressBar.openChestEvent -= StartOpenChests;
        CoinChallengeProgressBar.openChestEvent -= StartOpenChests;
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

    public bool IsShowCoinChallengeButton()
    {
        DayOfWeek dayOfWeek = System.DateTime.Now.DayOfWeek;

        if (dayOfWeek != DayOfWeek.Monday && dayOfWeek != DayOfWeek.Tuesday)
        {
            return false;
        }

        currentLevel.Load();

        return currentLevel.Value >= 12;
    }

    public bool IsCoinChallengeUnlocked()
    {
        // DayOfWeek dayOfWeek = System.DateTime.Now.DayOfWeek;

        // if (dayOfWeek != DayOfWeek.Monday && dayOfWeek != DayOfWeek.Tuesday)
        // {
        //     return false;
        // }

        return currentLevel.Value >= 12;
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
        if (currentLevel.ValueUsingPureJson < 30)
        {
            return;
        }

        bool isSuggested = DataUtility.Load("Is Coin Challenge Suggested", false);

        if (!isSuggested)
        {
            GamePersistentVariable.isClaimedLevelChest = false;

            StartCoroutine(PendingSuggestCoinChallenge());
        }
    }

    private IEnumerator PendingSuggestCoinChallenge()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        while (!GamePersistentVariable.isLoadingComplete)
        {
            yield return waitForSeconds;
        }

        while (!GamePersistentVariable.isClaimedLevelChest)
        {
            yield return waitForSeconds;
        }

        yield return new WaitForSeconds(0.8f);

        suggestCoinChallengeEvent?.Invoke();

        DataUtility.Save("Is Coin Challenge Suggested", true);
    }

    private void StartOpenChests()
    {
        if (_openingChestsCoroutine == null)
        {
            _openingChestsCoroutine = StartCoroutine(OpeningChests());
        }
    }

    private IEnumerator OpeningChests()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        string nextChest;

        while (GamePersistentVariable.chestQueue.Count > 0)
        {
            if (!GamePersistentVariable.isChestOpenning)
            {
                nextChest = GamePersistentVariable.chestQueue.Dequeue();

                Debug.Log("SAFERIO " + nextChest);

                if (nextChest == "Star Chest")
                {
                    openStarChestPopupEvent?.Invoke();
                }
                else if (nextChest == "Level Chest")
                {
                    openLevelChestPopupEvent?.Invoke();
                }
                else if (nextChest == "Coin Challenge")
                {
                    openCoinChallengePopupEvent?.Invoke();
                }

                GamePersistentVariable.isChestOpenning = true;
            }

            yield return waitForSeconds;
        }
    }
}
