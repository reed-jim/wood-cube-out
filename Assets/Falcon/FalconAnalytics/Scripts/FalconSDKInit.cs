using System.Collections;
using Falcon.FalconCore.Scripts.Controllers.Interfaces;
using UnityEngine;

public class FalconSDKInit : IFInit
{
    public IEnumerator Init()
    {
#if USE_APPSFLYER || USE_ADJUST
        GameObject obj = new GameObject("FalconSDKInit");
        obj.AddComponent<FalconAppsFlyerAndAdjust>();
#endif
        yield return null;
    }
}