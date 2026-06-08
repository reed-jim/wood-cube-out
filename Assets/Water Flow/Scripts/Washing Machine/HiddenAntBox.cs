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
    }

    public void Reveal()
    {
        SaferioTween.AlphaHide(questionMark);
        SaferioTween.AlphaHide(hiddenContainer);
    }
}
