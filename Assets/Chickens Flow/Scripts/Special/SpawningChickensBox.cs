using System.Collections.Generic;
using AntsFlow;
using UnityEngine;

public class SpawningChickensBox : MonoBehaviour
{
    public int totalChicken;
    public int numChickenLeft;

    public void SpawnChickens()
    {
        if (numChickenLeft <= 0)
        {
            return;
        }

        List<WaitingChickenSlot> slotsToSpawn = new List<WaitingChickenSlot>();

        foreach (WaitingChickenSlot slot in ChickenManager.Ins.waitingChickenSlots.Values)
        {
            if (slot.chicken == null)
            {
                slotsToSpawn.Add(slot);
            }
        }

        for (int i = 0; i < slotsToSpawn.Count; i++)
        {
            WaitingChickenSlot slot = slotsToSpawn[i];

            BaseChicken chicken = ObjectPoolingEverything.GetFromPool<BaseChicken>(GameConstants.chicken);

            chicken.PlayFlyAnim();

            DOTweenUtil.MoveParabol(chicken.transform, slot.GetRandomPosition());

            slot.chicken = chicken;

            ChickenManager.Ins.AddChicken(chicken);

            WashingMachineManager.Ins.AddFactionGrainRequirement(chicken.faction, chicken.numGrainPileRequired);

            numChickenLeft--;
        }
    }
}
