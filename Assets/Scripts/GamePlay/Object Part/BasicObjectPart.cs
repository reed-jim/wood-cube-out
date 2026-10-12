using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using PrimeTween;
using UnityEngine;

public class BasicObjectPart : MonoBehaviour, IObjectPart
{
    [SerializeField] private Rigidbody partRigidbody;
    [SerializeField] private MeshCollider meshCollider;

    [SerializeField] private BaseScrew[] attachedScrews;

    [Header("CUSTOMIZE")]
    [SerializeField] private Vector3 throwForceMultiplier = new Vector3(0.003f, 0.003f, 0.003f);
    [SerializeField] private int minJointToDisableKinematic = 1;

    #region COMPONENT
    protected ObjectPartMaterialPropertyBlock _pieceMaterialPropertyBlock;
    #endregion

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private List<Sequence> _sequences;
    private bool _isSelecting;
    private bool _isFree;
    private bool _isImmuneSwipeForce;
    private int _totalJoint;
    private bool _isConcave;
    private Vector3 _initialScale;
    private Vector3 _boundSize;
    #endregion

    #region CHECK HINGE JOINT MOVING
    private Vector3 _initialPosition;
    private Transform _lastScrew;
    private Transform _level;
    private float _lastDistance;
    private Vector3 _initialLocalPosition;
    private Vector3 _initialEulerAngles;
    #endregion  

    public bool IsConcave
    {
        get => _isConcave;
    }

    public Vector3 BoundSize
    {
        get => _boundSize;
    }

    public ObjectPartMaterialPropertyBlock PieceMaterialPropertyBlock
    {
        get => _pieceMaterialPropertyBlock;
    }

    #region EVENT
    public static event Action<int> selectObjectPartEvent;
    public static event Action<int> deselectObjectPartEvent;
    public static event Action<BaseScrew> loosenScrewOnObjectBrokenEvent;
    public static event Action shakeCameraEvent;
    public static event Action<int> dissolveObjectPartEvent;
    public static event Action<int> pieceCompleteEvent;
    #endregion

    #region LIFECYCLE
    void Awake()
    {
        ScrewSelectionInput.mouseUpEvent += Deselect;
        BaseScrew.breakJointEvent += OnJointBreakCallback;
        ScrewManager.levelSetupCompletedEvent += ScaleBeforeLevel;

        _tweens = new List<Tween>();
        _sequences = new List<Sequence>();

        partRigidbody = GetComponent<Rigidbody>();
        meshCollider = GetComponent<MeshCollider>();
        _pieceMaterialPropertyBlock = GetComponent<ObjectPartMaterialPropertyBlock>();

        _totalJoint = GetComponents<HingeJoint>().Length;

        throwForceMultiplier = new Vector3(1200f, 800f, 1200f);

        attachedScrews = TransformUtil.GetComponentArrayFromAllChildren<BaseScrew>(transform);

        partRigidbody.isKinematic = true;

        if (!meshCollider.convex)
        {
            _isConcave = true;
        }
        else
        {
            meshCollider.isTrigger = true;
        }

        partRigidbody.mass = 100;

        _boundSize = GetComponent<MeshRenderer>().bounds.size;

        AutoAssignPhysicsMaterial();
        MoreLogicOnAwake();
    }

    void OnDestroy()
    {
        ScrewSelectionInput.mouseUpEvent -= Deselect;
        BaseScrew.breakJointEvent -= OnJointBreakCallback;
        ScrewManager.levelSetupCompletedEvent -= ScaleBeforeLevel;

        CommonUtil.StopAllTweens(_tweens);
        CommonUtil.StopAllSequences(_sequences);

        MoreLogicOnDestroy();
    }

    protected virtual void MoreLogicOnAwake()
    {

    }

    protected virtual void MoreLogicOnDestroy()
    {

    }
    #endregion

    #region BETTER FEELING
    private void HideBeforeLevel()
    {
        _initialScale = transform.localScale;
        // DebugLogger.LogSimple(message:$"{name}: {_initialScale}");

        transform.localScale = Vector3.zero;
    }

    private void ScaleBeforeLevel()
    {
        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Delay(0.4f).OnComplete(() =>
        {
            HideBeforeLevel();
        }));

