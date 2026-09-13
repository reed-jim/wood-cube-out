using UnityEngine;

public class EnergyPathManager : Singleton<EnergyPathManager>
{
    public Transform centerBottomPoint;
    public Transform leftBottomPoint;
    public Transform rightBottomPoint;
    public Transform centerTopPoint;
    public Transform leftTopPoint;
    public Transform rightTopPoint;

    public Vector2 CenterBottomPoint => centerBottomPoint.position;
    public Vector2 LeftBottomPoint => leftBottomPoint.position;
    public Vector2 RightBottomPoint => rightBottomPoint.position;
    public Vector2 CenterTopPoint => centerTopPoint.position;
    public Vector2 LeftTopPoint => leftTopPoint.position;
    public Vector2 RightTopPoint => rightTopPoint.position;
}
