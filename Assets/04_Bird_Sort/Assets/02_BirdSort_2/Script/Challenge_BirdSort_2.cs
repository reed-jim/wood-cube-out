
namespace BirdSort2
{
    using DG.Tweening;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;
    using System;

    public class Challenge_BirdSort_2 : Level_BirdSort_2
    {
        public RandomBirdInGame rand;

        public override void Init()
        {
#if !TESTER
            Tools.SetActive(false);
#endif
            GamePlayController_BirdSort.Instance.levelInd = UseProfile.CurrentHardLevel;
            LsStandingBasesLeft = new List<StandingBase>();
            LsStandingBasesRight = new List<StandingBase>();
            numBirdCount = 0;

            standsInLevel = new List<StandingBase>();
            standsInLevel_Real = new List<StandingBase>();

            isBirdFlying = true;
            string pathLevel = StringHelper.PATH_CONFIG_LEVEL;

            TextAsset lvJson = null;
            lvJson = Resources.Load<TextAsset>(string.Format(pathLevel,
BirdSortGameController.Instance.segmentController.GetCurrentSegment().challengeModeData.config.levelStart +
UseProfile.CurrentHardLevel * 5));

            if (lvJson != null)
            {
                levelConfig = JsonUtility.FromJson<LevelConfig>(lvJson.ToString());

                levelConfig = AddRandomExtralTypeMystery(levelConfig, levelConfig.standConfig.Count - 2, (levelConfig.standConfig.Count - 2) * 2);
                //}
                levelConfig = rand.DoRandomList(levelConfig);
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
                        //L?y c?a bên nhi?u hon
                        if (numStandRight + 1 < numStandLef) //Tính c? xem video thêm cành v?n bé hon 
                        {
                            StandParentLeft.ScaleWithNumStand(numStandLef);
                            StandParentRight.ScaleWithNumStand(numStandLef);
                            sideAddStand = 1;
                        }
                        else
                        {
                            numStandRight += 1; //Tính c? xem video thêm cành
                            StandParentLeft.ScaleWithNumStand(numStandRight);
                            StandParentRight.ScaleWithNumStand(numStandRight);
                            sideAddStand = 1;
                        }
                    }
                    else
                    {
                        //L?y c?a bên nhi?u hon
                        if (numStandLef + 1 < numStandRight) //Tính c? xem video thêm cành v?n bé hon 
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

        public override void CheckAllStandInited()
        {
            numStandsInited++;
            if (numStandsInited >= standsInLevel_Real.Count)
            {
                if (levelConfig != null)
                {
                    GamePlayController_BirdSort.Instance.extralPlayController.SpawnChallengeExtrals(levelConfig.extralsConfig);
                    StartCoroutine(Helper.StartAction(
                        () =>
                        {

                            GamePlayController_BirdSort.Instance.extralPlayController.AppearExtral();


                        },
                        () => GamePlayController_BirdSort.Instance.state == StateGame.Playing));
                }
            }
        }

        public override void Win()
        {
            GamePlayController_BirdSort.Instance.state = StateGame.Win;
            UseProfile.CurrentHardLevel += 1;

            GamePlayController_BirdSort.Instance.challengeGameplayController.WinLevel();
            GamePlayController_BirdSort.Instance.eventSaleController.WinLevel();
            StartCoroutine(WinHandle());
            StartCoroutine(RemoveBranchEndGame());
            BirdSortGameController.Instance.musicManager.PauseCoolDownSoundSource();
        }

        public override IEnumerator WinHandle()
        {
            isBirdFlying = true;
            yield return new WaitForSeconds(0.75f);
            //Hi?n popup win
            BirdSortGameController.Instance.musicManager.PlayWinSound();
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

            Debug.LogError("win");

            WinBoxChallenge.Setup().Show(star, (int)Mathf.Abs(timePlayRequire - timePlayLevel));
        }

        public override void NextLevel()
        {
            UseProfile.CurrentHardLevel += 1;
            BirdSortGameController.Instance.LoadScene(SceneName.GAME_PLAY_CHALLENGE);
        }

        public override void BackLevel()
        {
            UseProfile.CurrentHardLevel -= 1;
            BirdSortGameController.Instance.LoadScene(SceneName.GAME_PLAY_CHALLENGE);
        }

        public LevelConfig AddRandomExtralTypeMystery(LevelConfig level, int minBirdId, int maxBirdId, bool isTut = false)
        {
            if (isTut)
            {

                // t?o extral m?i
                ExtralPlayData extral = new ExtralPlayData
                {
                    type = ExtralPlayType.Mystery,

                    positionData = new ExtralPositionData
                    {
                        posType = ExtralPlayPosition.InBird,
                        indexStand = 2,
                        indexBird = 1,
                        localPosition = Vector3.zero
                    },
                    birdParent = null,
                    standParent = null
                };

                level.extralsConfig.Add(extral);
            }
            else
            {
                List<(int standIndex, int birdIndex)> validPositions = new();
                List<(int standIndex, int birdIndex)> eggsPos = new();

                foreach (var item in level.extralsConfig)
                {
                    if (item.type == ExtralPlayType.EggLock)
                    {
                        var stand = item.positionData.indexStand;
                        var bird = item.positionData.indexBird;
                        eggsPos.Add((stand, bird));
                    }
                    else
                        continue;
                }

                // tìm v? trí h?p l?
                for (int i = 0; i < level.standConfig.Count; i++)
                {
                    var stand = level.standConfig[i];

                    if (stand.idBirds == null || stand.idBirds.Count <= 1)
                        continue;

                    // b? con d?u stack
                    for (int j = 1; j < stand.idBirds.Count; j++)
                    {
                        if (!eggsPos.Contains((i, j)))
                        {
                            validPositions.Add((i, j));
                        }
                    }
                }

                // random bird id
                int randomBirdNum = UnityEngine.Random.Range(minBirdId, maxBirdId + 1);
                Debug.Log($"Add extral type 6 -> Bird Num {randomBirdNum}");

                while (randomBirdNum > 0)
                {
                    int index = UnityEngine.Random.Range(0, validPositions.Count);
                    // random v? trí
                    var pos = validPositions[index];
                    // t?o extral m?i
                    ExtralPlayData extral = new ExtralPlayData
                    {
                        type = ExtralPlayType.Mystery,

                        positionData = new ExtralPositionData
                        {
                            posType = ExtralPlayPosition.InBird,
                            indexStand = pos.standIndex,
                            indexBird = pos.birdIndex,
                            localPosition = Vector3.zero
                        },
                        birdParent = null,
                        standParent = null
                    };

                    level.extralsConfig.Add(extral);

                    validPositions.RemoveAt(index);

                    randomBirdNum--;
                }

            }
            return level;
        }
    }


}
