using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class TutorialCoinItem : TutorialBase
{
    public const string TUT_COIN_ITEM_1 = "tut_coin_item_1";

    [SerializeField] private CanvasGroup tutObj;
    
    public override void Init(TutorialFunController controller)
    {
        base.Init(controller);
        handTut.gameObject.SetActive(false);
        tutObj.alpha = 0;
        tutObj.interactable = false;
        tutObj.blocksRaycasts = false;
    }

    // Update is called once per frame
    protected override void SetNameTut()
    {
        nameTut = TUT_COIN_ITEM_1;
    }
    public override bool IsCanShowTut()
    {
        if (UseProfile.CurrentLevel != GameController.Instance.segmentController.GetCurrentSegment().challengeData.GetChallengeConfig(ChallengeType.CoinChallenge).level_unlock)
            return false;
        return base.IsCanShowTut();
    }
    public override void StartTut()
    {
        if (!GamePlayController.Instance.challengeGameplayController.itemsChallenge.ContainsKey(ChallengeType.CoinChallenge))
            return;
        List<ItemChallenge> items = new List<ItemChallenge>(GamePlayController.Instance.challengeGameplayController.itemsChallenge[ChallengeType.CoinChallenge]);
        items = Helper.SelectionSortListMono<ItemChallenge>(items, (a, b) =>
        {
            if (a.transform.position.x > b.transform.position.x)
                return 1;
            else
                return -1;
        });

        handTut.gameObject.SetActive(true);
        handTut.transform.position = new Vector3(items[1].transform.position.x, items[1].transform.position.y, 0);
        //items[1].ChangeLayer(-100000000, LayerGame.BIRD_FLY);
        //items[1].SetLayerPosZ();
        //items[1].fsm.lockFSM = true;
        //Debug.Log("birds.Count " + birds.Count);


        tutObj.alpha = 1;
        tutObj.interactable = true;
        tutObj.blocksRaycasts = true;
    }

    public override bool IsCanEndTut()
    {
        return true;
    }
    public override void OnEndTut()
    {
        base.OnEndTut();
        handTut.gameObject.SetActive(false);
        tutObj.DOKill();
        tutObj.DOFade(0, 0.3f);
        tutObj.interactable = false;
        tutObj.blocksRaycasts = false;
    }
}
