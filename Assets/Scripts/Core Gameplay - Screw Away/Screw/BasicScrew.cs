using System;
using System.Threading.Tasks;
using CandyCoded.HapticFeedback;
using PrimeTween;
using UnityEngine;

public class BasicScrew : BaseScrew
{
    // [Header("CUSTOMIZE - UNSCREW ANIMATION")]
    // [SerializeField] private float durationMoveOut = 0.3f;
    // [SerializeField] private float durationDelay = 0.1f;
    // [SerializeField] private float durationMoveToScrewPort = 0.3f;
    // [SerializeField] private float durationTightenScrewPort = 0.3f;

    private float _distanceBlocked;

    private void Update()
    {
        if (_isRotating)
        {
            transform.RotateAround(transform.forward, 0.3f);
        }

        if (_isRotatingToCorrectAngleOnScrewBox)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(new Vector3(-30, 180, 0)), 0.25f);

            if (transform.localRotation == Quaternion.Euler(new Vector3(-30, 180, 0)))
            {
                transform.localRotation = Quaternion.Euler(new Vector3(-30, 180, 0));

                _isRotatingToCorrectAngleOnScrewBox = false;
            }
        }
    }

    public override async void Loose(int screwId, GameEnum.GameFaction faction, ScrewBoxSlot screwBoxSlot)
    {
        if (screwId == this.screwId)
        {
            if (!_isInteractable)
            {
                return;
            }
            else
            {
                _isInteractable = false;
            }

            if (_isDone)
            {
                return;
            }

            CountBlockingObjects();

            // BLOCKED
            if (_numberBlockingObjects > 0)
            {
                FakeScrew fakeScrew = ObjectPoolingEverything.GetFromPool<FakeScrew>(GameConstants.FAKE_SCREW);

                fakeScrew.CloneFromScrew(this);

                fakeScrew.transform.SetParent(transform);

                GetComponent<MeshRenderer>().enabled = false;

                fakeScrew.IsRotating = true;

                SoundManager.Instance.PlaySoundLoosenScrewFail();

                HapticFeedback.MediumFeedback();

                _tweens.Add(Tween.LocalPosition(fakeScrew.transform, 0.1f * (fakeScrew.transform.localRotation * Vector3.forward),
                    cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.5f)
                .OnComplete(() =>
                {
                    fakeScrew.IsRotating = false;

                    ObjectPoolingEverything.ReturnToPool(GameConstants.FAKE_SCREW, fakeScrew.gameObject);
                    // fakeScrew.gameObject.SetActive(false);

                    GetComponent<MeshRenderer>().enabled = true;

                    _isInteractable = true;
                }));

                return;
            }

            if (joint != null)
            {
                Destroy(joint);
                // joint.breakForce = 0;

                InvokeBreakJointEvent();
            }

            screwBoxSlot.Fill(this);

            UnscrewAnimationAsync(screwBoxSlot);

            SoundManager.Instance.PlaySoundLoosenScrew();

            HapticFeedback.MediumFeedback();
        }
    }

    public int CountBlockingObjectsPrefabMode(bool isIncludeHiddenScrew)
    {
        const int MAX_OBSTACLE_ARRAY_SIZE = 10;

        Vector3 start = transform.position + transform.forward;

        Collider[] obstacles = new Collider[MAX_OBSTACLE_ARRAY_SIZE];

        // int numObstacle = gameObject.scene.GetPhysicsScene().OverlapCapsule(start, start + 10 * transform.forward, 0.2f, obstacles);

        RaycastHit hit;

        gameObject.scene.GetPhysicsScene().CapsuleCast(start, start + 0.1f * transform.forward, 0.2f, transform.forward, out hit);

        int number = 0;

        if (hit.collider != null)
        {
            if (hit.transform == transform.parent)
            {
                return 0;
            }

            if (hit.transform.GetComponent<IObjectPart>() != null)
            {
                float distance = Vector3.Distance(hit.point, transform.position);

                if (distance < 0.9f)
                {
                    number++;
                }
            }
        }

        // for (int i = 0; i < numObstacle; i++)
        // {

        // }

        return number;
    }

    public override int CountBlockingObjects(bool isIncludeHiddenScrew)
    {
        Vector3 start = transform.position;

        // DON'T cast too big capsule --> point 2 param is start + 0.1f * transform.forward
        RaycastHit[] hits = Physics.CapsuleCastAll(start, start + 0.01f * transform.forward, 0.2f, transform.forward, 10);

        int number = 0;

        float minBlockedDistance = float.MaxValue;

        if (hits != null)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform == transform.parent)
                {
                    continue;
                }

                if (hits[i].transform.GetComponent<IObjectPart>() != null)
                {
                    if (isIncludeHiddenScrew)
                    {
                        number++;

                        continue;
                    }

                    // Perpendicular distance
                    // Vector3 positionInScrewSpace = transform.InverseTransformPoint(hits[i].point);

                    // float distance = Mathf.Abs(positionInScrewSpace.z);

                    float distance = Vector3.Distance(hits[i].point, transform.position);

                    if (distance < minBlockedDistance)
                    {
                        minBlockedDistance = distance;
                    }

                    if (distance < 0.9f)
                    {
                        number++;
                    }
                }
            }
        }

        _numberBlockingObjects = number;
        _distanceBlocked = minBlockedDistance;

        return number;
    }
}
