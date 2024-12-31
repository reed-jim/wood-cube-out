using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private Vector2Variable canvasSize;

    [Header("CUSTOMIZE")]
    [SerializeField] private float transitionTime;
    [SerializeField] private float waitTimeBetweenClicks;

    public static event Action addMoreScrewPortEvent;
    public static event Action enableBreakObjectModeEvent;
    public static event Action clearAllScrewPortsEvent;

    private int _numScrewPortsAdded;
    private bool _isInTransition;

    private Vector2 _initialBoosterContainerPosition;
    private Vector2 _initialAddMoreScrewPortButtonPosition;
    private Vector2 _initialbreakObjectButtonPosition;
    private Vector2 _initialClearAllScrewPortsButtonPosition;

    private void Awake()
    {
        ScrewSelectionInput.breakObjectEvent += DisableBreakObjectMode;
        LevelLoader.startLevelEvent += Reset;

        addMoreScrewPortButton.onClick.AddListener(AddMoreScrewPort);
        breakObjectButton.onClick.AddListener(EnableBreakObjectMode);
        clearAllScrewPortsButton.onClick.AddListener(ClearAllScrewPorts);

        _initialBoosterContainerPosition = boosterContainer.localPosition;
        _initialAddMoreScrewPortButtonPosition = addMoreScrewPortButtonRT.localPosition;
        _initialbreakObjectButtonPosition = breakModeButtonRT.localPosition;
        _initialClearAllScrewPortsButtonPosition = clearAllScrewPortsButtonRT.localPosition;
    }

    void OnDestroy()
    {
        ScrewSelectionInput.breakObjectEvent -= DisableBreakObjectMode;
        LevelLoader.startLevelEvent -= Reset;
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
    }

    private void AddMoreScrewPort()
    {
        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        addMoreScrewPortEvent?.Invoke();

        _numScrewPortsAdded++;

        if (_numScrewPortsAdded == 2)
        {
            addMoreScrewPortButton.interactable = false;

            Tween.LocalPositionY(addMoreScrewPortButtonRT, addMoreScrewPortButtonRT.localPosition.y - 600, duration: 0.3f);

            Tween.LocalPositionX(breakModeButtonRT, -0.2f * canvasSize.Value.x, duration: 0.3f);
            Tween.LocalPositionX(clearAllScrewPortsButtonRT, 0.2f * canvasSize.Value.x, duration: 0.3f);
        }

        Tween.Delay(waitTimeBetweenClicks).OnComplete(() =>
        {
            _isInTransition = false;
        });
    }

    private void EnableBreakObjectMode()
    {
        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        breakModeContainer.gameObject.SetActive(true);

        Tween.LocalPositionY(boosterContainer, -canvasSize.Value.y, duration: transitionTime);
        Tween.LocalPositionY(breakModeContainer, _initialBoosterContainerPosition.y, duration: transitionTime)
        .OnComplete(() =>
        {
            enableBreakObjectModeEvent?.Invoke();

            _isInTransition = false;
        });
    }

    private void DisableBreakObjectMode()
    {
        Tween.LocalPositionY(boosterContainer, _initialBoosterContainerPosition.y, duration: transitionTime);
        Tween.LocalPositionY(breakModeContainer, -canvasSize.Value.y, duration: transitionTime)
        .OnComplete(() =>
        {
            breakModeContainer.gameObject.SetActive(false);
        });
    }

    private void ClearAllScrewPorts()
    {
        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        clearAllScrewPortsEvent?.Invoke();

        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.CLEAR_SCREW_PORTS_SOUND);

        sound.Play();

        Tween.Delay(waitTimeBetweenClicks).OnComplete(() =>
        {
            _isInTransition = false;
        });
    }
}
