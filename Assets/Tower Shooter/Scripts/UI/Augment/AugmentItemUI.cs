using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AugmentItemUIs : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text description;
    public Button showAugmentDetailButton;

    public AugmentContainer augmentContainer;

    private Augment augment;

    [SerializeField] private StatColorConfigSO colorConfig;

    public static event Action<Augment> showAugmentDetailEvent;

    private Action onAugmentSelected;

    void Awake()
    {
        showAugmentDetailButton.onClick.AddListener(PickAugment);
    }

    public void Init(Augment augment, Action onAugmentSelected)
    {
        gameObject.SetActive(true);

        this.augment = augment;

        SetupAugment(augment);

        this.onAugmentSelected = onAugmentSelected;
    }

    private void SetupAugment(Augment augment)
    {
        title.text = augment.augmentName;
        description.text = augment.BuildFullDescription(colorConfig);
    }

    private void ShowAugmentDetail()
    {
        showAugmentDetailEvent?.Invoke(augment);
    }

    public void PickAugment()
    {
        AugmentManager.Ins.PickAugment(augment);

        onAugmentSelected?.Invoke();
    }
}
