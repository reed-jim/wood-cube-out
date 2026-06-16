using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPage : PageBase
{
    public ShopPanel shopPanel;

    public override void Init(PageController controller)
    {
        shopPanel.Init();
        base.Init(controller);
        
    }

    public override void InitState()
    {
        shopPanel.InitState();
    }
}
