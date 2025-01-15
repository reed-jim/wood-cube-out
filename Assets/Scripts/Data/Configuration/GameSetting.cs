using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Saferio/Screw Away/GameSetting")]
public class GameSetting : ScriptableObject
{
    [SerializeField] private bool isTurnOnBackgroundMusic = true;
    [SerializeField] private bool isTurnOnSound = true;
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

    public async Task LoadAsync()
    {
        GameSetting savedGameSetting = await DataUtility.LoadAsync(GameConstants.GAME_SETTING, new GameSetting());

        isTurnOnBackgroundMusic = savedGameSetting.isTurnOnBackgroundMusic;
        isTurnOnSound = savedGameSetting.IsTurnOnSound;
        currentLanguage = savedGameSetting.CurrentLanguage;
    }
}
