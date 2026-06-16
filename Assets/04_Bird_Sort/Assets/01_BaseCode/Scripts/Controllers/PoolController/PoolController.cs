using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolController : MonoBehaviour
{
    public bool isLoadBirdDone;

  
    public PoolBirds poolBirds;


    public void Init()
    {
        isLoadBirdDone = false;
        poolBirds.Init();
    }


}
