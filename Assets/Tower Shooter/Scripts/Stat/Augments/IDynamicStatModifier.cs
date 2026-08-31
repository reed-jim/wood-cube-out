using System.Collections.Generic;
using UnityEngine;

public interface IDynamicStatModifier
{
    void CollectModifiers(
        CharacterStats stats,
        StatType statType,
        List<StatModifier> modifiers
    );
}
