using Crystal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelTestM
{
    public int duration;
    public List<BroadM> board;
}

[System.Serializable]
public class BroadM
{
    public string id;
    public int count;
}

public class Createlevel : MonoBehaviour
{
    public GameObject gamePlayScene;
    public int level;
    public bool isTestMap;
    public bool isTestAnim;
    public long timeTest;

    public List<Transform> posTestAnim;

    public void Start()
    {
        Debug.Log("Time " + System.DateTime.FromBinary(timeTest));
        Debug.Log("Clam " + Mathf.Clamp(6, 0, 5));
        //AnalyticLevel();

        UseProfile.CurrentLevel = level;
        GameController.Instance.useProfile.CurrentLevelPlay = level;
        if (isTestAnim)
        {
            //UseProfile.CurrentLevel = 10000;
            GameController.Instance.useProfile.CurrentLevelPlay = 10000;
            StartCoroutine(Helper.StartAction(() =>
            {
                GamePlayController.Instance.level.map.flockController.isLockFlock = true;
                GamePlayController.Instance.level.map.gameObject.SetActive(false);
                Camera.main.orthographicSize = 4;
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    int index = i;
                    birds[i].isLockMove = true;
                    birds[i].fsm.lockFSM = true;
                   // birds[i].collider2D.enabled = false;
                    birds[i].gameObject.transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);
                    StartCoroutine(Helper.StartAction(() =>
                    {
                        birds[index].transform.position = new Vector3(posTestAnim[index].transform.position.x, posTestAnim[index].transform.position.y, 0);
                    }, 5f));
                }

                var safeArea = GameObject.FindObjectOfType<SafeArea>();
                safeArea.gameObject.SetActive(false);

            }, 1.5f));
        }

        var gS = Instantiate(gamePlayScene, null);

        if (isTestMap)
        {
            GamePlayController.Instance.level.map.flockController.isLockFlock = true;

            var birds = GameObject.FindObjectsOfType<BirdBase>();
            for (int i = 0; i < birds.Length; i++)
            {
                birds[i].isLockMove = true;
                birds[i].gameObject.transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);
            }


        }

    }

    public void AnalyticLevel()
    {
        Dictionary<string, int> itemsDict = new Dictionary<string, int>();
        int numElemt = 0;
        int numTypeInlevel = 0;

        for (int i = 1; i < 100; i++)
        {
            string pathLevel = "LevelsM/Level000{0}";
            if (i < 10)
                pathLevel = "LevelsM/Level000{0}";
            else if (i >= 10 && i <= 99)
            {
                pathLevel = "LevelsM/Level00{0}";
            }
            else if (i >= 100 && i <= 999)
            {
                pathLevel = "LevelsM/Level0{0}";
            }
            else
            {
                pathLevel = "LevelsM/Level{0}";
            }

            TextAsset lvJson = Resources.Load<TextAsset>(string.Format(pathLevel, i));
            if (lvJson != null)
            {
                var levelConfig = JsonUtility.FromJson<LevelTestM>(lvJson.ToString());

                int totalElemtInlevel = 0;
                for (int j = 0; j < levelConfig.board.Count; j++)
                {
                    if (itemsDict.ContainsKey(levelConfig.board[j].id))
                    {
                        itemsDict[levelConfig.board[j].id]++;
                    }
                    else
                    {
                        itemsDict.Add(levelConfig.board[j].id, 1);
                    }
                    totalElemtInlevel += levelConfig.board[j].count;

                }

                numTypeInlevel += levelConfig.board.Count;
                numElemt += totalElemtInlevel;
            }
        }

        Debug.Log("Tổng số loại: " + itemsDict.Count);
        Debug.Log("TB số lượng element / level: " + numElemt / 500);
        Debug.Log("TB số loại / level: " + numTypeInlevel / 500);
    }

    private void Update()
    {
        if (isTestAnim)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    birds[i].PlayAnim(BirdBase.IDLE, true);
                }
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    birds[i].PlayAnim(BirdBase.IDLE_2, true);
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    birds[i].PlayAnim(BirdBase.SELLECT, true);
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    birds[i].PlayAnim(BirdBase.FLY, true);
                }
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    birds[i].PlayAnim(BirdBase.GLIDE, true);
                }
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    int index = i;
                    birds[i].PlayAnim(BirdBase.GROUNDING, false, () => { birds[index].PlayAnim(BirdBase.IDLE, true); });
                }
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    int index = i;
                    birds[i].PlayAnim(BirdBase.GROUNDING, false, () => { birds[index].PlayAnim(BirdBase.IDLE_2, true); });
                }
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    int index = i;
                    birds[i].PlayAnim(BirdBase.PRE_SLEEP, false, () => { birds[index].PlayAnim(BirdBase.SLEEP, true); });
                }
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    int index = i;
                    birds[i].PlayAnim(BirdBase.WAKE_UP, false, () => { birds[index].PlayAnim(BirdBase.IDLE, true); });
                }
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    int index = i;
                    birds[i].PlayAnim(BirdBase.WAKE_UP, false, () => { birds[index].PlayAnim(BirdBase.IDLE_2, true); });
                }
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    birds[i].PlayAnim(BirdBase.DIE, false);
                }
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    birds[i].isLockMove = false;
                    birds[i].HightLight();
                }
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    birds[i].isLockMove = true;
                    birds[i].HightLight();
                }
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                var birds = GameObject.FindObjectsOfType<BirdBase>();
                for (int i = 0; i < birds.Length; i++)
                {
                    birds[i].isLockMove = true;
                    birds[i].DisableOutLine();
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            AnalyticLevel();
        }    
    }

}