        _tweens.Add(Tween.Delay(0.9f).OnComplete(() =>
        {
            _tweens.Add(Tween.Scale(transform, 1.1f * _initialScale, duration: 0.3f)
                .OnComplete(() =>
                {
                    _tweens.Add(Tween.Scale(transform, _initialScale, duration: 0.1f));
                })
            );
        }));
    }

    private void AutoAssignPhysicsMaterial()
    {
        PhysicsMaterial physicsMaterial = new PhysicsMaterial();

        physicsMaterial.staticFriction = 0;
        physicsMaterial.dynamicFriction = 0;
        physicsMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;

        meshCollider.material = physicsMaterial;
    }
    #endregion

    public void Select()
    {
        // DebugLogger.LogSimple();
        selectObjectPartEvent?.Invoke(gameObject.GetInstanceID());

        _isSelecting = true;

        for (int i = 0; i < attachedScrews.Length; i++)
        {
            if (!attachedScrews[i].IsDone)
            {
                attachedScrews[i].ScrewServiceLocator.screwOutline.ShowOutline();
            }
        }
    }

    private void Deselect()
    {
        // DebugLogger.LogSimple();
        if (_isSelecting)
        {
            deselectObjectPartEvent?.Invoke(gameObject.GetInstanceID());

            _isSelecting = false;

            for (int i = 0; i < attachedScrews.Length; i++)
            {
                if (!attachedScrews[i].IsDone)
                {
                    attachedScrews[i].ScrewServiceLocator.screwOutline.HideOutline();
                }
            }
        }
    }

    private async void OnJointBreakCallback(int instanceId)
    {
        if (instanceId == gameObject.GetInstanceID())
        {
            _totalJoint--;

            if (partRigidbody == null)
            {
                return;
            }

            if (partRigidbody.isKinematic)
            {
                if (_totalJoint <= 1 && _totalJoint <= minJointToDisableKinematic)
                {
                    if (meshCollider != null)
                    {
                        if (!meshCollider.convex)
                        {
                            meshCollider.convex = true;
                            meshCollider.isTrigger = true;
                        }
                    }

                    partRigidbody.isKinematic = false;
                }
            }

            if (_totalJoint == 0)
            {
                if (meshCollider.enabled && !meshCollider.convex)
                {
                    meshCollider.enabled = false;
                }

                _isFree = true;

                if (_isConcave)
                {
                    Throw();
                }

                DetachFromParent();

                pieceCompleteEvent?.Invoke(transform.GetInstanceID());

                if (GamePersistentVariable.isLoadingOldLevelState)
                {
                    gameObject.SetActive(false);
                }

                HandleRewardPiece();
            }
        }
    }

    public async void Break(Vector3 touchPosition, RaycastHit hit)
    {
        DebugLogger.LogSimple();
        BoosterHammer hammer = ObjectPoolingEverything.GetFromPool<BoosterHammer>(GameConstants.HAMMER);

        hammer.transform.rotation = Quaternion.identity;

        Quaternion surfaceRotation = Quaternion.LookRotation(hit.normal);

        hammer.transform.rotation = surfaceRotation;
        hammer.transform.position = touchPosition - new Vector3(0, 0.5f, 0) + 2f * hit.normal;

        Vector3 rotationInitialAngles = hammer.transform.eulerAngles;

        _sequences.Add(Tween.Scale(hammer.transform, 6, duration: 0.4f).Chain
        (
            Tween.Scale(hammer.transform, 3, duration: 0.2f)
        ));

        _sequences.Add(Tween.Rotation(hammer.transform, rotationInitialAngles + new Vector3(120, 30, 0), duration: 0.4f)
        .Chain(
        Tween.Rotation(hammer.transform, rotationInitialAngles - new Vector3(30, 0, 10), duration: 0.2f)
        .OnComplete(() =>
        {
            hammer.PlayHitFx();

            SoundManager.Instance.PlaySoundBreakObject();

            hammer.Dissolve();

            shakeCameraEvent?.Invoke();

            GetComponent<Renderer>().enabled = false;
        })));

        await Task.Delay(600);

        // BE CAREFUL OF USING childCount
        List<BaseScrew> screws = new List<BaseScrew>();

        for (int i = 0; i < transform.childCount; i++)
        {
            BaseScrew screw = transform.GetChild(i).GetComponent<BaseScrew>();

            screws.Add(screw);
        }

        for (int i = 0; i < screws.Count; i++)
        {
            // Screw may be null if the user unscrews it before the break animation finishes.
            BaseScrew screw = screws[i];

            loosenScrewOnObjectBrokenEvent?.Invoke(screw);

            await Task.Delay(33);
        }

        // Throw(forceBoost: 2);
    }

    private void Throw(float forceBoost = 1)
    {
        Vector3 direction;

        Vector3 virtualOrigin = new Vector3(0, -99, 0);

        direction = transform.position - virtualOrigin;

        direction = direction.normalized;

        bool isObstacleRight = Physics.Raycast(transform.position, transform.right, maxDistance: 10);
        bool isObstacleFront = Physics.Raycast(transform.position, transform.forward, maxDistance: 10);
        bool isObstacleUp = Physics.Raycast(transform.position, transform.up, maxDistance: 10);

        if (isObstacleRight)
        {
            direction += -transform.right;
        }
        else
        {
            direction += transform.right;
        }

        if (isObstacleFront)
        {
            direction += -transform.forward;
        }
        else
        {
            direction += transform.forward;
        }

        if (isObstacleUp)
        {
            direction += -transform.up;
        }
        else
        {
            direction += transform.up;
        }

        direction.x = direction.x > 0 ? 1 : -1;
        direction.y = direction.y > 0 ? 1 : -1;
        direction.z = direction.z > 0 ? 1 : -1;

        partRigidbody.AddForce(TransformUtil.ComponentWiseMultiply(forceBoost * throwForceMultiplier, direction), ForceMode.Impulse);
    }

    private void DetachFromParent()
    {
        transform.SetParent(LevelGarbageBin.GarbageBin);
    }

    #region CHECK HINGE JOINT MOVING
    private IEnumerator CheckHingeJointMoving()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        while (_totalJoint > 0)
        {
            if (Mathf.Abs(_lastDistance - Vector3.Distance(transform.position, _lastScrew.position)) > 0.05f)
            {
                partRigidbody.isKinematic = true;

                _tweens.Add(Tween.LocalPosition(transform, _initialLocalPosition, duration: 0.3f));
                _tweens.Add(Tween.LocalEulerAngles(transform, transform.localEulerAngles, _initialEulerAngles, duration: 0.3f));

                break;
            }

            yield return waitForSeconds;
        }
    }
    #endregion  

    #region UTIL
    private bool IsMeshConcave()
    {
        return !meshCollider.convex;
    }
    #endregion

    #region SPECIAL
    private void HandleRewardPiece()
    {
        RewardPiece rewardPiece = GetComponent<RewardPiece>();

        if (rewardPiece != null)
        {
            rewardPiece.EarnReward();
        }
    }
    #endregion
}
