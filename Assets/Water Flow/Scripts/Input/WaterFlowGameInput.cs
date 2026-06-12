using UnityEngine;

public enum AntsFlowInputMode
{
    Normal,
    SelectTile
}

public class WaterFlowGameInput : Singleton<WaterFlowGameInput>
{
    public AntsFlowInputMode inputMode;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(
            worldPoint,
            Vector2.zero,
            0f
        );

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
                    AntsFlow.BaseTile tile = hit.collider.GetComponent<AntsFlow.BaseTile>();

                    if (tile != null && !tile.isAnimating)
                    {
                        tile.ClearAllTilesOfTheSameColor();

                        inputMode = AntsFlowInputMode.Normal;
                    }
                }
            }
        }
    }
}
