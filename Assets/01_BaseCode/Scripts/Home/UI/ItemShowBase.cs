using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemShowBase : MonoBehaviour
{
    public CanvasGroup body;
    [SerializeField] protected Image iconImg;
    [SerializeField] protected Text amountTxt;
    [SerializeField] protected GameObject fxXValueObj;
    private int currentValue;

    public virtual void Init(RewardRandom reward)
    {
        iconImg.sprite = GameController.Instance.dataContain.giftDatabase.GetIconItem(reward.typeItem);
        amountTxt.text = reward.amount.ToString();

        if (fxXValueObj != null)
            fxXValueObj.gameObject.SetActive(false);

        currentValue = reward.amount;
    }

    public void XValueElement(int newValue, bool isUnlimitedItem = false)
    {
        if (fxXValueObj != null)
            fxXValueObj.gameObject.SetActive(true);

        if (amountTxt != null)
        {
            if (newValue != 0)
            {
                int amount = currentValue;
                amountTxt.gameObject.SetActive(true);
                // amountTxt.transform.DOScale(0, 0.15f).OnComplete(() =>
                // {
                DOTween.To(() => amount, x => amount = x, newValue, 0.75f)
                      .OnUpdate(() =>
                      {
                          if (isUnlimitedItem)
                          {
                              amountTxt.text = TimeManager.ShowTime4((long)amount);
                          }
                          else
                          {
                              amountTxt.text = amount.ToString();
                          }
                      })
                      .OnComplete(() =>
                      {
                          if (isUnlimitedItem)
                          {
                              amountTxt.text = TimeManager.ShowTime4((long)newValue);
                          }
                          else
                          {
                              amountTxt.text = newValue.ToString();
                          }
                      });


                // amountTxt.transform.DOScale(1, 0.15f).SetEase(Ease.InBack).SetDelay(0.05f);
                // }).SetEase(Ease.OutBack);
            }
            else
            {
                amountTxt.text = "";
            }
        }
    }
}
