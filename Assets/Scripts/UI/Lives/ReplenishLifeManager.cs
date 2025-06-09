using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameEnum;

public class ReplenishLifeManager : MonoBehaviour
{
    private int MaxLives = 5;
    private int ReplenishTime = 1800;

    private LivesData _livesData;
    private bool _isFetchRemoteConfig;

    public static event Action<string> updateLivesReplenishTimeEvent;
    public static event Action<int> updateLivesNumberEvent;
    public static event Action<ScreenRoute> switchRouteEvent;

    void Awake()
    {
        ReturnHomePopup.changeLivesNumberEvent += ChangeLivesNumber;
        LivesShopPopup.changeLivesNumberEvent += ChangeLivesNumber;
        ReplayPopup.changeLivesNumberEvent += ChangeLivesNumber;
        LosePopup.changeLivesNumberEvent += ChangeLivesNumber;
        SceneManager.sceneLoaded += OnSceneChanged;
        RemoteConfigController.remoteConfigReadyEvent += OnRemoteConfigReady;
    }

    void Start()
    {
        // await WaitForFirebaseReadyAsync();

        LivesData defaultLivesData = new LivesData();

        MaxLives = 5;
        ReplenishTime = 60 * 20;

        defaultLivesData.CurrentLives = MaxLives;
        defaultLivesData.LastReplenishTime = DateTime.Now;

        _livesData = DataUtility.Load(GameConstants.USER_LIVES_DATA, defaultLivesData);

        DataUtility.Save(GameConstants.USER_LIVES_DATA, _livesData);

        GamePersistentVariable.livesData = _livesData;

        StartCoroutine(Counting());

        updateLivesNumberEvent?.Invoke(_livesData.CurrentLives);
    }

    private void OnRemoteConfigReady()
    {
        MaxLives = GamePersistentVariable.livesConfig.maxLive;
        ReplenishTime = GamePersistentVariable.livesConfig.secondReplenishLive;
    }

    void OnDestroy()
    {
        ReturnHomePopup.changeLivesNumberEvent -= ChangeLivesNumber;
        LivesShopPopup.changeLivesNumberEvent -= ChangeLivesNumber;
        ReplayPopup.changeLivesNumberEvent -= ChangeLivesNumber;
        LosePopup.changeLivesNumberEvent -= ChangeLivesNumber;
        SceneManager.sceneLoaded -= OnSceneChanged;
        RemoteConfigController.remoteConfigReadyEvent -= OnRemoteConfigReady;
    }

    void OnSceneChanged(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (_livesData == null)
        {
            return;
        }

        updateLivesNumberEvent?.Invoke(_livesData.CurrentLives);
    }

    private IEnumerator Counting()
    {
        WaitForSeconds waitOneSecond = new WaitForSeconds(1);

        while (true)
        {
            if (_livesData.CurrentLives < MaxLives)
            {
                UpdateRemainingReplenishLife();
            }
            else
            {

            }

            yield return waitOneSecond;
        }
    }

    private void UpdateRemainingReplenishLife()
    {
        if (!_isFetchRemoteConfig)
        {
            if (GamePersistentVariable.isFirebaseRemoteConfigLoaded)
            {
                MaxLives = GamePersistentVariable.livesConfig.maxLive;
                ReplenishTime = GamePersistentVariable.livesConfig.secondReplenishLive;

                _isFetchRemoteConfig = true;
            }
        }

        TimeSpan timeSinceLastReplenish = DateTime.Now - _livesData.LastReplenishTime;

        double remainingTime = ReplenishTime - timeSinceLastReplenish.TotalSeconds;

        int totalLivesToReplenish = (int)(timeSinceLastReplenish.TotalSeconds / ReplenishTime);

        totalLivesToReplenish = Mathf.Min(totalLivesToReplenish, MaxLives - _livesData.CurrentLives);

        if (remainingTime <= 0)
        {
            _livesData.CurrentLives += totalLivesToReplenish;
            _livesData.LastReplenishTime = DateTime.Now;

            updateLivesNumberEvent?.Invoke(_livesData.CurrentLives);
        }
        else
        {
            updateLivesReplenishTimeEvent?.Invoke(ConvertTimeFormat(remainingTime));
        }

        DataUtility.Save(GameConstants.USER_LIVES_DATA, _livesData);

        GamePersistentVariable.livesData = _livesData;
    }

    private void Replenish(int numLives)
    {

    }

    private void ChangeLivesNumber(int value)
    {
        if (_livesData.CurrentLives == MaxLives)
        {
            _livesData.LastReplenishTime = DateTime.Now;
        }

        _livesData.CurrentLives += value;

        DataUtility.Save(GameConstants.USER_LIVES_DATA, _livesData);

        GamePersistentVariable.livesData = _livesData;

        UpdateRemainingReplenishLife();

        updateLivesNumberEvent?.Invoke(_livesData.CurrentLives);
    }

    #region UTIL
    private string ConvertTimeFormat(double time)
    {
        return $"{TimeSpan.FromSeconds(time).ToString(@"mm\:ss")}";
    }
    #endregion
}

[System.Serializable]
public class LivesData
{
    private int currentLives;
    private DateTime lastReplenishTime;

    public int CurrentLives
    {
        get => currentLives;
        set => currentLives = value;
    }

    public DateTime LastReplenishTime
    {
        get => lastReplenishTime;
        set => lastReplenishTime = value;
    }
}
