using EventDispatcher;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SmallBFPackBox : BasePackBox
{
    private static SmallBFPackBox instance;
    public static SmallBFPackBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<SmallBFPackBox>(PathPrefabs.SMALL_BLACK_FRIDAY_PACK_BOX));
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
        isPlayAnime = false;
        SaleText.text = string.Format(Localization.Get("s_sale_off"), GameController.Instance.segmentController.GetCurrentSegment().saleData.config.dailyOfferPack.salePercent.ToString("0.##"));
    }

    public Text EndTime;

    private bool isPlayAnime = false;

    private void Update()
    {
        
    }

    public override void Close()
    {
        if (!isPlayAnime)
        {
            base.Close();
            isPlayAnime = true;
        }
    }

    protected override void OnBuyPackSuccess()
    {
        Close();
    }
}
