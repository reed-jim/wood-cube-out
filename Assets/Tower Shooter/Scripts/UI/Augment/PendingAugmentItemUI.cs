using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PendingAugmentItemUI : MonoBehaviour
{
    public TMP_Text nameText;
    public Button applyButton;

    public Augment pendingAugment;
    public BaseCharacter node;

    private Action onApply;

    void Awake()
    {
        applyButton.onClick.AddListener(ApplyPendingAugment);
    }

    public void Init(Augment pendingAugment, BaseCharacter node, Action onApply)
    {
        nameText.text = pendingAugment.augmentName;

        this.pendingAugment = pendingAugment;
        this.node = node;

        this.onApply = onApply;
    }

    public void ApplyPendingAugment()
    {
        pendingAugment.Apply(node.serviceLocator.stats);

        AugmentManager.Ins.RemovePendingAugment(pendingAugment);

        onApply?.Invoke();
    }
}
