using System;
using System.Collections.Generic;
using BirdSort2;
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
    [SerializeField] private Vector2Variable canvasSize;
    [SerializeField] private Vector2Variable canvasSizeOfReferenceDevice;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private ScrewBoxesObserver screwBoxesObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private float transitionTime;
    [SerializeField] private float waitTimeBetweenClicks;

    public static event Action addMoreScrewPortEvent;
    public static event Action enableBreakObjectModeEvent;
    public static event Action disableBreakObjectModeEvent;
    public static event Action clearAllScrewPortsEvent;
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
    }

    private void Start()
    {
       
    }

    void OnDestroy()
    {
      
    }

    private void Reset()
    {
        
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

    
}
