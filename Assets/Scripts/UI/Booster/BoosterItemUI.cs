using System;
using System.Linq;
using System.Threading.Tasks;
using Lean.Localization;
using NamPhuThuy.GamePlay;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class BoosterItemUI : MonoBehaviour
{
    [SerializeField] private RectTransform quantityTextContainer;
    [SerializeField] private RectTransform quantityTextRT;
    [SerializeField] private RectTransform addButtonRT;
    [SerializeField] private SkeletonGraphic skeletonGraphic;

    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string pressAnimation;

    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button pressButton;
    [SerializeField] private Button addButton;

    [Header("Lock State")]
    [SerializeField] private CanvasGroup lockCanvasGroup;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedLevelToUnlock;

    [Header("CUSTOMIZE")]
    [SerializeField] private int boosterIndex;
    [SerializeField] private BoosterType boosterType;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    public static event Action<BoosterType> showBuyBoosterPopupEvent;

    void Awake()
    {
        BuyBoosterPopup.updateBoosterQuantityEvent += UpdateQuantityText;
        BoosterUI.updateBoosterQuantityEvent += UpdateQuantityText;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShow;
        LevelLoader.startLevelEvent += Setup;

        addButton.onClick.AddListener(ShowBuyBoosterPopup);
        // pressButton.onClick.AddListener(Press);

        Setup();
    }

    void OnDestroy()
    {
        BuyBoosterPopup.updateBoosterQuantityEvent -= UpdateQuantityText;
        BoosterUI.updateBoosterQuantityEvent -= UpdateQuantityText;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourceEarnPopupShow;
        LevelLoader.startLevelEvent -= Setup;
    }

    private void Setup()
    {
        userResourcesObserver.Load();

        int quantity = userResourcesObserver.UserResources.BoosterQuantities[boosterIndex];

        bool isUnlocked = currentLevel.Value >= GameConstants.GetLevelToUnlockBooster(boosterType);
        bool isShowQuantityText = quantity > 0 && isUnlocked;
        bool isShowAddButton = quantity <= 0 && isUnlocked;

        quantityTextContainer.gameObject.SetActive(isShowQuantityText);
        addButtonRT.gameObject.SetActive(isShowAddButton);

        if (quantity > 0)
        {
            quantityText.text = $"{quantity}";
        }
        else
        {

        }

        if (currentLevel.Value < GameConstants.GetLevelToUnlockBooster(boosterType))
        {
            lockCanvasGroup.gameObject.SetActive(true);

            localizedLevelToUnlock.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{GameConstants.GetLevelToUnlockBooster(boosterType)}");
        }
        else
        {
            lockCanvasGroup.gameObject.SetActive(false);
        }
    }

    private void Press()
    {
        skeletonGraphic.AnimationState.Complete += OnAnimationComplete;

        skeletonGraphic.AnimationState.SetAnimation(0, pressAnimation, false);

        void OnAnimationComplete(TrackEntry trackEntry)
        {
            skeletonGraphic.AnimationState.Complete -= OnAnimationComplete;

            if (trackEntry.Animation.Name == pressAnimation)
            {
                skeletonGraphic.AnimationState.SetAnimation(0, idleAnimation, false);
            }
        }
    }

    private void ShowBuyBoosterPopup()
    {
        showBuyBoosterPopupEvent?.Invoke((BoosterType)boosterIndex);
    }

    private void UpdateQuantityText(int boosterIndex)
    {
        if (boosterIndex == this.boosterIndex)
        {
            userResourcesObserver.Load();

            int quantity = userResourcesObserver.UserResources.BoosterQuantities[boosterIndex];

            bool isUnlocked = currentLevel.Value >= GameConstants.GetLevelToUnlockBooster(boosterType);
            bool isShowQuantityText = quantity > 0 && isUnlocked;
            bool isShowAddButton = quantity <= 0 && isUnlocked;

            quantityTextContainer.gameObject.SetActive(isShowQuantityText);
            addButtonRT.gameObject.SetActive(isShowAddButton);

            if (quantity > 0)
            {
                quantityText.text = $"{quantity}";
            }
        }
    }

    private void OnResourceEarnPopupShow(bool isShow)
    {
        if (!isShow)
        {
            userResourcesObserver.Load();

            int quantity = userResourcesObserver.UserResources.BoosterQuantities[boosterIndex];

            bool isUnlocked = currentLevel.Value >= GameConstants.GetLevelToUnlockBooster(boosterType);
            bool isShowQuantityText = quantity > 0 && isUnlocked;
            bool isShowAddButton = quantity <= 0 && isUnlocked;

            quantityTextContainer.gameObject.SetActive(isShowQuantityText);
            addButtonRT.gameObject.SetActive(isShowAddButton);

            if (quantity > 0)
            {
                quantityText.text = $"{quantity}";
            }
        }
    }
}
