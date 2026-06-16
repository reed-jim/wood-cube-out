using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ClaimGiftMiniGameTripleBird : MonoBehaviour
{
    public Image thumnail;
    public Image bgThumnail;
    public Button buttonClaim;
    public Image extraImage1;
    public Image extraImage2;
    public Text tvNumb;
    public Text tvNumb1;
    public Text tvNumb2;
    public TripleBirdMGLogic controller;
    public GameObject Hand;
    public Text newText;
    public void Init(TripleBirdMGLogic param)
    {
        controller = param;
    }
    public void SetUp(Sprite paramThumnail, Sprite paramBg, string numb)
    {
        bgThumnail.gameObject.SetActive(true);
        buttonClaim.gameObject.SetActive(true);
        thumnail.gameObject.SetActive(true);
        extraImage1.gameObject.SetActive(false);
        extraImage2.gameObject.SetActive(false);
        thumnail.sprite = paramThumnail;
        bgThumnail.sprite = paramBg;
        tvNumb.text = "" + numb;
        newText.text = "";
        if (controller.scoreBar.currentGirt.reward.giftType == GiftType.EventTripleBirdBranch || controller.scoreBar.currentGirt.reward.giftType == GiftType.EventTripleBirdTheme)
        {
            thumnail.GetComponent<RectTransform>().localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
        else
        {
            thumnail.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        }
        if (controller.scoreBar.currentGirt.reward.giftType != GiftType.EventTripleBirdBranch /*Anhtusz || controller.scoreBar.currentGirt.reward.giftType != GiftType.EventTripleBirdTheme || controller.scoreBar.currentGirt.reward.giftType != GiftType.BirdSkin_8_3*/)
        {
            tvNumb.text = "";
            newText.text = "" + numb;
        }
    }
    public void SetUpExtra(Sprite paramThumnail_1, Sprite paramThumnail_2, Sprite paramBg, int num1, int num2)
    {
        bgThumnail.gameObject.SetActive(true);
        buttonClaim.gameObject.SetActive(true);
        thumnail.gameObject.SetActive(false);
        extraImage1.gameObject.SetActive(true);
        extraImage2.gameObject.SetActive(true);
        extraImage1.sprite = paramThumnail_1;
        extraImage2.sprite = paramThumnail_2;
        bgThumnail.sprite = paramBg;
        newText.text = "";
        tvNumb1.text = "" + num1;
        tvNumb2.text = "" + num2;
    }
    public void SetUpComplete()
    {
        buttonClaim.onClick.RemoveAllListeners();
        buttonClaim.GetComponent<ScaleInOutButton>().StopScaleInOut();
        extraImage1.gameObject.SetActive(false);
        extraImage2.gameObject.SetActive(false);
        buttonClaim.gameObject.SetActive(false);
        tvNumb.text = "";
        tvNumb1.text = "";
        tvNumb2.text = "";
    }
    public void HandleActionClaimReward(Action action)
    {
        buttonClaim.onClick.RemoveAllListeners();
        buttonClaim.onClick.AddListener(() => action());
    }
}
