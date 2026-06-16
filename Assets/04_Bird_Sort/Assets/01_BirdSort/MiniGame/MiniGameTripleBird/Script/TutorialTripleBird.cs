using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialTripleBird : MonoBehaviour
{
    public GameObject hand;
    public TripleBirdMGLogic controller;
    private bool isOffTut = false;
    public void Init(TripleBirdMGLogic controller)
    {
        this.controller = controller;
        hand.SetActive(true);
        ScaleInOut();
        MoveToAnotherBird();
    }
    public void ScaleInOut()
    {
        hand.gameObject.SetActive(true);
        hand.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).OnComplete(() =>
         hand.transform.DOScale(new Vector3(1, 1, 1), 0.5f).OnComplete(delegate { ScaleInOut(); })
        );
    }
    public void MoveToAnotherBird()
    {
        for (int i = 0; i < controller.levelManager.lsAllBird.Count; i++)
        {
            if (!controller.levelManager.lsSort.Contains(controller.levelManager.lsAllBird[i]))
            {
                isOffTut = false;
                hand.transform.DOMove(controller.levelManager.lsAllBird[i].transform.position, 1);
                break;
            }
            else
            {
                isOffTut = true;
            }
        }
        if (isOffTut)
        {
            OffTut();
        }
    }
    public void OffTut()
    {
        hand.transform.DOKill();
        hand.gameObject.SetActive(false);
    }
}
