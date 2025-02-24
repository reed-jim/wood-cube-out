using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

[Serializable]
//public class ConnectBirdConfigData : MiniGameConfig
//{
//}

public class ConnectBirdMGController : MiniGameRegister
{
    public GameObject hand;
    // public Transform aura;

    public override void Init()
    {
        var conf = GameConfig;
        base.Init();
        LoadDataRewardConnectBirdMiniGame();
        Tutorial();
        CheckResetData();
    }
    public override int Version
    {

        get { return UseProfile.Version; }
        set { UseProfile.Version = value; }
    }

    public int PointConnectBirdMiniGame
    {
        get
        {
            return UseProfile.PointConnectBirdMiniGame;
        }
        set
        {
            UseProfile.PointConnectBirdMiniGame = value;
            // GameController.Instance.playerData.SaveData();
        }
    }

    // List chứa id những phần quà người chơi đã nhận được
    public List<int> lsRewardConnectBirdMNGOfPlayer;

    //Các phương thức xử dụng list data id
    public void AddAndSaveDataReward(int id)
    {
        lsRewardConnectBirdMNGOfPlayer.Add(id);
        string data = JsonConvert.SerializeObject(lsRewardConnectBirdMNGOfPlayer);
        UseProfile.lsRewardConnectBirdMNGOfPlayer =  data;
    }
    public void LoadDataRewardConnectBirdMiniGame()
    {
        var data = JsonConvert.DeserializeObject<List<int>>(UseProfile.lsRewardConnectBirdMNGOfPlayer);
        if (data != null)
        {
            lsRewardConnectBirdMNGOfPlayer.Clear();
            lsRewardConnectBirdMNGOfPlayer = data;
        }
        Debug.Log("lsRewardConnectBirdMNGOfPlayer " + lsRewardConnectBirdMNGOfPlayer.Count);

    }
    public void ResetDataReaedConnectBirdMiniGame()
    {
        lsRewardConnectBirdMNGOfPlayer.Clear();
        string data = JsonConvert.SerializeObject(lsRewardConnectBirdMNGOfPlayer);
        UseProfile.lsRewardConnectBirdMNGOfPlayer =  data;
    }
    public int GetRewardMiniGameOfPlay(int param)
    {
        if (lsRewardConnectBirdMNGOfPlayer.Count > 0)
        {
            foreach (int item in lsRewardConnectBirdMNGOfPlayer)
            {
                if (item == param)
                {
                    return item;
                }
            }
            return 0;
        }
        else
        {
            return 0;
        }

    }
    public static bool WasCommpleteConnectBirdMiniGame
    {
        get
        {
            return UseProfile.WasCommpleteConnectBirdMiniGame == 1;
        }
        set
        {
            UseProfile.WasCommpleteConnectBirdMiniGame = value ? 1 : 0;
        }
    }
    public int FirstWeek
    {
        get
        {
            return UseProfile.FirstWeek;
        }
        set
        {
            UseProfile.FirstWeek = value;
        }
    }




    //Số lượt quay còn lại trong ngày
    public int ConnectBirdTurnFree
    {
        get
        {
            return UseProfile.ConnectBirdTurnFree;
        }
        set
        {
            UseProfile.ConnectBirdTurnFree = value;
            // GameController.Instance.playerData.SaveData();
        }
    }

   
  // Giới hạn xem quảng cáo
    public int LimitNumbWatchVideoInDay
    {
        get
        {
            return UseProfile.LimitNumbWatchVideoInDay;
        }
        set
        {
            UseProfile.LimitNumbWatchVideoInDay = value;
        }
    }
    // first Open
    public static bool FirstOpenConnectBirdMNGame
    {
        get
        {
            return UseProfile.FirstOpenConnectBirdMNGame == 1;
        }
        set
        {

            UseProfile.FirstOpenConnectBirdMNGame = value ? 1 : 0;
            PlayerPrefs.Save();
            // GameController.Instance.playerData.SaveData();
        }
    }
    // first show home
    public bool FirstShowHand
    {
        get
        {
            return UseProfile.FirstShowHand == 1;
        }
        set
        {
            UseProfile.FirstShowHand = value ? 1 : 0;
            PlayerPrefs.Save();
        }
    }


    public bool wasChoose;


    //Số lần đã xem video để mua thêm lượt chơi trong ngày
    public int CurrentNumWatchVideoInDay
    {
        get
        {
            return UseProfile.CurrentNumWatchVideoInDay;
        }
        set
        {
            Debug.Log("Current num " + value );
            UseProfile.CurrentNumWatchVideoInDay= value;
        }
    }

    private MiniGameConfig config;
    public override MiniGameConfig GameConfig
    {
        get
        {
            if (this.config != null)
                return this.config;
            this.config = GameController.Instance.segmentController.GetCurrentSegment().minigameConnectSegmentData
                .config;
            return this.config;
        }
    }
    public override void ResetData()
    {
        if (!((MiniGameConfig)GameConfig).IsActive)
        {
            return;
        }
        ResetDay();
        PointConnectBirdMiniGame = 0;
        ResetDataReaedConnectBirdMiniGame();
        if (WasCommpleteConnectBirdMiniGame)
        {
            FirstWeek = 1;
        }
        WasCommpleteConnectBirdMiniGame = false;
        UseProfile.ConnectBirdTurnBuy = 0;
        Debug.LogError("ResetDataMiniGame");
    }

    public override void ResetDay()
    {
        if (!((MiniGameConfig)GameConfig).IsActive)
        {
            return;
        }
        ConnectBirdTurnFree = ((MiniGameConfig)GameConfig).num_turn_free;
        LimitNumbWatchVideoInDay = ((MiniGameConfig)GameConfig).num_watch_video_buy_egg;
        CurrentNumWatchVideoInDay = ((MiniGameConfig)GameConfig).num_watch_video_buy_egg;

    }

    public override void OpenBox()
    {
        ConnectBirdMGBox.Setup();
    }

    public void Tutorial()
    {
        //  Debug.LogError("FirstShowHand " + FirstShowHand);
        if (FirstOpenConnectBirdMNGame)
        {
            return;
        }
        if (GameConfig == null)
            return;
        if (GameConfig.IsActive)
        {
            if (UseProfile.CurrentLevel >= GameConfig.min_level)
            {
                if (!FirstShowHand)
                {
                    //hand.SetActive(true);
                    FirstShowHand = true;
                }
                else
                {
                    hand.SetActive(false);
                }
            }
        }

    }



    public DateTime DataToResetMiniGame
    {
        get
        {
            return UseProfile.DataToResetMiniGame;
        }
        set
        {

            UseProfile.DataToResetMiniGame = value;
        }
    }
    public void CheckResetData()
    {
        var timeNow = TimeManager.ParseTimeStartDay(UnbiasedTime.Instance.Now());
        var timeOld = TimeManager.ParseTimeStartDay(DataToResetMiniGame);
        var Caculate = TimeManager.CaculateTime(timeOld, timeNow);

        if (Caculate >= 7 * 86400)
        {
            //ResetData();

            DataToResetMiniGame = UnbiasedTime.Instance.Now();
        }


    }
}
