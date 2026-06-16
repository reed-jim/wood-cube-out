using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using DG.Tweening;

public class ItemDecorProgessTripleBird : MonoBehaviour
{
    public int id;
    public Image thumnail;
    public Image bgThumnail;
    public Image extraImage1;
    public Image extraImage2;
    public Text tvNumb;
    public Text tvNumb1;
    public Text tvNumb2;
    public Text tvNumbItem;
    public GameObject aura;
    public GameObject decorCurrent;
    public DataGiftMiniGame dataGiftMiniGame;
    public string tempString;
    public void InitState()
    {
        var giftData = GameController.Instance.dataContain.giftDatabase;
        SetUp(giftData.GetIconItem(dataGiftMiniGame.reward.giftType), dataGiftMiniGame.thumnailDecor, dataGiftMiniGame.reward.amount.ToString());
    }
    public void SetUp(Sprite paramThumnail, Sprite paramBg, string numb)
    {
        var giftData = GameController.Instance.dataContain.giftDatabase;
        bgThumnail.gameObject.SetActive(true);

        thumnail.gameObject.SetActive(true);
        extraImage1.gameObject.SetActive(false);
        extraImage2.gameObject.SetActive(false);
        thumnail.sprite = paramThumnail;
        bgThumnail.sprite = paramBg;
        tvNumb.text = "" + numb;
        tvNumbItem.text = "";
        aura.gameObject.SetActive(false);
        decorCurrent.transform.DOKill();
        decorCurrent.gameObject.SetActive(false);
        bgThumnail.color = new Color32(255, 255, 255, 255);
        thumnail.color = new Color32(255, 255, 255, 255);
        extraImage1.color = new Color32(255, 255, 255, 255);
        extraImage2.color = new Color32(255, 255, 255, 255);
        tvNumb.color = new Color32(255, 255, 255, 255);
        tvNumb1.color = new Color32(255, 255, 255, 255);
        tvNumb2.color = new Color32(255, 255, 255, 255);
        tvNumbItem.color = new Color32(255, 255, 255, 255);
        tempString = numb;
        if (/*Anhtuz dataGiftMiniGame.reward.giftType == GiftType.BirdSkin_8_3 ||*/ dataGiftMiniGame.reward.giftType == GiftType.EventTripleBirdTheme || dataGiftMiniGame.reward.giftType == GiftType.EventTripleBirdBranch)
        {
            tvNumb.text = "";
            tvNumbItem.text = "" + numb;
        }
    }
    public void HandleSpecialItem()
    {
        if (HomeController.Instance.miniGameController.tripleBirdMG.FirstWeek != 1)
        {
            return;
        }
        if (/*Anhtusz dataGiftMiniGame.reward.giftType == GiftType.BirdSkin_8_3 &&*/ TripleBirdMGController.WasCommpleteTripleBirdMiniGame)
        {
            SetUpComplete();
        }
        else
        {
            var giftData = GameController.Instance.dataContain.giftDatabase;
            switch (dataGiftMiniGame.reward.giftType) // check 3 qua cuoi xem nguoi choi da co chua
            {
                case GiftType.EventTripleBirdBranch:
                    SetUp(giftData.GetIconItem(dataGiftMiniGame.reward.giftType), dataGiftMiniGame.thumnailDecor, "New Branch");
                    var oldBranchData = JsonConvert.DeserializeObject<List<int>>(GameController.Instance.useProfile.OwnedBranchSkin);
                    if (oldBranchData == null)
                    {
                        oldBranchData = new List<int>();
                    }
                    if (oldBranchData.Contains((int)ThemeType.EventTripleBird))
                    {
                        SetUp(giftData.GetIconItem(dataGiftMiniGame.rewardExtral[0].giftType), dataGiftMiniGame.thumnailDecor, dataGiftMiniGame.rewardExtral[0].amount.ToString());

                    }
                    break;
                case GiftType.EventTripleBirdTheme:
                    SetUp(giftData.GetIconItem(dataGiftMiniGame.reward.giftType), dataGiftMiniGame.thumnailDecor, "New Theme");
                    var oldThemeData = JsonConvert.DeserializeObject<List<int>>(GameController.Instance.useProfile.OwnedThemeSkin);
                    if (oldThemeData == null)
                    {
                        oldThemeData = new List<int>();
                    }
                    if (oldThemeData.Contains((int)ThemeType.EventTripleBird))
                    {
                        SetUp(giftData.GetIconItem(dataGiftMiniGame.rewardExtral[0].giftType), dataGiftMiniGame.thumnailDecor, dataGiftMiniGame.rewardExtral[0].amount.ToString());
                    }
                    break;
            }
        }
    }
    public void HandelSwichText(string param)
    {
        if (/*Anhtusz dataGiftMiniGame.reward.giftType == GiftType.BirdSkin_8_3 ||*/ dataGiftMiniGame.reward.giftType == GiftType.EventTripleBirdTheme || dataGiftMiniGame.reward.giftType == GiftType.EventTripleBirdBranch)
        {
            tvNumb.text = "";
            tvNumbItem.text = "" + param;
        }
    }
    public void SetUpExtra(Sprite paramThumnail_1, Sprite paramThumnail_2, Sprite paramBg, int num1, int num2)
    {
        bgThumnail.gameObject.SetActive(true);
        thumnail.gameObject.SetActive(false);
        extraImage1.gameObject.SetActive(true);
        extraImage2.gameObject.SetActive(true);
        extraImage1.sprite = paramThumnail_1;
        extraImage2.sprite = paramThumnail_2;
        bgThumnail.sprite = paramBg;
        tvNumb1.text = "" + num1;
        tvNumb2.text = "" + num2;
        tvNumbItem.text = "";
    }
    public void SetUpComplete()
    {
        extraImage1.gameObject.SetActive(false);
        extraImage2.gameObject.SetActive(false);
        tvNumb.text = "";
        tvNumb1.text = "";
        tvNumb2.text = "";
    }
    public void NotReady()
    {
        tvNumb.color = new Color32(118, 118, 118, 255);
        tvNumb1.color = new Color32(118, 118, 118, 255);
        tvNumb2.color = new Color32(118, 118, 118, 255);
        tvNumbItem.color = new Color32(118, 118, 118, 255);
        bgThumnail.color = new Color32(118, 118, 118, 255);
        thumnail.color = new Color32(118, 118, 118, 255);
        extraImage1.color = new Color32(118, 118, 118, 255);
        extraImage2.color = new Color32(118, 118, 118, 255);
    }

    public void HandleCurrentItem()
    {
        decorCurrent.transform.DOKill();
        decorCurrent.gameObject.SetActive(true);
        if (HomeController.Instance.miniGameController.tripleBirdMG.PointTripleBirdMiniGame >= dataGiftMiniGame.point)
        {
            aura.gameObject.SetActive(true);
        }
    }
    public void OffText()
    {
        tvNumb.text = "";
        tvNumb1.text = "";
        tvNumb2.text = "";
        tvNumbItem.text = "";
    }
}
