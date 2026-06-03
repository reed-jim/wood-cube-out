using System.Collections.Generic;
using UnityEngine;

public class AntBoxLink : MonoBehaviour
{
    [SerializeField] private WashingMachineItem[] antBoxes;

    void Awake()
    {
        WashingMachineItem.selectWashingMachineEvent += OnAntBoxSelected;
    }

    void OnDestroy()
    {
        WashingMachineItem.selectWashingMachineEvent -= OnAntBoxSelected;
    }

    void Start()
    {
        Setup();
    }

    private void Setup()
    {
        for (int i = 0; i < antBoxes.Length; i++)
        {
            WashingMachineItem antBox = antBoxes[i];

            antBox.numSlotRequired = antBoxes.Length;
        }
    }

    private void OnAntBoxSelected(WashingMachineItem antBox)
    {
        WashingMachineItem selectedAntBox = null;

        for (int i = 0; i < antBoxes.Length; i++)
        {
            if (antBoxes[i] == antBox)
            {
                selectedAntBox = antBox;
            }
        }

        if (selectedAntBox != null)
        {
            List<WashingMachineItem> otherAntBoxes = new List<WashingMachineItem>();

            for (int i = 0; i < antBoxes.Length; i++)
            {
                if (antBoxes[i] != selectedAntBox)
                {
                    otherAntBoxes.Add(antBoxes[i]);
                }
            }
        }
    }
}
