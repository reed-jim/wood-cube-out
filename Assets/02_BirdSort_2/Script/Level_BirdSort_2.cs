
namespace BirdSort2
{
    using DG.Tweening;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;
    using System;

    [System.Serializable]
    public class LevelConfig
    {
        [Header("========= Level Id ============")]
        public int id;
        [Header("========= HardMode ============")]
        public int idHardMode;
        [Header("========= Stands Config ============")]
        public List<StandConfig> standConfig;

        [Header("========= Extrals Config ============")]
        public List<ExtralPlayData> extralsConfig;
        [Header("========= Time Config ============")]
        public int min;
        public int second;
        public int TotalTime
        {
            get
            {
                return (min * 60) + second;
            }
        }

        public bool Equals2(LevelConfig level)
        {
            if (standConfig.Count != level.standConfig.Count)
                return false;

            if (extralsConfig == null && level.extralsConfig != null)
                return false;

            if (extralsConfig != null && level.extralsConfig == null)
                return false;

            if (extralsConfig.Count != level.extralsConfig.Count)
                return false;

            for (int i = 0; i < standConfig.Count; i++)
            {
                if (standConfig[i].idBirds == null && level.standConfig[i].idBirds != null)
                    return false;
                if (standConfig[i].idBirds != null && level.standConfig[i].idBirds == null)
                    return false;

                if (standConfig[i].idBirds.Count != level.standConfig[i].idBirds.Count)
                    return false;

                for (int t = 0; t < standConfig[i].idBirds.Count; t++)
                {
                    if (standConfig[i].idBirds[t] != level.standConfig[i].idBirds[t])
                    {
                        return false;
                    }
                }
            }

            for (int i = 0; i < extralsConfig.Count; i++)
            {
                if (extralsConfig[i].type != level.extralsConfig[i].type)
                    return false;

                if (extralsConfig[i].positionData.indexBird != level.extralsConfig[i].positionData.indexBird)
                    return false;
                if (extralsConfig[i].positionData.indexStand != level.extralsConfig[i].positionData.indexStand)
                    return false;

                if (extralsConfig[i].extralValues == null && level.extralsConfig[i].extralValues != null)
                    return false;
                if (extralsConfig[i].extralValues != null && level.extralsConfig[i].extralValues == null)
                    return false;

                for (int t = 0; t < extralsConfig[i].extralValues.Length; t++)
                {
                    if (extralsConfig[i].extralValues[t] != level.extralsConfig[i].extralValues[t])
                        return false;
                }
            }

            return true;
        }
    }

    [System.Serializable]
    public class StandConfig
    {
        public List<int> idBirds = new List<int>();
        public StandData standData;
        public int side;
    }

    public class Level_BirdSort_2 : MonoBehaviour
    {
        public LevelConfig levelConfig;

        [Header("Stands Data")] public List<StandingBase> standsPrefab;

        private StandingBase m_standChosing;

        public StandingBase standChosing
        {
            get { return m_standChosing; }
            set
            {
                m_standChosing = value;
                //if (m_standChosing == null)
                //    EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.DISABLE_OUTLINE_BIRD);
            }
        }
        //[HideInInspector] public float timePlayLevel;
        //[HideInInspector] public float timePlayRequire;
        [HideInInspector] public int starWin;
        public List<StandingBase> standsInLevel;
        public List<StandingBase> standsInLevel_Real;

        public Transform leftStandParent;
        public Transform rightStandParent;
        public Transform leftStandParent_Real;
        public Transform rightStandParent_Real;

        public TypeStand typeStandInLevel;

        [Header("Bird Data")] public List<BirdBase> birdsPrefab;
        public BirdBase GetBirdWithId(int param)
        {
            foreach(var item in birdsPrefab)
            {
                if(item.id == param)
                {
                    return item;
                }
            }
            return null;
        }
        public Transform birdParent;
        private int numBirdCount;
        private int numBirdInLevel;
        [HideInInspector] public bool isBirdFlying;

