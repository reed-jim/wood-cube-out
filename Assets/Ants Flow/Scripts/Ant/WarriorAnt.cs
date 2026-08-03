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
        [SpineAnimation] public string attackAnimation;

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
            skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);

            this.faction = faction;

            stuckTime = 0;
            isMovingToSideTile = false;

            skeletonAnimation.transform.localPosition = skeletonAnimationInitialPosition;
        }

        private void OnSpineEvent(TrackEntry trackEntry, Spine.Event e)
        {
            if (trackEntry.Animation.Name == attackAnimation && e.Data.Name == "call")
            {
                Attack(onCompleteAction: () =>
                {
                    ReverseMove();
                });
            }
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

            missile.position = transform.position.ChangeZ(target.position.z - 0.1f);

            Rotate(missile, target.position - missile.position);

            Tween.Position(missile, target.position, duration: 0.3f).OnComplete(() =>
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
