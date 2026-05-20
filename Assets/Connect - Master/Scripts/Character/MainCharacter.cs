using System.Collections.Generic;
using UnityEngine;

public class MainCharacter : BaseCharacter
{
    public float currentExp;
    public float level;

    private float GetRequiredExpForNextLevel()
    {
        return 100f + 50f * level * level;
    }

    public float GetLevelProgress()
    {
        return currentExp / GetRequiredExpForNextLevel();
    }

    public void EarnExp(float value)
    {
        currentExp += value;

        if (currentExp > GetRequiredExpForNextLevel())
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        currentExp = 0;

        AugmentManager.Ins.StartAugmentSelection();
    }
}
