using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
[Serializable]
//public class TripleBirdConfigData : MiniGameConfig
//{
//    public int version_except;
//}

public class TripleBirdMGController : MiniGameRegister
{
    #region KEY
    [HideInInspector] public string CONFIG_MINI_GAME_TRIPLE_BIRD = "config_mini_game_triple_bird";
    [HideInInspector] public static string VERSION_MINI_GAME_TRIPLE_BIRD = "version_mini_game_minigame";
    [HideInInspector] public static string POINT_MINIGAME_TRIPLE_BIRD = "point_minigame_connect_bird";
    [HideInInspector] public static string LIST_REWARD_MINI_GAME_TRIPLE_BIRD = "list_reward_mini_game_connect_bird";
    [HideInInspector] public static string WAS_COMMPLETE_MINI_GAME_TRIPLE_BIRD = "was_commplete_mini_game_connect_bird";
    [HideInInspector] public static string TRIPLE_BIRD_TURN_FREE = "connect_bird_turn_free";
    [HideInInspector] public string TRIPLE_BIRD_TURN_BUY = "connect_bird_turn_buy";
    [HideInInspector] public string LIMIT_NUMB_WATCH_VIDEO_IN_DAY_TRIPLE_BIRD = "limit_numb_watch_video_in_day";
    [HideInInspector] public static string FIRST_OPEN_TRIPLE_BIRD_MINI_GAME = "first_open_triple_bird_mini_game";
    [HideInInspector] public string FIRST_SHOW_HAND_TRIPLE_BIRD = "first_show_hand_triple_bird";
    [HideInInspector] public string CURRENT_NUM_WATCH_VIDEO_IN_DAY_MINI_GAME_TRIPLE_BIRD = "current_num_watch_video_in_day_mini_game_connect_bird";
    [HideInInspector] public string DATA_TO_RESET_MINI_GAME_TRIPLE_BIRD = "data_to_reset_mini_game_triple_bird";
    [HideInInspector] public string FIRST_WEEK_TRIPLE_BIRD = "first_week_triple_bird";
    #endregion
    public GameObject hand;
    // public Transform aura;
    public static int Version_Except = 2;

    public override void Init()
    {
     
        var conf = GameConfig;
        base.Init();
        LoadDataRewardTripleBirdMiniGame();
        Tutorial();
        CheckResetData();
     
    }

    [Button]
    private void DebugConfig()
    {
        var temp = JsonConvert.SerializeObject(GameConfig);
        Debug.LogError("-" + temp + "-");
    }    
    private void Update()
    {
        //aura.localEulerAngles += new Vector3(0, 0, 30) * Time.deltaTime;
    }
    public override int Version
    {

        get { return PlayerPrefs.GetInt(VERSION_MINI_GAME_TRIPLE_BIRD, 0); }
        set { PlayerPrefs.SetInt(VERSION_MINI_GAME_TRIPLE_BIRD, value); }
    }

    public int PointTripleBirdMiniGame
    {
        get
        {
            return PlayerPrefs.GetInt(POINT_MINIGAME_TRIPLE_BIRD, 0);
        }
        set
        {
            PlayerPrefs.SetInt(POINT_MINIGAME_TRIPLE_BIRD, value);
            // GameController.Instance.playerData.SaveData();
        }
    }

