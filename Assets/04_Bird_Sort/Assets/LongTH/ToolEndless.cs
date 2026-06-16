using EnhancedScrollerDemos.SnappingDemo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SuggetPopupHomeController;
using System;
using static SaleSegemntData;

public class ToolEndless : MonoBehaviour
{
    [SerializeField] private TMP_InputField levelTxt;
    [SerializeField] private Button btn;
    [SerializeField] private Button reset;

    private EventBase eventSaleController;
    private EndlessSale config;
    // Start is called before the first frame update
    void Start()
    {
#if !TESTER
        gameObject.SetActive(false);
#endif

        config = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive();
        eventSaleController = GameController.Instance.eventSaleController.GetEventSales(config.gameTheme);

        btn.onClick.AddListener(() => AddItem());
        reset.onClick.AddListener(() => ResetItem());
    }

    private void ResetItem()
    {
        eventSaleController.data.CurrentIndexReward = 1;
        eventSaleController.data.RememberCurrentNumItem = eventSaleController.data.CurrentNumItem = 0;

        EndLessSaleBox.instance.StartPack = 0;
        EndLessSaleBox.instance.CurrentIndex = 0;
        EndLessSaleBox.instance.InitState();
    }

    private void AddItem()
    {
        int num = 0;
        try
        {
            num = int.Parse(levelTxt.text);
        }
        catch (FormatException)
        {
            num = -1;
        }

        if (levelTxt != null && num != -1) 
        {
            eventSaleController.data.RememberCurrentNumItem = eventSaleController.data.CurrentNumItem = num;

            foreach (var item in config.rewardData)
            {
                if (num >= item.requireToken) {
                    eventSaleController.data.CurrentIndexReward = config.rewardData.IndexOf(item) + 1; 
                }
            }

            EndLessSaleBox.instance.InitState();
        } 
        else
        {
            GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                    (
                     btn.transform.position,
                    "Nhập số vào bạn ei :>",
                    Color.white,
                    isSpawnItemPlayer: false
                    );
        }
    }

}
