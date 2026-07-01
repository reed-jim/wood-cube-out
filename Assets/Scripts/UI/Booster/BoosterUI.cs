using System;
using System.Collections.Generic;
using NamPhuThuy.GamePlay;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;
using static GameEnum;
using Spine.Unity;
using Spine;
using Coffee.UIExtensions;

public class BoosterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform boosterContainer;
    [SerializeField] private RectTransform addMoreScrewPortButtonRT;
    [SerializeField] private RectTransform breakModeButtonRT;
    [SerializeField] private RectTransform clearAllScrewPortsButtonRT;
    [SerializeField] private RectTransform breakModeContainer;

    [SerializeField] private Button addMoreScrewPortButton;
    [SerializeField] private Button breakObjectButton;
    public Button ShuffleButton => breakObjectButton;
    [SerializeField] private Button clearAllScrewPortsButton;
    [SerializeField] private Button disableBreakObjectModeEarlyButton;
    [SerializeField] private Button freezeTimeButton;
    public Button FreezeTimeButton => freezeTimeButton;
    [SerializeField] private Button hintButton;
    public Button HintButton => hintButton;
    [SerializeField] private Button unlockThemeButton;
    public Button UnlockThemeButton => unlockThemeButton;
    [SerializeField] private ParticleSystem fxFreezeTrail;
    [SerializeField] private CanvasGroup freezeGroup;
    [SerializeField] private SkeletonGraphic animFreeze;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private Vector2Variable canvasSize;
    [SerializeField] private Vector2Variable canvasSizeOfReferenceDevice;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private ScrewBoxesObserver screwBoxesObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private float transitionTime;
    [SerializeField] private float waitTimeBetweenClicks;

    [Header("WARNING POPUP")]
    [SerializeField] private CanvasGroup warningPopupObj;
    [SerializeField] private LeanLocalizedTextMeshProUGUI warningLocalizedText;

    public static event Action<BoosterType> useBoosterEvent;
    public static event Action addMoreScrewPortEvent;
    public static event Action enableBreakObjectModeEvent;
    public static event Action disableBreakObjectModeEvent;
    public static event Action clearAllScrewPortsEvent;
    public static event Action<BoosterType> showBuyBoosterPopupEvent;
    public static event Action<int> updateBoosterQuantityEvent;
    public static event Action shakeScrewPortsEvent;
    public static event Action<string, string> showNotificationEvent;
    public static event Action<string, string, string, string> showNotificationWithParamEvent;

    private List<Tween> _tweens;
    private int _numScrewPortsAdded;
    private bool _isInTransition;
    public bool IsInTransition => _isInTransition;
    private bool _isEnable;
    private bool _isPreventShowWarningPopup;

    private Tween _warningTween;

    private void ShowWarning(string translationKey, float duration = 2f)
    {
        if (warningPopupObj == null || warningLocalizedText == null) return;

        warningPopupObj.gameObject.SetActive(true);
        warningLocalizedText.TranslationName = translationKey;
        _warningTween = Tween.Alpha(warningPopupObj, 1f, duration: 0.5f).OnComplete(() =>
        {
            _warningTween = Tween.Delay(duration).OnComplete(() =>
            {
                Tween.Alpha(warningPopupObj, 0f, duration: 0.5f).OnComplete(() =>
                {
                    if (warningPopupObj != null)
                        warningPopupObj.gameObject.SetActive(false);
                });
            });

        });
    }

    private Vector2 _initialBoosterContainerPosition;
    private Vector2 _initialAddMoreScrewPortButtonPosition;
    private Vector2 _initialbreakObjectButtonPosition;
    private Vector2 _initialClearAllScrewPortsButtonPosition;

    private bool _isInitialized;

    private void InitializePositions()
    {
        if (_isInitialized) return;
        _initialBoosterContainerPosition = boosterContainer.localPosition;
        _initialAddMoreScrewPortButtonPosition = addMoreScrewPortButtonRT.localPosition;
        _initialbreakObjectButtonPosition = breakModeButtonRT.localPosition;
        _initialClearAllScrewPortsButtonPosition = clearAllScrewPortsButtonRT.localPosition;
        _isInitialized = true;
    }

    private void Awake()
    {
        ScrewSelectionInput.breakObjectEvent += DisableBreakObjectMode;
        LevelLoader.startLevelEvent += Reset;
        BuyBoosterPopup.useBoosterEvent += UseBooster;
        ScrewSelectionInput.breakObjectEvent += UseBreakObjectBooster;
        ScrewBoxManager.enableBoosterEvent += EnableBooster;
        ScrewManager.enableBoosterEvent += EnableBoosterWithoutWarningPopup;
        TileManager.winLevelEvent += HideFreezeGroup;
        TileManager.loseLevelEvent += HideFreezeGroup;
        ScrewBoxManager.screwPortsClearedEvent += OnAllScrewPortsCleared;
        RevivePopup.reviveEvent += OnRevived;

        BuyBoosterPopup.updateBoosterQuantityEvent += OnBoosterQuantityChanged;
        BoosterUI.updateBoosterQuantityEvent += OnBoosterQuantityChanged;
        //LevelLoader.startLevelCompletedEvent += UpdateUnlockThemeButtonState;
        //BuyBoosterPopup.unlockScrewBoxEvent += OnUnlockScrewBoxEvent;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShow;
        TileManager.updateActiveSlotsEvent += UpdateUnlockThemeButtonState;

        addMoreScrewPortButton.onClick.AddListener(() => UseBooster(BoosterType.AddScrewPort));
        breakObjectButton.onClick.AddListener(() => UseBooster(BoosterType.Shuffle));
        clearAllScrewPortsButton.onClick.AddListener(() => UseBooster(BoosterType.ClearScrewPorts));
        if (disableBreakObjectModeEarlyButton != null)
        {
            disableBreakObjectModeEarlyButton.onClick.AddListener(DisableBreakObjectModeEarly);
        }
        if (freezeTimeButton != null)
        {
            freezeTimeButton.onClick.AddListener(() => UseBooster(BoosterType.FreezeTime));
        }
        if (hintButton != null)
        {
            hintButton.onClick.AddListener(() => UseBooster(BoosterType.Hint));
        }
        if (unlockThemeButton != null)
        {
            unlockThemeButton.onClick.AddListener(() => UseBooster(BoosterType.UnlockTheme));
        }

        _tweens = new List<Tween>();

        _isEnable = true;
    }

    private void Start()
    {
        InitializePositions();
        //UpdateUnlockThemeButtonState();
    }

    void OnDestroy()
    {
        ScrewSelectionInput.breakObjectEvent -= DisableBreakObjectMode;
        LevelLoader.startLevelEvent -= Reset;
        BuyBoosterPopup.useBoosterEvent -= UseBooster;
        ScrewSelectionInput.breakObjectEvent -= UseBreakObjectBooster;
        ScrewBoxManager.enableBoosterEvent -= EnableBooster;
        ScrewManager.enableBoosterEvent -= EnableBoosterWithoutWarningPopup;
        ScrewBoxManager.screwPortsClearedEvent -= OnAllScrewPortsCleared;
        RevivePopup.reviveEvent -= OnRevived;
        TileManager.winLevelEvent -= HideFreezeGroup;
        TileManager.loseLevelEvent -= HideFreezeGroup;

        BuyBoosterPopup.updateBoosterQuantityEvent -= OnBoosterQuantityChanged;
        BoosterUI.updateBoosterQuantityEvent -= OnBoosterQuantityChanged;
        //LevelLoader.startLevelCompletedEvent -= UpdateUnlockThemeButtonState;
        //BuyBoosterPopup.unlockScrewBoxEvent -= OnUnlockScrewBoxEvent;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourceEarnPopupShow;
        TileManager.updateActiveSlotsEvent -= UpdateUnlockThemeButtonState;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void HideFreezeGroup()
    {
        if (freezeGroup != null)
        {
            freezeGroup.alpha = 0;
            freezeGroup.gameObject.SetActive(false);
        }
    }

    private void Reset()
    {
        InitializePositions();

        //Tween.LocalPosition(boosterContainer, _initialBoosterContainerPosition, duration: transitionTime);
        //Tween.LocalPosition(addMoreScrewPortButtonRT, _initialAddMoreScrewPortButtonPosition, duration: transitionTime);
        //Tween.LocalPosition(breakModeButtonRT, _initialbreakObjectButtonPosition, duration: transitionTime);
        //Tween.LocalPosition(clearAllScrewPortsButtonRT, _initialClearAllScrewPortsButtonPosition, duration: transitionTime);

        breakModeContainer.gameObject.SetActive(false);
        
        if (freezeGroup != null)
        {
            freezeGroup.alpha = 0;
            freezeGroup.gameObject.SetActive(false);
        }

        addMoreScrewPortButton.interactable = true;

        _numScrewPortsAdded = 0;
        _isInTransition = false;
        _isEnable = true;
        _isPreventShowWarningPopup = false;

        //UpdateUnlockThemeButtonState();
    }

    private void OnRevived(BoosterType boosterType)
    {
        _isEnable = false;
        _isPreventShowWarningPopup = false;

        _tweens.Add(Tween.Delay(2f).OnComplete(() =>
        {
            _isEnable = true;
            _isPreventShowWarningPopup = true;
        }));
    }

    private void UseBooster(BoosterType boosterType)
    {
        if (TileManager.Ins != null && (TileManager.Ins.IsShuffling || TileManager.Ins.IsDealingCards || TileManager.Ins.IsAnyTileDragging()))
        {
            return;
        }

        if (!_isEnable)
        {
            if (!_isPreventShowWarningPopup)
            {
                showNotificationEvent?.Invoke(GameConstants.WARNING, GameConstants.TOO_FAST_ACTION);
            }

            return;
        }

        int levelToUnlockBooster = GameConstants.GetLevelToUnlockBooster(boosterType);

        if (currentLevel.Value < GameConstants.GetLevelToUnlockBooster(boosterType))
        {
            showNotificationWithParamEvent?.Invoke(GameConstants.TIPS, "UnlockAtLevel", "level", $"{levelToUnlockBooster}");

            return;
        }

        if (boosterType == BoosterType.AddScrewPort)
        {
            AddMoreScrewPort();
        }
        else if (boosterType == BoosterType.Shuffle)
        {
            Shuffle();
        }
        else if (boosterType == BoosterType.ClearScrewPorts)
        {
            ClearAllScrewPorts();
        }
        else if (boosterType == BoosterType.FreezeTime)
        {
            UseFreezeTime();
        }
        else if (boosterType == BoosterType.Hint)
        {
            UseHint();
        }
        else if (boosterType == BoosterType.UnlockTheme)
        {
            UseBoosterUnlockTheme();
        }

        useBoosterEvent?.Invoke(boosterType);
    }

    private void AddMoreScrewPort()
    {
        if (!IsBoosterAvailable(boosterIndex: (int)BoosterType.AddScrewPort))
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.AddScrewPort);

            return;
        }

        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        ConsumeBooster(boosterIndex: (int)BoosterType.AddScrewPort);

        TileManager.Ins.UseAddHoleBooster();
        // addMoreScrewPortEvent?.Invoke();

        _numScrewPortsAdded++;

        if (_numScrewPortsAdded == 2)
        {
            addMoreScrewPortButton.interactable = false;

            Tween.LocalPositionY(addMoreScrewPortButtonRT, addMoreScrewPortButtonRT.localPosition.y - 600, duration: 0.3f);

            Tween.LocalPositionX(breakModeButtonRT, -0.7f * breakModeButtonRT.sizeDelta.x, duration: 0.3f);
            Tween.LocalPositionX(clearAllScrewPortsButtonRT, 0.7f * breakModeButtonRT.sizeDelta.x, duration: 0.3f);
        }

        Tween.Delay(waitTimeBetweenClicks).OnComplete(() =>
        {
            _isInTransition = false;
        });
    }

    private void Shuffle()
    {
        bool isBoosterAvailable = IsBoosterAvailable(boosterIndex: 1);

        if (!TileManager.Ins.CanUseShuffleBooster())
        {
            if (isBoosterAvailable)
            {
                ShowWarning("no_shuffle_available");
                return;
            }
            else
            {
                showBuyBoosterPopupEvent?.Invoke(BoosterType.Shuffle);
                return;
            }
        }

        if (!isBoosterAvailable)
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.Shuffle);

            return;
        }

        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        ConsumeBooster(boosterIndex: 1);

        TileManager.Ins.UseShuffleBooster();
        SoundManager.Instance.PlaySoundShuffleBooster();

        Tween.Delay(waitTimeBetweenClicks).OnComplete(() =>
        {
            _isInTransition = false;
        });
    }

    private void UseFreezeTime()
    {
        if (!IsBoosterAvailable(boosterIndex: 2))
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.FreezeTime);

            return;
        }

        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        ConsumeBooster(boosterIndex: 2);

        SoundManager.Instance.PlaySoundFreezeBooster();

        BoosterItemUI freezeItemUI = freezeTimeButton.GetComponentInParent<BoosterItemUI>();
        if (freezeItemUI != null)
        {
            freezeItemUI.PlayFreezeAnimation(
                onActivateEffect: () =>
                {
                    TileManager.Ins.UseFreezeTimeBooster(15f, () =>
                    {
                        Tween.Alpha(freezeGroup, 0f, duration: 2f).OnComplete(() =>
                        {
                            freezeGroup.gameObject.SetActive(false);
                        }
                        );
                    });
                },
                onComplete: () =>
                {
                    //fxFreezeTrail.gameObject.SetActive(true);
                    animFreeze.gameObject.SetActive(true);
                    //freezeItemUI.fxTrailFreeze.transform.SetParent(transform);
                    //           freezeItemUI.fxTrailFreeze.transform.position = animFreeze.transform.position;
                    //           UIParticle uiParticle = freezeItemUI.fxTrailFreeze.GetComponent<UIParticle>();
                    //           Tween.Custom(startValue: 1f, endValue: 4f, duration: 0.2f, onValueChange: newVal =>
                    //    {
                    //        uiParticle.scale = newVal;
                    //    }
                    //);

                    TrackEntry e = animFreeze.AnimationState.SetAnimation(0, "animation", false);

                    e.Complete += (TrackEntry trackEntry) =>
                    {
                        freezeGroup.gameObject.SetActive(true);

                        Tween.Alpha(freezeGroup, 1f, duration: 2f).OnComplete(() =>
                        {

                            animFreeze.gameObject.SetActive(false);
                            //freezeItemUI.fxTrailFreeze.transform.SetParent(freezeItemUI.boosterIcon.transform);
                            //uiParticle.scale = 1f;
                            //freezeItemUI.fxTrailFreeze.transform.localPosition = Vector3.zero;
                        }
                        );
                        Tween.Delay(waitTimeBetweenClicks).OnComplete(() =>
                        {

                            _isInTransition = false;
                        });
                    };

                }
            );
        }
        else
        {
            TileManager.Ins.UseFreezeTimeBooster(15f);
            Tween.Delay(waitTimeBetweenClicks).OnComplete(() =>
            {
                _isInTransition = false;
            });
        }
    }



    private void UseHint()
    {
        if (TileManager.Ins != null && TileManager.Ins.IsAnyThemeCompleting())
        {
            return;
        }

        bool isBoosterAvailable = IsBoosterAvailable(boosterIndex: 0);

        if (TileManager.Ins != null && !TileManager.Ins.CanUseHintBooster())
        {
            if (isBoosterAvailable)
            {
                ShowWarning("no_hint_available");
                return;
            }
            else
            {
                showBuyBoosterPopupEvent?.Invoke(BoosterType.Hint);
                return;
            }
        }

        if (!isBoosterAvailable)
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.Hint);

            return;
        }

        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        ConsumeBooster(boosterIndex: 0);

        TileManager.Ins.UseHintBooster();
        SoundManager.Instance.PlaySoundHintBooster();

        Tween.Delay(waitTimeBetweenClicks).OnComplete(() =>
        {
            _isInTransition = false;
        });
    }

    private void UseBoosterUnlockTheme()
    {
        //if (TileManager.Ins == null || !TileManager.Ins.CanUseUnlockThemeBooster())
        //{
        //    ShowWarning("no_unlock_available");
        //    return;
        //}

        bool isBoosterAvailable = IsBoosterAvailable(boosterIndex: (int)BoosterType.UnlockTheme);

        if (TileManager.Ins.remainingSlotContainer <= 0)
        {
            ShowWarning("no_unlock_available");

            if (isBoosterAvailable)
            {
                // Khi có booster nhưng đã mở khóa hết thì mới ko cho ấn vào nữa
                return;
            }
            else
            {
                // Khi hết booster nhưng đã mở khóa hết thì vẫn phải ấn được nút để mua booster
                showBuyBoosterPopupEvent?.Invoke(BoosterType.UnlockTheme);
                return;
            }
        }

        if (!isBoosterAvailable)
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.UnlockTheme);
            return;
        }

        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        ConsumeBooster(boosterIndex: (int)BoosterType.UnlockTheme);

        TileSlotContainer targetContainer = TileManager.Ins.GetNextLockedTileSlotContainer();

        BoosterItemUI unlockItemUI = unlockThemeButton.GetComponentInParent<BoosterItemUI>();
        if (unlockItemUI != null && targetContainer != null)
        {
            unlockItemUI.PlayUnlockThemeAnimation(
                targetContainer.transform,
                onActivateEffect: () =>
                {
                    SoundManager.Instance.PlaySoundAddSlot();

                    TileManager.Ins.UseUnlockThemeBooster();
                    //UpdateUnlockThemeButtonState();
                },
                onComplete: () =>
                {
                    Tween.Delay(0.1f).OnComplete(() =>
                    {
                        _isInTransition = false;
                    });
                }
            );
        }
        else
        {
            TileManager.Ins.UseUnlockThemeBooster();
            //UpdateUnlockThemeButtonState();
            Tween.Delay(0.1f).OnComplete(() =>
            {
                _isInTransition = false;
            });
        }
    }

    private void UpdateUnlockThemeButtonState()
    {
        if (unlockThemeButton == null) return;
        if (TileManager.Ins == null) return;

        bool canUseUnlockThemeBooster = TileManager.Ins.CanUseUnlockThemeBooster();
        if (!canUseUnlockThemeBooster)
        {
            //unlockThemeButton.interactable = false;
            //Show thong bao
            return;
        }

        bool hasLockedTheme = TileManager.Ins.NumLockedTileSlotContainer > 0;
        bool isBoosterAvailable = IsBoosterAvailable((int)BoosterType.UnlockTheme);

        if (!hasLockedTheme)
        {
            if (isBoosterAvailable)
            {
                //Show thong bao
                // Khi có booster nhưng đã mở khóa hết thì mới ko cho ấn vào nữa
                //unlockThemeButton.interactable = false;
            }
            else
            {
                // Khi hết booster nhưng đã mở khóa hết thì vẫn phải ấn được nút để mua booster
                unlockThemeButton.interactable = true;
            }
        }
        else
        {
            // Nếu vẫn còn theme bị khóa, nút luôn luôn ấn được (dùng booster hoặc mua booster)
            unlockThemeButton.interactable = true;
        }
    }

    private void OnBoosterQuantityChanged(int boosterIndex)
    {
        if (boosterIndex == (int)BoosterType.UnlockTheme)
        {
            UpdateUnlockThemeButtonState();
        }
    }

    private void OnUnlockScrewBoxEvent()
    {
        Tween.Delay(0.1f).OnComplete(UpdateUnlockThemeButtonState);
    }

    private void OnResourceEarnPopupShow(bool isShow)
    {
        if (!isShow)
        {
            UpdateUnlockThemeButtonState();
        }
    }

    private void EnableBooster(bool isEnable)
    {
        _isEnable = isEnable;
    }

    private void EnableBoosterWithoutWarningPopup(bool isEnable)
    {
        _isEnable = isEnable;
        _isPreventShowWarningPopup = true;
    }

    private void EnableBreakObjectMode()
    {
        if (!IsBoosterAvailable(boosterIndex: 1))
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.Shuffle);

            return;
        }

        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        breakModeContainer.gameObject.SetActive(true);

        Tween.LocalPositionY(boosterContainer, -canvasSizeOfReferenceDevice.Value.y, duration: transitionTime);
        Tween.LocalPositionY(breakModeContainer, _initialBoosterContainerPosition.y + 0.5f * breakModeContainer.sizeDelta.y, duration: transitionTime)
        .OnComplete(() =>
        {
            enableBreakObjectModeEvent?.Invoke();

            _isInTransition = false;
        });
    }

    private void DisableBreakObjectMode()
    {
        Tween.LocalPositionY(boosterContainer, _initialBoosterContainerPosition.y, duration: transitionTime);
        Tween.LocalPositionY(breakModeContainer, -canvasSizeOfReferenceDevice.Value.y, duration: transitionTime)
        .OnComplete(() =>
        {
            breakModeContainer.gameObject.SetActive(false);
        });
    }

    private void DisableBreakObjectModeEarly()
    {
        DisableBreakObjectMode();

        disableBreakObjectModeEvent?.Invoke();
    }

    private void ClearAllScrewPorts()
    {
        if (!TileManager.Ins.IsAnyTileOnShelf())
        {
            // shakeScrewPortsEvent?.Invoke();
            showNotificationEvent?.Invoke(GameConstants.TIPS, GameConstants.NO_SCREW_TO_BE_CLEARED);

            return;
        }

        if (!IsBoosterAvailable(boosterIndex: 2))
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.ClearScrewPorts);

            return;
        }

        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        TileManager.Ins.UseSweeperBooster();
        // clearAllScrewPortsEvent?.Invoke();

        SoundManager.Instance.PlaySoundClearScrewPorts();

        Tween.Delay(waitTimeBetweenClicks).OnComplete(() =>
        {
            _isInTransition = false;
        });
    }

    private bool IsBoosterAvailable(int boosterIndex)
    {
        userResourcesObserver.Load();

        if (userResourcesObserver.UserResources.BoosterQuantities[boosterIndex] > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ConsumeBooster(int boosterIndex)
    {
        userResourcesObserver.ConsumeBooster(boosterIndex: boosterIndex);
        levelBoosterObserver.UseBooster(boosterIndex);

        updateBoosterQuantityEvent?.Invoke(boosterIndex);

        BoosterType boosterType = (BoosterType)boosterIndex;

        SaferioTracking.TrackBoosterUsage(boosterType);
        SaferioTracking.TrackResourceSink(
            currentLevel.Value, boosterType.ToString(),
            1, userResourcesObserver.UserResources.BoosterQuantities[boosterIndex], $"Consume_Booster_{boosterType}");
    }

    private void UseBreakObjectBooster()
    {
        ConsumeBooster(boosterIndex: 1);
    }

    private void OnAllScrewPortsCleared(int numScrewCleared)
    {
        if (numScrewCleared > 0)
        {
            ConsumeBooster(boosterIndex: 2);
        }
        else
        {
            showNotificationEvent?.Invoke(GameConstants.TIPS, GameConstants.NO_SCREW_TO_BE_CLEARED);
        }
    }
}
