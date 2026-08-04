using System;
using AntsFlow;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace AntsFlow
{
    public class WarriorAnt : BaseAnt
    {
        private const string moveAnimation = "move";

        [SpineAnimation] public string attackAnimation;

        // Ghost/revive: warrior đứng ở rìa map bắn boss rồi biến mất (không quay về theo đường đi).
        private bool isGhostAttack;
        private Action ghostAttackOnComplete;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            skeletonAnimation.AnimationState.Event += OnSpineEvent;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            skeletonAnimation.AnimationState.Event -= OnSpineEvent;
        }

        public override void Init(Color faction)
        {
            ghostBaseZPosition = -8f;

            skeletonAnimation.Skeleton.SetSlotsToSetupPose();
            skeletonAnimation.AnimationState.SetAnimation(0, moveAnimation, true);
            skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);

            this.faction = faction;

            stuckTime = 0;
            isMovingToSideTile = false;

            isGhostAttack = false;
            ghostAttackOnComplete = null;

            skeletonAnimation.transform.localPosition = skeletonAnimationInitialPosition;
        }

        private void OnSpineEvent(TrackEntry trackEntry, Spine.Event e)
        {
            if (trackEntry.Animation.Name == attackAnimation && e.Data.Name == "call")
            {
                if (isGhostAttack)
                {
                    // Revive: bắn boss xong thì biến mất tại chỗ (rìa map), không quay về theo đường đi.
                    Attack(onCompleteAction: DisappearAfterGhostAttack);
                }
                else
                {
                    Attack(onCompleteAction: () =>
                    {
                        ReverseMove();
                    });
                }
            }
        }

        // Ghost/revive: đứng tại vị trí rìa map (standPosition) rồi bắn missile vào boss - không cần đường
        // đi tới boss. Dùng khi revive warrior ant box mà chưa có hành lang thông tới bossArea.
        public void GhostAttackBoss(Vector3 standPosition, Action onComplete)
        {
            isGhostAttack = true;
            ghostAttackOnComplete = onComplete;

            GamePersistentVariable.numActiveAnt++;

            Vector3 targetPos = standPosition.ChangeZ(ghostBaseZPosition);

            float speed = 10f;
            float distance = Vector2.Distance(transform.position, targetPos);
            float duration = Mathf.Max(0.2f, distance / speed);

            tweens.Add(Tween.Position(transform, targetPos, duration: duration, ease: Ease.Linear)
            .OnComplete(() =>
            {
                // Play anim tấn công; event "call" trong anim sẽ kích hoạt Attack() -> missile -> boss.Hit().
                skeletonAnimation.AnimationState.SetAnimation(0, attackAnimation, false);
            }));
        }

        private void DisappearAfterGhostAttack()
        {
            Vector3 antScale = transform.localScale;

            tweens.Add(Tween.Scale(transform, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);

                transform.localScale = antScale;

                GamePersistentVariable.numActiveAnt--;

                isGhostAttack = false;

                Action callback = ghostAttackOnComplete;
                ghostAttackOnComplete = null;
                callback?.Invoke();
            }));
        }
        protected override void OnAntReachTarget()
        {
            skeletonAnimation.AnimationState.SetAnimation(0, attackAnimation, false);
            //Attack(onCompleteAction: () =>
            //{
            //    ReverseMove();
            //});
        }

        protected override void OnAntMovedToNest()
        {
            Vector3 antScale = transform.localScale;

            tweens.Add(Tween.Position(transform, TileManager.Ins.antNest.transform.position.AddY(-0.65f), duration: 0.3f));
            tweens.Add(Tween.Scale(transform, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                tile.Complete();
                gameObject.SetActive(false);

                transform.localScale = antScale;

                isMovingToNest = false;
                isDroppingTile = false;
                tile = null;

                GamePersistentVariable.numActiveAnt--;
            }));

            GameController.Instance.ActionLimitQueue.PlayAction(ActionLimitQueue.NameAction.FxAntComeToNest);

            isDroppingTile = true;
        }

        private void Attack(Action onCompleteAction)
        {
            Transform missile = ObjectPoolingEverything.GetFromPool<Transform>(GameConstants.missile);

            Transform target = SpecialElementManager.Ins.bossArea.transform;
            float missileZ = SpecialElementManager.Ins.bossArea.lazyHealthBar.transform.position.z - 0.1f;

            missile.position = transform.position.ChangeZ(missileZ);

            Rotate(missile, target.position - missile.position);

            Tween.Position(missile, target.position.ChangeZ(missileZ), duration: 0.3f).OnComplete(() =>
            {
                ObjectPoolingEverything.ReturnToPool(GameConstants.missile, missile.gameObject);

                SpecialElementManager.Ins.bossArea.Hit();

                onCompleteAction?.Invoke();
            });
        }

        void Rotate(Transform target, Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.01f && !isDroppingTile)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);

                target.rotation = targetRotation;
            }
        }
    }
}
