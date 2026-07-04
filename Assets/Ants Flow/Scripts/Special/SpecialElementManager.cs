using System;
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
        public Sprite wallTileSprite;

        public IntVariable currentLevel;
        public int[] levelsShowSpecialElementIntroduction;

        public static event Action<int> showSpecialElementIntroduction;

        public void Setup(AntsFlow.LevelItem levelItem)
        {
            specialElementConfig = levelItem.specialElementConfig;

            for (int i = 0; i < antBoxLinks.Count; i++)
            {
                ObjectPoolingEverything.ReturnToPool(GameConstants.antBoxLink, antBoxLinks[i].gameObject);
            }

            antBoxLinks.Clear();

            if (levelsShowSpecialElementIntroduction.Contains(currentLevel.Value))
            {
                showSpecialElementIntroduction?.Invoke(Array.IndexOf(levelsShowSpecialElementIntroduction, currentLevel.Value));
            }
        }

        public void SpawnAntBoxLinkFirstAntBoxes(int order)
        {
            if (!specialElementConfig.doubleAntBoxConfig.IsValidToSpawn(order))
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

                antBoxLink.Setup(validAntBoxesToLink.Take(2).ToArray());

                antBoxLinks.Add(antBoxLink);

                specialElementConfig.doubleAntBoxConfig.RemoveOneSpecialElementInPool();
            }
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

        public int GetMaxValueFreezedAntBox(int rowIndex, int columnIndex)
        {
            int maxFreezedAntBoxRequiredNumberToUnfreeze = 0;

            for (int i = 0; i < WashingMachineManager.Ins.washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = WashingMachineManager.Ins.washingMachineStartSlots[i];

                if (washingMachineStartSlot.column != columnIndex)
                {
                    WashingMachineItem antBox = washingMachineStartSlot.washingMachine;

                    if (antBox != null && !antBox.isDouble && antBox is not FreezedAntBox)
                    {
                        maxFreezedAntBoxRequiredNumberToUnfreeze++;
                    }
                }
                else if (washingMachineStartSlot.row < rowIndex)
                {
                    WashingMachineItem antBox = washingMachineStartSlot.washingMachine;

                    if (antBox != null && !antBox.isDouble && antBox is not FreezedAntBox)
                    {
                        maxFreezedAntBoxRequiredNumberToUnfreeze++;
                    }
                }
            }

            return maxFreezedAntBoxRequiredNumberToUnfreeze;
        }

        public void FillHiddenTiles(Tilemap tilemap, int width, int height, int manualStart = -1)
        {
            if (manualStart != -1)
            {
                Color targetColor = ColorUtil.HexToColor("#B04AFD");

                for (int i = 0; i < TileManager.Ins.tiles.Count; i++)
                {
                    BaseTile tile = TileManager.Ins.tiles[i];

                    if (tile.Faction == targetColor)
                    {
                        Vector3Int cellPos = tile.tilemapPosition;

                        Tile tilemapTile = ScriptableObject.CreateInstance<Tile>();
                        tilemapTile.sprite = hiddenTileSprite;

                        tilemap.SetTile(cellPos, tilemapTile);

                        tile.isHidden = true;
                    }
                }

                return;
            }

            int clusterSize = 100;
            int[] indexesToFill = GetRandomCluster(width, height, clusterSize, manualStart);

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

        public void FillWallTiles(Tilemap tilemap, int width, int height)
        {
            // int clusterSize = 300;
            // int[] indexesToFill = GetRandomCluster(width, height, clusterSize);

            // for (int i = 0; i < indexesToFill.Length; i++)
            // {
            //     int index = indexesToFill[i];
            //     Node node = TileManager.Ins.nodes[index];

            //     if (node.tile != null)
            //     {
            //         continue;
            //     }

            //     int x = index % width;
            //     int y = index / width;

            //     if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
            //     {
            //         continue;
            //     }

            //     Vector3Int cellPos = CommonUtil.GetVector3Int(node.initialPosition / GamePersistentVariable.tileMapScaleFactor);

            //     Tile tilemapTile = ScriptableObject.CreateInstance<Tile>();
            //     tilemapTile.sprite = wallTileSprite;

            //     tilemap.SetTile(cellPos, tilemapTile);

            //     node.isObstacle = true;
            // }

            List<Node> nodes = TileManager.Ins.nodes;

            int maxX = nodes.Max(item => item.x);
            int maxY = nodes.Max(item => item.y);

            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];

                if (node.x == 1 || node.x == maxX - 1 || node.y == 1 || node.y == maxY - 1)
                {
                    if (node.x > 0 && node.x < maxX && node.y > 0 && node.y < maxY)
                    {
                        if (node.tile != null)
                        {
                            continue;
                        }

                        Vector3Int cellPos = CommonUtil.GetVector3Int(node.initialPosition / GamePersistentVariable.tileMapScaleFactor);

                        Tile tilemapTile = ScriptableObject.CreateInstance<Tile>();
                        tilemapTile.sprite = wallTileSprite;

                        tilemap.SetTile(cellPos, tilemapTile);

                        node.isObstacle = true;
                    }
                }
            }
        }

        public static int[] GetRandomCluster(int width, int height, int clusterSize, int manualStart)
        {
            int totalTiles = width * height;
            clusterSize = Mathf.Min(clusterSize, totalTiles);

            List<int> cluster = new();
            HashSet<int> visited = new();
            List<int> frontier = new();

            int start = UnityEngine.Random.Range(0, totalTiles);

            if (manualStart != -1)
            {
                start = manualStart;
            }

            cluster.Add(start);
            visited.Add(start);
            frontier.Add(start);

            while (cluster.Count < clusterSize && frontier.Count > 0)
            {
                int current = frontier[UnityEngine.Random.Range(0, frontier.Count)];

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
                    int j = UnityEngine.Random.Range(0, i + 1);
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
