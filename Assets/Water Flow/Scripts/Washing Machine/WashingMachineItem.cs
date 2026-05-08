using UnityEngine;

public class WashingMachineItem : MonoBehaviour
{
    public int number;

    public void Select()
    {
        WashingMachineManager.Ins.MoveItemToSlot(this);
    }
}
