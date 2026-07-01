using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using Dreamteck.Utilities;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum LevelElement
{
    NormalAntBox,
    HiddenAntBox,
    AntBoxLink,
    FreezedAntBox
}

namespace AntsFlow
{

    public class SpecialElementManager : Singleton<SpecialElementManager>
    {
        public LevelElement nextLevelElement;
        public SpecialElementConfig specialElementConfig;

        public int nextFreezeAntBoxRequiredNumber;
        public List<AntBoxLink> antBoxLinks;

        public Sprite hiddenTileSprite;

        public void Setup(AntsFlow.LevelItem levelItem)
        {
            specialElementConfig = levelItem.specialElementConfig;

            for (int i = 0; i < antBoxLinks.Count; i++)
            {
                ObjectPoolingEverything.ReturnToPool(GameConstants.antBoxLink, antBoxLinks[i].gameObject);
            }

            antBoxLinks.Clear();
        }

        public void SpawnAntBoxLink()
        {
            if (!specialElementConfig.doubleAntBoxConfig.IsValidToSpawn(WashingMachineManager.Ins.numAntBoxSpawned))
            {
                return;
            }

            if (antBoxLinks.Count >= 5)
            {
                return;
            }

            List<WashingMachineItem> validAntBoxesToLink = new List<WashingMachineItem>();

            for (int i = 0; i < WashingMachineManager.Ins.washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = WashingMachineManager.Ins.washingMachineStartSlots[i];

                if (washingMachineStartSlot.row < 2)
                {
                    continue;
                }

                WashingMachineItem washingMachine = WashingMachineManager.Ins.washingMachineStartSlots[i].washingMachine;

                if (
                    washingMachine != null &&
                    washingMachine is not HiddenAntBox &&
                    washingMachine is not FreezedAntBox
                )
                {
                    if (washingMachine.numSlotRequired == 1)
                    {
                        validAntBoxesToLink.Add(washingMachine);
                    }
                }
            }

            if (validAntBoxesToLink.Count >= 2)
            {
                AntBoxLink antBoxLink = ObjectPoolingEverything.GetFromPool<AntBoxLink>(GameConstants.antBoxLink);

                validAntBoxesToLink.Shuffle();

                antBoxLink.Setup(validAntBoxesToLink.Take(2).ToArray());

                antBoxLinks.Add(antBoxLink);

                specialElementConfig.doubleAntBoxConfig.RemoveOneSpecialElementInPool();
            }
        }

        public void RandomizeNextLevelElement()
        {
            if (specialElementConfig.hiddenAntBoxConfig.IsValidToSpawn(WashingMachineManager.Ins.numAntBoxSpawned))
            {
                nextLevelElement = LevelElement.HiddenAntBox;

                specialElementConfig.hiddenAntBoxConfig.RemoveOneSpecialElementInPool();
            }
            else if (specialElementConfig.frozenAntBoxConfig.IsValidToSpawn(WashingMachineManager.Ins.numAntBoxSpawned))
            {
                nextLevelElement = LevelElement.FreezedAntBox;

                nextFreezeAntBoxRequiredNumber = specialElementConfig.frozenAntBoxConfig.numBoxToUnfreeze[0];
                specialElementConfig.frozenAntBoxConfig.numBoxToUnfreeze.RemoveAt(0);

                specialElementConfig.frozenAntBoxConfig.RemoveOneSpecialElementInPool();
            }
            else
            {
                nextLevelElement = LevelElement.NormalAntBox;
            }
        }

