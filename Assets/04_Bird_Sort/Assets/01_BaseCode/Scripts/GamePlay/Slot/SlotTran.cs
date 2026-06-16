using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotTran : MonoBehaviour
{
    [HideInInspector] public int id;
    public BirdBase birdOn;
    public Transform posMiddle;
    public Transform posTrans;
    public int indexLayer;

    public bool IsEmpty
    {
        get
        {
            if (birdOn != null)
                return false;
            else
                return true;
        }
    }

    public int IdBirdOn
    {
        get
        {
            if (birdOn != null)
                return birdOn.id;
            else
                return -1;
        }
    }

    public void Init(int id)
    {
        this.id = id;
        birdOn = null;
    }

    public void Return()
    {
        birdOn = null;
    }    
}
