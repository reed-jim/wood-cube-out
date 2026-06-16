using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightElectricItemInGame : LightElectricItem
{
    public override void Init()
    {
        numIDBirdClear = 1;
    }


    protected override bool ActiveItemHandle()
    {
        numIDBirdClear = 1;
        return base.ActiveItemHandle();
    }

    public override bool IsCanUseItem()
    {
        return true;
    }
}
