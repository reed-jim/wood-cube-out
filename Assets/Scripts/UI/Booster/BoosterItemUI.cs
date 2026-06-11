using System;
using System.Linq;
using System.Threading.Tasks;
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

    [Header("CUSTOMIZE")]
    [SerializeField] private int boosterIndex;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    public static event Action<BoosterType> showBuyBoosterPopupEvent;

    void Awake()
    {
        BuyBoosterPopup.updateBoosterQuantityEvent += UpdateQuantityText;
        BoosterUI.updateBoosterQuantityEvent += UpdateQuantityText;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShow;

        addButton.onClick.AddListener(ShowBuyBoosterPopup);
        pressButton.onClick.AddListener(Press);

        Setup();
    }

    void OnDestroy()
    {
        BuyBoosterPopup.updateBoosterQuantityEvent -= UpdateQuantityText;
        BoosterUI.updateBoosterQuantityEvent -= UpdateQuantityText;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourceEarnPopupShow;
    }

    private void Setup()
    {
        userResourcesObserver.Load();

        int quantity = userResourcesObserver.UserResources.BoosterQuantities[boosterIndex];

        if (quantity > 0)
        {
            quantityTextContainer.gameObject.SetActive(true);
            addButtonRT.gameObject.SetActive(false);

            quantityText.text = $"{quantity}";
        }
        else
        {
            quantityTextContainer.gameObject.SetActive(false);
            addButtonRT.gameObject.SetActive(true);
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

            if (quantity > 0)
            {
                quantityTextContainer.gameObject.SetActive(true);
                addButtonRT.gameObject.SetActive(false);

                quantityText.text = $"{quantity}";
            }
            else
            {
                quantityTextContainer.gameObject.SetActive(false);
                addButtonRT.gameObject.SetActive(true);
            }
        }
    }

    private void OnResourceEarnPopupShow(bool isShow)
    {
        if (!isShow)
        {
            userResourcesObserver.Load();

            int quantity = userResourcesObserver.UserResources.BoosterQuantities[boosterIndex];

            if (quantity > 0)
            {
                quantityTextContainer.gameObject.SetActive(true);
                addButtonRT.gameObject.SetActive(false);

                quantityText.text = $"{quantity}";
            }
            else
            {
                quantityTextContainer.gameObject.SetActive(false);
                addButtonRT.gameObject.SetActive(true);
            }
        }
    }
}
