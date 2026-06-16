using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayCampainBox : BaseBox
{
    public static PlayCampainBox instance;
    public static PlayCampainBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<PlayCampainBox>(PathPrefabs.PLAY_CAMPAIN_BOX));
            instance.Init();
        }
        //ChickenDataManager.CountTillShowRate = 0;
        instance.gameObject.SetActive(true);
        instance.InitState();
        return instance;
    }

    [SerializeField] protected Button playBtn;
    [SerializeField] protected Text levelTxt;
    [SerializeField] protected GameObject[] hardModeObj;

    [SerializeField] protected SellectItemUI[] itemsUI;
    protected AdsSegmentData adsData;
    public TutorialFunController tutUseItemController;

    [SerializeField] protected ItemX2WeeklyUI itemX2WeeklyUI;

    public virtual void Init()
    {
        playBtn.onClick.AddListener(OnClickPlay);
        for (int i = 0; i < itemsUI.Length; i++)
        {
            itemsUI[i].Init(itemsUI[i].typeItem);
        }

        adsData = GameController.Instance.segmentController.GetCurrentSegment().adsData;

        if (tutUseItemController != null)
        {
            tutUseItemController.Init();
        }
    }    

    public virtual void InitState()
    {
        for (int i = 0; i < itemsUI.Length; i++)
        {
            itemsUI[i].InitState();
        }

        levelTxt.text = string.Format(Localization.Get("s_level"), GameController.Instance.useProfile.CurrentLevelPlay);
        itemX2WeeklyUI.InitState();
    }  

    public void Show(int idHardMode)
    {
        if(idHardMode > 0)
        {
            hardModeObj[idHardMode - 1].gameObject.SetActive(true);
        }
        else
        {
            for (int i = 0; i < hardModeObj.Length; i++)
            {
                hardModeObj[i].gameObject.SetActive(false);
            }
        }    

        Show();

        if (tutUseItemController != null)
        {
            tutUseItemController.StartTut();
        }
    }    
    
    public virtual void OnClickPlay()
    {
        bool isCanPlay = false;
        if(UseProfile.IsUnlimitedHealth)
        {
            isCanPlay = true;
        }    
        else
        {
            if(UseProfile.Health >= 1)
            {
                isCanPlay = true;
                //UseProfile.Health -= 1;
                if (!UseProfile.IsRefillHealth)
                {
                    UseProfile.TimeLastOverHealth = UnbiasedTime.Instance.Now();
                    UseProfile.IsRefillHealth = true;
                }
            }    
        }   
        
        if(isCanPlay)
        {
            Close();
            GameController.Instance.LoadScene(SceneName.GAME_PLAY);
        }    
        else
        {
            //Hiện popup mua Health
            OutOfHealthBox.Setup().Show();
            GameController.Instance.musicManager.PlayClickSound();
        }    

        
    }    
}
