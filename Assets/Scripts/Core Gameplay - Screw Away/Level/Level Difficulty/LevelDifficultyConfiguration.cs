using System;
using UnityEngine;
using static GameEnum;

[CreateAssetMenu(menuName = "ScriptableObjects/Saferio/Screw Away/LevelDifficultyConfiguration")]
public class LevelDifficultyConfiguration : ScriptableObject
{
    [SerializeField] private LevelPhase[] levelPhases;
    [Header("Modify chance to get screw ports' faction. Range: -99 --> 99")]
    [SerializeField] private int screwPortFactionDifficultyModifier;

    public LevelPhase[] LevelPhases
    {
        get => levelPhases;
    }

    public int ScrewPortFactionDifficultyModifier
    {
        get => screwPortFactionDifficultyModifier;
    }
}

[Serializable]
public class LevelPhase
{
    [SerializeField] private float endProgress;
    // [SerializeField] private LevelDifficulty levelDifficulty;

    public float EndProgress
    {
        get => endProgress;
    }

    // public LevelDifficulty LevelDifficulty
    // {
    //     get => levelDifficulty;
    // }
}
