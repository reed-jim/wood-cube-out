using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntsFlow;
using Firebase.RemoteConfig;
using NamPhuThuy;
using NamPhuThuy.Data;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;
using static GameEnum;

namespace NamPhuThuy.GamePlay
{
    public class LevelLoader : MonoBehaviour
    {
        [Header("SCRIPTABLE OBJECT")]
        [SerializeField] private IntVariable currentLevel;
        [SerializeField] private LevelDataContainer levelDataContainer;
        [SerializeField] private LevelBoosterObserver levelBoosterObserver;
        [SerializeField] private ObjectPartMaterialContainer objectPartMaterialContainer;

        [Header("Customize")]
        [SerializeField] private float levelScaleXRatio = 18f;
        [SerializeField] private float levelScaleYRatio = 15f;
        [SerializeField] private float finalLevelScaleMultilier = 1f;
        [SerializeField] private float levelYOffsetRatio = 0.175f;
        [SerializeField] private float multiplierScaleX = 54f;
        [SerializeField] private float multiplierScaleY = 54f;

        #region PRIVATE FIELD
        private Camera _mainCamera;
        private List<Tween> _tweens;
        private List<Tween> _autoCenterLevelTweens;
        private LevelDifficultyConfiguration _currentLevelDifficultyConfiguration;
        private int _maxLevel;
        private BasicObjectPart[] _pieces;
        private Transform _currentLevelContainer;
        private Transform _pivotAdjust;
        private bool _isSetLevelGradientBound;
        private bool _isNewLoopLevel;
        private int _nextLevel;
        private bool _isLoopLevel;
        #endregion

        #region Memory Management
        AsyncOperationHandle<TextAsset> _loadLevelHandle;
        #endregion

        #region EVENT
        public static event Action startLevelEvent;
        public static event Action startLevelCompletedEvent;
        public static event Action endCurrentLevelEvent;
        public static event Action setLevelNumberEvent;
        public static event Action<int> setLevelScrewNumberEvent;
        public static event Action<int> setMultiPhaseLevelScrewNumberEvent;
        public static event Action showSceneTransitionEvent;
        public static event Action<ScreenRoute> switchRouteEvent;
        public static event Action setIsLevelLoadedEvent;
        public static event Action<bool> enableSwipingEvent;
        public static event Action<float, bool> setTargetOrthographicSizeEvent;
        public static event Action<bool> showLoadLevelErrorEvent;

        public static event Action<bool> specialTutIntroduceEvent; // true if this is the first time a special tut is shown
        #endregion

        [Header("Tilemap")]
        public Transform glowTilemapGrid;
        public Tilemap shadowTilemap;
        public Transform shadowTilemapGrid;
        public Sprite shadowSprite;
        public List<Transform> tilemapGrids;
        public float scaleFactor;
        public Vector3 shadowTilemapOffset;

        private void Awake()
        {
            PausePopup.replayLevelEvent += Replay;
            LosePopup.replayLevelEvent += Replay;
            ReplayPopup.replayLevelEvent += Replay;
            SettingPopup.replayLevelEvent += Replay;
            LevelComingSoonPopup.replayLevelEvent += Replay;
            DebugPopup.toLevelEvent += GoLevel;
            DebugPopup.nextLevelEvent += NextLevel;
            DebugPopup.prevLevelEvent += PrevLevel;
            WinPopup.nextLevelEvent += NextLevel;
            WinPopup.goLevelEvent += GoLevel;
            LosePopup.skipLevelEvent += NextLevel;
            BasicObjectPart.pieceCompleteEvent += OnPieceCompleted;
            LoadLevelErrorScene.retryLoadLevelEvent += LoadLevel;
            ChooseDifficultyPopup.hardModeSuggestionClosedEvent += OnHardModeSuggestionClosed;

            _tweens = new List<Tween>();
            _autoCenterLevelTweens = new List<Tween>();
            tilemapGrids = new List<Transform>();

            currentLevel.Load();

            _mainCamera = Camera.main;

            _maxLevel = levelDataContainer.MaxLevel;

            GoLevel(currentLevel.Value);

            GamePersistentVariable.hasPlayerReachedGameplay = true;
            GamePersistentVariable.tileMapScaleFactor = scaleFactor;
        }

        private void Start()
        {
            // GoLevel(currentLevel.Value);

            GamePersistentVariable.screenSizeWorld = CommonUtil.GetScreenSizeWorld(Camera.main);
        }

        void OnDestroy()
        {
            PausePopup.replayLevelEvent -= Replay;
            LosePopup.replayLevelEvent -= Replay;
            ReplayPopup.replayLevelEvent -= Replay;
            SettingPopup.replayLevelEvent -= Replay;
            LevelComingSoonPopup.replayLevelEvent -= Replay;
            DebugPopup.toLevelEvent -= GoLevel;
            DebugPopup.nextLevelEvent -= NextLevel;
            DebugPopup.prevLevelEvent -= PrevLevel;
            WinPopup.nextLevelEvent -= NextLevel;
            WinPopup.goLevelEvent -= GoLevel;
            LosePopup.skipLevelEvent -= NextLevel;
            BasicObjectPart.pieceCompleteEvent -= OnPieceCompleted;
            LoadLevelErrorScene.retryLoadLevelEvent -= LoadLevel;
            ChooseDifficultyPopup.hardModeSuggestionClosedEvent -= OnHardModeSuggestionClosed;

            CommonUtil.StopAllTweens(_tweens);
            CommonUtil.StopAllTweens(_autoCenterLevelTweens);
        }

