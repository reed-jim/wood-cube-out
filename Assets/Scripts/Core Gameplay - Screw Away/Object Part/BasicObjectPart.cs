using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdSort2;
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

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private List<Sequence> _sequences;
    private bool _isSelecting;
    private bool _isFree;
    private bool _isImmuneSwipeForce;
    private int _totalJoint;
    private bool _isConcave;
    private Vector3 _initialScale;
    #endregion
    

    public bool IsConcave
    {
        get => _isConcave;
    }
    

    #region LIFECYCLE
    public void Init()
    {
        _tweens = new List<Tween>();
        _sequences = new List<Sequence>();
        partRigidbody = GetComponent<Rigidbody>();
        meshCollider = GetComponent<MeshCollider>();

        _totalJoint = GetComponents<HingeJoint>().Length;

        throwForceMultiplier = new Vector3(1200f, 800f, 1200f);

        attachedScrews = new BaseScrew[transform.childCount];

        for (int i = 0; i < attachedScrews.Length; i++)
        {
            attachedScrews[i] = transform.GetChild(i).GetComponent<BaseScrew>();
        }

        // if (_totalJoint > 1)
        // {
        //     partRigidbody.isKinematic = true;
        // }

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
        bool isEnableJoint = RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_JOINT, true);
        
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

            //meshCollider.enabled = false;
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
        HideBeforeLevel();
    }
    #endregion

    public void Select()
    {
        GetComponent<ObjectPartMaterialPropertyBlock>().OnObjectPartSelected(gameObject.GetInstanceID());

        _isSelecting = true;

        for (int i = 0; i < attachedScrews.Length; i++)
        {
            if (!attachedScrews[i].IsDone)
            {
                attachedScrews[i].ScrewServiceLocator.screwOutline.ShowOutline();
            }
        }
    }

    public void Deselect()
    {
        if (_isSelecting)
        {
            GetComponent<ObjectPartMaterialPropertyBlock>().OnObjectPartDeselected(gameObject.GetInstanceID());

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

    public async void OnJointBreakCallback(int instanceId)
    { 
        if (instanceId == gameObject.GetInstanceID())
        {
            await Task.Delay(500);

            _totalJoint--;

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

                Throw();

                // dissolveObjectPartEvent?.Invoke(gameObject.GetInstanceID());
            }
        }
    }

    public async void Break(Vector3 touchPosition, RaycastHit hit)
    {
        BoosterHammer hammer = ObjectPoolingEverything.GetFromPool<BoosterHammer>(GameConstants.HAMMER);
        Debug.Log("Breaking");
        // hammer.PlayHitFx();
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

                        GameController.Instance.musicManager.PlayOneShot(BirdSort2.NameMusic.BreakObject);

                        hammer.Dissolve();

                        GamePlayController_BirdSort.Instance.gamePlayCamera.Shake();

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

            GamePlayController_BirdSort.Instance.level.screwBoxManager.LoosenScrewOnObjectBroken(screw);

            await Task.Delay(33);
        }
    }

    private void Throw(float forceBoost = 1)
    {
        Vector3 direction;

        if (levelCenter != null)
        {
            direction = transform.position - levelCenter.position;
        }
        else
        {
            direction = transform.position - transform.parent.position;
        }

        direction = direction.normalized;

        if (direction.x > 0)
        {
            direction.x = 1;
        }
        else
        {
            direction.x = -1;
        }

        // if (direction.y > 0)
        // {
        //     direction.y = 1;
        // }
        // else
        // {
        //     direction.y = -1;
        // }

        direction.y = 1;

        if (direction.z > 0)
        {
            direction.z = 1;
        }
        else
        {
            direction.z = -1;
        }

        // // direction.y = Mathf.Abs(direction.y);

        // if (Mathf.Abs(direction.x) < 0.01f)
        // {
        //     direction.x = 1f;
        // }
        // if (Mathf.Abs(direction.y) < 0.01f)
        // {
        //     direction.y = 1f;
        // }
        // if (Mathf.Abs(direction.z) < 0.01f)
        // {
        //     direction.z = 1f;
        // }

        partRigidbody.AddForce(TransformUtil.ComponentWiseMultiply(forceBoost * throwForceMultiplier, direction), ForceMode.Impulse);

        transform.SetParent(LevelGarbageBin.GarbageBin);
    }
    
    #region BETTER FEELING
    private void HideBeforeLevel()
    {
        _initialScale = transform.localScale;

        transform.localScale = Vector3.zero;
    }

    public void ScaleBeforeLevel()
    {
        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Delay(0.4f).OnComplete(() =>
        {
            //HideBeforeLevel();
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
    #endregion
}
