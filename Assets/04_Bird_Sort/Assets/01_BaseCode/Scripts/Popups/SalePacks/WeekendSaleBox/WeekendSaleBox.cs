using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeekendSaleBox : BasePackBox
{
    private static WeekendSaleBox instance;
    public static WeekendSaleBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<WeekendSaleBox>(PathPrefabs.WEEKLY_SALE_BOX));
            instance.Init();
        }
        instance.InitState();
        instance.isPostEventPopupClose = isPostEventPopupClose;
        return instance;
    }

    [SerializeField]private Text SaleText;

    public override void InitState()
    {
        base.InitState();
        SaleText.text = string.Format(Localization.Get("s_sale_off"), GameController.Instance.segmentController.GetCurrentSegment().saleData.config.weeklySalePack.salePercent.ToString("0.##"));
    }

    protected override void OnBuyPackSuccess()
    {
        Close();
    }
}