        #region LEVEL
        private async void LoadLevel()
        {
            // showSceneTransitionEvent?.Invoke();
            // // NO INTERNET
            // if (Application.internetReachability == NetworkReachability.NotReachable)
            // {
            //     showLoadLevelErrorEvent?.Invoke(true);

            //     return;
            // }
            // else
            // {
            //     showLoadLevelErrorEvent?.Invoke(false);
            // }
            // //

            int nextLevel = currentLevel.Value;

            bool isLoopLevel = false;

            // if (!UserData.IsLoopLevel)
            // {
            //     if (currentLevel.Value <= _maxLevel)
            //     {
            //         nextLevel = currentLevel.Value;
            //     }
            //     else
            //     {
            //         nextLevel = GetLoopLevel();

            //         await UserData.SetIsLoopLevel(true);
            //         await UserData.SetIsLastLevelBeforeLoop(_maxLevel);

            //         // isLoopLevel = true;
            //         SetIsLoopLevel(true);
            //     }
            // }
            // else
            // {
            //     if (currentLevel.Value - UserData.NumLevelLoop >= _maxLevel)
            //     {
            //         nextLevel = GetLoopLevel();
            //     }
            //     else
            //     {
            //         nextLevel = currentLevel.Value - UserData.NumLevelLoop;
            //     }

            //     // isLoopLevel = true;
            //     SetIsLoopLevel(true);
            // }

            // NOT LOOP
            if (currentLevel.Value > _maxLevel)
            {
                isLoopLevel = true;
            }

            if (isLoopLevel)
            {
                showSceneTransitionEvent?.Invoke();
                setIsLevelLoadedEvent?.Invoke();
                switchRouteEvent?.Invoke(ScreenRoute.LevelComingSoon);
                setLevelNumberEvent?.Invoke();

                GamePersistentVariable.levelItem = null;

                TutorialLevelOne.Ins.HideTutorial();
                BoosterTutorial.Ins.EndTutorial();

                return;
            }
            // NOT LOOP

            nextLevel = Mathf.Clamp(nextLevel, 1, _maxLevel);

            GamePersistentVariable.levelPrefab = nextLevel;

            setLevelNumberEvent?.Invoke();

            // nextLevel = RemoteConfigLevelManager.GetReplacedLevelIndexInGroup(nextLevel);
            // nextLevel = RemoteConfigLevelManager.GetReplacedLevelIndex(nextLevel);

            GamePersistentVariable.currentLevelMode = levelDataContainer.LevelsData.First(item => item.levelId + 1 == nextLevel).levelMode;

            string levelName = $"Level {nextLevel}";

            // Memory Management
            Resources.UnloadUnusedAssets();

            if (_loadLevelHandle.IsValid())
            {
                Addressables.Release(_loadLevelHandle);
            }

            // _loadLevelHandle = Addressables.InstantiateAsync(levelName, transform);
            // load from json
            _loadLevelHandle = Addressables.LoadAssetAsync<TextAsset>(levelName);

            _loadLevelHandle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    ActualLoadLevel(op.Result);
                }
            };

            startLevelEvent?.Invoke();

            showSceneTransitionEvent?.Invoke();

            void ActualLoadLevel(TextAsset resultLoad)
            {
                // GameObject level = resultLoad;
                // AntsFlow.LevelItem levelItem = level.GetComponent<AntsFlow.LevelItem>();
                // Transform levelTransform = level.transform;

                // load from json
                string json = resultLoad.text;
                AntLevelData antLevelData = JsonUtility.FromJson<AntLevelData>(json);

                Transform levelTransform;

                AntsFlow.LevelItem levelItem = JsonToLevelItem(antLevelData, out levelTransform);

                GamePersistentVariable.levelItem = levelItem;
                GamePersistentVariable.isLevelLost = false;

                if (levelItem.isOverrideLevelScale)
                {
                    if (levelItem.levelScaleXRatio > 1 && levelItem.levelScaleYRatio > 1)
                    {
                        levelScaleXRatio = levelItem.levelScaleXRatio;
                        levelScaleYRatio = levelItem.levelScaleYRatio;
                    }
                    multiplierScaleX = levelItem.multiplierScaleX;
                    multiplierScaleY = levelItem.multiplierScaleY;
                }

                CalculateLevelScale(levelItem);

                antLevelData = AutoSpacingAllTiles(antLevelData);
                // FitLevelToScreenArea(levelTransform);
                // CenterLevel(levelTransform);

                FillTileMap(antLevelData, levelTransform);
                // // BuildMesh(levelTransform);   

                if (currentLevel.Value == 2)
                {
                    levelItem.isTutorialSpeedUpLevel = true;
                }

                WashingMachineManager.Ins.StartLevel(levelItem);

                Tween.Delay(1).OnComplete(() =>
                {
                    setIsLevelLoadedEvent?.Invoke();
                });

                GamePersistentVariable.startLevelTime = Time.time;
                GamePersistentVariable.levelRetryTime++;
            }

