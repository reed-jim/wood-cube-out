using System;
using AntsFlow;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using static GameEnum;

public class WashingMachineItem : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public TMP_Text numberText;

    public Color faction;
    public int number;
    public bool isMoving;
    public WashingMachineStartSlot washingMachineStartSlot;

    public static event Action<WashingMachineItem> selectWashingMachineEvent;

    void Awake()
    {
        washingMachineStartSlot = transform.parent.GetComponent<WashingMachineStartSlot>();
    }

    public void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number)
    {
        this.faction = faction;
        this.number = number;

        spriteRenderer.color = faction;
        numberText.text = $"{number}";

        this.washingMachineStartSlot = washingMachineStartSlot;

        Vector3 initialScale = transform.localScale;

        transform.SetParent(washingMachineStartSlot.transform);
        transform.localPosition = new Vector3(0, 0, -1);
        transform.localScale = Vector3.zero;

        Tween.Scale(transform, initialScale, duration: 0.3f);
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

        selectWashingMachineEvent?.Invoke(this);
    }

    public void SpawnAnts()
    {
        BaseAnt ant = ObjectPoolingEverything.GetFromPool<BaseAnt>(GameConstants.ant);

        ant.transform.position = transform.position.AddZ(1);

        ant.Init(faction);
        ant.MoveToTile();
    }
}
