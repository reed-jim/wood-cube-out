using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using System;
using System.Collections;

public class LevelManager : SerializedMonoBehaviour
{
    public TripleBirdMGLogic controller;
    public LevelTripleBird level;
    public List<Color[,]> lsArray;
    public List<List<Tile>> lsGrass;
    public List<CountIdTile> countIdTiles;
    public List<GameObject> lsParent;
    public List<Tile> lsSort;
    public List<PosWait> lsPosWait;
    public Dictionary<int, int> dictCheck = new Dictionary<int, int>();
    public Queue<Tile> birdWaiting = new Queue<Tile>();
    public List<Tile> lsComplete;
    public List<GameObject> lsPost;
    public List<Tile> lsAllBird = new List<Tile>();
    public List<GameObject> lsEgg = new List<GameObject>();
    public List<IEnumerator> listAllCoroutine = new List<IEnumerator>();
    public DataSprites dataCells;
    public GameObject CellParent;
    public GameObject CellMother;
    public GameObject Clean;
    public List<Tile> listComplete = new List<Tile>();
    public List<Transform> listOutside = new List<Transform>();
    public GameObject Top;
    public GameObject PointParent;
    public int countTileSpawn;
    public float speedMove;
    public float speedMoveToWait;
    public float speedMoveOut;
    public AudioClip eggBroken;
    public AudioClip flySound;
    public AudioClip flyOutSound;
    public AudioClip earning;
    public bool canClickBird = true;
    public void InitState(TripleBirdMGLogic controller)
    {
        this.controller = controller;
        ClearData();
        CheckScale();
        for (int i = 0; i < level.lsArray.Count; i++)
        {
            lsGrass.Add(new List<Tile>() { });
        }
        countIdTiles = new List<CountIdTile>();
        foreach (CountIdTile item in level.countIdTiles)
        {
            CountIdTile cc = new CountIdTile(item);
            countIdTiles.Add(cc);
        }
        controller.numBird = level.lsIdLevel.Count;
        SetUp();
    }
    public void ClearData()
    {
        lsGrass.Clear();
        lsEgg.Clear();
        countTileSpawn = 0;
        lsArray = level.lsArray;
    }
    public void ClearMinigameTripleBird()
    {
        for (int i = 0; i < listAllCoroutine.Count; i++)
        {
            if (listAllCoroutine[i] != null)
            {
                StopCoroutine(listAllCoroutine[i]);
            }
        }
        listAllCoroutine.Clear();
        for (int i = lsAllBird.Count - 1; i >= 0; i--)
        {
            if (lsAllBird[i] == null)
            {
                continue;
            }
            else
            {
                Destroy(lsAllBird[i].gameObject);
            }
        }
        for (int i = 0; i < lsEgg.Count; i++)
        {
            if (lsEgg[i] != null)
                Destroy(lsEgg[i]);
        }
        for (int i = 0; i < lsPosWait.Count; i++)
        {
            lsPosWait[i].hasBird = false;
        }
        lsAllBird.Clear();
        lsSort.Clear();
        lsEgg.Clear();
        lsComplete.Clear();
        listComplete.Clear();
        birdWaiting.Clear();
        canClickBird = true;
    }
    public void ClearAllBirdInScene()
    {
        for (int i = 0; i < listAllCoroutine.Count; i++)
        {
            if (listAllCoroutine[i] != null)
            {
                StopCoroutine(listAllCoroutine[i]);
            }
        }
        listAllCoroutine.Clear();
        for (int i = lsAllBird.Count - 1; i >= 0; i--)
        {
            if (lsAllBird[i] == null)
            {
                continue;
            }
            else
            {
                if (lsAllBird[i].hasOutEgg && lsAllBird[i]._typeTile == TypeTile.CanNotClick)
                {
                    Transform outside = GetOutside();
                    float valueScaleX = Mathf.Abs(lsAllBird[i].anim.transform.localScale.x);
                    float timeMove = Vector2.Distance(lsAllBird[i].transform.position, outside.position) / speedMoveOut;
                    lsAllBird[i].SetFly();
                    lsAllBird[i].transform.SetParent(Top.transform);
                    if (outside.position.x > lsAllBird[i].transform.position.x)
                    {
                        lsAllBird[i].anim.transform.localScale = new Vector3(valueScaleX, lsAllBird[i].anim.transform.localScale.y, lsAllBird[i].anim.transform.localScale.z);
                    }
                    else
                    {
                        lsAllBird[i].anim.transform.localScale = new Vector3(-valueScaleX, lsAllBird[i].anim.transform.localScale.y, lsAllBird[i].anim.transform.localScale.z);
                    }
                    var tempBird = lsAllBird[i];
                    tempBird.transform.DOKill();
                    tempBird.transform.DOMove(outside.position, timeMove).OnComplete(() => { Destroy(tempBird.gameObject); });
                }
                else if (lsAllBird[i].hasOutEgg && lsAllBird[i]._typeTile == TypeTile.CanClick)
                {
                    Transform outside = GetOutside();
                    float valueScaleX = Mathf.Abs(lsAllBird[i].anim.transform.localScale.x);
                    float timeMove = Vector2.Distance(lsAllBird[i].transform.position, outside.position) / speedMoveOut;
                    lsAllBird[i].SetFly();
                    lsAllBird[i].shadow.SetActive(false);
                    lsAllBird[i].transform.SetParent(Clean.transform);
                    if (outside.position.x > lsAllBird[i].transform.position.x)
                    {
                        lsAllBird[i].anim.transform.localScale = new Vector3(valueScaleX, lsAllBird[i].anim.transform.localScale.y, lsAllBird[i].anim.transform.localScale.z);
                    }
                    else
                    {
                        lsAllBird[i].anim.transform.localScale = new Vector3(-valueScaleX, lsAllBird[i].anim.transform.localScale.y, lsAllBird[i].anim.transform.localScale.z);
                    }
                    var tempBird = lsAllBird[i];
                    tempBird.transform.DOKill();
                    tempBird.transform.DOMove(outside.position, timeMove).OnComplete(() => { Destroy(tempBird.gameObject); });
                }
                else
                {
                    Destroy(lsAllBird[i].gameObject);
                }
            }
        }
        for (int i = 0; i < lsEgg.Count; i++)
        {
            if (lsEgg[i] != null)
            {
                lsEgg[i].transform.DOScale(0, 0.5f).OnComplete(() => { Destroy(lsEgg[i]); });
            }
        }
        for (int i = 0; i < lsPosWait.Count; i++)
        {
            lsPosWait[i].hasBird = false;
        }
        lsAllBird.Clear();
        lsSort.Clear();
        lsEgg.Clear();
        lsComplete.Clear();
        listComplete.Clear();
        birdWaiting.Clear();
        canClickBird = true;
    }
    public void SetUp()
    {

        int index = 0;
        for (int i = 0; i < lsArray.Count; i++)
        {
            index = i;
            for (int j = 0; j < lsArray[index].GetLength(0); j++)
            {
                for (int k = 0; k < lsArray[index].GetLength(1); k++)
                {
                    if (!lsArray[index][j, k].Equals(new Color(0, 0, 0, 0)))
                    {
                        Tile tempTile = GetTileData();
                        var tile = Instantiate(tempTile, new Vector3((j + level.lsPos[index].x) * 160, (k + level.lsPos[index].y) * 160, 0), Quaternion.identity).GetComponent<Tile>();
                        var tempPos = tile.GetComponent<RectTransform>().anchoredPosition;
                        tile.transform.SetParent(lsParent[index].transform);
                        tile.transform.SetAsFirstSibling();
                        tile.transform.localScale = Vector3.one;
                        tile.GetComponent<RectTransform>().anchoredPosition = tempPos;
                        tile.pos = new Vector2Int(j, k);
                        if (!lsArray[index][j, k].Equals(new Color(0, 0, 0, 0)) && !lsArray[index][j, k].Equals(new Color(1, 1, 1, 0)))
                        {
                            tile.hasItem = true;
                        }
                        lsAllBird.Add(tile);
                        lsGrass[index].Add(tile); if (index == 0) continue;
                        #region Setlayer
                        for (int m = 0; m < lsGrass[index - 1].Count; m++)
                        {
                            if (lsGrass[index - 1][m].pos == new Vector2Int(j, k) || lsGrass[index - 1][m].pos == new Vector2Int(j, k - 1) || lsGrass[index - 1][m].pos == new Vector2Int(j - 1, k - 1) || lsGrass[index - 1][m].pos == new Vector2Int(j - 1, k))
                            {
                                tile.parents.Add(lsGrass[index - 1][m]);
                                lsGrass[index - 1][m].children.Add(tile);
                            }
                        }
                        #endregion
                    }
                }
            }
        }
        SetUpGrass();
    }
    public void SetUpGrass()
    {
        for (int i = 0; i < lsGrass.Count; i++)
        {
            for (int j = 0; j < lsGrass[i].Count; j++)
            {
                if (lsGrass[i][j].parents.Count == 0)
                {
                    lsGrass[i][j].Init(TypeTile.CanClick, controller);
                }
                else
                {
                    lsGrass[i][j].Init(TypeTile.CanNotClick, controller);
                }
            }
        }
    }
    public Tile GetTileData()
    {
        for (int i = 0; i < dataCells.Data.Count; i++)
        {
            if (dataCells.Data[i].ID == level.lsIdLevel[countTileSpawn])
            {
                countTileSpawn++;
                return dataCells.Data[i].prefabTile;
            }
        }
        return null;
    }
    public void AddListComplete(Tile param)
    {
        lsComplete.Add(param);
        CountTileId(param.id).count += 1;
        if (CountTileId(param.id).count == 3)
        {
            //Anhtusz GameController.Instance.musicManager.PlaySingle(earning, MusicManager.SourceAudio.Effect);
            int count = 0;
            for (int i = lsComplete.Count - 1; i >= 0; i--)
            {
                int index = i;
                Tile tmp = lsComplete[index];
                if (tmp.id == param.id && count < 3)
                {
                    count++;
                    listComplete.Add(tmp);
                }
            }
            Queue<Tile> birdFlyOut = new Queue<Tile>();
            for (int i = listComplete.Count - 1; i >= 0; i--)
            {
                int index = i;
                Tile tile = listComplete[index];
                tile.willDelete = true;
                birdFlyOut.Enqueue(tile);
            }
            for (int i = lsSort.Count - 1; i >= 0; i--)
            {
                if (lsSort[i].willDelete == true)
                {
                    lsSort.Remove(lsSort[i]);
                }
            }
            StartFlyOut(birdFlyOut);
            CountTileId(param.id).count = 0;
        }
    }
    public void StartFlyOut(Queue<Tile> birdFlyOut)
    {
        ThreeBirdsFly(birdFlyOut);
    }
    public void SortList(Tile param)
    {
        for (int i = 0; i < lsSort.Count; i++)
        {
            int index = i;
            if (lsSort.Count > lsPost.Count) return;
            var temp = lsPost[index].transform;
            float valueScaleX = Mathf.Abs(lsSort[index].anim.transform.localScale.x);
            if (lsSort[index].gameObject.transform.position != lsPost[index].transform.parent.position && lsSort[index] != param)
            {
                if (temp.position.x > lsSort[index].transform.position.x)
                {
                    lsSort[index].anim.transform.localScale = new Vector3(valueScaleX, lsSort[index].anim.transform.localScale.y, lsSort[index].anim.transform.localScale.z);
                }
                else
                {
                    lsSort[index].anim.transform.localScale = new Vector3(-valueScaleX, lsSort[index].anim.transform.localScale.y, lsSort[index].anim.transform.localScale.z);
                }
                lsSort[index].SetFly();
                float timeMove = Vector2.Distance(lsSort[index].transform.position, lsPost[index].transform.position) / speedMoveToWait;
                lsSort[index].gameObject.transform.DOKill();
                lsSort[index].gameObject.transform.DOMove(lsPost[index].transform.position, timeMove).OnComplete(() =>
                {
                    lsSort[index].SetGround();
                    var tempNest = temp.parent.position;
                    float timeGround = Vector2.Distance(lsSort[index].transform.position, tempNest) / speedMoveToWait;
                    lsSort[index].gameObject.transform.DOMove(tempNest, timeGround).OnComplete(() =>
                    {
                        if (lsSort[index].isFlying)
                        {
                            lsSort[index].isFlying = false;
                            lsSort[index].CheckThatTile();
                        }
                        lsSort[index].SetIdle();
                    }
                        );
                }
                );
            }
        }
    }
    public void SortList()
    {
        for (int i = 0; i < lsSort.Count; i++)
        {
            int index = i;
            if (lsSort.Count > lsPost.Count) return;
            var temp = lsPost[index].transform;
            float valueScaleX = Mathf.Abs(lsSort[index].anim.transform.localScale.x);
            if (lsSort[index].gameObject.transform.position != lsPost[index].transform.parent.position)
            {
                if (temp.position.x > lsSort[index].transform.position.x)
                {
                    lsSort[index].anim.transform.localScale = new Vector3(valueScaleX, lsSort[index].anim.transform.localScale.y, lsSort[index].anim.transform.localScale.z);
                }
                else
                {
                    lsSort[index].anim.transform.localScale = new Vector3(-valueScaleX, lsSort[index].anim.transform.localScale.y, lsSort[index].anim.transform.localScale.z);
                }
                lsSort[index].SetFly();
                float timeMove = Vector2.Distance(lsSort[index].transform.position, lsPost[index].transform.position) / speedMoveToWait;
                lsSort[index].gameObject.transform.DOKill();
                lsSort[index].gameObject.transform.DOMove(lsPost[index].transform.position, timeMove).OnComplete(() =>
                {
                    lsSort[index].SetGround();
                    var tempNest = temp.parent.position;
                    float timeGround = Vector2.Distance(lsSort[index].transform.position, tempNest) / speedMoveToWait;
                    lsSort[index].gameObject.transform.DOMove(tempNest, timeGround).OnComplete(() =>
                    {
                        if (lsSort[index].isFlying)
                        {
                            lsSort[index].isFlying = false;
                            lsSort[index].CheckThatTile();
                        }
                        lsSort[index].SetIdle();
                    }
                        );
                }
                );
            }
        }
    }

