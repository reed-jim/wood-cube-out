using UnityEngine;

public class Rabadon : InventoryItem
{
    public float healthMultiplier;

    public override void SpecialEffect()
    {
        health *= healthMultiplier;
    }

    public override string GetSpecialEffectDescription()
    {
        return
            $"Multiply your reward by {healthMultiplier} ({CharacterManager.Ins.mainCharacter.characterServiceLocator.characterStat.health.currentValue * (healthMultiplier - 1)})";
    }
}
