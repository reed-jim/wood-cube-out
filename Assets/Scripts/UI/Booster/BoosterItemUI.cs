using System;
using System.Linq;
using System.Threading.Tasks;
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

    [Header("CUSTOMIZE")]
    [SerializeField] private int boosterIndex;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    public static event Action<BoosterType> showBuyBoosterPopupEvent;

    #region MonoBehaviour Callbacks

    void Awake()
    {
        BuyBoosterPopup.updateBoosterQuantityEvent += UpdateQuantityText;
        BoosterUI.updateBoosterQuantityEvent += UpdateQuantityText;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShow;
        YarnLevelLoader.updateBoosterTextEvent += UpdateQuantityText;

        addButton.onClick.AddListener(ShowBuyBoosterPopup);

        Setup();
    }

    void OnDestroy()
    {
        BuyBoosterPopup.updateBoosterQuantityEvent -= UpdateQuantityText;
        BoosterUI.updateBoosterQuantityEvent -= UpdateQuantityText;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourceEarnPopupShow;
        
        addButton.onClick.RemoveAllListeners();
    }
    
    

    #endregion
    private async void Setup()
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

    private void ShowBuyBoosterPopup()
    {
        showBuyBoosterPopupEvent?.Invoke((BoosterType)boosterIndex);
    }

    private void UpdateQuantityText(BoosterType boosterType)
    {
        if ((int)boosterType == this.boosterIndex)
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
    
    private void UpdateQuantityText(int _boosterIndex)
    {
        if (_boosterIndex == this.boosterIndex)
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
