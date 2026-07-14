using System;
using AntsFlow;
using PrimeTween;
using UnityEngine;

namespace AntsFlow
{
    public class WarriorAnt : BaseAnt
    {
        protected override void OnAntReachTarget()
        {
            Attack(onCompleteAction: () =>
            {
                ReverseMove();
            });
        }

        private void Attack(Action onCompleteAction)
        {
            Transform missile = ObjectPoolingEverything.GetFromPool<Transform>(GameConstants.missile);

            Transform target = SpecialElementManager.Ins.bossArea.transform;

            missile.transform.position = transform.position;

            Rotate(missile, target.transform.position - missile.transform.position);

            Tween.Position(missile, target.transform.position, duration: 0.3f).OnComplete(() =>
            {
                ObjectPoolingEverything.ReturnToPool(GameConstants.missile, missile.gameObject);

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
