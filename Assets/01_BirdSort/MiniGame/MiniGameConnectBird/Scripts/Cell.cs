using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using EventDispatcher;

public class Cell : MonoBehaviour
{
    public int id;
    [Header("Basic Information")]
    [SerializeField] private Vector2Int cellPosition;
    [SerializeField] private int cellCatType;
    [SerializeField] private bool isEmpty;
    [SerializeField] private int faceDirection = 1;
    public Color cellColor;
    private Transform itemContainer;
    public Button button;
    [SerializeField] private Animator animator;
    public MiniGameBird miniGameBird;
    private ConnectBirdMGLogic controller;
    public PointConnectBirdMiniGame point;
    public bool cellHasItem;

    public void Initialize(ConnectBirdMGLogic controllerParam)
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(() => OnCellClicked());

        animator.enabled = false;
        controller = controllerParam;
        button.interactable = false;
    }


    private void OnCellClicked()
    {
        //if(GameplayController.Instance.level.board.clickControlMatrix[cellPosition.x, cellPosition.y])
        //{
        //    if (!GetCatItemManager().CheckHasItemInList(CELL_ITEM_TYPE.Box) && !GetCatItemManager().CheckHasItemInList(CELL_ITEM_TYPE.Cage))
        //    {
        // Debug.LogError("POS: " + cellPosition);



        controller.boardUserInput.InvokeCellClicked(cellPosition);


        if (cellHasItem)
        {

            if (!ConnectBirdMGController.FirstOpenConnectBirdMNGame)
            {
                HomeController.Instance.miniGameController.connectBirdMG.wasChoose = true;
                controller.tut.MovePost2();
            }

        }
        GameController.Instance.musicManager.PlayClickSound();

        //    }
        //    else
        //    {
        //        Debug.Log("There is box not opened/cage at this position !");
        //    }
        //}
        //     Debug.Log("There is box not opened/cage at this position !");
    }

    public void RemoveCell()
    {
        //  Debug.Log("remove cell at " + cellPosition);
        button.enabled = false;
        transform.Find("Hint").Find("Shine").GetComponent<Image>().enabled = false;
        transform.Find("Hint").Find("Radial").GetComponent<Image>().enabled = false;
        //if (GetCatItemManager() != null)
        //{
        //    GetCatItemManager().OnThisCellRemovedOutOfBoard();
        //}
        if (cellHasItem)
        {

            // var controller1 = controller.GetPoint(id);
            // controller1.transform.SetParent(controller.gameAssets.canvas);
            // controller1.HandleMovePointToPost(controller.iconFeather.transform.position , delegate {
            //    GamePlayController.Instance.miniGameController.connectBirdMG.PointConnectBirdMiniGame += 1;
            //    controller.numbPoint += 1;
            //    controller.tvFeather.text = "" + controller.numbPoint;
            //    controller.numbPointToReset += 1;

            //    controller.point.Remove(controller1);
            //    SimplePool.Despawn(controller1.gameObject);

            //});
            if (point != null)
            {
                point.transform.SetParent(controller.gameAssets.panelGame);
                controller.numbPoint += 1;
                controller.numbPointToReset += 1;
                controller.queueScore.Enqueue(controller.numbPoint);
                GameController.Instance.musicManager.PlayOneShot(controller.gameAssets.claimSound);
                point.HandleMovePointToPost(controller.iconFeather.transform.position, delegate
                {
                    HomeController.Instance.miniGameController.connectBirdMG.PointConnectBirdMiniGame += 1;

                    //controller.tvFeather.text = "" + controller.numbPoint;


                    controller.point.Remove(point);

                    SimplePool2.Despawn(point.gameObject);
                    if (controller.queueScore.Count > 0)
                    {
                        controller.tvFeather.text = "" + controller.numbPoint;
                    }

                });

            }







        }
        else
        {

        }






        var cat = GetCat();
        if (cat != null)
        {

            //var bird = GetCat_2();
            //if (bird != null)
            //{
            //    bird.GetComponent<MiniGameBird>().FlyOut(miniGameBird.idLeftRight ? controller.gameAssets.post2.transform.position : controller.gameAssets.post1.transform.position, delegate
            //{
            //});
            //  var a = Vector3.Lerp(new Vector3(1,1,1), new Vector3(2,2,1), -1);

            //}
            //  Debug.LogError(cellPosition);
            //MiniGameBird mn = transform.Find("Cat").Find("miniGameBird").GetComponent<MiniGameBird>();
            //mn.transform.SetParent(controller.gameAssets.panelGame);
            //mn.FlyOut(miniGameBird.idLeftRight ? controller.gameAssets.post2.transform.position : controller.gameAssets.post1.transform.position, delegate
            //    {
            //        SimplePool2.Despawn(miniGameBird.gameObject);
            //    });
            if (miniGameBird != null)
            {

                miniGameBird.transform.SetParent(controller.gameAssets.panelGame);
                //Anhtusz GameController.Instance.musicManager.PlayOneShot(NameMusic.FlyOut, controller.gameAssets.birdFlyOutClip);
                miniGameBird.FlyOut(miniGameBird.idLeftRight ? controller.gameAssets.post2.transform.position : controller.gameAssets.post1.transform.position, delegate
                    {
                        SimplePool2.Despawn(miniGameBird.gameObject);
                    });
            }
            if (cellHasItem)
            {
                controller.tut.OffTut();
            }



            Destroy(cat.gameObject);
            // Destroy(GetCat().gameObject);
        }
        isEmpty = true;
    }
    public void BirdFlyOut()
    {


    }



    public void RemoveCellNotOnBoard()
    {
        //   Debug.Log("remove cell (wall) at " + cellPosition);
        button.enabled = false;
        transform.Find("Hint").Find("Shine").GetComponent<Image>().enabled = false;
        transform.Find("Hint").Find("Radial").GetComponent<Image>().enabled = false;
        if (GetCat() != null)
        {
            Destroy(GetCat().gameObject);
        }
        isEmpty = true;
    }

    private void ChangeBoardLineModifierDirection(string dir)
    {
        controller.board.ChangeBoardLineModifiersDirection(dir);
    }

    public void EnableCell()
    {
        isEmpty = false;
        button.enabled = true;
    }

    public void DisableCell()
    {
        button.enabled = false;
    }

    public void DisplayCat()
    {
        if (GetCat() != null)
        {
            Debug.Log("Display cat at " + cellPosition);
            var rootTransform = GetCat().GetChild(0).Find("root");
            rootTransform.GetComponent<SkeletonUtilityBone>().mode = SkeletonUtilityBone.Mode.Override;
            RectTransform catRootRectTransform = rootTransform.GetComponent<RectTransform>();
            var scale = catRootRectTransform.localScale;
            scale.x = 1;
            scale.y = 1;
            catRootRectTransform.localScale = scale;
            EnableCell();
        }
    }

    public void HideCat()
    {
        if (GetCat() != null)
        {
            Debug.Log("Hide cat at " + cellPosition);
            var rootTransform = GetCat().GetChild(0).Find("root");
            rootTransform.GetComponent<SkeletonUtilityBone>().mode = SkeletonUtilityBone.Mode.Override;
            RectTransform catRootRectTransform = rootTransform.GetComponent<RectTransform>();
            var scale = catRootRectTransform.localScale;
            scale.x = 0;
            scale.y = 0;
            catRootRectTransform.localScale = scale;
            DisableCell();
        }
    }

    public void ResetCellScale()
    {
        RectTransform rect = this.GetComponent<RectTransform>();
        var scale = rect.localScale;
        scale.x = 1f;
        scale.y = 1f;
        rect.localScale = scale;
    }

    public bool isCatScale = false;

    public void ToggleCellScale(bool isScaleUp)
    {
        if (GetCat() != null)
        {
            isCatScale = isScaleUp;
            RectTransform rect = this.GetComponent<RectTransform>();
            var scale = rect.localScale;
            scale.x = (isScaleUp) ? 1.2f : 1f;
            scale.y = (isScaleUp) ? 1.2f : 1f;
            rect.localScale = scale;
        }
    }

    public bool isCellZooming = false;

    public void ToggleCatZoomEffect(bool value)
    {
        if (isCatScale)
        {
            ToggleCellScale(false);
        }
        isCellZooming = value;
        animator.enabled = value;
        if (value)
        {
            animator.Play("CatZoomEffect", -1, 0f);
        }
    }

    private Vector2 oldPivot = Vector2.zero;

    public void SetCatPositionInPlaceBox()
    {
        if (GetCat() != null)
        {
            RectTransform catRect = GetCat().GetComponent<RectTransform>();
            oldPivot = catRect.pivot;
            catRect.pivot = new Vector2(0.5f, 0.5f);
            catRect.anchoredPosition = new Vector2(0, 20);
        }
    }

    public void SetCatPositionInPlaceOld()
    {
        if (GetCat() != null)
        {
            RectTransform catRect = GetCat().GetComponent<RectTransform>();
            catRect.pivot = oldPivot;
            catRect.anchoredPosition = new Vector2(0, 0);
        }
    }

    public void FlipCatTo(int dir)
    {
        bool changed = (faceDirection != dir) ? true : false;
        var scale = GetCat().GetComponent<RectTransform>().localScale;
        scale = new Vector3((dir == -1) ? -1f : 1f, 1f, 1f);
        GetCat().GetComponent<RectTransform>().localScale = scale;

        //if (GetCatItemManager() != null && changed)
        //{
        //    Debug.Log("Flip all items");
        //    GetCatItemManager().FlipAllItems();
        //}
    }

    #region ITEMS RELATEDs

    private List<CellItem> startCellItems = new List<CellItem>();
    private Coroutine coroutine;
    public void AddStartCellItem(CellItem cellItem)
    {
        startCellItems.Add(cellItem);
        cellHasItem = true;

        coroutine = null;
        coroutine = StartCoroutine(SpawnItem());

        //var a = GetCat();
        //a.GetComponent<Image>().color = Color.red;

    }

    public IEnumerator SpawnItem()
    {
        yield return new WaitForSeconds(2);
        var a = GetCat();

        point = SimplePool2.Spawn(controller.gameAssets.point, new Vector3(a.position.x, a.position.y, a.position.z), Quaternion.identity);
        point.transform.SetParent(a);
        point.transform.localPosition = new Vector3(40, -40, 0);
        point.transform.localScale = new Vector3(1, 1, 1);
        point.HandleScaleInOut();
        point.gameObject.name = "point";
        point.id = id;
        controller.point.Add(point);
        StopCoroutine(coroutine);
    }

    public PointConnectBirdMiniGame GetPoint()
    {
        if (IsCellEmpty())
        {
            Debug.LogError("This cell is set to empty before " + cellPosition);
            return null;
        }

        return this.transform.Find("point").gameObject.GetComponent<PointConnectBirdMiniGame>();
    }

    public void InitializeAllCellItems()
    {
        //if (GetCatItemManager())
        //{
        //    Debug.Log("insert items");
        //    CatItemManager catItemManager = GetCatItemManager();
        //    catItemManager.InitializeItemTransform();
        //    catItemManager.GetItemsFromListCellItem(startCellItems);
        //    catItemManager.HideAllItems();
        //    catItemManager.ShowCurrentItems();
        //    FlipCatTo(faceDirection);
        //}
        //startCellItems.Clear();
    }

    //public CatItemManager GetCatItemManager()
    //{
    //    if(GetCat() == null || IsCellEmpty())
    //    {
    //        return null;
    //    }
    //    return GetCat().Find("Items").GetComponent<CatItemManager>();
    //}

    public bool CheckHasItemInThisCell(CELL_ITEM_TYPE itemType)
    {
        if (IsCellEmpty())
        {
            return false;
        }
        //if (GetCatItemManager() == null)
        //{
        //    return false;
        //}
        //return GetCatItemManager().CheckHasItemInList(itemType);
        return false;
    }

    public bool CheckHasBlockItemsInThisCell()
    {
        if (CheckHasItemInThisCell(CELL_ITEM_TYPE.Cage) || CheckHasItemInThisCell(CELL_ITEM_TYPE.Box))
        {
            return true;
        }
        return false;
    }

    #endregion

    public Transform GetCat()
    {
        if (IsCellEmpty())
        {
            Debug.Log("This cell is set to empty before " + cellPosition);
            return null;
        }
        return this.transform.Find("Cat");
    }
    public GameObject GetCat_2()
    {
        if (IsCellEmpty())
        {
            Debug.Log("This cell is set to empty before " + cellPosition);
            return null;
        }
        return this.transform.Find("miniGameBird").gameObject;
    }

    public Vector2Int GetCellPosition()
    {
        return this.cellPosition;
    }

    public void SetCellPosition(Vector2Int value)
    {
        this.cellPosition = value;
    }

    public int GetCellCatType()
    {
        return this.cellCatType;
    }

    public void SetCellCatType(int value)
    {
        cellCatType = value;
    }

    public void SetCellBackground(Sprite sprite)
    {
        transform.Find("Background").GetComponent<Image>().sprite = sprite;
    }

    public bool IsCellEmpty()
    {
        return this.isEmpty;
    }

    //This function is call only one each level, set up begin cat
    public void SetCellCat()
    {

        if (transform.Find("Cat") != null)
        {
            Destroy(transform.Find("Cat").gameObject);
        }
        GameObject catPrefab = null;

        catPrefab = controller.gameAssets.catPrefabs[cellCatType - 1];
        var catGo = Instantiate(catPrefab, this.transform);
        catGo.name = "Cat";
        miniGameBird = controller.gameAssets.SpawnBird(cellCatType - 1, new Vector2(catPrefab.GetComponent<RectTransform>().anchoredPosition.x, catPrefab.GetComponent<RectTransform>().anchoredPosition.y - 70), catGo.transform, controller.board.boardHeight, delegate { Invoke("Wait", 0.5f); });
        miniGameBird.gameObject.name = "miniGameBird";
        //    Debug.LogError("miniGameBird");
        itemContainer = transform.Find("Items");
        //  CatItemManager catItemManager = itemContainer.gameObject.AddComponent<CatItemManager>() as CatItemManager;
        itemContainer.SetParent(GetCat()); //make items always on top of cat
        RectTransform itemContainerRect = itemContainer.GetComponent<RectTransform>();
        itemContainerRect.anchoredPosition = new Vector2(0, 0);
    }
    private void Wait()
    {
        button.interactable = true;
    }

    private void SetCellColor()
    {
        Color color = new Color();
        switch (cellCatType)
        {
            case 1:
                color = new Color(255f / 255f, 140f / 255f, 0);
                break;
            case 2:
                color = new Color(139f / 255f, 69f / 255f, 19f / 255f);
                break;
            case 3:
                color = new Color(255f / 255f, 105f / 255f, 180f / 255f);
                break;
            case 4:
                color = new Color(153f / 255f, 51f / 255f, 255f / 255f);
                break;
            case 5:
                color = new Color(248f / 255f, 248f / 255f, 255f / 255f);
                break;
            case 6:
                color = new Color(51f / 255f, 102f / 255f, 0);
                break;
            case 7:
                color = new Color(255f / 255f, 215f / 255f, 0);
                break;
            case 8:
                color = new Color(127f / 255f, 255f / 255f, 212f / 255f);
                break;
        }
        cellColor = color;
        transform.Find("Background").GetComponent<Image>().color = color;
    }

    public void SetCatFaceDirection(int dir)
    {
        this.faceDirection = dir;
    }

    public Vector3 GetWorldPositionOfCellCat()
    {
        if (GetCat())
        {
            return GetCat().position;
        }
        else
        {
            return controller.board.cellWorldPointPositions[cellPosition.x, cellPosition.y];
        }
    }
}
