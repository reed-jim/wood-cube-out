using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaleCoinBox : BasePackBox
{
    private static SaleCoinBox instance;
    public static SaleCoinBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<SaleCoinBox>(PathPrefabs.SALE_COIN_BOX));
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
        SaleText.text = string.Format(Localization.Get("s_sale_off"), GameController.Instance.segmentController.GetCurrentSegment().saleData.config.coinSalePack.salePercent.ToString("0.##"));
    }

    public Text EndTime;

    protected override void OnBuyPackSuccess()
    {
        Close();
    }
}
