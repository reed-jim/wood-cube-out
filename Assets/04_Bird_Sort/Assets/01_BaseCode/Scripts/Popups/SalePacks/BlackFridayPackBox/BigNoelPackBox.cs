using EventDispatcher;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using static SaleSegemntData;

public class BigNoelPackBox : BasePackBox
{
    private static BigNoelPackBox instance;
    public static BigNoelPackBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<BigNoelPackBox>(PathPrefabs.BIG_NOEL_PACK_BOX));
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
