using System;
using UnityEngine;

public class UserResourcesManager : MonoBehaviour
{
    public static UserResourcesManager Instance;

    [SerializeField] private UserResourcesObserver userResourcesObserver;

    #region EVENT
    public static event Action<float> updateCoinTextEvent;
    #endregion

    void Awake()
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

        RewardCollectingUI.addCoinEvent += AddCoin;
    }

    void OnDestroy()
    {
        RewardCollectingUI.addCoinEvent -= AddCoin;
    }

    private void AddCoin(float value)
    {
        // userResourcesObserver.ChangeCoin(value);

        userResourcesObserver.Load();

        updateCoinTextEvent?.Invoke(userResourcesObserver.UserResources.CoinQuantity);
    }

    public void UpdateCoinTextAnimation()
    {
        userResourcesObserver.Load();

        updateCoinTextEvent?.Invoke(userResourcesObserver.UserResources.CoinQuantity - GamePersistentVariable.spawnedRewardItemsData.GetValueLeft());
    }
}
