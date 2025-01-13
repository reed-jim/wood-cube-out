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
        set
        {
            isTurnOnBackgroundMusic = value;
            Save();
        }
    }

    public bool IsTurnOnSound
    {
        get => isTurnOnSound;
        set
        {
            isTurnOnSound = value;
            Save();
        }
    }

    public string CurrentLanguage
    {
        get => currentLanguage;
        set
        {
            currentLanguage = value;
            Save();
        }
    }

    public void Save()
    {
        DataUtility.SaveAsync(GameConstants.GAME_SETTING, this);
    }

    public void Load()
    {
        GameSetting savedGameSetting = DataUtility.Load(GameConstants.GAME_SETTING, new GameSetting());

        isTurnOnBackgroundMusic = savedGameSetting.isTurnOnBackgroundMusic;
        isTurnOnSound = savedGameSetting.IsTurnOnSound;
        currentLanguage = savedGameSetting.CurrentLanguage;
    }
}
