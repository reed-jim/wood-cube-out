using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class IAPShopItem : MonoBehaviour
{
    [SerializeField] private string productId;

    [SerializeField] private Button buyButton;

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    #endregion

    public static event Action<string> buyIAPEvent;

    private void Awake()
    {
        buyButton.onClick.AddListener(BuyIAP);

        _tweens = new List<Tween>();
    }

    private void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
    }

    private void BuyIAP()
    {
        GamePersistentVariable.lastPurchasedPackPosition = buyButton.transform.position;
        buyIAPEvent?.Invoke(productId);

        buyButton.interactable = false;

        _tweens.Add(Tween.Delay(1f).OnComplete(() =>
        {
            buyButton.interactable = true;
        }));
    }
}
