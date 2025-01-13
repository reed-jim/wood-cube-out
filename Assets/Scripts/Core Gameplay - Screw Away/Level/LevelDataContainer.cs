using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Saferio/ScrewAway/LevelDataContainer")]
public class LevelDataContainer : ScriptableObject
{
    [SerializeField] private LevelData[] levelsData;
    [SerializeField] private LevelDifficultyConfiguration fallbackLevelDifficultyConfiguration;

    public LevelData[] LevelsData
    {
        get => levelsData;
    }

    public LevelDifficultyConfiguration GetLevelDifficultyConfiguration(int level)
    {
        int levelIndex = level - 1;

        if (levelIndex < levelsData.Length)
        {
            if (levelsData[levelIndex] != null)
            {
                return levelsData[levelIndex].LevelDifficultyConfiguration;
            }
        }

        return fallbackLevelDifficultyConfiguration;
    }
}