        public LevelElement GetNextLevelElement(int rowIndex, int columnIndex)
        {
            if (specialElementConfig.hiddenAntBoxConfig.IsValidToSpawn(WashingMachineManager.Ins.numAntBoxSpawned))
            {
                specialElementConfig.hiddenAntBoxConfig.RemoveOneSpecialElementInPool();

                return LevelElement.HiddenAntBox;
            }
            else if (specialElementConfig.frozenAntBoxConfig.IsValidToSpawn(WashingMachineManager.Ins.numAntBoxSpawned))
            {
                try
                {
                    nextFreezeAntBoxRequiredNumber = specialElementConfig.frozenAntBoxConfig.numBoxToUnfreeze[0];
                    specialElementConfig.frozenAntBoxConfig.numBoxToUnfreeze.RemoveAt(0);
                }
                catch (System.Exception)
                {
                    nextFreezeAntBoxRequiredNumber = 2;
                }

                int maxValue = GetMaxValueFreezedAntBox(rowIndex, columnIndex);

                if (maxValue == 0)
                {
                    return LevelElement.NormalAntBox;
                }
                else
                {
                    nextFreezeAntBoxRequiredNumber = Mathf.Min(nextFreezeAntBoxRequiredNumber, maxValue);
                }

                specialElementConfig.frozenAntBoxConfig.RemoveOneSpecialElementInPool();

                return LevelElement.FreezedAntBox;
            }
            else
            {
                return LevelElement.NormalAntBox;
            }
        }

        private int GetMaxValueFreezedAntBox(int rowIndex, int columnIndex)
        {
            int maxFreezedAntBoxRequiredNumberToUnfreeze = 0;

            for (int i = 0; i < WashingMachineManager.Ins.washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = WashingMachineManager.Ins.washingMachineStartSlots[i];

                if (washingMachineStartSlot.column != columnIndex)
                {
                    WashingMachineItem antBox = washingMachineStartSlot.washingMachine;

                    if (antBox != null && antBox is not FreezedAntBox)
                    {
                        maxFreezedAntBoxRequiredNumberToUnfreeze++;
                    }
                }
                else if (washingMachineStartSlot.row < rowIndex)
                {
                    WashingMachineItem antBox = washingMachineStartSlot.washingMachine;

                    if (antBox != null && antBox is not FreezedAntBox)
                    {
                        maxFreezedAntBoxRequiredNumberToUnfreeze++;
                    }
                }
            }

            return maxFreezedAntBoxRequiredNumberToUnfreeze;
        }

        public void FillHiddenTiles(Tilemap tilemap, int width, int height)
        {
            int clusterSize = 50;
            int[] indexesToFill = GetRandomCluster(width, height, clusterSize);

            for (int i = 0; i < indexesToFill.Length; i++)
            {
                int index = indexesToFill[i];
                BaseTile tile = TileManager.Ins.tiles[index];

                Vector3Int cellPos = tile.tilemapPosition;

                Tile tilemapTile = ScriptableObject.CreateInstance<Tile>();
                tilemapTile.sprite = hiddenTileSprite;

                tilemap.SetTile(cellPos, tilemapTile);

                tile.isHidden = true;
            }
        }

        public static int[] GetRandomCluster(int width, int height, int clusterSize)
        {
            int totalTiles = width * height;
            clusterSize = Mathf.Min(clusterSize, totalTiles);

            List<int> cluster = new();
            HashSet<int> visited = new();
            List<int> frontier = new();

            int start = Random.Range(0, totalTiles);

            cluster.Add(start);
            visited.Add(start);
            frontier.Add(start);

            while (cluster.Count < clusterSize && frontier.Count > 0)
            {
                int current = frontier[Random.Range(0, frontier.Count)];

                int x = current % width;
                int y = current / width;

                int[] neighbors =
                {
                    (y > 0) ? current - width : -1,              // Down
                    (y < height - 1) ? current + width : -1,     // Up
                    (x > 0) ? current - 1 : -1,                  // Left
                    (x < width - 1) ? current + 1 : -1           // Right
                };

                // Shuffle neighbors
                for (int i = neighbors.Length - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (neighbors[i], neighbors[j]) = (neighbors[j], neighbors[i]);
                }

                bool expanded = false;

                foreach (int next in neighbors)
                {
                    if (next == -1 || visited.Contains(next))
                        continue;

                    visited.Add(next);
                    cluster.Add(next);
                    frontier.Add(next);

                    expanded = true;
                    break;
                }

                if (!expanded)
                    frontier.Remove(current);
            }

            return cluster.ToArray();
        }
    }
}
