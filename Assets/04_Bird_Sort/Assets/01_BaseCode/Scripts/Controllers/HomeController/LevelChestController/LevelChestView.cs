using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class LevelChestView : MonoBehaviour
{
    [SerializeField] private GameObject chestObj;
    [SerializeField] private GameObject chestAnim;
    //[SerializeField] private SkeletonAnimation chestAnim;
    [SerializeField] private Image progessImg;
    [SerializeField] private Button chestBtn;

    private LevelChestController controller;

    public void Init(LevelChestController controller)
    {
        this.controller = controller;
        progessImg.fillAmount = (float)LevelChestController.RememberLevelChest / controller.levelChestData.levelSpacing;
        chestBtn.onClick.AddListener(OnClickChest);

        if (UseProfile.CurrentLevel < controller.levelChestData.unlockLevel)
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
        if (UseProfile.CurrentLevel < controller.levelChestData.unlockLevel)
            return;

        if (LevelChestController.CurrentLevelChest != LevelChestController.RememberLevelChest)
        {
            UnityAction actionLevel = () =>
            {
                float currentProgess = (float)LevelChestController.CurrentLevelChest / controller.levelChestData.levelSpacing;
                progessImg.DOFillAmount(currentProgess, 0.3f).OnComplete(() => { controller.actionOpenChest?.Invoke(); });
            };

            if (HomeController.Instance.suggetPopupHomeController.IsHasSuperStarLegendaryShow)
            {
                StartCoroutine(Helper.StartAction(() =>
                {
                    actionLevel.Invoke();
                }, () => HomeController.Instance.suggetPopupHomeController.IsHasSuperStarLegendaryShow == false));

            }
            else
            {
                actionLevel.Invoke();
            }
        }
    }

    public void OnOpenChestDone()
    {
        float currentProgess = (float)LevelChestController.CurrentLevelChest / controller.levelChestData.levelSpacing;
        progessImg.fillAmount = currentProgess;
    }

    private void OnClickChest()
    {
        if (LevelChestController.CurrentLevelChest < controller.levelChestData.levelSpacing - 1)
        {
            ChestLevelInfoBox.Setup().Show();
        }
        else
        {
            controller.OpenChestByButton();
        }
        GameController.Instance.musicManager.PlayClickSound();
    }
}
