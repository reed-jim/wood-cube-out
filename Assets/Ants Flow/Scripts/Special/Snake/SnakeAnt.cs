using System;
using AntsFlow;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace AntsFlow
{
    public class SnakeAnt : BaseAnt
    {
        protected override void OnAntReachTarget()
        {
            Attack(onCompleteAction: () =>
            {
                ReverseMove();
            });
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
