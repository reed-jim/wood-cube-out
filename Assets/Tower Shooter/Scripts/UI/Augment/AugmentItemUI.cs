using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AugmentItemUIs : PageViewItem
{
    public TMP_Text title;
    public Button showAugmentDetailButton;

    public AugmentContainer augmentContainer;

    private Augment augment;

    public static event Action<Augment> showAugmentDetailEvent;

    public override void Setup(int index)
    {
        base.Setup(index);

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

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        showAugmentDetailButton.onClick.AddListener(ShowAugmentDetail);
    }

    private void SetupAugment(Augment augment)
    {
        title.text = augment.augmentName;
    }

    private void ShowAugmentDetail()
    {
        showAugmentDetailEvent?.Invoke(augment);
    }
}
