using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class BasicObjectPart : MonoBehaviour, IObjectPart
{
    [SerializeField] private Transform levelCenter;
    [SerializeField] private Rigidbody partRigidbody;
    [SerializeField] private MeshCollider meshCollider;

    [SerializeField] private BaseScrew[] attachedScrews;

    [Header("CUSTOMIZE")]
    [SerializeField] private Vector3 throwForceMultiplier = new Vector3(0.003f, 0.003f, 0.003f);
    [SerializeField] private int minJointToDisableKinematic = 1;

    #region COMPONENT
    private ObjectPartMaterialPropertyBlock _pieceMaterialPropertyBlock;
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

        attachedScrews = new BaseScrew[transform.childCount];

        for (int i = 0; i < attachedScrews.Length; i++)
        {
            attachedScrews[i] = transform.GetChild(i).GetComponent<BaseScrew>();
        }




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

        // REMOTE CONFIG ENABLE JOINT
        bool isEnableJoint = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_ENABLE_HINGE_JOINT, true);

        if (!isEnableJoint)
        {
            HingeJoint[] hingeJoints = GetComponents<HingeJoint>();

            List<HingeJoint> toRemoveJoints = hingeJoints.ToList();

            for (int i = 0; i < hingeJoints.Length; i++)
            {
                hingeJoints[i].breakForce = 0;
            }

            // foreach (var item in toRemoveJoints)
            // {
            //     Destroy(item);
            // }

            minJointToDisableKinematic = 0;

            meshCollider.enabled = false;
        }
        else
        {
            if (meshCollider.convex)
            {
                minJointToDisableKinematic = 1;

                meshCollider.isTrigger = false;
            }
            else
            {
                minJointToDisableKinematic = 0;
            }
        }

        // BETTER FEELING
        // HideBeforeLevel();

        _boundSize = GetComponent<MeshRenderer>().bounds.size;

        AutoAssignPhysicsMaterial();
    }

    void OnDestroy()
    {
        ScrewSelectionInput.mouseUpEvent -= Deselect;
        BaseScrew.breakJointEvent -= OnJointBreakCallback;
        ScrewManager.levelSetupCompletedEvent -= ScaleBeforeLevel;

        CommonUtil.StopAllTweens(_tweens);
        CommonUtil.StopAllSequences(_sequences);
    }
    #endregion

    #region BETTER FEELING
    private void HideBeforeLevel()
    {
        _initialScale = transform.localScale;

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

        // _tweens.Add(Tween.Scale(transform, 1.1f * _initialScale, duration: 0.3f, startDelay: 0.6f)
        //     .OnComplete(() =>
        //     {
        //         _tweens.Add(Tween.Scale(transform, _initialScale, duration: 0.1f));
        //     })
        // );
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
            partRigidbody.isKinematic = true;

            await Task.Delay(500);

            if (!_isConcave)
            {
                partRigidbody.isKinematic = false;
            }

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

                // dissolveObjectPartEvent?.Invoke(gameObject.GetInstanceID());
            }
        }
    }

    public async void Break(Vector3 touchPosition, RaycastHit hit)
    {
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

        // transform.SetParent(LevelGarbageBin.GarbageBin);
    }

    private void DetachFromParent()
    {
        transform.SetParent(LevelGarbageBin.GarbageBin);
    }

    #region UTIL
    private bool IsMeshConcave()
    {
        return !meshCollider.convex;
    }
    #endregion
}
