namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using System.Globalization;
    using BestHTTP.Extensions;
   // using MoreMountains.NiceVibrations;
    using Newtonsoft.Json;
    using Facebook.Unity;

    using EventDispatcher;

    public class UseProfile : MonoBehaviour
    {


        public static int CurrentLevel
        {
            get { return GPlayerPrefs.GetInt(StringHelper.CURRENT_LEVEL, 1); }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.CURRENT_LEVEL, value);
                GPlayerPrefs.Save();
            }
        }

        public static int CurrentLevelPlay
        {
            get { return PlayerPrefs.GetInt(StringHelper.CURRENT_LEVEL_PLAY, 1); }
            set
            {
                PlayerPrefs.SetInt(StringHelper.CURRENT_LEVEL_PLAY, value);
                PlayerPrefs.Save();
            }
        }

        public static bool IsRemoveAds
        {
            get { return PlayerPrefs.GetInt(StringHelper.REMOVE_ADS, 0) == 1; }
            set
            {
                PlayerPrefs.SetInt(StringHelper.REMOVE_ADS, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool OnVibration
        {
            get { return GPlayerPrefs.GetInt(StringHelper.ONOFF_VIBRATION, 1) == 1; }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.ONOFF_VIBRATION, value ? 1 : 0);
                //MMVibrationManager.SetHapticsActive(value);
                GPlayerPrefs.Save();
            }
        }

        public static bool OnSound
        {
            get { return GPlayerPrefs.GetInt(StringHelper.ONOFF_SOUND, 1) == 1; }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.ONOFF_SOUND, value ? 1 : 0);
                GameController.Instance.musicManager.SetSoundVolume(value ? 1 : 0);
                GPlayerPrefs.Save();
            }
        }

        public static bool OnMusic
        {
            get { return GPlayerPrefs.GetInt(StringHelper.ONOFF_MUSIC, 1) == 1; }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.ONOFF_MUSIC, value ? 1 : 0);
                GameController.Instance.musicManager.SetMusicVolume(value ? 0.7f : 0);
                GPlayerPrefs.Save();
            }
        }

        public static bool IsFirstTimeInstall
        {
            get { return GPlayerPrefs.GetInt(StringHelper.FIRST_TIME_INSTALL, 1) == 1; }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.FIRST_TIME_INSTALL, value ? 1 : 0);
                GPlayerPrefs.Save();
            }
        }

        public static int RetentionD
        {
            get { return GPlayerPrefs.GetInt(StringHelper.RETENTION_D, 0); }
            set
            {
                if (value < 0)
                    value = 0;

                GPlayerPrefs.SetInt(StringHelper.RETENTION_D, value);
                GPlayerPrefs.Save();
            }
        }










        public static int DaysPlayed
        {
            get { return GPlayerPrefs.GetInt(StringHelper.DAYS_PLAYED, 1); }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.DAYS_PLAYED, value);
                GPlayerPrefs.Save();
            }
        }

        public static int PayingType
        {
            get { return GPlayerPrefs.GetInt(StringHelper.PAYING_TYPE, 0); }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.PAYING_TYPE, value);
                GPlayerPrefs.Save();
            }
        }



        public static bool IsTutedReturn
        {
            get { return GPlayerPrefs.GetInt(StringHelper.IS_TUTED_RETURN, 0) == 1; }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.IS_TUTED_RETURN, value ? 1 : 0);
                GPlayerPrefs.Save();
            }
        }

        public static int CurrentNumReturn
        {
            get
            {
                return GPlayerPrefs.GetInt(StringHelper.CURRENT_NUM_RETURN,
                    RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_RETURN, 2));
            }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.CURRENT_NUM_RETURN, value);
                GPlayerPrefs.Save();
            }
        }

        public static int CurrentNumAddBranch
        {
            get
            {
                return GPlayerPrefs.GetInt(StringHelper.CURRENT_NUM_ADD_STAND,
                    RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_ADD_BRANCH, 1000));
            }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.CURRENT_NUM_ADD_STAND, value);
                GPlayerPrefs.Save();
            }
        }

        public static int CurrentNumRemoveBomb
        {
            get
            {
                return GPlayerPrefs.GetInt(StringHelper.CURRENT_NUM_REMOVE_BOMB,
                    RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_REMOVE_BOMB, 0));
            }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.CURRENT_NUM_REMOVE_BOMB, value);
                GPlayerPrefs.Save();
            }
        }

        public static int CurrentNumRemoveCage
        {
            get
            {
                return GPlayerPrefs.GetInt(StringHelper.CURRENT_NUM_REMOVE_CAGE,
                    RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_REMOVE_CAGE, 0));
            }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.CURRENT_NUM_REMOVE_CAGE, value);
                GPlayerPrefs.Save();
            }
        }

        public static int CurrentNumRemoveEgg
        {
            get
            {
                return GPlayerPrefs.GetInt(StringHelper.CURRENT_NUM_REMOVE_EGG,
                    RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_REMOVE_EGG, 0));
            }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.CURRENT_NUM_REMOVE_EGG, value);
                GPlayerPrefs.Save();
            }
        }

        public static int CurrentNumRemoveSleep
        {
            get
            {
                return GPlayerPrefs.GetInt(StringHelper.CURRENT_NUM_REMOVE_SLEEP,
                    RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_REMOVE_SLEEP, 0));
            }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.CURRENT_NUM_REMOVE_SLEEP, value);
                GPlayerPrefs.Save();
            }
        }

        public static int CurrentNumRemoveJail
        {
            get
            {
                return GPlayerPrefs.GetInt(StringHelper.CURRENT_NUM_REMOVE_JAIL,
                    RemoteConfigController.GetIntConfig(FirebaseConfig.DEFAULT_NUM_REMOVE_JAIL, 0));
            }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.CURRENT_NUM_REMOVE_JAIL, value);
                GPlayerPrefs.Save();
            }
        }

        public static bool IsTutedBuyStand
        {
            get { return PlayerPrefs.GetInt(StringHelper.IS_TUTED_BUY_STAND, 0) == 1; }
            set
            {
                PlayerPrefs.SetInt(StringHelper.IS_TUTED_BUY_STAND, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }




        public static int NumberOfAdsInPlay;

        public static int NumberOfAdsInDay
        {
            get { return GPlayerPrefs.GetInt(StringHelper.NUMBER_OF_ADS_IN_DAY, 0); }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.NUMBER_OF_ADS_IN_DAY, value);
                GPlayerPrefs.Save();
            }
        }



        public static int Coin
        {
            get { return GPlayerPrefs.GetInt(StringHelper.COIN, 3000); }
            set
            {
                int prevCoin = Coin;
                PlayerPrefs.SetInt(StringHelper.COIN, Mathf.Clamp(value, 0, value));
                GPlayerPrefs.Save();
                // EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CHANGE_COIN, prevCoin);
            }
        }

     



       

        public static int IQScore
        {
            get
            {
                return PlayerPrefs.GetInt(StringHelper.IQ_SCORE, 0);
            }
            set
            {

                GPlayerPrefs.SetInt(StringHelper.IQ_SCORE, value);
                GPlayerPrefs.Save();
            }
        }

        public static bool hadBuySuggest { get; set; }

        public static bool CanShowRate
        {
            get
            {
                return PlayerPrefs.GetInt(StringHelper.CAN_SHOW_RATE, 1) == 1;
            }
            set
            {
                GPlayerPrefs.SetInt(StringHelper.CAN_SHOW_RATE, value ? 1 : 0);
                GPlayerPrefs.Save();
            }
        }

        #region Item
        public static int GetNumItem(GiftType typeItem, int defaultNum = 5)
        {
            return PlayerPrefs.GetInt(StringHelper.NUM_ITEM + typeItem.ToString(),
                RemoteConfigController.GetIntConfig(String.Format(FirebaseConfig.DEFAULT_NUM_ITEM, typeItem.ToString()),
             GiftDatabase.DefaultItem.ContainsKey(typeItem) ? GiftDatabase.DefaultItem[typeItem] : defaultNum));
        }

        public static void SetItem(GiftType typeItem, int amount)
        {
            GPlayerPrefs.SetInt(StringHelper.NUM_ITEM + typeItem.ToString(), amount);
        }

        public static void AddItem(GiftType typeItem, int amount)
        {
            if (amount < 0)
            {
                if (IsUnlimitedItem(typeItem))
                    return;
            }
            int current = GetNumItem(typeItem);
            current += amount;

            GPlayerPrefs.SetInt(StringHelper.NUM_ITEM + typeItem.ToString(), current);
        }

        public static bool IsUseItem(GiftType typeItem)
        {
            if (!IsUnlimitedItem(typeItem) && GetNumItem(typeItem) <= 0)
            {
                UseItem(typeItem, false);
                return false;
            }

            return PlayerPrefs.GetInt(StringHelper.USE_ITEM + typeItem.ToString(), 0) == 0 ? false : true;
        }

        public static void UseItem(GiftType typeItem, bool isUse)
        {
            GPlayerPrefs.SetInt(StringHelper.USE_ITEM + typeItem.ToString(), isUse ? 1 : 0);
        }

        public static bool IsUnlimitedItem(GiftType typeItem)
        {
            if (typeItem == GiftType.ReturnItem_BirdSort || typeItem == GiftType.AddBrandItem_BirdSort || typeItem == GiftType.ShuffleItem_BirdSort || typeItem == GiftType.BreakTheRule_BirdSort)
                return false;

            return GetTimeEndUnlimitedItem(typeItem) > UnbiasedTime.Instance.Now();
        }

        public static System.DateTime GetTimeEndUnlimitedItem(GiftType typeItem)
        {
            if (GPlayerPrefs.HasKey(StringHelper.TIME_END_UNLIMITED_ITEM + typeItem))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(StringHelper.TIME_END_UNLIMITED_ITEM + typeItem));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                GPlayerPrefs.SetString(StringHelper.TIME_END_UNLIMITED_ITEM + typeItem, newDateTime.ToBinary().ToString());
                GPlayerPrefs.Save();
                return newDateTime;
            }
        }

        public static void AddTimeEndUnlimitedItem(GiftType typeItem, int seccons)
        {
            var time = GetTimeEndUnlimitedItem(typeItem);
            if (time < UnbiasedTime.Instance.Now())
                time = UnbiasedTime.Instance.Now();
            time = time.AddSeconds(seccons);
            GPlayerPrefs.SetString(StringHelper.TIME_END_UNLIMITED_ITEM + typeItem, time.ToBinary().ToString());
            GPlayerPrefs.Save();
        }

        public static void SetTimeEndUnlimitedItem(GiftType typeItem, long timeEnd)
        {
            GPlayerPrefs.SetString(StringHelper.TIME_END_UNLIMITED_ITEM + typeItem, timeEnd.ToString());
            GPlayerPrefs.Save();
        }

        public static int GetCurrentTurnSuggetBuyItemVideo(GiftType typeItem)
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_TURN_SUGGET_VIDEO_BUY_ITEM + typeItem, 0);
        }

        public static void AddTurnSuggetBuyItemVideo(GiftType typeItem, int value)
        {
            int current = GetCurrentTurnSuggetBuyItemVideo(typeItem);
            GPlayerPrefs.SetInt(StringHelper.CURRENT_TURN_SUGGET_VIDEO_BUY_ITEM + typeItem, current + value);
            GPlayerPrefs.Save();
        }

        public static void SetTurnSuggetBuyItemVideo(GiftType typeItem, int value)
        {
            GPlayerPrefs.SetInt(StringHelper.CURRENT_TURN_SUGGET_VIDEO_BUY_ITEM + typeItem, value);
            GPlayerPrefs.Save();
        }

        public static DateTime GetLastTimeOverSuggetVideoBuyItem(GiftType typeItem)
        {

            if (GPlayerPrefs.HasKey(StringHelper.LAST_TIME_OVER_SUGGET_VIDEO_BUY_ITEM + typeItem))
            {
                var temp = Convert.ToInt64(GPlayerPrefs.GetString(StringHelper.LAST_TIME_OVER_SUGGET_VIDEO_BUY_ITEM + typeItem));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                GPlayerPrefs.SetString(StringHelper.LAST_TIME_OVER_SUGGET_VIDEO_BUY_ITEM + typeItem, newDateTime.ToBinary().ToString());
                GPlayerPrefs.Save();
                return newDateTime;
            }
        }

        public static void SetLastTimeOverSuggetVideoBuyItem(GiftType typeItem, DateTime time)
        {
            GPlayerPrefs.SetString(StringHelper.LAST_TIME_OVER_SUGGET_VIDEO_BUY_ITEM + typeItem, time.ToBinary().ToString());
            GPlayerPrefs.Save();
        }
        #endregion
    }
}

    

     


       

      
     

    

 
