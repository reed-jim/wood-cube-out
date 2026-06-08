using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using Dreamteck.Utilities;
using UnityEngine;

public enum LevelElement
{
    NormalAntBox,
    HiddenAntBox,
    AntBoxLink,
    FreezedAntBox
}

public class SpecialElementManager : Singleton<SpecialElementManager>
{
    public LevelElement nextLevelElement;

    public List<AntBoxLink> antBoxLinks;

    public void SpawnAntBoxLink()
    {
        AntBoxLink antBoxLink = ObjectPoolingEverything.GetFromPool<AntBoxLink>(GameConstants.antBoxLink);

        List<WashingMachineItem> validAntBoxesToLink = new List<WashingMachineItem>();

        for (int i = 0; i < WashingMachineManager.Ins.washingMachineStartSlots.Count; i++)
        {
            WashingMachineItem washingMachine = WashingMachineManager.Ins.washingMachineStartSlots[i].washingMachine;

            if (washingMachine != null)
            {
                if (washingMachine.numSlotRequired == 1)
                {
                    validAntBoxesToLink.Add(washingMachine);
                }
            }
        }

        if (validAntBoxesToLink.Count >= 2)
        {
            validAntBoxesToLink.Shuffle();

            antBoxLink.Setup(validAntBoxesToLink.Take(2).ToArray());
        }

        antBoxLinks.Add(antBoxLink);
    }

    public void RandomizeNextLevelElement()
    {
        // nextLevelElement = LevelElement.NormalAntBox;
        nextLevelElement = CommonUtil.GetRandomEnumValue<LevelElement>();
    }
}
