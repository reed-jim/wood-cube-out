using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeHomeController : MonoBehaviour
{
    public List<ChallengeViewBase> challegensView;

    public void Init()
    {
        for (int i = 0; i < challegensView.Count; i++)
        {
            challegensView[i].AwakeHomeSetup();
        }
    }    
}