    public void RemoveList()
    {
        for (int i = lsGrass.Count - 1; i >= 0; i--)
        {
            for (int j = lsGrass[i].Count - 1; j >= 0; j--)
            {
                if (lsGrass[i][j] == null)
                {
                    lsGrass[i].Remove(lsGrass[i][j]);
                }
                else
                {
                    foreach (var item in lsGrass[i][j].children.ToArray())
                    {
                        if (item == null)
                        {
                            lsGrass[i][j].children.Remove(item);
                        }
                    }
                }
            }
            if (lsGrass[i].Count == 0)
            {
                lsGrass.Remove(lsGrass[i]);
            }
        }
    }
    public CountIdTile CountTileId(int id)
    {
        foreach (CountIdTile item in countIdTiles)
        {
            if (id == item.id)
            {
                return item;
            }
        }
        return null;
    }
    public void ThreeBirdsFly(Queue<Tile> paramQueue)
    {
        while (paramQueue.Count > 0)
        {
            var param = paramQueue.Dequeue();
            lsComplete.Remove(param);
            listComplete.Remove(param);
            controller.numBird--;
            param.transform.SetParent(PointParent.transform);
            if (param.point != null)
            {
                param.point.transform.SetParent(PointParent.transform);
                controller.numbPoint++;
                controller.numbPointToReset++;
                param.point.HandleMovePointToPost(controller.iconFeather.transform.position, () =>
                {
                    if (controller.numbPoint > 0)
                    {
                        controller.tvFeather.text = "" + controller.numbPoint;
                    }
                    Destroy(param.point.gameObject);
                }
                );
            }
            param.SetFly();
            Transform outside = GetOutside();
            float valueScaleX = Mathf.Abs(param.anim.transform.localScale.x);
            if (outside.position.x > this.transform.position.x)
            {
                param.anim.transform.localScale = new Vector3(valueScaleX, param.anim.transform.localScale.y, param.anim.transform.localScale.z);
            }
            else
            {
                param.anim.transform.localScale = new Vector3(-valueScaleX, param.anim.transform.localScale.y, param.anim.transform.localScale.z);
            }
            //Anhtusz GameController.Instance.musicManager.PlayOneShot(NameMusic.FlyOut, flyOutSound, param.audioSourceFly);
            float timeMove = Vector2.Distance(param.transform.position, outside.position) / speedMove;
            param.transform.DOKill();
            param.transform.DOMove(outside.position, timeMove).OnComplete(() => { Destroy(param.gameObject); });
        }
        SortList();
        controller.CheckWinLose();
        CheckQuere();
    }
    public void CheckQuere()
    {
        if (birdWaiting.Count > 0)
        {
            var tempBird = birdWaiting.Dequeue();
            tempBird.MoveToPos();
            if (lsSort.Count < 6)
            {
                CheckQuere();
            }
        }
    }
    public void CheckScale()
    {
        CellParent.transform.localScale = new Vector3(level.ScaleObjtect, level.ScaleObjtect, 0);
    }
    public Transform GetOutside()
    {
        return listOutside[UnityEngine.Random.Range(0, listOutside.Count - 1)];
    }
    public bool CheckCanClear()
    {
        for (int i = 0; i < lsSort.Count; i++)
        {
            int tempValue;
            if (dictCheck.TryGetValue(lsSort[i].id, out tempValue))
            {
                dictCheck.SetOrAdd(lsSort[i].id, tempValue + 1);
            }
            else
            {
                dictCheck.Add(lsSort[i].id, 1);
            }
        }
        foreach (var item in dictCheck)
        {
            if (item.Value >= 3)
            {
                dictCheck.Clear();
                return true;
            }
        }
        dictCheck.Clear();
        return false;
    }
}
[Serializable]
public class CountIdTile
{
    public int id;
    public int count;
    public CountIdTile(CountIdTile tile)
    {
        this.id = tile.id;
        this.count = tile.count;
    }
    public CountIdTile(int ID)
    {
        this.id = ID;
    }
}
