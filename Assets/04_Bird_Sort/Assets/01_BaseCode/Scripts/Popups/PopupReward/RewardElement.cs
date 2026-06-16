using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RewardElement : MonoBehaviour
{
    public Image iconImg;
    [SerializeField] private Text valueTxt;
    [SerializeField] private GameObject rotateObj;
    [SerializeField] private GameObject unlimitedObj;
    [SerializeField] private GameObject x2ItemObj;
    [SerializeField] private GameObject fxXValueObj;

    private int currentValue;

    public virtual void Init(Sprite iconSpr, int value, GameObject rewardAnim = null, bool isAnim = true, bool isUnlimitedItem = false, bool isX2Item = false)
    {
        SetUpUI(iconSpr, value, rewardAnim, isAnim, isUnlimitedItem, isX2Item);

        if (isAnim)
        {
            this.transform.localScale = Vector3.zero;
            this.transform.DOKill();
            this.transform.DOScale(1, 0.3f).SetUpdate(true).SetEase(Ease.InBack).OnComplete(() => { if (rotateObj != null) rotateObj.gameObject.SetActive(true); });
        }
        else
        {
            this.transform.localScale = Vector3.one;
        }

        currentValue = value;
    }

    protected virtual void SetUpUI(Sprite iconSpr, int value, GameObject rewardAnim = null, bool isAnim = true, bool isUnlimitedItem = false, bool isX2Item = false)
    {
        iconImg.gameObject.SetActive(false);
        unlimitedObj.gameObject.SetActive(isUnlimitedItem);

        if (rewardAnim != null)
        {
            GameObject rw = SimplePool.Spawn(rewardAnim);
            Vector3 originalScale = rewardAnim.transform.localScale;

            rw.transform.localScale = originalScale;
            rw.transform.localPosition = Vector2.zero;
        }
        else if (iconImg != null && iconSpr != null)
        {
            iconImg.gameObject.SetActive(true);
            iconImg.sprite = iconSpr;
        }

        if (valueTxt != null)
        {
            valueTxt.gameObject.SetActive(true);
            if (isUnlimitedItem)
            {
                valueTxt.text = TimeManager.ShowTime4((long)value);
            }
            else
            {
                if (value == 0)
                {
                    valueTxt.text = "";
                }
                else
                    valueTxt.text = value.ToString();
            }
        }

        if (isX2Item)
        {
            if (x2ItemObj != null)
            {
                x2ItemObj.gameObject.SetActive(true);
                valueTxt.text = TimeManager.ShowTime4((long)value);
            }
        }
        else
        {
            if (x2ItemObj != null)
            {
                x2ItemObj.gameObject.SetActive(false);
            }
        }

        currentValue = value;
    }

    public void XValueElement(int newValue, bool isUnlimitedItem = false)
    {
        if (fxXValueObj != null)
            fxXValueObj.gameObject.SetActive(true);

        if (valueTxt != null)
        {
            if (newValue != 0)
            {
                int amount = currentValue;
                //valueTxt.gameObject.SetActive(true);
                //valueTxt.transform.DOScale(0, 0.15f).OnComplete(() =>
                //{
                DOTween.To(() => amount, x => amount = x, newValue, 0.75f)
                      .OnUpdate(() =>
                      {
                          if (isUnlimitedItem)
                          {
                              valueTxt.text = TimeManager.ShowTime4((long)amount);
                          }
                          else
                          {
                              valueTxt.text = amount.ToString();
                          }
                      })
                      .OnComplete(() =>
                      {
                          if (isUnlimitedItem)
                          {
                              valueTxt.text = TimeManager.ShowTime4((long)newValue);
                          }
                          else
                          {
                              valueTxt.text = newValue.ToString();
                          }
                      });


                // valueTxt.transform.DOScale(1, 0.15f).SetEase(Ease.InBack).SetDelay(0.05f);
                //}).SetEase(Ease.OutBack);
            }
            else
            {
                valueTxt.text = "";
            }
        }
    }
}