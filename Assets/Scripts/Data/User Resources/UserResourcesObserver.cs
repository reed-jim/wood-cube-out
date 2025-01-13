using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Saferio/Screw Away/UserResourcesObserver")]
public class UserResourcesObserver : ScriptableObject
{
    [SerializeField] private UserResources userResources;

    public UserResources UserResources
    {
        get => userResources;
        set
        {
            userResources = value;

            Save();
        }
    }

    public void Save()
    {
        DataUtility.SaveAsync(GameConstants.USER_RESOURCES, userResources);
    }

    public void Load()
    {
        UserResources defaultUserResources = new UserResources();

        defaultUserResources.BoosterQuantities = new int[3];

        userResources = DataUtility.Load(GameConstants.USER_RESOURCES, defaultUserResources);
    }

    public void ConsumeBooster(int boosterIndex)
    {
        userResources.BoosterQuantities[boosterIndex]--;
    }
}

[Serializable]
public class UserResources
{
    [SerializeField] private float coinQuantity;
    [SerializeField] private float diamondQuantity;
    [SerializeField] private int[] boosterQuantities;

    public float CoinQuantity
    {
        get => coinQuantity;
        set
        {
            coinQuantity = value;
        }
    }

    public float DiamondQuantity
    {
        get => diamondQuantity;
        set
        {
            diamondQuantity = value;
        }
    }

    public int[] BoosterQuantities
    {
        get => boosterQuantities;
        set
        {
            boosterQuantities = value;
        }
    }
}
