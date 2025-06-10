using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource backgroundMusic;

    [SerializeField] private GameSetting gameSetting;

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private bool _isEnableGameSound;
    private bool _isEnableGameMusic;
    #endregion

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }
        else
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        GameSettingManager.enableBackgroundMusicEvent += EnableBackgroundMusic;
        GameSettingManager.enableGameSoundEvent += EnableGameSound;
        GameVariableInitializer.gameSettingLoadedEvent += OnGameSettingLoaded;

        _tweens = new List<Tween>();
    }

    private void OnDestroy()
    {
        GameSettingManager.enableBackgroundMusicEvent -= EnableBackgroundMusic;
        GameSettingManager.enableGameSoundEvent -= EnableGameSound;
        GameVariableInitializer.gameSettingLoadedEvent -= OnGameSettingLoaded;

        CommonUtil.StopAllTweens(_tweens);
    }

    public void EnableBackgroundMusic(bool isEnable)
    {
        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.AudioVolume(backgroundMusic, isEnable ? 0.5f : 0, duration: 0.3f));

        // backgroundMusic.enabled = isEnable;

        _isEnableGameMusic = isEnable;
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

    public void PlaySoundClick()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.CLICK_SOUND);

        sound.Play();
    }

    public void PlaySoundClose()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.CLOSE_POPUP_SOUND);

        sound.Play();
    }

    public void PlaySoundScrewBoxDone()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource screwBoxDoneSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.SCREW_BOX_DONE_SOUND);

        screwBoxDoneSound.Play();
    }

    public void PlaySoundWin()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        TemporarilyDisableBackgroundMusic();

        AudioSource winSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.WIN_SOUND);

        winSound.Play();
    }

    public void PlaySoundLose()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        TemporarilyDisableBackgroundMusic();

        AudioSource loseSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.LOSE_SOUND);

        loseSound.Play();
    }

    public void PlaySoundBreakObject()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource breakObjectSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.BREAK_OBJECT_SOUND);

        breakObjectSound.Play();
    }

    public void PlaySoundClearScrewPorts()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.CLEAR_SCREW_PORTS_SOUND);

        sound.Play();
    }

    public void PlaySoundUnlockScrewBox()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource breakObjectSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.UNLOCK_ADS_SCREW_BOX_SOUND);

        breakObjectSound.Play();
    }

    public void PlaySoundCountingTime()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.COUNTING_TIME_SOUND);

        sound.Play();
    }

    public void PlaySoundOpenChest()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.OPEN_CHEST_SOUND);

        sound.Play();
    }

    public void PlaySoundRewardItem()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.REWARD_ITEM_SOUND);

        sound.Play();
    }

    private void FadeOutBackgroundMusic()
    {
        _tweens.Add(Tween.Custom(0.5f, 0, duration: 0.3f, onValueChange: newVal =>
        {
            backgroundMusic.volume = newVal;
        }));
    }

    private void FadeInBackgroundMusic()
    {
        _tweens.Add(Tween.Custom(0, 0.5f, duration: 0.3f, onValueChange: newVal =>
        {
            backgroundMusic.volume = newVal;
        }));
    }

    private void TemporarilyDisableBackgroundMusic()
    {
        if (!_isEnableGameMusic)
        {
            return;
        }

        FadeOutBackgroundMusic();

        _tweens.Add(Tween.Delay(5).OnComplete(() => FadeInBackgroundMusic()));
    }

    private void OnGameSettingLoaded()
    {
        EnableBackgroundMusic(gameSetting.IsTurnOnBackgroundMusic);
        EnableGameSound(gameSetting.IsTurnOnSound);
    }
}
