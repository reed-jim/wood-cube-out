using System.Collections.Generic;
using UnityEngine;

public class DotConnectManager : MonoBehaviour
{
    [SerializeField] private List<BaseDot> dotList;

    void Awake()
    {
        BaseDot.selectDotEvent += ConnectDot;

        dotList = new List<BaseDot>();
    }

    void OnDestroy()
    {
        BaseDot.selectDotEvent -= ConnectDot;
    }

    private void ConnectDot(BaseDot dot, bool isConnect)
    {
        if (dotList.Count > 0)
        {

        }
        else
        {
            if (isConnect)
            {
                dotList.Add(dot);
            }
            else
            {
                dotList.Remove(dot);
            }
        }
    }
}
