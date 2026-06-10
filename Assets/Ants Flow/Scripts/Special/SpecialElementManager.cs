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
    public SpecialElementConfig specialElementConfig;

    // private FrozenAntBoxConfig frozenAntBoxConfig;
    // private SpecialElementItemConfig hiddenAntBoxConfig;
    // private SpecialElementItemConfig doubleAntBoxConfig;

    public List<AntBoxLink> antBoxLinks;

    public void Setup(AntsFlow.LevelItem levelItem)
    {
        specialElementConfig = levelItem.specialElementConfig;
    }

    public void SpawnAntBoxLink()
    {
        AntBoxLink antBoxLink = ObjectPoolingEverything.GetFromPool<AntBoxLink>(GameConstants.antBoxLink);

        List<WashingMachineItem> validAntBoxesToLink = new List<WashingMachineItem>();

        for (int i = 0; i < WashingMachineManager.Ins.washingMachineStartSlots.Count; i++)
        {
            WashingMachineItem washingMachine = WashingMachineManager.Ins.washingMachineStartSlots[i].washingMachine;

            if (
                washingMachine != null &&
                washingMachine is not HiddenAntBox &&
                washingMachine is not FreezedAntBox
            )
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
        int random = Random.Range(0, 10);

        if (random < 5)
        {
            if (specialElementConfig.hiddenAntBoxConfig.num > 0)
            {
                nextLevelElement = LevelElement.HiddenAntBox;

                specialElementConfig.hiddenAntBoxConfig.num--;
            }
            else if (specialElementConfig.frozenAntBoxConfig.num > 0)
            {
                nextLevelElement = LevelElement.FreezedAntBox;

                specialElementConfig.frozenAntBoxConfig.num--;
            }
        }
        else
        {
            nextLevelElement = LevelElement.NormalAntBox;
        }
    }
}
