using PrimeTween;
using UnityEngine;
using static GameEnum;

public class ClothingPiece : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public GameFaction faction;
    public bool isMoving;

    public void Setup()
    {
        faction = CommonUtil.GetRandomEnumValue<GameFaction>(numLastValuesToSkip: 2);

        spriteRenderer.color = FactionUtility.GetColorForFaction(faction);
    }

    public void Init(GameFaction faction)
    {
        this.faction = faction;

        spriteRenderer.color = FactionUtility.GetColorForFaction(faction);
    }

    public void MoveToWashingMachine(WashingMachineItem washingMachineItem)
    {
        Tween.Position(transform, washingMachineItem.transform.position, duration: 0.3f)
        .OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