    // List chứa id những phần quà người chơi đã nhận được
    public List<int> lsRewardTripleBirdMNGOfPlayer;
    //Các phương thức xử dụng list data id
    public void AddAndSaveDataReward(int id)
    {
        lsRewardTripleBirdMNGOfPlayer.Add(id);
        string data = JsonConvert.SerializeObject(lsRewardTripleBirdMNGOfPlayer);
        PlayerPrefs.SetString(LIST_REWARD_MINI_GAME_TRIPLE_BIRD, data);
        // GameController.Instance.playerData.SaveData();
    }
    public void LoadDataRewardTripleBirdMiniGame()
    {
        var data = JsonConvert.DeserializeObject<List<int>>(PlayerPrefs.GetString(LIST_REWARD_MINI_GAME_TRIPLE_BIRD, "[]"));
       
        if (data != null)
        {
            lsRewardTripleBirdMNGOfPlayer.Clear();
            lsRewardTripleBirdMNGOfPlayer = data;
        }
    }
    public void ResetDataReadTripleBirdMiniGame()
    {
        lsRewardTripleBirdMNGOfPlayer.Clear();
        string data = JsonConvert.SerializeObject(lsRewardTripleBirdMNGOfPlayer);
        PlayerPrefs.SetString(LIST_REWARD_MINI_GAME_TRIPLE_BIRD, data);
    }
    public int GetRewardMiniGameOfPlay(int param)
    {
        if (lsRewardTripleBirdMNGOfPlayer.Count > 0)
        {
            foreach (int item in lsRewardTripleBirdMNGOfPlayer)
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
    public static bool WasCommpleteTripleBirdMiniGame
    {
        get
        {
            return PlayerPrefs.GetInt(WAS_COMMPLETE_MINI_GAME_TRIPLE_BIRD, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(WAS_COMMPLETE_MINI_GAME_TRIPLE_BIRD, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    public int FirstWeek
    {
        get
        {
            return PlayerPrefs.GetInt(FIRST_WEEK_TRIPLE_BIRD, 0);
        }
        set
        {
            PlayerPrefs.SetInt(FIRST_WEEK_TRIPLE_BIRD, value);
        }
    }




    //Số lượt quay còn lại trong ngày
    public int TripleBirdTurnFree
    {
        get
        {
            return PlayerPrefs.GetInt(TRIPLE_BIRD_TURN_FREE, config.num_turn_free);
        }
        set
        {
            PlayerPrefs.SetInt(TRIPLE_BIRD_TURN_FREE, value);
            // GameController.Instance.playerData.SaveData();
        }
    }

    //Số lượt mua thêm
    public int TripleBirdTurnBuy
    {
        get
        {
            return PlayerPrefs.GetInt(TRIPLE_BIRD_TURN_BUY, 0);
        }
        set
        {
            PlayerPrefs.SetInt(TRIPLE_BIRD_TURN_BUY, value);
        }
    }
    // Giới hạn xem quảng cáo
    public int LimitNumbWatchVideoInDay
    {
        get
        {
            return PlayerPrefs.GetInt(LIMIT_NUMB_WATCH_VIDEO_IN_DAY_TRIPLE_BIRD, config.num_watch_video_buy_egg);
        }
        set
        {
            PlayerPrefs.SetInt(LIMIT_NUMB_WATCH_VIDEO_IN_DAY_TRIPLE_BIRD, value);
        }
    }
    // first Open
    public static bool FirstOpenTripleBirdMNGame
    {
        get
        {
            return PlayerPrefs.GetInt(FIRST_OPEN_TRIPLE_BIRD_MINI_GAME, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(FIRST_OPEN_TRIPLE_BIRD_MINI_GAME, value ? 1 : 0);
            PlayerPrefs.Save();
            // GameController.Instance.playerData.SaveData();
        }
    }
    // first show home
    public bool FirstShowHand
    {
        get
        {
            return PlayerPrefs.GetInt(FIRST_SHOW_HAND_TRIPLE_BIRD, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(FIRST_SHOW_HAND_TRIPLE_BIRD, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }


    public bool wasChoose;


    //Số lần đã xem video để mua thêm lượt chơi trong ngày
    public int CurrentNumWatchVideoInDay
    {
        get
        {
            return PlayerPrefs.GetInt(CURRENT_NUM_WATCH_VIDEO_IN_DAY_MINI_GAME_TRIPLE_BIRD, 3);
        }
        set
        {
            PlayerPrefs.SetInt(CURRENT_NUM_WATCH_VIDEO_IN_DAY_MINI_GAME_TRIPLE_BIRD, value);
        }
    }

    private MiniGameConfig config;
    public override MiniGameConfig GameConfig
    {
        get
        {
            if (this.config != null)
                return this.config;

            if (RemoteConfigController.GetJsonConfig<MiniGameConfig>(FirebaseConfig.CONFIG_EVENT_GAME, out MiniGameConfig e_config))
            {

                this.config = e_config;
                DateTime.TryParse(this.config.str_start_date,out this.config.start_date);
                DateTime.TryParse(this.config.str_end_date,out this.config.end_date);
                // this.config.start_date = DateTime.Parse(this.config.str_start_date);
                // this.config.end_date = DateTime.Parse(this.config.str_end_date);
                Debug.Log("GameConfig " + this.config.min_level);
                Debug.Log("GameConfigisActive " + this.config.IsActive);
                return this.config;
            }
            else
            {


#if UNITY_EDITOR
                var m_config = new MiniGameConfig()
                {
                    version = 1,
                    num_turn_free = 3,
                    num_watch_video_buy_egg = 3,
                    is_active = true,
                    str_start_date = "2022/11/1 00:00:00",
                    str_end_date = "2022/11/30 00:00:00",
                    min_level = 16,
                    version_accept = 3
                };

                this.config = m_config;
                DateTime.TryParse(this.config.str_start_date,out this.config.start_date);
                DateTime.TryParse(this.config.str_end_date,out this.config.end_date);
                // this.config.start_date = DateTime.Parse(this.config.str_start_date);
                // this.config.end_date = DateTime.Parse(this.config.str_end_date);
                return this.config;
#else
                var m_config = new MiniGameConfig()
                {
                    version = 1,
                    num_turn_free = 3,
                    num_watch_video_buy_egg = 3,
                    is_active = false,
                    str_start_date = "2022/11/1 00:00:00",
                    str_end_date = "2022/11/30 00:00:00",
                    min_level = 16,
                    version_accept = 3
                };

                this.config = m_config;
                this.config.start_date = DateTime.Parse(this.config.str_start_date);
                this.config.end_date = DateTime.Parse(this.config.str_end_date);
                return this.config;
#endif
            }
        }
    }

    public override void ResetData()
    {
        if (!((MiniGameConfig)GameConfig).is_active)
        {
            return;
        }
        ResetDay();
        PointTripleBirdMiniGame = 0;
        ResetDataReadTripleBirdMiniGame();
        if (WasCommpleteTripleBirdMiniGame)
        {
            FirstWeek = 1;
        }
        WasCommpleteTripleBirdMiniGame = false;

    }

    public override void ResetDay()
    {
        if (!((MiniGameConfig)GameConfig).is_active)
        {
            return;
        }
        TripleBirdTurnFree = ((MiniGameConfig)GameConfig).num_turn_free;
        LimitNumbWatchVideoInDay = ((MiniGameConfig)GameConfig).num_watch_video_buy_egg;
        CurrentNumWatchVideoInDay = ((MiniGameConfig)GameConfig).num_watch_video_buy_egg;
    }

    public override void OpenBox()
    {
        TripleBirdMGBox.Setup().Show();
    }

    public void Tutorial()
    {
        //  Debug.LogError("FirstShowHand " + FirstShowHand);
        if (FirstOpenTripleBirdMNGame)
        {
            return;
        }
        if (GameConfig == null)
            return;
        if (GameConfig.is_active)
        {
            if (UseProfile.CurrentLevel >= GameConfig.min_level)
            {
                if (!FirstShowHand)
                {
                    hand.SetActive(true);
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
            if (!PlayerPrefs.HasKey(DATA_TO_RESET_MINI_GAME_TRIPLE_BIRD))
            {

                var temp = JsonConvert.SerializeObject(UnbiasedTime.Instance.Now());
                PlayerPrefs.SetString(DATA_TO_RESET_MINI_GAME_TRIPLE_BIRD, temp);
                PlayerPrefs.Save();
                return UnbiasedTime.Instance.Now();
            }
            else
            {
                var oldata = JsonConvert.DeserializeObject<DateTime>(PlayerPrefs.GetString(DATA_TO_RESET_MINI_GAME_TRIPLE_BIRD));
                return oldata;
            }
        }
        set
        {

            var temp = JsonConvert.SerializeObject(value);
            PlayerPrefs.SetString(DATA_TO_RESET_MINI_GAME_TRIPLE_BIRD, temp);
        }
    }
    public void CheckResetData()
    {
        var timeNow = TimeManager.ParseTimeStartWeek(UnbiasedTime.Instance.Now());
        var timeOld = TimeManager.ParseTimeStartWeek(DataToResetMiniGame);
        var Caculate = TimeManager.CaculateTime(timeOld, timeNow);
        if (Caculate >= 7 * 86400)
        {
            //ResetData();

            DataToResetMiniGame = UnbiasedTime.Instance.Now();
        }

    }
}
