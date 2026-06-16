using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayBox : LoseBox
{
    private static ReplayBox _instance;
    public static ReplayBox Setup2()
    {
        if (_instance == null)
        {
            _instance = Instantiate(Resources.Load<ReplayBox>(PathPrefabs.REPLAY_BOX));
            _instance.Init();
        }

        _instance.gameObject.SetActive(true);
        _instance.InitState();
        return _instance;
    }

    public override void InitState()
    {
        base.InitState();
        BirdSort2.GamePlayController_BirdSort.Instance.state = BirdSort2.StateGame.Pause;
    }

    public override void OnClickClose()
    {
        if (adsData.config.interstitials.showAfterQuit)
        {
            GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
            {
                Close();
                //  BirdSort2.GamePlayController_BirdSort.Instance.countDownController.ContinueCollDown();
                BirdSort2.GamePlayController_BirdSort.Instance.state = BirdSort2.StateGame.Playing;
            }, actionWatchLog: AdsPlacement.Interstitials.REPLAY_LEVEL_CAMPAIN);
        }
        else
        {
            Close();
            //  BirdSort2.GamePlayController_BirdSort.Instance.countDownController.ContinueCollDown();
            BirdSort2.GamePlayController_BirdSort.Instance.state = BirdSort2.StateGame.Playing;
        }
    }
}
