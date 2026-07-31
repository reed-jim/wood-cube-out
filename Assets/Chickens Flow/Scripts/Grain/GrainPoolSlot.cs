using Saferio.Util.SaferioTween;
using UnityEngine;

public class GrainPoolSlot : MonoBehaviour
{
    public GrainPile grainPile;

    public float percentInLoopPath;

    public Transform topRightPoint;
    public Transform bottomRightPoint;

    public Vector3 topRight;
    public Vector3 bottomRight;
    public Vector3 topLeft;
    public Vector3 bottomLeft;

    public float sizeX;

    public void Setup(float sizeX)
    {
        topRight = topRightPoint.localPosition;
        bottomRight = bottomRightPoint.localPosition;

        topLeft = topRight.AddX(-0.8f * sizeX);
        bottomLeft = bottomRight.AddX(-0.8f * sizeX);

        this.sizeX = sizeX;
    }
}
