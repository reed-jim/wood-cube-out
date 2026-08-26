using System.Collections.Generic;
using AntsFlow;
using UnityEngine;

public class SpawningChickensBox : MonoBehaviour
{
    public int totalChicken;
    public int numChickenLeft;

    [ContextMenu("SpawnChickens")]
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
            if (numChickenLeft <= 0)
            {
                break;
            }

            WaitingChickenSlot slot = slotsToSpawn[i];

            BaseChicken chicken = ObjectPoolingEverything.GetFromPool<BaseChicken>(GameConstants.chicken);

            chicken.Reset();

            chicken.PlayFlyAnim();

            DOTweenUtil.MoveParabol(chicken.transform, slot.GetRandomPosition(), onComplete: () =>
            {
                chicken.SetActiveSpine(false);
                chicken.ForcePlayRandomIdleAnim();
                ChickenManager.Ins.chickenRenderer.UpdateChickenRendering(chicken);
            });

            slot.chicken = chicken;

            ChickenManager.Ins.AddChicken(chicken);

            WashingMachineManager.Ins.AddFactionGrainRequirement(chicken.faction, chicken.numGrainPileRequired);

            numChickenLeft--;
        }
    }
}
