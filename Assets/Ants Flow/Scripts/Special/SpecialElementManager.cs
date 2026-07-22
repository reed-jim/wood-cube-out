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
    FreezedAntBox,
    WarriorAntBox,
    OpenCloseAntBox,
    MagnetAntBox,
    KeyAntBox,
    SnakeAntBox
}

namespace AntsFlow
{
    public class SpecialElementManager : Singleton<SpecialElementManager>
    {
        public LevelElement nextLevelElement;
        public SpecialElementConfig specialElementConfig;

        public int nextFreezeAntBoxRequiredNumber;
        public bool nextOpenCloseAntBoxInitialState = true;
        public List<AntBoxLink> antBoxLinks;
        public List<MagnetAntBoxLink> magnetAntBoxLinks;
        public SpawningTileBox spawningTileBox;
        public BossArea bossArea;
        public Snake snake;

        public Sprite hiddenTileSprite;
        public Sprite wallTileSprite;

        public IntVariable currentLevel;
        public int[] levelsShowSpecialElementIntroduction;

        public static event Action<int> showSpecialElementIntroduction;

        public void Setup(AntsFlow.LevelItem levelItem)
        {
            Reset();

            specialElementConfig = levelItem.specialElementConfig;

            for (int i = 0; i < antBoxLinks.Count; i++)
            {
                ObjectPoolingEverything.ReturnToPool(GameConstants.antBoxLink, antBoxLinks[i].gameObject);
            }

            antBoxLinks.Clear();

            for (int i = 0; i < magnetAntBoxLinks.Count; i++)
            {
                ObjectPoolingEverything.ReturnToPool(GameConstants.magnetAntBoxLink, magnetAntBoxLinks[i].gameObject);
            }

            magnetAntBoxLinks.Clear();

            if (levelsShowSpecialElementIntroduction.Contains(currentLevel.Value))
            {
                showSpecialElementIntroduction?.Invoke(Array.IndexOf(levelsShowSpecialElementIntroduction, currentLevel.Value));
            }

            SetupSpawningTileBoxes();
            SetupBossArea(levelItem.specialElementConfig.bossAreaConfig);
            SetupSnake(levelItem.specialElementConfig.snakeConfig);
        }

