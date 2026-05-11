using TMPro;
using UnityEngine;
using static GameEnum;

public class WashingMachineItem : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public TMP_Text numberText;

    public GameFaction faction;
    public int number;
    public WashingMachineStartSlot washingMachineStartSlot;

    void Awake()
    {
        washingMachineStartSlot = transform.parent.GetComponent<WashingMachineStartSlot>();
    }

    public void Setup()
    {
        faction = CommonUtil.GetRandomEnumValue<GameFaction>(numLastValuesToSkip: 2);
        number = Random.Range(1, 10);

        spriteRenderer.color = FactionUtility.GetColorForFaction(faction);
        numberText.text = $"{number}";
    }

    public void Init(WashingMachineStartSlot washingMachineStartSlot, GameFaction faction)
    {
        this.faction = faction;
        number = Random.Range(1, 10);

        spriteRenderer.color = FactionUtility.GetColorForFaction(faction);
        numberText.text = $"{number}";

        this.washingMachineStartSlot = washingMachineStartSlot;

        transform.SetParent(washingMachineStartSlot.transform);
        transform.localPosition = new Vector3(0, 0, -1);
    }

    public void ReceiveClothes()
    {
        number--;

        numberText.text = $"{number}";
    }

    public void Select()
    {
        WashingMachineManager.Ins.MoveItemToSlot(this);
    }
}
