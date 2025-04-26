//using MoreMountains.NiceVibrations;

using System;
using System.Collections;
using System.Collections.Generic;
using GPlay.GplayGoogleUMP;
using Gplay.Notifications;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
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
    [SerializeField] private SaferioToggle musicToggle;
    [SerializeField] private SaferioToggle soundToggle;
    [SerializeField] private SaferioToggle vibrationToggle;

    [Header("Settings Btns")]
    [SerializeField]
    private Button languageBtn;

    [SerializeField] private Button supportBtn;
    [SerializeField] private Button termsBtn;
    [SerializeField] private Button privacyBtn;
    [SerializeField] private Button rateUsBtn;
    [SerializeField] private Button restorePurchaseBtn;
    [SerializeField] private Button privacyOptionBtn;

    [SerializeField] private RectTransform chooseLanguageContainer;
    [SerializeField] private CanvasGroup chooseLanguageContainerCanvasGroup;
    [SerializeField] private LanguageItem[] _flagItems;
    [SerializeField] private LanguageSpriteDictionary flagDict;
    [SerializeField] private Transform parentGroup;
    [SerializeField] private LanguageItem sampleFlagButton;
    
    //[SerializeField] private ToggleUIHandle toggleTargetAds;

    [SerializeField] private Text versionTxt;
    [SerializeField] private Text idTxt;

    public void Init()
    {
        CloseLanguageDropdown();
        musicToggle.SetState(GameController.Instance.useProfile.OnMusic);
        soundToggle.SetState(GameController.Instance.useProfile.OnSound);
        vibrationToggle.SetState(GameController.Instance.useProfile.OnVibration);
        musicToggle.RegisterOnSwitchedEvent(EnableGameMusic);
        soundToggle.RegisterOnSwitchedEvent(EnableGameSound);
        vibrationToggle.RegisterOnSwitchedEvent(EnableVibration);

        languageBtn.onClick.AddListener(OpenLanguageDropdown);
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
        
    }
    
    
    private void EnableGameMusic(bool isTurnOn)
    {
        GameController.Instance.useProfile.OnMusic = isTurnOn;
    }

    private void EnableGameSound(bool isTurnOn)
    {
        GameController.Instance.useProfile.OnSound = isTurnOn;
    }

    private void EnableVibration(bool isTurnOn)
    {
        GameController.Instance.useProfile.OnVibration = isTurnOn;
    }
    private void OpenLanguageDropdown()
    {
        chooseLanguageContainer.gameObject.SetActive(true);

        chooseLanguageContainer.localScale = new Vector3(1, 0, 1);

        Tween.ScaleY(chooseLanguageContainer, 1, duration: 0.3f);

        languageBtn.onClick.RemoveListener(OpenLanguageDropdown);
        languageBtn.onClick.AddListener(CloseLanguageDropdown);

        _flagItems = new LanguageItem[flagDict.Value.Count];
        int i = 0;
        foreach (Transform child in parentGroup.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var lanuage in flagDict.Value) {
            if(_flagItems[i] != null) Destroy(_flagItems[i].gameObject);
            var newFlag = Instantiate(sampleFlagButton, parentGroup.transform, false);
            newFlag.Init(lanuage, item => {
                OnSelectFlagItem(item);
                OnClickLanguage(lanuage.ToString());
            });
            newFlag.ActiveSelected(
                string.Compare(Localization.language, lanuage.ToString(), StringComparison.Ordinal) == 0);
            
            newFlag.name = lanuage.ToString();
            Debug.Log(i);
            _flagItems[i] = newFlag;
            i++;
        }
    }
    public void OnClickLanguage(string language) {
        if (string.CompareOrdinal(Localization.language, language) != 0)
        {
            //PlayerPrefs.SetString("Language", language);
            Localization.LoadCSV(Localization.loadFunction.Invoke(language), true);
            Localization.language = language;
        }

        var uiLnaguage = FindObjectsOfType<LocalizeBehaviour>();
        for (var i = 0; i < uiLnaguage.Length; i++) {
            uiLnaguage[i].OnLocalize();
        }
        
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CHANGE_LANGUAGE);
    }
    private void OnSelectFlagItem(LanguageItem item) {
        foreach (LanguageItem flagItem in _flagItems) {
            flagItem.ActiveSelected(flagItem == item);            
        }
    }
    private void CloseLanguageDropdown()
    {
        Tween.Custom(1, 0, duration: 0.2f, onValueChange: newVal =>
            {
                chooseLanguageContainerCanvasGroup.alpha = newVal;
            })
            .OnComplete(() =>
            {
                chooseLanguageContainerCanvasGroup.alpha = 1;

                chooseLanguageContainer.gameObject.SetActive(false);

                languageBtn.onClick.RemoveListener(CloseLanguageDropdown);
                languageBtn.onClick.AddListener(OpenLanguageDropdown);
                foreach (var i in _flagItems)
                {
                    Destroy(i.gameObject);
                }

                _flagItems = Array.Empty<LanguageItem>();
            });

        Tween.ScaleY(chooseLanguageContainer, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                chooseLanguageContainer.localScale = Vector3.one;
            });

        //isAbleToHideOnClickOutside = true;
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