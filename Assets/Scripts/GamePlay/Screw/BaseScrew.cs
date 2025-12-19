using System;
using System.Collections.Generic;
using CandyCoded.HapticFeedback;
using NamPhuThuy;
using NamPhuThuy.VFX;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static GameEnum;

namespace NamPhuThuy.GamePlay
{
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
        protected bool _isMoving;

        protected bool _isRotating;
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
        // public static event Action screwLoosenedEvent;
        public static event Action<BaseScrew> screwLoosenedEvent;
        public static event Action updateOutsideScrewNumberEvent;
        public static event Action updateEventItem;
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

            _mainCamera = Camera.main;

            MoreLogicInAwake();
        }

        void Start()
        {
            transform.localEulerAngles = transform.localEulerAngles.ChangeZ(0);
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

        #region SELECT SCREW
        public void Select()
        {
            // DebugLogger.LogSimple();
            if (IsValidToLoose())
            {
                selectScrewEvent?.Invoke(screwId, Faction);
            }
            else
            {
                ShakeInvalidScrew();
            }
        }

        public void AutoSelect()
        {
            autoSelectScrewEvent?.Invoke(screwId, Faction);
        }

        protected virtual bool IsValidToLoose()
        {
            return true;

            // CountBlockingObjects();

            // if (_numberBlockingObjects > 0)
            // {
            //     return false;
            // }
            // else
            // {
            //     return true;
            // }
        }

        public virtual void Loose(int screwId, GameFaction faction, ScrewBoxSlot screwBoxSlot)
        {

        }

        public virtual void Loose(int screwId, GameFaction faction, ScrewBoxSlot screwBoxSlot, bool isCheckObstacles)
        {

        }

        public void LooseImmediately(ScrewBoxSlot screwBoxSlot)
        {
            if (joint != null)
            {
                Destroy(joint);

                InvokeBreakJointEvent();

                transform.SetParent(screwBoxSlot.transform);

                Vector3 finalLocalPosition = new Vector3(0, -0.1f, 0.2f);
                _localRotationOnScrewSlot = new Vector3(-30f, 180 + 0f, 0);

                if (screwBoxSlot.IsScrewPort)
                {
                    finalLocalPosition = new Vector3(0, -0.05f, 0.2f);
                    _localRotationOnScrewSlot = new Vector3(-20f, 180 + 0f, 0);
                }

                transform.localScale = scaleOnScrewBox * Vector3.one;
                transform.localPosition = finalLocalPosition;
                transform.localRotation = Quaternion.Euler(_localRotationOnScrewSlot);
            }

            screwBoxSlot.Fill(this);
            screwBoxSlot.CompleteFill();
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

            if (!GamePersistentVariable.isLevelDirty)
            {
                GamePersistentVariable.isLevelDirty = true;
            }

            screwStartLoosenedEvent?.Invoke(gameObject.GetInstanceID());

            _isMoving = true;
            _isRotating = true;

            _screwBoxSlot = screwBoxSlot;

            Vector3 screwSize = TransformUtil.ComponentWiseMultiply(transform.lossyScale, meshFilter.mesh.bounds.size);

            // LOOSE FX
            if (!_isInScrewPort && !_isOutsideWithNoScrewBox)
            {
                PlayLooseFX();
            }
            //

            if (IsOutsideWithNoScrewBox)
            {
                Vector3 destination = _mainCamera.ScreenToWorldPoint(GamePersistentVariable.boxForOutsideScrewScreenPosition);
                destination.z = transform.position.z;
                transform.position = destination;
            }

            transform.SetParent(screwBoxSlot.transform);

            _tweens.Add(Tween.LocalRotation(
                                transform, transform.eulerAngles - new Vector3(0, 0, 135), cycles: 2, cycleMode: CycleMode.Incremental,
                                duration: durationMoveOut / 2, ease: Ease.Linear
                            ));
            _tweens.Add(Tween.Position(transform, transform.position + screwSize.z * transform.forward, duration: durationMoveOut, ease: Ease.InOutSine).OnComplete(() =>
            {
                _tweens.Add(Tween.Delay(duration: durationDelay).OnComplete(() =>
                {
                    _isRotating = false;

                    float timeMultiplier = Vector3.Distance(new Vector3(0, 0.3f, -0.3f), transform.localPosition);
                    timeMultiplier = Mathf.Min(timeMultiplier, 8);

                    Vector3 preFinalLocalPosition = new Vector3(0, 0.4f, -24f);
                    Vector3 finalLocalPosition = new Vector3(0, -0.1f, 0.25f);
                    _localRotationOnScrewSlot = new Vector3(-25f, 180 + 0f, 0);

                    if (screwBoxSlot.IsScrewPort)
                    {
                        preFinalLocalPosition = new Vector3(0, 0.3f, -4f);
                        finalLocalPosition = new Vector3(0, -0.05f, 0.2f);
                        _localRotationOnScrewSlot = new Vector3(-15f, 180 + 0f, 0);
                    }

                    float durationMultiplier = Vector3.Distance(finalLocalPosition, transform.localPosition) / 10f;

                    durationMultiplier = Mathf.Clamp(durationMultiplier, 0.6f, 1);

                    float finalDurationMoveToScrewPort = durationMultiplier * durationMoveToScrewPort;

                    // Avoid screw left under screw holes area
                    transform.localPosition = transform.localPosition.ChangeZ(preFinalLocalPosition.z);

                    Vector3 currentScale = transform.localScale;

                    _tweens.Add(
                        Tween.LocalRotation(transform, Quaternion.Euler(_localRotationOnScrewSlot),
                        duration: durationMultiplier * durationMoveToScrewPort, ease: Ease.InOutSine));
                    _tweens.Add(Tween.Custom(0, 1, duration: durationMultiplier * durationMoveToScrewPort, ease: Ease.InOutSine, onValueChange: newVal =>
                    {
                        if (newVal < 0.5f)
                        {
                            transform.localScale = currentScale + (newVal / 0.5f) * (1.3f * scaleOnScrewBox * Vector3.one - currentScale);
                        }
                        else
                        {
                            transform.localScale = (1.3f - (2 * (newVal - 0.5f) * 0.3f)) * scaleOnScrewBox * Vector3.one;
                        }
                    }));
                    _tweens.Add(Tween.LocalPosition(transform, preFinalLocalPosition, duration: durationMultiplier * durationMoveToScrewPort, ease: Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        transform.localPosition = transform.localPosition.ChangeZ(-1.6f);
                        
                        bool canPlayTightenSound = !GamePersistentVariable.isSoundTightenScrewPlaying
                                                   && !_isInScrewPort
                                                   && !_isOutsideWithNoScrewBox;
                        
                        if (canPlayTightenSound)
                        {
                            SoundManager.Instance.PlaySoundTightenScrew();
                            GamePersistentVariable.isSoundTightenScrewPlaying = true;
                        }

                        Vector3 targetRotation = _localRotationOnScrewSlot - new Vector3(0f, 0f, 180f);
                        float tightenHalfDuration = durationTightenScrewPort / 2f;
                        
                        _tweens.Add(Tween.LocalRotation(
                            transform, 
                            targetRotation, 
                            cycles: 2, 
                            cycleMode: CycleMode.Incremental,
                            duration: tightenHalfDuration, 
                            ease: Ease.Linear
                        ));

                        _tweens.Add(Tween.LocalPosition(
                                transform, 
                                finalLocalPosition, 
                                duration: durationTightenScrewPort, 
                                ease: Ease.InOutSine)
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
        }

        public void PlayLooseFX()
        {
            try
            {
                ParticleSystem looseScrewFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX);
                ParticleSystem ringFx = looseScrewFx.transform.GetChild(0).GetComponent<ParticleSystem>();

                var looseScrewFxMain = looseScrewFx.main;
                var ringFxMain = ringFx.main;

                Color factionColor = FactionUtility.GetColorForFaction(Faction);
                Color brighterFactionColor = ColorUtil.GetNearWhiteColor(factionColor);

                looseScrewFxMain.startColor = factionColor;
                ringFxMain.startColor = factionColor;
                ringFxMain.startSize = 1.2f * transform.lossyScale.x;

                looseScrewFx.gameObject.SetActive(true);
                looseScrewFx.transform.rotation = transform.rotation;
                looseScrewFx.transform.position = GamePersistentVariable.lastScrewSelectedSurfaceHit.point + 0.03f * GamePersistentVariable.lastScrewSelectedSurfaceHit.normal;

                ringFx.transform.position = GamePersistentVariable.lastScrewSelectedSurfaceHit.point + 0.03f * GamePersistentVariable.lastScrewSelectedSurfaceHit.normal;

                looseScrewFx.Play();

                ringFx.transform.SetParent(transform.parent);

                _tweens.Add(Tween.Delay(1).OnComplete(() =>
                {   
                    ringFx.transform.SetParent(looseScrewFx.transform);
                    ringFx.transform.localPosition = new Vector3(0, 0, -0.19f);
                    ringFx.transform.localRotation = Quaternion.identity;
                }));
                // DebugLogger.LogBreak("");
            }
            catch (System.Exception)
            {
                DebugLogger.LogError($"BaseScrew.UnscrewAnimationAsync(): Cannot play loosefx");
            }
        }

        #endregion

        protected virtual void AddScrewToList()
        {
            _tweens.Add(Tween.Delay(0.2f).OnComplete(() =>
            {
                addScrewToListEvent?.Invoke(this);
            }));
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
                joint.breakForce = 0;

                Destroy(joint);

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
                Vector3 destination = _mainCamera.ScreenToWorldPoint(GamePersistentVariable.boxForOutsideScrewScreenPosition);

                destination.z = transform.position.z;

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

        #region EVENT
        protected void InvokeScrewLoosenedEvent()
        {
            screwLoosenedEvent?.Invoke(this);
        }

        public void InvokeBreakJointEvent()
        {
            breakJointEvent?.Invoke(joint.gameObject.GetInstanceID());
        }
        #endregion

        #region Invalid to unscrew
        private void ShakeInvalidScrew()
        {
            DebugLogger.LogSimple();
            VFXManager.Ins.PlayAt(VFXType.POPUP_TEXT,
                message: VFXPopupTextMessage.CANT_CLICK,
                initialParent: VFXManager.Ins.transform);
            if (!_isInteractable)
            {
                return;
            }
            else
            {
                _isInteractable = false;
            }

            FakeScrew fakeScrew = SpawnFakeScrew();

            GetComponent<MeshRenderer>().enabled = false;

            fakeScrew.IsRotating = true;

            SoundManager.Instance.PlaySoundLoosenScrewFail();

            if (gameSetting.IsVibrate)
            {
                HapticFeedback.MediumFeedback();
            }

            _tweens.Add(Tween.ShakeScale(fakeScrew.transform, strength: 0.3f * Vector3.one, duration: 0.3f)
            .OnComplete(() =>
            {
                fakeScrew.IsRotating = false;

                ObjectPoolingEverything.ReturnToPool(GameConstants.FAKE_SCREW, fakeScrew.gameObject);

                GetComponent<MeshRenderer>().enabled = true;

                _isInteractable = true;
            }));

            fakeScrew.ColorAlert();
        }

        protected virtual FakeScrew SpawnFakeScrew()
        {
            FakeScrew fakeScrew = ObjectPoolingEverything.GetFromPool<FakeScrew>(GameConstants.FAKE_SCREW);

            fakeScrew.CloneFromScrew(this);

            fakeScrew.transform.SetParent(transform);

            return fakeScrew;
        }
        #endregion
    }
}
