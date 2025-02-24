using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShopBox : BaseBox
{
    private static ShopBox instance;
    

    public static ShopBox Setup(bool isSaveBox = false, UnityAction actionOpenBoxSave = null, Action<BaseBox> OnLoadedFirst = null, Action<BaseBox> OnLoaded = null)
    {
        if (instance == null)
        {
            BoxController.Instance.GetBoxAsync(
                instance,
                (m_instance) =>
                {
                    instance = (ShopBox)m_instance;
                    instance.Show();
                    instance.Init();
                    instance.InitState();
                    if (OnLoadedFirst != null)
                        OnLoadedFirst(instance);
                },
                (instance) =>
                {
                    instance.Show();
                    instance.InitBoxSave(isSaveBox, actionOpenBoxSave);
                    if (OnLoaded != null)
                        OnLoaded(instance);
                },
                PathPrefabs.SHOP_GAME_BOX);
        }
        else
        {
            instance.Show();
            instance.InitBoxSave(isSaveBox, actionOpenBoxSave);
            if (OnLoaded != null)
            {
                OnLoaded(instance);
                // instance.InitState();
            }

            instance.InitState();
        }

        return instance;
    }


    [SerializeField] private ShopSuggetPanel shopPanel;

    public void Init()
    {
        shopPanel.Init();
    }

    public void InitState()
    {
        shopPanel.InitState();
    }
}
