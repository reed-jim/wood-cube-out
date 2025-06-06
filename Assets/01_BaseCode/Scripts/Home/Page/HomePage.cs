using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class HomePage : PageBase
{
    [SerializeField] private Button playBtn;
    [SerializeField] protected GameObject[] hardModeObj;
    [SerializeField] protected Sprite[] hardModeSprt;
    [SerializeField] private Text levelTxt;
    [SerializeField] protected Text playTxt;
    [SerializeField] protected Color[] outLineColor;

    private LevelConfig levelConfig;

    public SaleHomeController saleHomeController;
    public StartChestController startChestController;
    public LevelChestController levelChestController;
    public GameObject handTutPlayLevel;

    public override void Init(PageController controller)
    {
        GameController.Instance.challengeController.InitHome();
        saleHomeController.Init();
        startChestController.Init();
        levelChestController.Init();


        base.Init(controller);

        playBtn.onClick.AddListener(OnClickPlay);
        //LongTH
        string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
        AsyncOperationHandle<GameObject> handle =
            Addressables.LoadAssetAsync<GameObject>($"Level {UseProfile.CurrentLevel}");

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject level = op.Result;
                levelTxt.text = GameController.Instance.useProfile.CurrentLevelPlay.ToString();
            }
            else
            {
                levelTxt.text = Localization.Get("s_comming_soon");
                for (int i = 0; i < hardModeObj.Length; i++)
                {
                    hardModeObj[i].gameObject.SetActive(false);
                }

                playTxt.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 7);
                playBtn.GetComponent<Image>().sprite = hardModeSprt[0];
                playTxt.transform.GetComponent<UnityEngine.UI.Outline>().effectColor = outLineColor[0];
            }


        };
    }

    public override void InitState()
    {
        GameController.Instance.challengeController.InitStateHome();
        saleHomeController.InitSate();
        startChestController.InitState();
        levelChestController.InitState();
        if (UseProfile.CurrentLevel >= RemoteConfigController.GetIntConfig(FirebaseConfig.START_LEVEL_BACK_TO_HOME, 5) + 1)
        {
            if(!UseProfile.IsClickTutPlayHome)
            {
                handTutPlayLevel.gameObject.SetActive(true);
            }    
            else
            {
                handTutPlayLevel.gameObject.SetActive(false);
            }    
        }    
        else
        {
            handTutPlayLevel.gameObject.SetActive(false);
        }    
    }


    private void OnClickPlay()
    {
        AsyncOperationHandle<GameObject> handle =
            Addressables.LoadAssetAsync<GameObject>($"Level {UseProfile.CurrentLevel}");

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            { 
                UseProfile.IsClickTutPlayHome = true;
                GameController.Instance.LoadScene(SceneName.GAME_PLAY);
            }
            else
            {
                GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                (
                    playBtn.transform.position,
                    Localization.Get("s_comming_soon"),
                    Color.white,
                    isSpawnItemPlayer: true
                );
            }

            GameController.Instance.musicManager.PlayClickSound();
        };
    }    
}
