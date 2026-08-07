using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private bool isGhostAttack;
        private Action ghostAttackOnComplete;

        private Renderer warriorRenderer;
        private MaterialPropertyBlock warriorPropertyBlock;

        protected override void Awake()
        {
            base.Awake();
            warriorRenderer = GetComponentInChildren<Renderer>();
            warriorPropertyBlock = new MaterialPropertyBlock();
        }

        public void SetAlpha(float alpha)
        {
            if (skeletonAnimation != null && skeletonAnimation.Skeleton != null)
            {
                skeletonAnimation.Skeleton.A = alpha;
            }

            if (warriorRenderer != null)
            {
                if (warriorPropertyBlock == null)
                {
                    warriorPropertyBlock = new MaterialPropertyBlock();
                }
                warriorPropertyBlock.SetColor("_Color", ColorUtil.WithAlpha(faction, alpha));
                warriorRenderer.SetPropertyBlock(warriorPropertyBlock);
            }
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
            baseZPosition = -8f;
            ghostBaseZPosition = -9f;
            transform.position = transform.position.ChangeZ(baseZPosition);
            transform.localScale = Vector3.one;

            this.faction = faction;
            SetAlpha(1f);

            if (skeletonAnimation != null && skeletonAnimation.Skeleton != null)
            {
                skeletonAnimation.Skeleton.SetSlotsToSetupPose();
                skeletonAnimation.AnimationState.SetAnimation(0, moveAnimation, true);
                skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
            }

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
                Attack(onCompleteAction: DisappearAfterGhostAttack);
            }
        }

        public void GhostAttackBoss(Vector3 standPosition, Action onComplete)
        {
            isGhostAttack = true;
            ghostAttackOnComplete = onComplete;

            GamePersistentVariable.numActiveAnt++;

            Vector3 targetPos = standPosition.ChangeZ(ghostBaseZPosition);

            float speed = 10f;
            float distance = Vector2.Distance(transform.position, targetPos);
            float duration = Mathf.Max(0.2f, distance / speed);

            Vector2 dir = (Vector2)(targetPos - transform.position);
            Rotate(transform, dir);

            tweens.Add(Tween.Position(transform, targetPos, duration: duration, ease: Ease.Linear)
            .OnComplete(() =>
            {
                if (SpecialElementManager.Ins != null && SpecialElementManager.Ins.bossArea != null)
                {
                    Rotate(transform, SpecialElementManager.Ins.bossArea.transform.position - transform.position);
                }

                skeletonAnimation.AnimationState.SetAnimation(0, attackAnimation, false);
            }));
        }

        private void DisappearAfterGhostAttack()
        {
            float startAlpha = (skeletonAnimation != null && skeletonAnimation.Skeleton != null) ? skeletonAnimation.Skeleton.A : 1f;

            tweens.Add(Tween.Custom(startAlpha, 0f, duration: 0.3f, onValueChange: a =>
            {
                SetAlpha(a);
            })
            .OnComplete(() =>
            {
                gameObject.SetActive(false);

                SetAlpha(1f);

                if (tile != null)
                {
                    tile.Complete();
                    tile = null;
                }

                GamePersistentVariable.numActiveAnt--;

                isGhostAttack = false;

                Action callback = ghostAttackOnComplete;
                ghostAttackOnComplete = null;
                callback?.Invoke();
            }));
        }

        private Coroutine warriorMovingCoroutine;

        public new void MoveTo(BaseTile tile, int index)
        {
            List<Node> path = tile.path;

            if (warriorMovingCoroutine != null)
            {
                StopCoroutine(warriorMovingCoroutine);
            }

            if (antAntennaeAttachments != null && skeletonAnimation != null && skeletonAnimation.Skeleton != null)
            {
                for (int i = 0; i < antAntennaeAttachments.Count; i++)
                {
                    skeletonAnimation.Skeleton.SetAttachment(antAntennaeAttachments[i], antAntennaeAttachments[i]);
                }
            }

            warriorMovingCoroutine = StartCoroutine(MovingWarriorCurved(path));

            tile.isAntApproaching = true;
            tile.InvokeAntStartMoveToTileEvent();

            this.tile = tile;
        }

        private IEnumerator MovingWarriorCurved(List<Node> path)
        {
            if (path == null || path.Count == 0)
            {
                OnAntReachTarget();
                yield break;
            }

            List<Vector3> points = path.Select(item => item.initialPosition).ToList();

            int firstBottomMostPointIndex;


            bool isCenter = Math.Abs(transform.position.x) < 0.1f;

            if (isCenter)
            {
                int sign = 1;


                points.Insert(0, TileManager.Ins.antNest.position.ChangeY(-3));
                points.Insert(1, TileManager.Ins.antNest.position.ChangeY(-3).ChangeX(sign * 3f));
                points.Insert(2, TileManager.Ins.bottomLeftMostNode.initialPosition.ChangeX(sign * 3f));

                firstBottomMostPointIndex = 2;
            }
            else
            {
                points.Insert(0, TileManager.Ins.bottomLeftMostNode.initialPosition.ChangeX(transform.position.x));

                firstBottomMostPointIndex = 0;
            }

            float randomWiggleMagnitude = 0.5f * UnityEngine.Random.Range(0, 2);

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 point = points[i];

                bool isNearBottomMost = Mathf.Abs(point.y - TileManager.Ins.bottomLeftMostNode.initialPosition.y) < 0.1f;
                bool isNearLeftMost = Mathf.Abs(point.x - TileManager.Ins.bottomLeftMostNode.initialPosition.x) < 0.1f;
                bool isNearRightMost = Mathf.Abs(point.x - TileManager.Ins.bottomRightMostNode.initialPosition.x) < 0.1f;

                if (isNearBottomMost)
                {
                    continue;
                }

                if (isNearLeftMost)
                {
                    points[i] = points[i].AddX(-randomWiggleMagnitude);
                }

                if (isNearRightMost)
                {
                    points[i] = points[i].AddX(randomWiggleMagnitude);
                }
            }

            if (points.Count > firstBottomMostPointIndex + 2 &&
                points[firstBottomMostPointIndex].x > points[firstBottomMostPointIndex + 1].x &&
                points[firstBottomMostPointIndex].x < points[firstBottomMostPointIndex + 2].x)
            {
                points.RemoveAt(firstBottomMostPointIndex + 1);
            }

            if (points.Count > firstBottomMostPointIndex + 2 &&
                points[firstBottomMostPointIndex].x > points[firstBottomMostPointIndex + 2].x &&
                points[firstBottomMostPointIndex].x < points[firstBottomMostPointIndex + 1].x)
            {
                points.RemoveAt(firstBottomMostPointIndex + 1);
            }

            points = RemoveNearbyPoints(points, 0.8f * GamePersistentVariable.tileDistance.x);

            float totalDistance = 0f;
            if (points.Count > 0)
            {
                totalDistance += Vector2.Distance(points[0], transform.position);
                for (int i = 0; i < points.Count - 1; i++)
                {
                    totalDistance += Vector2.Distance(points[i], points[i + 1]);
                }
            }


            float speed = speedMove > 0 ? speedMove : (initialSpeedMove > 0 ? initialSpeedMove : 5f);
            float totalTime = totalDistance > 0 ? totalDistance / speed : 0.1f;

            int siblingIndex = transform.GetSiblingIndex();
            float zPosition = baseZPosition - 0.001f * siblingIndex;
            Quaternion targetRotation;

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 start = transform.position;
                Vector3 end = points[i];

                Vector2 direction = (Vector2)(end - start).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                targetRotation = Quaternion.Euler(0, 0, angle - 90f);

                float distance = Vector2.Distance(start, end);
                float timeNeeded = totalDistance > 0 ? (distance / totalDistance * totalTime) : 0f;
                float timePassed = 0f;

                while (timePassed < timeNeeded)
                {
                    timePassed += Time.deltaTime;
                    float t = timeNeeded > 0 ? (timePassed / timeNeeded) : 1f;

                    Vector3 pos = Vector3.Lerp(start, end, t);
                    transform.position = pos.ChangeZ(zPosition);

                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 2f * t);

                    yield return null;
                }

                transform.position = end.ChangeZ(zPosition);
            }

            OnAntReachTarget();
        }

        public override void MoveToTileGhostMode(BaseTile tile)
        {
            tile.isAntApproaching = true;
            this.tile = tile;

            float speed = 10;
            float zPosition = ghostBaseZPosition - 0.001f * transform.GetSiblingIndex();

            Vector3 tilePosition = TileManager.Ins.GetTilePosition(tile);
            Vector2 dir = (Vector2)(tilePosition - transform.position);
            float distance = dir.magnitude;
            float duration = distance / speed;

            Rotate(transform, dir);
            transform.position = transform.position.ChangeZ(zPosition);

            tweens.Add(Tween.Position(transform, tilePosition.ChangeZ(zPosition), duration: duration, ease: Ease.Linear)
            .OnComplete(() =>
            {
                OnAntReachTarget();
            }));
        }

        protected override void OnAntReachTarget()
        {
            float zPosition = (isGhostAttack ? ghostBaseZPosition : baseZPosition) - 0.001f * transform.GetSiblingIndex();
            transform.position = transform.position.ChangeZ(zPosition);


            if (SpecialElementManager.Ins != null && SpecialElementManager.Ins.bossArea != null)
            {
                Rotate(transform, SpecialElementManager.Ins.bossArea.transform.position - transform.position);
            }

            skeletonAnimation.AnimationState.SetAnimation(0, attackAnimation, false);
        }

        protected override void OnAntMovedToNest()
        {
            DisappearAfterGhostAttack();
        }

        private void Attack(Action onCompleteAction)
        {
            Transform missile = ObjectPoolingEverything.GetFromPool<Transform>(GameConstants.missile);
            SoundManager.Instance.PlaySound(GameConstants.warriorAntBowSound);

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
