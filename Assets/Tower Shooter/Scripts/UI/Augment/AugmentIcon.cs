using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AugmentIcon : MonoBehaviour
{
    [SerializeField] private Image border;
    [SerializeField] private Image icon;

    public List<AugmentIconConfig> augmentIconConfigs;

    public void SetupIcon(Augment augment, Color qualityColor)
    {
        StatType strongestStat = StatType.Damage;

        float strongestStatValue = 0;

        if (augment.dynamicEffects.Count == 0)
        {
            for (int i = 0; i < augment.statModifiers.Count; i++)
            {
                StatModifierData statModifierData = augment.statModifiers[i];

                if (statModifierData.value > strongestStatValue)
                {
                    strongestStat = statModifierData.statType;
                    strongestStatValue = statModifierData.value;
                }
            }
        }
        else
        {
            List<GenericAugmentEffect> genericAugmentEffects = new List<GenericAugmentEffect>();

            for (int i = 0; i < augment.dynamicEffects.Count; i++)
            {
                AugmentEffect augmentEffect = augment.dynamicEffects[i];

                if (augmentEffect is not GenericAugmentEffect)
                {
                    continue;
                }

                GenericAugmentEffect genericAugmentEffect = (GenericAugmentEffect)augment.dynamicEffects[i];

                genericAugmentEffects.Add(genericAugmentEffect);
            }

            GenericAugmentEffect strongestAugmentEffect = AugmentScoringUtility.GetStrongestAugment(genericAugmentEffects);

            strongestStat = strongestAugmentEffect.config.targetStat;
        }

        for (int i = 0; i < augmentIconConfigs.Count; i++)
        {
            AugmentIconConfig augmentIconConfig = augmentIconConfigs[i];

            if (augmentIconConfig.statType == strongestStat)
            {
                icon.sprite = augmentIconConfig.icon;

                break;
            }
        }

        border.color = qualityColor;
        icon.color = qualityColor;
    }
}

public class AugmentIconConfig
{
    public StatType statType;
    public Sprite icon;
}