        [Header("Stand Parent")] public StandParent StandParentLeft;
        public StandParent StandParentRight;
        [HideInInspector] public int sideAddStand;
        public int numStandsInited;

        [HideInInspector] public bool isHasExtral;
        public Transform extralShowParent;
        public Transform parentPanelGamePlay;

        public bool isInited;
        public float timePlayLevel;
        [HideInInspector] public float timePlayRequire;
        public AudioSource dropAudioLeft;
        public AudioSource dropAudioRight;
        public AudioClip audioClipDrop;

        public StandingBase GetStandingBaseReal(int id)
        {

            foreach (var item in standsInLevel_Real)
            {
                if (item.ID == id)
                {
                    return item;
                }
            }
            return null;

        }

        public List<StandingBase> LsStandingBasesLeft;
        public List<StandingBase> LsStandingBasesRight;
        private bool checkLsStandLeft;
        private bool checkLsStandRight;
        [HideInInspector] public bool isTempPauseLevel;
        public bool ListStandLeftDone
        {
            get
            {
                checkLsStandLeft = true;
                foreach (var item in LsStandingBasesLeft)
                {
                    if (item.icon.gameObject.activeSelf)
                    {
                        checkLsStandLeft = false;
                    }
                }
                return  checkLsStandLeft;
            }
        }
        public bool ListStandRightDone
        {
            get
            {
                checkLsStandRight = true;
                foreach (var item in LsStandingBasesRight)
                {
                    if (item.icon.gameObject.activeSelf)
                    {
                        checkLsStandRight = false;
                    }
                }
                return  checkLsStandRight;
            }
        }

        public List<RewardRandom> GetItemYouMiss()
        {
            List<RewardRandom> rewardMiss = new List<RewardRandom>();
            rewardMiss.Add(new RewardRandom() { typeItem = GiftType.StarCampain, amount = -1 });

            //Challenge
            var scoreTtemsChallegen = BirdSort2.GamePlayController_BirdSort.Instance.challengeGameplayController.PredictItems;
            foreach (var it in scoreTtemsChallegen)
            {
                if (it.Value > 0)
                {
                    rewardMiss.Add(new RewardRandom() { typeItem = GameController.Instance.challengeController.GetGiftTypeItemChallenge(it.Key), amount = it.Value });
                }
            }


            return rewardMiss;
        }

