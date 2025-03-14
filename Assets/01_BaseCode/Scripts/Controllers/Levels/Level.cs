using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;

[System.Serializable]
public class LevelConfig
{
    public int idMap;
    public int timePlay;
    public int idHardMode;
    public float percentRequirementCover;

    public List<BirdLevelData> birdLevelData;
    public List<CheckPointConfig> specialPoints;
    public List<RequriemntData> requirements;
    

    public int GetTotalBird()
    {
        int total = 0;
        if (birdLevelData != null)
        {
            for (int i = 0; i < birdLevelData.Count; i++)
            {
                total += birdLevelData[i].total;
            }
        }

        if (specialPoints != null)
        {
            total += specialPoints.Count;
        }

        return total;
    }

    public void ReLoadLevelBySkillUser()
    {

    }
}

[System.Serializable]
public class BirdLevelData
{
    public int idBird;
    public int total;
}

[System.Serializable]
public class CheckPointConfig
{
    public int idSlot;
    public int idBird;
}

[System.Serializable]
public class RequriemntData
{
    public int idBird;
    public int numRequire;
    public int numFound;
}

public class Level : MonoBehaviour
{
    public static bool isHasBeenWinLevel;

    public LevelConfig levelConfig;

    public bool isInited;
    [HideInInspector] public float timePlayLevel;
    [HideInInspector] public float timePlayRequire;

    [Header("Bird Data")]
    public List<BirdBase> birdsPrefab;
    [HideInInspector] public Dictionary<int, List<BirdBase>> birdsInLevel;
    [HideInInspector] public Dictionary<int, List<BirdBase>> birdsHasExtral;
    public Transform birdParent;
    private int numBirdCount;
    public int numBirdInLevel;
    [HideInInspector] public bool isBirdFlying;

    [Header("Map")]
    [HideInInspector] public Map map;
    [SerializeField] private Transform parentMap;
    private List<CheckPointTran> checkPoints;

    [Header("Requirement")]
    [HideInInspector] public Dictionary<int, RequriemntData> requirements;

    [Header("Slot")]
    public SlotController slotController;

    private IEnumerator match3SortCoroutine;
    private IEnumerator waringTimePlayCorutine;
    private IEnumerator spawnExtralCorutine;
    private IEnumerator reviveCorutine;

    [HideInInspector] public BirdBase birdChoosing;

    public Transform posLimitScreenTL;
    public Transform posLimitScreenTR;
    public Transform posLimitScreenBL;
    public Transform posLimitScreenBR;

    [HideInInspector] public bool isClickToScreen;

    [HideInInspector] public bool isHasExtral;
    public Transform extralShowParent;

    [HideInInspector] public int starWin;
    public CanvasGroup panelBlack;

    [SerializeField] private AudioSource coolDownAudioSource;
    [SerializeField] private GameObject[] fxWinsPre;
    [SerializeField] private SpriteRenderer bgWinsPre;
    [HideInInspector] public bool isTempPauseLevel;

    public int totalBirdInCover = 0;//Tất cả các bird bị che

    public virtual void Init()
    {
        numBirdCount = 0;
        slotController.Init();
        isInited = true;

        panelBlack.gameObject.SetActive(true);
        StartCoroutine(SpawnLevel());
    }

    public void InitState()
    {

    }

    public IEnumerator SpawnLevel()
    {
        birdsInLevel = new Dictionary<int, List<BirdBase>>();
        birdsHasExtral = new Dictionary<int, List<BirdBase>>();
        requirements = new Dictionary<int, RequriemntData>();

        //LongTH
        string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
        //string pathLevel = StringHelper.PATH_CONFIG_LEVEL_BIRD1;
        //--------------------------------------------------------------------------
        if (!RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_TEST_LEVEL, true))
            pathLevel = StringHelper.PATH_CONFIG_LEVEL_EASY;

        TextAsset lvJson = Resources.Load<TextAsset>(string.Format(pathLevel, GameController.Instance.useProfile.CurrentLevelPlay));
        if (lvJson != null)
        {
            levelConfig = JsonUtility.FromJson<LevelConfig>(lvJson.ToString());
            //Debug.Log("isHasExtral " + isHasExtral);
        }
        else
        {
            yield return null;
        }


