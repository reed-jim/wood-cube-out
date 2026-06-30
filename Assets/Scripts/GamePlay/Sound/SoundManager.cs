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
    private bool _isEnableGameSound = true;
    private bool _isEnableGameMusic = true;
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

    public void PlaySoundGenerateTask()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.GENERATE_TASK_SOUND);

        sound.Play();
    }

    public void PlaySoundCompleteTask()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.COMPlETE_TASK_SOUND);

        sound.Play();
    }

    public void PlaySoundChainBreak()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.CHAIN_BREAK_SOUND);

        sound.Play();
    }

    public void PlaySoundRustyBreak()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.RUSTY_BREAK_SOUND);

        sound.Play();
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

    public void PlaySoundChestAppear()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.CHEST_APPEAR_SOUND);

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

    public void PlaySoundCollectCoin()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.collectCoinSound);

        sound.loop = false;
        sound.Play();
    }

    public void PlaySoundAddSlot()
    {
        if (!_isEnableGameSound) return;
        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.ADD_SLOT_SOUND);
        sound.loop = false;
        sound.Play();
    }

    //public void PlaySoundCardPutIntoTheme()
    //{
    //    if (!_isEnableGameSound) return;
    //    AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.CARD_PUT_INTO_THEME_SOUND);
    //    sound.loop = false;
    //    sound.Play();
    //}

    //public void PlaySoundDealCards()
    //{
    //    if (!_isEnableGameSound) return;
    //    AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.DEAL_CARDS_SOUND);
    //    sound.loop = false;
    //    sound.Play();
    //}

    public void PlaySoundFreezeBooster()
    {
        if (!_isEnableGameSound) return;
        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.FREEZE_BOOSTER_SOUND);
        sound.loop = false;
        sound.Play();
    }

    public void PlaySoundMatchCorrectCard()
    {
        if (!_isEnableGameSound) return;
        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.MATCH_CORRECT_CARD_SOUND);
        sound.loop = false;
        sound.Play();
    }

    public void PlaySoundHintBooster()
    {
        if (!_isEnableGameSound) return;
        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.HINT_BOOSTER_SOUND);
        sound.loop = false;
        sound.Play();
    }

    public void PlaySoundCompleteTheme()
    {
        if (!_isEnableGameSound) return;
        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.COMPLETE_THEME_SOUND);
        sound.loop = false;
        sound.Play();
    }

    public void PlaySoundFlipCard()
    {
        if (!_isEnableGameSound) return;
        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.FLIP_CARD_SOUND);
        sound.loop = false;
        sound.Play();
    }

    public void PlaySoundTakeCardFromGrid()
    {
        if (!_isEnableGameSound) return;
        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.TAKE_CARD_FROM_GRID_SOUND);
        sound.loop = false;
        sound.Play();
    }

    public void PlaySoundShuffleBooster()
    {
        if (!_isEnableGameSound) return;
        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.SHUFFLE_BOOSTER_SOUND);
        sound.loop = false;
        sound.Play();
    }

    public void PlaySoundDropCardBackToTable()
    {
        if (!_isEnableGameSound) return;
        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.DROP_CARD_BACK_TO_TABLE_SOUND);
        sound.loop = false;
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
