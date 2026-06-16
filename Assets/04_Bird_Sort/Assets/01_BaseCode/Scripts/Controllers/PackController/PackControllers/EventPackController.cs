using System.Collections.Generic;
using UnityEngine;

public class EventPackController : MonoBehaviour
{
    public bool HadBoughtSaleEvent
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.BOUHGT_EVENT_SALE, 0) == 1;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.BOUHGT_EVENT_SALE, value ? 1 : 0);
            GPlayerPrefs.Save();
        }
    }

    public TypePackIAP OpenPack
    {
        get
        {
            return (TypePackIAP)PlayerPrefs.GetInt(StringHelper.EVENT_SALE_ACTIVE, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.EVENT_SALE_ACTIVE, (int)value);
            GPlayerPrefs.Save();
        }
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
