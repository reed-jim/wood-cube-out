using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class BasicObjectPart : MonoBehaviour, IObjectPart
{
    [SerializeField] private Transform levelCenter;
    [SerializeField] private Rigidbody partRigidbody;

    [Header("CUSTOMIZE")]
    [SerializeField] private Vector3 throwForceMultiplier = new Vector3(0.003f, 0.001f, 0.003f);

    #region PRIVATE FIELD
    private bool _isSelecting;
    private bool _isFree;
    private bool _isImmuneSwipeForce;
    private int _totalJoint;
    #endregion

    #region EVENT
    public static event Action<int> selectObjectPartEvent;
    public static event Action<int> deselectObjectPartEvent;
    public static event Action<BaseScrew> loosenScrewOnObjectBrokenEvent;
    public static event Action shakeCameraEvent;
    public static event Action<int> dissolveObjectPartEvent;
    #endregion

    #region LIFECYCLE
    void Awake()
    {
        ScrewSelectionInput.mouseUpEvent += Deselect;
        BaseScrew.breakJointEvent += OnJointBreakCallback;
        SwipeGesture.swipeGestureEvent += OnSwipe;

        partRigidbody = GetComponent<Rigidbody>();

        _totalJoint = GetComponents<HingeJoint>().Length;

        throwForceMultiplier = new Vector3(66f, 9f, 66f) * 0.2f;
    }

    void OnDestroy()
    {
        ScrewSelectionInput.mouseUpEvent -= Deselect;
        BaseScrew.breakJointEvent -= OnJointBreakCallback;
        SwipeGesture.swipeGestureEvent -= OnSwipe;
    }
    #endregion

    public void Select()
    {
        selectObjectPartEvent?.Invoke(gameObject.GetInstanceID());

        _isSelecting = true;
    }

    private void Deselect()
    {
        if (_isSelecting)
        {
            deselectObjectPartEvent?.Invoke(gameObject.GetInstanceID());

            _isSelecting = false;
        }
    }

    private async void OnJointBreakCallback(int instanceId)
    {
        if (instanceId == gameObject.GetInstanceID())
        {
            await Task.Delay(500);

            _totalJoint--;

            if (_totalJoint == 0)
            {
                _isFree = true;

                StartCoroutine(Throwing());

                // dissolveObjectPartEvent?.Invoke(gameObject.GetInstanceID());
            }
        }
    }

    private IEnumerator Throwing()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        while (true)
        {
            Throw();

            yield return waitForSeconds;
        }
    }

    private async void OnSwipe(Vector2 direction)
    {
        // return;

        // if (_totalJoint <= 1 && !_isImmuneSwipeForce)
        // {
        //     // partRigidbody.AddForce(-30f * direction);

        //     if (partRigidbody.linearVelocity.magnitude > 5)
        //     {
        //         return;
        //     }

        //     // OUT OF CENTER
        //     if (_totalJoint == 0)
        //     {
        //         Vector3 forceDirection = direction;

        //         forceDirection.y = 0;

        //         partRigidbody.AddForce(-20f * forceDirection);
        //     }
        //     else
        //     {
        //         partRigidbody.AddForce(20f * (Vector3.zero - transform.localPosition));
        //     }

        //     _isImmuneSwipeForce = true;

        //     // await Task.Delay(1000);

        //     _isImmuneSwipeForce = false;
        // }
    }

    public async void Break(Vector3 touchPosition)
    {
        BoosterHammer hammer = ObjectPoolingEverything.GetFromPool<BoosterHammer>(GameConstants.HAMMER);

        Vector3 position1 = transform.position + 5 * transform.forward;
        Vector3 position2 = transform.position - 5 * transform.forward;

        float expectedYDirection;
        float expectedZDirection;

        Vector3 expectedPosition;

        if (Vector3.Distance(position1, Vector3.zero) < Vector3.Distance(position2, Vector3.zero))
        {
            // expectedPosition = position2 - 0.8f * transform.up;

            expectedZDirection = -1;
        }
        else
        {
            // expectedPosition = position1 - 0.8f * transform.up;

            expectedZDirection = 1;
        }

        if (Vector3.Distance(hammer.transform.position + transform.up, transform.position) < Vector3.Distance(hammer.transform.position - transform.up, transform.position))
        {
            expectedYDirection = 1;
        }
        else
        {
            expectedYDirection = -1;
        }

        // hammer.transform.position = transform.position + 5 * expectedZDirection * transform.forward + 0 * transform.up;
        hammer.transform.rotation = Quaternion.LookRotation(transform.forward);
        hammer.transform.Rotate(new Vector3(0, 90, 0));

        // hammer.transform.position = transform.position + 5 * expectedZDirection * transform.forward + 0 * transform.up;
        hammer.transform.position = touchPosition;

        Tween.Position(hammer.transform, transform.position + 2f * expectedZDirection * transform.forward + 1 * expectedYDirection * transform.up, duration: 0.2f)
        .Chain(Tween.Rotation(hammer.transform, hammer.transform.rotation.eulerAngles + new Vector3(0, 0, expectedZDirection * 60), duration: 0.6f))
        .Chain(Tween.Rotation(hammer.transform, hammer.transform.rotation.eulerAngles + new Vector3(0, 0, -expectedZDirection * 30), duration: 0.2f)
        .OnComplete(() =>
        {
            Tween.Position(hammer.transform, 50f * expectedZDirection * transform.forward, startDelay: 0.3f, duration: 0.3f);
        }));

        await Task.Delay(950);

        AudioSource breakObjectSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.BREAK_OBJECT_SOUND);

        breakObjectSound.Play();

        // hammer.PlayHitFx();

        shakeCameraEvent?.Invoke();

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

            if (screw.IsValidToLoose())
            {
                loosenScrewOnObjectBrokenEvent?.Invoke(screw);
            }
            else
            {
                screw.ForceUnscrew();
            }

            await Task.Delay(133);
        }

        Throw(forceBoost: 2);
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

        if (Mathf.Abs(direction.x) < 0.01f)
        {
            direction.x = 1f;
        }
        if (Mathf.Abs(direction.y) < 0.01f)
        {
            direction.y = 1f;
        }
        if (Mathf.Abs(direction.z) < 0.01f)
        {
            direction.z = 1f;
        }

        partRigidbody.AddForce(TransformUtil.ComponentWiseMultiply(forceBoost * throwForceMultiplier, direction), ForceMode.Impulse);
    }
}
