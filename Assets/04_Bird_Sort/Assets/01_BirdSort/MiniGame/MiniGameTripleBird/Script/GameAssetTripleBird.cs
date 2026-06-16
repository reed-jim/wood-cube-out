using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using DG.Tweening;
using System;
public class GameAssetTripleBird : MonoBehaviour
{
    public Text textTurn;
    public Text textCoolDown;
    public Text tvTimeMin;
    public Text tvTimeSecond;
    public PointTripleBirdMiniGame point;
    public Sprite yellowThumnails;
    public Sprite sliverThumnails;
    public GameObject xObject;
    public TripleBirdMGLogic controller;
    public void InitState(TripleBirdMGLogic controller)
    {
        this.controller = controller;
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.MINI_GAME_CONNECT_BIRD_EGG, HandleShowEgg);
    }
    public void HandleShowEgg(object param)
    {
        var miniGameData = HomeController.Instance.miniGameController.tripleBirdMG;
        textTurn.text = "" + (miniGameData.TripleBirdTurnFree + UseProfile.ConnectBirdTurnBuy);
    }
    private void OnDisable()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.MINI_GAME_CONNECT_BIRD_EGG, HandleShowEgg);
    }
    public void SpawnXObject(Vector3 post)
    {
        var XObject = SimplePool2.Spawn(xObject, new Vector3(post.x, post.y + 0.3f, post.z), Quaternion.identity);
        XObject.transform.SetParent(controller.levelManager.Top.transform);
        XObject.GetComponent<XObject>().ScaleInOut();
    }
}
