using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Saferio/Screw Away/UserResourcesObserver")]
public class UserResourcesObserver : ScriptableObject
{
    [SerializeField] private UserResources userResources;

    public UserResources UserResources
    {
        get
        {
            if (userResources == null)
            {
                userResources = new UserResources();
            }
            userResources.CoinQuantity = UseProfile.Coin;
            
            if (userResources.BoosterQuantities == null || userResources.BoosterQuantities.Length < 5)
            {
                int[] temp = userResources.BoosterQuantities;
                Array.Resize(ref temp, 5);
                userResources.BoosterQuantities = temp;
            }

            for (int i = 0; i < userResources.BoosterQuantities.Length; i++)
            {
                GiftType giftType = GameEnum.MapBoosterToGiftType((GameEnum.BoosterType)i);
                if (giftType != GiftType.None)
                {
                    userResources.BoosterQuantities[i] = UseProfile.GetNumItem(giftType);
                }
            }
            return userResources;
        }
        set
        {
            userResources = value;
            if (userResources != null)
            {
                UseProfile.Coin = (int)userResources.CoinQuantity;
                
                for (int i = 0; i < userResources.BoosterQuantities.Length; i++)
                {
                    GiftType giftType = GameEnum.MapBoosterToGiftType((GameEnum.BoosterType)i);
                    if (giftType != GiftType.None)
                    {
                        UseProfile.SetItem(giftType, userResources.BoosterQuantities[i]);
                    }
                }
            }
        }
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (userResources == null)
        {
            userResources = new UserResources();
        }
        userResources.CoinQuantity = UseProfile.Coin;
        
        if (userResources.BoosterQuantities == null || userResources.BoosterQuantities.Length < 5)
        {
            int[] temp = userResources.BoosterQuantities;
            Array.Resize(ref temp, 5);
            userResources.BoosterQuantities = temp;
        }

        for (int i = 0; i < userResources.BoosterQuantities.Length; i++)
        {
            GiftType giftType = GameEnum.MapBoosterToGiftType((GameEnum.BoosterType)i);
            if (giftType != GiftType.None)
            {
                userResources.BoosterQuantities[i] = UseProfile.GetNumItem(giftType);
            }
        }
    }

    public void ChangeCoin(float value)
    {
        UseProfile.Coin += (int)value;
        if (userResources != null)
        {
            userResources.CoinQuantity = UseProfile.Coin;
        }
        Save();
    }

    public void ChangeBoosterQuantity(int boosterIndex, int value)
    {
        GiftType giftType = GameEnum.MapBoosterToGiftType((GameEnum.BoosterType)boosterIndex);
        if (giftType != GiftType.None)
        {
            UseProfile.AddItem(giftType, value);
        }
        Load();
        Save();
    }

    public void ConsumeBooster(int boosterIndex)
    {
        GiftType giftType = GameEnum.MapBoosterToGiftType((GameEnum.BoosterType)boosterIndex);
        if (giftType != GiftType.None)
        {
            if (GameController.Instance != null && GameController.Instance.dataContain != null && GameController.Instance.dataContain.giftDatabase != null)
            {
                GameController.Instance.dataContain.giftDatabase.Claim(giftType, -1, Reason.in_campain_level);
            }
            else
            {
                UseProfile.AddItem(giftType, -1);
            }
        }
        Load();
        Save();
    }
}

[Serializable]
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

    public UserResources()
    {
        boosterQuantities = new int[5];
    }
}
