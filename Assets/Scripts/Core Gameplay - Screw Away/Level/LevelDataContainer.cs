using System;
using NamPhuThuy.GamePlay;
using UnityEngine;

[CreateAssetMenu(menuName = "WoodCube/LevelDataContainer")]
public class LevelDataContainer : ScriptableObject
{
    [SerializeField] private int maxLevel;
    [SerializeField] private LevelData[] levelsData;
    [SerializeField] private LevelDifficultyConfiguration[] templateLevelDifficultyConfiguration;
    [SerializeField] private LevelDifficultyConfiguration fallbackLevelDifficultyConfiguration;

    public int MaxLevel
    {
        get => maxLevel;
    }

    public LevelData[] LevelsData
    {
        get => levelsData;
    }

    public LevelDifficultyConfiguration GetLevelDifficultyConfiguration(int level)
    {
        int modulus = (level - 1) % 5;

        return templateLevelDifficultyConfiguration[modulus];
    }

    private void OnValidate()
    {
        if (levelsData == null || levelsData.Length == 0)
            return;

        bool hasChanges = false;

        for (int i = 0; i < levelsData.Length; i++)
        {
            if (levelsData[i] != null)
            {
                int expectedLevelId = i + 1; // Level IDs start from 1
            
                if (levelsData[i].levelId != expectedLevelId)
                {
                    levelsData[i].levelId = expectedLevelId;
                    hasChanges = true;
                }
            }
        }

        // Update maxLevel to match array length
        /*if (maxLevel != levelsData.Length)
        {
            maxLevel = levelsData.Length;
            hasChanges = true;
        }*/

#if UNITY_EDITOR
        if (hasChanges)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}

[Serializable]
public class LevelData
{
    public int levelId;
    public GameObject levelPrefab;
    public LevelMode levelMode;
}
