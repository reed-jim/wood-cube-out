using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeeklyChallengeUIItem : MonoBehaviour, ISaferioScrollViewItem
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text orderText;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TMP_Text rewardQuantity;
    [SerializeField] private GameObject lockObject;
    [SerializeField] private GameObject doneObject;
    [SerializeField] private GameObject activeState;
    [SerializeField] private GameObject glow;
    [SerializeField] private Button claimButton;

    [SerializeField] private Sprite[] rewardSprites;

    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private DailyRewardDataContainer weeklyChallengeDataContainer;

    private int _index;

    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;

    void Awake()
    {
        claimButton.onClick.AddListener(Claim);
    }

    public bool IsValidAtIndex(int index)
    {
        return index >= 0 && index < 30;
    }

    public void Refresh(int index)
    {
        WeeklyChallengeData weeklyChallengeData = GamePersistentVariable.weeklyChallengeData;

        orderText.text = $"{index + 1}";

        if (index <= weeklyChallengeData.CurrentChallengeIndex)
        {
            lockObject.SetActive(false);
        }
        else
        {
            lockObject.SetActive(true);
        }

        if (weeklyChallengeData.Items[index].IsClaimed)
        {
            doneObject.SetActive(true);
            activeState.SetActive(false);
            glow.SetActive(false);

            canvasGroup.interactable = false;
            canvasGroup.alpha = 0.5f;
        }
        else
        {
            doneObject.SetActive(false);

            if (index < weeklyChallengeData.CurrentChallengeIndex)
            {
                activeState.SetActive(true);
                glow.SetActive(true);

                canvasGroup.interactable = true;
            }
            else
            {
                activeState.SetActive(false);
                glow.SetActive(false);

                canvasGroup.interactable = false;
            }

            canvasGroup.alpha = 1f;
        }

        try
        {
            int coinQuantity = weeklyChallengeDataContainer.Items[index].CoinQuantity;
            int[] boosterQuantites = weeklyChallengeDataContainer.Items[index].BoosterQuantities;

            if (coinQuantity > 0)
            {
                rewardIcon.sprite = rewardSprites[0];
                rewardQuantity.text = $"{coinQuantity}";
            }
            else
            {
                for (int i = 0; i < boosterQuantites.Length; i++)
                {
                    if (boosterQuantites[i] > 0)
                    {
                        rewardIcon.sprite = rewardSprites[i + 1];
                        rewardQuantity.text = $"{boosterQuantites[i]}";

                        break;
                    }
                }
            }

            UIUtil.SetSizeKeepRatioY(rewardIcon, rewardIcon.rectTransform.sizeDelta.x);
        }
        catch (System.Exception)
        {

        }
    }

    public void Setup(int index, RectTransform parent)
    {
        Refresh(index);

        _index = index;
    }

    public void Claim()
    {
        int coinQuantity = weeklyChallengeDataContainer.Items[_index].CoinQuantity;
        int[] boosterQuantites = weeklyChallengeDataContainer.Items[_index].BoosterQuantities;

        userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
        userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
        userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

        userResourcesObserver.ChangeCoin(coinQuantity);

        showResourcesEarnPopupEvent(coinQuantity, boosterQuantites, false);

        GamePersistentVariable.weeklyChallengeData.Items[_index].IsClaimed = true;

        DataUtility.Save(GameConstants.WEEKLY_CHALLENGE_DATA, GamePersistentVariable.weeklyChallengeData);

        Refresh(_index);
    }
}
