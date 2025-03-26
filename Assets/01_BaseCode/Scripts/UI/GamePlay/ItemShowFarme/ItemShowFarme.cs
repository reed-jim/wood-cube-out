using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ItemShowFarme : MonoBehaviour
{
    private ChallengeType type;
    public Image iconItemImg;
    [SerializeField] protected Text numItemTxt;
    private float timer;
    [SerializeField] protected float timeLife;
    private int currentScore;
    [SerializeField] protected RectTransform contentRect;
    [SerializeField] protected CanvasGroup canvasGroup;

    public void Init(ChallengeType type)
    {
        timeLife = 2.5f;
        this.type = type;
        currentScore = 0;
        gameObject.SetActive(false);
        numItemTxt.text = "";
    }

    public void Show(Sprite iconSpr, int amount)
    {
        iconItemImg.sprite = iconSpr;
        numItemTxt.text = amount.ToString();
        timer = 0;
        this.gameObject.SetActive(true);
        //Anim Show
        contentRect.DOKill();
        contentRect.DOAnchorPosX(0, 0.3f).SetEase(Ease.OutBack);
    }

    public void UpdateScore(int amount)
    {
        currentScore += amount;
        numItemTxt.text = currentScore.ToString();
        numItemTxt.transform.DOKill();
        numItemTxt.transform.localScale = Vector3.one;
        numItemTxt.transform.DOScale(1.8f, 0.2f).OnComplete(() =>
        {
            numItemTxt.transform.DOScale(1f, 0.2f);
        });
    }

    public void Close()
    {
        //Anim

        contentRect.DOKill();
        contentRect.DOAnchorPosX(-300, 0.2f).OnComplete(() =>
        {
            this.gameObject.SetActive(false);
        }).SetEase(Ease.Unset);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= timeLife)
        {
            Close();
        }
    }
}
