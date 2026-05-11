using PrimeTween;
using UnityEngine;
using static GameEnum;

public class ClothingPiece : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public GameFaction faction;

    public void Setup()
    {
        faction = CommonUtil.GetRandomEnumValue<GameFaction>();

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
