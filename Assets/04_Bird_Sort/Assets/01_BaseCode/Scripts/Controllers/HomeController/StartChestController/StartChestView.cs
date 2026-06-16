using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class StartChestView : MonoBehaviour
{
    [SerializeField] private GameObject chestObj;
    [SerializeField] private Transform posStarSpawn;
    //[SerializeField] private SkeletonAnimation chestAnim;
    [SerializeField] private GameObject chestAnim;
    [SerializeField] private StartChestEffect starClaim;
    [SerializeField] private Image progessImg;
    [SerializeField] private Button chestBtn;

    private StartChestController controller;

    public void Init(StartChestController controller)
    {
        this.controller = controller;
        starClaim.Init();
        progessImg.fillAmount = (float)StartChestController.RememberCurrentStar / controller.startChestData.startRequirement;
        chestBtn.onClick.AddListener(OnClickChest);

        if(UseProfile.CurrentLevel < controller.startChestData.unlockLevel)
        {
            chestObj.gameObject.SetActive(false);
        }    
        else
        {
            chestObj.gameObject.SetActive(true);
        }    
    }

    public void InitState()
    {
        if (UseProfile.CurrentLevel < controller.startChestData.unlockLevel)
            return;

        if (StartChestController.RememberCurrentStar != StartChestController.CurrentStar)
        {
            int remberCurrentStar = StartChestController.RememberCurrentStar;
            int currentStar = StartChestController.CurrentStar;

            UnityAction actionStar = () => 
            {
                starClaim.ShowStar(
              posStarSpawn.position,
              chestAnim.transform.position,
              Mathf.Abs(currentStar - remberCurrentStar),
              actionStarDone: () =>
              {
                  float currentProgess = (float)currentStar / controller.startChestData.startRequirement;
                  progessImg.DOFillAmount(currentProgess, 0.3f).OnComplete(() => { controller.actionOpenChest?.Invoke(); });
              });
            };


            if(HomeController.Instance.suggetPopupHomeController.IsHasSuperStarLegendaryShow)
            {
                StartCoroutine(Helper.StartAction(() =>
                {
                    actionStar.Invoke();
                }, () => HomeController.Instance.suggetPopupHomeController.IsHasSuperStarLegendaryShow == false));

            }  
            else
            {
                actionStar.Invoke();
            }

           
        }    
    }

    public void OnOpenChestDone()
    {
        float currentProgess = (float)StartChestController.CurrentStar / controller.startChestData.startRequirement;
        progessImg.fillAmount = currentProgess;
    }    
    
    private void OnClickChest()
    {
        if (StartChestController.CurrentStar < controller.startChestData.startRequirement)
        {
            ChestStarInfoBox.Setup().Show();
        }    
        else
        {
            controller.OpenChestByButton();
        }
        GameController.Instance.musicManager.PlayClickSound();
    }    

}
