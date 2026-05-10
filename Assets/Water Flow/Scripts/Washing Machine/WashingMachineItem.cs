using UnityEngine;

public class WashingMachineItem : MonoBehaviour
{
    public int number;
    public WashingMachineStartSlot washingMachineStartSlot;

    void Awake()
    {
        washingMachineStartSlot = transform.parent.GetComponent<WashingMachineStartSlot>();
    }

    public void Select()
    {
        WashingMachineManager.Ins.MoveItemToSlot(this);
    }
}
