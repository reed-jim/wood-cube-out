using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaleItemBox : BasePackBox
{
    private static SaleItemBox instance;
    public static SaleItemBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<SaleItemBox>(PathPrefabs.SALE_ITEM_BOX));
            instance.Init();
        }
        instance.InitState();
        instance.isPostEventPopupClose = isPostEventPopupClose;
        return instance;
    }

    [SerializeField] private Text SaleText;

    public override void InitState()
    {
        base.InitState();
        SaleText.text = string.Format(Localization.Get("s_sale_off"), GameController.Instance.segmentController.GetCurrentSegment().saleData.config.itemSalePack.salePercent.ToString("0.##"));
    }

    public Text EndTime;
    protected override void OnBuyPackSuccess()
    {
        Close();
    }
}
