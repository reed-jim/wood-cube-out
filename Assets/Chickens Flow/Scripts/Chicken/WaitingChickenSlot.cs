using System;
using UnityEngine;

[Serializable]
public class WaitingChickenSlot
{
    public Vector3 position;
    public Vector3 randomPosition;
    public int column;
    public int row;

    public bool isWall;

    public BaseChicken chicken;

    public Vector3 GetRandomPosition()
    {
        float radius = 0.1f;

        randomPosition = position;
        randomPosition.x = position.x + UnityEngine.Random.Range(-radius, radius);
        randomPosition.y = position.y + UnityEngine.Random.Range(-radius, radius);

        return randomPosition;
    }
}
