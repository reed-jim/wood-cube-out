using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;
using Spine;
using System.Collections;
public enum TypeTile
{
    CanClick,
    CanNotClick,
}
public class Tile : MonoBehaviour
{
    #region config
    public const string IDLE = "idle";
    public const string FLY = "fly";
    public const string GROUNDING = "grounding";
    public const string IDLE_EGG = "2-idle";
    public const string BREAK = "3-break";
    private TripleBirdMGLogic controller;
    public TypeTile typeTile;
    public TypeTile _typeTile
    {
        get
        {
            return typeTile;
        }
        set
        {
            typeTile = value;
            switch (typeTile)
            {
                case TypeTile.CanClick:
                    hasSpawn = false;
                    hasOutEgg = true;
                    anim.gameObject.SetActive(true);
                    anim.SetAnimation(IDLE, true);
                    var valueScale = anim.transform.localScale.x;
                    anim.transform.DOScale(valueScale + 0.1f, 0.5f).SetEase(Ease.InOutQuad).SetDelay(0.5f);
                    egg.transform.SetParent(controller.levelManager.CellParent.transform);
                    controller.levelManager.lsEgg.Add(egg.gameObject);
                    GameController.Instance.musicManager.PlayOneShot(controller.levelManager.eggBroken, audioSourceEgg);
                    egg.SetAnimation(BREAK, false, () =>
                    {
                        Destroy(egg.gameObject);
                        ShowPoint();
                    });

                    break;
                case TypeTile.CanNotClick:
                    if (hasSpawn)
                    {
                        hasSpawn = false;
                        anim.gameObject.SetActive(false);
                        egg.gameObject.SetActive(true);
                        egg.AnimationState.ClearTracks();
                    }
                    break;
            }
        }
    }
    public int id;
    public Vector2Int pos;
    public PosWait posWait;
    public List<Tile> parents;
    public List<Tile> children;
    public bool willDelete;
    public bool hasItem = false;
    public bool hasOutEgg = false;
    private bool hasSpawn = true;
    public bool isFlying = false;
    public Button btn;
    public GameObject shadow;
    public SkeletonGraphic egg;
    public SkeletonGraphic anim;
    public AudioClip birdSound;
    public AudioSource audioSourceFly;
    public AudioSource audioSourceBird;
    public AudioSource audioSourceEgg;
    [HideInInspector] public PointTripleBirdMiniGame point;
    private int SortTitle
    {
        get
        {
            bool wasFind = false;
            int temp = 0;
            var lvMng = controller.levelManager;
            for (int i = lvMng.lsSort.Count - 1; i >= 0; i--)
            {
                if (lvMng.lsSort[i].id == this.id)
                {
                    wasFind = true;
                    lvMng.lsSort.Insert(i + 1, this);
                    lvMng.RemoveList();
                    lvMng.SortList(this);
                    temp = i + 1;
                    break;
                }
            }
            if (!wasFind)
            {
                lvMng.lsSort.Add(this);
                lvMng.RemoveList();
                lvMng.SortList(this);
                return lvMng.lsSort.Count - 1;
            }
            else
            {
                return temp;
            }
        }
    }
    #endregion
    #region
    public void Init(TypeTile typeTile, TripleBirdMGLogic controller)
    {
        this.controller = controller;
        shadow.SetActive(true);
        btn.onClick.AddListener(HandleOnClick);
        this._typeTile = typeTile;
        SpawnItemInside();
    }
    public void SpawnItemInside()
    {
        if (hasItem)
        {
            var tempPoint = Instantiate(controller.gameAssets.point, this.transform);
            point = tempPoint;
            tempPoint.transform.SetAsLastSibling();
            tempPoint.GetComponent<Image>().SetNativeSize();
            tempPoint.GetComponent<RectTransform>().localScale = Vector3.zero;
            tempPoint.GetComponent<RectTransform>().anchoredPosition = new Vector3(40, -20, 0);
        }
    }
    public void ShowPoint()
    {
        if (point != null)
        {
            point.HandleScaleInOut();
        }
    }
    public void PlaySound()
    {
        GameController.Instance.musicManager.PlayOneShot(controller.levelManager.flySound, audioSourceFly);
        GameController.Instance.musicManager.PlayOneShot(birdSound, audioSourceBird);
    }
    public void SetFly()
    {
        anim.SetAnimation(FLY, true);
    }
    public void SetIdle()
    {
        anim.SetAnimation(IDLE, true);
    }
    public void SetGround()
    {
        anim.SetAnimation(GROUNDING, false);
    }
    public void CheckAround(Tile tile)
    {
        for (int i = 0; i < tile.children.Count; i++)
        {
            int index = i;
            tile.children[index].parents.Remove(tile);
            if (tile.children[index].parents.Count == 0)
            {
                tile.children[index]._typeTile = TypeTile.CanClick;
            }
        }
    }
    public void StopFlySound()
    {
        audioSourceFly.DOFade(0, 0.5f);
    }
    #endregion
    private void HandleOnClick()
    {
        var lvMng = controller.levelManager;

        if (_typeTile == TypeTile.CanClick & controller.levelManager.canClickBird)
        {
            if (controller.isLose)
            {
                controller.gameAssets.SpawnXObject(anim.transform.position);
                return;
            }
            if (controller.levelManager.lsSort.Count < 6 && controller.levelManager.birdWaiting.Count == 0)
            {
                this._typeTile = TypeTile.CanNotClick;
                CheckAround(this);
                PlaySound();
                anim.SetAnimation(FLY, true);
                shadow.gameObject.SetActive(false);
                this.transform.SetParent(lvMng.CellMother.transform);
                var temp = lvMng.lsPost[SortTitle].gameObject.transform;
                controller.CheckWinLose();
                if (!TripleBirdMGController.FirstOpenTripleBirdMNGame)
                {
                    controller.tut.MoveToAnotherBird();
                }
                float valueScaleX = Mathf.Abs(anim.transform.localScale.x);
                if (temp.position.x > this.transform.position.x)
                {
                    anim.transform.localScale = new Vector3(valueScaleX, anim.transform.localScale.y, anim.transform.localScale.z);
                }
                else
                {
                    anim.transform.localScale = new Vector3(-valueScaleX, anim.transform.localScale.y, anim.transform.localScale.z);
                }
                float timeMove = Vector2.Distance(this.transform.position, temp.position) / controller.levelManager.speedMove;
                isFlying = true;
                this.transform.DOMove(temp.position, timeMove).OnComplete(() =>
                {
                    anim.SetAnimation(GROUNDING, false);
                    var tempNest = temp.parent.transform.position;
                    float timeGround = Vector2.Distance(this.transform.position, tempNest) / controller.levelManager.speedMove;
                    this.transform.DOMove(tempNest, timeGround).OnComplete(() =>
                     {
                         isFlying = false;
                         anim.SetAnimation(IDLE, true);
                         StopFlySound();
                         lvMng.AddListComplete(this);
                         lvMng.RemoveList();
                     }
                    );
                });
            }
            else
            {
                if (controller.levelManager.birdWaiting.Count < 6 && controller.levelManager.CheckCanClear())
                {

                    for (int i = 0; i < lvMng.lsPosWait.Count; i++)
                    {
                        if (!lvMng.lsPosWait[i].hasBird)
                        {
                            lvMng.lsPosWait[i].hasBird = true;
                            posWait = lvMng.lsPosWait[i];
                            controller.levelManager.birdWaiting.Enqueue(this);
                            this._typeTile = TypeTile.CanNotClick;
                            CheckAround(this);
                            PlaySound();
                            anim.SetAnimation(FLY, true);
                            shadow.gameObject.SetActive(false);
                            this.transform.SetParent(lvMng.CellMother.transform);
                            var temp = lvMng.lsPosWait[i].transform.position;
                            float valueScaleX = Mathf.Abs(anim.transform.localScale.x);
                            if (temp.x > this.transform.position.x)
                            {
                                anim.transform.localScale = new Vector3(valueScaleX, anim.transform.localScale.y, anim.transform.localScale.z);
                            }
                            else
                            {
                                anim.transform.localScale = new Vector3(-valueScaleX, anim.transform.localScale.y, anim.transform.localScale.z);
                            }
                            float timeMove = Vector2.Distance(this.transform.position, temp) / controller.levelManager.speedMoveToWait;
                            if (controller.levelManager.birdWaiting.Count >= 6)
                            {
                                controller.levelManager.canClickBird = false;
                            }
                            this.transform.DOMove(temp, timeMove);
                            break;
                        }
                    }
                }
                else
                {
                    if (!controller.levelManager.CheckCanClear())
                    {
                        controller.CheckWinLose();
                    }
                }
            }
        }
        else return;
    }
    public void CheckThatTile()
    {
        var lvMng = controller.levelManager;
        anim.SetAnimation(IDLE, true);
        StopFlySound();
        lvMng.AddListComplete(this);
        lvMng.RemoveList();
    }
    public void MoveToPos()
    {
        var lvMng = controller.levelManager;
        var temp = lvMng.lsPost[SortTitle].gameObject.transform;
        controller.CheckWinLose();
        if (posWait != null)
        {
            posWait.hasBird = false;
            posWait = null;
        }
        controller.levelManager.canClickBird = true;
        if (!TripleBirdMGController.FirstOpenTripleBirdMNGame)
        {
            controller.tut.MoveToAnotherBird();
        }
        float valueScaleX = Mathf.Abs(anim.transform.localScale.x);
        if (temp.position.x > this.transform.position.x)
        {
            anim.transform.localScale = new Vector3(valueScaleX, anim.transform.localScale.y, anim.transform.localScale.z);
        }
        else
        {
            anim.transform.localScale = new Vector3(-valueScaleX, anim.transform.localScale.y, anim.transform.localScale.z);
        }
        float timeMove = Vector2.Distance(this.transform.position, temp.position) / controller.levelManager.speedMove;
        controller.levelManager.canClickBird = true;
        this.transform.DOKill();
        isFlying = true;
        this.transform.DOMove(temp.position, timeMove).OnComplete(() =>
        {
            anim.SetAnimation(GROUNDING, false);
            var tempNest = temp.parent.transform.position;
            float timeGround = Vector2.Distance(this.transform.position, tempNest) / controller.levelManager.speedMove;
            this.transform.DOMove(tempNest, timeGround).OnComplete(() =>
            {
                isFlying = false;
                anim.SetAnimation(IDLE, true);
                StopFlySound();
                lvMng.AddListComplete(this);
                lvMng.RemoveList();
            });
        });
    }
}
