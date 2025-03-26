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
