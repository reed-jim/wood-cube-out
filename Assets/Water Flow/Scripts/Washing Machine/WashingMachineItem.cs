using UnityEngine;
using static GameEnum;

public class WashingMachineItem : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public GameFaction faction;
    public int number;
    public WashingMachineStartSlot washingMachineStartSlot;

    void Awake()
    {
        washingMachineStartSlot = transform.parent.GetComponent<WashingMachineStartSlot>();

        Setup();
    }

    public void Setup()
    {
        faction = CommonUtil.GetRandomEnumValue<GameFaction>(numLastValuesToSkip: 2);

        spriteRenderer.color = FactionUtility.GetColorForFaction(faction);
    }

    public void Select()
    {
        WashingMachineManager.Ins.MoveItemToSlot(this);
    }
}
