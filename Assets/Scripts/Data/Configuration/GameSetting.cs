using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Saferio/Screw Away/GameSetting")]
public class GameSetting : ScriptableObject
{
    [SerializeField] private bool isDebug;
    [SerializeField] private bool isTurnOnBackgroundMusic = true;
    [SerializeField] private bool isTurnOnSound = true;
    [SerializeField] private bool isVibrate = true;
    [SerializeField] private string currentLanguage;

    [Header("=== TEST SEGMENT (tick để ép dùng 1 kịch bản trong dataSegment) ===")]
    [Tooltip("Bật để ép dùng kịch bản theo Test Segment Index, bỏ qua dữ liệu AppsFlyer thật. Nhớ TẮT trước khi build release chính thức.")]
    [SerializeField] private bool forceTestSegment;

    [Tooltip("Thứ tự kịch bản trong dataSegment muốn test: 0 = kịch bản đầu tiên, 1 = kịch bản thứ hai, ...")]
    [SerializeField] private int testSegmentIndex;

    public bool IsDebug
    {
        get => isDebug;
    }

    // Các field test bên dưới KHÔNG được LoadAsync ghi đè (giống isDebug),
    // nên chỉ điều khiển trực tiếp trên asset GameSetting trong Inspector.
    public bool ForceTestSegment
    {
        get => forceTestSegment;
    }

    public int TestSegmentIndex
    {
        get => testSegmentIndex;
    }

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

    public bool IsVibrate
    {
        get => isVibrate;
        set
        {
            isVibrate = value;
            Save();
        }
    }

    public bool isNotification;

    public string CurrentLanguage
    {
        get => currentLanguage;
        set
        {
            currentLanguage = value;
            Save();
        }
    }

    public GameSetting(string defaultLanguage)
    {
        currentLanguage = defaultLanguage;
    }

    public void Save()
    {
        DataUtility.SaveAsync(GameConstants.GAME_SETTING, this);
    }

    public async Task LoadAsync()
    {
        GameSetting defaultGameSetting = new GameSetting(GameConstants.DEFAULT_LANGUAGE);

        // GameSetting defaultGameSetting = ScriptableObject.CreateInstance<GameSetting>();
        // defaultGameSetting.currentLanguage = GameConstants.DEFAULT_LANGUAGE;

        GameSetting savedGameSetting = DataUtility.LoadAsync(GameConstants.GAME_SETTING, defaultGameSetting);

        isTurnOnBackgroundMusic = savedGameSetting.isTurnOnBackgroundMusic;
        isTurnOnSound = savedGameSetting.IsTurnOnSound;
        isVibrate = savedGameSetting.IsVibrate;
        currentLanguage = savedGameSetting.CurrentLanguage;
    }
}
