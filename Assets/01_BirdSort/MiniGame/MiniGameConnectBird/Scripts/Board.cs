using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EventDispatcher;
using DG.Tweening.Core;
using DG.Tweening;
using Spine.Unity;

public class Board : MonoBehaviour
{
    public GridLayoutGroup gridLayoutGroup;
    public GridLayoutGroup cellBackgroundGridLayoutGroup;
    public Transform mainCanvas;
    public Transform catHomeContainer;
    public RectTransform gamePanel;
    public Transform cellBackgroundsContainer;
    public Transform pathMoveControllerContainer;
    public Transform initialItemPopupContainer;

    [Header("Board information")]
    public int boardHeight;
    public int boardWidth;

    //Save all cell-objects
    public GameObject[,] cellGameObjectsInScene;
    private List<GameObject> allCatMoveEffectGameObjects = new List<GameObject>();

    public Vector2[,] cellWorldPointPositions;
    public Vector3[,] cellRectPositions;
    public Vector2[,] catWorldPositions;

    public int[,] boardIntergerMatrix;

    public List<StartCellItem> startCellItems;

    public Dictionary<int, LINE_MODIFIER_TYPE> rowModifier = new Dictionary<int, LINE_MODIFIER_TYPE>();
    public Dictionary<int, LINE_MODIFIER_TYPE> columnModifier = new Dictionary<int, LINE_MODIFIER_TYPE>();

    private LineRenderer lineRenderer;
    private float timeWaitToSetBoard = 1.1f;

    private Queue<Vector2Int> movingCats = new Queue<Vector2Int>();
    private Queue<GameplayAction> gameplayActions = new Queue<GameplayAction>();
    public bool[,] clickControlMatrix;
    public float cellDistance;
    public bool isCatMove = false;
    public Vector3[] keyPositions = new Vector3[5];
    public List<GameObject> lsCell;
    private ConnectBirdMGLogic controller;

   
  



    public void Init(ConnectBirdMGLogic controller)
    {
        this.controller = controller;
    }    


