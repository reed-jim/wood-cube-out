using System;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GoldPassMilestoneItem : MonoBehaviour
{
    #region SHARED
    [SerializeField] private RectTransform container;
    [SerializeField] private GameObject doneState;
    [SerializeField] private TMP_Text passNumberText;
    [SerializeField] private Sprite[] rewardSprites;
    [SerializeField] private Button claimButton;
    #endregion

    #region FREE PASS
    [SerializeField] private TMP_Text freePassRewardQuantity;
    [SerializeField] private Image freePassRewardIcon;
    [SerializeField] private Image freePassDoneIcon;
    [SerializeField] private RectTransform freePassChest;
    [SerializeField] private Button freePassChestButton;
    #endregion

    #region GOLD PASS
    [SerializeField] private TMP_Text goldPassRewardQuantity;
    [SerializeField] private GameObject goldPassLock;
    [SerializeField] private Image goldPassRewardIcon;
    [SerializeField] private Image goldPassDoneIcon;
    [SerializeField] private RectTransform goldPassChest;
    [SerializeField] private Button goldPassChestButton;
    #endregion

    #region CHEST PREVIEW
    [SerializeField] private RectTransform chestPreviewContainer;
    [SerializeField] private CanvasGroup chestPreviewCanvasGroup;
    [SerializeField] private RectTransform[] chestPreviewItems;
    [SerializeField] private TMP_Text[] chestPreviewQuantityTexts;
    #endregion

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private GoldPassDataContainer goldPassDataContainer;
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isChestReward;

    private int _index;

    #region EVENT
    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    #endregion

    private void Awake()
    {
        claimButton.onClick.AddListener(Claim);
        freePassChestButton.onClick.AddListener(ShowPreviewChestFreePass);
        goldPassChestButton.onClick.AddListener(ShowPreviewChestGoldPass);
    }

    void Update()
    {
        CheckClosePreviewChest();
    }

    public void Setup(int index)
    {
        GoldPassMilestoneData goldPassMilestoneData = goldPassDataContainer.goldPassMilestones[index];
        DailyRewardData rewardData = goldPassDataContainer.goldPassRewards[index];

        int numReward = 0;

        int singleRewardQuantity = 0;
        int rewardSpriteIndex = 0;

        if (rewardData.CoinQuantity > 0)
        {
            numReward++;

            singleRewardQuantity = rewardData.CoinQuantity;
            rewardSpriteIndex = 0;
        }

        for (int i = 0; i < rewardData.BoosterQuantities.Length; i++)
        {
            if (rewardData.BoosterQuantities[i] > 0)
            {
                numReward++;

                singleRewardQuantity = rewardData.BoosterQuantities[i];
                rewardSpriteIndex = 1 + i;
            }
        }

        bool isSingleReward = numReward == 1;

        freePassChest.gameObject.SetActive(!isSingleReward);
        goldPassChest.gameObject.SetActive(!isSingleReward);

        freePassRewardIcon.gameObject.SetActive(isSingleReward);
        freePassRewardQuantity.gameObject.SetActive(isSingleReward);

        goldPassRewardIcon.gameObject.SetActive(isSingleReward);
        goldPassRewardQuantity.gameObject.SetActive(isSingleReward);

        freePassRewardQuantity.text = $"{singleRewardQuantity}";
        goldPassRewardQuantity.text = $"{GamePersistentVariable.goldPassMultiplier * singleRewardQuantity}";

        freePassRewardIcon.sprite = rewardSprites[rewardSpriteIndex];
        goldPassRewardIcon.sprite = rewardSprites[rewardSpriteIndex];

        UIUtil.SetSizeKeepRatioX(freePassRewardIcon, 0.5f * container.sizeDelta.y);
        UIUtil.SetSizeKeepRatioX(goldPassRewardIcon, 0.5f * container.sizeDelta.y);

        #region CHEST
        if (goldPassMilestoneData.IsChest)
        {
            freePassChest.gameObject.SetActive(true);
            goldPassChest.gameObject.SetActive(true);

            freePassRewardIcon.gameObject.SetActive(false);
            freePassRewardQuantity.gameObject.SetActive(false);

            goldPassRewardIcon.gameObject.SetActive(false);
            goldPassRewardQuantity.gameObject.SetActive(false);

            chestPreviewQuantityTexts[0].text = $"{rewardData.BoosterQuantities[0]}";
            chestPreviewQuantityTexts[1].text = $"{rewardData.BoosterQuantities[1]}";
            chestPreviewQuantityTexts[2].text = $"{rewardData.BoosterQuantities[2]}";
            chestPreviewQuantityTexts[3].text = $"{rewardData.CoinQuantity}";
        }

        chestPreviewContainer.gameObject.SetActive(false);
        #endregion

        doneState.gameObject.SetActive(goldPassMilestoneData.IsPassed && !goldPassMilestoneData.IsClaimed);
        freePassDoneIcon.gameObject.SetActive(goldPassMilestoneData.IsClaimed);

        claimButton.interactable = goldPassMilestoneData.IsPassed && !goldPassMilestoneData.IsClaimed;

        goldPassLock.SetActive(!GamePersistentVariable.isGoldPassUnlocked);

        _index = index;
    }

    private void Claim()
    {
        int coinQuantity = goldPassDataContainer.goldPassRewards[_index].CoinQuantity;
        int[] boosterQuantites = goldPassDataContainer.goldPassRewards[_index].BoosterQuantities;

        if (goldPassDataContainer.goldPassData.isGoldPass)
        {
            coinQuantity *= GamePersistentVariable.goldPassMultiplier;

            for (int i = 0; i < boosterQuantites.Length; i++)
            {
                boosterQuantites[i] *= GamePersistentVariable.goldPassMultiplier;
            }
        }

        userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
        userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
        userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

        userResourcesObserver.ChangeCoin(coinQuantity);

        showResourcesEarnPopupEvent?.Invoke(coinQuantity, boosterQuantites, false);

        goldPassDataContainer.goldPassMilestones[_index].IsClaimed = true;

        Setup(_index);
    }

    private void ShowPreviewChestFreePass()
    {
        DailyRewardData rewardData = goldPassDataContainer.goldPassRewards[_index];

        chestPreviewContainer.position = chestPreviewContainer.position.ChangeX(freePassChest.position.x);

        chestPreviewQuantityTexts[0].text = $"{rewardData.BoosterQuantities[0]}";
        chestPreviewQuantityTexts[1].text = $"{rewardData.BoosterQuantities[1]}";
        chestPreviewQuantityTexts[2].text = $"{rewardData.BoosterQuantities[2]}";
        chestPreviewQuantityTexts[3].text = $"{rewardData.CoinQuantity}";

        ShowPreviewChest(true);
    }

    private void ShowPreviewChestGoldPass()
    {
        DailyRewardData rewardData = goldPassDataContainer.goldPassRewards[_index];

        chestPreviewContainer.position = chestPreviewContainer.position.ChangeX(goldPassChest.position.x);

        chestPreviewQuantityTexts[0].text = $"{GamePersistentVariable.goldPassMultiplier * rewardData.BoosterQuantities[0]}";
        chestPreviewQuantityTexts[1].text = $"{GamePersistentVariable.goldPassMultiplier * rewardData.BoosterQuantities[1]}";
        chestPreviewQuantityTexts[2].text = $"{GamePersistentVariable.goldPassMultiplier * rewardData.BoosterQuantities[2]}";
        chestPreviewQuantityTexts[3].text = $"{GamePersistentVariable.goldPassMultiplier * rewardData.CoinQuantity}";

        ShowPreviewChest(true);
    }

    private void ShowPreviewChest(bool isShow)
    {
        if (chestPreviewContainer.gameObject.activeInHierarchy == isShow)
        {
            return;
        }

        if (isShow)
        {
            chestPreviewContainer.gameObject.SetActive(true);

            chestPreviewCanvasGroup.alpha = 0;

            Tween.Alpha(chestPreviewCanvasGroup, 1, duration: 0.3f).OnComplete(() =>
            {

            });
        }
        else
        {
            Tween.Alpha(chestPreviewCanvasGroup, 0, duration: 0.3f).OnComplete(() =>
            {
                chestPreviewContainer.gameObject.SetActive(false);
            });
        }
    }

    private void CheckClosePreviewChest()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!chestPreviewContainer.gameObject.activeInHierarchy)
            {
                return;
            }

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var raycastResults = new System.Collections.Generic.List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            bool clickedOutside = true;

            bool isFound = false;

            foreach (var result in raycastResults)
            {
                if (result.gameObject == chestPreviewContainer.gameObject)
                {
                    clickedOutside = false;

                    break;
                }

                if (isFound)
                {
                    break;
                }
            }

            if (clickedOutside)
            {
                ShowPreviewChest(false);
            }
        }
    }
}
