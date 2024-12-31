using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Saferio/Screw Away/UserResourcesObserver")]
public class UserResourcesObserver : ScriptableObject
{
    [SerializeField] private UserResources userResources = new UserResources();

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
}


public class UserResources
{
    [SerializeField] private float coinQuantity;
    [SerializeField] private int[] boosterQuantities;

    public float CoinQuantity
    {
        get => coinQuantity;
        set
        {
            coinQuantity = value;
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
