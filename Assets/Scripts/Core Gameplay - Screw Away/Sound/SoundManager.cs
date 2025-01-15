using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource backgroundMusic;

    [SerializeField] private GameSetting gameSetting;

    #region PRIVATE FIELD
    private bool _isEnableGameSound;
    #endregion

    void Awake()
    {
        GameSettingManager.enableBackgroundMusicEvent += EnableBackgroundMusic;
        GameSettingManager.enableGameSoundEvent += EnableGameSound;
        GameVariableInitializer.gameSettingLoadedEvent += OnGameSettingLoaded;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy()
    {
        GameSettingManager.enableBackgroundMusicEvent -= EnableBackgroundMusic;
        GameSettingManager.enableGameSoundEvent -= EnableGameSound;
        GameVariableInitializer.gameSettingLoadedEvent -= OnGameSettingLoaded;
    }

    public void EnableBackgroundMusic(bool isEnable)
    {
        backgroundMusic.enabled = isEnable;
    }

    public void EnableGameSound(bool isEnable)
    {
        _isEnableGameSound = isEnable;
    }

    public void PlaySoundLoosenScrew()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.LOOSEN_SCREW_SOUND);

        sound.Play();
    }

    public void PlaySoundTightenScrew()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.TIGHTEN_SCREW_SOUND);

        sound.Play();
    }

    public void PlaySoundLoosenScrewFail()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.LOOSEN_SCREW_FAIL_SOUND);

        sound.Play();
    }

    private void OnGameSettingLoaded()
    {
        EnableBackgroundMusic(gameSetting.IsTurnOnBackgroundMusic);
        EnableGameSound(gameSetting.IsTurnOnSound);
    }
}
