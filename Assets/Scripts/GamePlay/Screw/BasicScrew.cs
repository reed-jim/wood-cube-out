using CandyCoded.HapticFeedback;
using PrimeTween;
using UnityEngine;

namespace NamPhuThuy.GamePlay
{
    public class BasicScrew : BaseScrew
    {
        [Header("BASIC SCREW - CUSTOMIZE")]
        [SerializeField]
        private LayerMask checkObstacleLayer;

        #region PRIVATE FIELD

        private int stepRotateToCorrectAngleOnScrewBox;
        private Vector3 _obstacleHitPointWorld;

        #endregion

        public override async void Loose(int screwId, GameEnum.GameFaction faction, ScrewBoxSlot screwBoxSlot)
        {
            Loose(screwId, faction, screwBoxSlot, isCheckObstacles: true);
        }

        public override async void Loose(int screwId, GameEnum.GameFaction faction, ScrewBoxSlot screwBoxSlot, bool isCheckObstacles)
        {
            if (screwId != this.screwId) return;

            if (!_isInteractable) return;
            else
            {
                _isInteractable = false;
            }

            if (_isDone) return;
            if (_isMoving) return;

            if (isCheckObstacles && !IsOutsideWithNoScrewBox)
            {
                if (!GamePersistentVariable.isAutoTestingLevel)
                {
                    // avoid loose screw that is covering by other objects
                    bool isNotVisible = Physics.Raycast(
                        transform.position + 0.5f * _renderer.bounds.size.z * transform.forward,
                        _mainCamera.transform.position - transform.position, 5);

                    if (isNotVisible)
                    {
                        _isInteractable = true;

                        return;
                    }
                }

                CountBlockingObjects();
            }
            else
            {
                _numberBlockingObjects = 0;
            }

            // BLOCKED
            if (_numberBlockingObjects > 0)
            {
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

                return;
            }

            if (joint != null)
            {
                joint.breakForce = 0;

                Destroy(joint);

                InvokeBreakJointEvent();
            }

            screwBoxSlot.Fill(this);

            // confirm done screw soon
            if (!screwBoxSlot.IsScrewPort)
            {
                _isDone = true;
            }

            UnscrewAnimationAsync(screwBoxSlot);

            if (!_isInScrewPort && !_isOutsideWithNoScrewBox)
            {
                SoundManager.Instance.PlaySoundLoosenScrew();

                if (gameSetting.IsVibrate)
                {
                    HapticFeedback.MediumFeedback();
                }
            }
        }

        public override int CountBlockingObjects(bool isIncludeHiddenScrew)
        {
            float screwRadius = transform.lossyScale.x * 0.45f * 0.779f * meshFilter.sharedMesh.bounds.size.x;

            Vector3 pointOne = transform.position + transform.lossyScale.z * 1f * 0.779f *
                meshFilter.sharedMesh.bounds.size.z * transform.forward;
            Vector3 pointTwo = pointOne + 10f * transform.forward;

            Collider[] hits = new Collider[16];

            Physics.OverlapCapsuleNonAlloc(pointOne, pointTwo, screwRadius, hits, checkObstacleLayer);

            int number = 0;

            if (hits != null)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i] == null)
                    {
                        continue;
                    }

                    if (hits[i].transform.GetInstanceID() == transform.parent.GetInstanceID())
                    {
                        continue;
                    }

                    IObjectPart objectPart = hits[i].transform.GetComponent<IObjectPart>();

                    if (objectPart != null)
                    {
                        int totalScrewOfPiece = hits[i].transform.childCount;

                        if (isIncludeHiddenScrew)
                        {
                            number += totalScrewOfPiece;

                            continue;
                        }

                        Vector3 hitPos;

                        // Concave
                        if (objectPart.IsConcave)
                        {
                            RaycastHit hit;

                            float radius = transform.lossyScale.x * 0.45f * 0.779f *
                                           meshFilter.sharedMesh.bounds.size.x;

                            Physics.CapsuleCast(
                                pointOne - radius * transform.forward,
                                pointOne - radius * transform.forward + 0.01f * transform.forward,
                                transform.lossyScale.x * 0.45f * 0.779f * meshFilter.sharedMesh.bounds.size.x,
                                transform.forward, out hit, 10, checkObstacleLayer);

                            if (hit.collider != null)
                            {
                                hitPos = hit.point;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            hitPos = hits[i].ClosestPoint(pointOne);
                        }

                        // Perpendicular distance
                        Vector3 positionInScrewSpace = transform.InverseTransformPoint(hitPos);

                        float distance = positionInScrewSpace.z -
                                         transform.lossyScale.z * 1f * 0.779f * meshFilter.sharedMesh.bounds.size.z;

                        distance = Mathf.Max(distance, 0);

                        if (distance < transform.lossyScale.z * 0.5f * 0.779f * meshFilter.sharedMesh.bounds.size.z)
                        {
                            if (Mathf.Abs(positionInScrewSpace.x) > screwRadius ||
                                Mathf.Abs(positionInScrewSpace.y) > screwRadius)
                            {
                                continue;
                            }

                            number += totalScrewOfPiece;

                            _obstacleHitPointWorld = hitPos;

                            // hitPoint = positionInScrewSpace;
                            // obstacle = hits[i].gameObject;
                        }
                    }
                }
            }

            _numberBlockingObjects = number;

            return number;
        }
    }
}