        yield return new WaitForSeconds(0.2f);
        panelBlack.DOFade(0, 0.3f).OnComplete(() => { panelBlack.gameObject.SetActive(false); });
        {
            if (levelConfig != null)
            {
                //Spawn Map
                //Add CheckPont from Map Prefab
                string pathMap = StringHelper.PATH_CONFIG_MAP;
                map = Instantiate(Resources.Load<Map>(string.Format(pathMap, levelConfig.idMap)), parentMap);
                map.Init(isRandomCheckPoint: !isHasExtral);
                map.InitState();

                //Current Skill User
                var currentSkillUser = GameController.Instance.skillUserController.GetCurrentSkillData();

                checkPoints = new List<CheckPointTran>(map.checkPoints);
                Dictionary<int, List<BirdBase>> requireBirds = new Dictionary<int, List<BirdBase>>();
                Dictionary<int, int> numRequireBirdsInCover = new Dictionary<int, int>();
                Dictionary<int, List<BirdBase>> birdInSpecialPoint = new Dictionary<int, List<BirdBase>>();
                float percentCover = levelConfig.percentRequirementCover;
                percentCover *= currentSkillUser.coPercentCover;
                if (percentCover > 1)
                    percentCover = 1;
                if (percentCover < 0)
                    percentCover = 0;

                int numBirdInCover = 0;
                int totalBirdRequire = 0;
                for (int i = 0; i < levelConfig.requirements.Count; i++)
                {
                    int index = i;
                    var require = levelConfig.requirements[index];
                    var idBird = require.idBird;
                    var numRequire = require.numRequire;

                    requirements.Add(idBird, require);
                    int numInCover = Mathf.RoundToInt(percentCover * numRequire);
                    if (numInCover > numRequire)
                        numInCover = numRequire;

                    numBirdInCover += (numInCover * 2);
                    totalBirdRequire += numRequire;
                    numRequireBirdsInCover.Add(idBird, numInCover);
                }

                //Get Total Bird
                Dictionary<int, int> birdsTrash = new Dictionary<int, int>();
                List<int> idsBirdTrash = new List<int>();
                int totalBird = 0;
                int numBirdTrash = 0;//Tổng số bird rác, ko tính các bird trong special point
                int totalBirdNoRequiremntInSpecialPoint = 0;
                for (int i = 0; i < levelConfig.specialPoints.Count; i++)
                {
                    var idBird = levelConfig.specialPoints[i].idBird;

                    totalBirdNoRequiremntInSpecialPoint++;
                    totalBird++;
                }

                for (int i = 0; i < levelConfig.birdLevelData.Count; i++)
                {
                    var idBird = levelConfig.birdLevelData[i].idBird;
                    if (!requirements.ContainsKey(idBird))
                    {
                        if (birdsTrash.ContainsKey(idBird))
                        {
                            birdsTrash[idBird] += levelConfig.birdLevelData[i].total;
                            numBirdTrash += levelConfig.birdLevelData[i].total;

                        }
                        else
                        {
                            birdsTrash.Add(idBird, levelConfig.birdLevelData[i].total);
                            numBirdTrash += levelConfig.birdLevelData[i].total;
                        }
                        idsBirdTrash.Add(idBird);
                    }
                    totalBird += levelConfig.birdLevelData[i].total;
                }

                int numBirdTrashCorrectionSkillUser = (int)(numBirdTrash * currentSkillUser.coNumber);
                if (currentSkillUser.coNumber != 1)
                    numBirdTrashCorrectionSkillUser = MMMaths.RoundingDivideAll(3, numBirdTrashCorrectionSkillUser, currentSkillUser.coNumber > 0);
                int totalBirdAfter = totalBirdRequire + numBirdTrashCorrectionSkillUser + totalBirdNoRequiremntInSpecialPoint;
                if (totalBirdAfter > map.maxNumCheckPoint)
                {
                    int c = totalBirdAfter - map.maxNumCheckPoint;//Bird rác thừa ra
                    numBirdTrashCorrectionSkillUser -= c;
                }

                timePlayRequire = levelConfig.timePlay;
                GamePlayController.Instance.gameScene.ShowTimePlayGame(timePlayRequire);

                //Spawn Birds Special
                int countCheckPoint = checkPoints.Count;
                for (int i = 0; i < levelConfig.specialPoints.Count; i++)
                {
                    int index = i;
                    var checkPointData = levelConfig.specialPoints[Mathf.Clamp(index, 0, countCheckPoint - 1)];
                    var bird = SpawnBird(checkPointData.idBird, checkPointData.idSlot, true);

                    if (requirements.ContainsKey(bird.id))
                    {
                        if (requireBirds.ContainsKey(bird.id))
                        {
                            requireBirds[bird.id].Add(bird);
                        }
                        else
                        {
                            List<BirdBase> lst = new List<BirdBase>();
                            lst.Add(bird);
                            requireBirds.Add(bird.id, lst);
                        }
                    }

                    if (birdInSpecialPoint.ContainsKey(bird.id))
                    {
                        birdInSpecialPoint[bird.id].Add(bird);
                    }
                    else
                    {
                        List<BirdBase> lst = new List<BirdBase>();
                        lst.Add(bird);

                        birdInSpecialPoint.Add(bird.id, lst);
                    }

                    //yield return new WaitForSeconds(0.01f);
                }
                checkPoints.RemoveAll(s => !s.IsEmpty);
                checkPoints.RemoveAll(s => s.IsOutScreen);
                checkPoints = Helper.DisruptiveList(checkPoints);
                countCheckPoint = checkPoints.Count;
                int indexCheckPoint = 0;
                float timeDelay = 0;

                //Spawn Requiremt Birds
                foreach (var item in requirements)
                {
                    if (requireBirds.ContainsKey(item.Key))
                    {
                        if (requireBirds[item.Key].Count < item.Value.numRequire)
                        {
                            int numLeft = item.Value.numRequire - requireBirds[item.Key].Count;
                            for (int i = 0; i < numLeft; i++)
                            {
                                int index = i;
                                var checkPoint = checkPoints[Mathf.Clamp(indexCheckPoint, 0, countCheckPoint - 1)];
                                var bird = SpawnBird(item.Key, checkPoint, true, timeDelay);
                                indexCheckPoint++;
                                timeDelay += 0.02f;
                                requireBirds[item.Key].Add(bird);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < item.Value.numRequire; i++)
                        {
                            int index = i;
                            var checkPoint = checkPoints[Mathf.Clamp(indexCheckPoint, 0, countCheckPoint - 1)];
                            var bird = SpawnBird(item.Key, checkPoint, true, timeDelay);
                            indexCheckPoint++;
                            timeDelay += 0.02f;


                            if (requireBirds.ContainsKey(bird.id))
                            {
                                requireBirds[bird.id].Add(bird);
                            }
                            else
                            {
                                List<BirdBase> lst = new List<BirdBase>();
                                lst.Add(bird);
                                requireBirds.Add(bird.id, lst);
                            }
                        }
                    }
                }


                //Spawn Other Bird & Cover Bird in Special Point
                checkPoints.RemoveAll(s => !s.IsEmpty);
                checkPoints.RemoveAll(s => s.IsOutScreen);
                checkPoints = Helper.DisruptiveList(checkPoints);
                countCheckPoint = checkPoints.Count;
                indexCheckPoint = 0;
                List<BirdBase> birdsCover = new List<BirdBase>();

                UnityAction<int> actionSpawnBirds = (index) =>
                {
                    if (numBirdInCover > 0)
                    {
                        //Spawn Cover Bird
                        var birdPrefab = GamePlayController.Instance.level.birdsPrefab[levelConfig.birdLevelData[index].idBird];
                        var bird = Instantiate(birdPrefab, GamePlayController.Instance.level.birdParent);
                        bird.gameObject.SetActive(true);
                        birdsCover.Add(bird);
                        numBirdInCover--;
                    }
                    else
                    {
                        //Spawn Other Bird
                        var checkPoint = checkPoints[Mathf.Clamp(indexCheckPoint, 0, countCheckPoint - 1)];
                        var bird = SpawnBird(levelConfig.birdLevelData[index].idBird, checkPoint, true, timeDelay);
                        indexCheckPoint++;
                        timeDelay += 0.02f;
                    }
                };

                for (int i = 0; i < levelConfig.birdLevelData.Count; i++)
                {
                    if (numBirdTrashCorrectionSkillUser <= 0)
                        break;
                    if (requireBirds.ContainsKey(levelConfig.birdLevelData[i].idBird))
                        continue;
                    if (!birdInSpecialPoint.ContainsKey(levelConfig.birdLevelData[i].idBird))
                        continue;
                    int index = i;
                    for (int j = 0; j < levelConfig.birdLevelData[i].total; j++)
                    {
                        if (numBirdTrashCorrectionSkillUser <= 0)
                            break;

                        actionSpawnBirds(index);
                        numBirdTrashCorrectionSkillUser--;
                    }
                }



                //Spawn Other Bird & Cover Bird
                checkPoints.RemoveAll(s => !s.IsEmpty);
                checkPoints.RemoveAll(s => s.IsOutScreen);
                checkPoints = Helper.DisruptiveList(checkPoints);
                countCheckPoint = checkPoints.Count;
                indexCheckPoint = 0;
                for (int i = 0; i < levelConfig.birdLevelData.Count; i++)
                {
                    if (numBirdTrashCorrectionSkillUser <= 0)
                        break;
                    if (requireBirds.ContainsKey(levelConfig.birdLevelData[i].idBird))
                        continue;
                    if (birdInSpecialPoint.ContainsKey(levelConfig.birdLevelData[i].idBird))
                        continue;
                    int index = i;
                    for (int j = 0; j < levelConfig.birdLevelData[i].total; j++)
                    {
                        if (numBirdTrashCorrectionSkillUser <= 0)
                            break;

                        actionSpawnBirds(index);
                        numBirdTrashCorrectionSkillUser--;
                    }
                }

                timeDelay = 0;
                //Set position Cover Bird
                int indexBirdCover = 0;
                birdsCover = Helper.DisruptiveList(birdsCover);
                foreach (var item in requireBirds)
                {
                    int numBirdRequieInCover = numRequireBirdsInCover[item.Key];
                    for (int i = 0; i < numBirdRequieInCover; i++)
                    {
                        var checkPointsCover = item.Value[i].checkPointIn.checkPointsCover;
                        item.Value[i].isBirdHasCover = true;
                        item.Value[i].isBirdInCover = true;
                        totalBirdInCover++;
                        if (indexBirdCover >= birdsCover.Count)
                            break;
                        for (int j = 0; j < checkPointsCover.Count; j++)
                        {
                            var birdCover = birdsCover[indexBirdCover];

                            SetUpSpawnBird(birdCover, checkPointsCover[j], true, timeDelay);
                            birdCover.isBirdHasCover = true;
                            timeDelay += 0.02f;
                            indexBirdCover++;
                            if (indexBirdCover >= birdsCover.Count)
                                break;
                        }
                    }
                }

                if (numBirdTrashCorrectionSkillUser > 0)
                {
                    int totalBirdCurrent = 0;
                    foreach (var lstBird in birdsInLevel)
                    {
                        totalBirdCurrent += lstBird.Value.Count;
                    }

                    numBirdTrashCorrectionSkillUser = MMMaths.RoundingDivideAll(3, numBirdTrashCorrectionSkillUser, true);
                    if (totalBirdCurrent + numBirdTrashCorrectionSkillUser > map.maxNumCheckPoint)
                    {
                        int c = Mathf.Abs(map.maxNumCheckPoint - (totalBirdCurrent + numBirdTrashCorrectionSkillUser));
                        numBirdTrashCorrectionSkillUser -= c;
                        if (numBirdTrashCorrectionSkillUser > 2)
                        {
                            numBirdTrashCorrectionSkillUser = MMMaths.RoundingDivideAll(3, numBirdTrashCorrectionSkillUser, true);
                            if (totalBirdCurrent + numBirdTrashCorrectionSkillUser > map.maxNumCheckPoint)
                            {
                                numBirdTrashCorrectionSkillUser -= 3;
                            }
                        }
                    }

                    if (idsBirdTrash.Count > 0)
                    {
                        int indexId = 0;
                        if (numBirdTrashCorrectionSkillUser % 3 == 0)
                        {
                            //Spawn bird rác (nếu còn) cho skill user system
                            while (numBirdTrashCorrectionSkillUser > 0)
                            {
                                numBirdTrashCorrectionSkillUser -= 3;
                                for (int i = 0; i < 3; i++)
                                {
                                    var checkPoint = checkPoints[Mathf.Clamp(indexCheckPoint, 0, countCheckPoint - 1)];
                                    var bird = SpawnBird(idsBirdTrash[Mathf.Clamp(indexId, 0, idsBirdTrash.Count - 1)], checkPoint, true, timeDelay);
                                    indexCheckPoint++;
                                    timeDelay += 0.02f;
                                }
                                indexId++;
                            }
                        }
                    }
                }

                //Sinh thêm bird nếu thiếu (không chia hết cho 3 sau khi nhân hệ số skill)
                Dictionary<int, List<BirdBase>> m_birdsInLevel = new Dictionary<int, List<BirdBase>>(birdsInLevel);
                foreach (var item in m_birdsInLevel)
                {
                    int currentCount = item.Value.Count;

                    if (currentCount % 3 != 0)
                    {
                        int m_currentCount = MMMaths.RoundingDivideAll(3, currentCount, true);
                        int numAdd = Mathf.Abs(m_currentCount - currentCount);
                        for (int i = 0; i < numAdd; i++)
                        {
                            var checkPoint = checkPoints[Mathf.Clamp(indexCheckPoint, 0, countCheckPoint - 1)];
                            var bird = SpawnBird(item.Key, checkPoint, true, timeDelay);
                            indexCheckPoint++;
                            timeDelay += 0.02f;
                        }
                    }
                }

                //Spawn Requirement UI
                for (int i = 0; i < levelConfig.requirements.Count; i++)
                {
                    int index = i;
                    var require = levelConfig.requirements[index];
                    var idBird = require.idBird;
                    var numRequire = require.numRequire;
                    var iconBird = birdsInLevel[idBird][0].icon;

                    GamePlayController.Instance.gameScene.SpawnRequirement(idBird, iconBird, numRequire);
                }

                if (UseProfile.CurrentLevel <= 1)
                {
                    map.flockController.isLockFlock = true;
                }

                StartCoroutine(GamePlayController.Instance.gameScene.ShowRequirement());
                waringTimePlayCorutine = WaringTime();
                StartCoroutine(waringTimePlayCorutine);

                GameController.Instance.AnalyticsController.LogStartLevel(GameController.Instance.useProfile.CurrentLevelPlay);
            }
        }

       // DebugTotalBird();
    }

