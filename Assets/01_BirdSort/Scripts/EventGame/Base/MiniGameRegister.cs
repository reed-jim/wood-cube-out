using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniGameData : ScriptableObject
{
    public List<RewardMiniGame> rewards;
}

[System.Serializable]
public class RewardMiniGame
{
    public int requireScore;
    public GiftType typeReward;
    public int valueReward;
}


public abstract class MiniGameRegister : MonoBehaviour
{
    public abstract int Version { get; set; }
    public abstract MiniGameConfig GameConfig { get; }
    public abstract void ResetData();
    public abstract void OpenBox();

    public abstract void ResetDay();

    public int ID;

    [Header("UI")]
    [SerializeField] protected Text coolDownTxt;
    [SerializeField] protected Button openBtn;
    [SerializeField] protected GameObject notiObj;



    public virtual void Init()
    {
        CheckEventActive();
        ReloadConfig();
        ///Viết ResetDay vào đây
        ///
    }

    protected virtual void ReloadConfig()
    {

    }

    public virtual void CheckEventActive()
    {
        var config = GameConfig;
        bool isActive = config.IsActive;
        if (isActive && HomeController.Instance.IsShowChristmasEvent())
        {
            if (Version < config.version)
            {
                ResetData();
                Version = config.version;
            }
            if (openBtn != null)
            {
                openBtn.gameObject.SetActive(true);
                openBtn.onClick.RemoveAllListeners();
                openBtn.onClick.AddListener(OnClickOpenButton);
            }
            else
            {
                openBtn.gameObject.SetActive(false);
            }
            //   CountDownEvent();
            CountDownEventByDay(config);
        }
        else
        {
            if (openBtn != null)
            {
                openBtn.gameObject.SetActive(false);
            }
        }
    }

    private void CountDownEvent()
    {
        if (coolDownTxt == null)
            return;

        coolDownTxt.text = string.Empty;
        var config = GameConfig;
        bool isActive = config.IsActive;

        if (isActive)
        {
            DateTime timeEndParse = config.end_date;
            var countDownObservable = Observable.Interval(TimeSpan.FromSeconds(1f))
                                                .Select(newDate => timeEndParse - UnbiasedTime.Instance.Now());

            countDownObservable.Where(newDate => !(newDate.Hours < 0 || newDate.Minutes < 0 || newDate.Seconds < 0))
                               .Subscribe(newDate =>
                               {
                                   if (coolDownTxt != null)
                                   {
                                       coolDownTxt.text = newDate.Days > 0
                                       ? string.Format(Localization.Get("lb_wb_ends_in_days"), newDate.Days)
                                       : string.Format(Localization.Get("lb_wb_ends_in"),
                                           $"{newDate.Hours:D2}:{newDate.Minutes:D2}:{newDate.Seconds:D2}");
                                   }
                               }).AddTo(this);

            countDownObservable.Where(newDate => newDate.Hours < 0 || newDate.Minutes < 0 || newDate.Seconds < 0)
                               .Subscribe(newDate =>
                               {
                                   if (coolDownTxt != null)
                                       coolDownTxt.text = "00:00:00";
                               }).AddTo(this);
        }
    }
    private void CountDownEventByDay(MiniGameConfig config)
    {
        var day = UnbiasedTime.Instance.Now().DayOfWeek;
        if (config.IsActive && UseProfile.CurrentLevel >= config.min_level && HomeController.Instance.IsShowChristmasEvent())
        {
            if (openBtn != null)
            {
                openBtn.gameObject.SetActive(true);
            }
            var tDay = UnbiasedTime.Instance.Now();
            CountDownTime(config.end_date);
            //if (day == DayOfWeek.Sunday)
            //{
            //    var date = TimeManager.ParseTimeStartDay(UnbiasedTime.Instance.Now().AddDays(1));
            //    CountDownTime(date);
            //}
            //else
            //{
            //    var nextSunday = new DateTime();
            //    nextSunday = tDay.AddDays(8 - (int)tDay.DayOfWeek);
            //    CountDownTime(nextSunday);
            //}
        }
        else
        {
            if (openBtn != null)
            {
                openBtn.gameObject.SetActive(false);
            }
        }
    }




    private void CountDownTime(DateTime param)
    {
        Debug.Log("EndTime " + param);
        var countDownObservable = Observable.Interval(TimeSpan.FromSeconds(1f))
                                    .Select(newDate => param - UnbiasedTime.Instance.Now());

        countDownObservable.Where(newDate => !(newDate.Hours < 0 || newDate.Minutes < 0 || newDate.Seconds < 0))
                           .Subscribe(newDate =>
                           {
                               //Debug.LogError(newDate);
                               if (coolDownTxt != null)
                               {
                                   coolDownTxt.text = newDate.Days > 0
                                   ? string.Format("End in: {0} Days", newDate.Days)
                                   : string.Format("{0}",
                                       $"{newDate.Hours:D2}:{newDate.Minutes:D2}:{newDate.Seconds:D2}");
                               }
                           }).AddTo(this);

        countDownObservable.Where(newDate => newDate.Hours < 0 || newDate.Minutes < 0 || newDate.Seconds < 0)
                           .Subscribe(newDate =>
                           {
                               //Debug.LogError(newDate);
                               if (coolDownTxt != null)
                               {
                                   coolDownTxt.text = "00:00:00";
                                   coolDownTxt.gameObject.SetActive(false);
                                   if(!HomeController.Instance.IsShowChristmasEvent()) openBtn.gameObject.SetActive(false);
                               }
                           }).AddTo(this);

    }
    private void CountDownTime7(DateTime param)
    {
        var countDownObservable = Observable.Interval(TimeSpan.FromSeconds(1f))
                                    .Select(newDate => param - UnbiasedTime.Instance.Now());

        countDownObservable.Where(newDate => !(newDate.Hours < 0 || newDate.Minutes < 0 || newDate.Seconds < 0))
                           .Subscribe(newDate =>
                           {
                               if (coolDownTxt != null)
                               {
                                   coolDownTxt.text = newDate.Days > 0
                                   ? string.Format("End in: {0} day", newDate.Days)
                                   : string.Format("End in: {0}",
                                       $"{newDate.Hours:D2}:{newDate.Minutes:D2}:{newDate.Seconds:D2}");
                               }
                           }).AddTo(this);

        countDownObservable.Where(newDate => newDate.Hours < 0 || newDate.Minutes < 0 || newDate.Seconds < 0)
                           .Subscribe(newDate =>
                           {
                               if (coolDownTxt != null)
                                   coolDownTxt.text = "00:00:00";
                           }).AddTo(this);
    }


    private void OnClickOpenButton()
    {
        GameController.Instance.musicManager.PlayClickSound();
        //if (ValidateUser())
        //    OpenBox();
    }

    private bool ValidateUser()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            //Context.Confirm.AddMessageOK(Localization.Get("tit_error"), Localization.Get("lb_check_internet") + ChickenErrorCodes.GetErrorCode(FunctionCode.OnlineGameLobbyController_ButtonEvent_ConnecRoom, CantConnectCode.NoInternet));
            return false;
        }

        return true;
    }

}