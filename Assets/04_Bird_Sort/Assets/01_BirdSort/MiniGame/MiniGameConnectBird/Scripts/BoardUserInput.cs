using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EventDispatcher;
using Spine.Unity;

public class BoardUserInput : MonoBehaviour
{
    public Vector2Int firstCell = new Vector2Int(-1, -1);
    public Vector2Int secondCell = new Vector2Int(-1, -1);
    private bool isNoCellSelected = true;
    private bool enableClickOnCell = true;

    private ConnectBirdMGLogic controller;

    public void Init(ConnectBirdMGLogic controllerParam)
    {
        controller = controllerParam;
    }    


    public void Initialize()
    {
        isNoCellSelected = true;
        this.RegisterListener(EventID.CAT_MOVE_ON_BOARD_BEGIN, (param) => OnCatMoveOnBoardBegin());
        this.RegisterListener(EventID.CAT_MOVE_ON_BOARD_COMPLETED, (param) => OnCatMoveOnBoardCompleted());
    }

    //Khi 1 cell click se invoke phuong thuc nay
    public void InvokeCellClicked(Vector2Int cellClicked)
    {
     
        if (enableClickOnCell)
        {
            //Chua ch?n ô nào => ch?n ô dó làm ô d?u tiên, highlight
            if (isNoCellSelected)
            {
                firstCell = cellClicked;
                isNoCellSelected = false;

             //   BirdSortGameController.Instance.soundController.PlaySound(AUDIO_CLIP_TYPE.CatMewSound, 1);
                HighlightCat(cellClicked);

                //GameplayController.Instance.handTutorialController.InvokeHighlightCat(cellClicked);
                return;
            }
            //Ch?n vào chính ô v?a du?c ch?n => B? ch?n ô dó, un-highlight
            if (!isNoCellSelected && cellClicked == firstCell)
            {
                UnhighlightCat(firstCell);
                firstCell = new Vector2Int(-1, -1);
                isNoCellSelected = true;
                return;
            }
           // Debug.LogError("FC:" + firstCell + " CC:" + cellClicked);
            //Ch?n vào 1 ô khác lo?i v?i ô ban d?u du?c ch?n => Ch?n ô v?a click làm ô d?u, highlight ô click
            if (!isNoCellSelected && !controller.board.CheckTwoCellsSameAnimalType(firstCell, cellClicked))
            {
                UnhighlightCat(firstCell);
                firstCell = cellClicked;
                isNoCellSelected = false;
                controller.tut.MovePost1();
                if (HomeController.Instance.miniGameController.connectBirdMG.wasChoose)
                {
                    HomeController.Instance.miniGameController.connectBirdMG.wasChoose = false;
                }

                    // BirdSortGameController.Instance.soundController.PlaySound(AUDIO_CLIP_TYPE.CatMewSound, 1);
                    HighlightCat(cellClicked);

           //     GamePlayController.Instance.handTutorialController.InvokeHighlightCat(cellClicked);
                return;
            }
            //Ch?n vào 1 ô cùng lo?i v?i ô ban d?u du?c ch?n => check n?i, du?c thì xóa 2 ô => h?y ch?n c? hai, un-highlight
            if (!isNoCellSelected && controller.board.CheckTwoCellsSameAnimalType(firstCell, cellClicked))
            {
                secondCell = cellClicked;

                //Neu co the ket noi
                if (controller.board.CheckTwoCellsCanBeConnected(firstCell, secondCell))
                {
                    //     BirdSortGameController.Instance.soundController.PlaySound(AUDIO_CLIP_TYPE.CatMewSound, 2);
                    //     GameplayController.Instance.gameplayUIController.hand.gameObject.SetActive(false);
                    isNoCellSelected = true;
                    HighlightCat(secondCell);
                    //     GameplayController.Instance.handTutorialController.InvokeTwoCellsConnected(firstCell, secondCell);

                   controller.board.NewConnectTwoPointsWithLines(firstCell, secondCell,
                        Utility.IntToCatType(controller.board.GetCatTypeFromCell(firstCell)));

                  controller.board.DisableClickControlMatrixAt(firstCell);
                  controller.board.DisableClickControlMatrixAt(secondCell);

              
                    if (HomeController.Instance.miniGameController.connectBirdMG.wasChoose)
                    {
                        controller.tut.OffTut();
                    }
                    //GameplayController.Instance.level.board.DisableClickControlAllCellsOnBoard();
                }
                else
                {
                    //  BirdSortGameController.Instance.soundController.PlaySound(AUDIO_CLIP_TYPE.CannotConnect);
                   
                    UnhighlightCat(firstCell);
                    //MMVibrationManager.Haptic(HapticTypes.Selection, false, true, this);
                    controller.gameAssets.SpawnXObject(controller.board.GetCellFromPosition(secondCell).transform.position, controller.board.GetCellFromPosition(secondCell).transform);

                }
                secondCell = new Vector2Int(-1, -1);
                isNoCellSelected = true;
                
                return;
            }
            Debug.LogError("Not fall into any cases, maybe error !");
        }
    }

    private void RemoveTwoSelectedCells(Vector2Int cell1, Vector2Int cell2)
    {
      controller.board.RemoveCell(cell1);
      controller.board.RemoveCell(cell2);
    }

    public void HighlightCat(Vector2Int cellPosition)
    {
        if (controller.board.GetCellFromPosition(cellPosition).IsCellEmpty())
        {
            return;
        }
      //  Debug.Log("Highlight this cell");
        //Debug.Log(GameplayController.Instance.level.board.cellGameObjectsInScene[cellPosition.x, cellPosition.y].transform.Find("Cat").GetComponent<SkeletonGraphic>().skeletonDataAsset.name);
        //RectTransform rectTransform = GamePlayController.Instance.level.board.cellGameObjectsInScene[cellPosition.x, cellPosition.y].transform.Find("Cat").GetChild(0).Find("root").Find("SHADOW").GetComponent<RectTransform>();

        //var scale = rectTransform.localScale;
        //scale.x = 1;
        //scale.y = 1;
        //rectTransform.localScale = scale;
    
        if(!controller.board.GetCellFromPosition(cellPosition).isCellZooming)
        {
            var temp = controller.board.GetCellFromPosition(cellPosition);
            temp.ToggleCellScale(true);
            temp.GetComponent<Cell>().button.interactable = false;
            temp.GetComponent<Cell>().miniGameBird.OnOutLine();
         
        }
    }

    public void UnhighlightCat(Vector2Int cellPosition)
    {
        if (controller.board.GetCellFromPosition(cellPosition).IsCellEmpty())
        {
            return;
        }
        //RectTransform rectTransform = GamePlayController.Instance.level.board.cellGameObjectsInScene[cellPosition.x, cellPosition.y].transform.Find("Cat").GetChild(0).Find("root").Find("SHADOW").GetComponent<RectTransform>();
        //var scale = rectTransform.localScale;
        //scale.x = 0;
        //scale.y = 0;
        //rectTransform.localScale = scale;

        var temp = controller.board.GetCellFromPosition(cellPosition);
        temp.ToggleCellScale(false);
        temp.GetComponent<Cell>().button.interactable = true;
        temp.GetComponent<Cell>().miniGameBird.OffOutLine();
    }

    public void OnCatMoveOnBoardBegin()
    {
        enableClickOnCell = false;
    }

    public void OnCatMoveOnBoardCompleted()
    {
        enableClickOnCell = true;
    }
}

