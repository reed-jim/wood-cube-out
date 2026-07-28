using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public enum AntsFlowInputMode
{
    Normal,
    SelectTile
}

public class WaterFlowGameInput : Singleton<WaterFlowGameInput>
{
    // Scene-specific - khong DontDestroyOnLoad
    protected override bool ShouldPersist() => false;

    [SerializeField] private Camera mainCamera;

    public AntsFlowInputMode inputMode;

    public static event Action<Color> useClearOneColorBoosterEvent;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(
                worldPoint,
                Vector2.zero,
                0f
            );

            if (IsClickedOnUI())
            {
                return;
            }

            if (inputMode == AntsFlowInputMode.SelectTile)
            {
                Vector3 touchPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);

                Vector3Int cellPos = AntsFlow.TileManager.Ins.baseTilemap.WorldToCell(touchPoint);

                float smallestDistance = float.MaxValue;
                AntsFlow.BaseTile selectedTile = null;

                for (int i = 0; i < AntsFlow.TileManager.Ins.tiles.Count; i++)
                {
                    AntsFlow.BaseTile tile = AntsFlow.TileManager.Ins.tiles[i];

                    if (tile.isDone)
                    {
                        continue;
                    }

                    Vector3 tilePosition = AntsFlow.TileManager.Ins.GetTilePosition(tile);

                    float distance = Vector2.Distance(tilePosition, touchPoint);

                    if (distance < smallestDistance)
                    {
                        smallestDistance = distance;
                        selectedTile = tile;
                    }
                }

                float tileSize = GamePersistentVariable.obstacleScaleFactor;

                if (smallestDistance > tileSize)
                {
                    selectedTile = null;
                }

                if (selectedTile != null)
                {
                    Color faction = selectedTile.Faction;

                    useClearOneColorBoosterEvent?.Invoke(faction);

                    inputMode = AntsFlowInputMode.Normal;
                }

                // for (int i = 0; i < AntsFlow.TileManager.Ins.tilemaps.Length; i++)
                // {
                //     Sprite sprite = AntsFlow.TileManager.Ins.tilemaps[i].GetSprite(cellPos);

                //     if (sprite != null)
                //     {
                //         Color faction = FactionMapper.Ins.GetFaction(sprite);

                //         useClearOneColorBoosterEvent?.Invoke(faction);

                //         inputMode = AntsFlowInputMode.Normal;

                //         break;
                //     }
                // }
            }

            if (hit.collider != null)
            {
                if (inputMode == AntsFlowInputMode.Normal)
                {
                    WashingMachineItem washingMachineItem = hit.collider.GetComponent<WashingMachineItem>();

                    if (washingMachineItem != null)
                    {
                        washingMachineItem.Select();
                    }
                }
                else if (inputMode == AntsFlowInputMode.SelectTile)
                {
                    // AntsFlow.BaseTile tile = hit.collider.GetComponent<AntsFlow.BaseTile>();

                    // if (tile != null && !tile.isAnimating)
                    // {
                    //     tile.ClearAllTilesOfTheSameColor();

                    //     inputMode = AntsFlowInputMode.Normal;
                    // }
                }
            }
        }
    }

    private bool IsClickedOnUI()
    {
        bool res;
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count > 0)
        {
            res = true;
        }
        else
        {
            res = false;
        }

        return res;
    }
}
