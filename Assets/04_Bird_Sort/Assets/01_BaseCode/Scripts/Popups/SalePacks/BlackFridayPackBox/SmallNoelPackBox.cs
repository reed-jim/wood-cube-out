using EventDispatcher;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using static SaleSegemntData;

public class SmallNoelPackBox : BasePackBox
{
    private static SmallNoelPackBox instance;
    public static SmallNoelPackBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<SmallNoelPackBox>(PathPrefabs.SMALL_NOEL_PACK_BOX));
            instance.Init();
        }
        instance.InitState();
        instance.isPostEventPopupClose = isPostEventPopupClose;
        return instance;
    }


    [SerializeField] private Text SaleText;

    private EventBase eventSaleController;
    private EndlessSale config;

    bool isCounting = false;
    public override void InitState()
    {
        base.InitState();
        isPlayAnime = false;
        //SaleText.text = string.Format(Localization.Get("s_sale_off"), GameController.Instance.segmentController.GetCurrentSegment().saleData.config.dailyOfferPack.salePercent.ToString("0.##"));

        config = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive();
        eventSaleController = GameController.Instance.eventSaleController.GetEventSales(config.gameTheme);

        isCounting = true;
    }

    public Text EndTime;

    private bool isPlayAnime = false;

    private void Update()
    {
        if (EndTime != null && (pack.PackTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds > 0)
        {
            EndTime.text = $"{TimeManager.ShowTime((long)(pack.PackTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds)}";
            if ((pack.PackTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds < 1)
            {
                Close();
            }
        }
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
