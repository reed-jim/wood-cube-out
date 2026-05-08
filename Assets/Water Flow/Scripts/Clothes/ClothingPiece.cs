using PrimeTween;
using UnityEngine;

public class ClothingPiece : MonoBehaviour
{
    public void MoveToWashingMachine(WashingMachineItem washingMachineItem)
    {
        Tween.Position(transform, washingMachineItem.transform.position, duration: 0.3f)
        .OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
