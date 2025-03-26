using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System;
//using MoreMountains.NiceVibrations;
using UnityEngine.Events;

public class GameScene : BaseScene
{
    [Header("UI")]
    [SerializeField] private Text levelTxt;
    [SerializeField] private Button pauseBtn;
    public Text timePlayLevelTxt;
    [SerializeField] private Color[] colorWaringTime1;
    [SerializeField] private float lefpTimeColor1;
    private int colorIndexWarningTime1;
    [SerializeField] private Color[] colorWaringTime2;
    [SerializeField] private float lefpTimeColor2;
    private int colorIndexWarningTime2;
    [SerializeField] private Color[] colorWaringTime3;
    [SerializeField] private float lefpTimeColor3;
    private int colorIndexWarningTime3;
    [SerializeField] private Color[] colorIce;
    [SerializeField] private float lefpTimeColorIce;
    private int colorIndexIce;
    private float timmerColor;

    [Header("Requirement")]
    [SerializeField] private RequirementUI requirementUIPrefab;
    [HideInInspector] public Dictionary<int, RequirementUI> requirementsUI;
    [SerializeField] private Transform requirementUIParent;
    [SerializeField] private Transform[] lstPosRequirements;
    private int numRequirement;

    [Header("Item")]
    [SerializeField] private ItemUI itemUIPrefab;
    public Dictionary<GiftType, ItemUI> itemsUI;
    [SerializeField] private Transform parentItem;
    public HorizontalLayoutGroup layoutGruopItem;

    private Action<object> actionOpenShop;

    public void Init()
    {
        // levelTxt.text = string.Format(Localization.Get("s_level"), GameController.Instance.useProfile.CurrentLevelPlay.ToString());
        levelTxt.text = string.Format(Localization.Get("s_level"),  GameController.Instance.useProfile.CurrentLevelPlay.ToString());
        timePlayLevelTxt.text = "";

        requirementsUI = new Dictionary<int, RequirementUI>();

        pauseBtn.onClick.AddListener(OnClickPause);

        actionOpenShop = (sender) => { OpenShop(); };
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.OPEN_SHOP, actionOpenShop);

        itemsUI = new Dictionary<GiftType, ItemUI>();

