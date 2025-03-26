using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class PackItem : MonoBehaviour
{
    private SalePackDataModel currentSalePack;
    public Image icon;
    public Text timeTxt;
    public Button showBtn;

    private double remainTime;
    // Update is called once per frame
    private void Update()
    {
        remainTime -= Time.deltaTime;
        if(remainTime <= 0)
        {
            remainTime = 0;
        }
        if (currentSalePack != null)
        {
            currentSalePack.CheckEndSale((long)remainTime);
            if(remainTime < 86400)
            {
                timeTxt.text = currentSalePack.GetRemainTimeFormat((long)remainTime);
            }
            else
            {
                timeTxt.text = "End in: " + currentSalePack.GetRemainTimeFormat((long)remainTime);
            }
        }
    }
    public void Init()
    {
        showBtn.onClick.AddListener(OnClickShowPack);
        ActiveBody();
    }

    public void ActiveBodyAnim()
    {
        this.gameObject.SetActive(true);
        this.transform.localScale = Vector3.zero;
        this.transform.DOScale(Vector3.one * 1.2f, 0.4f).OnComplete(() =>
        {
            this.transform.DOScale(Vector3.one, 0.3f);
        });
    }
    public void DeactivateBodyAnim()
    {
        this.transform.DOKill();
        this.transform.DOScale(Vector3.one * 1.2f, 0.4f).OnComplete(() =>
        {
            this.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
            {
                this.gameObject.SetActive(false);
            });
        });
    }
    public void ActiveBody()
    {
        this.gameObject.SetActive(true);
    }
    public void DeactivateBody()
    {
        this.gameObject.SetActive(false);
    }
    public void OnClickShowPack()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if(currentSalePack != null)
        {
            switch (currentSalePack.typePackIAP)
            {
                default:
                    break;
            }
        }
    }
    
}
