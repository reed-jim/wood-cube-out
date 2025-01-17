using System;
using UnityEngine;
using static GameEnum;

[CreateAssetMenu(menuName = "ScriptableObjects/Saferio/Screw Away/LevelDifficultyConfiguration")]
public class LevelDifficultyConfiguration : ScriptableObject
{
    [SerializeField] private LevelPhase[] levelPhases;
    [Header("Modify chance to get screw ports' faction. Range: -99 --> 99")]
    [SerializeField] private int screwPortFactionDifficultyModifier;
    [SerializeField] private float ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame;

    public LevelPhase[] LevelPhases
    {
        get => levelPhases;
        set => levelPhases = value;
    }

    public int ScrewPortFactionDifficultyModifier
    {
        get => screwPortFactionDifficultyModifier;
    }

    public float RatioGeneratingSameFactionForTripleNoBlockedScrewStartGame
    {
        get => ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame;
    }
}

[Serializable]
public class LevelPhase
{
    [SerializeField] private int phaseIndex;
    [SerializeField] private float endProgress;
    // [SerializeField] private LevelDifficulty levelDifficulty;

    public LevelPhase()
    {

    }

    public LevelPhase(int phaseIndex, float endProgress)
    {
        this.phaseIndex = phaseIndex;
        this.endProgress = endProgress;
    }

    public int PhaseIndex
    {
        get => phaseIndex;
        set => phaseIndex = value;
    }

    public float EndProgress
    {
        get => endProgress;
        set => endProgress = value;
    }

    // public LevelDifficulty LevelDifficulty
    // {
    //     get => levelDifficulty;
    // }
}
