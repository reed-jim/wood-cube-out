using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterItemUI : MonoBehaviour
{
    [SerializeField] private RectTransform quantityTextContainer;
    [SerializeField] private RectTransform quantityTextRT;
    [SerializeField] private RectTransform addButtonRT;

    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button addButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private int boosterIndex;

    [SerializeField] private UserResourcesObserver userResourcesObserver;

    void Awake()
    {
        Setup();
    }

    private void Setup()
    {
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
