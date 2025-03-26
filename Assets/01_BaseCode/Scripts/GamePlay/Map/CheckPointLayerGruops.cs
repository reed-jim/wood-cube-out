using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GruopCheckPoint
{
    public List<CheckPointTran> lstCheckPoints;
}

public class CheckPointLayerGruops : MonoBehaviour
{
    public List<GruopCheckPoint> gruops;
}
