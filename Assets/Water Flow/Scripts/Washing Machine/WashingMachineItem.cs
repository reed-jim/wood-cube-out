using System;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using static GameEnum;

public class WashingMachineItem : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public TMP_Text numberText;

    public GameFaction faction;
    public int number;
    public bool isMoving;
    public WashingMachineStartSlot washingMachineStartSlot;

    void Awake()
    {
        washingMachineStartSlot = transform.parent.GetComponent<WashingMachineStartSlot>();
    }

    public void Setup()
    {
        faction = CommonUtil.GetRandomEnumValue<GameFaction>(numLastValuesToSkip: 2);
        number = UnityEngine.Random.Range(1, 10);

        spriteRenderer.color = FactionUtility.GetColorForFaction(faction);
        numberText.text = $"{number}";
    }

    public void Init(WashingMachineStartSlot washingMachineStartSlot, GameFaction faction, int number)
    {
        this.faction = faction;
        this.number = number;

        spriteRenderer.color = FactionUtility.GetColorForFaction(faction);
        numberText.text = $"{number}";

        this.washingMachineStartSlot = washingMachineStartSlot;

        transform.SetParent(washingMachineStartSlot.transform);
        transform.localPosition = new Vector3(0, 0, -1);
        transform.localScale = Vector3.zero;

        Tween.Scale(transform, 1, duration: 0.3f);
    }

    public void MinusClothingPiece()
    {
        number--;
    }

    public void ReceiveClothes(Action onCompleteAction)
    {
        numberText.text = $"{number}";

        if (number <= 0)
        {
            SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
            {
                onCompleteAction?.Invoke();
            });
        }
    }

    public void Select()
    {
        WashingMachineManager.Ins.MoveItemToSlot(this);
    }
}
