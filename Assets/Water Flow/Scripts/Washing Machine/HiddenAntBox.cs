using Saferio.Util.SaferioTween;
using UnityEngine;

public class HiddenAntBox : WashingMachineItem
{
    [SerializeField] private SpriteRenderer hiddenContainer;
    [SerializeField] private SpriteRenderer questionMark;

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

    private void Hide()
    {
        SaferioTween.AlphaShow(questionMark);
        SaferioTween.AlphaShow(hiddenContainer);
    }

    public void Reveal()
    {
        SaferioTween.AlphaHide(questionMark);
        SaferioTween.AlphaHide(hiddenContainer);
    }
}
