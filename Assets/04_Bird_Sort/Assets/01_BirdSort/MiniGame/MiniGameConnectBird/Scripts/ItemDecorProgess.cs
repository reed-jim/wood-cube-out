using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
public class ItemDecorProgess : MonoBehaviour
{
    public int id;
    public Image thumnail;
    public Image bgThumnail;
    public Image numProgress;

    public Image extraImage1;
    public Image extraImage2;
    public Text tvNumb;
    public Text tvNumb1;
    public Text tvNumb2;
    public Text tvNumbItem;
    public GameObject aura;
    public GameObject decorCurrent;
    public RectTransform landingPos;

    public DataGiftMiniGame dataGiftMiniGame;
    public string tempString;
    public Sprite locked;
    public Sprite unLocked;
    public Sprite numLocked;
    public Sprite numUnLocked;
    public Sprite numCurrent;
    public GameObject fireWork;
    public GameObject complete;
        
    public void InitState()
    {

        var giftData = GameController.Instance.dataContain.giftDatabase;

        SetUp(giftData.GetIconItem(dataGiftMiniGame.reward.giftType), dataGiftMiniGame.thumnailDecor, dataGiftMiniGame.reward.amount.ToString());


    }


    
    public void SetUp(Sprite paramThumnail, Sprite paramBg, string numb)
    {
        var giftData = GameController.Instance.dataContain.giftDatabase;
        bgThumnail.gameObject.SetActive(true);
        unFireFireWork();
        thumnail.gameObject.SetActive(true);
        extraImage1.gameObject.SetActive(false);
        extraImage2.gameObject.SetActive(false);
        thumnail.sprite = paramThumnail;
        bgThumnail.sprite = paramBg;
        tvNumb.text = "" + numb;
        tvNumbItem.text = "" + numb;
        aura.gameObject.SetActive(false);
        //decorCurrent.gameObject.SetActive(false);
        bgThumnail.sprite = unLocked;
        numProgress.sprite = numUnLocked;
        //complete.SetActive(true);
        thumnail.color = new Color32(255, 255, 255, 255);
        extraImage1.color = new Color32(255, 255, 255, 255);
        extraImage2.color = new Color32(255, 255, 255, 255);

        tvNumb.color = new Color32(255, 255, 255, 255);
        tvNumb1.color = new Color32(255, 255, 255, 255);
        tvNumb2.color = new Color32(255, 255, 255, 255);
        tvNumbItem.color = new Color32(255, 255, 255, 255);
        tempString = numb;
        //if (dataGiftMiniGame.reward.giftType == GiftType.BirdSkinWitch || dataGiftMiniGame.reward.giftType == GiftType.ThemeWitch || dataGiftMiniGame.reward.giftType == GiftType.BranchWitch)
       // {
          
       

       // }



    }
    public void HandleSpecialItem()
    {
        if (HomeController.Instance.miniGameController.connectBirdMG.FirstWeek != 1)
        {
            return;
        }

    }
    public void HandelSwichText(string param)
    {
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
        //buttonClaim.GetComponent<Image>().sprite = controller.gameAssets.sliverThumnails;

        thumnail.gameObject.SetActive(false);
        extraImage1.gameObject.SetActive(false);
        extraImage2.gameObject.SetActive(false);

        bgThumnail.gameObject.SetActive(false);
        tvNumb.text = "";
        tvNumb1.text = "";
        tvNumb2.text = "";
        //    tvNumbItem.text = "";
    }
    public void NotReady()
    {
        tvNumb.color = new Color32(118, 118, 118, 255);
        tvNumb1.color = new Color32(118, 118, 118, 255);
        tvNumb2.color = new Color32(118, 118, 118, 255);
        //tvNumbItem.color = new Color32(118, 118, 118, 255);
        bgThumnail.sprite = locked;
        //thumnail.color = new Color32(118, 118, 118, 255);
        extraImage1.color = new Color32(118, 118, 118, 255);
        extraImage2.color = new Color32(118, 118, 118, 255);
        numProgress.sprite = numLocked;
        complete.SetActive(false);
    }

    public void SetUpClaimed()
    {
        complete.SetActive(true);
    }
    public void HandleCurrentItem()
    {
        decorCurrent.gameObject.SetActive(true);
        if (HomeController.Instance.miniGameController.connectBirdMG.PointConnectBirdMiniGame >= dataGiftMiniGame.point)
        {
            aura.gameObject.SetActive(true);
        }

        numProgress.sprite = numCurrent;
        complete.SetActive(false);
    }

    public void OffText()
    {
        tvNumb.text = "";
        tvNumb1.text = "";
        tvNumb2.text = "";
        tvNumbItem.text = "";
    }

    public void fireFirework()
    {
        fireWork.gameObject.SetActive(true);
    }

    public void unFireFireWork()
    {
        
        fireWork.gameObject.SetActive(false);
    }

}