    private void DebugTotalBird()
    {
        int totalBirdCurrent = 0;
        foreach (var lstBird in birdsInLevel)
        {
            totalBirdCurrent += lstBird.Value.Count;
        }

        //LongTH
        string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
        //string pathLevel = StringHelper.PATH_CONFIG_LEVEL_BIRD1;
        //--------------------------------------------------------------------------
        TextAsset lvJson = Resources.Load<TextAsset>(string.Format(pathLevel, GameController.Instance.useProfile.CurrentLevelPlay));
        if (lvJson != null)
        {
            levelConfig = JsonUtility.FromJson<LevelConfig>(lvJson.ToString());
            //Debug.Log("isHasExtral " + isHasExtral);
            Debug.Log("============= Tổng số bird Trc nhân " + levelConfig.GetTotalBird());
        }

        Debug.Log("============= Tổng số bird " + totalBirdCurrent);
    }

    public BirdBase SpawnBird(int idBird, int idCheckPoint, bool isFlyIn, float delayTimeFlyOut = 0)
    {
        return SpawnBird(idBird, checkPoints[idCheckPoint], isFlyIn, delayTimeFlyOut);
    }

    public BirdBase SpawnBird(int idBird, CheckPointTran checkPoint, bool isFlyIn, float delayTimeFlyOut = 0)
    {
        var birdPrefab = GamePlayController.Instance.level.birdsPrefab[idBird];
        var bird = Instantiate(birdPrefab, GamePlayController.Instance.level.birdParent);

        SetUpSpawnBird(bird, checkPoint, isFlyIn, delayTimeFlyOut);

        return bird;
    }

