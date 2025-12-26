using System;
using NamPhuThuy.Common;
using UnityEngine;
using static GameEnum;

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
           
        }
    }

    public void Save()
    {
        DataUtility.SaveAsync(GameConstants.USER_RESOURCES, userResources);
    }

    public void Load()
    {
        UserResources defaultUserResources = new UserResources();

        defaultUserResources.CoinQuantity = 500;
        defaultUserResources.BoosterQuantities = new int[3];

        /*for (int i = 0; i < defaultUserResources.BoosterQuantities.Length; i++)
        {
            defaultUserResources.BoosterQuantities[i] = 2;
        }*/
        
        for (int i = 0; i < defaultUserResources.BoosterTutReceived.Length; i++)
        {
            defaultUserResources.BoosterTutReceived[i] = false;
        }

        userResources = DataUtility.Load(GameConstants.USER_RESOURCES, defaultUserResources);
    }

    public void ChangeCoin(float value)
    {
        Load();

        userResources.CoinQuantity += value;
        userResources.CoinQuantity = Mathf.Max(userResources.CoinQuantity, 0);

        Save();
    }

    public void ChangeBoosterQuantity(int boosterIndex, int value)
    {
        DebugLogger.Log();
        Load();

        userResources.BoosterQuantities[boosterIndex] += value;
        userResources.BoosterQuantities[boosterIndex] = Mathf.Max(userResources.BoosterQuantities[boosterIndex], 0);

        // updatedUserResources.BoosterQuantities = updatedBoosterQuantities;
        // userResources = updatedUserResources;

        Save();
    }

    public void ConsumeBooster(BoosterType boosterType)
    {
        Load();

        if (userResources.BoosterQuantities[(int)boosterType] > 0)
        {
            userResources.BoosterQuantities[(int)boosterType]--;

            Save();
        }
    }
    
    public void MarkBoosterTutReceived(int boosterIndex)
    {
        Load();

        if (userResources.BoosterTutReceived == null)
        {
            userResources.BoosterTutReceived = new bool[3];
        }

        userResources.BoosterTutReceived[boosterIndex] = true;

        Save();
    }
    
    public bool IsBoosterTutReceived(int boosterIndex)
    {
        Load();

        if (userResources.BoosterTutReceived == null)
        {
            userResources.BoosterTutReceived = new bool[3];
        }

        bool res = userResources.BoosterTutReceived[boosterIndex];
        
        return res;
    }
}

[Serializable]
public class UserResources
{
    [SerializeField] private float coinQuantity;
    [SerializeField] private int[] boosterQuantities;
    [SerializeField] private bool[] boosterTutReceived;
    
    public bool[] BoosterTutReceived
    {
        get => boosterTutReceived;
        set
        {
            boosterTutReceived = value;
        }
    }

    public float CoinQuantity
    {
        get => coinQuantity;
        set
        {
            coinQuantity = value;
            // DebugLogger.Log(message: $"currentCoin: {coinQuantity}");
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
    

    public UserResources()
    {
        boosterQuantities = new int[3];
        boosterTutReceived = new bool[3];
    }
}
