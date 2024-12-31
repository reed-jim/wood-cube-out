using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Saferio/ScrewAway/LevelDataContainer")]
public class LevelDataContainer : ScriptableObject
{
    [SerializeField] private LevelData[] levelsData;

    public LevelData[] LevelsData
    {
        get => levelsData;
    }
}
