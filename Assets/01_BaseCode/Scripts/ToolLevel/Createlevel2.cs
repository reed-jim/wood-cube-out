using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Createlevel2 : MonoBehaviour
{
    public static string jsonLevel;

    public InputField jsonLevelInp;
    public InputField jsonMaxLevelInp;

    public struct BirdSlot
    {
        public int idBird;
        public int idSlot;
    }    

    public int maxLevel;
    public long binaryTimeTest;
    private void Start()
    {
        
    }

    public void Go()
    {
        jsonLevel = jsonLevelInp.text;
        GameController.Instance.LoadScene(SceneName.GAME_PLAY);
    }

    public void TestAllLevel()
    {
        //  int maxLv = System.Int32.Parse(jsonMaxLevelInp.text);
        for (int i = 1; i <= maxLevel; i++)
        {
            //LongTH
            string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
            //string pathLevel = StringHelper.PATH_CONFIG_LEVEL_BIRD1;
            //--------------------------------------------------------------------------
            TextAsset lvJson = Resources.Load<TextAsset>(string.Format(pathLevel, i));
            if (lvJson != null)
            {
                Debug.LogError("============ Check Level: " + i);
                var levelConfig = JsonUtility.FromJson<LevelConfig>(lvJson.ToString());

                CheckLevelHandle(levelConfig, i);

            }
        }
    }

    private void CheckLevelHandle(LevelConfig levelConfig, int level)
    {
        //Check extral có ở special point hay k?
        //Debug.Log("============ Check extral có ở special point hay k ===================");
        Dictionary<BirdSlot, ExtralPlayType> birdInLock = new Dictionary<BirdSlot, ExtralPlayType>();
        Dictionary<int, ExtralPlayType> idSlot = new Dictionary<int, ExtralPlayType>();

        for (int i = 0; i < levelConfig.extralsConfig.Count; i++)
        {
            if (levelConfig.extralsConfig[i].type == ExtralPlayType.Boom)
            {
                var idCheckPoint = levelConfig.extralsConfig[i].positionData.idCheckPoint;
                var idBird = levelConfig.extralsConfig[i].positionData.idBird;

                bool isHasInSpeciall = false;
                for (int z = 0; z < levelConfig.specialPoints.Count; z++)
                {
                    if (levelConfig.specialPoints[z].idBird == idBird && levelConfig.specialPoints[z].idSlot == idCheckPoint)
                    {
                        isHasInSpeciall = true;
                        break;
                    }
                }

                if (isHasInSpeciall)
                {
                    //break;
                }
                else
                {
                    Debug.Log("Mode Boom: Chim cầm boom không có trong special point " + " Level " + level);
                }
            }
            else if (levelConfig.extralsConfig[i].type == ExtralPlayType.LockBird)
            {
                var idCheckPoint = levelConfig.extralsConfig[i].positionData.idCheckPoint;
                var idBird = levelConfig.extralsConfig[i].positionData.idBird;

                BirdSlot bs = new BirdSlot();
                bs.idBird = idBird;
                bs.idSlot = idCheckPoint;

                if (birdInLock.ContainsKey(bs))
                {
                    Debug.Log("Mode Lồng: Chim trong lồng đang nằm trong mode " + birdInLock[bs].ToString() + " Level " + level);
                }
                else
                {
                    birdInLock.Add(bs, ExtralPlayType.LockBird);
                }

                if (idSlot.ContainsKey(idCheckPoint))
                {
                    Debug.Log("Mode Lồng: chim trong lồng bị trùng id check point vs mode " + idSlot[idCheckPoint].ToString() + " Level " + level);
                }
                else
                {
                    idSlot.Add(idCheckPoint, ExtralPlayType.LockBird);
                }

                bool isHasInSpeciall = false;
                for (int z = 0; z < levelConfig.specialPoints.Count; z++)
                {
                    if (levelConfig.specialPoints[z].idBird == idBird && levelConfig.specialPoints[z].idSlot == idCheckPoint)
                    {
                        isHasInSpeciall = true;
                        break;
                    }
                }

                if (isHasInSpeciall)
                {
                    //break;
                }
                else
                {
                    Debug.Log("Mode Lồng: Chim trong lồng không có trong special point " + " Level " + level);
                }

                bool _isHasInSpeciall = false;
                for (int t = 0; t < levelConfig.extralsConfig[i].extralValues.Length - 1; t += 2)
                {
                    int idBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t + 1];
                    int idSlotBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t];

                    for (int z = 0; z < levelConfig.specialPoints.Count; z++)
                    {
                        if (levelConfig.specialPoints[z].idBird == idBirdHasKey && levelConfig.specialPoints[z].idSlot == idSlotBirdHasKey)
                        {
                            _isHasInSpeciall = true;
                            break;
                        }
                    }
                }

                if (_isHasInSpeciall)
                {
                    //break;
                }
                else
                {
                    Debug.Log("Mode Lồng: Chim cầm chìa khoá không có trong special point " + " Level " + level);
                }

                for (int t = 0; t < levelConfig.extralsConfig[i].extralValues.Length - 1; t += 2)
                {
                    int idBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t + 1];
                    int idSlotBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t];

                    if (idBirdHasKey == levelConfig.extralsConfig[i].positionData.idBird && idSlotBirdHasKey == levelConfig.extralsConfig[i].positionData.idCheckPoint)
                    {
                        Debug.Log("Mode Lồng: Chim cầm chìa khoá nằm trong lồng " + " Level " + level);
                    }
                }
            }
            else if (levelConfig.extralsConfig[i].type == ExtralPlayType.EggLock)
            {
                var idCheckPoint = levelConfig.extralsConfig[i].positionData.idCheckPoint;
                var idBird = levelConfig.extralsConfig[i].positionData.idBird;

                BirdSlot bs = new BirdSlot();
                bs.idBird = idBird;
                bs.idSlot = idCheckPoint;

                if (birdInLock.ContainsKey(bs))
                {
                    Debug.Log("Mode Trứng: Chim trong trứng đang nằm trong mode " + birdInLock[bs].ToString() + " Level " + level);
                }
                else
                {
                    birdInLock.Add(bs, ExtralPlayType.EggLock);
                }

                if (idSlot.ContainsKey(idCheckPoint))
                {
                    Debug.Log("Mode Trứng: chim trong lồng bị trùng id check point vs mode " + idSlot[idCheckPoint].ToString() + " Level " + level);
                }
                else
                {
                    idSlot.Add(idCheckPoint, ExtralPlayType.EggLock);
                }


                bool isHasInSpeciall = false;
                for (int z = 0; z < levelConfig.specialPoints.Count; z++)
                {
                    if (levelConfig.specialPoints[z].idBird == idBird && levelConfig.specialPoints[z].idSlot == idCheckPoint)
                    {
                        isHasInSpeciall = true;
                        break;
                    }
                }

                if (isHasInSpeciall)
                {
                    //break;
                }
                else
                {
                    Debug.Log("Mode Trứng: Chim trong trứng không có trong special point " + " Level " + level);
                }

                bool _isHasInSpeciall = false;
                for (int t = 0; t < levelConfig.extralsConfig[i].extralValues.Length - 2; t += 3)
                {
                    int idBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t + 1];
                    int idSlotBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t];

                    for (int z = 0; z < levelConfig.specialPoints.Count; z++)
                    {
                        if (levelConfig.specialPoints[z].idBird == idBirdHasKey && levelConfig.specialPoints[z].idSlot == idSlotBirdHasKey)
                        {
                            _isHasInSpeciall = true;
                            break;
                        }
                    }
                }

                if (_isHasInSpeciall)
                {
                    //break;
                }
                else
                {
                    Debug.Log("Mode Trứng: Chim cầm búa khoá không có trong special point " + " Level " + level);
                }

                for (int t = 0; t < levelConfig.extralsConfig[i].extralValues.Length - 2; t += 3)
                {
                    int idBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t + 1];
                    int idSlotBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t];

                    for (int z = 0; z < levelConfig.specialPoints.Count; z++)
                    {
                        if (idBirdHasKey == levelConfig.extralsConfig[i].positionData.idBird && idSlotBirdHasKey == levelConfig.extralsConfig[i].positionData.idCheckPoint)
                        {
                            Debug.Log("Mode Trứng: Chim cầm búa nằm trong trứng " + " Level " + level);
                        }
                    }
                }
            }
            else if (levelConfig.extralsConfig[i].type == ExtralPlayType.SleepBird)
            {
                var idCheckPoint = levelConfig.extralsConfig[i].positionData.idCheckPoint;
                var idBird = levelConfig.extralsConfig[i].positionData.idBird;

                bool isHasInSpeciall = false;
                for (int z = 0; z < levelConfig.specialPoints.Count; z++)
                {
                    if (levelConfig.specialPoints[z].idBird == idBird && levelConfig.specialPoints[z].idSlot == idCheckPoint)
                    {
                        isHasInSpeciall = true;
                        break;
                    }
                }

                if (isHasInSpeciall)
                {
                    // break;
                }
                else
                {
                    Debug.Log("Mode Sleep: Chim cầm đồng hồ không có trong special point " + " Level " + level);
                }


                for (int t = 0; t < levelConfig.extralsConfig[i].extralValues.Length - 1; t += 2)
                {
                    int idBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t + 1];
                    int idSlotBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t];
                    bool _isHasInSpeciall = false;
                    for (int z = 0; z < levelConfig.specialPoints.Count; z++)
                    {
                        if (levelConfig.specialPoints[z].idBird == idBirdHasKey && levelConfig.specialPoints[z].idSlot == idSlotBirdHasKey)
                        {
                            _isHasInSpeciall = true;
                            break;
                        }
                    }

                    if (!_isHasInSpeciall)
                    {
                        Debug.Log("Mode Sleep: Chim ngủ không có trong special point " + " ID chim " + idBirdHasKey + " ID Check Point " + idSlotBirdHasKey + " Level " + level);
                    }
                }

                for (int t = 0; t < levelConfig.extralsConfig[i].extralValues.Length - 1; t += 2)
                {
                    int idBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t + 1];
                    int idSlotBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t];


                    if (idBirdHasKey == levelConfig.extralsConfig[i].positionData.idBird && idSlotBirdHasKey == levelConfig.extralsConfig[i].positionData.idCheckPoint)
                    {
                        Debug.Log("Mode Ngủ: Chim ngủ cầm đồng hồ " + " Level " + level);
                    }
                }
            }
            else if (levelConfig.extralsConfig[i].type == ExtralPlayType.Rope)
            {
                Debug.Log("Mode ROPE" + " Level " + level);

                var idCheckPoint = levelConfig.extralsConfig[i].positionData.idCheckPoint;
                var idBird = levelConfig.extralsConfig[i].positionData.idBird;

                bool isHasInSpeciall = false;
                for (int z = 0; z < levelConfig.specialPoints.Count; z++)
                {
                    if (levelConfig.specialPoints[z].idBird == idBird && levelConfig.specialPoints[z].idSlot == idCheckPoint)
                    {
                        isHasInSpeciall = true;
                        break;
                    }
                }

                if (isHasInSpeciall)
                {
                    //break;
                }
                else
                {
                    Debug.Log("Mode Rope: Chim đầu nguồn không có trong special point " + " Level " + level);
                }


                Dictionary<string, int> _birds = new Dictionary<string, int>();
                for (int t = 0; t < levelConfig.extralsConfig[i].extralValues.Length - 1; t += 2)
                {
                    int idBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t + 1];
                    int idSlotBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t];
                    string birdSlotCheck = idBirdHasKey + "_" + idSlotBirdHasKey;
                    bool _isHasInSpeciall = false;
                    for (int z = 0; z < levelConfig.specialPoints.Count; z++)
                    {
                        if (levelConfig.specialPoints[z].idBird == idBirdHasKey && levelConfig.specialPoints[z].idSlot == idSlotBirdHasKey)
                        {
                            _isHasInSpeciall = true;
                            break;
                        }
                    }

                    if(_birds.ContainsKey(birdSlotCheck))
                    {
                        Debug.Log("Mode Rope: Các chim buộc giống nhau " + " ID chim " + idBirdHasKey + " ID Check Point " + idSlotBirdHasKey + " Level " + level);
                    }   
                    else
                    {
                        _birds.Add(birdSlotCheck, 0);
                    }

                    if (!_isHasInSpeciall)
                    {
                        Debug.Log("Mode Rope: Chim buộc không có trong special point " + " ID chim " + idBirdHasKey + " ID Check Point " + idSlotBirdHasKey + " Level " + level);
                    }

                    if(idBirdHasKey == idBird && idSlotBirdHasKey == idCheckPoint)
                    {
                        Debug.Log("Mode Rope: Chim buộc giống chim đầu nguồn" + " ID chim " + idBirdHasKey + " ID Check Point " + idSlotBirdHasKey + " Level " + level);
                    }
                }
            }
            else if (levelConfig.extralsConfig[i].type == ExtralPlayType.LockStand)
            {
                var idCheckPoint = levelConfig.extralsConfig[i].positionData.idCheckPoint;
                var idBird = levelConfig.extralsConfig[i].positionData.idBird;

                BirdSlot bs = new BirdSlot();
                bs.idBird = idBird;
                bs.idSlot = idCheckPoint;

                if (birdInLock.ContainsKey(bs))
                {
                    Debug.Log("Mode Lồng Kép: Chim trong lồng đang nằm trong mode " + birdInLock[bs].ToString() + " Level " + level);
                }
                else
                {
                    birdInLock.Add(bs, ExtralPlayType.LockStand);
                }

                if (idSlot.ContainsKey(idCheckPoint))
                {
                    Debug.Log("Mode Lồng kép: chim trong lồng bị trùng id check point vs mode " + idSlot[idCheckPoint].ToString() + " Level " + level);
                }
                else
                {
                    idSlot.Add(idCheckPoint, ExtralPlayType.LockStand);
                }


                bool isHasInSpeciall = false;
                for (int z = 0; z < levelConfig.specialPoints.Count; z++)
                {
                    if (levelConfig.specialPoints[z].idBird == idBird && levelConfig.specialPoints[z].idSlot == idCheckPoint)
                    {
                        isHasInSpeciall = true;
                        break;
                    }
                }

                if (isHasInSpeciall)
                {
                    //break;
                }
                else
                {
                    Debug.Log("Mode Lồng Kép: Chim trong lồng không có trong special point " + " Level " + level);
                }

                Dictionary<string, int> _birds = new Dictionary<string, int>();
                for (int t = 1; t < levelConfig.extralsConfig[i].extralValues.Length - 1; t += 2)
                {
                    int idBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t + 1];
                    int idSlotBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t];
                    string birdSlotCheck = idBirdHasKey + "_" + idSlotBirdHasKey;
                    bool _isHasInSpeciall = false;
                    for (int z = 0; z < levelConfig.specialPoints.Count; z++)
                    {
                        if (levelConfig.specialPoints[z].idBird == idBirdHasKey && levelConfig.specialPoints[z].idSlot == idSlotBirdHasKey)
                        {
                            _isHasInSpeciall = true;
                            break;
                        }
                    }

                    if (!_isHasInSpeciall)
                    {
                        Debug.Log("Mode Lồng kép: Chim cầm chìa không có trong special point " + " ID chim " + idBirdHasKey + " ID Check Point " + idSlotBirdHasKey + " Level " + level);
                    }

                    if (_birds.ContainsKey(birdSlotCheck))
                    {
                        Debug.Log("Mode Lồng kép: Các chim cầm chìa giống nhau " + " ID chim " + idBirdHasKey + " ID Check Point " + idSlotBirdHasKey + " Level " + level);
                    }
                    else
                    {
                        _birds.Add(birdSlotCheck, 0);
                    }
                }

                for (int t = 1; t < levelConfig.extralsConfig[i].extralValues.Length - 1; t += 2)
                {
                    int idBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t + 1];
                    int idSlotBirdHasKey = (int)levelConfig.extralsConfig[i].extralValues[t];

                    if (idBirdHasKey == levelConfig.extralsConfig[i].positionData.idBird && idSlotBirdHasKey == levelConfig.extralsConfig[i].positionData.idCheckPoint)
                    {
                        Debug.Log("Mode Lồng kép: Chim cầm chìa khoá nằm trong lồng " + " Level " + level);
                    }
                }
            }
        }

        // Debug.Log("============ Cộng tổng special point và all có chia hết cho 3 hay k? ===================");
        //Cộng tổng special point và all có chia hết cho 3 hay k?
        Dictionary<int, int> birdsInGame = new Dictionary<int, int>();
        for (int i = 0; i < levelConfig.specialPoints.Count; i++)
        {
            var idBird = levelConfig.specialPoints[i].idBird;
            if (birdsInGame.ContainsKey(idBird))
            {
                birdsInGame[idBird] += 1;
            }
            else
            {
                birdsInGame.Add(idBird, 1);
            }

        }

        for (int i = 0; i < levelConfig.birdLevelData.Count; i++)
        {
            var idBird = levelConfig.birdLevelData[i].idBird;
            if (birdsInGame.ContainsKey(idBird))
            {
                birdsInGame[idBird] += levelConfig.birdLevelData[i].total;
            }
            else
            {
                birdsInGame.Add(idBird, levelConfig.birdLevelData[i].total);
            }
        }

        int totalBird = 0;
        foreach (var item in birdsInGame)
        {
            if (item.Value % 3 != 0)
            {
                Debug.Log("Chim ID " + item.Key + " Ko chia hết cho 3" + " Level " + level);
            }
            totalBird += item.Value;
        }
        if (totalBird > 150)
            Debug.Log("Số chim vượt quá 150" + " Level " + level);


        //  Debug.Log("============ Check số lượng chim trong requirement có đủ hay k? ===================");
        //Check số lượng chim trong requirement có đủ hay k?
        for (int i = 0; i < levelConfig.requirements.Count; i++)
        {
            var idR = levelConfig.requirements[i].idBird;
            var numR = levelConfig.requirements[i].numRequire;

            if (birdsInGame.ContainsKey(idR))
            {
                if (birdsInGame[idR] != numR)
                    Debug.Log("Số lượng Bird " + idR + " (" + birdsInGame[idR] + ")" + " Không giống số lượng của Yêu cầu" + " (" + numR + ")" + " Level " + level);
            }
            else
            {
                Debug.Log("Bird Yêu cầu không có trong level. ID Bird lỗi: " + idR + " Level " + level);
            }
        }

        float percentCover = levelConfig.percentRequirementCover;
        int numBirdInCover = 0;
        Dictionary<int, int> requireBirds = new Dictionary<int, int>();
        for (int i = 0; i < levelConfig.requirements.Count; i++)
        {
            int index = i;
            var require = levelConfig.requirements[index];
            var idBird = require.idBird;
            var numRequire = require.numRequire;

            int numInCover = Mathf.RoundToInt(percentCover * numRequire);
            if (numInCover > numRequire)
                numInCover = numRequire;

            numBirdInCover += (numInCover * 2);
            requireBirds.Add(idBird, numRequire);
        }

        int totalBirdNoneRequire = 0;
        for (int i = 0; i < levelConfig.birdLevelData.Count; i++)
        {
            for (int j = 0; j < levelConfig.birdLevelData[i].total; j++)
            {
                if (requireBirds.ContainsKey(levelConfig.birdLevelData[i].idBird))
                    continue;

                totalBirdNoneRequire++;
            }
        }

        if (totalBirdNoneRequire < numBirdInCover)
            Debug.Log("Tổng số Bird rác ko đủ số lượng để che Bird yêu cầu" + " Level " + level);
    }

    public void CheckLevel()
    {
        var lv = jsonLevelInp.text;
        var levelConfig = JsonUtility.FromJson<LevelConfig>(lv);


        CheckLevelHandle(levelConfig, 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestAllLevel();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Application.runInBackground = true;
            StartCoroutine(SpawnLevelTest());
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("Time " +System.DateTime.FromBinary(binaryTimeTest));
        }
    }

    public int startLevelTest;
    public int endLevelTest;
    public GameObject gamePlay;
    public GameObject gamePlayPrefab;
    public IEnumerator SpawnLevelTest()
    {
        Time.timeScale = 5;

        for (int i = startLevelTest; i < endLevelTest + 1; i++)
        {
            SkillUserController.CurrentSkill = 1;
            for (int f = 0; f < 10; f++)
            {
                int indexLevel = i;
                GamePlayController.Instance = null;
                UseProfile.CurrentLevel = indexLevel;
                GameController.Instance.useProfile.CurrentLevelPlay = indexLevel;
                if (gamePlay != null)
                    Destroy(gamePlay);

                int total = 0;
                gamePlay = Instantiate(gamePlayPrefab, null);

                yield return new WaitUntil(() => GamePlayController.Instance != null);
                yield return new WaitUntil(() => GamePlayController.Instance.state == StateGame.Playing);
                yield return new WaitForSeconds(0.5f);
                foreach (var item in GamePlayController.Instance.level.birdsInLevel)
                {
                    for (int t = 0; t < item.Value.Count; t++)
                    {
                        if (item.Value[t].IsOutScreen())
                        {
                            //Debug.LogError("Level " + indexLevel + " Bird " + item.Value[t].id + " Nằm ngoài Check Point cho phép");
                        }
                    }
                    total += item.Value.Count;
                    SkillUserController.CurrentSkill++;
                    if(item.Value.Count % 3 != 0)
                    {
                        Debug.LogError("Level " + indexLevel + " Chim ID: " + item.Key + " Không chia hết cho 3 .Skill " + (SkillUserController.CurrentSkill - 1));
                    }    
                }
                if (total > 150)
                {
                    Debug.LogError("Level " + indexLevel + " Vượt quá 150 chim. Skill " + (SkillUserController.CurrentSkill - 1));
                }

                //yield return new WaitUntil(() => GamePlayController.Instance.extralPlayController.extralInGame != null &&
                //  GamePlayController.Instance.extralPlayController.extralInGame.Count > 0
                //  && GamePlayController.Instance.extralPlayController.IsExtralsReady() == true);
                //Debug.Log("=========Level " + indexLevel);
                yield return new WaitForSeconds(1);
            }
        }
    }
}
