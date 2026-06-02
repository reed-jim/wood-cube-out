using Saferio.Util.SaferioTween;
using UnityEngine;

public class HiddenAntBox : WashingMachineItem
{
    [SerializeField] private SpriteRenderer hiddenContainer;
    [SerializeField] private SpriteRenderer questionMark;

    public override void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number)
    {
        base.Init(washingMachineStartSlot, faction, number);

        Hide();
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
