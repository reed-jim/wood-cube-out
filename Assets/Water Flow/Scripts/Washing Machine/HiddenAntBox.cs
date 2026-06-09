using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

public class HiddenAntBox : WashingMachineItem
{
    [SerializeField] private SpriteRenderer hiddenContainer;
    [SerializeField] private TMP_Text questionMark;

    public override void Setup(WashingMachineStartSlot washingMachineStartSlot)
    {
        base.Setup(washingMachineStartSlot);

        if (washingMachineStartSlot.isTopRow)
        {
            Reveal();
        }
        else
        {
            Hide();
        }
    }

    public override void Enable()
    {
        base.Enable();

        Reveal();
    }

    private void Hide()
    {
        SaferioTween.AlphaShow(questionMark);
        SaferioTween.AlphaShow(hiddenContainer);
        SaferioTween.AlphaHide(numberText);
    }

    public void Reveal()
    {
        SaferioTween.AlphaHide(questionMark);
        SaferioTween.AlphaHide(hiddenContainer);
        SaferioTween.AlphaShow(numberText);
    }

    protected override void PlayInvalidSelectAnimation()
    {
        Tween.ShakeScale(transform, strength: 0.2f * Vector3.one, duration: 0.3f);
    }
}
