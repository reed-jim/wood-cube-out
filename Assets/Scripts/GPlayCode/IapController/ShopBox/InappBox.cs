using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class InappBox : BaseBox
{
    private static InappBox instance;

    public static InappBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<InappBox>(PathPrefabs.SHOP_BOX));
        }

        return instance;
    }

    protected override void OnStart()
    {
        base.OnStart();

        InitData();
    }

    public override void Show()
    {
        base.Show();
    }

    private void InitData()
    {

    }
}