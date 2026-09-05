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

    void Awake()
    {
        showAugmentDetailButton.onClick.AddListener(ApplyAugment);
    }

    public void Init(int index)
    {
        if (index < augmentContainer.augments.Count)
        {
            gameObject.SetActive(true);

            augment = augmentContainer.augments[index];

            SetupAugment(augment);
        }
        else
        {
            gameObject.SetActive(false);
        }
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

    public void ApplyAugment()
    {
        AugmentManager.Ins.AddAugment(augment);
    }
}
