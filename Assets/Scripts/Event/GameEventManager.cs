using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager Instance;

    [SerializeField] private IntVariable currentLevel;

    private List<Tween> _tweens;
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

        _tweens = new List<Tween>();
    }

    void OnDestroy()
    {
        StarChestProgressBar.openChestEvent -= StartOpenChests;
        LevelChestProgressBar.openChestEvent -= StartOpenChests;
        CoinChallengeProgressBar.openChestEvent -= StartOpenChests;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        SuggestCoinChallenge();

        if (scene.name == "Menu" && GamePersistentVariable.hasPlayerReachedGameplay)
        {
            try
            {
                AvoidClickScreen.Instance.Show();

                StartCoroutine(WaitHideAvoidClickScreen());
            }
            catch (System.Exception)
            {

            }
        }
    }

    private IEnumerator WaitHideAvoidClickScreen()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        while (GamePersistentVariable.isMenuLoading)
        {
            yield return waitForSeconds;
        }

        yield return new WaitForSeconds(1.8f);

        AvoidClickScreen.Instance.Hide();
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
        if (_openingChestsCoroutine != null)
        {
            StopCoroutine(_openingChestsCoroutine);
        }

        _openingChestsCoroutine = StartCoroutine(OpeningChests());
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

                AdmobAdsMax.Instance.ResetAdsCooldown();

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

        _openingChestsCoroutine = null;
    }
}
