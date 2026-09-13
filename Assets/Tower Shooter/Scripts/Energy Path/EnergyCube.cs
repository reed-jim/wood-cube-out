using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCube : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Coroutine moveCoroutine;

    public void Move(List<Vector2> energyPoints)
    {
        if (energyPoints == null || energyPoints.Count == 0)
            return;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveAlongPath(energyPoints));
    }

    private IEnumerator MoveAlongPath(List<Vector2> energyPoints)
    {
        foreach (Vector2 targetPoint in energyPoints)
        {
            Vector3 target = new Vector3(targetPoint.x, targetPoint.y, transform.position.z);

            while (Vector2.Distance(transform.position, targetPoint) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            // Make sure we land exactly on the point
            transform.position = target;
        }

        moveCoroutine = null;

        gameObject.SetActive(false);
    }
}
