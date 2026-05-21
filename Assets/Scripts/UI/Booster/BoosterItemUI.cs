using System;
using System.Linq;
using System.Threading.Tasks;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class BoosterItemUI : MonoBehaviour
{
    [SerializeField] private RectTransform quantityTextContainer;
    [SerializeField] private RectTransform quantityTextRT;
    [SerializeField] private RectTransform addButtonRT;

    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button addButton;

    [Header("LOCK")]
    [SerializeField] private CanvasGroup lockCanvasGroup;
    [SerializeField] private TMP_Text levelToUnlockText;
    [SerializeField] private int levelToUnlock;

    [Header("CUSTOMIZE")]
    [SerializeField] private int boosterIndex;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    public static event Action<BoosterType> showBuyBoosterPopupEvent;

    void Awake()
    {
        BuyBoosterPopup.updateBoosterQuantityEvent += UpdateQuantityText;
        BoosterUI.updateBoosterQuantityEvent += UpdateQuantityText;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShow;

        addButton.onClick.AddListener(ShowBuyBoosterPopup);

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

        #region Lock
        if (currentLevel.Value < levelToUnlock)
        {
            SaferioTween.AlphaShow(lockCanvasGroup);

            levelToUnlockText.text = $"Level {levelToUnlock}";

            quantityTextContainer.gameObject.SetActive(false);
            addButton.gameObject.SetActive(false);
        }
        else
        {
            SaferioTween.AlphaHide(lockCanvasGroup);

            quantityTextContainer.gameObject.SetActive(true);
            addButton.gameObject.SetActive(true);
        }
        #endregion
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