        ResetState();
    }

    public void ResetState()
    {
        numRequirement = 0;
        requirementsUI.Clear();
    }    

    public void OnClickPause()
    {
        if (GamePlayController.Instance.state != StateGame.Playing)
            return;

        // PasueBox.Setup().Show();
        GameController.Instance.musicManager.PlayClickSound();
    }

    public void OpenShop()
    {
        ShopBox.Setup();
    }

    #region Requirement
    public void SpawnRequirement(int idBird, Sprite iconBird, int numRequire)
    {
        var require = Instantiate(requirementUIPrefab, requirementUIParent);
        require.Init(idBird, iconBird, numRequire);
        require.transform.position =lstPosRequirements[numRequirement].position + new Vector3(0, 1000, 0);
      //  require.transform.localPosition = new Vector3(this.transform.localPosition.x, 250, this.transform.localPosition.z);
       

        numRequirement++;


        requirementsUI.Add(idBird, require);
    }

    public IEnumerator ShowRequirement()
    {
        int _index = 0;
        yield return new WaitUntil(()=> GamePlayController.Instance.state == StateGame.Playing);
        foreach (var item in requirementsUI)
        {
            yield return new WaitForSeconds(0.3f);
            item.Value.StartAnim(lstPosRequirements[_index].position);
            _index++;
           
        }
    }    

    public RequirementUI GetRequirementUI(int idBird)
    {
        if (requirementsUI.ContainsKey(idBird))
            return requirementsUI[idBird];

        return null;
    }

    public void ReSortRequirement()
    {
        List<RequirementUI> requirements = new List<RequirementUI>();

        foreach (var item in requirementsUI)
        {
            if (item.Value.isComplete)
                continue;

            requirements.Add(item.Value);
        }

        for (int i = 0; i < requirements.Count; i++)
        {
            int index = i;
            requirements[index].ResortRequirement(lstPosRequirements[index].position);
        }
    }    
    #endregion

    #region Item
    public void SpawnItemsUI(List<ItemBase> lstItem)
    {
        for (int i = 0; i < lstItem.Count; i++)
        {
            int index = i;
            var itemUI = Instantiate(itemUIPrefab, parentItem);
            itemUI.Init(lstItem[index].typeItem);

            itemsUI.Add(lstItem[index].typeItem, itemUI);
        }
    }
    #endregion
    public void ShowTimePlayGame(float timeLeft)
    {
        timePlayLevelTxt.text = TimeManager.ShowTime2((long)timeLeft);
    }

    public void ActiveWarningTime()
    {
        timePlayLevelTxt.DOKill();
        timePlayLevelTxt.DOColor(new Color(1, 0, 19f / 255, 1), 0.3f).SetLoops(-1, LoopType.Yoyo);
    }

    public override void OnEscapeWhenStackBoxEmpty()
    {

    }

    private void OnDestroy()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.OPEN_SHOP, actionOpenShop);
    }

    public void ChangeColorTimeLevelTxt(float remmingTime, bool isIce)
    {
        if (!isIce)
        {
            if (remmingTime <= 31 && remmingTime > 19)
            {
                timePlayLevelTxt.color = Color.Lerp(timePlayLevelTxt.color, colorWaringTime1[colorIndexWarningTime1], lefpTimeColor1 * Time.deltaTime);

                timmerColor = Mathf.Lerp(timmerColor, 1, lefpTimeColor1 * Time.deltaTime);
                if (timmerColor > 0.9f)
                {
                    timmerColor = 0;
                    colorIndexWarningTime1++;
                    colorIndexWarningTime1 = (colorIndexWarningTime1 >= colorWaringTime1.Length) ? 0 : colorIndexWarningTime1;
                }
            }
            else if (remmingTime <= 19 && remmingTime > 10)
            {
                timePlayLevelTxt.color = Color.Lerp(timePlayLevelTxt.color, colorWaringTime2[colorIndexWarningTime2], lefpTimeColor2 * Time.deltaTime);

                timmerColor = Mathf.Lerp(timmerColor, 1, lefpTimeColor2 * Time.deltaTime);
                if (timmerColor > 0.9f)
                {
                    timmerColor = 0;
                    colorIndexWarningTime2++;
                    colorIndexWarningTime2 = (colorIndexWarningTime2 >= colorWaringTime2.Length) ? 0 : colorIndexWarningTime2;
                }
            }
            else if (remmingTime <= 10)
            {
                timePlayLevelTxt.color = Color.Lerp(timePlayLevelTxt.color, colorWaringTime3[colorIndexWarningTime3], lefpTimeColor3 * Time.deltaTime);

                timmerColor = Mathf.Lerp(timmerColor, 1, lefpTimeColor3 * Time.deltaTime);
                if (timmerColor > 0.9f)
                {
                    timmerColor = 0;
                    colorIndexWarningTime3++;
                    colorIndexWarningTime3 = (colorIndexWarningTime3 >= colorWaringTime3.Length) ? 0 : colorIndexWarningTime3;
                }
            }
            else
            {
                timePlayLevelTxt.color = Color.white;
            }    
        }
        else
        {
            timePlayLevelTxt.color = Color.Lerp(timePlayLevelTxt.color, colorIce[colorIndexIce], lefpTimeColorIce * Time.deltaTime);

            timmerColor = Mathf.Lerp(timmerColor, 1, lefpTimeColorIce * Time.deltaTime);
            if (timmerColor > 0.9f)
            {
                timmerColor = 0;
                colorIndexIce ++;
                colorIndexIce = (colorIndexIce >= colorIce.Length) ? 0 : colorIndexIce;
            }
        }    
    }
    public void UpdateNumAddBranch()
    {
        //if (GameController.Instance.useProfile.CurrentNumAddBranch > 0)
        {
            //iconVideoBuyStand.gameObject.SetActive(false);
            //numAddBranchObj.gameObject.SetActive(true);
            //numAddBranchVideoTxt.text = GameController.Instance.useProfile.CurrentNumAddBranch.ToString();
        }
        //else
        {
            //iconVideoBuyStand.gameObject.SetActive(true);
            //numAddBranchObj.gameObject.SetActive(false);
        }
    }
}
