using System;
using System.Collections.Generic;
using AntsFlow;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public enum BoosterMode
{
    None,
    EnableAllAntBoxes,
    ClearOneColor
}

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
    [SerializeField] private Button clearAllScrewPortsButton;
    [SerializeField] private Button clearOneColorButton;
    [SerializeField] private Button disableBreakObjectModeEarlyButton;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private Vector2Variable canvasSize;
    [SerializeField] private Vector2Variable canvasSizeOfReferenceDevice;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private ScrewBoxesObserver screwBoxesObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    [Header("Booster Anim")]
    [SerializeField] private SkeletonGraphic skeletonGraphic;
    [SpineAnimation] public string appearAnim;
    [SpineAnimation] public string ideaAnim;
    [SpineSlot] public string slotName;
    [SpineAttachment] public string[] boosterAttachments;

    [Header("CUSTOMIZE")]
    [SerializeField] private float transitionTime;
    [SerializeField] private float waitTimeBetweenClicks;

    public static event Action addMoreScrewPortEvent;
    public static event Action enableBreakObjectModeEvent;
    public static event Action disableBreakObjectModeEvent;
    public static event Action clearAllScrewPortsEvent;
    public static event Action<BoosterType> showBuyBoosterPopupEvent;
    public static event Action<int> updateBoosterQuantityEvent;
    public static event Action shakeScrewPortsEvent;
    public static event Action<string, string> showNotificationEvent;

    private List<Tween> _tweens;
    private int _numScrewPortsAdded;
    private bool _isInTransition;
    private bool _isEnable;
    private bool _isPreventShowWarningPopup;

    private Vector2 _initialBoosterContainerPosition;
    private Vector2 _initialAddMoreScrewPortButtonPosition;
    private Vector2 _initialbreakObjectButtonPosition;
    private Vector2 _initialClearAllScrewPortsButtonPosition;

    private void Awake()
    {
        ScrewSelectionInput.breakObjectEvent += DisableBreakObjectMode;
        AntsFlow.BaseTile.useClearOneColorBoosterEvent += ConfirmClearOneColor;
        LevelLoader.startLevelEvent += Reset;
        BuyBoosterPopup.useBoosterEvent += UseBooster;
        ScrewSelectionInput.breakObjectEvent += UseBreakObjectBooster;
        ScrewBoxManager.enableBoosterEvent += EnableBooster;
        ScrewManager.enableBoosterEvent += EnableBoosterWithoutWarningPopup;
        ScrewBoxManager.screwPortsClearedEvent += OnAllScrewPortsCleared;
        RevivePopup.reviveEvent += OnRevived;
        WashingMachineManager.finishUsingEnableAllAntBoxesBoosterEvent += ShowBooster;

        addMoreScrewPortButton.onClick.AddListener(() => UseBooster(BoosterType.AddScrewPort));
        breakObjectButton.onClick.AddListener(() => UseBooster(BoosterType.BreakObject));
        clearAllScrewPortsButton.onClick.AddListener(() => UseBooster(BoosterType.ClearScrewPorts));
        clearOneColorButton.onClick.AddListener(() => UseBooster(BoosterType.ClearOneColor));
        disableBreakObjectModeEarlyButton.onClick.AddListener(DisableBreakObjectModeEarly);

        _tweens = new List<Tween>();

        _isEnable = true;
    }

    private void Start()
    {
        _initialBoosterContainerPosition = boosterContainer.localPosition;
        _initialAddMoreScrewPortButtonPosition = addMoreScrewPortButtonRT.localPosition;
        _initialbreakObjectButtonPosition = breakModeButtonRT.localPosition;
        _initialClearAllScrewPortsButtonPosition = clearAllScrewPortsButtonRT.localPosition;

        skeletonGraphic.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        ScrewSelectionInput.breakObjectEvent -= DisableBreakObjectMode;
        AntsFlow.BaseTile.useClearOneColorBoosterEvent -= ConfirmClearOneColor;
        LevelLoader.startLevelEvent -= Reset;
        BuyBoosterPopup.useBoosterEvent -= UseBooster;
        ScrewSelectionInput.breakObjectEvent -= UseBreakObjectBooster;
        ScrewBoxManager.enableBoosterEvent -= EnableBooster;
        ScrewManager.enableBoosterEvent -= EnableBoosterWithoutWarningPopup;
        ScrewBoxManager.screwPortsClearedEvent -= OnAllScrewPortsCleared;
        RevivePopup.reviveEvent -= OnRevived;
        WashingMachineManager.finishUsingEnableAllAntBoxesBoosterEvent -= ShowBooster;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void Reset()
    {
        Tween.LocalPosition(boosterContainer, _initialBoosterContainerPosition, duration: transitionTime);
        Tween.LocalPosition(addMoreScrewPortButtonRT, _initialAddMoreScrewPortButtonPosition, duration: transitionTime);
        Tween.LocalPosition(breakModeButtonRT, _initialbreakObjectButtonPosition, duration: transitionTime);
        Tween.LocalPosition(clearAllScrewPortsButtonRT, _initialClearAllScrewPortsButtonPosition, duration: transitionTime);

        breakModeContainer.gameObject.SetActive(false);

        addMoreScrewPortButton.interactable = true;

        _numScrewPortsAdded = 0;
        _isInTransition = false;
        _isEnable = true;
        _isPreventShowWarningPopup = false;
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
        if (!_isEnable)
        {
            if (!_isPreventShowWarningPopup)
            {
                showNotificationEvent?.Invoke(GameConstants.WARNING, GameConstants.TOO_FAST_ACTION);
            }

            return;
        }

        if (IsBoosterAvailable(GetBoosterIndex(boosterType)))
        {
            PlayPreBoosterAnimation(boosterType, onCompleteAction: () =>
            {
                UseBooster(boosterType);
            });
        }
        else
        {
            UseBooster(boosterType);
        }

        void UseBooster(BoosterType boosterType)
        {
            if (boosterType == BoosterType.AddScrewPort)
            {
                AddMoreScrewPort();
            }
            else if (boosterType == BoosterType.BreakObject)
            {
                EnableAllAntBoxes();
            }
            else if (boosterType == BoosterType.ClearScrewPorts)
            {
                Shuffle();
            }
            else if (boosterType == BoosterType.ClearOneColor)
            {
                EnableClearOneColorMode();
            }
        }
    }

    private int GetBoosterIndex(BoosterType boosterType)
    {
        int boosterIndex = 0;

        if (boosterType == BoosterType.BreakObject)
        {
            boosterIndex = 1;
        }
        else if (boosterType == BoosterType.ClearScrewPorts)
        {
            boosterIndex = 2;
        }
        else
        {
            boosterIndex = 3;
        }

        return boosterIndex;
    }

    private void PlayPreBoosterAnimation(BoosterType boosterType, Action onCompleteAction)
    {
        skeletonGraphic.gameObject.SetActive(true);
        skeletonGraphic.AnimationState.SetAnimation(0, appearAnim, false);

        for (int i = 0; i < boosterAttachments.Length; i++)
        {
            skeletonGraphic.Skeleton.SetAttachment(boosterAttachments[i], null);
        }

        int boosterIndex = GetBoosterIndex(boosterType);

        skeletonGraphic.Skeleton.SetAttachment(boosterAttachments[boosterIndex], boosterAttachments[boosterIndex]);

        Tween.Delay(0.667f).OnComplete(() =>
        {
            skeletonGraphic.AnimationState.SetAnimation(0, ideaAnim, false);

            Tween.Delay(1.667f).OnComplete(() =>
            {
                Tween.Alpha(skeletonGraphic, 0, duration: 0.3f).OnComplete(() =>
                {
                    skeletonGraphic.color = Color.white;
                    skeletonGraphic.gameObject.SetActive(false);

                    onCompleteAction?.Invoke();
                });
            });
        });
    }

    private void AddMoreScrewPort()
    {
        if (!IsBoosterAvailable(boosterIndex: 0))
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

        ConsumeBooster(boosterIndex: 0);

        WashingMachineManager.Ins.UnlockWashingMachineSlot();

        // _numScrewPortsAdded++;

        // if (_numScrewPortsAdded == 2)
        // {
        //     addMoreScrewPortButton.interactable = false;

        //     Tween.LocalPositionY(addMoreScrewPortButtonRT, addMoreScrewPortButtonRT.localPosition.y - 600, duration: 0.3f);

        //     Tween.LocalPositionX(breakModeButtonRT, -0.7f * breakModeButtonRT.sizeDelta.x, duration: 0.3f);
        //     Tween.LocalPositionX(clearAllScrewPortsButtonRT, 0.7f * breakModeButtonRT.sizeDelta.x, duration: 0.3f);
        // }

        Tween.Delay(waitTimeBetweenClicks).OnComplete(() =>
        {
            _isInTransition = false;
        });
    }

    private void Shuffle()
    {
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

        ConsumeBooster(boosterIndex: 1);

        WashingMachineManager.Ins.UseBoosterShuffle();

        Tween.Delay(waitTimeBetweenClicks).OnComplete(() =>
        {
            _isInTransition = false;
        });
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

    private void EnableClearOneColorMode()
    {
        if (!IsBoosterAvailable(boosterIndex: 3))
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.ClearOneColor);

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
            WaterFlowGameInput.Ins.inputMode = AntsFlowInputMode.SelectTile;

            _isInTransition = false;
        });
    }

    private void ConfirmClearOneColor(Color faction)
    {
        ConsumeBooster(boosterIndex: 3);

        DisableBreakObjectMode();
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

    private void ShowBooster()
    {
        Tween.LocalPositionY(boosterContainer, _initialBoosterContainerPosition.y, duration: transitionTime);
    }

    private void HideBooster()
    {
        Tween.LocalPositionY(boosterContainer, -canvasSizeOfReferenceDevice.Value.y, duration: transitionTime);
    }

    private void EnableAllAntBoxes()
    {
        if (!IsBoosterAvailable(boosterIndex: 1))
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.BreakObject);

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

        HideBooster();

        WashingMachineManager.Ins.UseBoosterEnableAllAntBoxes();
        // WashingMachineManager.Ins.UseMagicBooster();

        SoundManager.Instance.PlaySoundClearScrewPorts();

        Tween.Delay(waitTimeBetweenClicks).OnComplete(() =>
        {
            _isInTransition = false;
        });

        GamePersistentVariable.boosterMode = BoosterMode.EnableAllAntBoxes;
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