        private IEnumerator waringTimePlayCorutine;
        private IEnumerator reviveCorutine;
        private bool isLose;
        public void Init()
        {
            GamePlayController_BirdSort.Instance.levelInd = UseProfile.CurrentLevelPlay;
            LsStandingBasesLeft = new List<StandingBase>();
            LsStandingBasesRight = new List<StandingBase>();
            
            if (UseProfile.CurrentLevel > Context.MAX_LEVEL)
                return;

            //  GameController.Instance.useProfile.CurrentLevelPlay = 10;


            standsInLevel = new List<StandingBase>();
            standsInLevel_Real = new List<StandingBase>();
            numBirdCount = 0;
            isBirdFlying = true;
           string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
           // if(RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_TEST_LEVEL, true))
              //  pathLevel = StringHelper.PATH_CONFIG_LEVEL_TEST;

            TextAsset lvJson = Resources.Load<TextAsset>(string.Format(pathLevel, UseProfile.CurrentLevelPlay));
          
            //pathLevel = "LevelsAbc/Level_1";
            //TextAsset lvJson = Resources.Load<TextAsset>(pathLevel);
            if (lvJson != null)
            {
                GamePlayController_BirdSort.Instance.hintController.Init(UseProfile.CurrentLevelPlay);
                levelConfig = JsonUtility.FromJson<LevelConfig>(lvJson.ToString());
                if (levelConfig != null)
                {
               
                    int numStandLef = 0;
                    int numStandRight = 0;
                    List<Vector3> posTutStand = new List<Vector3>();
                    for (int i = 0; i < levelConfig.standConfig.Count; i++)
                    {
                        int index = i;
                        numBirdInLevel += levelConfig.standConfig[index].idBirds.Count;
                        if (levelConfig.standConfig[index].side > 0)
                        {
                            numStandRight++;
                        }
                        else
                        {
                            numStandLef++;
                        }

                        typeStandInLevel = levelConfig.standConfig[index].standData.type;
                    }


                    if (numStandLef > numStandRight)
                    {
                        //Lấy của bên nhiều hơn
                        if (numStandRight + 1 < numStandLef) //Tính cả xem video thêm cành vẫn bé hơn 
                        {
                            StandParentLeft.ScaleWithNumStand(numStandLef);
                            StandParentRight.ScaleWithNumStand(numStandLef);
                            sideAddStand = 1;
                        }
                        else
                        {
                            numStandRight += 1; //Tính cả xem video thêm cành
                            StandParentLeft.ScaleWithNumStand(numStandRight);
                            StandParentRight.ScaleWithNumStand(numStandRight);
                            sideAddStand = 1;
                        }
                    }
                    else
                    {
                        //Lấy của bên nhiều hơn
                        if (numStandLef + 1 < numStandRight) //Tính cả xem video thêm cành vẫn bé hơn 
                        {
                            StandParentLeft.ScaleWithNumStand(numStandRight);
                            StandParentRight.ScaleWithNumStand(numStandRight);
                            sideAddStand = -1;
                        }
                        else
                        {
                            numStandLef += 1;
                            StandParentLeft.ScaleWithNumStand(numStandLef);
                            StandParentRight.ScaleWithNumStand(numStandLef);
                            sideAddStand = -1;
                        }
                    }

                    for (int i = 0; i < levelConfig.standConfig.Count; i++)
                    {
                        int index = i;
                        typeStandInLevel = levelConfig.standConfig[index].standData.type;
                        int idStand = (int)levelConfig.standConfig[index].standData.type;
                        int sidePosStand = levelConfig.standConfig[index].side;

                        //  Debug.LogError("levelConfig");

                        StandingBase stand = Instantiate(
                            standsPrefab[Mathf.Clamp(idStand, 0, standsPrefab.Count - 1)],
                            sidePosStand == 1 ? rightStandParent : leftStandParent);

                        if (stand != null)
                        {
                            stand.gameObject.name = (index + 1).ToString();
                            stand.icon.color = new Color32(255, 255, 255, 0);
                            stand.btnCick.gameObject.SetActive(false);
                            stand.transform.localScale = new Vector2(1.05f, 1.05f);
                            // StartCoroutine(stand.Init(levelConfig.standConfig[index].idBirds, index));
                        }

                        stand.parent = sidePosStand == 1 ? rightStandParent : leftStandParent;
                        standsInLevel.Add(stand);
                    }


                    for (int i = 0; i < levelConfig.standConfig.Count; i++)
                    {
                        int index = i;
                        typeStandInLevel = levelConfig.standConfig[index].standData.type;
                        int idStand = (int)levelConfig.standConfig[index].standData.type;
                        int sidePosStand = levelConfig.standConfig[index].side;

                        //  Debug.LogError("levelConfig");

                        StandingBase stand = Instantiate(
                            standsPrefab[Mathf.Clamp(idStand, 0, standsPrefab.Count - 1)],
                            sidePosStand == 1 ? rightStandParent_Real : leftStandParent_Real);

                        if (stand != null)
                        {
                            stand.gameObject.name = (index + 1).ToString();
                            stand.side = sidePosStand;
                            StartCoroutine(stand.Init(levelConfig.standConfig[index].idBirds, index));
                            stand.parent = sidePosStand == 1 ? rightStandParent_Real : leftStandParent_Real;
                            if (sidePosStand < 0)
                            {
                                LsStandingBasesLeft.Add(stand);
                            }
                            else
                            {
                                LsStandingBasesRight.Add(stand);
                                
                            }
                        }

                        standsInLevel_Real.Add(stand);
                    }

                    StartCoroutine(OffGridLayout());
                    //for(int i = 0; i < standsInLevel_Real.Count; i ++)
                    // {
                    //     standsInLevel_Real[i].rectTrans.anchoredPosition = standsInLevel[i].rectTrans.anchoredPosition;
                    // }    

                    //GameController.Instance.AnalyticsController.LogStartLevel(UseProfile
                    //    .CurrentLevelPlay);
                    StartCoroutine(Helper.StartAction(() => { GamePlayController_BirdSort.Instance.tutorialController.Init(); },
                        1.2f));

                    isHasExtral = false;
                    if (levelConfig.extralsConfig != null)
                    {
                        if (levelConfig.extralsConfig.Count > 0)
                        {
                            isHasExtral = true;
                        }
                    }
                }
            }
            timePlayRequire = levelConfig.TotalTime;
            timePlayRequire *= RemoteConfigController.GetFloatConfig(FirebaseConfig.CONFERI_MUTIL_TIME_LEVEL, 0.5f);
            isInited = true;

            waringTimePlayCorutine = WaringTime();
            StartCoroutine(waringTimePlayCorutine);
        }
        private IEnumerator OffGridLayout()
        {
            yield return new WaitForSeconds(1);
            rightStandParent_Real.GetComponent<GridLayoutGroup>().enabled = false;
            leftStandParent_Real.GetComponent<GridLayoutGroup>().enabled = false;

            rightStandParent_Real.GetComponent<ContentSizeFitter>().enabled = false;
            leftStandParent_Real.GetComponent<ContentSizeFitter>().enabled = false;
        }
        private void OnGridLayout()
        {

            rightStandParent_Real.GetComponent<GridLayoutGroup>().enabled = true;
            leftStandParent_Real.GetComponent<GridLayoutGroup>().enabled = true;

            rightStandParent_Real.GetComponent<ContentSizeFitter>().enabled = true;
            leftStandParent_Real.GetComponent<ContentSizeFitter>().enabled = true;
        }


