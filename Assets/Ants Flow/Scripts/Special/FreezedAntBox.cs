using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

public class FreezedAntBox : WashingMachineItem
{
    [SerializeField] private SpriteRenderer freeze;
    [SerializeField] private TMP_Text numAntBoxToUnfreezeText;

    public int numAntBoxToUnfreeze;

    public override void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number)
    {
        base.Init(washingMachineStartSlot, faction, number);

        numAntBoxToUnfreeze = Random.Range(1, 9);

        numAntBoxToUnfreezeText.text = $"{numAntBoxToUnfreeze}";

        freeze.gameObject.SetActive(true);
    }

    public override bool IsValidToSelect()
    {
        return base.IsValidToSelect() && numAntBoxToUnfreeze <= 0;
    }

    public override bool IsValidToSelect(int currentNumSlotRequired)
    {
        return base.IsValidToSelect(currentNumSlotRequired) && numAntBoxToUnfreeze <= 0;
    }

    private void Unfreeze()
    {
        SaferioTween.AlphaHide(freeze);
        SaferioTween.AlphaHide(numAntBoxToUnfreezeText);
    }

    protected override void OnAntBoxCompleted(WashingMachineItem antBox)
    {
        base.OnAntBoxCompleted(antBox);

        numAntBoxToUnfreeze--;
        numAntBoxToUnfreeze = Mathf.Max(numAntBoxToUnfreeze, 0);

        numAntBoxToUnfreezeText.text = $"{numAntBoxToUnfreeze}";

        if (numAntBoxToUnfreeze == 0)
        {
            Unfreeze();
        }
    }
}
