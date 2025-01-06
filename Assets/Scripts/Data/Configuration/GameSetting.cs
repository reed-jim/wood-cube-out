using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Saferio/Screw Away/GameSetting")]
public class GameSetting : ScriptableObject
{
    [SerializeField] private bool isTurnOnBackgroundMusic;
    [SerializeField] private bool isTurnOnSound;
    [SerializeField] private string currentLanguage;

    public bool IsTurnOnBackgroundMusic
    {
        get => isTurnOnBackgroundMusic;
        set => isTurnOnBackgroundMusic = value;
    }

    public bool IsTurnOnSound
    {
        get => isTurnOnSound;
        set => isTurnOnSound = value;
    }

    public string CurrentLanguage
    {
        get => currentLanguage;
        set => currentLanguage = value;
    }
}
