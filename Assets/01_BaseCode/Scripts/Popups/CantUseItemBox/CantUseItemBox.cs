//using MoreMountains.NiceVibrations;
using System.Collections;
using System.Collections.Generic;
using GPlay.GplayGoogleUMP;
using Gplay.Notifications;
using UnityEngine;
using UnityEngine.UI;
using GPlayUI;

public class CantUseItemBox : BaseBox
{
    private static CantUseItemBox instance;

    public static CantUseItemBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<CantUseItemBox>(PathPrefabs.CANT_USE_ITEM_BOX));
        }

        instance.gameObject.SetActive(true);
        return instance;
    }

    [SerializeField] private Text contentText;

    public void Show(GiftType giftType)
    {
        switch (giftType)
        {
            case GiftType.ClearHole:
                contentText.text = Localization.Get("s_noti_clear_hole");
                break;
        }
        base.Show();
    }
}