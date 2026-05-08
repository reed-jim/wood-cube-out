using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using UnityEngine;

public class AutoplayManager : Singleton<AutoplayManager>
{
    public float delayPerTap;

    public void StartAutoplay()
    {
        StartCoroutine(AutoplayLevel());
    }

    private IEnumerator AutoplayLevel()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(delayPerTap);

        List<BaseTile> poolTile = new List<BaseTile>();

        while (true)
        {
            for (int i = 0; i < TileManager.Ins.tiles.Count; i++)
            {
                BaseTile tile = TileManager.Ins.tiles[i];

                if (!tile.IsOnSlot)
                {
                    poolTile.Add(tile);
                }
            }

            poolTile.Shuffle();

            BaseTile selectedTile = poolTile.First();

            selectedTile.Select();

            GamePersistentVariable.isAutoTestingLevel = true;

            yield return waitForSeconds;
        }
    }
}
