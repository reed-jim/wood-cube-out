using AntsFlow;
using PrimeTween;
using Spine.Unity;
using TMPro;
using UnityEngine;

namespace BusFlow
{
    public enum BusFlowBusState
    {
        Launching,              // Đang di chuyển từ ô bãi đỗ lên điểm nhập làn trên spline
        DrivingPickup,          // Đang chạy vòng trên spline để đón khách cùng màu
        DrivingToExitJunction,  // Đã đón đủ khách, đang chạy tiếp trên loop tới điểm rẽ ra exit
        DrivingExit,            // Đang chạy trên đường thoát ra khỏi map
        Done
    }

    public class BaseBus : MonoBehaviour
    {
        public Color faction;
        public int number;
        public int remaining;
        public BusFlowBusState state;
        public bool isPaused;

        [Header("Pickup")]
        public float pickupRadius = 5f;
        private float pickupCheckTimer;

        private float speed = 30f;

        private RuntimeSplinePath pickupPath;
        private RuntimeSplinePath exitPath;

        private Tween launchTween;
        private Tween driveTween;

        // Prefab mode (Ant Box / Spine / Lid / Text)
        private SkeletonAnimation externalSpine;
        private SpriteRenderer externalLid;
        private TMP_Text externalNumberText;

        // Primitive mode
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock materialPropertyBlock;
        private TextMeshPro fallbackNumberLabel;

