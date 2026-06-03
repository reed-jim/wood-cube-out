using System;
using System.Collections;
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
    public bool isDisable;
    public int numSlotRequired = 1;
    public int number;
    public bool isMoving;
    public WashingMachineStartSlot washingMachineStartSlot;
    public WashingMachineSlot washingMachineSlot;

    public static event Action<WashingMachineItem> selectWashingMachineEvent;
    public static event Action<WashingMachineItem> antBoxDoneEvent;

    void Awake()
    {
        antBoxDoneEvent += OnAntBoxCompleted;

        washingMachineStartSlot = transform.parent.GetComponent<WashingMachineStartSlot>();

        ExtendAwake();
    }

    void OnDestroy()
    {
        antBoxDoneEvent -= OnAntBoxCompleted;

        ExtendOnDestroy();
    }

    protected virtual void ExtendAwake()
    {

    }

    protected virtual void ExtendOnDestroy()
    {

    }

    public virtual void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number)
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

        Tween.Scale(transform, 0.5f, duration: 0.3f);

        Setup(washingMachineStartSlot);
    }

    public virtual void Setup(WashingMachineStartSlot washingMachineStartSlot)
    {
        isDisable = !washingMachineStartSlot.isTopRow;

        numberText.alpha = isDisable ? 0.2f : 1;
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
                antBoxDoneEvent?.Invoke(this);
                onCompleteAction?.Invoke();
            });
        }
    }

    public void ReceiveTile()
    {
        number--;
        numberText.text = $"{number}";

        if (number <= 0)
        {
            SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
            {
                antBoxDoneEvent?.Invoke(this);
            });

            SoundManager.Instance.PlaySoundSlotDone();
        }
    }

    public virtual void Select()
    {
        Select(numSlotRequired);
    }

    public void Select(int numSlotRequired)
    {
        if (!IsValidToSelect(numSlotRequired))
        {
            SaferioTween.Punch(transform, 1.1f, initScale: transform.localScale.x);

            return;
        }

        WashingMachineSlot washingMachineSlot = WashingMachineManager.Ins.MoveItemToSlot(this);

        SoundManager.Instance.PlaySoundBoxTap();

        this.washingMachineSlot = washingMachineSlot;

        selectWashingMachineEvent?.Invoke(this);
    }

    public void ForceSelect()
    {
        if (isDisable)
        {
            Enable();
        }

        WashingMachineSlot washingMachineSlot = WashingMachineManager.Ins.MoveItemToSlot(this);

        SoundManager.Instance.PlaySoundBoxTap();

        this.washingMachineSlot = washingMachineSlot;

        selectWashingMachineEvent?.Invoke(this);
    }

    public virtual void Enable()
    {
        isDisable = false;

        numberText.alpha = 1;
    }

    public virtual bool IsValidToSelect()
    {
        return !isDisable && WashingMachineManager.Ins.GetAvailableWashingMachineSlots().Count >= numSlotRequired;
    }

    public virtual bool IsValidToSelect(int currentNumSlotRequired)
    {
        return !isDisable && WashingMachineManager.Ins.GetAvailableWashingMachineSlots().Count >= currentNumSlotRequired;
    }

    public void SpawnAnts()
    {
        // BaseAnt ant = ObjectPoolingEverything.GetFromPool<BaseAnt>(GameConstants.ant);

        // ant.transform.position = transform.position.AddZ(1);

        // ant.Init(faction);
        // ant.MoveToTile();

        StartCoroutine(SpawningAnts());
    }

    private IEnumerator SpawningAnts()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.3f);

        while (number > 0)
        {
            if (AntsFlow.TileManager.Ins.GetAvailableTile(faction) != null)
            {
                BaseAnt ant = ObjectPoolingEverything.GetFromPool<BaseAnt>(GameConstants.ant);

                ant.transform.position = transform.position.ChangeZ(0);

                ant.Init(faction);
                ant.MoveToTile();

                SoundManager.Instance.PlaySoundAntPop();

                number--;
                numberText.text = $"{number}";
            }

            yield return waitForSeconds;
        }

        SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
        {
            washingMachineSlot.washingMachineItem = null;

            antBoxDoneEvent?.Invoke(this);
        });
    }

    protected virtual void OnAntBoxCompleted(WashingMachineItem antBox)
    {

    }
}