    protected void SetUpSpawnBird(BirdBase bird, CheckPointTran checkPoint, bool isFlyIn, float delayTimeFlyOut = 0)
    {
        // var bird = GameController.Instance.poolController.poolBirds.GetBird(idBird);
        bird.gameObject.SetActive(true);
        checkPoint.birdOn = bird;
        bird.checkPointIn = checkPoint;
        bird.ChangeLayer(checkPoint.indexLayer, LayerGame.BIRD);
        //bird.body.gameObject.SetActive(false);
        bird.Init(checkPoint.posStart.position, checkPoint.posMiddle.position, checkPoint.posTrans.position, () =>
        {
            bird.transform.parent = birdParent;
        }, isFlyIn, delayTimeFlyOut);

        if (birdsInLevel.ContainsKey(bird.id))
        {
            birdsInLevel[bird.id].Add(bird);
        }
        else
        {
            List<BirdBase> birds = new List<BirdBase>();
            birds.Add(bird);

            birdsInLevel.Add(bird.id, birds);
        }
        numBirdInLevel++;
    }

    public void CheckAllBirdReady()
    {
        numBirdCount++;
        if (numBirdCount >= numBirdInLevel)
        {
            //isTempPauseLevel = true;
            GamePlayController.Instance.state = StateGame.Playing;
            GamePlayController.Instance.itemController.ActiveItemSellectLevel();
            GamePlayController.Instance.challengeGameplayController.SpawnItems();
            isBirdFlying = false;
           
        }
    }