        public IEnumerator MoveBrand(float time = 0.5f)
        {
            yield return new WaitForSeconds(time);

            for (int i = 0; i < standsInLevel_Real.Count; i++)
            {
                var index = i;
                standsInLevel_Real[index].transform.DOMove(standsInLevel[index].gameObject.transform.position, 0.5f).SetEase(Ease.OutBounce).OnComplete(delegate { });
            }

        }
      

        public void CheckAllBirdReady()
        {
            numBirdCount++;
            if (numBirdCount >= numBirdInLevel)
            {
                GamePlayController_BirdSort.Instance.state = StateGame.Playing;
                isBirdFlying = false;
                GamePlayController_BirdSort.Instance.itemController.InteractButton(true);
                InitSellectItem();
                GamePlayController_BirdSort.Instance.suggetBuyerInGame.OnStartLevel();
            }
        }


        public void InitSellectItem()
        {
            //yield return new WaitForSeconds(1);
            GamePlayController_BirdSort.Instance.itemController.ActiveItemSellectLevel();
            GamePlayController_BirdSort.Instance.challengeGameplayController.SpawnItems();

            var gameplayController = GamePlayController_BirdSort.Instance;

            //gameplayController.extralPlayController.AppearExtral();

            gameplayController.tutItemInGameController.OnBirdReady();
            //gameplayController.tutItemsController.OnBirdReady();
            //gameplayController.tutExtralController.OnBirdReady();

            //gameplayController.tutCoreGameController.StartTut();
            gameplayController.tutItemInGameController.StartTut();

        }


