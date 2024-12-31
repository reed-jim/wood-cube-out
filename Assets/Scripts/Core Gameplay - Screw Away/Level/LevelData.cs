using System;
using UnityEngine;

[Serializable]
public class LevelData
{
    [SerializeField] private LevelDifficultyConfiguration levelDifficultyConfiguration;
    [SerializeField] private int numScrew;

    public LevelDifficultyConfiguration LevelDifficultyConfiguration
    {
        get => levelDifficultyConfiguration;
    }

    public int NumScrew
    {
        get => numScrew;
    }
}
