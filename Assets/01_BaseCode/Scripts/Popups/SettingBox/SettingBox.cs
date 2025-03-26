//using MoreMountains.NiceVibrations;
using System.Collections;
using System.Collections.Generic;
using GPlay.GplayGoogleUMP;
using Gplay.Notifications;
using UnityEngine;
using UnityEngine.UI;
using GPlayUI;

public class SettingBox : BaseBox
{
    private static SettingBox instance;

    public static SettingBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<SettingBox>(PathPrefabs.SETTING_BOX));
            instance.Init();
        }

        instance.gameObject.SetActive(true);
        return instance;
    }

    [Header("Sound/Music/Vibration/Notification")]
    [SerializeField]
    private Button vibrationBtn;

    [SerializeField] private Image vibrationImg;
    [SerializeField] private Sprite vibrationSprt_On;
    [SerializeField] private Sprite vibrationSprt_Off;

    [SerializeField] private Button soundBtn;
    [SerializeField] private Image soundImg;
    [SerializeField] private Sprite soundSprt_On;
    [SerializeField] private Sprite soundSprt_Off;

    [SerializeField] private Button musicBtn;
    [SerializeField] private Image musicImg;
    [SerializeField] private Sprite musicSprt_On;
    [SerializeField] private Sprite musicSprt_Off;

    [SerializeField] private Button notificationBtn;
    [SerializeField] private Image notificationImg;
    [SerializeField] private Sprite notificationOn;
    [SerializeField] private Sprite notificationOff;

    [Header("Settings Btns")]
    [SerializeField]
    private Button languageBtn;

    [SerializeField] private Button supportBtn;
    [SerializeField] private Button termsBtn;
    [SerializeField] private Button privacyBtn;
    [SerializeField] private Button rateUsBtn;
    [SerializeField] private Button restorePurchaseBtn;
    [SerializeField] private Button privacyOptionBtn;

    //[SerializeField] private ToggleUIHandle toggleTargetAds;

    [SerializeField] private Text versionTxt;
    [SerializeField] private Text idTxt;

    public void Init()
    {
        vibrationBtn.onClick.AddListener(OnClickVibration);
        soundBtn.onClick.AddListener(OnClickSound);
        musicBtn.onClick.AddListener(OnClickMusic);
        notificationBtn.onClick.AddListener(OnClickNotification);

        languageBtn.onClick.AddListener(OnClickLanguage);
        supportBtn.onClick.AddListener(OnClickSupport);
        termsBtn.onClick.AddListener(OnClickTermOfUse);
        privacyBtn.onClick.AddListener(OnClickPrivacy);
        restorePurchaseBtn.onClick.AddListener(OnClickRestorePurchase);
        rateUsBtn.onClick.AddListener(OnClickRateUs);
        privacyOptionBtn.onClick.AddListener(() =>
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp(
                    privacyOptionBtn.transform.position, Localization.Get("s_no_internet"), Color.white, isSpawnItemPlayer: true);
            }
            else
            {
                GplayUMP.ShowPrivacyOptionsForm(() =>
                {
                    GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp(
                    privacyOptionBtn.transform.position, "Error Show Privacy Options!", Color.white, isSpawnItemPlayer: true);
                });
            }
        });

        //notificationBtn.gameObject.SetActive(GameController.Instance.segmentController.GetCurrentSegment().notificationData.config.isActive);

#if UNITY_ANDROID
        restorePurchaseBtn.gameObject.SetActive(false);
#else
        restorePurchaseBtn.gameObject.SetActive(true);
#endif

        // toggleTargetAds.Init(
        //     actionOn: () =>
        //     {
        //         UseProfile.GDPRContentValue = true;
        //     },
        // actionOff: () =>
        // {
        //     //var trB = TrackingBox.Setup();
        //     //trB.Show();
        //
        //     //trB.OnCloseBox = () =>
        //     //{
        //     //    if (UseProfile.GDPRContentValue)
        //     //        toggleTargetAds.OnClickChoice();
        //     //    trB.OnCloseBox = null;
        //     //};
        //     UseProfile.GDPRContentValue = false;
        //
        // }, UseProfile.GDPRContentValue);

        versionTxt.text = "Version: " + Config.versionName.ToString();
        idTxt.text = UseProfile.PlayFabId;

        InitState();
    }

    public void InitState()
    {
        if (GameController.Instance.useProfile.OnVibration)
        {
            vibrationImg.sprite = vibrationSprt_On;
        }
        else
        {
            vibrationImg.sprite = vibrationSprt_Off;
        }

        if (GameController.Instance.useProfile.OnSound)
        {
            soundImg.sprite = soundSprt_On;
        }
        else
        {
            soundImg.sprite = soundSprt_Off;
        }

        if (GameController.Instance.useProfile.OnMusic)
        {
            musicImg.sprite = musicSprt_On;
        }
        else
        {
            musicImg.sprite = musicSprt_Off;
        }
        notificationImg.sprite = NotificationController.IsActive ? notificationOn : notificationOff;
    }

    public void OnClickVibration()
    {
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.useProfile.OnVibration = !GameController.Instance.useProfile.OnVibration;
        if (GameController.Instance.useProfile.OnVibration)
        {
            vibrationImg.sprite = vibrationSprt_On;
          //  MMVibrationManager.Haptic(HapticTypes.Success, false, true, this);
        }
        else
        {
            vibrationImg.sprite = vibrationSprt_Off;
        }
    }

    public void OnClickSound()
    {
        GameController.Instance.useProfile.OnSound = !GameController.Instance.useProfile.OnSound;
        if (GameController.Instance.useProfile.OnSound)
        {
            soundImg.sprite = soundSprt_On;
        }
        else
        {
            soundImg.sprite = soundSprt_Off;
        }

        GameController.Instance.musicManager.PlayClickSound();
    }

    public void OnClickMusic()
    {
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.useProfile.OnMusic = !GameController.Instance.useProfile.OnMusic;
        if (GameController.Instance.useProfile.OnMusic)
        {
            musicImg.sprite = musicSprt_On;
        }
        else
        {
            musicImg.sprite = musicSprt_Off;
        }
    }

    private void OnClickNotification()
    {
        GameController.Instance.musicManager.PlayClickSound();
        NotificationController.IsActive = !NotificationController.IsActive;
        notificationImg.sprite = NotificationController.IsActive ? notificationOn : notificationOff;
    }

    public void OnClickLanguage()
    {
        LanguageBox.Setup().Show();
        GameController.Instance.musicManager.PlayClickSound();
    }

    public void OnClickSupport()
    {
        GameController.Instance.musicManager.PlayClickSound();
        Application.OpenURL(Config.LinkFeedback);
    }

    public void OnClickTermOfUse()
    {
        GameController.Instance.musicManager.PlayClickSound();
        Application.OpenURL(Config.LinkTermOfSerice);
    }

    public void OnClickPrivacy()
    {
        GameController.Instance.musicManager.PlayClickSound();
        Application.OpenURL(Config.LinkPolicy);
    }

    public void OnClickRateUs()
    {
        GameController.Instance.musicManager.PlayClickSound();
        DialogueRate.Setup().Show();
    }

    public void OnClickRestorePurchase()
    {
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.iapController.RestorePurchases();
    }
}