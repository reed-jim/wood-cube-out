using System;
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

    void Awake()
    {
        BoosterUI.updateBoosterQuantityEvent += UpdateQuantityText;

        addButton.onClick.AddListener(ShowBuyBoosterPopup);

        Setup();
    }

    void OnDestroy()
    {
        BoosterUI.updateBoosterQuantityEvent -= UpdateQuantityText;
    }

    private async void Setup()
    {
        while (userResourcesObserver.UserResources.BoosterQuantities.Length == 0)
        {
            await Task.Delay(200);
        }

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

    private void UpdateQuantityText(int boosterIndex)
    {
        if (boosterIndex == this.boosterIndex)
        {
            quantityText.text = $"{userResourcesObserver.UserResources.BoosterQuantities[boosterIndex]}";
        }
    }
}
