using System;
using System.Collections.Generic;
using NamPhuThuy.Common;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

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
    [SerializeField] private Button disableBreakObjectModeEarlyButton;

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

    public static event Action addMoreScrewPortEvent;
    public static event Action magicPickPieceEvent;
    public static event Action enableBreakObjectModeEvent;
    public static event Action disableBreakObjectModeEvent;
    public static event Action clearAllScrewPortsEvent;
    public static event Action<BoosterType> showBuyBoosterPopupEvent;
    public static event Action<BoosterType> updateBoosterQuantityEvent;
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
        YarnLevelLoader.startLevelEvent += Reset;
        BuyBoosterPopup.useBoosterEvent += UseBooster;
        SpoolManager.enableBoosterEvent += EnableBooster;
        PieceManager.enableBoosterEvent += EnableBoosterWithoutWarningPopup;
        SpoolManager.piecePortsClearedEvent += OnAllScrewPortsCleared;
        RevivePopup.reviveEvent += OnRevived;

        addMoreScrewPortButton.onClick.AddListener(() => UseBooster(BoosterType.AddSpoolSlot));
        breakObjectButton.onClick.AddListener(() => UseBooster(BoosterType.MagicPick));
        clearAllScrewPortsButton.onClick.AddListener(() => UseBooster(BoosterType.ClearAlternativeSpools));
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
    }

    void OnDestroy()
    {
        ScrewSelectionInput.breakObjectEvent -= DisableBreakObjectMode;
        YarnLevelLoader.startLevelEvent -= Reset;
        BuyBoosterPopup.useBoosterEvent -= UseBooster;
        SpoolManager.enableBoosterEvent -= EnableBooster;
        PieceManager.enableBoosterEvent -= EnableBoosterWithoutWarningPopup;
        SpoolManager.piecePortsClearedEvent -= OnAllScrewPortsCleared;
        RevivePopup.reviveEvent -= OnRevived;

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
        DebugLogger.Log(message: $"{boosterType}");
        if (!_isEnable)
        {
            if (!_isPreventShowWarningPopup)
            {
                showNotificationEvent?.Invoke(GameConstants.WARNING, GameConstants.TOO_FAST_ACTION);
            }

            return;
        }

        if (boosterType == BoosterType.AddSpoolSlot)
        {
            DebugLogger.Log();
            AddMoreScrewPort();
        }
        else if (boosterType == BoosterType.MagicPick)
        {
            MagicPick();
        }
        else if (boosterType == BoosterType.ClearAlternativeSpools)
        {
            ClearAllScrewPorts();
        }
    }

    private void AddMoreScrewPort()
    {
        if (!IsBoosterAvailable(boosterIndex: 0))
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.AddSpoolSlot);

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

        ConsumeBooster(BoosterType.AddSpoolSlot);

        addMoreScrewPortEvent?.Invoke();
        DebugLogger.Log(message: $"Invoked AddMoreScrewPort Event", setBold: true);

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

    private void EnableBooster(bool isEnable)
    {
        _isEnable = isEnable;
    }

    private void EnableBoosterWithoutWarningPopup(bool isEnable)
    {
        _isEnable = isEnable;
        _isPreventShowWarningPopup = true;
    }

    private void MagicPick()
    {
        if (!IsBoosterAvailable(boosterIndex: 1))
        {
            // showBuyBoosterPopupEvent?.Invoke(BoosterType.BreakObject);

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

        ConsumeBooster(BoosterType.MagicPick);

        magicPickPieceEvent?.Invoke();

        Tween.Delay(waitTimeBetweenClicks).OnComplete(() =>
        {
            _isInTransition = false;
        });
    }

    private void EnableBreakObjectMode()
    {
        if (!IsBoosterAvailable(boosterIndex: 1))
        {
            // showBuyBoosterPopupEvent?.Invoke(BoosterType.BreakObject);

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
        if (screwBoxesObserver.NumScrewInScrewPorts == 0)
        {
            // shakeScrewPortsEvent?.Invoke();
            showNotificationEvent?.Invoke(GameConstants.TIPS, GameConstants.NO_SCREW_TO_BE_CLEARED);

            return;
        }

        if (!IsBoosterAvailable(boosterIndex: 2))
        {
            // showBuyBoosterPopupEvent?.Invoke(BoosterType.ClearScrewPorts);

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

        clearAllScrewPortsEvent?.Invoke();

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

    private void ConsumeBooster(BoosterType boosterType)
    {
        userResourcesObserver.ConsumeBooster(boosterType);
        levelBoosterObserver.UseBooster(boosterType);

        updateBoosterQuantityEvent?.Invoke(boosterType);

        SaferioTracking.TrackBoosterUsage(boosterType);
        SaferioTracking.TrackResourceSink(
            currentLevel.Value, boosterType.ToString(),
            1, userResourcesObserver.UserResources.BoosterQuantities[(int)boosterType], $"Consume_Booster_{boosterType}");
    }

    private void OnAllScrewPortsCleared(int numScrewCleared)
    {
        if (numScrewCleared > 0)
        {
            ConsumeBooster(BoosterType.ClearAlternativeSpools);
        }
        else
        {
            showNotificationEvent?.Invoke(GameConstants.TIPS, GameConstants.NO_SCREW_TO_BE_CLEARED);
        }
    }
}