    public void Initialize()
    {
        this.RegisterListener(EventID.MOVE_COMPLETED, (param) => OnMoveCompleted());
      
        SetupBoard();
    }
    public void OffCellBG()
    {
        foreach(GameObject item in lsCell)
        {
            if(item.gameObject.name == "CellBackGround")
            {
                item.GetComponent<Button>().interactable = false;
            }
        }
    }    
    public void SpawnBackGroundFirst()
    {
        BoardData boardData;
        boardData = controller.boardData;

        if (boardData != null)
        {
            boardHeight = boardData.boardHeight;
            boardWidth = boardData.boardWidth;

            //Control grid layout
            gridLayoutGroup.constraintCount = boardWidth + 2;
            cellBackgroundGridLayoutGroup.constraintCount = boardWidth + 2;
            float cellSize = new float();
            if (boardWidth == 6)
            {
                cellSize = 135;
            }
            else
            {
                cellSize = 120 + 15 * (8 - boardWidth);
            }
            gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
            cellBackgroundGridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);

            ProcessBoardLayoutString(boardData.boardLayout);
            if (boardData.lineModifier != "")
            {
                ProcessBoardLineModifierString(boardData.lineModifier);
            }


            CreateBoardBackground();

            //   controller.gameAssets.textLevel.text = boardData.levelName;
            //    GameplayController.Instance.gameplayUIController.SetUpLevelText(UserProfile.CurrentLevel.ToString());
        }
        else
        {
            Debug.LogWarning("ERROR: You loaded an empty board data !");
        }
       
    }


    //MAIN FUNCTION TO CREATE BOOARD
    public void SetupBoard()
    {
        BoardData boardData;
        boardData = controller.boardData;
       
        if (boardData != null)
        {
            boardHeight = boardData.boardHeight;
            boardWidth = boardData.boardWidth;

            //Control grid layout
            gridLayoutGroup.constraintCount = boardWidth + 2;
            cellBackgroundGridLayoutGroup.constraintCount = boardWidth + 2;
            float cellSize = new float();
            if (boardWidth == 6)
            {
                cellSize = 135;
            }
            else
            {
                cellSize = 120 + 15 * (8 - boardWidth);
            }
            gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
            cellBackgroundGridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);

            ProcessBoardLayoutString(boardData.boardLayout);
            if (boardData.lineModifier != "")
            {
                ProcessBoardLineModifierString(boardData.lineModifier);
            }

            CreateBoardOnScreen(boardData);
            CreateBoardBackground();
            InsertItemsToCells();
   //      Debug.LogError(boardData.levelName);
        //    GameplayController.Instance.gameplayUIController.SetUpLevelText(UserProfile.CurrentLevel.ToString());
        }
        else
        {
            Debug.LogWarning("ERROR: You loaded an empty board data !");
        }
        //Anhtusz  GameController.Instance.musicManager.PlayOneShot(NameMusic.FlyOut,controller.gameAssets.birdFlyOutClip);
    }

   

  

    //STEP 1: Process board string from json file
    private void ProcessBoardLayoutString(string boardLayoutString)
    {
        //Convert boardLayout string variable of level's json into right data
        cellGameObjectsInScene = new GameObject[boardHeight + 2, boardWidth + 2];
        boardIntergerMatrix = new int[boardHeight + 2, boardWidth + 2];
        clickControlMatrix = new bool[boardHeight + 2, boardWidth + 2];
        for (int i = 0; i <= boardHeight + 1; i++)
        {
            for (int j = 0; j <= boardWidth + 1; j++)
            {
                clickControlMatrix[i, j] = true;
            }
        }
        startCellItems.Clear();

        for (int i = 0; i <= boardHeight + 1; i++)
        {
            for (int j = 0; j <= boardWidth + 1; j++)
            {
                boardIntergerMatrix[i, j] = 0;
            }
        }
        string[] lines = boardLayoutString.Split('#');
        for (int line = 0; line < lines.Length; line++)
        {
            lines[line] = Utility.RemoveRedundantSpaces(lines[line]);
            string[] cellsInLine = lines[line].Split(' ');
            for (int word = 0; word < cellsInLine.Length; word++)
            {
                //Debug.Log("word: " + word + ", cellAtWord: " + cellsInLine[word]);
                string celTypeString = cellsInLine[word][0].ToString();
                string cellCatTypeString = "";
                if (celTypeString == "c")
                {
                    string[] cellParts = cellsInLine[word].Split('-');
                    cellCatTypeString = cellParts[0].Substring(1);
                    //cellCatTypeString = cellsInLine[word].Substring(1);
                //    Debug.Log(cellCatTypeString);
                    boardIntergerMatrix[line + 1, word + 1] = Int32.Parse(cellCatTypeString);

                    if (cellParts.Length > 1)
                    {
                        if (cellParts[1].Trim() == "b")
                        {
                            startCellItems.Add(new StartCellItem(new Vector2Int(line + 1, word + 1), new CellItem(CELL_ITEM_TYPE.Bomb, cellParts[2])));
                        }
                        if (cellParts[1].Trim() == "h")
                        {
                            startCellItems.Add(new StartCellItem(new Vector2Int(line + 1, word + 1), new CellItem(CELL_ITEM_TYPE.Box, "")));


                        }
                        if (cellParts[1].Trim() == "l")
                        {
                            startCellItems.Add(new StartCellItem(new Vector2Int(line + 1, word + 1), new CellItem(CELL_ITEM_TYPE.Cage, cellParts[2])));
                        }
                        if (cellParts[1].Trim() == "k")
                        {
                            startCellItems.Add(new StartCellItem(new Vector2Int(line + 1, word + 1), new CellItem(CELL_ITEM_TYPE.Key, cellParts[2])));
                        }
                        if (cellParts[1].Trim() == "s")
                        {
                            startCellItems.Add(new StartCellItem(new Vector2Int(line + 1, word + 1), new CellItem(CELL_ITEM_TYPE.Spy, cellParts[2])));
                        }
                    }
                }
            }
        }
    }

    public void RemoveCurrentBoardFromScene()
    {
        Transform boardContainer = gridLayoutGroup.transform;
        foreach (Transform child in boardContainer)
        {
            Destroy(child.gameObject);
        }
        boardHeight = 0;
        boardWidth = 0;
        Array.Clear(cellGameObjectsInScene, 0, cellGameObjectsInScene.Length);
        Array.Clear(cellWorldPointPositions, 0, cellWorldPointPositions.Length);
        Array.Clear(catWorldPositions, 0, catWorldPositions.Length);
        Array.Clear(boardIntergerMatrix, 0, boardIntergerMatrix.Length);
        startCellItems.Clear();
        rowModifier.Clear();
        columnModifier.Clear();
        movingCats.Clear();
    }

    //STEP 2: Process line modifiers
    private void ProcessBoardLineModifierString(string lineModifierString)
    {
        //Clear old dictionaries
        rowModifier.Clear();
        columnModifier.Clear();
        //Convert lineModifier string variable of level's json into right data
        string[] lineModifiersArray = lineModifierString.Split(' ');
        for (int modifier = 0; modifier < lineModifiersArray.Length; modifier++)
        {
            string[] modifierParts = lineModifiersArray[modifier].Split('_');

            //check first letter - r (row) or c (column)
            if (modifierParts[0][0] == 'r')
            {
                int rowOrder = Int32.Parse(modifierParts[0].Substring(1));
                if (modifierParts[1].Trim() == "Left")
                {
                    rowModifier.Add(rowOrder, LINE_MODIFIER_TYPE.Left);
                }
                else if (modifierParts[1].Trim() == "Right")
                {
                    rowModifier.Add(rowOrder, LINE_MODIFIER_TYPE.Right);
                }
            }
            else if (modifierParts[0][0] == 'c')
            {
                int columnOrder = Int32.Parse(modifierParts[0].Substring(1));
                if (modifierParts[1].Trim() == "Up")
                {
                    columnModifier.Add(columnOrder, LINE_MODIFIER_TYPE.Up);
                }
                else if (modifierParts[1].Trim() == "Down")
                {
                    columnModifier.Add(columnOrder, LINE_MODIFIER_TYPE.Down);
                }
            }
        }

        Debug.Log("Row modifiers:");
        foreach (var i in rowModifier)
        {
            Debug.Log(i.Key + " " + i.Value);
        }
        Debug.Log("Column modifiers:");
        foreach (var i in columnModifier)
        {
            Debug.Log(i.Key + " " + i.Value);
        }
    }
    public Coroutine coroutineStep3;
    public IEnumerator m_CoWaitForAllCellPositions;
    //STEP 3: Create main board's cells
    public void CreateBoardOnScreen(BoardData boardData)
    {
        foreach(GameObject item in lsCell)
        {
            Destroy(item);
        }
        lsCell.Clear();
    //    Debug.Log("Creating board on screen ...");
        
        cellWorldPointPositions = new Vector2[boardHeight + 2, boardWidth + 2];
        cellRectPositions = new Vector3[boardHeight + 2, boardWidth + 2];
        catWorldPositions = new Vector2[boardHeight + 2, boardWidth + 2];

        for (int i = 0; i <= boardHeight + 1; i++)
        {
            for (int j = 0; j <= boardWidth + 1; j++)
            {
                //Vector2 initCellPosition = new Vector2(j * cellSize + originalOffset.x, -1f * i * cellSize + originalOffset.y);
                int id = (i + j) * UnityEngine.Random.Range(1,100);
                GameObject cellGO = Instantiate(controller.gameAssets.cellPrefab, gamePanel);
                cellGO.name = "CellBackGround";
                lsCell.Add(cellGO);
                cellGameObjectsInScene[i, j] = cellGO;
                Cell cellScript = cellGO.GetComponent<Cell>();
                cellScript.Initialize(controller);
                cellScript.id = id;
                //if [i, j] is on board borders
                if (i == 0 || i == boardHeight + 1 || j == 0 || j == boardWidth + 1)
                {
                    boardIntergerMatrix[i, j] = 0;
                    cellScript.SetCellPosition(new Vector2Int(i, j));
                    cellScript.RemoveCellNotOnBoard();
                }
                // if [i, j] in board
                else
                {
                    cellScript.SetCellCatType(boardIntergerMatrix[i, j]);
                    cellScript.SetCellCat();
                    cellScript.SetCellPosition(new Vector2Int(i, j));
                }
            }
        }
        if (m_CoWaitForAllCellPositions != null)
        {
            StopCoroutine(m_CoWaitForAllCellPositions);
            m_CoWaitForAllCellPositions = null;
        }
        m_CoWaitForAllCellPositions = CoWaitForAllCellPositions();
        StartCoroutine(m_CoWaitForAllCellPositions);

    //    Debug.Log("Finish creating board on screen !");
    }
    

    //STEP 4: Create cells' background underneath
    public void CreateBoardBackground()
    {
        int runIndex = 0;
        for (int i = 0; i <= boardHeight + 1; i++)
        {
            for (int j = 0; j <= boardWidth + 1; j++)
            {
                var cellBackgroundTransform = Instantiate(controller.gameAssets.cellBackground, cellBackgroundsContainer);
                cellBackgroundTransform.GetComponent<CellBackGround>().ScaleIn();
               
                lsCell.Add(cellBackgroundTransform.gameObject);
                //if (i == 0 || i == boardHeight + 1 || j == 0 || j == boardWidth + 1)
                //{
                //    cellBackgroundTransform.GetComponent<Image>().enabled = false;
                //}
                //else
                //{
                //runIndex += 1;
                //if(runIndex % 2 == 0)
                //{
                //    cellBackgroundTransform.GetComponent<Image>().sprite = controller.gameAssets.grid_0;

                //}
                //else
                //{
                //    cellBackgroundTransform.GetComponent<Image>().sprite = controller.gameAssets.grid_1;


                //}
                cellBackgroundTransform.GetComponent<Image>().sprite = (runIndex % 2 == 0) ? controller.gameAssets.grid_0 : controller.gameAssets.grid_1;
                if (j == boardWidth + 1)
                {
                    if (boardWidth % 2 == 0)
                    {
                        runIndex += 0;
                    }
                    else
                    {
                        runIndex++;
                    }
                }
                else
                {
                    runIndex++;
                }

                



                //}
                //Debug.Log(new Vector2Int(i, j) + ":" + cellRectPositions[i, j]);
                //cellBackgroundTransform.GetComponent<RectTransform>().anchoredPosition = cellGameObjectsInScene[i, j].GetComponent<RectTransform>().anchoredPosition;
            }
        }
    }


    private void InsertItemsToCells()
    {
      //  Debug.Log("Inserting items to board's cells...");
        foreach (var startCellItem in startCellItems)
        {
          //  Debug.LogError("Pos:" + startCellItem.cellPosition);
            GetCellFromPosition(startCellItem.cellPosition).AddStartCellItem(startCellItem.cellItem);

            //GameObject cutOutEffectGO = Instantiate(GameAssets.Instance.cutOutCirclePrefab, cellGameObjectsInScene[startCellItem.cellPosition.x, startCellItem.cellPosition.y].transform).gameObject;
        }
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                if (!GetCellFromPosition(new Vector2Int(i, j)).IsCellEmpty())
                {
                    GetCellFromPosition(new Vector2Int(i, j)).InitializeAllCellItems();
                }
                else
                {
                    //Debug.Log("You're trying to set up cell items of an emty cell !");
                }
            }
        }
    //    Debug.Log("Finish inserting all items...");
    }

    IEnumerator CoWaitForPosition(Vector2Int cellPosition)
    {
        yield return new WaitForEndOfFrame();
        // Find position of objects in grid
        Vector3 symbolPosition = cellGameObjectsInScene[cellPosition.x, cellPosition.y].transform.position;
        Vector3 uiPosition = cellGameObjectsInScene[cellPosition.x, cellPosition.y].GetComponent<RectTransform>().localPosition;

        cellWorldPointPositions[cellPosition.x, cellPosition.y] = symbolPosition;
        cellRectPositions[cellPosition.x, cellPosition.y] = uiPosition;
    }

    IEnumerator CoWaitForAllCellPositions()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i <= boardHeight + 1; i++)
        {
            for (int j = 0; j <= boardWidth + 1; j++)
            {
                Vector3 symbolPosition = cellGameObjectsInScene[i, j].transform.position;
                cellWorldPointPositions[i, j] = symbolPosition;

                if (cellGameObjectsInScene[i, j].transform.Find("Cat") != null)
                {
                    Vector3 catPosition = cellGameObjectsInScene[i, j].transform.Find("Cat").position;
                    catWorldPositions[i, j] = catPosition;
                }
            }
        }

        cellDistance = cellWorldPointPositions[0, 1].x - cellWorldPointPositions[0, 0].x;

      
    }

    #region Utilities

    public void RemoveCell(Vector2Int cellPosition)
    {
        boardIntergerMatrix[cellPosition.x, cellPosition.y] = 0;
     
        GetCellFromPosition(cellPosition).RemoveCell();
     
    }

    public void EnableCell(Vector2Int cellPosition)
    {
        cellGameObjectsInScene[cellPosition.x, cellPosition.y].GetComponent<Cell>().EnableCell();
    }

    public void SetCatTypeToCell(Vector2Int cellPosition, int catType)
    {
        boardIntergerMatrix[cellPosition.x, cellPosition.y] = catType;
        GetCellFromPosition(cellPosition).SetCellCatType(catType);
    }

    private bool CheckCellEmpty(Vector2Int cellPosition)
    {
        if (cellPosition.x == 0 || cellPosition.x == boardHeight + 1 || cellPosition.y == 0 || cellPosition.y == boardWidth + 1)
        {
            return true;
        }
        return cellGameObjectsInScene[cellPosition.x, cellPosition.y].GetComponent<Cell>().IsCellEmpty();
    }

    public Cell GetCellFromPosition(Vector2Int cellPosition)
    {
        if (cellPosition.x < 1 || cellPosition.y < 1 || cellPosition.x > boardHeight || cellPosition.y > boardWidth)
        {
            Debug.Log("Getting empty cell");
        }
        return cellGameObjectsInScene[cellPosition.x, cellPosition.y].GetComponent<Cell>();
    }

    public int GetCatTypeFromCell(Vector2Int cellPosition)
    {
        return cellGameObjectsInScene[cellPosition.x, cellPosition.y].GetComponent<Cell>().GetCellCatType();
    }

    public Vector3 GetCellWorldPosition(Vector2Int cellPosition)
    {
        /*if(cellPosition.x < 1 || cellPosition.y < 1 || cellPosition.x > boardHeight || cellPosition.y > boardWidth)
        {
            float offset = 0.2f;
            Vector3 resultPoint = cellWorldPointPositions[cellPosition.x, cellPosition.y];
            //Left empty cells
            if (cellPosition.y < 1)
            {
                var temp = resultPoint;
                temp.x += offset;
                resultPoint = temp;
            }
            //right
            if (cellPosition.y > boardWidth)
            {
                var temp = resultPoint;
                temp.x -= offset;
                resultPoint = temp;
            }
            //up
            if (cellPosition.x < 1)
            {
                var temp = resultPoint;
                temp.y -= offset;
                resultPoint = temp;
            }
            //down
            if (cellPosition.x > boardHeight)
            {
                var temp = resultPoint;
                temp.y += offset;
                resultPoint = temp;
            }
            return resultPoint;
        }*/

        return cellWorldPointPositions[cellPosition.x, cellPosition.y];
    }

    public void RemoveTwoSelectedCells(Vector2Int p1, Vector2Int p2)
    {
        isDoingAllActions = false;

        RemoveCell(p1);
        RemoveCell(p2);

      //  GameController.Instance.soundController.PlaySound(AUDIO_CLIP_TYPE.Connect);

        //row col moving
        if (p1.x == p2.x)
        {
            CheckRowModifier(p1.x);
        }
        else
        {
            CheckRowModifier(p1.x, p2.x);
        }

        if (p1.y == p2.y)
        {
            CheckColumnModifier(p1.y);
        }
        else
        {
            CheckColumnModifier(p1.y, p2.y);
        }

        if (!CheckBoardHasLineModifiers())
        {
            DoAllActionsInGameplayActions();
        }
    }

    public void ConnectTwoPoint(Vector2Int p1, Vector2Int p2)
    {
        lineRenderer = new LineRenderer();
        lineRenderer = cellGameObjectsInScene[p1.x, p1.y].GetComponent<LineRenderer>();
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, cellWorldPointPositions[p1.x, p1.y]);
        lineRenderer.SetPosition(1, cellWorldPointPositions[p2.x, p2.y]);
    }

    private void AddPointToPath(ref List<Vector3> path, Vector2Int cellPos)
    {
        path.Add(GetCellWorldPosition(cellPos));
    }

    private void ConnectPath(List<Vector3> path, Vector2Int p1, Vector2Int p2)
    {
       
        PathMoveController pathMoveController = Instantiate(controller.gameAssets.pathMoveControllerPrefab, pathMoveControllerContainer).GetComponent<PathMoveController>();
        pathMoveController.Init(controller);
        pathMoveController.DoKill();
        controller.btnRetry.interactable = false;
        float speed = 25f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            float distanceBetweenTwoPoint = Utility.ManhattanDistance(path[i], path[i + 1]);
            speed += distanceBetweenTwoPoint * 6.5f;
        }

        Mathf.Clamp(speed, 25f, 100f);

    //    Debug.Log("Speed float:" + speed);

        pathMoveController.Setup(path, speed, () =>
        {
            StartCoroutine(Helper.StartAction(() =>
            {
                pathMoveController.DeleteHeart();
               controller.boardUserInput.UnhighlightCat(p1);
               controller.boardUserInput.UnhighlightCat(p2);
               RemoveTwoSelectedCells(p1, p2);
                controller.btnRetry.interactable = true;
                this.PostEvent(EventID.MOVE_COMPLETED);
            }, 0.1f));
        });
    }

    public IEnumerator ClearLine(Vector2Int p)
    {
        yield return new WaitForSeconds(0.225f);
        lineRenderer = cellGameObjectsInScene[p.x, p.y].GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    #endregion

    #region Check cell methods

    public bool CheckTwoCellsSameAnimalType(Vector2Int p1, Vector2Int p2)
    {
        return GetCellFromPosition(p1).GetCellCatType() == GetCellFromPosition(p2).GetCellCatType();
    }

    public bool CheckTwoCellsCanBeConnected(Vector2Int p1, Vector2Int p2)
    {
        if (!p1.Equals(p2) && CheckTwoCellsSameAnimalType(p1, p2))
        {
            if (p1.x == p2.x)
            {
                if (CheckLineX(p1.y, p2.y, p1.x)) return true;
            }
            if (p1.y == p2.y)
            {
                if (CheckLineY(p1.x, p2.x, p1.y)) return true;
            }
            int t = -1; // t is column find
                        // check in rectangle with x
            if ((t = CheckRectX(p1, p2)) != -1)
                return true;
            //check rectangle with x right
            if ((t = CheckRectX2(p1, p2, 1)) != -1)
                return true;
            //check rectangle with x left
            if ((t = CheckRectX2(p1, p2, -1)) != -1)
                return true;
            // check in rectangle with y
            if ((t = CheckRectY(p1, p2)) != -1)
                return true;
            // check rectangle with y down
            if ((t = CheckRectY2(p1, p2, 1)) != -1)
                return true;
            // check rectangle with y up
            if ((t = CheckRectY2(p1, p2, -1)) != -1)
                return true;
            // check more right
            if ((t = CheckMoreLineX(p1, p2, 1)) != -1)
                return true;
            // check more left
            if ((t = CheckMoreLineX(p1, p2, -1)) != -1)
                return true;
            // check more down
            if ((t = CheckMoreLineY(p1, p2, 1)) != -1)
                return true;
            // check more up
            if ((t = CheckMoreLineY(p1, p2, -1)) != -1)
                return true;
        }

        return false;
    }

    public void GetTwoCellsCanBeConnect(out Vector2Int p1, out Vector2Int p2)
    {
        bool isFindApair = false;
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                if (GetCellFromPosition(new Vector2Int(i, j)).IsCellEmpty() || GetCellFromPosition(new Vector2Int(i, j)).CheckHasBlockItemsInThisCell())
                {
                    continue;
                }
                else
                {
                    for (int m = 1; m <= boardHeight; m++)
                    {
                        for (int n = 1; n <= boardWidth; n++)
                        {
                            if ((i == m && j == n) || GetCellFromPosition(new Vector2Int(m, n)).IsCellEmpty() || GetCellFromPosition(new Vector2Int(m, n)).CheckHasBlockItemsInThisCell())
                            {
                                continue;
                            }
                            else
                            {
                         //       Debug.Log(i + "," + j + " | " + m + "," + n);
                                if (CheckTwoCellsCanBeConnected(new Vector2Int(i, j), new Vector2Int(m, n)))
                                {
                                    isFindApair = true;
                                    p1 = new Vector2Int(i, j);
                                    p2 = new Vector2Int(m, n);
                                //    Debug.Log("Hint:" + p1 + "," + p2);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
   //    Debug.Log("Can't find a pair able to connect !");
        p1 = new Vector2Int(-1, -1);
        p2 = new Vector2Int(-1, -1);
    }

    public bool CheckThereIsConnectableCells()
    {
        Vector2Int p1, p2;
       controller.board.GetTwoCellsCanBeConnect(out p1, out p2);
        if (p1 != new Vector2Int(-1, -1) && p2 != new Vector2Int(-1, -1))
        {
            return true;
        }
        return false;
    }
    public void ConnectTwoPointsWithLines(Vector2Int p1, Vector2Int p2, CAT_TYPE catType)
    {
        //CatMoveEffectManager.Create(catHomeContainer, catType, cellWorldPointPositions[p1.x, p1.y], GameplayController.Instance.gameplayUIController.catHome1.position, 3.5f, true);
        //CatMoveEffectManager.Create(catHomeContainer, catType, cellWorldPointPositions[p2.x, p2.y], GameplayController.Instance.gameplayUIController.catHome2.position, 3.5f, true);

        if (!p1.Equals(p2) && CheckTwoCellsSameAnimalType(p1, p2))
        {
            //check line with x
            if (p1.x == p2.x)
            {
                if (CheckLineX(p1.y, p2.y, p1.x))
                {
                    //PrepareHeartLine(p1, p2);

                    ConnectTwoPoint(p1, p2);
                    StartCoroutine(ClearLine(p1));
                    Debug.Log("Connecting two point");
                    return;
                }
            }
            // check line with y
            if (p1.y == p2.y)
            {
                if (CheckLineY(p1.x, p2.x, p1.y))
                {
                    //PrepareHeartLine(p1, p2);

                    ConnectTwoPoint(p1, p2);
                    StartCoroutine(ClearLine(p1));
                    return;
                }
            }
            int t = -1; // t is column find
                        // check in rectangle with x
            if ((t = CheckRectX(p1, p2)) != -1)
            {
                /*PrepareHeartLine(p1, new Vector2Int(p1.x, t),
                    new Vector2Int(p2.x, t), p2);*/

                ConnectTwoPoint(p1, new Vector2Int(p1.x, t));
                StartCoroutine(ClearLine(p1));
                ConnectTwoPoint(p2, new Vector2Int(p2.x, t));
                StartCoroutine(ClearLine(p2));
                ConnectTwoPoint(new Vector2Int(p1.x, t), new Vector2Int(p2.x, t));
                StartCoroutine(ClearLine(new Vector2Int(p1.x, t)));
                // ClearLine(lineRenderer, new Vector2Int(p1.x, t));
                return;
            }

            //check rectangle with x right
            if ((t = CheckRectX2(p1, p2, 1)) != -1)
            {
                /*PrepareHeartLine(p1, new Vector2Int(p1.x, t),
                    new Vector2Int(p2.x, t), p2);*/

                ConnectTwoPoint(p1, new Vector2Int(p1.x, t));
                StartCoroutine(ClearLine(p1));
                ConnectTwoPoint(p2, new Vector2Int(p2.x, t));
                StartCoroutine(ClearLine(p2));
                ConnectTwoPoint(new Vector2Int(p1.x, t), new Vector2Int(p2.x, t));
                StartCoroutine(ClearLine(new Vector2Int(p1.x, t)));
                // ClearLine(lineRenderer, new Vector2Int(p1.x, t));
                return;
            }

            //check rectangle with x left
            if ((t = CheckRectX2(p1, p2, -1)) != -1)
            {
                /*PrepareHeartLine(p1, new Vector2Int(p1.x, t),
                    new Vector2Int(p2.x, t), p2);*/

                ConnectTwoPoint(p1, new Vector2Int(p1.x, t));
                StartCoroutine(ClearLine(p1));
                ConnectTwoPoint(p2, new Vector2Int(p2.x, t));
                StartCoroutine(ClearLine(p2));
                ConnectTwoPoint(new Vector2Int(p1.x, t), new Vector2Int(p2.x, t));
                StartCoroutine(ClearLine(new Vector2Int(p1.x, t)));
                // ClearLine(lineRenderer, new Vector2Int(p1.x, t));
                return;
            }
            // check in rectangle with y
            if ((t = CheckRectY(p1, p2)) != -1)
            {
                /*PrepareHeartLine(p1, new Vector2Int(t, p1.y),
                    new Vector2Int(t, p2.y), p2);*/

                ConnectTwoPoint(p1, new Vector2Int(t, p1.y));
                StartCoroutine(ClearLine(p1));
                ConnectTwoPoint(p2, new Vector2Int(t, p2.y));
                StartCoroutine(ClearLine(p2));
                ConnectTwoPoint(new Vector2Int(t, p1.y), new Vector2Int(t, p2.y));
                StartCoroutine(ClearLine(new Vector2Int(t, p1.y)));
                return;
            }

            // check rectangle with y up
            if ((t = CheckRectY2(p1, p2, -1)) != -1)
            {
                /*PrepareHeartLine(p1, new Vector2Int(t, p1.y),
                    new Vector2Int(t, p2.y), p2);*/

                ConnectTwoPoint(p1, new Vector2Int(t, p1.y));
                StartCoroutine(ClearLine(p1));
                ConnectTwoPoint(p2, new Vector2Int(t, p2.y));
                StartCoroutine(ClearLine(p2));
                ConnectTwoPoint(new Vector2Int(t, p1.y), new Vector2Int(t, p2.y));
                StartCoroutine(ClearLine(new Vector2Int(t, p1.y)));
                return;
            }

            // check rectangle with y down
            if ((t = CheckRectY2(p1, p2, 1)) != -1)
            {
                /*PrepareHeartLine(p1, new Vector2Int(t, p1.y),
                    new Vector2Int(t, p2.y), p2);*/

                ConnectTwoPoint(p1, new Vector2Int(t, p1.y));
                StartCoroutine(ClearLine(p1));
                ConnectTwoPoint(p2, new Vector2Int(t, p2.y));
                StartCoroutine(ClearLine(p2));
                ConnectTwoPoint(new Vector2Int(t, p1.y), new Vector2Int(t, p2.y));
                StartCoroutine(ClearLine(new Vector2Int(t, p1.y)));
                return;
            }

            // check more right
            if ((t = CheckMoreLineX(p1, p2, 1)) != -1)
            {
                /*PrepareHeartLine(p1, new Vector2Int(p1.x, t),
                    new Vector2Int(p2.x, t), p2);*/

                Debug.Log("check ben phai");
                ConnectTwoPoint(p1, new Vector2Int(p1.x, t));
                StartCoroutine(ClearLine(p1));
                ConnectTwoPoint(p2, new Vector2Int(p2.x, t));
                StartCoroutine(ClearLine(p2));
                ConnectTwoPoint(new Vector2Int(p1.x, t), new Vector2Int(p2.x, t));
                StartCoroutine(ClearLine(new Vector2Int(p1.x, t)));
                //ClearLine(new Vector2Int(p1.x, t));
                return;
            }
            // check more left
            if ((t = CheckMoreLineX(p1, p2, -1)) != -1)
            {
                /*PrepareHeartLine(p1, new Vector2Int(p1.x, t),
                    new Vector2Int(p2.x, t), p2);*/

                Debug.Log("check ben trai");
                ConnectTwoPoint(p1, new Vector2Int(p1.x, t));
                StartCoroutine(ClearLine(p1));
                ConnectTwoPoint(p2, new Vector2Int(p2.x, t));
                StartCoroutine(ClearLine(p2));
                ConnectTwoPoint(new Vector2Int(p1.x, t), new Vector2Int(p2.x, t));
                StartCoroutine(ClearLine(new Vector2Int(p1.x, t)));
                return;
            }
            // check more up
            if ((t = CheckMoreLineY(p1, p2, -1)) != -1)
            {
                /*PrepareHeartLine(p1, new Vector2Int(t, p1.y),
                    new Vector2Int(t, p2.y), p2);*/

                ConnectTwoPoint(p1, new Vector2Int(t, p1.y));
                StartCoroutine(ClearLine(p1));
                ConnectTwoPoint(p2, new Vector2Int(t, p2.y));
                StartCoroutine(ClearLine(p2));
                ConnectTwoPoint(new Vector2Int(t, p1.y), new Vector2Int(t, p2.y));
                StartCoroutine(ClearLine(new Vector2Int(t, p1.y)));
                return;
            }
            // check more down
            if ((t = CheckMoreLineY(p1, p2, 1)) != -1)
            {
                /*PrepareHeartLine(p1, new Vector2Int(t, p1.y),
                    new Vector2Int(t, p2.y), p2);*/

                ConnectTwoPoint(p1, new Vector2Int(t, p1.y));
                StartCoroutine(ClearLine(p1));
                ConnectTwoPoint(p2, new Vector2Int(t, p2.y));
                StartCoroutine(ClearLine(p2));
                ConnectTwoPoint(new Vector2Int(t, p1.y), new Vector2Int(t, p2.y));
                StartCoroutine(ClearLine(new Vector2Int(t, p1.y)));
                return;
            }
        }
        return;
    }

    public void NewConnectTwoPointsWithLines(Vector2Int p1, Vector2Int p2, CAT_TYPE catType)
    {
        //CatMoveEffectManager.Create(catHomeContainer, catType, cellWorldPointPositions[p1.x, p1.y], GameplayController.Instance.gameplayUIController.catHome1.position, 3.5f, true);
        //CatMoveEffectManager.Create(catHomeContainer, catType, cellWorldPointPositions[p2.x, p2.y], GameplayController.Instance.gameplayUIController.catHome2.position, 3.5f, true);
        if (!p1.Equals(p2) && CheckTwoCellsSameAnimalType(p1, p2))
        {
            //check line with x
            if (p1.x == p2.x)
            {
                if (CheckLineX(p1.y, p2.y, p1.x))
                {
                    List<Vector3> path = new List<Vector3>();
                    AddPointToPath(ref path, p1);
                    AddPointToPath(ref path, p2);
                    ConnectPath(path, p1, p2);
                    return;
                }
            }
            // check line with y
            if (p1.y == p2.y)
            {
                if (CheckLineY(p1.x, p2.x, p1.y))
                {
                    List<Vector3> path = new List<Vector3>();
                    AddPointToPath(ref path, p1);
                    AddPointToPath(ref path, p2);
                    ConnectPath(path, p1, p2);
                    return;
                }
            }
            int t = -1; // t is column find
                        // check in rectangle with x
            if ((t = CheckRectX(p1, p2)) != -1)
            {
                List<Vector3> path = new List<Vector3>();
                AddPointToPath(ref path, p1);
                AddPointToPath(ref path, new Vector2Int(p1.x, t));
                AddPointToPath(ref path, new Vector2Int(p2.x, t));
                AddPointToPath(ref path, p2);
                ConnectPath(path, p1, p2);
                return;
            }

            //check rectangle with x right
            if ((t = CheckRectX2(p1, p2, 1)) != -1)
            {
                List<Vector3> path = new List<Vector3>();
                AddPointToPath(ref path, p1);
                AddPointToPath(ref path, new Vector2Int(p1.x, t));
                AddPointToPath(ref path, new Vector2Int(p2.x, t));
                AddPointToPath(ref path, p2);
                ConnectPath(path, p1, p2);
                return;
            }

            //check rectangle with x left
            if ((t = CheckRectX2(p1, p2, -1)) != -1)
            {
                List<Vector3> path = new List<Vector3>();
                AddPointToPath(ref path, p1);
                AddPointToPath(ref path, new Vector2Int(p1.x, t));
                AddPointToPath(ref path, new Vector2Int(p2.x, t));
                AddPointToPath(ref path, p2);
                ConnectPath(path, p1, p2);
                return;
            }
            // check in rectangle with y
            if ((t = CheckRectY(p1, p2)) != -1)
            {
                List<Vector3> path = new List<Vector3>();
                AddPointToPath(ref path, p1);
                AddPointToPath(ref path, new Vector2Int(t, p1.y));
                AddPointToPath(ref path, new Vector2Int(t, p2.y));
                AddPointToPath(ref path, p2);
                ConnectPath(path, p1, p2);
                return;
            }

            // check rectangle with y up
            if ((t = CheckRectY2(p1, p2, -1)) != -1)
            {
                List<Vector3> path = new List<Vector3>();
                AddPointToPath(ref path, p1);
                AddPointToPath(ref path, new Vector2Int(t, p1.y));
                AddPointToPath(ref path, new Vector2Int(t, p2.y));
                AddPointToPath(ref path, p2);
                ConnectPath(path, p1, p2);
                return;
            }

            // check rectangle with y down
            if ((t = CheckRectY2(p1, p2, 1)) != -1)
            {
                List<Vector3> path = new List<Vector3>();
                AddPointToPath(ref path, p1);
                AddPointToPath(ref path, new Vector2Int(t, p1.y));
                AddPointToPath(ref path, new Vector2Int(t, p2.y));
                AddPointToPath(ref path, p2);
                ConnectPath(path, p1, p2);
                return;
            }

            // check more right
            if ((t = CheckMoreLineX(p1, p2, 1)) != -1)
            {
                List<Vector3> path = new List<Vector3>();
                AddPointToPath(ref path, p1);
                AddPointToPath(ref path, new Vector2Int(p1.x, t));
                AddPointToPath(ref path, new Vector2Int(p2.x, t));
                AddPointToPath(ref path, p2);
                ConnectPath(path, p1, p2);
                return;
            }
            // check more left
            if ((t = CheckMoreLineX(p1, p2, -1)) != -1)
            {
                List<Vector3> path = new List<Vector3>();
                AddPointToPath(ref path, p1);
                AddPointToPath(ref path, new Vector2Int(p1.x, t));
                AddPointToPath(ref path, new Vector2Int(p2.x, t));
                AddPointToPath(ref path, p2);
                ConnectPath(path, p1, p2);
                return;
            }
            // check more up
            if ((t = CheckMoreLineY(p1, p2, -1)) != -1)
            {
                List<Vector3> path = new List<Vector3>();
                AddPointToPath(ref path, p1);
                AddPointToPath(ref path, new Vector2Int(t, p1.y));
                AddPointToPath(ref path, new Vector2Int(t, p2.y));
                AddPointToPath(ref path, p2);
                ConnectPath(path, p1, p2);
                return;
            }
            // check more down
            if ((t = CheckMoreLineY(p1, p2, 1)) != -1)
            {
                List<Vector3> path = new List<Vector3>();
                AddPointToPath(ref path, p1);
                AddPointToPath(ref path, new Vector2Int(t, p1.y));
                AddPointToPath(ref path, new Vector2Int(t, p2.y));
                AddPointToPath(ref path, p2);
                ConnectPath(path, p1, p2);
                return;
            }
        }
        return;
    }

    private bool CheckLineX(int y1, int y2, int x)
    {
        //Debug.Log("Check line " + x + " from " + y1 + " to " + y2);

        // find point have column max and min
        int min = Mathf.Min(y1, y2);
        int max = Mathf.Max(y1, y2);
        // run column
        for (int y = min + 1; y < max; y++)
        {
            if (!CheckCellEmpty(new Vector2Int(x, y)))
            { // if see barrier then die
              //Debug.Log("dieLineX: " + x + "" + y);
                return false;
            }
            //Debug.Log("ok: " + x + "" + y);
        }
        // not die -> success

        return true;
    }

    private bool CheckLineY(int x1, int x2, int y)
    {
        //Debug.Log("Check column " + y + " from " + x1 + " to " + x2);
        int min = Mathf.Min(x1, x2);
        int max = Mathf.Max(x1, x2);
        // run column
        for (int x = min + 1; x < max; x++)
        {
            if (!CheckCellEmpty(new Vector2Int(x, y)))
            {
                //Debug.Log("die: " + x + "" + y);
                return false;
            }
            //Debug.Log("ok: " + x + "" + y);
        }

        return true;
    }

    private int CheckRectX(Vector2Int p1, Vector2Int p2)
    {
        //Debug.Log("Check rect x");
        // find point have y min and max
        Vector2Int pMinY = p1, pMaxY = p2;
        if (p1.y > p2.y)
        {
            pMinY = p2;
            pMaxY = p1;
        }
        for (int y = pMinY.y; y <= pMaxY.y; y++)
        {
            if (y > pMinY.y && !CheckCellEmpty(new Vector2Int(pMinY.x, y)))
            {
                return -1;
            }
            // check two line
            if ((CheckCellEmpty(new Vector2Int(pMaxY.x, y)) || y == pMaxY.y) && CheckLineY(pMinY.x, pMaxY.x, y) && CheckLineX(y, pMaxY.y, pMaxY.x))
            {
                //Debug.Log("Rect x");
                /*Debug.Log("(" + pMinY.x + "," + pMinY.y + ") -> ("
                        + pMinY.x + "," + y + ") -> (" + pMaxY.x + "," + y
                        + ") -> (" + pMaxY.x + "," + pMaxY.y + ")");*/
                // if three line is true return column y

                return y;
            }
        }
        // have a line in three line not true then return -1
        return -1;
    }

    private int CheckRectY(Vector2Int p1, Vector2Int p2)
    {
        //Debug.Log("Check rect y");
        // find point have y min
        Vector2Int pMinX = p1, pMaxX = p2;
        if (p1.x > p2.x)
        {
            pMinX = p2;
            pMaxX = p1;
        }
        // find line and y begin
        for (int x = pMinX.x; x <= pMaxX.x; x++)
        {
            if (x > pMinX.x && !CheckCellEmpty(new Vector2Int(x, pMinX.y)))
            {
                return -1;
            }
            if (CheckCellEmpty(new Vector2Int(x, pMaxX.y)) && CheckLineX(pMinX.y, pMaxX.y, x) && CheckLineY(x, pMaxX.x, pMaxX.y))
            {
                /*Debug.Log("Rect y");
                Debug.Log("(" + pMinX.x + "," + pMinX.y + ") -> (" + x
                        + "," + pMinX.y + ") -> (" + x + "," + pMaxX.y
                        + ") -> (" + pMaxX.x + "," + pMaxX.y + ")");*/

                return x;
            }
        }
        return -1;
    }

    private int CheckRectX2(Vector2Int p1, Vector2Int p2, int type)
    {
        //Debug.Log("Check Rect X2");
        Vector2Int pMinY = p1, pMaxY = p2;
        if (p1.y > p2.y)
        {
            pMinY = p2;
            pMaxY = p1;
        }
        int y = pMaxY.y + type;
        if (pMinY.y == pMaxY.y)
        {
            while (CheckCellEmpty(new Vector2Int(pMinY.x, y)) && CheckCellEmpty(new Vector2Int(pMaxY.x, y)))
            {
                if (CheckLineY(pMinY.x, pMaxY.x, y))
                {
                    /*Debug.Log("TH X " + type);
                    Debug.Log("(" + pMinY.x + "," + pMinY.y + ") -> ("
                            + pMinY.x + "," + y + ") -> (" + pMaxY.x + "," + y
                            + ") -> (" + pMaxY.x + "," + pMaxY.y + ")");*/
                    return y;
                }
                y += type;
            }
        }
        return -1;
    }

    private int CheckRectY2(Vector2Int p1, Vector2Int p2, int type)
    {
        //Debug.Log("Check Rect Y2");
        Vector2Int pMinX = p1, pMaxX = p2;
        if (p1.x > p2.x)
        {
            pMinX = p2;
            pMaxX = p1;
        }
        int x = pMaxX.x + type;
        if (pMinX.x == pMaxX.x)
        {
            while (CheckCellEmpty(new Vector2Int(x, pMinX.y))
                   && CheckCellEmpty(new Vector2Int(x, pMaxX.y)))
            {
                if (CheckLineX(pMinX.y, pMaxX.y, x))
                {
                    /*Debug.Log("TH Y " + type);
                    Debug.Log("(" + pMinX.x + "," + pMinX.y + ") -> ("
                            + x + "," + pMinX.y + ") -> (" + x + "," + pMaxX.y
                            + ") -> (" + pMaxX.x + "," + pMaxX.y + ")");*/
                    return x;
                }
                x += type;
            }
        }
        return -1;
    }

    private int CheckMoreLineX(Vector2Int p1, Vector2Int p2, int type)
    {
        //Debug.Log("check more line x");
        // find point have y min
        Vector2Int pMinY = p1, pMaxY = p2;
        if (p1.y > p2.y)
        {
            pMinY = p2;
            pMaxY = p1;
        }
        // find line and y begin
        int y = pMaxY.y + type;
        int row = pMinY.x;
        int colFinish = pMaxY.y;
        if (type == -1)
        {
            colFinish = pMinY.y;
            y = pMinY.y + type;
            row = pMaxY.x;
            //Debug.Log("colFinish = " + colFinish);
        }
        // find column finish of line
        // check more
        if ((CheckCellEmpty(new Vector2Int(row, colFinish)) || pMinY.y == pMaxY.y) && CheckLineX(pMinY.y, pMaxY.y, row))
        {
            while (CheckCellEmpty(new Vector2Int(pMinY.x, y)) && CheckCellEmpty(new Vector2Int(pMaxY.x, y)))
            {
                if (CheckLineY(pMinY.x, pMaxY.x, y))
                {
                    /*Debug.Log("TH X " + type);
                    Debug.Log("(" + pMinY.x + "," + pMinY.y + ") -> ("
                            + pMinY.x + "," + y + ") -> (" + pMaxY.x + "," + y
                            + ") -> (" + pMaxY.x + "," + pMaxY.y + ")");*/
                    return y;
                }
                y += type;
            }
        }
        return -1;
    }

    private int CheckMoreLineY(Vector2Int p1, Vector2Int p2, int type)
    {
        //Debug.Log("check more y");
        Vector2Int pMinX = p1, pMaxX = p2;
        if (p1.x > p2.x)
        {
            pMinX = p2;
            pMaxX = p1;
        }
        int x = pMaxX.x + type;
        int col = pMinX.y;
        int rowFinish = pMaxX.x;
        if (type == -1)
        {
            rowFinish = pMinX.x;
            x = pMinX.x + type;
            col = pMaxX.y;
        }
        if ((CheckCellEmpty(new Vector2Int(rowFinish, col)) || pMinX.x == pMaxX.x) && CheckLineY(pMinX.x, pMaxX.x, col))
        {
            while (CheckCellEmpty(new Vector2Int(x, pMinX.y))
                    && CheckCellEmpty(new Vector2Int(x, pMaxX.y)))
            {
                if (CheckLineX(pMinX.y, pMaxX.y, x))
                {
                    /*Debug.Log("TH Y " + type);
                    Debug.Log("(" + pMinX.x + "," + pMinX.y + ") -> ("
                            + x + "," + pMinX.y + ") -> (" + x + "," + pMaxX.y
                            + ") -> (" + pMaxX.x + "," + pMaxX.y + ")");*/

                    return x;
                }
                x += type;
            }
        }
        return -1;
    }
    #endregion

    #region Check line modifiers

    public void CheckRowModifier(int row)
    {
        if (rowModifier.ContainsKey(row))
        {
            int type = (rowModifier[row] == LINE_MODIFIER_TYPE.Left) ? -1 : 1;
            MoveHorizontalLineBoard(row, type);
        }
    }

    public void CheckRowModifier(int row1, int row2)
    {
        if (rowModifier.ContainsKey(row1))
        {
            //Ca hai hang deu co modifiers
            if (rowModifier.ContainsKey(row2))
            {
              //  Debug.Log("check row modifier -> contain key row1 and row2");
                int type1 = (rowModifier[row1] == LINE_MODIFIER_TYPE.Left) ? -1 : 1;
                int type2 = (rowModifier[row2] == LINE_MODIFIER_TYPE.Left) ? -1 : 1;
             //   Debug.Log("Type 1:" + type1 + ", Type 2:" + type2);
                MoveHorizontalLineBoard(row1, row2, type1, type2);
            }
            //Chi hang row1 co modifier
            else
            {
                int type = (rowModifier[row1] == LINE_MODIFIER_TYPE.Left) ? -1 : 1;
                MoveHorizontalLineBoard(row1, type);
            }
        }
        else
        {
            //Chi hang row2 co modifier
            if (rowModifier.ContainsKey(row2))
            {
                int type = (rowModifier[row2] == LINE_MODIFIER_TYPE.Left) ? -1 : 1;
                MoveHorizontalLineBoard(row2, type);
            }
            //Khong hang nao co modifier
            else
            {
             //   Debug.Log("Two rows have no modifers.");
            }
        }
    }

    public void CheckColumnModifier(int column)
    {
        if (columnModifier.ContainsKey(column))
        {
            int type = (columnModifier[column] == LINE_MODIFIER_TYPE.Up) ? -1 : 1;
            MoveVerticalLineBoard(column, type);
        }
    }

    public void CheckColumnModifier(int column1, int column2)
    {
        if (columnModifier.ContainsKey(column1))
        {
            //Ca hai cot deu co modifiers
            if (columnModifier.ContainsKey(column2))
            {
                //Debug.Log("check column modifier -> contain key col1 and col2");
                int type1 = (columnModifier[column1] == LINE_MODIFIER_TYPE.Up) ? -1 : 1;
                int type2 = (columnModifier[column2] == LINE_MODIFIER_TYPE.Up) ? -1 : 1;
               // Debug.Log("Type 1:" + type1 + ", Type 2:" + type2);
                MoveVerticalLineBoard(column1, column2, type1, type2);
            }
            //Chi cot col1 co modifier
            else
            {
                int type = (columnModifier[column1] == LINE_MODIFIER_TYPE.Up) ? -1 : 1;
                MoveVerticalLineBoard(column1, type);
            }
        }
        else
        {
            //Chi cot col2 co modifier
            if (columnModifier.ContainsKey(column2))
            {
                int type = (columnModifier[column2] == LINE_MODIFIER_TYPE.Up) ? -1 : 1;
                MoveVerticalLineBoard(column2, type);
            }
            //Khong cot nao co modifier
            else
            {
           //     Debug.Log("Two columns have no modifers.");
            }
        }
    }

    #endregion

    #region Alter matrix
    int maxRandomTime = 10;
    int randomTime = 0;

    public bool RandomizeBoard()
    {
        if (controller.boardUserInput.firstCell != new Vector2Int(-1, -1))
        {
            Cell cell = GetCellFromPosition(controller.boardUserInput.firstCell);
            if (!cell.IsCellEmpty() && !cell.CheckHasBlockItemsInThisCell())
            {
                Debug.Log("Reset selected cell");
                cell.ResetCellScale();
                cell.ToggleCellScale(false);
                cell.ToggleCatZoomEffect(false);
               controller.boardUserInput.UnhighlightCat(controller.boardUserInput.firstCell);
            }
        }
        if (CheckBoardOnlyRemainsSameCats())
        {
            return false;
        }
        int[,] newBoard = RandomizeIntegerBoard(randomTime);
        while (TwoBoardEqual(boardIntergerMatrix, newBoard))
        {
            newBoard = RandomizeIntegerBoard(randomTime);
            randomTime++;
            if (randomTime > maxRandomTime)
            {
                Debug.Log("You random ten times and still get same board");
                break;
            }
        }
        randomTime = 0;
        SetUpNextNewBoard(newBoard, true);
      //  GamePlayController.Instance.gameplayUIController.RemoveAllHintUIOnCells();
        return true;
    }

    public List<PosPos> posPairs = new List<PosPos>();
    public List<Vector2Int> randInt = new List<Vector2Int>();

    private int[,] RandomizeIntegerBoard(int seed)
    {
        posPairs.Clear();
        randInt.Clear();

        int e = 0;
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                if (boardIntergerMatrix[i, j] > 0
                    && !GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Box)
                    && !GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Cage)
                    && !GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Key)
                    && !GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Spy))
                {
                    randInt.Add(new Vector2Int(i, j));
                    posPairs.Add(new PosPos(new Vector2Int(i, j), new Vector2Int(i, e)));
                }
            }
        }

        Utility.Shuffle(randInt, seed);

        int r = 0;
        foreach (var i in posPairs)
        {
            i.pos2 = randInt[r];
            r++;
        }

        int[,] newIntegerBoard = new int[boardHeight + 2, boardWidth + 2];
        for (int i = 0; i <= boardHeight + 1; i++)
        {
            for (int j = 0; j <= boardWidth + 1; j++)
            {
                newIntegerBoard[i, j] = boardIntergerMatrix[i, j];
            }
        }

        /*int run = 0;

        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                if (boardIntergerMatrix[i, j] > 0
                    && !GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Box)
                    && !GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Cage)
                    && !GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Key)
                    && !GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Spy))
                {
                    CatTeleportFromP1ToP2(posPairs[run], cells[run].pos, Utility.IntToCatType(boardIntergerMatrix[i, j]));
                    newIntegerBoard[i, j] = cells[run].value;
                    run++;
                }
            }
        }*/

        CatsTeleportFromP1ToP2(posPairs, ref newIntegerBoard);

        return newIntegerBoard;
    }

    public void MoveHorizontalLineBoard(int row, int type)
    {
        List<IntPos> listModifiedIntPos;
        int[,] newBoard = MoveHorizontalLineIntegerBoard(row, type, out listModifiedIntPos);

        SetUpNextNewBoard(newBoard, false);
    }

    public void MoveHorizontalLineBoard(int row1, int row2, int type1, int type2)
    {
        List<IntPos> listModifiedIntPos;
        int[,] newBoard = MoveHorizontalLineIntegerBoard(row1, row2, type1, type2, out listModifiedIntPos);

        SetUpNextNewBoard(newBoard, false);
    }

    public void MoveVerticalLineBoard(int column, int type)
    {
        List<IntPos> listModifiedIntPos;
        int[,] newBoard = MoveVerticalLineIntegerBoard(column, type, out listModifiedIntPos);

        SetUpNextNewBoard(newBoard, false);
    }

    public void MoveVerticalLineBoard(int column1, int column2, int type1, int type2)
    {
        List<IntPos> listModifiedIntPos;
        int[,] newBoard = MoveVerticalLineIntegerBoard(column1, column2, type1, type2, out listModifiedIntPos);

        SetUpNextNewBoard(newBoard, false);
    }

    private int[,] MoveHorizontalLineIntegerBoard(int row, int type, out List<IntPos> listModifiedIntPos)
    {
        listModifiedIntPos = new List<IntPos>();
        List<int> cats = new List<int>();
        List<Vector2Int> oldPositions = new List<Vector2Int>();

        int[,] newIntegerBoard = new int[boardHeight + 2, boardWidth + 2];
        for (int i = 0; i <= boardHeight + 1; i++)
        {
            for (int j = 0; j <= boardWidth + 1; j++)
            {
                newIntegerBoard[i, j] = boardIntergerMatrix[i, j];
            }
        }

        bool isThisNeedMove = true;
        bool isHaveEmptySpace = false;
        GetAllCatsInLine((type == -1) ? 1 : 2, row, ref isHaveEmptySpace, ref cats, ref oldPositions);
        if (cats.Count == 0)
        {
            isThisNeedMove = false;
        }
        if (isThisNeedMove)
        {
            isHaveEmptySpace = false;
            MoveCatsInLine((type == -1) ? 1 : 2, row, ref isHaveEmptySpace, cats, oldPositions, ref newIntegerBoard, ref listModifiedIntPos);
        }

        Debug.Log("Move horizonal one row at " + row);
        Utility.DebugIntegerMatrix(newIntegerBoard);

        return newIntegerBoard;
    }

    private int[,] MoveHorizontalLineIntegerBoard(int row1, int row2, int type1, int type2, out List<IntPos> listModifiedIntPos)
    {
        listModifiedIntPos = new List<IntPos>();
        List<int> cats = new List<int>();
        List<Vector2Int> oldPositions = new List<Vector2Int>();

        int[,] newIntegerBoard = new int[boardHeight + 2, boardWidth + 2];
        for (int i = 0; i <= boardHeight + 1; i++)
        {
            for (int j = 0; j <= boardWidth + 1; j++)
            {
                newIntegerBoard[i, j] = boardIntergerMatrix[i, j];
            }
        }

        bool isThisNeedMove = true;
        bool isHaveEmptySpace = false;
        GetAllCatsInLine((type1 == -1) ? 1 : 2, row1, ref isHaveEmptySpace, ref cats, ref oldPositions);
        if (cats.Count == 0)
        {
            isThisNeedMove = false;
        }
        if (isThisNeedMove)
        {
            isHaveEmptySpace = false;
            MoveCatsInLine((type1 == -1) ? 1 : 2, row1, ref isHaveEmptySpace, cats, oldPositions, ref newIntegerBoard, ref listModifiedIntPos);
        }

        cats.Clear();
        oldPositions.Clear();

        isThisNeedMove = true;
        isHaveEmptySpace = false;
        GetAllCatsInLine((type2 == -1) ? 1 : 2, row2, ref isHaveEmptySpace, ref cats, ref oldPositions);
        if (cats.Count == 0)
        {
            isThisNeedMove = false;
        }
        if (isThisNeedMove)
        {
            isHaveEmptySpace = false;
            MoveCatsInLine((type2 == -1) ? 1 : 2, row2, ref isHaveEmptySpace, cats, oldPositions, ref newIntegerBoard, ref listModifiedIntPos);
        }

        Debug.Log("Move horizonal 2 row at " + row1 + " and " + row2);
        Utility.DebugIntegerMatrix(newIntegerBoard);

        return newIntegerBoard;
    }

    private int[,] MoveVerticalLineIntegerBoard(int column, int type, out List<IntPos> listModifiedIntPos)
    {
        listModifiedIntPos = new List<IntPos>();
        List<int> cats = new List<int>();
        List<Vector2Int> oldPositions = new List<Vector2Int>();

        int[,] newIntegerBoard = new int[boardHeight + 2, boardWidth + 2];
        for (int i = 0; i <= boardHeight + 1; i++)
        {
            for (int j = 0; j <= boardWidth + 1; j++)
            {
                newIntegerBoard[i, j] = boardIntergerMatrix[i, j];
            }
        }

        bool isThisNeedMove = true;
        bool isHaveEmptySpace = false;
        GetAllCatsInLine((type == -1) ? 3 : 4, column, ref isHaveEmptySpace, ref cats, ref oldPositions);
        if (cats.Count == 0)
        {
            isThisNeedMove = false;
        }
        if (isThisNeedMove)
        {
            isHaveEmptySpace = false;
            MoveCatsInLine((type == -1) ? 3 : 4, column, ref isHaveEmptySpace, cats, oldPositions, ref newIntegerBoard, ref listModifiedIntPos);
        }

        return newIntegerBoard;
    }

    private int[,] MoveVerticalLineIntegerBoard(int column1, int column2, int type1, int type2, out List<IntPos> listModifiedIntPos)
    {
        listModifiedIntPos = new List<IntPos>();
        List<int> cats = new List<int>();
        List<Vector2Int> oldPositions = new List<Vector2Int>();

        int[,] newIntegerBoard = new int[boardHeight + 2, boardWidth + 2];
        for (int i = 0; i <= boardHeight + 1; i++)
        {
            for (int j = 0; j <= boardWidth + 1; j++)
            {
                newIntegerBoard[i, j] = boardIntergerMatrix[i, j];
            }
        }

        bool isThisNeedMove = true;
        bool isHaveEmptySpace = false;
        GetAllCatsInLine((type1 == -1) ? 3 : 4, column1, ref isHaveEmptySpace, ref cats, ref oldPositions);
        if (cats.Count == 0)
        {
            isThisNeedMove = false;
        }
        if (isThisNeedMove)
        {
            isHaveEmptySpace = false;
            MoveCatsInLine((type1 == -1) ? 3 : 4, column1, ref isHaveEmptySpace, cats, oldPositions, ref newIntegerBoard, ref listModifiedIntPos);
        }

        cats.Clear();
        oldPositions.Clear();

        isThisNeedMove = true;
        isHaveEmptySpace = false;
        GetAllCatsInLine((type2 == -1) ? 3 : 4, column2, ref isHaveEmptySpace, ref cats, ref oldPositions);
        if (cats.Count == 0)
        {
            isThisNeedMove = false;
        }
        if (isThisNeedMove)
        {
            isHaveEmptySpace = false;
            MoveCatsInLine((type2 == -1) ? 3 : 4, column2, ref isHaveEmptySpace, cats, oldPositions, ref newIntegerBoard, ref listModifiedIntPos);
        }

        return newIntegerBoard;
    }

    private bool TwoBoardEqual(int[,] board1, int[,] board2)
    {
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                if (board1[i, j] != board2[i, j])
                {
                    Debug.Log("Dif at (" + i + "," + j + ")");
                    return false;
                }
            }
        }
        return true;
    }

    private void GetAllCatsInLine(int dir, int linePos, ref bool isHaveEmptySpace, ref List<int> cats, ref List<Vector2Int> oldPositions)
    {
        //dir = 1: left, dir = 2: right, dir = 3: up, dir = 4: down
        switch (dir)
        {
            case 1:
                {
                    //Get all cats in row
                    for (int j = 1; j <= boardWidth; j++)
                    {
                        if (boardIntergerMatrix[linePos, j] == 0)
                        {
                            isHaveEmptySpace = true;
                        }
                        if (boardIntergerMatrix[linePos, j] > 0 && isHaveEmptySpace
                            && (!GetCellFromPosition(new Vector2Int(linePos, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Box)
                            && !GetCellFromPosition(new Vector2Int(linePos, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Cage)))
                        {
                            cats.Add(boardIntergerMatrix[linePos, j]);
                            oldPositions.Add(new Vector2Int(linePos, j));
                        }
                    }

                    string catsString = "";
                    foreach (var cat in cats)
                    {
                        catsString += cat.ToString() + " ";
                    }
                    Debug.Log("All cats in row:" + catsString);
                    break;
                }
            case 2:
                {
                    //Get all cats in row
                    for (int j = boardWidth; j >= 1; j--)
                    {
                        if (boardIntergerMatrix[linePos, j] == 0)
                        {
                            isHaveEmptySpace = true;
                        }
                        if (boardIntergerMatrix[linePos, j] > 0 && isHaveEmptySpace
                            && (!GetCellFromPosition(new Vector2Int(linePos, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Box)
                            && !GetCellFromPosition(new Vector2Int(linePos, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Cage)))
                        {
                            cats.Add(boardIntergerMatrix[linePos, j]);
                            oldPositions.Add(new Vector2Int(linePos, j));
                        }
                    }

                    string catsString = "";
                    foreach (var cat in cats)
                    {
                        catsString += cat.ToString() + " ";
                    }
                    Debug.Log("All cats in row:" + catsString);
                    break;
                }
            case 3:
                {
                    //Get all cats in row
                    for (int i = 1; i <= boardHeight; i++)
                    {
                        if (boardIntergerMatrix[i, linePos] == 0)
                        {
                            isHaveEmptySpace = true;
                        }
                        if (boardIntergerMatrix[i, linePos] > 0 && isHaveEmptySpace
                            && (!GetCellFromPosition(new Vector2Int(i, linePos)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Box)
                            && !GetCellFromPosition(new Vector2Int(i, linePos)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Cage)))
                        {
                            cats.Add(boardIntergerMatrix[i, linePos]);
                            oldPositions.Add(new Vector2Int(i, linePos));
                        }
                    }

                    string catsString = "";
                    foreach (var cat in cats)
                    {
                        catsString += cat.ToString() + " ";
                    }
                    Debug.Log("All cats in column:" + catsString);
                    break;
                }
            case 4:
                {
                    //Get all cats in row
                    for (int i = boardHeight; i >= 1; i--)
                    {
                        if (boardIntergerMatrix[i, linePos] == 0)
                        {
                            isHaveEmptySpace = true;
                        }
                        if (boardIntergerMatrix[i, linePos] > 0 && isHaveEmptySpace
                            && (!GetCellFromPosition(new Vector2Int(i, linePos)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Box)
                            && !GetCellFromPosition(new Vector2Int(i, linePos)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Cage)))
                        {
                            cats.Add(boardIntergerMatrix[i, linePos]);
                            oldPositions.Add(new Vector2Int(i, linePos));
                        }
                    }

                    string catsString = "";
                    foreach (var cat in cats)
                    {
                        catsString += cat.ToString() + " ";
                    }
                    Debug.Log("All cats in column:" + catsString);
                    break;
                }
        }
    }

    private void MoveCatsInLine(int dir, int linePos, ref bool isHaveEmptySpace, List<int> cats, List<Vector2Int> oldPositions, ref int[,] newIntegerBoard, ref List<IntPos> listModifiedIntPos)
    {
        DisableClickControlAtLine((dir == 1 || dir == 2) ? "row" : "col", linePos);
        switch (dir)
        {
            case 1:
                {
                    //remove all cats
                    for (int j = 1; j <= boardWidth; j++)
                    {
                        if (newIntegerBoard[linePos, j] == 0)
                        {
                            isHaveEmptySpace = true;
                        }
                        if (newIntegerBoard[linePos, j] > 0 && isHaveEmptySpace
                            && (!GetCellFromPosition(new Vector2Int(linePos, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Box)
                            && !GetCellFromPosition(new Vector2Int(linePos, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Cage)))
                        {
                            newIntegerBoard[linePos, j] = 0;
                            //GetCellFromPosition(new Vector2Int(linePos, j)).RemoveCell();
                        }
                    }
                    //add cats from left -> right (simulte left move)
                    int run = 0;
                    for (int j = 1; j <= boardWidth; j++)
                    {
                        if (newIntegerBoard[linePos, j] == 0)
                        {
                            newIntegerBoard[linePos, j] = cats[run];
                            if (oldPositions[run].y != j)
                            {
                                listModifiedIntPos.Add(new IntPos(new Vector2Int(linePos, j), cats[run]));
                                MoveCat(oldPositions[run], new Vector2Int(linePos, j), Utility.IntToCatType(cats[run]));
                                GetCellFromPosition(new Vector2Int(linePos, j)).FlipCatTo(-1);
                            }
                            run++;
                            if (run > cats.Count - 1)
                            {
                                break;
                            }
                        }
                    }
                    break;
                }
            case 2:
                {
                    //remove all cats
                    for (int j = boardWidth; j >= 1; j--)
                    {
                        if (newIntegerBoard[linePos, j] == 0)
                        {
                            isHaveEmptySpace = true;
                        }
                        if (newIntegerBoard[linePos, j] > 0 && isHaveEmptySpace
                            && (!GetCellFromPosition(new Vector2Int(linePos, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Box)
                            && !GetCellFromPosition(new Vector2Int(linePos, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Cage)))
                        {
                            newIntegerBoard[linePos, j] = 0;
                            //GetCellFromPosition(new Vector2Int(linePos, j)).RemoveCell();
                        }
                    }

                    int run = 0;
                    for (int j = boardWidth; j >= 1; j--)
                    {
                        if (newIntegerBoard[linePos, j] == 0)
                        {
                            newIntegerBoard[linePos, j] = cats[run];
                            if (oldPositions[run].y != j)
                            {
                                listModifiedIntPos.Add(new IntPos(new Vector2Int(linePos, j), cats[run]));
                                MoveCat(oldPositions[run], new Vector2Int(linePos, j), Utility.IntToCatType(cats[run]));
                                GetCellFromPosition(new Vector2Int(linePos, j)).FlipCatTo(1);
                            }
                            run++;
                            if (run > cats.Count - 1)
                            {
                                break;
                            }
                        }
                    }
                    break;
                }
            case 3:
                {
                    //remove all cats
                    for (int i = 1; i <= boardHeight; i++)
                    {
                        if (newIntegerBoard[i, linePos] == 0)
                        {
                            isHaveEmptySpace = true;
                        }
                        if (newIntegerBoard[i, linePos] > 0 && isHaveEmptySpace
                            && (!GetCellFromPosition(new Vector2Int(i, linePos)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Box)
                            && !GetCellFromPosition(new Vector2Int(i, linePos)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Cage)))
                        {
                            newIntegerBoard[i, linePos] = 0;
                            //GetCellFromPosition(new Vector2Int(i, linePos)).RemoveCell();
                        }
                    }
                    //up
                    int run = 0;
                    for (int i = 1; i <= boardHeight; i++)
                    {
                        if (newIntegerBoard[i, linePos] == 0)
                        {
                            newIntegerBoard[i, linePos] = cats[run];

                            if (oldPositions[run].x != i)
                            {
                                listModifiedIntPos.Add(new IntPos(new Vector2Int(i, linePos), cats[run]));
                                MoveCat(oldPositions[run], new Vector2Int(i, linePos), Utility.IntToCatType(cats[run]));
                            }
                            run++;
                            if (run > cats.Count - 1)
                            {
                                break;
                            }
                        }
                    }
                    break;
                }
            case 4:
                {
                    //remove all cats
                    for (int i = boardHeight; i >= 1; i--)
                    {
                        if (newIntegerBoard[i, linePos] == 0)
                        {
                            isHaveEmptySpace = true;
                        }
                        if (newIntegerBoard[i, linePos] > 0 && isHaveEmptySpace
                            && (!GetCellFromPosition(new Vector2Int(i, linePos)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Box)
                            && !GetCellFromPosition(new Vector2Int(i, linePos)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Cage)))
                        {
                            newIntegerBoard[i, linePos] = 0;
                            //GetCellFromPosition(new Vector2Int(i, linePos)).RemoveCell();
                        }
                    }
                    //up
                    int run = 0;
                    for (int i = boardHeight; i >= 1; i--)
                    {
                        if (newIntegerBoard[i, linePos] == 0)
                        {
                            newIntegerBoard[i, linePos] = cats[run];
                            if (oldPositions[run].x != i)
                            {
                                listModifiedIntPos.Add(new IntPos(new Vector2Int(i, linePos), cats[run]));
                                MoveCat(oldPositions[run], new Vector2Int(i, linePos), Utility.IntToCatType(cats[run]));
                            }
                            run++;
                            if (run > cats.Count - 1)
                            {
                                break;
                            }
                        }
                    }
                    break;
                }
        }

    }

    private void MoveCat(Vector2Int p1, Vector2Int p2, CAT_TYPE catType)
    {
        CatMoveFromP1ToP2(p1, p2, catType);
    }

    #region Cat move animations

    private void CatMoveFromP1ToP2(Vector2Int p1, Vector2Int p2, CAT_TYPE catType)
    {
        Transform cat = GetCellFromPosition(p1).GetCat();
    
        Transform targetTransform = cellGameObjectsInScene[p2.x, p2.y].transform;
    
        if (cat == null)
        {
            Debug.LogError("You're trying to move cat of an empty cell at position " + p1);
            return;
        }

        movingCats.Enqueue(p1);

        cat.SetParent(gamePanel);

        Vector3 beginPoint = cat.position;
        Vector3 endPoint = cellWorldPointPositions[p2.x, p2.y];

        SkeletonGraphic catSkeletonGraphic = cat.GetComponent<SkeletonGraphic>();
        //catSkeletonGraphic.Initialize(true);
        //catSkeletonGraphic.SetMaterialDirty();

        //CatAnimationData catAnimationData = Utility.GetCatAnimationData(catType);
        //AnimationReferenceAsset idle = catAnimationData.idle;
        //AnimationReferenceAsset jumping = catAnimationData.jumping;
        //AnimationReferenceAsset grounding = catAnimationData.grounding;

        float catSpeed = 6f;

        if (beginPoint.x > endPoint.x)
        {
            //Reverse transform
            var tempScale = this.transform.localScale;
            tempScale.x *= -1;
            cat.transform.localScale = tempScale;
        }

        cat.DOKill();
     
        float distance = Vector2.Distance(beginPoint, endPoint);
        //float moveTime = Mathf.Pow(distance, (distance > 1) ? 2 : 1) / catSpeed;
        float moveTime = distance / catSpeed;
        //    catSkeletonGraphic.AnimationState.SetAnimation(0, idle, false);

        //cat.DOJump(endPoint, 0.25f, 1, moveTime, false).SetDelay(0.12f).OnStart(() =>
        //{
        //    //catSkeletonGraphic.timeScale = 1.5f;
        // //   catSkeletonGraphic.AnimationState.SetAnimation(0, jumping, false);
        //}).OnComplete(() =>
        //{

        //});
        //cat.GetComponentInParent<Cell>().miniGameBird.transform.DOJump(endPoint, 0.25f, 1, 0.5f, false);
        cat.DOJump(endPoint, 0.25f, 1, 0.5f, false);
      
        cat.SetParent(targetTransform);
        cat.localScale = new Vector2(1, 1);
        GetCellFromPosition(p1).RemoveCell();
        GetCellFromPosition(p2).EnableCell();
        GetCellFromPosition(p2).SetCellCatType(Utility.CatTypeToInt(catType));
    }

    private void CatsTeleportFromP1ToP2(List<PosPos> posPairs, ref int[,] newBoard)
    {
        foreach (var i in posPairs)
        {
            newBoard[i.pos2.x, i.pos2.y] = GetCellFromPosition(i.pos1).GetCellCatType();

            Transform cat = GetCellFromPosition(i.pos1).GetCat();
            Transform targetTransform = cellGameObjectsInScene[i.pos2.x, i.pos2.y].transform;

            if (cat == null)
            {
                Debug.LogError("You're trying to teleport cat of an empty cell at position " + i.pos1);
            }
            cat.SetParent(gamePanel);
            Vector3 endPoint = cellWorldPointPositions[i.pos2.x, i.pos2.y];
            cat.position = endPoint;
            cat.SetParent(targetTransform);
        }

        List<int> oldCellCatTypeList = new List<int>();
        foreach (var i in posPairs)
        {
            oldCellCatTypeList.Add(GetCellFromPosition(i.pos1).GetCellCatType());
        }

        foreach (var i in posPairs)
        {
            Debug.Log("Set " + i.pos2 + " = " + oldCellCatTypeList[posPairs.IndexOf(i)]);
            GetCellFromPosition(i.pos2).SetCellCatType(oldCellCatTypeList[posPairs.IndexOf(i)]);
        }

    }

    #endregion

    private void SetUpNextNewBoard(int[,] newBoard, bool isImmediate)
    {
        Debug.Log("SET UP NEXT NEW BOARD");
        Utility.DebugIntegerMatrix(boardIntergerMatrix);
        Utility.DebugIntegerMatrix(newBoard);

        if (TwoBoardEqual(newBoard, boardIntergerMatrix))
        {
            Debug.Log("Board doesn't change !");
            DoAllActionsInGameplayActions();
            return;
        }

        for (int i = 0; i <= boardHeight + 1; i++)
        {
            for (int j = 0; j <= boardWidth + 1; j++)
            {
                boardIntergerMatrix[i, j] = newBoard[i, j];
            }
        }
    }

    public void ChangeBoardLineModifiersDirection(string dir)
    {
        rowModifier.Clear();
        columnModifier.Clear();
        switch (dir)
        {
            case "l":
                {
                    for (int i = 1; i <= boardHeight; i++)
                    {
                        rowModifier.Add(i, LINE_MODIFIER_TYPE.Left);
                    }
                    for (int i = 1; i <= boardHeight; i++)
                    {
                        CheckRowModifier(i);
                    }
                    break;
                }
            case "r":
                {
                    for (int i = 1; i <= boardHeight; i++)
                    {
                        rowModifier.Add(i, LINE_MODIFIER_TYPE.Right);
                    }
                    for (int i = 1; i <= boardHeight; i++)
                    {
                        CheckRowModifier(i);
                    }
                    break;
                }
            case "u":
                {
                    for (int i = 1; i <= boardWidth; i++)
                    {
                        columnModifier.Add(i, LINE_MODIFIER_TYPE.Up);
                    }
                    for (int i = 1; i <= boardWidth; i++)
                    {
                        CheckColumnModifier(i);
                    }
                    break;
                }
            //down
            default:
                {
                    for (int i = 1; i <= boardWidth; i++)
                    {
                        columnModifier.Add(i, LINE_MODIFIER_TYPE.Down);
                    }
                    for (int i = 1; i <= boardWidth; i++)
                    {
                        CheckColumnModifier(i);
                    }
                    break;
                }
        }
    }

    public void SearchBoardAndOpenCage(int id)
    {
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                Vector2Int cellPos = new Vector2Int(i, j);
                Cell cell = GetCellFromPosition(cellPos);
                if (!cell.IsCellEmpty())
                {
                    //CatItemManager catItemManager = cell.GetCatItemManager();
                    //if (catItemManager != null)
                    //{
                    //    if (catItemManager.GetCageID() == id)
                    //    {
                    //        catItemManager.OpenCage();
                    //    }
                    //}
                }
            }
        }
    }

    public void SearchBoardAndBombCountdown()
    {
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                Vector2Int cellPos = new Vector2Int(i, j);
                Cell cell = GetCellFromPosition(cellPos);
                if (!cell.IsCellEmpty())
                {
              //      CatItemManager catItemManager = cell.GetCatItemManager();
                    //if (catItemManager != null && catItemManager.CheckHasItemInList(CELL_ITEM_TYPE.Bomb))
                    //{
                    //    catItemManager.SetBombCounter(catItemManager.GetBombCounter() - 1);
                    //}
                }
            }
        }
    }

    public void OpenAllCages()
    {
      controller.board.DisableClickControlAllCellsOnBoard();
      controller.board.isCompletelyDisableControl = true;
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                Vector2Int cellPos = new Vector2Int(i, j);
                Cell cell = GetCellFromPosition(cellPos);
                if (!cell.IsCellEmpty())
                {
                    //CatItemManager catItemManager = cell.GetCatItemManager();
                    //if (catItemManager != null)
                    //{
                    //    if (catItemManager.CheckHasItemInList(CELL_ITEM_TYPE.Key))
                    //    {
                    //        catItemManager.RemoveItem(CELL_ITEM_TYPE.Key);
                    //    }
                    //    if (catItemManager.CheckHasItemInList(CELL_ITEM_TYPE.Cage))
                    //    {
                    //        catItemManager.PowerupDestroyCage();
                    //    }
                    //}
                }
            }
        }
    }

    public void OpenAllBoxes()
    {
     controller.board.DisableClickControlAllCellsOnBoard();
     controller.board.isCompletelyDisableControl = true;
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                Vector2Int cellPos = new Vector2Int(i, j);
                Cell cell = GetCellFromPosition(cellPos);
                if (!cell.IsCellEmpty())
                {
                    //CatItemManager catItemManager = cell.GetCatItemManager();
                    //if (catItemManager != null)
                    //{
                    //    if (catItemManager.CheckHasItemInList(CELL_ITEM_TYPE.Box))
                    //    {
                    //        catItemManager.PowerupDestroyBox();
                    //    }
                    //}
                }
            }
        }
    }

    public void DestroyAllBombs()
    {
       controller.board.DisableClickControlAllCellsOnBoard();
       controller.board.isCompletelyDisableControl = true;
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                Vector2Int cellPos = new Vector2Int(i, j);
                Cell cell = GetCellFromPosition(cellPos);
                if (!cell.IsCellEmpty())
                {
                    //CatItemManager catItemManager = cell.GetCatItemManager();
                    //if (catItemManager != null)
                    //{
                    //    if (catItemManager.CheckHasItemInList(CELL_ITEM_TYPE.Bomb))
                    //    {
                    //        catItemManager.PowerupRemoveBomb();
                    //    }
                    //}
                }
            }
        }
    }

    #endregion

    #region Board State

    public void OnMoveCompleted()
    {
        //CheckBoardState();
    }

    public void CheckBoardState()
    {
        //Check win
        if (CheckThereIsNoAnimalCellLeft())
        {
            OnWinLevel();
        }
        //Check others
        //Check bomb
    }

    public void OnWinLevel()
    {
        if (!ConnectBirdMGController.FirstOpenConnectBirdMNGame)
        {
            ConnectBirdMGController.FirstOpenConnectBirdMNGame = true;
            Win();
            controller.HandleCoolDown();
        }
        else
        {
            //  controller.btnRetry.gameObject.SetActive(false);
            controller.btnRetry.interactable = false;
            //Invoke("Win", 1.3f);
            Win();
            Debug.Log("You Win !");
        }    
     
    }
    public void Win()
    {
        controller.CurrentLevelMiniGame++;
        controller.BroadInit();
      
    }


    public void OnLoseLevel()
    {
     //   GamePlayController.Instance.gameState = GameState.Lose;
//        LosePopup.Setup().Show();
        Debug.Log("You Lose !");
    }

    private bool CheckThereIsNoAnimalCellLeft()
    {
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                if (boardIntergerMatrix[i, j] > 0) return false;
            }
        }
        return true;
    }

    public bool CheckBoardHasLineModifiers()
    {
        if (rowModifier.Count == 0 && columnModifier.Count == 0)
        {
            return false;
        }
        return true;
    }

    private bool CheckBoardOnlyRemainsSameCats()
    {
        Dictionary<int, int> boardCells = new Dictionary<int, int>();
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                if (boardIntergerMatrix[i, j] > 0
                    && !GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Box)
                    && !GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Cage)
                    && !GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Key)
                    && !GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(CELL_ITEM_TYPE.Spy))
                {
                    if (!boardCells.ContainsKey(boardIntergerMatrix[i, j]))
                    {
                        boardCells.Add(boardIntergerMatrix[i, j], 1);
                    }
                }
            }
        }
        if (boardCells.Count <= 1)
        {
            return true;
        }
        return false;
    }

    public bool CheckBoardHasThisItem(CELL_ITEM_TYPE itemType)
    {
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                if (GetCellFromPosition(new Vector2Int(i, j)).CheckHasItemInThisCell(itemType))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool isDoingAllActions = false;

    public void DoAllActionsInGameplayActions()
    {
        if (!isDoingAllActions)
        {
            isDoingAllActions = true;
            foreach (var i in gameplayActions)
            {
                GameplayAction currentAction = i;
                switch (currentAction.gameplayActionType)
                {
                    case GAMEPLAY_ACTION_TYPE.ChangeBoardLineModifiers:
                        {
                            ChangeBoardLineModifiersDirection(currentAction.description);
                            break;
                        }
                    case GAMEPLAY_ACTION_TYPE.OpenCage:
                        {
                            int keyId = Int32.Parse(currentAction.description);
                            SearchBoardAndOpenCage(keyId);
                            break;
                        }
                    case GAMEPLAY_ACTION_TYPE.OpenBox:
                        {
                            string[] posString = currentAction.description.Split(',');
                            Vector2Int pos = new Vector2Int(Int32.Parse(posString[0]), Int32.Parse(posString[1]));
                            Cell cell = GetCellFromPosition(pos);
                            if (cell != null)
                            {
                                if (cell.CheckHasItemInThisCell(CELL_ITEM_TYPE.Box))
                                {
                      
                                }
                                else
                                {
                                    Debug.Log("You try to open box at this cell " + cell.GetCellPosition() + " but it has no box !");
                                }
                            }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
             //   Debug.Log("Action " + currentAction.gameplayActionType + " " + currentAction.description + " is done !");
            }



         if(!CheckThereIsConnectableCells() && !CheckThereIsNoAnimalCellLeft())
            {
                controller.btnRetry.GetComponent<BtnRetryInMiniGame>().ScaleInOut() ;

            }
      





            CheckBoardState();
            if (!isCompletelyDisableControl)
            {
                EnableClickControlAllCellsOnBoard();
            }
         //   Debug.Log("All actions are done and dequeued");
        }
    }

    public void EnqueueActionToGameplayActions(GameplayAction gameplayAction)
    {
        this.gameplayActions.Enqueue(gameplayAction);
    }

    public void DequeueGameplayActions()
    {
        this.gameplayActions.Dequeue();
    }

    public bool isCompletelyDisableControl = false;

    public void EnableClickControlMatrixAt(Vector2Int cellPos)
    {
      //  Debug.Log("Enable click at " + cellPos);
        clickControlMatrix[cellPos.x, cellPos.y] = true;
    }
    public void DisableClickControlMatrixAt(Vector2Int cellPos)
    {
      //  Debug.Log("Disable click at " + cellPos);
        clickControlMatrix[cellPos.x, cellPos.y] = false;
    }
    public void EnableClickControlAllCellsOnBoard()
    {
     //   Debug.Log("Enable all click");
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                clickControlMatrix[i, j] = true;
            }
        }
    }

    public void DisableClickControlAllCellsOnBoard()
    {
      //  Debug.Log("Disable all click");
        for (int i = 1; i <= boardHeight; i++)
        {
            for (int j = 1; j <= boardWidth; j++)
            {
                clickControlMatrix[i, j] = false;
            }
        }
    }

    public void EnableClickControlAtLine(string type, int linePos)
    {
      //  Debug.Log("Enable click at " + type + " " + linePos);
        //row
        if (type == "row")
        {
            for (int i = 1; i <= boardWidth; i++)
            {
                clickControlMatrix[linePos, i] = true;
            }
        }
        //col
        else
        {
            for (int i = 1; i <= boardHeight; i++)
            {
                clickControlMatrix[i, linePos] = true;
            }
        }
    }

    public void DisableClickControlAtLine(string type, int linePos)
    {
     //   Debug.Log("Disable click at " + type + " " + linePos);
        //row
        if (type == "row")
        {
            for (int i = 1; i <= boardWidth; i++)
            {
                clickControlMatrix[linePos, i] = false;
            }
        }
        //col
        else
        {
            for (int i = 1; i <= boardHeight; i++)
            {
                clickControlMatrix[i, linePos] = false;
            }
        }
    }

    #endregion

    public void ResetBoard()
    {
        StopAllCoroutines();
        foreach (GameObject item in lsCell)
        {
            Destroy(item);
        }
        lsCell.Clear();
    }
    public void TestBooster()
    {
     var a =    RandomizeBoard();
    }
    private void OnDisable()
    {
        if (m_CoWaitForAllCellPositions != null)
        {
            StopCoroutine(m_CoWaitForAllCellPositions);
            m_CoWaitForAllCellPositions = null;
        }
    }
}

[Serializable]
public class IntPos
{
    public Vector2Int pos;
    public int value;

    public IntPos(Vector2Int pos, int value)
    {
        this.pos = pos;
        this.value = value;
    }
}

[Serializable]
public class PosPos
{
    public Vector2Int pos1;
    public Vector2Int pos2;

    public PosPos(Vector2Int pos1, Vector2Int pos2)
    {
        this.pos1 = pos1;
        this.pos2 = pos2;
    }
}