        private void Reset()
        {
            spawningTileBox.gameObject.SetActive(false);
            bossArea.gameObject.SetActive(false);
            snake.gameObject.SetActive(false);
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

                if (washingMachine != null && washingMachine is not HiddenAntBox &&
                    washingMachine is not FreezedAntBox && washingMachine is not KeyAntBox &&
                    washingMachine is not WarriorAntBox &&
                    !washingMachine.isOpenCloseType && !washingMachine.isMagnet)
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
                    washingMachine is not FreezedAntBox &&
                    washingMachine is not KeyAntBox &&
                    washingMachine is not WarriorAntBox &&
                    !washingMachine.isOpenCloseType &&
                    !washingMachine.isMagnet
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

        public void SpawnMagnetAntBoxLinkFirstAntBoxes(int order)
        {
            if (specialElementConfig.magnetAntBoxConfig == null ||
                !specialElementConfig.magnetAntBoxConfig.IsValidToSpawn(order))
            {
                return;
            }

            //if (magnetAntBoxLinks.Count >= 5)
            //{
            //    return;
            //}

            List<WashingMachineItem> validAntBoxesToLink = new List<WashingMachineItem>();

            for (int i = 0; i < WashingMachineManager.Ins.washingMachineStartSlots.Count; i++)
            {
                WashingMachineItem washingMachine = WashingMachineManager.Ins.washingMachineStartSlots[i].washingMachine;

                if (washingMachine != null && washingMachine is not HiddenAntBox &&
                    washingMachine is not FreezedAntBox && washingMachine is not KeyAntBox &&
                    washingMachine is not WarriorAntBox &&
                    !washingMachine.isOpenCloseType && !washingMachine.isDouble &&
                    !washingMachine.isMagnet)
                {
                    if (washingMachine.numSlotRequired == 1)
                    {
                        validAntBoxesToLink.Add(washingMachine);
                    }
                }
            }

            WashingMachineItem box1 = null;
            WashingMachineItem box2 = null;
            bool foundPair = false;

            for (int i = 0; i < validAntBoxesToLink.Count; i++)
            {
                for (int j = i + 1; j < validAntBoxesToLink.Count; j++)
                {
                    int col1 = validAntBoxesToLink[i].washingMachineStartSlot.column;
                    int col2 = validAntBoxesToLink[j].washingMachineStartSlot.column;
                    if (Mathf.Abs(col1 - col2) == 1)
                    {
                        box1 = validAntBoxesToLink[i];
                        box2 = validAntBoxesToLink[j];
                        foundPair = true;
                        break;
                    }
                }
                if (foundPair) break;
            }

            if (foundPair)
            {
                MagnetAntBoxLink magnetAntBoxLink = ObjectPoolingEverything.GetFromPool<MagnetAntBoxLink>(GameConstants.magnetAntBoxLink);

                magnetAntBoxLink.Setup(new WashingMachineItem[] { box1, box2 });

                magnetAntBoxLinks.Add(magnetAntBoxLink);

                specialElementConfig.magnetAntBoxConfig.RemoveOneSpecialElementInPool();
            }
        }

        public void SpawnMagnetAntBoxLink()
        {
            if (specialElementConfig.magnetAntBoxConfig == null ||
                !specialElementConfig.magnetAntBoxConfig.IsValidToSpawn(WashingMachineManager.Ins.numAntBoxSpawned))
            {
                return;
            }

            //if (magnetAntBoxLinks.Count >= 5)
            //{
            //    return;
            //}

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
                    washingMachine is not FreezedAntBox &&
                    washingMachine is not KeyAntBox &&
                    washingMachine is not WarriorAntBox &&
                    !washingMachine.isOpenCloseType &&
                    !washingMachine.isDouble &&
                    !washingMachine.isMagnet
                )
                {
                    if (washingMachine.numSlotRequired == 1)
                    {
                        validAntBoxesToLink.Add(washingMachine);
                    }
                }
            }

            WashingMachineItem box1 = null;
            WashingMachineItem box2 = null;
            bool foundPair = false;

            for (int i = 0; i < validAntBoxesToLink.Count; i++)
            {
                for (int j = i + 1; j < validAntBoxesToLink.Count; j++)
                {
                    int col1 = validAntBoxesToLink[i].washingMachineStartSlot.column;
                    int col2 = validAntBoxesToLink[j].washingMachineStartSlot.column;
                    if (Mathf.Abs(col1 - col2) == 1)
                    {
                        box1 = validAntBoxesToLink[i];
                        box2 = validAntBoxesToLink[j];
                        foundPair = true;
                        break;
                    }
                }
                if (foundPair) break;
            }