            void SetIsLoopLevel(bool isLoop)
            {
                isLoopLevel = isLoop;
            }
        }

        private void CalculateLevelScale(AntsFlow.LevelItem levelItem)
        {
            int numRows = GamePersistentVariable.numRow;
            int numCols = GamePersistentVariable.numColumn;

            if (levelItem.isLevelHavingTransparentPixels)
            {
                numRows = levelItem.numRow;
                numCols = levelItem.numColumn;
            }

            float scaleByX = levelScaleXRatio / numCols;
            float scaleByY = levelScaleYRatio / numRows;
            float finalScale = finalLevelScaleMultilier * Mathf.Min(scaleByX, scaleByY);
            float safeAreaRatio = 1 - GetTopUnsafeRatio();

            GamePersistentVariable.obstacleScaleFactor = finalScale * safeAreaRatio;

            finalScale /= multiplierScaleX;
            finalScale *= safeAreaRatio;

            scaleFactor = finalScale;
            GamePersistentVariable.tileMapScaleFactor = scaleFactor;

            float GetTopUnsafeRatio()
            {
                Rect safe = Screen.safeArea;

                float screenHeight = Screen.height;

                // safe.yMax is the top of the safe area in screen pixels
                float safeTop = safe.yMax;

                // unsafe top area height
                float unsafeTopHeight = screenHeight - safeTop;

                return unsafeTopHeight / screenHeight;
            }
        }

        private void FillTileMap(AntLevelData antLevelData, Transform levelTransform)
        {
            int numRows = GamePersistentVariable.numRow;
            int numCols = GamePersistentVariable.numColumn;

            shadowTilemapGrid.SetParent(levelTransform);
            shadowTilemapGrid.position = shadowTilemapOffset;
            shadowTilemapGrid.localScale = scaleFactor * Vector3.one;
            glowTilemapGrid.localScale = scaleFactor * Vector3.one;

            float ratioXY = multiplierScaleX * (multiplierScaleX / multiplierScaleY);

            GamePersistentVariable.tileDistance = GamePersistentVariable.obstacleScaleFactor * new Vector2(1, multiplierScaleX / multiplierScaleY);
            GamePersistentVariable.ratioXY = ratioXY;

            Transform spawnedTileMapGrid = ObjectPoolingEverything.GetFromPool<Transform>(GameConstants.tileMap);
            Tilemap spawnedTileMap = spawnedTileMapGrid.GetComponentInChildren<Tilemap>();

            spawnedTileMapGrid.SetParent(levelTransform);
            spawnedTileMapGrid.position = spawnedTileMapGrid.position.ChangeZ(-6f);
            spawnedTileMapGrid.localScale = scaleFactor * Vector3.one;

            AntsFlow.TileManager.Ins.tilemap = spawnedTileMap;

            tilemapGrids.Add(spawnedTileMapGrid);

            Tile tile = ScriptableObject.CreateInstance<Tile>();
            Tile shadowTile = ScriptableObject.CreateInstance<Tile>();
            shadowTile.sprite = shadowSprite;

            AntsFlow.TileManager.Ins.shadowTile = shadowTile;

            if (antLevelData.levelItemData.isLevelHavingTransparentPixels)
            {
                for (int i = 0; i < antLevelData.antLevelDataItems.Count; i++)
                {
                    Vector3Int cellPos = CommonUtil.GetVector3Int(antLevelData.antLevelDataItems[i].position.MultiplyX(54).MultiplyY(ratioXY));

                    tile.sprite = FactionMapper.Ins.GetTileSprite(FactionMapper.Ins.indexesOfColors[antLevelData.antLevelDataItems[i].factionIndex]);

                    spawnedTileMap.SetTile(cellPos, tile);
                    shadowTilemap.SetTile(cellPos, shadowTile);

                    AntsFlow.TileManager.Ins.tiles[i].tilemap = spawnedTileMap;
                    AntsFlow.TileManager.Ins.tiles[i].tilemapPosition = cellPos;

                    // TileObstacleManager.Ins.SpawnObstacleAt(AntsFlow.TileManager.Ins.tiles[i]);
                }
            }
            else
            {
                for (int i = 0; i < numRows; i++)
                {
                    // Transform spawnedTileMapGrid = ObjectPoolingEverything.GetFromPool<Transform>(GameConstants.tileMap);
                    // Tilemap spawnedTileMap = spawnedTileMapGrid.GetComponentInChildren<Tilemap>();

                    // spawnedTileMapGrid.SetParent(levelTransform);
                    // spawnedTileMapGrid.position = spawnedTileMapGrid.position.ChangeZ(-0.01f * (numRows - i));
                    // spawnedTileMapGrid.localScale = scaleFactor * Vector3.one;

                    // tilemapGrids.Add(spawnedTileMapGrid);

                    for (int j = 0; j < numCols; j++)
                    {
                        int index = j + i * numCols;

                        Vector3Int cellPos = CommonUtil.GetVector3Int(antLevelData.antLevelDataItems[index].position.MultiplyX(54).MultiplyY(ratioXY));

                        // Tile tile = ScriptableObject.CreateInstance<Tile>();
                        tile.sprite = FactionMapper.Ins.GetTileSprite(FactionMapper.Ins.indexesOfColors[antLevelData.antLevelDataItems[index].factionIndex]);

                        spawnedTileMap.SetTile(cellPos, tile);
                        shadowTilemap.SetTile(cellPos, shadowTile);

                        AntsFlow.TileManager.Ins.tiles[index].tilemap = spawnedTileMap;
                        AntsFlow.TileManager.Ins.tiles[index].tilemapPosition = cellPos;

                        // TileObstacleManager.Ins.SpawnObstacleAt(AntsFlow.TileManager.Ins.tiles[index]);
                    }
                }
            }

            if (GamePersistentVariable.levelItem.specialElementConfig.isContainsHiddenTiles)
            {
                int manualStart = currentLevel.Value == 40 ? 0 : -1;

                SpecialElementManager.Ins.FillHiddenTiles(spawnedTileMap, numCols, numRows, manualStart);
            }
        }

        private AntsFlow.LevelItem JsonToLevelItem(AntLevelData antLevelData, out Transform levelTransform)
        {
            List<Color> poolFaction = FactionMapper.Ins.GetPoolFactionFromPoolFactionIndex(antLevelData.levelItemData.poolFactionIndexes);

            GameObject level = new GameObject();

            level.name = "Level";

            levelTransform = level.transform;

            level.transform.SetParent(transform);

            AntsFlow.LevelItem spawnedLevelItem = level.AddComponent<AntsFlow.LevelItem>();
            spawnedLevelItem.poolFaction = poolFaction;
            spawnedLevelItem.numColumn = antLevelData.levelItemData.numColumn;
            spawnedLevelItem.numRow = antLevelData.levelItemData.numRow;
            spawnedLevelItem.isLevelHavingTransparentPixels = antLevelData.levelItemData.isLevelHavingTransparentPixels;
            spawnedLevelItem.isOverrideLevelScale = antLevelData.levelItemData.isOverrideLevelScale;
            spawnedLevelItem.levelScaleXRatio = antLevelData.levelItemData.levelScaleXRatio;
            spawnedLevelItem.levelScaleYRatio = antLevelData.levelItemData.levelScaleYRatio;
            spawnedLevelItem.multiplierScaleX = antLevelData.levelItemData.multiplierScaleX;
            spawnedLevelItem.multiplierScaleY = antLevelData.levelItemData.multiplierScaleY;
            spawnedLevelItem.isTutorialLevel = antLevelData.levelItemData.isTutorialLevel;
            spawnedLevelItem.isUseManualFactionSetupForFirstAntBoxes = antLevelData.levelItemData.isUseManualFactionSetupForFirstAntBoxes;
            spawnedLevelItem.levelFactionConfig = antLevelData.levelItemData.levelFactionConfig;
            spawnedLevelItem.difficulty = antLevelData.levelItemData.difficulty;
            spawnedLevelItem.initialDifficulty = antLevelData.levelItemData.initialDifficulty;
            spawnedLevelItem.overrideInitialDifficulty = antLevelData.levelItemData.overrideInitialDifficulty;
            spawnedLevelItem.numStartAntBoxes = antLevelData.levelItemData.numStartAntBoxes;
            spawnedLevelItem.specialElementConfig = antLevelData.levelItemData.specialElementConfig;

            List<AntsFlow.BaseTile> tiles = new List<AntsFlow.BaseTile>();

            GameObject tileComponentHolder = new GameObject();
            tileComponentHolder.name = "tileComponentHolder";

            for (int i = 0; i < antLevelData.antLevelDataItems.Count; i++)
            {
                AntLevelDataItem antLevelDataItem = antLevelData.antLevelDataItems[i];

                // AntsFlow.BaseTile tile = Instantiate(tilePrefab, level.transform);
                // AntsFlow.BaseTile tile = ObjectPoolingEverything.GetFromPool<AntsFlow.BaseTile>(GameConstants.tile);
                // AntsFlow.BaseTile tile = tileComponentHolder.AddComponent<AntsFlow.BaseTile>();
                AntsFlow.BaseTile tile = new AntsFlow.BaseTile();

                // tile.transform.SetParent(levelTransform);

                // tile.transform.position = antLevelDataItem.position;

                tile.tileName = i;
                tile.position = antLevelDataItem.position;
                tile.Faction = FactionMapper.Ins.mapper[antLevelDataItem.factionIndex].color;
                tile.ring = antLevelDataItem.ring;
                tile.priority = antLevelDataItem.priority;

                tiles.Add(tile);
            }

            // find num column
            float firstRowY = antLevelData.antLevelDataItems[0].position.y;
            float tolerance = 0.01f;

            GamePersistentVariable.numColumn = 0;

            foreach (var tile in antLevelData.antLevelDataItems)
            {
                if (Mathf.Abs(tile.position.y - firstRowY) < tolerance)
                {
                    GamePersistentVariable.numColumn++;
                }
                else
                {
                    break;
                }
            }

            GamePersistentVariable.numRow = antLevelData.antLevelDataItems.Count / GamePersistentVariable.numColumn;
            //

            for (int i = 0; i < tiles.Count; i++)
            {
                AntsFlow.BaseTile tile = tiles[i];
                AntLevelDataItem antLevelDataItem = antLevelData.antLevelDataItems[i];

                int upIndex = i + GamePersistentVariable.numColumn;
                int downIndex = i - GamePersistentVariable.numColumn;
                int rightIndex = i + 1;
                int leftIndex = i - 1;

                int topRightIndex = i + GamePersistentVariable.numColumn + 1;
                int topLeftIndex = i + GamePersistentVariable.numColumn - 1;
                int downRightIndex = i - GamePersistentVariable.numColumn + 1;
                int downLeftIndex = i - GamePersistentVariable.numColumn - 1;

                tile.blockages = new List<AntsFlow.BaseTile>();

                if (upIndex < tiles.Count)
                {
                    tile.blockages.Add(tiles[upIndex]);
                }
                if (downIndex >= 0)
                {
                    tile.blockages.Add(tiles[downIndex]);
                }
                if (rightIndex < tiles.Count && SameRow(i, rightIndex, GamePersistentVariable.numColumn))
                {
                    tile.blockages.Add(tiles[rightIndex]);
                }
                if (leftIndex >= 0 && SameRow(i, leftIndex, GamePersistentVariable.numColumn))
                {
                    tile.blockages.Add(tiles[leftIndex]);
                }

                // if (topRightIndex >= 0 && topRightIndex < tiles.Count && i + 1 < GamePersistentVariable.numColumn)
                // {
                //     tile.blockages.Add(tiles[topRightIndex]);
                // }
                // if (topLeftIndex >= 0 && topLeftIndex < tiles.Count && i - 1 >= 0)
                // {
                //     tile.blockages.Add(tiles[topLeftIndex]);
                // }
                // if (downRightIndex >= 0 && downRightIndex < tiles.Count && i + 1 < GamePersistentVariable.numColumn)
                // {
                //     tile.blockages.Add(tiles[downRightIndex]);
                // }
                // if (downLeftIndex >= 0 && downLeftIndex < tiles.Count && i - 1 >= 0)
                // {
                //     tile.blockages.Add(tiles[downLeftIndex]);
                // }

                // tile.blockages = new List<AntsFlow.BaseTile>();

                // for (int j = 0; j < antLevelDataItem.blockageIndexes.Count; j++)
                // {
                //     int index = antLevelDataItem.blockageIndexes[j];

                //     tile.blockages.Add(tiles[index]);
                // }
            }

            // // Clear in case this is rebuilt
            // foreach (var tile in tiles)
            // {
            //     tile.blockedByMe = new HashSet<AntsFlow.BaseTile>();
            // }

            // // Build reverse references
            // foreach (var tile in tiles)
            // {
            //     foreach (var blockage in tile.blockages)
            //     {
            //         blockage.blockedByMe.Add(tile);
            //     }
            // }

            AntsFlow.TileManager.Ins.tiles = new List<AntsFlow.BaseTile>();
            AntsFlow.TileManager.Ins.tiles.AddRange(tiles);

            return spawnedLevelItem;

            bool SameRow(int indexA, int indexB, int numCol)
            {
                return (indexA / numCol) == (indexB / numCol);
            }
        }

        private void CenterLevel(Transform level)
        {
            Vector2 average = Vector2.zero;

            foreach (Transform child in level.transform)
            {
                average += (Vector2)child.position;
            }

            average /= level.transform.childCount;

            level.transform.position += Vector3.left * average.x;
            // level.transform.position += Vector3.down * average.y;

            float distance = Mathf.Abs(Camera.main.transform.position.z);

            Vector3 worldPos = Camera.main.ViewportToWorldPoint(
                new Vector3(0, 0.55f, distance)
            );

            transform.position = transform.position.ChangeY(worldPos.y);
        }

        public AntLevelData AutoSpacingAllTiles(AntLevelData antLevelData)
        {
            AntLevelData result = antLevelData;

            if (antLevelData.levelItemData.isLevelHavingTransparentPixels)
            {
                int numRows = antLevelData.levelItemData.numRow;
                int numCols = antLevelData.levelItemData.numColumn;

                for (int i = 0; i < result.antLevelDataItems.Count; i++)
                {
                    AntLevelDataItem antLevelDataItem = result.antLevelDataItems[i];

                    antLevelDataItem.position = antLevelDataItem.position.AddX(-(numCols - 1) / 2f).AddY(levelYOffsetRatio * numRows);
                }
            }
            else
            {
                int numRows = GamePersistentVariable.numRow;
                int numCols = GamePersistentVariable.numColumn;

                for (int row = 0; row < numRows; row++)
                {
                    for (int col = 0; col < numCols; col++)
                    {
                        Vector3 position = new Vector3(
                            -(numCols - 1) / 2f + col * 1,
                            (levelYOffsetRatio * numRows) + row * 1,
                            0f
                        );

                        int index = col + row * numCols;

                        result.antLevelDataItems[index].position = position;

                        AntsFlow.TileManager.Ins.tiles[index].position = scaleFactor * position;
                    }
                }
            }

            return result;
        }

        void FitLevelToScreenArea(Transform level)
        {
            // Get level bounds
            Bounds bounds = GetBoundsFromTiles(level);

            float levelWidth = bounds.size.x;
            float levelHeight = bounds.size.y;

            // Orthographic camera size
            float camHeight = Camera.main.orthographicSize * 2f;
            float camWidth = camHeight * Camera.main.aspect;

            // Available screen area
            float availableHeight = camHeight * 0.35f;
            float availableWidth = 0.9f * camWidth;

            float scaleX = level.localScale.x * availableWidth / levelWidth;
            float scaleY = level.localScale.y * availableHeight / levelHeight;

            float scale = Mathf.Min(scaleX, scaleY);

            level.localScale = Vector3.one * scale;
        }

        public Bounds GetBoundsFromTiles(Transform level)
        {
            if (level.childCount == 0)
                return new Bounds(level.position, Vector3.zero);

            Vector3 min = level.GetChild(0).position;
            Vector3 max = min;

            for (int i = 1; i < level.childCount; i++)
            {
                Vector3 pos = level.GetChild(i).position;

                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }

            Bounds bounds = new Bounds();
            bounds.SetMinMax(min, max);

            return bounds;
        }

        private async void GoLevel(int level)
        {
            endCurrentLevelEvent?.Invoke();

            await Task.Delay(500);

            shadowTilemapGrid.SetParent(transform.parent);
            shadowTilemap.ClearAllTiles();

            for (int i = 0; i < tilemapGrids.Count; i++)
            {
                tilemapGrids[i].GetComponentInChildren<Tilemap>().ClearAllTiles();

                ObjectPoolingEverything.ReturnToPool(GameConstants.tileMap, tilemapGrids[i].gameObject);
            }

            tilemapGrids.Clear();

            if (transform.childCount > 0)
            {
                Destroy(transform.GetChild(0).gameObject);
            }

            currentLevel.Value = level;

            currentLevel.Save();

            levelBoosterObserver.Reset();

            LoadLevel();

            GamePersistentVariable.isLevelDirty = false;
        }

        private void NextLevel()
        {
            GoLevel(currentLevel.Value + 1);
        }

        private void PrevLevel()
        {
            GoLevel(currentLevel.Value - 1);
        }

        private void Replay()
        {
            GamePersistentVariable.isPreventLoadLevelData = true;

            GoLevel(currentLevel.Value);
        }
        #endregion

        #region SPECIAL LEVELS

        private string ModifyNameInFixedLevel(string levelName, int levelId)
        {
            string res = levelName;

            if (GamePersistentVariable.specialLevelConfig.isEnable) return res;
            if (levelDataContainer == null || levelDataContainer.LevelsData == null) return res;

            LevelMode mode = levelDataContainer.LevelsData[levelId - 1].levelMode;

            // find level to replace MultipleLayers & ProtectedObjectPart levels
            if (mode == LevelMode.MultipleLayers || mode == LevelMode.ProtectedObjectPart)
            {
                // int replaceLevelIndex = levelDataContainer.LevelsData.First(item => item.levelId > 20 && item.levelMode == LevelMode.Normal).levelId;
                int replaceLevelIndex = levelDataContainer.LevelsData
                    .Where(i => i.levelId > 20 && i.levelMode == LevelMode.Normal)
                    .OrderBy(_ => UnityEngine.Random.value)
                    .Select(i => i.levelId + 1)
                    .FirstOrDefault();

                res = $"Level {replaceLevelIndex}";
            }

            return res;
        }

        #endregion

        #region ASSIGN SCREW FACTION
        private void AutoAssignScrewFactionMultiPhaseLevel(GameObject level)
        {
            MultiPhaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<MultiPhaseScrew>(level.transform).ToArray();

            Dictionary<int, List<GameFaction>> remainingFactionForScrewsByPhase = new Dictionary<int, List<GameFaction>>();

            Dictionary<int, int> currentFactionByPhase = new Dictionary<int, int>();

            for (int i = 0; i < screws.Length; i++)
            {
                int phase = screws[i].Phase;

                if (!remainingFactionForScrewsByPhase.ContainsKey(phase))
                {
                    currentFactionByPhase.Add(phase, 0);

                    remainingFactionForScrewsByPhase.Add(phase, new List<GameFaction>() { GameConstants.SCREW_FACTION[currentFactionByPhase[phase]] });
                }
                else
                {
                    remainingFactionForScrewsByPhase[phase].Add(GameConstants.SCREW_FACTION[currentFactionByPhase[phase]]);

                    if (i > 0 && (i + 1) % 3 == 0)
                    {
                        currentFactionByPhase[phase]++;

                        if (currentFactionByPhase[phase] >= GameConstants.SCREW_FACTION.Length)
                        {
                            currentFactionByPhase[phase] = 0;
                        }
                    }
                }
            }

            for (int i = 0; i < screws.Length; i++)
            {
                int phase = screws[i].Phase;

                int randomIndex = UnityEngine.Random.Range(0, remainingFactionForScrewsByPhase[phase].Count);

                screws[i].ScrewId = i;
                screws[i].Faction = remainingFactionForScrewsByPhase[phase][randomIndex];

                screws[i].ScrewServiceLocator.screwFaction.SetColorByFaction();

                remainingFactionForScrewsByPhase[phase].RemoveAt(randomIndex);
            }

            setMultiPhaseLevelScrewNumberEvent?.Invoke(screws.Length);
        }

        private void AutoAssignScrewFactionLevelManualGroup(GameObject level)
        {
            BaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<BaseScrew>(level.transform).ToArray();

            int numScrew = screws.Length;

            int numTripleScrews = numScrew / 3;

            List<GameFaction> factionPool = new List<GameFaction>();

            for (int i = 0; i < numTripleScrews; i++)
            {
                GameFaction randomFaction = GameConstants.SCREW_FACTION[UnityEngine.Random.Range(0, GameConstants.SCREW_FACTION.Length)];

                factionPool.Add(randomFaction);
            }

            ScrewFactionGroupManual[] screwFactionGroups = new ScrewFactionGroupManual[numScrew];

            for (int i = 0; i < numScrew; i++)
            {
                screwFactionGroups[i] = screws[i].GetComponent<ScrewFactionGroupManual>();
            }

            // screwFactionGroups = screwFactionGroups.OrderBy(item => item.Group).ToArray();

            for (int i = 0; i < numScrew; i++)
            {
                screws[i].ScrewId = i;
                screws[i].Faction = factionPool[screwFactionGroups[i].Group];

                screws[i].ScrewServiceLocator.screwFaction.SetColorByFaction();
            }

            setLevelScrewNumberEvent?.Invoke(numScrew);
        }

        private void AutoAssignScrewFaction(GameObject level)
        {
            BaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<BaseScrew>(level.transform).ToArray();

            int numScrew = screws.Length;

            List<BaseScrew> noBlockedScrews = new List<BaseScrew>();
            List<BaseScrew> blockedScrews = new List<BaseScrew>();

            for (int i = 0; i < screws.Length; i++)
            {
                int numBlockedObject = screws[i].CountBlockingObjects(isIncludeHiddenScrew: true);

                if (numBlockedObject == 0)
                {
                    noBlockedScrews.Add(screws[i]);
                }
                else
                {
                    blockedScrews.Add(screws[i]);
                }
            }

            // SHUFFLE SCREW LISTS
            ShuffleList(noBlockedScrews);
            ShuffleList(blockedScrews);

            List<BaseScrew> shuffledScrews = new List<BaseScrew>();

            float ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame = _currentLevelDifficultyConfiguration.RatioGeneratingSameFactionForTripleNoBlockedScrewStartGame;

            int numNoBlockedScrewsToGetSameFaction = (int)(ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame * noBlockedScrews.Count);
            int numRemainingScrews = numScrew - numNoBlockedScrewsToGetSameFaction;

            // EASY SCREWS
            for (int i = 0; i < numNoBlockedScrewsToGetSameFaction; i++)
            {
                BaseScrew nextScrew = noBlockedScrews.First();

                shuffledScrews.Add(nextScrew);

                noBlockedScrews.Remove(nextScrew);
            }

            // REMAINING SCREWS
            int currentIndex = 0;

            for (int i = 0; i < numRemainingScrews; i++)
            {
                BaseScrew nextScrew;

                if (currentIndex % (GameConstants.NUMBER_SLOT_PER_SCREW_BOX - 1) == 0)
                {
                    if (noBlockedScrews.Count > 0)
                    {
                        nextScrew = noBlockedScrews.First();

                        noBlockedScrews.Remove(nextScrew);
                    }
                    else
                    {
                        nextScrew = blockedScrews.First();

                        blockedScrews.Remove(nextScrew);
                    }
                }
                else
                {
                    if (blockedScrews.Count > 0)
                    {
                        nextScrew = blockedScrews.First();

                        blockedScrews.Remove(nextScrew);
                    }
                    else
                    {
                        nextScrew = noBlockedScrews.First();

                        noBlockedScrews.Remove(nextScrew);
                    }
                }

                shuffledScrews.Add(nextScrew);

                currentIndex++;
            }

            int currentFaction = 0;

            List<GameFaction> remainingFactionForBlockedScrews = new List<GameFaction>();

            for (int i = 0; i < shuffledScrews.Count; i++)
            {
                remainingFactionForBlockedScrews.Add(GameConstants.SCREW_FACTION[currentFaction]);

                if (i > 0 && (i + 1) % 3 == 0)
                {
                    currentFaction++;

                    if (currentFaction >= GameConstants.SCREW_FACTION.Length)
                    {
                        currentFaction = 0;
                    }
                }
            }

            for (int i = 0; i < shuffledScrews.Count; i++)
            {
                GameFaction nextFaction = remainingFactionForBlockedScrews.First();

                shuffledScrews[i].ScrewId = i;
                shuffledScrews[i].Faction = nextFaction;

                shuffledScrews[i].ScrewServiceLocator.screwFaction.SetColorByFaction();

                remainingFactionForBlockedScrews.Remove(nextFaction);
            }

            setLevelScrewNumberEvent?.Invoke(shuffledScrews.Count);
        }
        #endregion

        #region AUTO CENTER LEVEL
        private async void OnPieceCompleted(int pieceInstanceId)
        {
            DebugLogger.LogSimple();
            // if (_isNotAutoCenterLevel)
            // {
            //     return;
            // }

            await Task.Delay(500);

            try
            {
                Vector3 currentPosition = _pivotAdjust.transform.position;
                Vector3 currentRotation = _pivotAdjust.transform.eulerAngles;

                _pivotAdjust.transform.position = Vector3.zero;
                _pivotAdjust.transform.eulerAngles = Vector3.zero;

                Vector3 center = GetCenterOfObjectsWithBoundsAndPivot(_pieces, offset: GamePersistentVariable.levelOffset, isLevelDirty: true);

                _pivotAdjust.transform.position = currentPosition;
                _pivotAdjust.transform.eulerAngles = currentRotation;

                Vector3 newCenter = -center / _currentLevelContainer.transform.localScale.x;

                CommonUtil.StopAllTweens(_autoCenterLevelTweens);

                _autoCenterLevelTweens.Add(Tween.LocalPosition(_pivotAdjust, newCenter, duration: 0.3f, ease: Ease.InOutSine));
            }
            catch (System.Exception)
            {

            }
        }

        Vector3 GetCenterOfObjectsWithBoundsAndPivot(BasicObjectPart[] objects, Vector3 offset, bool isLevelDirty)
        {
            if (objects.Length == 0)
            {
                return Vector3.zero;
            }

            float minX = float.MaxValue;
            float maxX = float.MinValue;

            float minY = float.MaxValue;
            float maxY = float.MinValue;

            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            // temp set pivot adjust
            Vector3 currentPosition = _pivotAdjust.transform.position;
            Vector3 currentRotation = _pivotAdjust.transform.eulerAngles;

            _pivotAdjust.transform.position = Vector3.zero;
            _pivotAdjust.transform.eulerAngles = Vector3.zero;

            transform.rotation = Quaternion.identity;

            float pieceLossyScale = objects[0].transform.lossyScale.x;

            foreach (BasicObjectPart obj in objects)
            {
                if (!obj.transform.IsChildOf(_pivotAdjust))
                {
                    continue;
                }

                if (!obj.gameObject.activeSelf)
                {
                    continue;
                }

                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();

                Vector3 objectPosition = obj.transform.position + meshFilter.mesh.bounds.center * pieceLossyScale;

                float boundXMin = objectPosition.x - 0.5f * obj.BoundSize.x;
                float boundXMax = objectPosition.x + 0.5f * obj.BoundSize.x;
                float boundYMin = objectPosition.y - 0.5f * obj.BoundSize.y;
                float boundYMax = objectPosition.y + 0.5f * obj.BoundSize.y;
                float boundZMin = objectPosition.z - 0.5f * obj.BoundSize.z;
                float boundZMax = objectPosition.z + 0.5f * obj.BoundSize.z;

                minX = Mathf.Min(boundXMin, minX);
                minY = Mathf.Min(boundYMin, minY);
                minZ = Mathf.Min(boundZMin, minZ);

                maxX = Mathf.Max(boundXMax, maxX);
                maxY = Mathf.Max(boundYMax, maxY);
                maxZ = Mathf.Max(boundZMax, maxZ);
            }

            _pivotAdjust.transform.position = currentPosition;
            _pivotAdjust.transform.eulerAngles = currentRotation;

            // Set Shader World Bound
            Vector3 currentCenter = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2) + offset;
            Vector3 newCenter = -currentCenter / _currentLevelContainer.transform.localScale.x;

            if (!_isSetLevelGradientBound)
            {
                SetLevelGradientMinMaxIn(newCenter, minY, maxY);
            }

            SetFitCameraOrthographicSize(new Vector3(maxX - minX, maxY - minY, maxZ - minZ), isLevelDirty);

            // float halfDistance = (maxY - minY) / 2;
            // float shaderMinY = newCenter.y - halfDistance;
            // float shaderMaxY = newCenter.y + halfDistance;

            // if (!_isSetLevelGradientBound)
            // {
            //     if (_pieces != null)
            //     {
            //         for (int i = 0; i < _pieces.Length; i++)
            //         {
            //             if (_pieces[i] != null)
            //             {
            //                 _pieces[i].PieceMaterialPropertyBlock.SetWorldBound(shaderMinY, shaderMaxY);
            //             }
            //         }

            //         _isSetLevelGradientBound = true;
            //     }
            // }

            return new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2) + offset;
        }
        #endregion

        #region LEVEL SETUP
        private void SetFitCameraOrthographicSize(Vector3 expectedLevelSize, bool isLevelDirty)
        {
            float maxExpectedLevelSize = expectedLevelSize.x;

            if (expectedLevelSize.y > maxExpectedLevelSize)
            {
                maxExpectedLevelSize = expectedLevelSize.y;
            }

            if (expectedLevelSize.z > maxExpectedLevelSize)
            {
                maxExpectedLevelSize = 1f * expectedLevelSize.z;
            }

            // the higher targetOrthographicSize, the smaller model
            float targetOrthographicSize;

            targetOrthographicSize = 1.5f * expectedLevelSize.y;

            if (maxExpectedLevelSize > expectedLevelSize.y)
            {
                float ratio = (maxExpectedLevelSize / expectedLevelSize.y);

                targetOrthographicSize *= (1 + 0.25f * ratio * ratio);
            }

            if (!isLevelDirty)
            {
                try
                {
                    ManualLevelZoomMultiplier manualLevelZoomMultiplier = transform.GetChild(0).GetComponent<ManualLevelZoomMultiplier>();

                    if (manualLevelZoomMultiplier != null)
                    {
                        targetOrthographicSize *= manualLevelZoomMultiplier.zoomMultiplier;
                    }
                }
                catch (System.Exception)
                {

                }
            }

            if (isLevelDirty)
            {
                if (targetOrthographicSize > _mainCamera.orthographicSize || targetOrthographicSize < 6f)
                {
                    return;
                }
            }

            setTargetOrthographicSizeEvent?.Invoke(targetOrthographicSize, isLevelDirty);

            GamePersistentVariable.currentTargetOrthographicSize = targetOrthographicSize;
        }

        private bool AreRoughlySimilar(float a, float b)
        {
            if (a < b)
            {
                if (a > 0.75f * b)
                {
                    return true;
                }
            }
            else
            {
                if (b > 0.75f * a)
                {
                    return true;
                }
            }

            return false;
        }

        private void SetLevelGradientMinMaxIn(Vector3 levelCenter, float minWorldY, float maxWorldY)
        {
            float halfDistance = (maxWorldY - minWorldY) / 2;
            float shaderMinY = levelCenter.y - halfDistance;
            float shaderMaxY = levelCenter.y + halfDistance;

            shaderMinY = 3 * Mathf.Min(shaderMinY, 0);
            shaderMaxY = 3 * Mathf.Max(shaderMaxY, 0);

            if (!_isSetLevelGradientBound)
            {
                if (_pieces != null)
                {
                    for (int i = 0; i < _pieces.Length; i++)
                    {
                        if (_pieces[i] != null)
                        {
                            _pieces[i].PieceMaterialPropertyBlock.SetWorldBound(shaderMinY, shaderMaxY);
                        }
                    }

                    _isSetLevelGradientBound = true;
                }
            }
        }
        #endregion

        #region BOOSTER SUGGESTION

        private LevelMode _tutorialMode = LevelMode.Normal;
        private void ShowSuggestion(int nextLevel, bool isLoopLevel)
        {
            int randomSuggestionIndex = UnityEngine.Random.Range(0, 2);
            int currentLevel = nextLevel - 1;

            try
            {
                LevelMode currentLevelMode = levelDataContainer.LevelsData[currentLevel].levelMode;

                if (levelDataContainer.LevelsData[currentLevel].isHasTutorial)
                {
                    GamePersistentVariable.currentTutLevelMode = currentLevelMode;

                    specialTutIntroduceEvent?.Invoke(true);
                    DelayShowTutorialSuggestion();
                    return;
                }
                else
                {
                    specialTutIntroduceEvent?.Invoke(false);
                }
            }
            catch (Exception e)
            {
                specialTutIntroduceEvent?.Invoke(false);
            }

            if (randomSuggestionIndex == 0)
            {
                if (IsShowDoubleScrewBoxesSuggestion(nextLevel))
                {
                    DelayShowDoubleScrewBoxesSuggestion();
                }
                else
                {
                    if (IsShowIAPSuggestion(nextLevel, isLoopLevel))
                    {
                        DelayShowIAPSuggestion();
                    }
                }
            }
            else
            {
                if (IsShowIAPSuggestion(nextLevel, isLoopLevel))
                {
                    DelayShowIAPSuggestion();
                }
                else
                {
                    if (IsShowDoubleScrewBoxesSuggestion(nextLevel))
                    {
                        DelayShowDoubleScrewBoxesSuggestion();
                    }
                }
            }
        }

        private bool IsShowBoosterSuggestion(int level, bool isLoopLevel)
        {
            // if (isLoopLevel)
            // {
            //     return false;
            // }

            bool remoteConfigIsShowBoosterSuggestion = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_BOOSTER_SUGGESTION, false);
            int numLevelShowBoosterSuggestion = RemoteConfigController.GetIntConfig(FirebaseConfig.LEVEL_INTERVAL_SHOW_BOOSTER_SUGGESTION, 2);

            if (!remoteConfigIsShowBoosterSuggestion)
            {
                return false;
            }

            if (level <= 3 || level % numLevelShowBoosterSuggestion != 0)
            {
                return false;
            }

            return true;
        }

        private bool IsShowDoubleScrewBoxesSuggestion(int level)
        {
            bool remoteConfigIsShowBoosterSuggestion = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_DOUBLE_SCREW_BOXES_SUGGESTION, false);
            int numLevelShowBoosterSuggestion = RemoteConfigController.GetIntConfig(FirebaseConfig.LEVEL_INTERVAL_SHOW_BOOSTER_SUGGESTION, 1);

            if (!remoteConfigIsShowBoosterSuggestion)
            {
                return false;
            }

            if (DataUtility.Load(GameConstants.IS_UNLOCKED_ALL_SCREW_BOXES, false))
            {
                return false;
            }

            if (level <= 3 || level % numLevelShowBoosterSuggestion != 0)
            {
                return false;
            }

            if (!GamePersistentVariable.isEnableRewardedAds)
            {
                return false;
            }

            return true;
        }

        private bool IsShowIAPSuggestion(int level, bool isLoopLevel)
        {
            bool isBoughtHotDeal = DataUtility.Load(GameConstants.IS_BOUGHT_HOT_DEAL, false);

            if (isBoughtHotDeal)
            {
                return false;
            }

            bool remoteConfigIsShowIAPSuggestion = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_IAP_SUGGESTION, true);
            int minLevelShow = RemoteConfigController.GetIntConfig(FirebaseConfig.MIN_LEVEL_SHOW_IAP_SUGGESTION, 10);
            int numLevelShowBoosterSuggestion = RemoteConfigController.GetIntConfig(FirebaseConfig.LEVEL_INTERVAL_SHOW_BOOSTER_SUGGESTION, 2);

            if (!remoteConfigIsShowIAPSuggestion)
            {
                return false;
            }

            if (level < minLevelShow || level % numLevelShowBoosterSuggestion != 0)
            {
                return false;
            }

            return true;
        }

        private void DelayShowBoosterSuggest()
        {
            CommonUtil.StopAllTweens(_tweens);

            _tweens.Add(Tween.Delay(3).OnComplete(() =>
            {
                switchRouteEvent?.Invoke(ScreenRoute.BoosterSuggest);
            }));
        }

        private void DelayShowDoubleScrewBoxesSuggestion()
        {
            CommonUtil.StopAllTweens(_tweens);

            switchRouteEvent?.Invoke(ScreenRoute.UnlockDoubleScrewBoxesSuggestion);

            _tweens.Add(Tween.Delay(1).OnComplete(() =>
            {
                switchRouteEvent?.Invoke(ScreenRoute.UnlockDoubleScrewBoxesSuggestion);
            }));
        }

        private void DelayShowIAPSuggestion()
        {
            CommonUtil.StopAllTweens(_tweens);

            switchRouteEvent?.Invoke(ScreenRoute.IAPSuggestion);

            _tweens.Add(Tween.Delay(1).OnComplete(() =>
            {
                switchRouteEvent?.Invoke(ScreenRoute.IAPSuggestion);
            }));
        }

        private void DelayShowTutorialSuggestion()
        {
            DebugLogger.LogSimple(context: this);
            CommonUtil.StopAllTweens(_tweens);

            _tweens.Add(Tween.Delay(.3f).OnComplete(() =>
            {
                switchRouteEvent?.Invoke(ScreenRoute.SpecialLevelTutorial);
            }));
        }
        #endregion

        #region HARD MODE
        private void SuggestHardMode()
        {
            _tweens.Add(Tween.Delay(3).OnComplete(() =>
            {
                switchRouteEvent?.Invoke(ScreenRoute.ChooseDifficulty);
            }));
        }

        private void OnHardModeSuggestionClosed()
        {
            DebugLogger.LogSimple();
            ShowSuggestion(_nextLevel, _isLoopLevel);
        }
        #endregion

        #region UTIL
        private int GetLoopLevel()
        {
            int currentLoopLevel = DataUtility.Load(GameConstants.CURRENT_LEVEL_LOOP, -1);

            _isNewLoopLevel = currentLoopLevel == -1;

            if (currentLoopLevel == -1)
            {
                int randomLoopLevel = UnityEngine.Random.Range(6, _maxLevel);

                DataUtility.Save(GameConstants.CURRENT_LEVEL_LOOP, randomLoopLevel);

                return randomLoopLevel;
            }
            else
            {
                return currentLoopLevel;
            }
        }

        private void RandomizeLevelModelColor(GameObject level)
        {
            DebugLogger.LogSimple();
            BasicObjectPart[] objectParts = TransformUtil.GetComponentsFromAllChildren<BasicObjectPart>(level.transform).ToArray();
            Renderer[] objectPartRenderers = new Renderer[objectParts.Length];

            // MATERIALS
            List<Material> materialPool = objectPartMaterialContainer.Materials.ToList();

            List<Material> piecesMaterials = new List<Material>();

            for (int i = 0; i < objectParts.Length; i++)
            {
                objectPartRenderers[i] = objectParts[i].GetComponent<MeshRenderer>();
            }

            Material material = null;

            float randomTilingX = UnityEngine.Random.Range(0, 129) / 129f;
            float randomTilingY = UnityEngine.Random.Range(0, 129) / 129f;

            bool isUsingTexturedMaterial = false;

            if (objectPartRenderers[0].sharedMaterial.name == "Wood Texture Master")
            {
                isUsingTexturedMaterial = true;
            }

            if (isUsingTexturedMaterial)
            {
                for (int i = 0; i < objectParts.Length; i++)
                {
                    objectPartRenderers[i].sharedMaterial.SetVector("_MainTextureOffset", new Vector2(randomTilingX, randomTilingY));
                }

                return;
            }

            // Use gradient materials
            for (int i = 0; i < objectParts.Length; i++)
            {
                if (!materialPool.Contains(objectPartRenderers[i].sharedMaterial))
                {
                    return;
                }

                if (!piecesMaterials.Contains(objectPartRenderers[i].sharedMaterial))
                {
                    piecesMaterials.Add(objectPartRenderers[i].sharedMaterial);
                }
            }

            Dictionary<Material, int> newMaterialIndexes = new Dictionary<Material, int>();

            const int NUM_GRADIENT_MATERIAL = 9;

            // Random new material for each material index
            foreach (var pieceMaterial in piecesMaterials)
            {
                newMaterialIndexes.Add(
                    pieceMaterial, UnityEngine.Random.Range(0, materialPool.Count - NUM_GRADIENT_MATERIAL));
            }

            for (int i = 0; i < objectParts.Length; i++)
            {
                int newMaterialIndex = newMaterialIndexes[objectPartRenderers[i].sharedMaterial];

                objectPartRenderers[i].sharedMaterial = materialPool[newMaterialIndex];
            }
        }

        void ShuffleList<T>(List<T> list)
        {
            System.Random rand = new System.Random();
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        #endregion
    }
}
