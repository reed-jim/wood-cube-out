using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static SuggetBuyerInGameSegmentData;

namespace BirdSort2
{
    public class SuggetBuyerInGame : MonoBehaviour
    {
        public static int Space_Showed_Sugget_Start_Level = -1;
        public static int NumSuggetStartLevelInDay
        {
            get
            {
                return PlayerPrefs.GetInt("Num_Sugget_Start_Level_In_Day", 0);
            }
            set
            {
                PlayerPrefs.SetInt("Num_Sugget_Start_Level_In_Day", value);
            }
        }

        public static int Space_Showed_Sugget_End_Level = -1;
        public static int NumSuggetEndLevelInDay
        {
            get
            {
                return PlayerPrefs.GetInt("Num_Sugget_End_Level_In_Day", 0);
            }
            set
            {
                PlayerPrefs.SetInt("Num_Sugget_End_Level_In_Day", value);
            }
        }

        public class SuggetBuyerModel
        {
            public int id;
            public System.Func<bool> actionCheckCondition;
            public UnityAction actionShow;
        }    

        private List<SuggetBuyerModel> suggetsStartLevel;
        private List<SuggetBuyerModel> suggetsOnPopup;
        private List<SuggetBuyerModel> suggetsEndLevel;

        private SuggetBuyerInGameConfig config;
        private System.Action<object> actionClosePopup;

        public void Init()
        {
            config = GameController.Instance.segmentController.GetCurrentSegment().suggetBuyerInGameSegmentData.config;
            actionClosePopup = (sender) => { GamePlayController_BirdSort.Instance.state = StateGame.Playing; };
            EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.CLOSE_POPUP, actionClosePopup);

            InitSuggetStartLevel();
            InitSuggetEndLevel();
        }    


        #region Start Level
        public void InitSuggetStartLevel()
        {
            suggetsStartLevel = new List<SuggetBuyerModel>();
            suggetsStartLevel.Add(GetSuggetBuyItem());
        }

        public void OnStartLevel()
        {
            var _config = config.GetConfigType(TypePlaceSugget.StartLevel);

            if (Space_Showed_Sugget_Start_Level < 0)
                Space_Showed_Sugget_Start_Level = _config.space_level_show_sugget;
            Space_Showed_Sugget_Start_Level++;
            if (Space_Showed_Sugget_Start_Level > _config.space_level_show_sugget)
                Space_Showed_Sugget_Start_Level = 0;
            bool isHasSugget = _config.is_active
                && UseProfile.CurrentLevel >= _config.level_unlock
                && Space_Showed_Sugget_Start_Level <= 0
                && NumSuggetStartLevelInDay <= _config.num_sugget_in_day;
            if (isHasSugget)
            {
                var listSuggetActive = _config.suggets;
                for (int i = 0; i < suggetsStartLevel.Count; i++)
                {
                    bool isHas = false;
                    for (int z = 0; z < listSuggetActive.Count; z++)
                    {
                        if (listSuggetActive[z] == suggetsStartLevel[i].id)
                        {
                            isHas = true;
                            break;
                        }
                    }
                    if (isHas)
                    {
                        if (suggetsStartLevel[i].actionCheckCondition())
                        {
                            GamePlayController_BirdSort.Instance.state = StateGame.Pause;
                            suggetsStartLevel[i].actionShow?.Invoke();
                            BirdSort2.SuggetBuyerInGame.NumSuggetStartLevelInDay++;
                            return;
                        }
                    }
                }
            }
        }
        #endregion

        #region End Level
        public void InitSuggetEndLevel()
        {
            suggetsEndLevel = new List<SuggetBuyerModel>();
            suggetsEndLevel.Add(GetSuggetVideoGirl());
            suggetsEndLevel.Add(GetSuggetBigRemoveAds());
        }
        int reward = 0;
        public bool OnEndLevel()
        {
            var _config = config.GetConfigType(TypePlaceSugget.EndLevel);

            if (Space_Showed_Sugget_End_Level < 0)
                Space_Showed_Sugget_End_Level = _config.space_level_show_sugget;
            Space_Showed_Sugget_End_Level++;
            if (Space_Showed_Sugget_End_Level > _config.space_level_show_sugget)
                Space_Showed_Sugget_End_Level = 0;
            bool isHasSugget = _config.is_active
                && UseProfile.CurrentLevel > _config.level_unlock
                && Space_Showed_Sugget_End_Level <= 0
                && NumSuggetEndLevelInDay <= _config.num_sugget_in_day;
            if (isHasSugget)
            {
                var listSuggetActive = _config.suggets;
                for (int i = 0; i < suggetsEndLevel.Count; i++)
                {
                    bool isHas = false;
                    for (int z = 0; z < listSuggetActive.Count; z++)
                    {
                        if (listSuggetActive[z] == suggetsEndLevel[i].id)
                        {
                            isHas = true;
                            break;
                        }
                    }

                    if (isHas)
                    {
                        if (suggetsEndLevel[i].actionCheckCondition())
                        {
                            suggetsEndLevel[i].actionShow?.Invoke();
                            BirdSort2.SuggetBuyerInGame.NumSuggetEndLevelInDay++;
                            return true;
                        }
                    }
                }
            }

            return false;

            //// LongTH - Add codition to check
            //suggetsEndLevel[0].actionShow?.Invoke();
            //return true;
        }
        #endregion

        #region Sugget Buyer Model
        private SuggetBuyerModel GetSuggetVideoGirl()
        {
            var packVideoGirl = GameController.Instance.videoPackController.GetPack(TypePackVideo.VideoGirl);

            SuggetBuyerModel result = new SuggetBuyerModel();
            result.id = 1;
            result.actionCheckCondition = () => packVideoGirl.IsCanShowSugget();
            result.actionShow = () => 
            {
                packVideoGirl.ShowPopup(true);
                packVideoGirl.packModel.SetTimeShowHomeNow();
            };

            return result;
        }

        private SuggetBuyerModel GetSuggetBuyItem()
        {
            SuggetBuyerModel result = new SuggetBuyerModel();
            result.id = 0;
            result.actionCheckCondition = () => GamePlayController_BirdSort.Instance.suggetBuyItemController.IsCanSugget();
            result.actionShow = () => GamePlayController_BirdSort.Instance.suggetBuyItemController.OnSuggetBuyItem();

            return result;
        }

        private SuggetBuyerModel GetSuggetBigRemoveAds()
        {
            var pack = GameController.Instance.packController.GetPack(TypePackIAP.BigRemoveAdsPack);

            SuggetBuyerModel result = new SuggetBuyerModel();
            result.id = 2;
            result.actionCheckCondition = () => pack.IsCanShowSugget();
            result.actionShow = () => 
            { 
                pack.ShowPopup(true);
                pack.packModel.SetTimeShowHomeNow();
            };

            return result;
        }
        #endregion

        private void OnDestroy()
        {
            if (actionClosePopup != null)
            {
                EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CLOSE_POPUP, actionClosePopup);
                actionClosePopup = null;
            }
        }    
    }
}
