using System;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using Dreamteck.Utilities;
using NamPhuThuy.GamePlay;
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
        public List<SpawningTileBox> activeSpawningTileBoxes = new List<SpawningTileBox>();
        public BossArea bossArea;
        public Snake snake;

        public Sprite hiddenTileSprite;
        public Sprite wallTileSprite;

        public IntVariable currentLevel;
        public int[] levelsShowSpecialElementIntroduction;

        public static event Action<int> showSpecialElementIntroduction;

        protected override void MoreActionOnAwake()
        {
            base.MoreActionOnAwake();

            LevelLoader.startLevelEvent += OnLevelReplayed;
            SettingPopup.replayLevelEvent += OnLevelReplayed;
            LosePopup.replayLevelEvent += OnLevelReplayed;
            DebugPopup.switchLevelEvent += OnLevelReplayed;
            TileManager.winLevelEvent += OnLevelReplayed;
        }

        private void OnDestroy()
        {
            LevelLoader.startLevelEvent -= OnLevelReplayed;
            SettingPopup.replayLevelEvent -= OnLevelReplayed;
            LosePopup.replayLevelEvent -= OnLevelReplayed;
            DebugPopup.switchLevelEvent -= OnLevelReplayed;
            TileManager.winLevelEvent -= OnLevelReplayed;
        }

        private void OnLevelReplayed()
        {
            Reset();
        }

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
            foreach (var box in activeSpawningTileBoxes)
            {
                if (box != null && box != spawningTileBox) Destroy(box.gameObject);
            }
            activeSpawningTileBoxes.Clear();
            bossArea.gameObject.SetActive(false);

            if (snake != null)
            {
                snake.Reset();
                snake.gameObject.SetActive(false);
            }

            if (HardPixelManager.Ins != null)
            {
                HardPixelManager.Ins.Reset();
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

                if (washingMachine != null && washingMachine is not HiddenAntBox &&
                    washingMachine is not FreezedAntBox && washingMachine is not KeyAntBox &&
                    washingMachine is not WarriorAntBox && washingMachine is not SnakeAntBox &&
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
                    washingMachine is not SnakeAntBox &&
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
                    washingMachine is not WarriorAntBox && washingMachine is not SnakeAntBox &&
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
                    washingMachine is not SnakeAntBox &&
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
            // Boss/Warrior box được ưu tiên kiểm tra TRƯỚC: nếu spawnOrder của boss trùng spawnOrder của
            // loại box khác (vd Hidden) thì hộp hiện ra vẫn là warrior box đúng thứ tự, boss luôn đủ hộp.
            // Loại kia chỉ nhường đúng slot trùng đó, không ảnh hưởng gì.
            if (specialElementConfig.bossAreaConfig.IsValidToSpawn(WashingMachineManager.Ins.numAntBoxSpawned))
            {
                specialElementConfig.bossAreaConfig.RemoveOneSpecialElementInPool();

                return LevelElement.WarriorAntBox;
            }
            else if (specialElementConfig.hiddenAntBoxConfig.IsValidToSpawn(WashingMachineManager.Ins.numAntBoxSpawned))
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

        // Boss còn warrior box chờ trong spawnOrder hay không (không đụng gì tới state).
        public bool HasPendingBossBox()
        {
            return specialElementConfig != null &&
                   specialElementConfig.bossAreaConfig != null &&
                   specialElementConfig.bossAreaConfig.spawnOrder != null &&
                   specialElementConfig.bossAreaConfig.spawnOrder.Count > 0;
        }

        // Rắn còn snake box chờ trong spawnOrder hay không (không đụng gì tới state).
        public bool HasPendingSnakeBox()
        {
            return specialElementConfig != null &&
                   specialElementConfig.snakeConfig != null &&
                   specialElementConfig.snakeConfig.spawnOrder != null &&
                   specialElementConfig.snakeConfig.spawnOrder.Count > 0;
        }

        // Còn box đặc biệt (boss hoặc rắn) chờ trong spawnOrder hay không.
        public bool HasPendingSpecialBossBox()
        {
            return HasPendingBossBox() || HasPendingSnakeBox();
        }

        public LevelElement GetNextForcedSpecialElement()
        {
            if (HasPendingBossBox())
            {
                specialElementConfig.bossAreaConfig.spawnOrder.RemoveAt(0);

                return LevelElement.WarriorAntBox;
            }

            if (HasPendingSnakeBox())
            {
                specialElementConfig.snakeConfig.spawnOrder.RemoveAt(0);

                return LevelElement.SnakeAntBox;
            }

            return LevelElement.NormalAntBox;
        }

        // Ép lấy 1 warrior box còn lại của boss (bỏ qua so khớp đúng numAntBoxSpawned). Dùng khi pool
        // faction thường đã cạn: lúc này thứ tự không còn ý nghĩa, chỉ cần nhả nốt cho boss đủ hộp.
        public LevelElement GetNextForcedBossElement()
        {
            if (HasPendingBossBox())
            {
                specialElementConfig.bossAreaConfig.spawnOrder.RemoveAt(0);

                return LevelElement.WarriorAntBox;
            }

            return LevelElement.NormalAntBox;
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
            // Tile đang bị chiếm bởi rắn/boss/hộp (node.isObstacle) hoặc đã done thì KHÔNG được biến thành
            // hidden tile: nó đã bị tách khỏi chuỗi blockage nên không bao giờ được reveal -> hidden tile
            // kẹt trên map, không clear được. Đặc biệt là các ô đường đi con rắn đang chiếm hiện tại.
            HashSet<BaseTile> blockedTiles = GetTilesBlockedForHidden();

            if (manualStart != -1)
            {
                Color targetColor = ColorUtil.HexToColor("#B792FA");

                for (int i = 0; i < TileManager.Ins.tiles.Count; i++)
                {
                    BaseTile tile = TileManager.Ins.tiles[i];

                    if (tile.Faction == targetColor)
                    {
                        if (tile.isDone || tile.isHidden || blockedTiles.Contains(tile))
                        {
                            continue;
                        }

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

            List<int> notValidIndexes = new List<int>();

            for (int i = 0; i < width * height; i++)
            {
                if (i >= TileManager.Ins.tiles.Count)
                {
                    notValidIndexes.Add(i);
                }
            }

            int[] indexesToFill = GetRandomCluster(width, height, clusterSize, manualStart, notValidIndexes);

            for (int i = 0; i < indexesToFill.Length; i++)
            {
                int index = indexesToFill[i];

                if (index < 0 || index >= TileManager.Ins.tiles.Count)
                {
                    continue;
                }

                BaseTile tile = TileManager.Ins.tiles[index];

                if (tile.isDone || tile.isHidden || blockedTiles.Contains(tile))
                {
                    continue;
                }

                Vector3Int cellPos = tile.tilemapPosition;

                Tile tilemapTile = ScriptableObject.CreateInstance<Tile>();
                tilemapTile.sprite = hiddenTileSprite;

                tilemap.SetTile(cellPos, tilemapTile);

                tile.isHidden = true;
            }
        }

        // Tập tile không được phép chuyển thành hidden tile: các tile nằm trên node đang là obstacle
        // (rắn/boss/hộp spawn... đang chiếm). Những tile này nằm ngoài chuỗi blockage bình thường nên
        // hidden tile đặt lên đó sẽ không bao giờ được reveal/clear.
        private HashSet<BaseTile> GetTilesBlockedForHidden()
        {
            HashSet<BaseTile> blockedTiles = new HashSet<BaseTile>();

            List<Node> nodes = TileManager.Ins.nodes;

            if (nodes == null)
            {
                return blockedTiles;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];

                if (node.isObstacle && node.tile != null)
                {
                    blockedTiles.Add(node.tile);
                }
            }

            return blockedTiles;
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

        public static int[] GetRandomCluster
        (
            int width,
            int height,
            int clusterSize,
            int manualStart,
            List<int> emptyIndexes
        )
        {
            int totalTiles = width * height;

            HashSet<int> blocked = new(emptyIndexes);

            List<int> validIndexes = new(totalTiles - blocked.Count);
            for (int i = 0; i < totalTiles; i++)
            {
                if (!blocked.Contains(i))
                    validIndexes.Add(i);
            }

            if (validIndexes.Count == 0)
                return Array.Empty<int>();

            // Available tiles
            int availableTiles = totalTiles - blocked.Count;
            clusterSize = Mathf.Min(clusterSize, availableTiles);

            if (clusterSize <= 0)
                return System.Array.Empty<int>();

            List<int> cluster = new(clusterSize);
            HashSet<int> visited = new(blocked); // blocked tiles count as already visited
            List<int> frontier = new();

            int start;

            if (manualStart != -1)
            {
                if (blocked.Contains(manualStart))
                    return Array.Empty<int>();

                start = manualStart;
            }
            else
            {
                start = validIndexes[UnityEngine.Random.Range(0, validIndexes.Count)];
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
                y > 0 ? current - width : -1,
                y < height - 1 ? current + width : -1,
                x > 0 ? current - 1 : -1,
                x < width - 1 ? current + 1 : -1
            };

                // Fisher-Yates shuffle
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

        //public void SetupSpawningTileBoxes()
        //{
        //    /*
        //    if (specialElementConfig.spawningTileBoxConfig.items == null)
        //    {
        //        return;
        //    }

        //    spawningTileBox.spawnNumberOfFactions = new Dictionary<Color, int>();

        //    for (int i = 0; i < specialElementConfig.spawningTileBoxConfig.items.Count; i++)
        //    {
        //        SpawningTileBoxItemConfig item = specialElementConfig.spawningTileBoxConfig.items[i];

        //        for (int j = 0; j < item.number; j++)
        //        {
        //            WashingMachineManager.Ins.poolFaction.Add(item.faction);
        //        }

        //        spawningTileBox.spawnNumberOfFactions.Add(item.faction, item.number);
        //    }

        //    spawningTileBox.NextSpawningTileBoxes();
        //    */
        //}

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

        // Level có boss/rắn thì chỉ được tính win khi cả hai đã bị hạ. Dùng để chặn win sớm khi
        // người chơi clear hết tile màu thường (vd booster clear-one-color) mà chưa hạ boss/rắn.
        // Level không có yếu tố nào (bossArea/snake inactive) thì luôn trả về true -> không ảnh hưởng.
        public bool AreSpecialBossElementsCleared()
        {
            if (bossArea != null && bossArea.BlocksWin)
            {
                return false;
            }

            if (snake != null && snake.BlocksWin)
            {
                return false;
            }

            return true;
        }

        public void SetupSnake(SnakeConfig snakeConfig)
        {
            if (snakeConfig == null || snakeConfig.spawnOrder == null || snakeConfig.spawnOrder.Count == 0)
            {
                return;
            }

            snake.Setup(snakeConfig);
        }

        // public bool NextSpawningTileBoxes()
        // {
        //     if (specialElementConfig.spawningTileBoxConfig.items.Count == 0)
        //     {
        //         return false;
        //     }

        //     Vector2 screenSizeWorld = GamePersistentVariable.screenSizeWorld;

        //     SpawningTileBoxItemConfig item = specialElementConfig.spawningTileBoxConfig.items[0];

        //     specialElementConfig.spawningTileBoxConfig.items.RemoveAt(0);

        //     spawningTileBox.gameObject.SetActive(true);

        //     spawningTileBox.transform.position = new Vector3(0.4f * screenSizeWorld.x, 0.35f * screenSizeWorld.y, -5);

        //     spawningTileBox.Setup(item.faction, item.number);

        //     SaferioTween.ScaleUpWithPunch(spawningTileBox.transform, 1.1f);

        //     return true;
        // }
        public void SetupSpawningTileBoxes()
        {
            if (specialElementConfig.spawningTileBoxConfig == null || specialElementConfig.spawningTileBoxConfig.items == null)
            {
                return;
            }

            for (int i = 0; i < specialElementConfig.spawningTileBoxConfig.items.Count; i++)
            {
                SpawningTileBoxItemConfig item = specialElementConfig.spawningTileBoxConfig.items[i];

                if (item.number <= 0) continue;

                SpawningTileBox newBox = Instantiate(spawningTileBox, spawningTileBox.transform.parent);
                newBox.gameObject.SetActive(true);

                List<Node> boxNodes = new List<Node>();
                int destroyedTileCount = 0;
                if (item.startNodeId >= 0 && item.startNodeId < TileManager.Ins.nodes.Count)
                {
                    Node startNode = TileManager.Ins.nodes[item.startNodeId];
                    int startX = startNode.x;
                    int startY = startNode.y;

                    Vector3 centerPos = Vector3.zero;
                    int count = 0;
                    for (int r = 0; r < item.numCoverRow; r++)
                    {
                        for (int c = 0; c < item.numCoverColumn; c++)
                        {
                            int targetX = startX + c;
                            int targetY = startY + r;

                            Node node = TileManager.Ins.nodes.FirstOrDefault(n => n.x == targetX && n.y == targetY);
                            if (node != null)
                            {
                                centerPos += node.initialPosition;
                                count++;
                                node.isObstacle = true;
                                boxNodes.Add(node);

                                if (node.tile != null)
                                {
                                    // Remove the tile's color from poolFaction
                                    GamePersistentVariable.levelItem.poolFaction.Remove(node.tile.Faction);

                                    // Remove from TileManager tiles
                                    TileManager.Ins.tiles.Remove(node.tile);

                                    // Destroy the tile
                                    node.tile.Hide();
                                    node.tile = null;
                                    destroyedTileCount++;
                                }
                            }
                        }
                    }
                    if (count > 0)
                    {
                        centerPos /= count;
                        newBox.transform.position = centerPos.ChangeZ(-7);
                    }
                }

                newBox.Setup(item.faction, item.number, boxNodes);
                activeSpawningTileBoxes.Add(newBox);
            }
        }

        public void OnBoosterClearOneColor(Color selectedFaction, List<BaseTile> clearedTiles)
        {
            if (activeSpawningTileBoxes == null || activeSpawningTileBoxes.Count == 0 || clearedTiles == null || clearedTiles.Count == 0)
            {
                return;
            }

            foreach (var box in activeSpawningTileBoxes)
            {
                if (box != null && box.gameObject.activeSelf && box.faction == selectedFaction && box.numberLeft > 0)
                {
                    box.ForceComplete();
                }
            }
        }
    }
}