        public bool IsWin()
        {
            //Luật check Win: Tất cả các cành đều phải không có chim
            for (int i = 0; i < standsInLevel_Real.Count; i++)
            {
                if (standsInLevel_Real[i].birdsOn.Count > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public void CheckPassLevel()
        {
            if (GamePlayController_BirdSort.Instance.state == StateGame.Win)
                return;


            if (IsWin())
            {
                Win();
            }
        }

        public  void Win()
        {
            GamePlayController_BirdSort.Instance.state = StateGame.Win;
            UseProfile.CurrentLevel += 1;
            if (UseProfile.CurrentLevel > Context.MAX_LEVEL)
                UseProfile.CurrentLevel = Context.MAX_LEVEL;
            UseProfile.CurrentLevelPlay = UseProfile.CurrentLevel;
            GamePlayController_BirdSort.Instance.challengeGameplayController.WinLevel();
            StartCoroutine(WinHandle());
            StartCoroutine(RemoveBranchEndGame());
            GameController.Instance.AnalyticsController.LogLevelComplet(UseProfile.CurrentLevel - 1, (int)timePlayLevel);
            GameController.Instance.musicManager.PauseCoolDownSoundSource();
        }

        private IEnumerator RemoveBranchEndGame()
        {
            foreach (var item in standsInLevel_Real)
            {
                if (!item.isBlock)
                {
                    item.RemoveBrandEndGame2();
                    yield return new WaitForSeconds(0.25f);
                }
            }
        }    

        public  IEnumerator WinHandle()
        {
            isBirdFlying = true;
            yield return new WaitForSeconds(0.75f);
            //Hiện popup win
            GameController.Instance.musicManager.PlayWinSound();
            float percentTimeRemming = ((timePlayRequire - timePlayLevel) / timePlayRequire);
            int star = 3;
            if (percentTimeRemming >= 0.7f)//Segment
            {
                star = 3;
            }
            else if (percentTimeRemming < 0.7f && percentTimeRemming >= 0.3f)
            {
                star = 2;
            }
            else
            {
                star = 1;
            }
            starWin = star;
            StartChestController.CurrentStar += star;
            LevelChestController.CurrentLevelChest++;
            
            Debug.LogError("win");
          
            //if (GamePlayController_BirdSort.Instance.state == StateGame.Win)
            {
                  WinBox.Setup().Show(star, (int)Mathf.Abs(timePlayRequire - timePlayLevel));
            }

           
            //    WinBox.Setup().Show();
            // Scene scene = SceneManager.GetActiveScene();
            // SceneManager.LoadScene(scene.name);
        }

        public void ShowWinTest()
        {
            float percentTimeRemming = ((timePlayRequire - timePlayLevel) / timePlayRequire) * 100;
            int star = 3;
            if (percentTimeRemming >= 60)//Segment
            {
                star = 3;
            }
            else if (percentTimeRemming < 60 && percentTimeRemming >= 30)
            {
                star = 2;
            }
            else
            {
                star = 1;
            }
            starWin = star;
            if (GamePlayController_BirdSort.Instance.state == StateGame.Playing)
            {
                WinBox.Setup().Show(star, (int)Mathf.Abs(timePlayRequire - timePlayLevel));
            }
        }    


        public  void AddStandHandle()
        {


            int index = standsInLevel_Real.Count - 1;

            int idStand = (int)levelConfig.standConfig[index].standData.type;
            int sidePosStand = sideAddStand;




            StandingBase stand = Instantiate(
               standsPrefab[Mathf.Clamp(idStand, 0, standsPrefab.Count - 1)],
               sidePosStand == 1 ? rightStandParent : leftStandParent);

            if (stand != null)
            {
                stand.gameObject.name = (index).ToString();
                StartCoroutine(stand.Init(new List<int>(), index));
                standsInLevel.Add(stand);
                stand.gameObject.transform.DOKill();
                stand.gameObject.transform.localScale = Vector3.zero;
                stand.icon.color = new Color32(255, 255, 255, 30);
                stand.btnCick.gameObject.SetActive(false);
                stand.transform.localScale = new Vector2(1.05f, 1.05f);

                //StandParentLeft.HandleZoom();
                //StandParentRight.HandleZoom();

                int side = -(int)levelConfig.standConfig[index].side;
                sideAddStand = -side;
                levelConfig.standConfig.Add(new StandConfig() { side = side, standData = stand.data });
            }


            //OnGridLayout();
            StandingBase stand_Read = Instantiate(
                standsPrefab[Mathf.Clamp(idStand, 0, standsPrefab.Count - 1)],
                sidePosStand == 1 ? rightStandParent_Real : leftStandParent_Real);

            if (stand_Read != null)
            {
                stand_Read.gameObject.name = (index).ToString();
                StartCoroutine(stand_Read.Init(new List<int>(), index));
                standsInLevel_Real.Add(stand_Read);
                stand_Read.gameObject.transform.DOKill();
                stand_Read.gameObject.transform.localScale = Vector3.zero;
                stand_Read.gameObject.transform.DOScale(1, 0.3f);

            }

            // StartCoroutine(OffGridLayout());

            StartCoroutine(MoveBrand(0));

            //StartCoroutine(SpawnBrand());


            //IEnumerator SpawnBrand()
            //{

            //    yield return new WaitForSeconds(0.6f);

            //}

        }

        public bool AddStandHandleBool()
        {
           if(standsInLevel_Real.Count >= 22)
            {
                return false;
            }    

            int index = standsInLevel_Real.Count - 1;

            int numStandLeft = 0;
            int numStandRight = 0;
            for (int i = 0; i < standsInLevel_Real.Count; i++)
            {
                if (standsInLevel_Real[i].side == 1)
                    numStandRight++;
                else
                    numStandLeft++;
            }

            int idStand = (int)levelConfig.standConfig[index].standData.type;
            int sidePosStand = 1;
            if (numStandRight > numStandLeft)
                sidePosStand = -1;
            StandingBase stand = Instantiate(
               standsPrefab[Mathf.Clamp(idStand, 0, standsPrefab.Count - 1)],
               sidePosStand == 1 ? rightStandParent : leftStandParent);

            if (stand != null)
            {
                stand.gameObject.name = (index).ToString();
                StartCoroutine(stand.Init(new List<int>(), index));
                standsInLevel.Add(stand);
                stand.gameObject.transform.DOKill();
                stand.gameObject.transform.localScale = Vector3.zero;
                stand.icon.color = new Color32(255, 255, 255, 0);
                stand.btnCick.gameObject.SetActive(false);
                stand.transform.localScale = new Vector2(1.05f, 1.05f);

                int standAfterBreak = (int)Math.Ceiling((decimal)((numStandsInited + 2) / 2));
                StandParentLeft.ScaleWithNumStand(standAfterBreak);
                StandParentRight.ScaleWithNumStand(standAfterBreak);

                int side = -(int)levelConfig.standConfig[index].side;
                sideAddStand = -side;
                levelConfig.standConfig.Add(new StandConfig() { side = side, standData = stand.data });
                stand.parent =  sidePosStand == 1 ? rightStandParent : leftStandParent;
            }

            //OnGridLayout();
            StandingBase stand_Read = Instantiate(
                standsPrefab[Mathf.Clamp(idStand, 0, standsPrefab.Count - 1)],
                sidePosStand == 1 ? rightStandParent_Real : leftStandParent_Real);
            if (stand_Read != null)
            {
             
                // stand_Read.gameObject.name = (index).ToString();
                StartCoroutine(stand_Read.Init(new List<int>(), index));
                standsInLevel_Real.Add(stand_Read);
                stand_Read.gameObject.transform.DOKill();
                stand_Read.gameObject.transform.localScale = Vector3.zero;
                stand_Read.gameObject.transform.DOScale(1, 0.3f);
                stand_Read.side = sidePosStand;
                if (sidePosStand < 0)
                {
                    LsStandingBasesLeft.Add(stand_Read);
                }
                else
                {
                    LsStandingBasesRight.Add(stand_Read);
                                
                }
                stand_Read.ID = standsInLevel_Real[standsInLevel_Real.Count - 1].ID + 1;
                stand_Read.parent =  sidePosStand == 1 ? rightStandParent_Real : leftStandParent_Real;
            }

         

            StartCoroutine(MoveBrand(0));


            UseProfile.IsTutedBuyStand = true;
            GamePlayController_BirdSort.Instance.tutorialController.OnClickHintBuyStandBtn();

            return true;
      

        }
        public void CheckAllStandInited()
        {
            numStandsInited++;
            if (numStandsInited >= standsInLevel_Real.Count)
            {
                if (levelConfig != null)
                {
                    GamePlayController_BirdSort.Instance.extralPlayController.SpawnExtrals(levelConfig.extralsConfig);
                    StartCoroutine(Helper.StartAction(
                        () => {
                            
                            GamePlayController_BirdSort.Instance.extralPlayController.AppearExtral();

                          
                        },
                        () => GamePlayController_BirdSort.Instance.state == StateGame.Playing));
                }
            }
        }

        private void Update()
        {
            if (isTempPauseLevel)
                return;
            if (GamePlayController_BirdSort.Instance.state != StateGame.Playing)
                return;


            if (IceItem_BirdSort.isUseIce)
            {
                GamePlayController_BirdSort.Instance.gameScene.ChangeColorTimeLevelTxt(Mathf.Abs(timePlayRequire - timePlayLevel + 1), true);
                return;
            }
            timePlayLevel += Time.deltaTime;
            float percentTimeRemming = ((timePlayRequire - timePlayLevel) / timePlayRequire);
            GamePlayController_BirdSort.Instance.gameScene.ShowCoolDownStar(percentTimeRemming);
        }


        public void PauseLevel(bool isPause)
        {
            if (isPause)
            {
                //if (GamePlayController.Instance.state != StateGame.Playing)
                //    return;

                GamePlayController_BirdSort.Instance.state = StateGame.Pause;
                GameController.Instance.musicManager.PauseCoolDownSoundSource();
            }
            else
            {
                //if (GamePlayController.Instance.state != StateGame.Pause)
                //    return;

                GamePlayController_BirdSort.Instance.state = StateGame.Playing;
                if (Mathf.Abs(timePlayRequire - timePlayLevel) <= 10)
                {
                    GameController.Instance.musicManager.PlayCoolDownSoundSource();
                }
            }
        }

        public IEnumerator WaringTime()
        {
            yield return new WaitUntil(() => Mathf.Abs(timePlayRequire - timePlayLevel) <= 10);
            //GameController.Instance.musicManager.PlayCoolDownSoundSource();
        }

        public void Revive(bool isAddTime, bool isResetSlot, bool isReviveLoseExtral = false, bool isUseElectric = false, int numElectricUse = 3)
        {
            isLose = false;
            if (reviveCorutine != null)
                StopCoroutine(reviveCorutine);

            reviveCorutine = ReviveHandle(isAddTime, isResetSlot, isReviveLoseExtral, isUseElectric, numElectricUse);
            StartCoroutine(reviveCorutine);
        }

        private IEnumerator ReviveHandle(bool isAddTime, bool isResetSlot, bool isReviveLoseExtral = false, bool isUseElectric = false, int numElectricUse = 3)
        {
            yield return new WaitForSeconds(0.5f);
            if (isAddTime)
            {
                StartCoroutine(waringTimePlayCorutine);
            }

            BirdSort2.GamePlayController_BirdSort.Instance.state = StateGame.Playing;

            GamePlayController.Instance.itemController.suggetUseItemInGame.ResetSugget();
        }


        public void OnClickOutSide()
        {
            m_standChosing = null;
            GamePlayController_BirdSort.Instance.breakRuleController.standCurrentClick = null;
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.DISABLE_OUTLINE_BIRD);
        }    

        private void OnDestroy()
        {
            if (waringTimePlayCorutine != null)
                StopCoroutine(waringTimePlayCorutine);
            if (reviveCorutine != null)
                StopCoroutine(reviveCorutine);

            GameController.Instance.musicManager.PauseCoolDownSoundSource();
        }
    }

   
}
