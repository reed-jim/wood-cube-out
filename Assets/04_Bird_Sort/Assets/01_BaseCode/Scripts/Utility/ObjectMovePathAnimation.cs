using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
public class ObjectMovePathAnimation : MonoBehaviour
{
    public List<Transform> pointTransforms;
    public PathType pathType = PathType.CatmullRom;
    public PathMode pathMode = PathMode.Full3D;
    public float delay = 0;
    public float duration = 1;
    public Ease ease = Ease.Linear;
    public int loop = 1;
    [ShowIf("loop", -1)] public LoopType loopType = LoopType.Restart;

    private Vector3[] wayPoints;
    void Start()
    {
        wayPoints = new Vector3[pointTransforms.Count];
        for (int i = 0; i < pointTransforms.Count; i++)
        {
            wayPoints[i] = pointTransforms[i].position;
        }
        transform.DOPath(wayPoints, duration, pathType, pathMode).SetEase(ease).SetLoops(loop, loopType).SetDelay(delay) ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