    public void CheckRequiremntTicked(int idBird)
    {
        if (requirements.ContainsKey(idBird))
        {
            requirements[idBird].numFound++;
            int numLeft = requirements[idBird].numRequire - requirements[idBird].numFound;
            var requirementUI = GamePlayController.Instance.gameScene.GetRequirementUI(idBird);
            if (requirementUI != null)
                requirementUI.AddRequire(idBird, numLeft >= 0 ? numLeft : 0);
            if (numLeft <= 0)
            {
                if (requirementUI != null)
                    requirementUI.CompleteRequireTicked(idBird);
            }
        }
    }

    public void ReturnRequiremnt(int idBird)
    {
        if (requirements.ContainsKey(idBird))
        {
            requirements[idBird].numFound--;
            if (requirements[idBird].numFound < 0)
                requirements[idBird].numFound = 0;

            int numLeft = requirements[idBird].numRequire - requirements[idBird].numFound;
            var requirementUI = GamePlayController.Instance.gameScene.GetRequirementUI(idBird);
            if (requirementUI != null)
                requirementUI.AddRequire(idBird, numLeft >= 0 ? numLeft : 0);
            if (numLeft <= 0)
            {
                if (requirementUI != null)
                    requirementUI.CompleteRequireTicked(idBird);
            }
        }
    }

    /// <summary>
    /// Khi 1 Bird bay vào slot => check trong slot đã có đủ 3 chim cùng loại chưa => check end game
    /// </summary>
    public bool CheckMatch3Bird(bool isCheck = false)
    {
        //Lọc
        List<BirdBase> birdsInSlot = new List<BirdBase>();
        for (int i = 0; i < slotController.slots.Count; i++)
        {
            if (!slotController.slots[i].IsEmpty)
            {
                birdsInSlot.Add(slotController.slots[i].birdOn);
            }
        }

        //Đếm (Ko tính các bird đang bay chuyển slot)
        Dictionary<int, List<BirdBase>> dictBird = new Dictionary<int, List<BirdBase>>();
        for (int i = 0; i < birdsInSlot.Count; i++)
        {
            if (!isCheck)
                if (birdsInSlot[i].state != StateBird.InSlot)
                {
                    continue;
                }

            if (dictBird.ContainsKey(birdsInSlot[i].id))
            {
                dictBird[birdsInSlot[i].id].Add(birdsInSlot[i]);
            }
            else
            {
                List<BirdBase> lstBird = new List<BirdBase>();
                lstBird.Add(birdsInSlot[i]);

                dictBird.Add(birdsInSlot[i].id, lstBird);
            }
        }

        bool isHas3 = false;
        //Check Merge Bird
        foreach (var gr in dictBird)
        {
            if (gr.Value.Count >= 3)
            {
                if (!isCheck)
                    Match3Bird(gr.Value[0], gr.Value[1], gr.Value[2]);
                isHas3 = true;
            }
        }

        if (!isCheck)
            CheckPassLevel();

        return isHas3;
    }

