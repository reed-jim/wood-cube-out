
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BirdSort2
{
    using DG.Tweening;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;
    using System;
        
    public class Level_BirdSort_2 : MonoBehaviour
    {
        public LevelConfig levelConfig;
        public ScrewBoxManager screwBoxManager;
        public ScrewManager screwManager;
        
        //[HideInInspector] public float timePlayLevel;
        //[HideInInspector] public float timePlayRequire;
        [HideInInspector] public int starWin;
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
        public Transform parentPanelGamePlay;

        public bool isInited;
        public float timePlayLevel;
        [HideInInspector] public float timePlayRequire;
        public AudioSource dropAudioLeft;
        public AudioSource dropAudioRight;
        public AudioClip audioClipDrop;
        private bool checkLsStandLeft;
        private bool checkLsStandRight;
        [HideInInspector] public bool isTempPauseLevel;
        
        public Dictionary<int, List<Transform>> pieceDependencies;
        
        private LevelDifficultyConfiguration _currentLevelDifficultyConfiguration;

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
            if (UseProfile.CurrentLevel > Context.MAX_LEVEL)
                return;
            numBirdCount = 0;
            isBirdFlying = true;
            string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>($"Level {UseProfile.CurrentLevel}");

            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject level = Instantiate(op.Result, transform);
                    List<BasicObjectPart> pieces = TransformUtil.GetComponentsFromAllChildren<BasicObjectPart>(level.transform);

                    Dictionary<int, List<Transform>> obstaclesOfPieces = LevelDifficultyCalculatorUltility.FindPieceDependency(pieces.ToArray());
                    if (level.GetComponent<LevelTransformManualModifier>() == null)
                    {
                        level.transform.position = new Vector3(0, -0.03f * GamePlayController_BirdSort.Instance.gameScene.screenSizeWorld.y, 10);
                        // level.transform.rotation = Quaternion.Euler(new Vector3(-30, -45, 30));
                        level.transform.rotation = Quaternion.Euler(new Vector3(-0, -45, 0));
                    }

                    _currentLevelDifficultyConfiguration = LevelDifficultyCalculatorUltility.GenerateScrewBoxDifficultyTemplate(UseProfile.CurrentLevel);

                    if (UseProfile.CurrentLevel == 1)
                    {
                        AutoAssignScrewFactionLevelManualGroup(level);
                    }
                    else
                    {
                        AutoAssignScrewFaction(level);
                    }

                    AutoAssignScrewFactionMultiPhaseLevel(level);
                    screwBoxManager.Init();
                    screwManager.Init();
                    //if (isLoopLevel)
                    //{
                        //RandomizeLevelModelColor(level);
                    //}
                    //else
                    //{
                        //SaferioTracking.TrackLevelStart(UseProfile.CurrentLevel);
                    //}

                    //if (IsShowBoosterSuggestion(nextLevel, isLoopLevel))
                    //{
                    //    DelayShowBoosterSuggest();
                    //}
                }
                else
                {
                    Debug.Log("LoadLevelFailed");
                }
            };
            isInited = true;
            waringTimePlayCorutine = WaringTime();
            StartCoroutine(waringTimePlayCorutine);
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
        private void AutoAssignScrewFactionLevelManualGroup(GameObject level)
        {
            BaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<BaseScrew>(level.transform).ToArray();

            int numScrew = screws.Length;

            int numTripleScrews = numScrew / 3;

            List<GameEnum.GameFaction> factionPool = new List<GameEnum.GameFaction>();

            for (int i = 0; i < numTripleScrews; i++)
            {
                GameEnum.GameFaction randomFaction = GameConstants.SCREW_FACTION[UnityEngine.Random.Range(0, GameConstants.SCREW_FACTION.Length)];

                factionPool.Add(randomFaction);
            }

            ScrewFactionGroupManual[] screwFactionGroups = new ScrewFactionGroupManual[numScrew];

            for (int i = 0; i < numScrew; i++)
            {
                screwFactionGroups[i] = screws[i].GetComponent<ScrewFactionGroupManual>();
            }

            // screwFactionGroups = screwFactionGroups.OrderBy(item => item.Group).ToArray();

            for (int i = 0; i < numScrew; i++)
            {
                screws[i].ScrewId = i;
                screws[i].Faction = factionPool[screwFactionGroups[i].Group];

                screws[i].ScrewServiceLocator.screwFaction.SetColorByFaction();
            }

            //setLevelScrewNumberEvent?.Invoke(numScrew);
        }

        public bool IsWin()
        {
            

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
            GameController.Instance.AnalyticsController.LogLevelComplet(UseProfile.CurrentLevel - 1, (int)timePlayLevel);
            GameController.Instance.musicManager.PauseCoolDownSoundSource();
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
                GamePlayController_BirdSort.Instance.state = StateGame.Pause;
                GameController.Instance.musicManager.PauseCoolDownSoundSource();
            }
            else
            {
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
        private void AutoAssignScrewFaction(GameObject level)
        {
            BaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<BaseScrew>(level.transform).ToArray();

            int numScrew = screws.Length;

            List<BaseScrew> noBlockedScrews = new List<BaseScrew>();
            List<BaseScrew> blockedScrews = new List<BaseScrew>();

            for (int i = 0; i < screws.Length; i++)
            {
                int numBlockedObject = screws[i].CountBlockingObjects(isIncludeHiddenScrew: true);

                if (numBlockedObject == 0)
                {
                    noBlockedScrews.Add(screws[i]);
                }
                else
                {
                    blockedScrews.Add(screws[i]);
                }
            }

            // SHUFFLE SCREW LISTS
            ShuffleList(noBlockedScrews);
            ShuffleList(blockedScrews);

            List<BaseScrew> shuffledScrews = new List<BaseScrew>();

            float ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame = _currentLevelDifficultyConfiguration.RatioGeneratingSameFactionForTripleNoBlockedScrewStartGame;

            // float ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame =
            //     levelDataContainer.LevelsData[currentLevel.Value].LevelDifficultyConfiguration.RatioGeneratingSameFactionForTripleNoBlockedScrewStartGame;

            int numNoBlockedScrewsToGetSameFaction = (int)(ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame * noBlockedScrews.Count);
            int numRemainingScrews = numScrew - numNoBlockedScrewsToGetSameFaction;

            // EASY SCREWS
            for (int i = 0; i < numNoBlockedScrewsToGetSameFaction; i++)
            {
                BaseScrew nextScrew = noBlockedScrews.First();

                shuffledScrews.Add(nextScrew);

                noBlockedScrews.Remove(nextScrew);
            }

            // REMAINING SCREWS
            int currentIndex = 0;

            for (int i = 0; i < numRemainingScrews; i++)
            {
                BaseScrew nextScrew;

                if (currentIndex % (GameConstants.NUMBER_SLOT_PER_SCREW_BOX - 1) == 0)
                {
                    if (noBlockedScrews.Count > 0)
                    {
                        nextScrew = noBlockedScrews.First();

                        noBlockedScrews.Remove(nextScrew);
                    }
                    else
                    {
                        nextScrew = blockedScrews.First();

                        blockedScrews.Remove(nextScrew);
                    }
                }
                else
                {
                    if (blockedScrews.Count > 0)
                    {
                        nextScrew = blockedScrews.First();

                        blockedScrews.Remove(nextScrew);
                    }
                    else
                    {
                        nextScrew = noBlockedScrews.First();

                        noBlockedScrews.Remove(nextScrew);
                    }
                }

                shuffledScrews.Add(nextScrew);

                currentIndex++;
            }

            int currentFaction = 0;

            List<GameEnum.GameFaction> remainingFactionForBlockedScrews = new List<GameEnum.GameFaction>();

            for (int i = 0; i < shuffledScrews.Count; i++)
            {
                remainingFactionForBlockedScrews.Add(GameConstants.SCREW_FACTION[currentFaction]);

                if (i > 0 && (i + 1) % 3 == 0)
                {
                    currentFaction++;

                    if (currentFaction >= GameConstants.SCREW_FACTION.Length)
                    {
                        currentFaction = 0;
                    }
                }
            }

            for (int i = 0; i < shuffledScrews.Count; i++)
            {
                GameEnum.GameFaction nextFaction = remainingFactionForBlockedScrews.First();

                shuffledScrews[i].ScrewId = i;
                shuffledScrews[i].Faction = nextFaction;

                shuffledScrews[i].ScrewServiceLocator.screwFaction.SetColorByFaction();

                remainingFactionForBlockedScrews.Remove(nextFaction);
            }

            // RAMDOMIZE
            // for (int i = 0; i < shuffledScrews.Count; i++)
            // {
            //     int randomIndex = UnityEngine.Random.Range(0, remainingFactionForBlockedScrews.Count);

            //     shuffledScrews[i].ScrewId = i;
            //     shuffledScrews[i].Faction = remainingFactionForBlockedScrews[randomIndex];

            //     shuffledScrews[i].ScrewServiceLocator.screwFaction.SetColorByFaction();

            //     remainingFactionForBlockedScrews.RemoveAt(randomIndex);
            // }

            //setLevelScrewNumberEvent?.Invoke(shuffledScrews.Count);
        }
        void ShuffleList<T>(List<T> list)
        {
            System.Random rand = new System.Random();
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        private void AutoAssignScrewFactionMultiPhaseLevel(GameObject level)
        {
            MultiPhaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<MultiPhaseScrew>(level.transform).ToArray();

            Dictionary<int, List<GameEnum.GameFaction>> remainingFactionForScrewsByPhase = new Dictionary<int, List<GameEnum.GameFaction>>();

            Dictionary<int, int> currentFactionByPhase = new Dictionary<int, int>();

            for (int i = 0; i < screws.Length; i++)
            {
                int phase = screws[i].Phase;

                if (!remainingFactionForScrewsByPhase.ContainsKey(phase))
                {
                    currentFactionByPhase.Add(phase, 0);

                    remainingFactionForScrewsByPhase.Add(phase, new List<GameEnum.GameFaction>() { GameConstants.SCREW_FACTION[currentFactionByPhase[phase]] });
                }
                else
                {
                    remainingFactionForScrewsByPhase[phase].Add(GameConstants.SCREW_FACTION[currentFactionByPhase[phase]]);

                    if (i > 0 && (i + 1) % 3 == 0)
                    {
                        currentFactionByPhase[phase]++;

                        if (currentFactionByPhase[phase] >= GameConstants.SCREW_FACTION.Length)
                        {
                            currentFactionByPhase[phase] = 0;
                        }
                    }
                }
            }

            for (int i = 0; i < screws.Length; i++)
            {
                int phase = screws[i].Phase;

                int randomIndex = UnityEngine.Random.Range(0, remainingFactionForScrewsByPhase[phase].Count);

                screws[i].ScrewId = i;
                screws[i].Faction = remainingFactionForScrewsByPhase[phase][randomIndex];

                screws[i].ScrewServiceLocator.screwFaction.SetColorByFaction();

                remainingFactionForScrewsByPhase[phase].RemoveAt(randomIndex);
            }

            //setMultiPhaseLevelScrewNumberEvent?.Invoke(screws.Length);
        }
    }

   
}
