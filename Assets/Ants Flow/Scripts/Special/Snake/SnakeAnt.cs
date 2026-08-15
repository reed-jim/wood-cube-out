using System;
using System.Collections.Generic;
using AntsFlow;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using Spine.Unity;

namespace AntsFlow
{
    public class SnakeAnt : BaseAnt
    {
        public override void Init(Color faction)
        {
            this.faction = faction;

            stuckTime = 0;
            isMovingToSideTile = false;

            skeletonAnimation.transform.localPosition = skeletonAnimationInitialPosition;

            if (SpecialElementManager.Ins != null && SpecialElementManager.Ins.snake != null)
            {
                Transform tailTrans = SpecialElementManager.Ins.snake.Tail;
                if (tailTrans != null)
                {
                    Renderer tailRenderer = tailTrans.GetComponentInChildren<Renderer>();
                    Renderer myRenderer = GetComponentInChildren<Renderer>();
                    if (tailRenderer != null && myRenderer != null)
                    {
                        myRenderer.sortingLayerID = tailRenderer.sortingLayerID;
                        myRenderer.sortingOrder = tailRenderer.sortingOrder + 1;
                    }
                }
            }
        }

        protected override void OnAntReachTarget()
        {
            Attack(onCompleteAction: () =>
            {
                DisappearIntoTail();
            });
        }

        // Guard bounds: đường đi tới tile có thể quá ngắn khi snake chiếm tile sát rìa map, khiến
        // points không đủ 3 phần tử tính từ firstBottomMostPointIndex → index+2 out of range.
        protected override void TrimRedundantPoints(List<Vector3> points, int firstBottomMostPointIndex)
        {
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
        }

        // points có thể rỗng khi toàn bộ đường về nằm ở hàng đáy (vd snake chiếm tile sát rìa map):
        // khi đó dùng điểm hàng đáy gần tổ nhất làm mốc để kiến vẫn quay về tổ, tránh points[^1]
        // văng ArgumentOutOfRangeException.
        protected override void AppendNearNestPointAndTrim(List<Vector3> points, Vector3 antNestPosition, Vector3 closestPointToNest)
        {
            Vector3 referencePoint = points.Count > 0 ? points[^1] : closestPointToNest;

            Vector3 nearAntNestPosition = antNestPosition + 1f * (referencePoint - antNestPosition).normalized;

            points.Add(nearAntNestPosition);

            if (points.Count > 1)
            {
                points.RemoveAt(0);
            }
        }

        private void DisappearIntoTail()
        {
            Vector3 antScale = transform.localScale;

            if (SpecialElementManager.Ins != null && SpecialElementManager.Ins.snake != null)
            {
                Transform tailTrans = SpecialElementManager.Ins.snake.Tail;
                if (tailTrans != null)
                {
                    tweens.Add(Tween.Position(transform, tailTrans.position, duration: 0.25f));
                }
            }

            tweens.Add(Tween.Scale(transform, 0, duration: 0.25f)
            .OnComplete(() =>
            {
                if (tile != null)
                {
                    tile.Complete();
                }
                gameObject.SetActive(false);

                transform.localScale = antScale;

                isMovingToNest = false;
                isDroppingTile = false;
                tile = null;

                GamePersistentVariable.numActiveAnt--;
            }));

            isDroppingTile = true;
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

        public override void MoveToTileGhostMode(BaseTile tile)
        {
            tile.isAntApproaching = true;

            this.tile = tile;

            float speed = 10;
            float distance;
            float duration;
            float zPosition = -7 - 0.001f * transform.GetSiblingIndex();
            if (SpecialElementManager.Ins != null && SpecialElementManager.Ins.snake != null && SpecialElementManager.Ins.snake.Tail != null)
            {
                zPosition = SpecialElementManager.Ins.snake.Tail.position.z - 0.1f - 0.001f * transform.GetSiblingIndex();
            }

            Vector3 tilePosition = TileManager.Ins.GetTilePosition(tile);

            Vector2 dir = (Vector2)(tilePosition - transform.position);
            distance = dir.magnitude;
            duration = distance / speed;

            Rotate(transform, dir);

            transform.position = transform.position.ChangeZ(zPosition);

            tweens.Add(Tween.Position(transform, tilePosition.ChangeZ(zPosition), duration: duration, ease: Ease.Linear)
            .OnComplete(() =>
            {
                Attack(onCompleteAction: () =>
                {
                    DisappearIntoTail();
                });
            }));
        }

        private void Attack(Action onCompleteAction)
        {
            SpecialElementManager.Ins.snake.Hit();

            onCompleteAction?.Invoke();
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
