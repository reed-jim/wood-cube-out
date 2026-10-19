using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource grainInTroughSound;

    [SerializeField] private GameSetting gameSetting;

    [Header("CUSTOMIZE")]
    public float grainInTroughSoundVolume;

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private bool _isEnableGameSound;
    private bool _isEnableGameMusic;
    #endregion

    #region Chicken Jam
    [Header("Chicken Jam")]
    private Tween grainInTroughSoundTween;
    private float _lastChickenPeckSoundTime;
    public float chickenPeckSoundCooldown;
    public float chickenFlySoundCooldown;
    private float _lastChickenFlySoundTime;
    private float _lastChickenCompleteSoundTime;
    public bool isPlayingChickenSound;
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

    #region Idle Spaceship
    public void PlayCycleComplete()
    {
        if (GamePersistentVariable.isPopupShowing)
        {
            return;
        }

        PlaySound(GameConstants.cycleCompleteSound);
    }
    #endregion

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

        if (sound != null)
        {
            sound.Play();
        }
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

        sound.Play();
    }

    #region Chicken Jam
    public void PlaySoundGrainHit()
    {
        PlaySound(GameConstants.grainHitSound);
    }

    public void PlayChickenFly()
    {
        PlaySoundWithCooldown(GameConstants.chickenFlySound, ref _lastChickenFlySoundTime, chickenFlySoundCooldown);
    }

    public void PlayChickenComplete()
    {
        PlaySoundWithCooldown(GameConstants.chickenCompleteSound, ref _lastChickenCompleteSoundTime, chickenFlySoundCooldown);
    }

    public void PlayGrainInTroughSound()
    {
        return;

        if (!_isEnableGameSound)
        {
            return;
        }

        if (grainInTroughSound == null)
        {
            grainInTroughSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.grainInTroughSound);

            grainInTroughSound.Play();
        }
        else
        {
            if (grainInTroughSound.volume < grainInTroughSoundVolume)
            {
                if (grainInTroughSoundTween.isAlive)
                {
                    grainInTroughSoundTween.Stop();
                }

                grainInTroughSoundTween = Tween.AudioVolume(grainInTroughSound, grainInTroughSoundVolume, duration: 0.3f);
            }
        }
    }

    public void ResumeGrainInTroughSound()
    {
        return;

        if (!_isEnableGameSound)
        {
            return;
        }

        if (grainInTroughSound == null)
        {
            return;
        }

        if (grainInTroughSoundTween.isAlive)
        {
            grainInTroughSoundTween.Stop();
        }

        grainInTroughSoundTween = Tween.AudioVolume(grainInTroughSound, grainInTroughSoundVolume, duration: 0.3f);
    }

    public void StopGrainInTroughSound()
    {
        return;

        if (grainInTroughSound == null)
        {
            return;
        }

        if (grainInTroughSoundTween.isAlive)
        {
            grainInTroughSoundTween.Stop();
        }

        grainInTroughSoundTween = Tween.AudioVolume(grainInTroughSound, 0, duration: 0.3f);
    }

    public void PlaySoundChickenPeck()
    {
        PlaySoundWithCooldown(GameConstants.cubeLandSound, ref _lastChickenPeckSoundTime, chickenPeckSoundCooldown);
    }

    public void PlayChickenSound()
    {
        if (_isEnableGameSound)
        {
            isPlayingChickenSound = true;
        }
    }

    public void StopChickenSound()
    {
        isPlayingChickenSound = false;
    }

    void DisableSoundObjectAfterFinishPlaying(AudioSource audioSource)
    {
        Tween.StopAll(audioSource);

        TweenManager.Ins.AddTween(Tween.Delay(audioSource.clip.length).OnComplete(() =>
        {
            audioSource.gameObject.SetActive(false);
        }));
    }
    #endregion

    #region Ants Flow
    public void PlaySoundAntPop()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.antPopSound);

        sound.Play();
    }

    public void PlaySoundBoxTap()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.boxTapSound);

        sound.Play();
    }

    public void PlaySoundCubeEat()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.cubeEatSound);

        sound.Play();
    }

    public void PlaySoundCubeLand()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.cubeLandSound);

        sound.Play();
    }

    public void PlaySoundSlotDone()
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.slotDoneSound);

        sound.Play();
    }

    public AudioSource PlaySound(string soundKey)
    {
        if (!_isEnableGameSound)
        {
            return null;
        }

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(soundKey);

        sound.Play();

        return sound;
    }

    public void PlaySoundWithCooldown(string soundKey, ref float lastPlaySoundTime, float cooldown)
    {
        if (!_isEnableGameSound)
        {
            return;
        }

        if (Time.time - lastPlaySoundTime < cooldown)
        {
            return;
        }

        lastPlaySoundTime = Time.time;

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(soundKey);

        sound.Play();

        DisableSoundObjectAfterFinishPlaying(sound);
    }
    #endregion

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

    public void OnGameSettingLoaded()
    {
        EnableBackgroundMusic(gameSetting.IsTurnOnBackgroundMusic);
        EnableGameSound(gameSetting.IsTurnOnSound);
    }
}
