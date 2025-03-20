using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

public class BaseScrew : MonoBehaviour, IScrew
{
    [SerializeField] protected int screwId;

    #region COMPONENT
    [SerializeField] protected ScrewServiceLocator screwServiceLocator;
    [SerializeField] protected MeshFilter meshFilter;
    [SerializeField] protected HingeJoint joint;
    #endregion

    [Header("CUSTOMIZE")]
    [SerializeField] protected float scaleOnScrewBox;
    [SerializeField] private int screwDifficultyModifier;
    [SerializeField] protected float rotateSpeed = 0.3f;

    [Header("CUSTOMIZE - UNSCREW ANIMATION")]
    [SerializeField] private float durationMoveOut = 0.3f;
    [SerializeField] private float durationDelay = 0.1f;
    [SerializeField] private float durationMoveToScrewPort = 0.3f;
    [SerializeField] private float durationTightenScrewPort = 0.3f;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] protected GameSetting gameSetting;

    #region PRIVATE FIELD
    protected List<Tween> _tweens = new List<Tween>();
    protected Camera _mainCamera;
    protected Renderer _renderer;
    protected int _numberBlockingObjects;
    protected Vector3 _initialScale;
    protected float _initialRotateSpeed;
    protected bool _isMoving;
    protected bool _isRotating;
    protected bool _isRotatingToCorrectAngleOnScrewBox;
    protected Vector3 _localRotationOnScrewSlot;
    protected bool _isInteractable = true;
    protected bool _isDone;
    protected bool _isOutsideWithNoScrewBox;
    protected bool _isLocked;
    protected bool _isInScrewPort;
    protected ScrewBoxSlot _screwBoxSlot;
    protected ScrewData _screwData;
    #endregion

    #region PROPERTY
    public int ScrewId
    {
        get => screwId;
        set => screwId = value;
    }

    public HingeJoint Joint
    {
        set => joint = value;
    }

    public MeshFilter MeshFilter
    {
        get => meshFilter;
    }

    public ScrewBoxSlot ScrewBoxSlot
    {
        get => _screwBoxSlot;
    }

    public ScrewData ScrewData
    {
        get => _screwData;
    }

    public ScrewServiceLocator ScrewServiceLocator
    {
        get => screwServiceLocator;
    }

    public GameFaction Faction
    {
        get => screwServiceLocator.screwFaction.Faction;
        set => screwServiceLocator.screwFaction.Faction = value;
    }

    public int NumberBlockingObjects
    {
        get => _numberBlockingObjects;
    }

    public bool IsMoving
    {
        get => _isMoving;
        set => _isMoving = value;
    }

    public bool IsRotating
    {
        get => _isRotating;
        set => _isRotating = value;
    }

    public bool IsDone
    {
        get => _isDone;
        set => _isDone = value;
    }

    public bool IsInteractable
    {
        get => _isInteractable;
        set => _isInteractable = value;
    }

    public bool IsOutsideWithNoScrewBox
    {
        get => _isOutsideWithNoScrewBox;
        set => _isOutsideWithNoScrewBox = value;
    }

    public bool IsLocked
    {
        get => _isLocked;
        set => _isLocked = value;
    }

    public bool IsInScrewPort
    {
        get => _isInScrewPort;
        set => _isInScrewPort = value;
    }

    public int ScrewDifficultyModifier
    {
        get => screwDifficultyModifier;
    }
    #endregion

    #region EVENT
    public static event Action<int, GameFaction> selectScrewEvent;
    public static event Action<int, GameFaction> autoSelectScrewEvent;
    public static event Action<BaseScrew> addScrewToListEvent;
    public static event Action<BaseScrew> screwMovedCompletedEvent;
    public static event Action<int> breakJointEvent;
    public static event Action<int> screwStartLoosenedEvent;
    public static event Action screwLoosenedEvent;
    public static event Action updateOutsideScrewNumberEvent;
    #endregion

    #region LIFE CYCLE
    protected virtual void Awake()
    {
        ScrewBoxManager.looseScrewEvent += Loose;
        ScrewBoxManager.looseScrewWithoutCheckingObstaclesEvent += LooseWithoutCheckingObstacles;

        RegisterMoreEvent();

        AddScrewToList();

        _renderer = GetComponent<Renderer>();

        _initialScale = transform.localScale;
        _initialRotateSpeed = rotateSpeed;

        _mainCamera = Camera.main;

        MoreLogicInAwake();
    }

    void OnDestroy()
    {
        ScrewBoxManager.looseScrewEvent -= Loose;
        ScrewBoxManager.looseScrewWithoutCheckingObstaclesEvent -= LooseWithoutCheckingObstacles;

        UnregisterMoreEvent();

        CommonUtil.StopAllTweens(_tweens);
    }

    protected virtual void MoreLogicInAwake()
    {

    }

    protected virtual void RegisterMoreEvent()
    {

    }

    protected virtual void UnregisterMoreEvent()
    {

    }
    #endregion

    public void Select()
    {
        selectScrewEvent?.Invoke(screwId, Faction);
    }

    public void AutoSelect()
    {
        autoSelectScrewEvent?.Invoke(screwId, Faction);
    }

    public virtual void Loose(int screwId, GameFaction faction, ScrewBoxSlot screwBoxSlot)
    {

    }

    public virtual void Loose(int screwId, GameFaction faction, ScrewBoxSlot screwBoxSlot, bool isCheckObstacles)
    {

    }

    public virtual void LooseWithoutCheckingObstacles(int screwId, GameFaction faction, ScrewBoxSlot screwBoxSlot)
    {
        Loose(screwId, faction, screwBoxSlot, isCheckObstacles: false);
    }

    public void UnscrewAnimationAsync(ScrewBoxSlot screwBoxSlot, Action onCompleteAction = null, bool isSwitchCamera = true)
    {
        if (_isMoving)
        {
            return;
        }

        screwStartLoosenedEvent?.Invoke(gameObject.GetInstanceID());

        _isMoving = true;
        _isRotating = true;

        _screwBoxSlot = screwBoxSlot;

        Vector3 screwSize = TransformUtil.ComponentWiseMultiply(transform.lossyScale, meshFilter.mesh.bounds.size);

        // // FOR ROTATING
        // await Task.Delay(100);

        // LOOSE FX
        if (!_isInScrewPort && !_isOutsideWithNoScrewBox)
        {
            ParticleSystem looseScrewFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX);
            ParticleSystem ringFx = looseScrewFx.transform.GetChild(0).GetComponent<ParticleSystem>();

            var looseScrewFxMain = looseScrewFx.main;
            var ringFxMain = ringFx.main;

            Color factionColor = FactionUtility.GetColorForFaction(Faction);

            looseScrewFxMain.startColor = 1.5f * factionColor;
            ringFxMain.startColor = 1.5f * factionColor;

            looseScrewFx.gameObject.SetActive(true);

            looseScrewFx.transform.position = transform.position + 0.7f * screwSize.z * transform.forward;
            looseScrewFx.transform.rotation = transform.rotation;

            looseScrewFx.Play();
        }
        //

        if (IsOutsideWithNoScrewBox)
        {
            Vector3 initialScale = transform.localScale;

            transform.localScale = Vector3.zero;

            _tweens.Add(Tween.Scale(transform, initialScale, duration: 0.3f));
        }

        _tweens.Add(Tween.Position(transform, transform.position + screwSize.z * transform.forward, duration: durationMoveOut, ease: Ease.Linear).OnComplete(() =>
        {
            _tweens.Add(Tween.Delay(duration: durationDelay).OnComplete(() =>
            {
                transform.SetParent(screwBoxSlot.transform);

                transform.localPosition = transform.localPosition.ChangeZ(-10);

                _isRotating = false;

                float timeMultiplier = Vector3.Distance(new Vector3(0, 0.3f, -0.3f), transform.localPosition);
                timeMultiplier = Mathf.Min(timeMultiplier, 8);

                _localRotationOnScrewSlot = new Vector3(-30f, 180 + 0f, 0);
                Vector3 finalLocalPosition = new Vector3(0, -0.1f, 0.2f);

                if (screwBoxSlot.IsScrewPort)
                {
                    _localRotationOnScrewSlot = new Vector3(-20f, 180 + 0f, 0);
                    finalLocalPosition = new Vector3(0, -0.05f, 0.2f);
                }

                _tweens.Add(Tween.LocalRotation(transform, Quaternion.Euler(_localRotationOnScrewSlot), duration: timeMultiplier * durationMoveToScrewPort, ease: Ease.Linear));
                _tweens.Add(Tween.Scale(transform, scaleOnScrewBox, duration: timeMultiplier * durationMoveToScrewPort, ease: Ease.Linear));
                _tweens.Add(Tween.LocalPosition(transform, new Vector3(0, 0.4f, -1.6f), duration: timeMultiplier * durationMoveToScrewPort, ease: Ease.Linear)
                .OnComplete(() =>
                {
                    // _tweens.Add(Tween.LocalRotation(transform, Quaternion.Euler(new Vector3(-30f, 180 + 0f, 0)), duration: durationDelay, ease: Ease.Linear));
                    _tweens.Add(Tween.Delay(duration: 0)
                    .OnComplete(() =>
                    {
                        _isRotating = true;
                        rotateSpeed *= 2;

                        if (!GamePersistentVariable.isSoundTightenScrewPlaying && !_isInScrewPort && !_isOutsideWithNoScrewBox)
                        {
                            SoundManager.Instance.PlaySoundTightenScrew();

                            GamePersistentVariable.isSoundTightenScrewPlaying = true;
                        }

                        _tweens.Add(Tween.Delay(0.9f * durationTightenScrewPort)
                        .OnComplete(() =>
                        {
                            _isRotating = false;
                            rotateSpeed = _initialRotateSpeed;
                            _isRotatingToCorrectAngleOnScrewBox = true;
                        }));
                        // _tweens.Add(Tween.LocalRotation(transform, Quaternion.Euler(new Vector3(-30f, 180 + 0f, 0)), duration: durationTightenScrewPort, ease: Ease.Linear));
                        _tweens.Add(Tween.LocalPosition(transform, finalLocalPosition, duration: durationTightenScrewPort, ease: Ease.Linear)
                            .OnComplete(() =>
                            {
                                screwBoxSlot.CompleteFill();

                                _isInScrewPort = screwBoxSlot.IsScrewPort;
                                _isMoving = false;

                                if (!_isInScrewPort)
                                {
                                    _isDone = true;
                                }

                                InvokeScrewLoosenedEvent();
                                screwMovedCompletedEvent?.Invoke(this);

                                onCompleteAction?.Invoke();

                                GamePersistentVariable.isSoundTightenScrewPlaying = false;
                            })
                        );
                    }));
                }));
            }));
        }));
    }

    private async void AddScrewToList()
    {
        await Task.Delay(200);

        addScrewToListEvent?.Invoke(this);
    }

    public bool IsValidToLoose()
    {
        CountBlockingObjects();

        if (_numberBlockingObjects > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public virtual void ForceUnscrew()
    {
        if (_isMoving)
        {
            return;
        }
        else
        {
            _isMoving = true;
        }

        if (joint != null)
        {
            Destroy(joint);
            // joint.breakForce = 0;

            breakJointEvent?.Invoke(joint.gameObject.GetInstanceID());
        }

        ThrowOutside();

        _isDone = false;
        _isOutsideWithNoScrewBox = true;
        _isInScrewPort = false;
    }

    public virtual void ThrowOutside()
    {
        _isMoving = true;
        _isRotating = true;

        _tweens.Add(Tween.Position(transform, transform.position + 3f * transform.forward, duration: 0.3f)
        .OnComplete(() =>
        {
            Vector3 destination = transform.position;

            if (transform.position.x > 0)
            {
                destination.x = 15;
            }
            else
            {
                destination.x = -15;
            }

            destination.y = 0;

            destination.x = 0.5f * GamePersistentVariable.screenSizeWorld.x;
            destination.y = 0.22f * GamePersistentVariable.screenSizeWorld.y;

            _tweens.Add(Tween.Position(transform, destination, duration: 0.3f).OnComplete(() =>
            {
                // InvokeScrewLoosenedEvent();

                GamePersistentVariable.numOutsideScrew++;

                updateOutsideScrewNumberEvent?.Invoke();

                screwServiceLocator.screwMeshRenderer.enabled = false;

                _isMoving = false;
                _isRotating = false;
            }));
        }));
    }

    public virtual int CountBlockingObjects(bool isIncludeHiddenScrew)
    {
        return 0;
    }

    public virtual int CountBlockingObjects()
    {
        return CountBlockingObjects(isIncludeHiddenScrew: false);
    }

    protected void InvokeScrewLoosenedEvent()
    {
        screwLoosenedEvent?.Invoke();
    }

    protected void InvokeBreakJointEvent()
    {
        breakJointEvent?.Invoke(joint.gameObject.GetInstanceID());
    }
}