    public void Match3Bird(BirdBase b1, BirdBase b2, BirdBase b3)
    {
        b1.state = StateBird.Match3;
        b2.state = StateBird.Match3;
        b3.state = StateBird.Match3;


        List<BirdBase> lstB = new List<BirdBase>();
        lstB.Add(b1);
        lstB.Add(b2);
        lstB.Add(b3);

        lstB = Helper.SelectionSortListMono<BirdBase>(lstB, (a, b) =>
        {
            if (a.transform.position.x > b.transform.position.x)
                return 1;
            else
                return -1;
        });

        UnityAction actionDone = () =>
        {
            for (int i = 0; i < lstB.Count; i++)
            {
                lstB[i].isMatch3Done = true;
            }

            GamePlayController.Instance.scoreController.AddScore();
            var currentCombo = GamePlayController.Instance.scoreController.GetCurrentComboData();
            if (currentCombo != null)
            {

                //  GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
                //lstB[1].transform.position,
                //GiftType.IQ,
                // currentCombo.score,
                // Color.yellow
                //);

                GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                    (
                     lstB[1].transform.position,
                    "+" + currentCombo.score + " IQ",
                    Color.yellow,
                    isSpawnItemPlayer: true
                    );
            }
        };

        //Merge
        int numMatch = 0;
        lstB[0].FlyMerge(lstB[1].transform.position, () =>
        {
            numMatch++;
            lstB[0].DisableBody();
            if (numMatch >= 2)
            {
                lstB[1].ActiveMatch3();
                actionDone?.Invoke();
            }
        }, speed: lstB[0].speedMerge, isSound: false, isSetStateIdle: false);
        lstB[2].FlyMerge(lstB[1].transform.position, () =>
        {
            numMatch++;
            lstB[2].DisableBody();
            if (numMatch >= 2)
            {
                lstB[1].ActiveMatch3();
                actionDone?.Invoke();
            }
        }, speed: lstB[2].speedMerge, isSound: false, isSetStateIdle: false);

        GamePlayController.Instance.returnController.IsLockReturn = true;

        if (match3SortCoroutine != null)
            StopCoroutine(match3SortCoroutine);
        match3SortCoroutine = slotController.ReloadSlot(() => numMatch >= 2);
        StartCoroutine(match3SortCoroutine);
    }

    public void CheckAllRequiremnt()
    {
        foreach (var req in requirements)
        {
            if (req.Value.numFound >= req.Value.numRequire)
            {
                var requirementUI = GamePlayController.Instance.gameScene.GetRequirementUI(req.Value.idBird);
                if (requirementUI != null)
                {
                    requirementUI.CompleteRequire(req.Value.idBird);
                }
            }
        }

        //CheckPassLevel();
    }