        public void Init(Color busFaction, int busNumber, RuntimeSplinePath pickup, RuntimeSplinePath exit, float busSpeed, Material overrideMaterial, Vector3 garageWorldPosition, bool animateLaunch = true)
        {
            faction = busFaction;
            number = busNumber;
            remaining = busNumber;
            pickupPath = pickup;
            exitPath = exit;
            speed = busSpeed;
            isPaused = false;

            SetupVisual(faction, remaining, overrideMaterial);

            // Nhập làn: tìm khoảng cách thực (distance) trên spline gần ô bãi đỗ nhất
            float entryDistance = pickupPath != null
                ? pickupPath.GetNearestDistanceToWorldPoint(garageWorldPosition)
                : 0f;

            Vector3 entrySplinePos = pickupPath != null
                ? pickupPath.GetPositionAtDistance(entryDistance)
                : garageWorldPosition;

            // Đặt Z = -0.15f để luôn nổi trên mặt đường và hàng khách
            entrySplinePos.z = -0.15f;
            Vector3 startPos = new Vector3(garageWorldPosition.x, garageWorldPosition.y, -0.15f);

            launchTween.Stop();
            driveTween.Stop();

            if (animateLaunch)
            {
                state = BusFlowBusState.Launching;
                transform.position = startPos;

                Vector3 dir = entrySplinePos - startPos;
                if (dir.sqrMagnitude > 0.01f)
                {
                    float launchAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, launchAngle);
                }

                float launchDuration = 0.35f;
                launchTween = Tween.Position(transform, entrySplinePos, duration: launchDuration, ease: Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        if (state == BusFlowBusState.Launching)
                        {
                            StartDrivingLoop(entryDistance);
                            if (BusFlowGameManager.Ins != null)
                            {
                                BusFlowGameManager.Ins.CheckMergeRule();
                            }
                        }
                    });
            }
            else
            {
                transform.position = entrySplinePos;
                StartDrivingLoop(entryDistance);
                if (BusFlowGameManager.Ins != null)
                {
                    BusFlowGameManager.Ins.CheckMergeRule();
                }
            }
        }

        private void StartDrivingLoop(float startDistance)
        {
            state = BusFlowBusState.DrivingPickup;

            if (pickupPath == null || pickupPath.Length <= 0.0001f)
            {
                return;
            }

            transform.position = SetZ(pickupPath.GetPositionAtDistance(startDistance), -0.15f);
            transform.rotation = pickupPath.GetRotationAtDistance(startDistance);

            driveTween.Stop();

            float totalLength = pickupPath.Length;
            // Thời gian 1 vòng = Tổng chiều dài / Vận tốc
            float lapDuration = Mathf.Max(0.1f, totalLength / speed);

            // Chạy đều theo chiều dài thực (distance) thay vì percent -> Tốc độ đồng đều 100% trên cả đoạn thẳng lẫn đoạn cong!
            driveTween = Tween.Custom(0f, totalLength, duration: lapDuration, ease: Ease.Linear, cycles: -1, onValueChange: progressDistance =>
            {
                if (isPaused || state != BusFlowBusState.DrivingPickup)
                {
                    return;
                }

                float currentDistance = (startDistance + progressDistance) % totalLength;
                if (currentDistance < 0f)
                {
                    currentDistance += totalLength;
                }

                transform.position = SetZ(pickupPath.GetPositionAtDistance(currentDistance), -0.15f);
                transform.rotation = pickupPath.GetRotationAtDistance(currentDistance);
            });
        }

        private void StartExit()
        {
            if (exitPath == null || exitPath.Length <= 0.0001f)
            {
                CompleteAndDespawn();
                return;
            }

            if (pickupPath == null || pickupPath.Length <= 0.0001f)
            {
                StartDrivingOnExitPath();
                return;
            }

            // Điểm bắt đầu của spline exit nằm trên đường đón khách (pickupPath).
            // Xe chạy tiếp dọc theo loop hiện tại tới đúng điểm rẽ này rồi mới chuyển làn sang exitPath.
            Vector3 exitStartPos = exitPath.GetPositionAtDistance(0f);
            float exitJunctionDistance = pickupPath.GetNearestDistanceToWorldPoint(exitStartPos);
            float currentDistance = GetCurrentSplineDistance();
            float loopLength = pickupPath.Length;

            float distToJunction = exitJunctionDistance - currentDistance;
            if (distToJunction < 0.05f)
            {
                distToJunction += loopLength;
            }

            // Nếu đã đứng ngay sát điểm rẽ, chuyển làn luôn
            if (distToJunction <= 0.1f)
            {
                StartDrivingOnExitPath();
                return;
            }

            state = BusFlowBusState.DrivingToExitJunction;
            driveTween.Stop();

            float duration = Mathf.Max(0.05f, distToJunction / speed);

            driveTween = Tween.Custom(0f, distToJunction, duration: duration, ease: Ease.Linear, onValueChange: progress =>
            {
                if (isPaused || state != BusFlowBusState.DrivingToExitJunction)
                {
                    return;
                }

                float d = (currentDistance + progress) % loopLength;
                if (d < 0f)
                {
                    d += loopLength;
                }

                transform.position = SetZ(pickupPath.GetPositionAtDistance(d), -0.15f);
                transform.rotation = pickupPath.GetRotationAtDistance(d);
            })
            .OnComplete(() =>
            {
                if (state == BusFlowBusState.DrivingToExitJunction)
                {
                    StartDrivingOnExitPath();
                }
            });
        }

        private void StartDrivingOnExitPath()
        {
            state = BusFlowBusState.DrivingExit;
            driveTween.Stop();

            if (exitPath == null || exitPath.Length <= 0.0001f)
            {
                CompleteAndDespawn();
                return;
            }

            float totalExitLength = exitPath.Length;
            float exitDuration = Mathf.Max(0.1f, totalExitLength / speed);

            driveTween = Tween.Custom(0f, totalExitLength, duration: exitDuration, ease: Ease.Linear, cycles: 1, onValueChange: currentDistance =>
            {
                if (isPaused || state != BusFlowBusState.DrivingExit)
                {
                    return;
                }

                transform.position = SetZ(exitPath.GetPositionAtDistance(currentDistance), -0.15f);
                transform.rotation = exitPath.GetRotationAtDistance(currentDistance);
            })
            .OnComplete(CompleteAndDespawn);
        }

        private void Update()
        {
            if (state != BusFlowBusState.DrivingPickup || isPaused || remaining <= 0)
            {
                return;
            }

            pickupCheckTimer += Time.deltaTime;
            if (pickupCheckTimer >= 0.05f)
            {
                pickupCheckTimer = 0f;
                CheckPassengerPickup();
            }
        }

        private void CheckPassengerPickup()
        {
            if (PassengerBoardManager.Ins == null || remaining <= 0)
            {
                return;
            }

            if (PassengerBoardManager.Ins.TryPickupPassengersForBus(this, pickupRadius, out int pickedCount))
            {
                if (pickedCount > 0)
                {
                    remaining = Mathf.Max(0, remaining - pickedCount);
                    UpdateNumberLabel();

                    if (BusFlowGameManager.Ins != null)
                    {
                        BusFlowGameManager.Ins.NotifyProgress();
                    }

                    if (remaining <= 0)
                    {
                        StartExit();
                    }
                }
            }
        }

        private void OnDestroy()
        {
            launchTween.Stop();
            driveTween.Stop();
        }

        private static Vector3 SetZ(Vector3 v, float z)
        {
            v.z = z;
            return v;
        }

        private void SetupVisual(Color busFaction, int count, Material overrideMaterial)
        {
            if (externalSpine != null || externalLid != null)
            {
                if (externalSpine != null && externalSpine.Skeleton != null)
                {
                    string skinName = FactionMapper.Ins != null ? FactionMapper.Ins.GetAntBoxSkin(busFaction) : null;
                    if (!string.IsNullOrEmpty(skinName))
                    {
                        var skin = externalSpine.Skeleton.Data.FindSkin(skinName);
                        if (skin != null)
                        {
                            externalSpine.Skeleton.SetSkin(skin);
                            externalSpine.Skeleton.SetSlotsToSetupPose();
                            externalSpine.AnimationState?.Apply(externalSpine.Skeleton);
                        }
                    }
                }

                if (externalLid != null)
                {
                    externalLid.color = busFaction;
                }

                if (externalNumberText != null)
                {
                    externalNumberText.text = count.ToString();
                }

                return;
            }

            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            if (meshRenderer != null)
            {
                BusFlowVisualFactory.ApplyFactionVisual(meshRenderer, busFaction, overrideMaterial, ref materialPropertyBlock);
            }

            if (fallbackNumberLabel == null)
            {
                GameObject labelGO = new GameObject("Number");
                labelGO.transform.SetParent(transform, false);
                labelGO.transform.localPosition = new Vector3(0f, 0f, -0.7f);

                fallbackNumberLabel = labelGO.AddComponent<TextMeshPro>();
                fallbackNumberLabel.alignment = TextAlignmentOptions.Center;
                fallbackNumberLabel.fontSize = 4f;
                fallbackNumberLabel.color = Color.white;
            }

            UpdateNumberLabel();
        }

        private void CompleteAndDespawn()
        {
            if (state == BusFlowBusState.Done)
            {
                return;
            }

            state = BusFlowBusState.Done;
            launchTween.Stop();
            driveTween.Stop();

            BusFlowGameManager.Ins.OnBusExited(this);

            Destroy(gameObject);
        }

        public float GetCurrentSplineDistance()
        {
            if (pickupPath != null)
            {
                return pickupPath.GetNearestDistanceToWorldPoint(transform.position);
            }

            return 0f;
        }

        public void PauseDriving()
        {
            driveTween.Stop();
        }

        public void ResumeDrivingLoop()
        {
            if (state == BusFlowBusState.DrivingToExitJunction || state == BusFlowBusState.DrivingExit || remaining <= 0)
            {
                StartExit();
            }
            else
            {
                float dist = GetCurrentSplineDistance();
                StartDrivingLoop(dist);
            }
        }

        // Di chuyển xe bị gộp về phía xe survivor, co nhỏ dần và biến mất
        public void AnimateMergeInto(Vector3 targetWorldPos, float duration, System.Action onComplete)
        {
            state = BusFlowBusState.Done;
            launchTween.Stop();
            driveTween.Stop();

            targetWorldPos.z = transform.position.z;

            Tween.Position(transform, targetWorldPos, duration: duration, ease: Ease.InQuad);
            Tween.Scale(transform, Vector3.zero, duration: duration, ease: Ease.InQuad)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    Destroy(gameObject);
                });
        }

        // Xe survivor nhận các xe khác gộp vào: scale to lên rồi về bình thường, cập nhật số lượng và tiếp tục chạy
        public void PlayMergedSuccessEffect(int newRemaining)
        {
            SetRemaining(newRemaining);

            Vector3 normalScale = transform.localScale;
            Vector3 popScale = normalScale * 1.35f;

            Tween.Scale(transform, popScale, duration: 0.18f, ease: Ease.OutQuad)
                .OnComplete(() =>
                {
                    Tween.Scale(transform, normalScale, duration: 0.2f, ease: Ease.OutBack)
                        .OnComplete(() =>
                        {
                            ResumeDrivingLoop();
                        });
                });
        }

        // Dùng cho luật gộp 3 xe cùng màu: xe bị gộp vào bị loại ngay, không cần chạy hết đường.
        public void ForceRemoveWithoutExit()
        {
            state = BusFlowBusState.Done;
            launchTween.Stop();
            driveTween.Stop();

            Destroy(gameObject);
        }

        // Dùng cho booster 3 / gộp màu: cộng dồn số khách cần đón vào xe này.
        public void SetRemaining(int newRemaining)
        {
            remaining = newRemaining;

            UpdateNumberLabel();
        }

        // Booster 3: hoàn thành ngay tại chỗ (hút hết khách còn lại), không cần đi hết đường đón.
        public void ForceComplete()
        {
            remaining = 0;

            StartExit();
        }

        private void UpdateNumberLabel()
        {
            if (externalNumberText != null)
            {
                externalNumberText.text = remaining.ToString();
            }

            if (fallbackNumberLabel != null)
            {
                fallbackNumberLabel.text = remaining.ToString();
            }
        }

        private void OnMouseDown()
        {
            if (BusFlowGameManager.Ins == null || !BusFlowGameManager.Ins.IsBooster3ModeActive)
            {
                return;
            }

            if (state != BusFlowBusState.DrivingPickup)
            {
                return;
            }

            BusFlowGameManager.Ins.ResolveBooster3Target(this);
        }

        public static BaseBus CreatePlaceholder(Transform parent)
        {
            GameObject go = BusFlowVisualFactory.CreatePlaceholder(
                PrimitiveType.Cube,
                parent,
                new Vector3(0.9f, 0.5f, 1.4f),
                keepCollider: true);

            go.name = "Bus";

            return go.AddComponent<BaseBus>();
        }

        public static BaseBus CreateFromPrefab(GameObject prefab, Transform parent)
        {
            GameObject go = Instantiate(prefab, parent);
            go.name = "Bus";

            SkeletonAnimation spine = go.GetComponentInChildren<SkeletonAnimation>();
            SpriteRenderer lid = null;
            TMP_Text numberText = null;

            WashingMachineItem washingMachineItem = go.GetComponent<WashingMachineItem>();

            if (washingMachineItem != null)
            {
                lid = washingMachineItem.lid;
                numberText = washingMachineItem.numberText;
                if (spine == null)
                {
                    spine = washingMachineItem.skeletonAnimation;
                }

                Destroy(washingMachineItem);
            }

            BaseBus controller = go.AddComponent<BaseBus>();
            controller.externalSpine = spine;
            controller.externalLid = lid;
            controller.externalNumberText = numberText;

            return controller;
        }
    }
}