            if (foundPair)
            {
                MagnetAntBoxLink magnetAntBoxLink = ObjectPoolingEverything.GetFromPool<MagnetAntBoxLink>(GameConstants.magnetAntBoxLink);

                magnetAntBoxLink.Setup(new WashingMachineItem[] { box1, box2 });

                magnetAntBoxLinks.Add(magnetAntBoxLink);

                specialElementConfig.magnetAntBoxConfig.RemoveOneSpecialElementInPool();
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
                    int index = specialElementConfig.frozenAntBoxConfig.spawnOrder.IndexOf(WashingMachineManager.Ins.numAntBoxSpawned);

                    nextFreezeAntBoxRequiredNumber = specialElementConfig.frozenAntBoxConfig.numBoxToUnfreeze[index];
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
            else if (specialElementConfig.bossAreaConfig.IsValidToSpawn(WashingMachineManager.Ins.numAntBoxSpawned))
            {
                specialElementConfig.bossAreaConfig.RemoveOneSpecialElementInPool();

                return LevelElement.WarriorAntBox;
            }
            else if (specialElementConfig.openCloseAntBoxConfig.IsValidToSpawn(WashingMachineManager.Ins.numAntBoxSpawned, true))
            {
                var config = specialElementConfig.openCloseAntBoxConfig;
                if (config != null && config.initialStates != null && config.initialStates.Count > 0)
                {
                    nextOpenCloseAntBoxInitialState = config.initialStates[0];
                    config.initialStates.RemoveAt(0);
                }
                else
                {
                    nextOpenCloseAntBoxInitialState = true;
                }
                specialElementConfig.openCloseAntBoxConfig.RemoveOneSpecialElementInPool();

                return LevelElement.OpenCloseAntBox;
            }
            else if (specialElementConfig.lockedSlotConfig.keyAntBoxConfig != null && specialElementConfig.lockedSlotConfig.keyAntBoxConfig.IsValidToSpawn(WashingMachineManager.Ins.numAntBoxSpawned))
            {
                specialElementConfig.lockedSlotConfig.keyAntBoxConfig.RemoveOneSpecialElementInPool();

                return LevelElement.KeyAntBox;
            }
            else if (specialElementConfig.snakeConfig.IsValidToSpawn(WashingMachineManager.Ins.numAntBoxSpawned))
            {
                specialElementConfig.snakeConfig.RemoveOneSpecialElementInPool();

                return LevelElement.SnakeAntBox;
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
                        TileManager.Ins.SetShadowTile(cellPos);

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

        public void SetupSpawningTileBoxes()
        {
            if (specialElementConfig.spawningTileBoxConfig.items == null)
            {
                return;
            }

            for (int i = 0; i < specialElementConfig.spawningTileBoxConfig.items.Count; i++)
            {
                SpawningTileBoxItemConfig item = specialElementConfig.spawningTileBoxConfig.items[i];

                for (int j = 0; j < item.number; j++)
                {
                    WashingMachineManager.Ins.poolFaction.Add(item.faction);
                }
            }

            NextSpawningTileBoxes();
        }

        public void SetupBossArea(BossAreaConfig bossAreaConfig)
        {
            if (bossAreaConfig == null)
            {
                return;
            }

            if (bossAreaConfig.spawnOrder == null)
            {
                return;
            }

            if (bossAreaConfig.spawnOrder.Count == 0 || bossAreaConfig.numCoverColumn == 0 || bossAreaConfig.numCoverRow == 0)
            {
                return;
            }

            bossArea.gameObject.SetActive(true);

            int startX = 0;
            int startY = 0;

            if (bossAreaConfig.startNodeId >= 0 && bossAreaConfig.startNodeId < TileManager.Ins.nodes.Count)
            {
                Node startNode = TileManager.Ins.nodes[bossAreaConfig.startNodeId];
                startX = startNode.x;
                startY = startNode.y;
            }

            List<Node> nodes = new List<Node>();

            for (int r = 0; r < bossAreaConfig.numCoverRow; r++)
            {
                for (int c = 0; c < bossAreaConfig.numCoverColumn; c++)
                {
                    int targetX = startX + c;
                    int targetY = startY + r;

                    Node node = TileManager.Ins.nodes.FirstOrDefault(n => n.x == targetX && n.y == targetY);
                    if (node != null)
                    {
                        nodes.Add(node);
                    }
                }
            }

            bossArea.Setup(nodes, bossAreaConfig);
        }

        public void SetupSnake(SnakeConfig snakeConfig)
        {
            if (snakeConfig.spawnOrder == null)
            {
                return;
            }

            if (snakeConfig.spawnOrder.Count == 0)
            {
                return;
            }

            snake.Setup(snakeConfig);
        }

        public bool NextSpawningTileBoxes()
        {
            if (specialElementConfig.spawningTileBoxConfig.items.Count == 0)
            {
                return false;
            }

            Vector2 screenSizeWorld = GamePersistentVariable.screenSizeWorld;

            SpawningTileBoxItemConfig item = specialElementConfig.spawningTileBoxConfig.items[0];

            specialElementConfig.spawningTileBoxConfig.items.RemoveAt(0);

            spawningTileBox.gameObject.SetActive(true);

            spawningTileBox.transform.position = new Vector3(0.4f * screenSizeWorld.x, 0.35f * screenSizeWorld.y, -5);

            spawningTileBox.Setup(item.faction, item.number);

            SaferioTween.ScaleUpWithPunch(spawningTileBox.transform, 1.1f);

            return true;
        }
    }
}