    public bool IsWin()
    {
        foreach (var req in requirements)
        {
            if (req.Value.numFound < req.Value.numRequire)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsLose()
    {
        bool isFullSlot = true;
        for (int i = 0; i < slotController.slots.Count; i++)
        {
            if (slotController.slots[i].IsEmpty)
            {
                isFullSlot = false;
                break;
            }
        }

        if (!isFullSlot)
            return false;
        else
            return !CheckMatch3Bird(isCheck: true);

    }

    public void CheckPassLevel()
    {
        if (GamePlayController.Instance.state == StateGame.Win)
            return;


        if (IsWin())
        {
            GamePlayController.Instance.state = StateGame.Win;
            Win();
        }
        else
        {
            CheckLose();
        }
    }

    public void CheckLose()
    {
        if (GamePlayController.Instance.state == StateGame.Win)
            return;
        if (GamePlayController.Instance.state == StateGame.Lose)
            return;

        if (IsLose())
        {
            Lose(type: 0);
        }
    }

    private IEnumerator AllBirdsFlyOut()
    {
        //bgWinsPre.DOKill();
        //bgWinsPre.DOFade(0.2f, 0.15f).OnComplete(()=> {
        //    for (int i = 0; i < fxWinsPre.Length; i++)
        //    {
        //        fxWinsPre[i].gameObject.SetActive(true);
        //    }

        //});
        //yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < fxWinsPre.Length; i++)
        {
            fxWinsPre[i].gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(0.15f);
        int numPlaySound = 0;
        foreach (var lstBird in birdsInLevel)
        {
            for (int i = 0; i < lstBird.Value.Count; i++)
            {
                int index = i;
                if (lstBird.Value[index].isBirdInGadern
                     && !lstBird.Value[index].isLockMove
                     && !lstBird.Value[index].isMatch3Done
                     /*&& lstBird.Value[index].state != StateBird.InSlot*/)
                {

                    bool isPlaySoundFly = false;
                    if (numPlaySound <= 10)
                        isPlaySoundFly = true;
                    lstBird.Value[index].FlyOutRandom(null);
                    numPlaySound++;
                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
    }

    private bool isWon;
    public virtual void Win()
    {
        if (isWon)
            return;

        isWon = true;
        int _currentLevel = UseProfile.CurrentLevel;
        GamePlayController.Instance.state = StateGame.Win;

        UseProfile.CurrentLevel += 1;

        if (UseProfile.CurrentLevel > Context.MAX_LEVEL)
            UseProfile.CurrentLevel = Context.MAX_LEVEL;
        GameController.Instance.useProfile.CurrentLevelPlay = UseProfile.CurrentLevel;
        isHasBeenWinLevel = true;
        GamePlayController.Instance.challengeGameplayController.WinLevel();
        StartCoroutine(WinHandle());
        GameController.Instance.AnalyticsController.LogLevelComplet(UseProfile.CurrentLevel - 1, (int)timePlayLevel);
       // GameController.Instance.musicManager.PauseCoolDownSoundSource();
        StartCoroutine(AllBirdsFlyOut());

        if (!GamePlayController.Instance.itemController.isUsedItem)
        {
            GameController.Instance.skillUserController.AddValueCondition(ConditionChangeSkill.TypeCondition.WinLevelNoUseItem, 1);
        }
        else
        {
            var useItemCondition = GameController.Instance.skillUserController.GetCondition(ConditionChangeSkill.TypeCondition.WinLevelNoUseItem);
            if (useItemCondition != null)
                useItemCondition.EndCondition();
        }

        GameController.Instance.skillUserController.CheckReDefaultCurrentSkill();
    }

    public virtual IEnumerator WinHandle()
    {
        //Show FX Win Ingame

        //Birds Fly out

        yield return new WaitForSeconds(1f);
        //Hiện popup win

        GameController.Instance.musicManager.PlayWinSound();
        //if (GamePlayController.Instance.IsShowRate())
        //{
        //    var rateBox = DialogueRate.Setup();
        //    rateBox.Show();
        //    rateBox.OnCloseBox = () =>
        //    {
        //        GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
        //        {
        //            GameController.Instance.LoadScene(SceneName.GAME_PLAY);
        //        }, actionWatchLog: "Next_Level");

        //        rateBox.OnCloseBox = null;
        //    };
        //}
        //else
        //{
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
        StartChestController.CurrentStar += star;
        LevelChestController.CurrentLevelChest++;
        WinBox.Setup().Show(star, (int)Mathf.Abs(timePlayRequire - timePlayLevel));
        GameController.Instance.skillUserController.AddValueCondition(ConditionChangeSkill.TypeCondition.StarAfterPassLevel, star);


        //Reset conditions Lose
        var loseLevelWithTime = GameController.Instance.skillUserController.GetCondition(ConditionChangeSkill.TypeCondition.LoseLevelWithTime);
        if (loseLevelWithTime != null)
            loseLevelWithTime.EndCondition();
        var numberLoseLevel = GameController.Instance.skillUserController.GetCondition(ConditionChangeSkill.TypeCondition.NumberLoseLevel);
        if (numberLoseLevel != null)
            numberLoseLevel.EndCondition();
        var timeWaitingLevel = GameController.Instance.skillUserController.GetCondition(ConditionChangeSkill.TypeCondition.TimeWaitingLevel);
        if (timeWaitingLevel != null)
            timeWaitingLevel.EndCondition();
        LoseLevelWithTimeCondition.isLosed = false;
    }

    private bool isLose;
    public virtual void Lose(int type, bool isLoseExtral = false)
    {
        if (isLose)
            return;
        isLose = true;
        Debug.Log("========== Lose ============");
        GamePlayController.Instance.state = StateGame.Lose;


        GameController.Instance.AnalyticsController.LogLevelFail(UseProfile.CurrentLevel);
      //  GameController.Instance.musicManager.PauseCoolDownSoundSource();

        GameController.Instance.musicManager.PlayLoseSound();

        StartCoroutine(Helper.StartAction(() => { ReviveBox.Setup().Show(type, GetItemYouMiss(), isLoseExtral); }, 0.45f));
    }
    

    public IEnumerator WaringTime()
    {
        yield return new WaitUntil(() => Mathf.Abs(timePlayRequire - timePlayLevel) <= 10);
      //  GameController.Instance.musicManager.PlayCoolDownSoundSource();
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

        if (isReviveLoseExtral)
        {
        }

        if (isResetSlot)
        {
            GamePlayController.Instance.returnController.ResetStack();
            slotController.ResetSlot();
        }

        GamePlayController.Instance.state = StateGame.Playing;

        if (isUseElectric)
        {
            int numTurn = numElectricUse / 3;
            for (int i = 0; i < numTurn; i++)
            {
                if (GamePlayController.Instance.itemController.UseItem(GiftType.ClearHole))
                {
                    //  GameController.Instance.dataContain.giftDatabase.Claim(GiftType.LightElectricItem, -1, Reason.in_campain_level);
                }
            }


        }
        GamePlayController.Instance.itemController.suggetUseItemInGame.ResetSugget();
    }

    public void PauseLevel(bool isPause)
    {
        if (isPause)
        {
            //if (GamePlayController.Instance.state != StateGame.Playing)
            //    return;

            GamePlayController.Instance.state = StateGame.Pause;
          //  GameController.Instance.musicManager.PauseCoolDownSoundSource();
        }
        else
        {
            //if (GamePlayController.Instance.state != StateGame.Pause)
            //    return;

            GamePlayController.Instance.state = StateGame.Playing;
            if (Mathf.Abs(timePlayRequire - timePlayLevel) <= 10)
            {
           //     GameController.Instance.musicManager.PlayCoolDownSoundSource();
            }
        }
    }

    /// <summary>
    /// Lấy tỉ lệ số lượng requirement đã tìm đc
    /// </summary>
    /// <returns></returns>
    public float GetPercentRequirementFound()
    {
        int numFound = 0;
        int totalRequirement = 0;

        foreach (var req in requirements)
        {
            numFound += req.Value.numFound;
            totalRequirement += req.Value.numRequire;
        }

        if (totalRequirement != 0)
            return (float)numFound / totalRequirement;
        else
            return 0;
    }

    public List<RewardRandom> GetItemYouMiss()
    {
        List<RewardRandom> rewardMiss = new List<RewardRandom>();
        rewardMiss.Add(new RewardRandom() { typeItem = GiftType.Health, amount = 1 });
        rewardMiss.Add(new RewardRandom() { typeItem = GiftType.StarCampain, amount = -1 });

        //Challenge
        var scoreTtemsChallegen = GamePlayController.Instance.challengeGameplayController.scoreItems;
        foreach(var it in scoreTtemsChallegen)
        {
            if(it.Value > 0)
            {
                rewardMiss.Add(new RewardRandom() { typeItem = GameController.Instance.challengeController.GetGiftTypeItemChallenge(it.Key), amount = it.Value });
            }    
        }    


        return rewardMiss;
    }

    private void Update()
    {
        if (isTempPauseLevel)
            return;
        if (GamePlayController.Instance.state != StateGame.Playing)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            isClickToScreen = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (birdChoosing != null)
            {
                birdChoosing.OnChoiceBird();
                birdChoosing = null;
            }

            isClickToScreen = false;
        }


        if (IceItem_BirdSort.isUseIce)
        {
            GamePlayController.Instance.gameScene.ChangeColorTimeLevelTxt(Mathf.Abs(timePlayRequire - timePlayLevel + 1), true);
            return;
        }
        timePlayLevel += Time.deltaTime;
        float timeShow = Mathf.Abs(timePlayRequire - timePlayLevel + 1);
        GamePlayController.Instance.gameScene.ShowTimePlayGame(timeShow);
        if (timePlayLevel >= timePlayRequire)
        {
            if (IsWin())
            {
                GamePlayController.Instance.state = StateGame.Win;
                Win();
            }
            else
            {
                Lose(type: 1);
            }
        }
        GamePlayController.Instance.gameScene.ChangeColorTimeLevelTxt(timeShow, false);
    }

    public void SetCheckPoint(ref CheckPointTran checkPoint, ref BirdBase bird)
    {
        bird.checkPointIn.birdOn = null;//Check point cũ Empty
        checkPoint.birdOn = bird;//Check point mới có Bird
        bird.checkPointIn = checkPoint;//Check point mới có Bird
    }

    public void AddBirdHasExtral(int idBird, BirdBase bird)
    {
        if (birdsHasExtral.ContainsKey(idBird))
        {
            birdsHasExtral[idBird].Add(bird);
        }
        else
        {
            List<BirdBase> lst = new List<BirdBase>();
            lst.Add(bird);

            birdsHasExtral.Add(idBird, lst);
        }
    }

    public void RemoveBirdHasExtral(int idBird, BirdBase bird)
    {
        if (birdsHasExtral.ContainsKey(idBird))
        {
            birdsHasExtral[idBird].Remove(bird);
        }
    }

    public BirdBase GetBirdNoneExtral(int idBird)
    {
        var lstBird = birdsInLevel[idBird];
        for (int i = 0; i < lstBird.Count; i++)
        {
            if (!lstBird[i].isHasExtral)
            {
                return lstBird[i];
            }
        }

        return null;
    }

    private void OnDestroy()
    {
        if (match3SortCoroutine != null)
            StopCoroutine(match3SortCoroutine);
        if (waringTimePlayCorutine != null)
            StopCoroutine(waringTimePlayCorutine);
        if (spawnExtralCorutine != null)
            StopCoroutine(spawnExtralCorutine);
        if (reviveCorutine != null)
            StopCoroutine(reviveCorutine);
    }

    public void ResetPoolBird()
    {
        //foreach (var item in birdsInLevel)
        //{
        //    for (int i = 0; i < item.Value.Count; i++)
        //    {
        //        GameController.Instance.poolController.poolBirds.BackToPool(item.Value[i]);
        //    }
        //}
    }
}
