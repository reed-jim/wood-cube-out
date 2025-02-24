using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartLine : MonoBehaviour
{
    private bool enableMove = true;
    private List<Vector2> wayPoints;
    private int wayPointIndex = 0;
    private float moveSpeed;

    //public static HeartLine Create(List<Vector2> positions, float moveSpeed)
    //{
    //    Transform heartEffectTransform = Instantiate(ConnectBirdMGBox.Setup(true).gameAssets.heartEffectPrefab, positions[0], Quaternion.identity);
    //    HeartLine heartLine = heartEffectTransform.GetComponent<HeartLine>();
    //    heartLine.Setup(positions, moveSpeed);
    //    return heartLine;
    //}

    private void Update()
    {
        HeartMove();
    }

    private void HeartMove()
    {
        if (wayPointIndex >= wayPoints.Count)
        {
            enableMove = false;
            Destroy(gameObject);
        }

        if (enableMove)
        {
            transform.position = Vector2.MoveTowards(transform.position,
            wayPoints[wayPointIndex], moveSpeed * Time.deltaTime);
            if ((Vector2)transform.position == wayPoints[wayPointIndex])
            {
                wayPointIndex++;
            }
        }
    }

    private void Setup(List<Vector2> positions, float moveSpeed)
    {
        this.wayPoints = new List<Vector2>();
        this.wayPoints = positions;
        this.moveSpeed = moveSpeed;
    }
}

