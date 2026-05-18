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
        float totalBonusStat = CharacterManager.Ins.mainCharacter.characterServiceLocator.characterStat.health.currentValue * (healthMultiplier - 1);

        return
            $"Multiply your reward by <color=#BA55D3>{healthMultiplier} ({totalBonusStat})";
    }
}
