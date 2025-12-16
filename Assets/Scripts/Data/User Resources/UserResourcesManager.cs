using System;
using UnityEngine;

public class UserResourcesManager : MonoBehaviour
{
    public static UserResourcesManager Instance;

    [SerializeField] private UserResourcesObserver userResourcesObserver;

    #region EVENT
    public static event Action<float> updateCoinTextEvent;
    #endregion

    #region MonoBehaviour Callbacks
    
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

        RewardCollectingUI.addCoinEvent += AddCoin;
    }

    private void OnDestroy()
    {
        RewardCollectingUI.addCoinEvent -= AddCoin;
    }
    
    #endregion

    private void AddCoin(float value)
    {
        // already change coin before
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
