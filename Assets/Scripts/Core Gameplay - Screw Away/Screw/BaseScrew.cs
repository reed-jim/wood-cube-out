using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using static GameEnum;

public class BaseScrew : MonoBehaviour, IScrew
{
    [SerializeField] protected int screwId;

    [SerializeField] protected ScrewServiceLocator screwServiceLocator;

    [SerializeField] protected HingeJoint joint;

    [Header("CUSTOMIZE")]
    [SerializeField] protected float scaleOnScrewBox;

    #region PRIVATE FIELD
    protected List<Tween> _tweens = new List<Tween>();
    protected int _numberBlockingObjects;
    protected Vector3 _initialScale;
    protected bool _isRotating;
    protected bool _isInteractable = true;
    protected bool _isDone;
    protected bool _isLocked;
    protected bool _isInScrewPort;
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
    #endregion

    #region EVENT
    public static event Action<int, GameFaction> selectScrewEvent;
    public static event Action<BaseScrew> addScrewToListEvent;
    public static event Action<int> breakJointEvent;
    public static event Action screwLoosenedEvent;
    #endregion

    #region LIFE CYCLE
    protected virtual void Awake()
    {
        ScrewBoxManager.looseScrewEvent += Loose;
        RegisterMoreEvent();

        AddScrewToList();

        _initialScale = transform.localScale;

        MoreLogicInAwake();
    }

    void OnDestroy()
    {
        ScrewBoxManager.looseScrewEvent -= Loose;
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

    public virtual void Loose(int screwId, GameFaction faction, ScrewBoxSlot screwBoxSlot)
    {

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
        joint.breakForce = 0;

        breakJointEvent?.Invoke(joint.gameObject.GetInstanceID());

        _isDone = true;

        ThrowOutside();
    }

    public virtual void ThrowOutside()
    {
        _isRotating = true;

        Tween.Position(transform, transform.position + 3f * transform.forward, duration: 0.3f)
        .OnComplete(() =>
        {
            Vector3 destination = transform.position;

            if (transform.position.x > 0)
            {
                destination.x = 10;
            }
            else
            {
                destination.x = -10;
            }

            destination.y = 0;

            Tween.Position(transform, destination, duration: 0.3f).OnComplete(() =>
            {
                InvokeScrewLoosenedEvent();

                gameObject.SetActive(false);
            });
        });
    }

    public virtual int CountBlockingObjects()
    {
        return 0;
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
