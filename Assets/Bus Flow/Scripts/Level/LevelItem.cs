using UnityEngine;
using static GameEnum;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using AntsFlow;
using Dreamteck.Splines;






#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AntsFlow
{
    public class LevelItem : MonoBehaviour
    {
        public Sprite picture;

        public int numColumn;
        public int numRow;
        public bool isLevelHavingTransparentPixels;
        public bool isOverrideLevelScale;
        public float levelScaleXRatio;
        public float levelScaleYRatio;
        public float multiplierScaleX;
        public float multiplierScaleY;
        public List<Color> poolFaction;
        public List<BaseTile> tiles;
        public bool isTutorialLevel;
        public bool isTutorialSpeedUpLevel;
        public bool isUseManualFactionSetupForFirstAntBoxes;
        public LevelFactionConfig levelFactionConfig;
        public int difficulty;
        public int initialDifficulty;
        public bool overrideInitialDifficulty;
        public int numStartAntBoxes = 6;

        public List<int> easySpawnOrder;

        public SpecialElementConfig specialElementConfig;

        public Vector3 spawnPosition = new Vector3(0, 3, 0);
        public Vector3 spawnScale = 0.6f * Vector3.one;

        public List<Color> factions;

        [Header("Bus Coordinate Space")]
        // Spacing DÙNG CHUNG giữa lúc vẽ spline (SimulateMap/DrawSampleBusPath) và lúc đặt khách runtime.
        // Phải cùng 1 giá trị thì spline mới khớp vị trí khách (cùng hệ toạ độ tile.position * spacing).
        // Bake ra JSON để runtime dùng đúng giá trị đã vẽ.
        public Vector2 busTileSpacing = new Vector2(1.6f, 1.6f);

        // Hằng số hệ toạ độ khách/spline dùng chung cho CẢ editor (SimulateMap, DrawSampleBusPath) lẫn
        // runtime (PassengerBoardManager). Công thức: local = ((x - centerCol) * sx, (y - minRow) * sy
        // + BusBoardOriginY). Đổi thì phải đổi 1 chỗ này, mọi nơi tự theo.
        public const float BusBoardOriginY = 3.5f;

        [Header("Bus Auto-Fit Ratios (dùng chung editor + runtime)")]
        [Range(0.3f, 1f)] public float fitWidthRatio = 0.72f;
        [Range(0.3f, 1f)] public float fitHeightRatio = 0.42f;
        [Range(0f, 1f)] public float fitCenterViewportY = 0.7f;
        [Range(0.3f, 1f)] public float garageWidthRatio = 0.82f;
        [Range(0f, 0.5f)] public float garageViewportY = 0.2f;

        public static Vector3 BusTileToLocal(float tileX, float tileY, float centerCol, float minRow, Vector2 spacing)
        {
            return new Vector3(
                (tileX - centerCol) * spacing.x,
                (tileY - minRow) * spacing.y + BusBoardOriginY,
                0f);
        }

        [Header("Bus Path")]
        // Đặt các SplineComputer làm con của LevelItem trong prefab level để vẽ đường đi cho xe bus
        // (nhiều path/hình dạng khác nhau trong 1 level). Convert to JSON sẽ bake điểm của spline
        // thành busPathConfig để runtime dùng, không phụ thuộc component Dreamteck lúc load level.
        public List<BusPathAuthoring> busPaths;

        // Chỉ có giá trị khi LevelItem được dựng lúc runtime từ JSON (LevelLoader.JsonToLevelItem) -
        // không có SplineComputer sống nên gán thẳng điểm đã bake vào đây. Test thủ công bằng prefab
        // thật (kéo Level 1.prefab vào BusFlowGameManager) thì field này null, dùng busPaths thay thế.
        [NonSerialized] public BusPathConfig runtimeBusPathConfig;

        [Header("Path Area Preview (chỉ để Simulate Map vẽ gợi ý, không phải data gameplay)")]
        [Tooltip("Bề rộng vùng path preview. Nếu nhỏ hơn bề rộng map thì tự lấy theo bề rộng map.")]
        public float pathAreaWidth = 10f;
        [Tooltip("Chiều cao vùng path preview.")]
        public float pathAreaHeight = 8f;
        [Tooltip("Đẩy mép trên của vùng path đè lên map bao nhiêu đơn vị, tính từ hàng dưới cùng của map (0 = sát mép dưới, không đè vào map).")]
        public float pathAreaOverlapIntoMap = 3f;

        [Header("Bus Garage Queue")]
        [Tooltip("Số cột trong bãi đỗ (hàng chờ) xe bus. Row 0 = hàng đầu, duy nhất pick được.")]
        public int busGarageNumColumns = 4;
        [Tooltip("Tổng số hàng vật lý của bãi đỗ (bao gồm cả những ô ban đầu chưa có xe, sẽ sinh dần khi cột phía sau hết xe).")]
        public int busGarageNumRows = 6;
        [Tooltip("Số xe được sinh sẵn ngay lúc bắt đầu level (giống numStartAntBoxes bên Ants Flow). Phần còn lại của bãi đỗ sinh dần khi pick.")]
        public int numStartBuses = 8;
        [Tooltip("Tổng sức chứa cần có của mỗi faction (cộng dồn qua toàn bộ xe sẽ sinh ra trong suốt level) - không phải danh sách xe cụ thể, xe cụ thể được chia nhỏ dần lúc runtime.")]
        public List<BusGarageItemConfig> busGarageQueue;

        [Header("Extra Passengers (backlog vượt quá lưới ban đầu)")]
        [Tooltip("Lặp lại đúng phân bố màu hiện có trên map thêm bấy nhiêu vòng làm backlog, khách mới sẽ rơi vào ô trống trên cùng của mỗi cột sau khi dồn xuống.")]
        public int extraPassengerRounds = 1;

        public void FindTileBlockages()
        {
            // BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(transform);

            // for (int i = 0; i < tiles.Length; i++)
            // {
            //     BaseTile tile = tiles[i];

            //     tile.blockages = new List<BaseTile>();

            //     BoxCollider2D boxCollider = tile.GetComponent<BoxCollider2D>();

            //     FindBlockage(tile, boxCollider, tile.transform.up);
            //     FindBlockage(tile, boxCollider, tile.transform.right);
            //     FindBlockage(tile, boxCollider, -tile.transform.up);
            //     FindBlockage(tile, boxCollider, -tile.transform.right);
            // }

#if UNITY_EDITOR
            EditorUtility.SetDirty(transform);
#endif

            void FindBlockage(BaseTile tile, BoxCollider2D collider, Vector2 direction)
            {
                RaycastHit2D[] hits = new RaycastHit2D[1];

                collider.Raycast(direction, hits);

                if (hits.Length > 0 && hits[0].collider != null)
                {
                    BaseTile blockage = hits[0].collider.GetComponent<BaseTile>();

                    if (blockage != null)
                    {
                        tile.blockages.Add(blockage);
                    }
                }
            }
        }

        public void ConvertToJson()
        {
#if UNITY_EDITOR
            // Đảm bảo transform đang ở phép fit trước khi bake spline (BuildBusPathConfig dùng
            // InverseTransformPoint(transform)) để điểm bake ra đúng hệ toạ độ runtime. Nếu chưa chạy
            // Simulate Map thì set luôn ở đây.
            {
                BusFlow.BusBoardFit.Fit fit = BusFlow.BusBoardFit.ComputeBoard(
                    Camera.main, tiles, busTileSpacing, Vector3.zero,
                    fitWidthRatio, fitHeightRatio, fitCenterViewportY);
                transform.position = fit.origin;
                transform.localScale = fit.scale * Vector3.one;
            }

            FactionMapper factionMapper =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ants Flow/Prefabs/Faction Mapper.prefab")?.GetComponent<FactionMapper>();

            AntLevelData antLevelData = new AntLevelData();

            antLevelData.antLevelDataItems = new List<AntLevelDataItem>();

            for (int i = 0; i < tiles.Count; i++)
            {
                BaseTile tile = tiles[i];

                AntLevelDataItem dataItem = new AntLevelDataItem();
                dataItem.position = tile.position;
                dataItem.factionIndex = factionMapper.mapper.FindIndex(item => item.color == tile.Faction);

                dataItem.blockageIndexes = new List<int>();

                if (tile.blockages != null)
                {
                    for (int j = 0; j < tile.blockages.Count; j++)
                    {
                        dataItem.blockageIndexes.Add(tiles.IndexOf(tile.blockages[j]));
                    }
                }

                antLevelData.antLevelDataItems.Add(dataItem);
            }

            List<int> poolFactionIndexes = factionMapper.GetPoolFactionIndexFromPoolFaction(poolFaction);

            antLevelData.levelItemData = new LevelItemData()
            {
                levelScale = transform.localScale,
                numColumn = numColumn,
                numRow = numRow,
                isLevelHavingTransparentPixels = isLevelHavingTransparentPixels,
                isOverrideLevelScale = isOverrideLevelScale,
                levelScaleXRatio = levelScaleXRatio,
                levelScaleYRatio = levelScaleYRatio,
                multiplierScaleX = multiplierScaleX,
                multiplierScaleY = multiplierScaleY,
                poolFactionIndexes = poolFactionIndexes,
                isTutorialLevel = isTutorialLevel,
                isUseManualFactionSetupForFirstAntBoxes = isUseManualFactionSetupForFirstAntBoxes,
                levelFactionConfig = levelFactionConfig,
                difficulty = difficulty,
                initialDifficulty = initialDifficulty,
                overrideInitialDifficulty = overrideInitialDifficulty,
                numStartAntBoxes = numStartAntBoxes,
                easySpawnOrder = easySpawnOrder,
                specialElementConfig = specialElementConfig,
                busPathConfig = BuildBusPathConfig(),
                busGarageNumColumns = busGarageNumColumns,
                busGarageNumRows = busGarageNumRows,
                numStartBuses = numStartBuses,
                busGarageQueue = busGarageQueue,
                extraPassengerRounds = extraPassengerRounds,
                busTileSpacing = busTileSpacing
            };

            // Use level name as file name
            string filePath = Path.Combine("Assets/Bus Flow/JSON/Levels", $"{name}.json");

            // Serialize and save
            string json = JsonUtility.ToJson(antLevelData, false);
            File.WriteAllText(filePath, json);

            AssetDatabase.Refresh();
#endif
        }

        public void ClearAllSpecialElements()
        {
            specialElementConfig = new SpecialElementConfig();
        }

        public void RandomizeSpecialElements()
        {
            specialElementConfig = new SpecialElementConfig();
        }

        private BusPathConfig BuildBusPathConfig()
        {
            BusPathConfig config = new BusPathConfig();
            config.paths = new List<BusPathItemConfig>();

            if (busPaths == null)
            {
                return config;
            }

            for (int i = 0; i < busPaths.Count; i++)
            {
                BusPathAuthoring authoring = busPaths[i];

                if (authoring == null || authoring.spline == null)
                {
                    continue;
                }

                SplinePoint[] splinePoints = authoring.spline.GetPoints();

                BusPathItemConfig itemConfig = new BusPathItemConfig();
                itemConfig.id = authoring.id;
                itemConfig.role = authoring.role;
                itemConfig.isLoop = authoring.isLoop;
                itemConfig.points = new List<Vector3>();

                // Bake về local space của LevelItem để không phụ thuộc SplineComputer lúc runtime
                // (level được load lại từ JSON, không giữ prefab con gốc).
                for (int p = 0; p < splinePoints.Length; p++)
                {
                    itemConfig.points.Add(transform.InverseTransformPoint(splinePoints[p].position));
                }

                config.paths.Add(itemConfig);
            }

            return config;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LevelItem))]
    public class LevelItemEditor : Editor
    {
        private string hexInput;

        // Change these to match your enum values
        private int startX = 0;
        private int startY = 0;
        private int size = 16;
        private Vector2 tileSpacing = 0.48f * Vector2.one;
        private string json;
        private float colorGroupThreshold = 0.05f;
        public float difficulty;
        public float moveTilesCloserFactor = 0.9f;
        public FactionMapper factionMapper;

        #region Snake
        private bool snakeConfigMode;
        private bool isPainting;
        private bool isShowingNodeId;
        private bool paintValue; // true = add, false = remove
        private readonly HashSet<int> paintedThisDrag = new();
        #endregion

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelItem levelItem = (LevelItem)target;

            GUILayout.Space(10);

            GUILayout.Label("Generate Level", EditorStyles.boldLabel);

            // startX = EditorGUILayout.IntField("StartX", startX);
            // startY = EditorGUILayout.IntField("StartY", startY);
            // size = EditorGUILayout.IntField("Size", size);
            tileSpacing = EditorGUILayout.Vector2Field("Tile Spacing", tileSpacing);

            EditorGUILayout.LabelField("Import Color Palette", EditorStyles.boldLabel);

            hexInput = EditorGUILayout.TextArea(hexInput, GUILayout.MinHeight(180));

            // colorGroupThreshold = EditorGUILayout.FloatField("colorGroupThreshold", colorGroupThreshold);
            // moveTilesCloserFactor = EditorGUILayout.FloatField("moveTilesCloserFactor", moveTilesCloserFactor);
            // factionMapper = (FactionMapper)EditorGUILayout.ObjectField("Faction Mapper", factionMapper, typeof(FactionMapper), true);

            // GUILayout.Label("Import Level JSON", EditorStyles.boldLabel);

            // json = EditorGUILayout.TextArea(json, GUILayout.MinHeight(200));

            // if (GUILayout.Button("Spawn Pixel Art"))
            // {
            //     SpawnPixelArt(levelItem);
            // }

            // if (GUILayout.Button("GenerateLevelFromJson"))
            // {
            //     GenerateLevelFromJson(levelItem, json);
            // }

            if (GUILayout.Button("Auto Fill Factions"))
            {
                FillFactionPool();
            }

            if (GUILayout.Button("GENERATE LEVEL"))
            {
                GenerateLevel(levelItem);
            }

            if (GUILayout.Button("Convert to JSON"))
            {
                levelItem.ConvertToJson();
            }

            if (GUILayout.Button("Clear All Special Elements"))
            {
                levelItem.ClearAllSpecialElements();
            }

            GUILayout.Space(10);

            GUILayout.Label("Simulate Map (Preview)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Chưa có asset 3D thật. Dùng tạm sprite tile (FactionMapper) làm hành khách, " +
                "prefab Ant Box (WashingMachineItem) làm xe bus, chỉ để xem layout — không có gameplay thật.",
                MessageType.Info);

            if (GUILayout.Button("Simulate Map"))
            {
                SimulateMap(levelItem, levelItem.busTileSpacing);
            }

            if (GUILayout.Button("Clear Map Preview"))
            {
                ClearMapPreview(levelItem);
            }

            EditorGUILayout.HelpBox(
                "Tự dựng 1 spline hình chữ U mẫu bằng SetPoints() (không cần vẽ tay), theo đúng " +
                "kích thước map + Path Area hiện tại. Dùng để có cái nhìn nhanh rồi tự chỉnh lại điểm sau.",
                MessageType.Info);

            if (GUILayout.Button("Draw Sample Bus Path (U-shape)"))
            {
                DrawSampleBusPath(levelItem, levelItem.busTileSpacing);
            }

            EditorGUILayout.HelpBox(
                "Tự dựng thêm 1 spline role=1 (đường thoát) bắt đầu từ đúng điểm cuối của path đón " +
                "khách (role=0), dẫn ra khỏi bãi đỗ. Cần cả 2 path (role 0 + role 1) thì level mới " +
                "đủ data để chơi được (xe hết khách/hết đường đón sẽ theo path này để thoát ra).",
                MessageType.Info);

            if (GUILayout.Button("Draw Sample Exit Path"))
            {
                DrawSampleExitPath(levelItem, levelItem.busTileSpacing);
            }

            if (GUILayout.Button("Close Bus Paths (Loop)"))
            {
                CloseBusPaths(levelItem);
            }

            EditorGUILayout.HelpBox(
                "Tự tính TỔNG sức chứa cần có mỗi faction (đủ đón hết khách kể cả backlog). " +
                "Việc chia thành từng xe cụ thể + chọn màu theo độ khó do BusGarageManager làm " +
                "dần lúc runtime, không sinh sẵn hết một lúc (giống WashingMachineManager).",
                MessageType.Info);

            if (GUILayout.Button("Auto Fill Bus Garage"))
            {
                AutoFillBusGarage(levelItem);
            }

            if (GUILayout.Button(
            snakeConfigMode
                ? "Disable Snake Config Mode"
                : "Enable Snake Config Mode"))
            {
                snakeConfigMode = !snakeConfigMode;

                // Force Scene view repaint
                SceneView.RepaintAll();
            }

            if (GUILayout.Button(
            isShowingNodeId
                ? "Hide Node Id"
                : "Show Node Id"))
            {
                isShowingNodeId = !isShowingNodeId;

                // Force Scene view repaint
                SceneView.RepaintAll();
            }

            // if (GUILayout.Button("Step 1: ReadLevel"))
            // {
            //     ReadLevel(levelItem);
            // }

            // if (GUILayout.Button("Step 2: GroupColors"))
            // {
            //     GroupColors(levelItem);
            // }

            // if (GUILayout.Button("Step 3: ReassignFactionForTiles"))
            // {
            //     ReassignFactionForTiles(levelItem);
            // }

            // if (GUILayout.Button("Step 4: AssignLevelPriority"))
            // {
            //     AssignLevelPriority(levelItem);
            // }

            // if (GUILayout.Button("Step 5: Find Tile Blockages"))
            // {
            //     levelItem.FindTileBlockages();
            // }

            // if (GUILayout.Button("Step 6: Get Pool Faction"))
            // {
            //     GetPoolFaction(levelItem);
            // }

            // if (GUILayout.Button("Center On X"))
            // {
            //     CenterOnX(levelItem);
            // }

            // if (GUILayout.Button("Step 8: MoveTilesCloserTogether"))
            // {
            //     MoveTilesCloserTogether(levelItem, moveTilesCloserFactor);
            // }

            // GUILayout.Label("Preview Level", EditorStyles.boldLabel);

            // if (GUILayout.Button("Preview Level"))
            // {
            //     PreviewLevel(levelItem);
            // }

            // GUILayout.Label("Manual Level Setup", EditorStyles.boldLabel);

            // if (GUILayout.Button("FillTheRestOfLevel"))
            // {
            //     FillTheRestOfLevel(levelItem);
            // }
        }

        private void OnSceneGUI()
        {
            ConfiguringSnake();
        }

        private void ConfiguringSnake()
        {
            if (!snakeConfigMode)
                return;

            LevelItem level = (LevelItem)target;

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            Event e = Event.current;

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                isPainting = false;
                paintedThisDrag.Clear();
            }

            int index = 0;

            int numColumn = level.numColumn + 2;
            int numRow = level.numRow + 2;
            float tileDistance = 0.1f;

            BaseTile[,] tileAtCoordinate = new BaseTile[numColumn, numRow];

            for (int i = 0; i < level.tiles.Count; i++)
            {
                BaseTile tile = level.tiles[i];
                tileAtCoordinate[(int)tile.position.x + 1, (int)tile.position.y + 1] = tile;
            }

            // Background
            float backgroundWidth = 10;

            Vector3[] background =
            {
        new Vector3(-backgroundWidth, -backgroundWidth, 1),
        new Vector3(-backgroundWidth, +backgroundWidth, 1),
        new Vector3(+backgroundWidth, +backgroundWidth, 1),
        new Vector3(+backgroundWidth, -backgroundWidth, 1),
    };

            Handles.DrawSolidRectangleWithOutline(
                background,
                new Color(0f, 0f, 0f, 0.85f),
                Color.white);

            // Highlight Hard Pixels
            if (level.specialElementConfig != null && level.specialElementConfig.hardPixelConfigs != null)
            {
                foreach (var config in level.specialElementConfig.hardPixelConfigs)
                {
                    if (config.startNodeId >= 0)
                    {
                        int startX = config.startNodeId % numColumn;
                        int startY = config.startNodeId / numColumn;

                        float left = (startX - 0.5f) * tileDistance;
                        float right = (startX + config.numCols - 0.5f) * tileDistance;
                        float bottom = (startY - 0.5f) * tileDistance;
                        float top = (startY + config.numRows - 0.5f) * tileDistance;

                        Vector3[] corners =
                        {
                            new Vector3(left, bottom, 0.5f),
                            new Vector3(left, top, 0.5f),
                            new Vector3(right, top, 0.5f),
                            new Vector3(right, bottom, 0.5f)
                        };

                        Handles.DrawSolidRectangleWithOutline(
                            corners,
                            new Color(1f, 0.5f, 0f, 0.25f), // Orange fill with transparency
                            new Color(1f, 0.5f, 0f, 1f)    // Solid orange outline
                        );

                        Vector3 centerPos = new Vector3((left + right) * 0.5f, (bottom + top) * 0.5f, 0);
                        GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteBoldLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 11
                        };
                        labelStyle.normal.textColor = new Color(1f, 0.7f, 0f, 1f);
                        Handles.Label(centerPos + Vector3.up * 0.02f, $"HP {config.id}\n({config.numCols}x{config.numRows})", labelStyle);
                    }
                }
            }

            // Highlight Boss Area
            if (level.specialElementConfig != null && level.specialElementConfig.bossAreaConfig != null)
            {
                var bossConfig = level.specialElementConfig.bossAreaConfig;
                if (bossConfig.startNodeId >= 0 && bossConfig.numCoverColumn > 0 && bossConfig.numCoverRow > 0)
                {
                    int startX = bossConfig.startNodeId % numColumn;
                    int startY = bossConfig.startNodeId / numColumn;

                    float left = (startX - 0.5f) * tileDistance;
                    float right = (startX + bossConfig.numCoverColumn - 0.5f) * tileDistance;
                    float bottom = (startY - 0.5f) * tileDistance;
                    float top = (startY + bossConfig.numCoverRow - 0.5f) * tileDistance;

                    Vector3[] corners =
                    {
                        new Vector3(left, bottom, 0.5f),
                        new Vector3(left, top, 0.5f),
                        new Vector3(right, top, 0.5f),
                        new Vector3(right, bottom, 0.5f)
                    };

                    Handles.DrawSolidRectangleWithOutline(
                        corners,
                        new Color(0f, 0.8f, 1f, 0.25f), // Cyan fill with transparency
                        new Color(0f, 0.8f, 1f, 1f)    // Solid cyan outline
                    );

                    Vector3 centerPos = new Vector3((left + right) * 0.5f, (bottom + top) * 0.5f, 0);
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteBoldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 11
                    };
                    labelStyle.normal.textColor = new Color(0f, 0.9f, 1f, 1f);
                    Handles.Label(centerPos + Vector3.up * 0.02f, $"Boss Area\n({bossConfig.numCoverColumn}x{bossConfig.numCoverRow})", labelStyle);
                }
            }

            for (int y = 0; y < numRow; y++)
            {
                for (int x = 0; x < numColumn; x++)
                {
                    Vector3 worldPos = tileDistance * new Vector2(x, y);

                    if (level.specialElementConfig.snakeConfig.snakeSegmentIndexes == null)
                    {
                        level.specialElementConfig.snakeConfig.snakeSegmentIndexes = new List<int>();
                    }

                    bool selected = level.specialElementConfig.snakeConfig.snakeSegmentIndexes.Contains(index);

                    float size = HandleUtility.GetHandleSize(worldPos) * 0.15f;

                    BaseTile tile = tileAtCoordinate[x, y];

                    if (tile != null)
                    {
                        Handles.color = tile.Faction;
                        Handles.DrawSolidDisc(
                            worldPos,
                            SceneView.currentDrawingSceneView.camera.transform.forward,
                            0.75f * size);
                    }

                    Handles.color = selected ? Color.green : Color.gray;
                    Handles.DrawWireDisc(
                        worldPos,
                        SceneView.currentDrawingSceneView.camera.transform.forward,
                        size);

                    // Node id
                    if (isShowingNodeId)
                    {
                        GUIStyle style = new GUIStyle(EditorStyles.whiteBoldLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 11
                        };
                        style.normal.textColor = Color.yellow;

                        Handles.Label(worldPos + Vector3.up * 0.05f, $"{x + y * numColumn}", style);
                    }

                    // Detect mouse over this circle
                    if (HandleUtility.DistanceToCircle(worldPos, size) == 0)
                    {
                        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                        if (e.type == EventType.MouseDown && e.button == 0)
                        {
                            Undo.RecordObject(level, "Snake Paint");

                            paintValue = !selected;
                            isPainting = true;
                            paintedThisDrag.Clear();

                            if (paintValue)
                                level.specialElementConfig.snakeConfig.snakeSegmentIndexes.Add(index);
                            else
                                level.specialElementConfig.snakeConfig.snakeSegmentIndexes.Remove(index);

                            paintedThisDrag.Add(index);

                            EditorUtility.SetDirty(level);
                            e.Use();
                        }
                        else if (isPainting &&
                                 e.type == EventType.MouseDrag &&
                                 !paintedThisDrag.Contains(index))
                        {
                            Undo.RecordObject(level, "Snake Paint");

                            if (paintValue)
                            {
                                if (!level.specialElementConfig.snakeConfig.snakeSegmentIndexes.Contains(index))
                                    level.specialElementConfig.snakeConfig.snakeSegmentIndexes.Add(index);
                            }
                            else
                            {
                                level.specialElementConfig.snakeConfig.snakeSegmentIndexes.Remove(index);
                            }

                            paintedThisDrag.Add(index);

                            EditorUtility.SetDirty(level);
                            e.Use();
                        }
                    }

                    index++;
                }
            }

            Vector3 previousPos = Vector3.zero;
            bool hasPrevious = false;

            for (int i = 0; i < level.specialElementConfig.snakeConfig.snakeSegmentIndexes.Count; i++)
            {
                index = level.specialElementConfig.snakeConfig.snakeSegmentIndexes[i];

                int x = index % numColumn;
                int y = index / numColumn;

                Vector3 worldPos = new Vector3(x * tileDistance, y * tileDistance, 0);

                if (hasPrevious)
                    DrawArrow(previousPos, worldPos, 0.05f, 20f);

                previousPos = worldPos;
                hasPrevious = true;
            }

            if (GUI.changed)
                SceneView.RepaintAll();
        }

        private void DrawArrow(Vector3 from, Vector3 to, float headLength, float headAngle)
        {
            float lineWidth = 5f; // thickness

            // Thick line
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(lineWidth, from, to);

            Vector3 direction = (to - from).normalized;

            Vector3 left = Quaternion.Euler(0, 0, headAngle) * -direction;
            Vector3 right = Quaternion.Euler(0, 0, -headAngle) * -direction;

            // Thicker arrow head
            Handles.DrawAAPolyLine(
                lineWidth,
                to,
                to + left * headLength
            );

            Handles.DrawAAPolyLine(
                lineWidth,
                to,
                to + right * headLength
            );
        }

        private void FillFactionPool()
        {
            var item = (LevelItem)target;

            Undo.RecordObject(item, "Import Color Palette");

            item.factions.Clear();

            foreach (var line in hexInput.Split('\n'))
            {
                string hex = line.Trim();

                if (string.IsNullOrEmpty(hex))
                    continue;

                if (!hex.StartsWith("#"))
                    hex = "#" + hex;

                if (ColorUtility.TryParseHtmlString(hex, out Color color))
                {
                    item.factions.Add(color);
                }
                else
                {
                    Debug.LogWarning($"Invalid color: {line}");
                }
            }

            EditorUtility.SetDirty(item);
        }

        private const string mapPreviewHolderName = "Map Preview (placeholder - no real 3D asset yet)";
        private const string garagePreviewHolderName = "Garage Preview (placeholder)";
        private const string antBoxPrefabPath = "Assets/Ants Flow/Prefabs/Ant Box.prefab";
        private const string factionMapperPrefabPath = "Assets/Ants Flow/Prefabs/Faction Mapper.prefab";
        private const string sampleBusPathName = "BusPath_Sample";
        private const string sampleExitPathName = "BusPath_Exit_Sample";

        // Tự dựng spline hình chữ U mẫu bằng SplineComputer.SetPoints() (theo đúng API Dreamteck),
        // không cần vẽ tay. Hình dựa theo bound map + Path Area hiện có của level, để bạn có điểm
        // xuất phát rồi tự kéo chỉnh lại cho khớp map thật (dùng icon Move trong Scene view).
        private void DrawSampleBusPath(LevelItem level, Vector2 tileSpacing)
        {
            if (level.tiles == null || level.tiles.Count == 0)
            {
                Debug.LogWarning("DrawSampleBusPath: chưa có tiles. Bấm GENERATE LEVEL trước.");
                return;
            }

            float minCol = level.tiles.Min(tile => tile.position.x);
            float maxCol = level.tiles.Max(tile => tile.position.x);
            float minRow = level.tiles.Min(tile => tile.position.y);
            float maxRow = level.tiles.Max(tile => tile.position.y);

            float mapWidth = (maxCol - minCol) * tileSpacing.x;

            float mapBottomY = LevelItem.BusBoardOriginY;
            float topY = mapBottomY + Mathf.Max(level.pathAreaOverlapIntoMap, (maxRow - minRow) * tileSpacing.y * 0.4f);
            float bottomY = -6f;

            float centerX = 0f;
            float legOffset = Mathf.Max(mapWidth * 0.2f, tileSpacing.x * 3.0f);

            List<Vector3> worldPoints = new List<Vector3>
            {
                new Vector3(centerX - legOffset, bottomY, 0),
                new Vector3(centerX - legOffset, topY - legOffset, 0),
                new Vector3(centerX - legOffset * 0.6f, topY, 0),
                new Vector3(centerX, topY + legOffset * 0.25f, 0),
                new Vector3(centerX + legOffset * 0.6f, topY, 0),
                new Vector3(centerX + legOffset, topY - legOffset, 0),
                new Vector3(centerX + legOffset, bottomY, 0),
            };

            CreateOrUpdateBusPathSpline(level, sampleBusPathName, role: BusPathRole.Pickup, worldPoints, "Draw Sample Bus Path");
        }

        // Đường thoát (role Exit): bắt đầu đúng điểm cuối của path đón khách (BusPath_Sample / role Pickup hiện tại,
        // kể cả sau khi đã được chỉnh tay trong Scene view) rồi dẫn tiếp ra ngoài, để xe hết khách có chỗ thoát khỏi map.
        private void DrawSampleExitPath(LevelItem level, Vector2 tileSpacing)
        {
            Vector3 startExitWorldPos;
            Vector3 exitDirection;

            // Tìm spline role Pickup (BusPath_Sample hoặc authoring role Pickup) hiện có trong level
            Transform pickupTransform = level.transform.Find(sampleBusPathName);
            SplineComputer pickupComputer = pickupTransform != null ? pickupTransform.GetComponent<SplineComputer>() : null;

            if (pickupComputer == null && level.busPaths != null)
            {
                BusPathAuthoring authoring = level.busPaths.FirstOrDefault(p => p != null && p.role == BusPathRole.Pickup && p.spline != null);
                if (authoring != null)
                {
                    pickupComputer = authoring.spline;
                }
            }

            if (pickupComputer != null && pickupComputer.pointCount > 0)
            {
                // Lấy toạ độ World trực tiếp từ Dreamteck Splines
                SplinePoint[] pts = pickupComputer.GetPoints(SplineComputer.Space.World);

                // Điểm bắt đầu thoát = điểm cuối cùng của đường loop đón khách
                startExitWorldPos = pts[pts.Length - 1].position;

                if (pts.Length >= 2)
                {
                    Vector3 prevWorldPos = pts[pts.Length - 2].position;
                    exitDirection = (startExitWorldPos - prevWorldPos).normalized;
                }
                else
                {
                    exitDirection = new Vector3(1f, -1f, 0f).normalized;
                }

                if (exitDirection.sqrMagnitude < 0.001f)
                {
                    exitDirection = new Vector3(1f, -1f, 0f).normalized;
                }
            }
            else
            {
                if (level.tiles == null || level.tiles.Count == 0)
                {
                    Debug.LogWarning("DrawSampleExitPath: chưa có BusPath_Sample và chưa có tiles. Bấm DRAW SAMPLE BUS PATH trước.");
                    return;
                }

                float minCol = level.tiles.Min(tile => tile.position.x);
                float maxCol = level.tiles.Max(tile => tile.position.x);
                float mapWidth = (maxCol - minCol) * tileSpacing.x;
                float bottomY = -6f;
                float centerX = 0f;
                float legOffset = Mathf.Max(mapWidth * 0.2f, tileSpacing.x * 3.0f);

                startExitWorldPos = level.transform.TransformPoint(new Vector3(centerX + legOffset, bottomY, 0f));
                exitDirection = new Vector3(1f, -1f, 0f).normalized;
            }

            startExitWorldPos.z = 0f;
            exitDirection.z = 0f;

            List<Vector3> worldPoints = new List<Vector3>
            {
                startExitWorldPos,
                startExitWorldPos + exitDirection * 3.5f,
                startExitWorldPos + exitDirection * 7.0f,
            };

            CreateOrUpdateBusPathSpline(level, sampleExitPathName, role: BusPathRole.Exit, worldPoints, "Draw Sample Exit Path");
        }

        private void CreateOrUpdateBusPathSpline(LevelItem level, string pathName, BusPathRole role, List<Vector3> worldPoints, string undoLabel)
        {
            Transform existing = level.transform.Find(pathName);
            GameObject pathGO;

            if (existing != null)
            {
                pathGO = existing.gameObject;
            }
            else
            {
                pathGO = new GameObject(pathName);
                Undo.RegisterCreatedObjectUndo(pathGO, undoLabel);
                pathGO.transform.SetParent(level.transform, false);
            }

            pathGO.transform.localPosition = Vector3.zero;
            pathGO.transform.localRotation = Quaternion.identity;
            pathGO.transform.localScale = Vector3.one;

            SplineComputer computer = pathGO.GetComponent<SplineComputer>();

            if (computer == null)
            {
                computer = pathGO.AddComponent<SplineComputer>();
            }

            computer.type = Spline.Type.CatmullRom;
            computer.is2D = true;

            SplinePoint[] splinePoints = new SplinePoint[worldPoints.Count];

            for (int i = 0; i < worldPoints.Count; i++)
            {
                Vector3 localPos = level.transform.InverseTransformPoint(worldPoints[i]);
                splinePoints[i] = new SplinePoint(localPos);
            }

            computer.SetPoints(splinePoints, SplineComputer.Space.Local);
            computer.RebuildImmediate();

            if (level.busPaths == null)
            {
                level.busPaths = new List<BusPathAuthoring>();
            }

            BusPathAuthoring authoring = level.busPaths.Find(item => item != null && item.spline == computer);

            if (authoring == null)
            {
                authoring = new BusPathAuthoring();
                authoring.id = level.busPaths.Count;
                authoring.role = role;
                authoring.spline = computer;
                level.busPaths.Add(authoring);
            }

            authoring.isLoop = false;

            EditorUtility.SetDirty(level);
            SceneView.RepaintAll();
        }

        // Gọi thẳng SplineComputer.Close() (API gốc Dreamteck) để nối node đầu - node cuối thành đường
        // kín, thay cho việc phải click chuột thật sát node đầu trong Scene view (dễ trật vị trí).
        // Cần spline có >=3 điểm, nếu không Close() sẽ tự log lỗi và không làm gì.
        // Chỉ áp dụng cho path đón khách (BusPath_Sample / role Pickup), bỏ qua đường thoát (BusPath_Exit_Sample / role Exit).
        private void CloseBusPaths(LevelItem level)
        {
            if (level.busPaths == null || level.busPaths.Count == 0)
            {
                Debug.LogWarning("CloseBusPaths: level chưa có bus path nào trong danh sách busPaths.");
                return;
            }

            for (int i = 0; i < level.busPaths.Count; i++)
            {
                BusPathAuthoring authoring = level.busPaths[i];

                if (authoring == null || authoring.spline == null)
                {
                    continue;
                }

                // Bỏ qua đường thoát (BusPath_Exit_Sample hoặc role == Exit)
                if (authoring.spline.gameObject.name == sampleExitPathName || authoring.role == BusPathRole.Exit)
                {
                    continue;
                }

                if (!authoring.spline.isClosed)
                {
                    authoring.spline.Close();
                }

                authoring.isLoop = true;
            }

            EditorUtility.SetDirty(level);
            SceneView.RepaintAll();
        }

        // Sinh busGarageQueue đủ để đón hết khách (tiles + backlog extraPassengerRounds), chia mỗi
        // faction thành nhiều xe sức chứa ngẫu nhiên 5-15, xáo trộn vào bãi đỗ. Đảm bảo level luôn
        // giải được (đủ tổng số khách mỗi màu) trong lúc chưa có người thiết kế bãi đỗ tay.
        // Chỉ tính TỔNG sức chứa cần có mỗi faction (đủ để đón hết khách kể cả backlog) - MỖI faction
        // đúng 1 entry. Việc chia nhỏ thành từng xe cụ thể + chọn màu theo độ khó được làm dần lúc
        // runtime (BusGarageManager), giống cách WashingMachineManager sinh ant box dần theo pick,
        // không sinh sẵn hết một lúc.
        private void AutoFillBusGarage(LevelItem level)
        {
            if (level.tiles == null || level.tiles.Count == 0)
            {
                Debug.LogWarning("AutoFillBusGarage: chưa có tiles. Bấm GENERATE LEVEL trước.");
                return;
            }

            Dictionary<Color, int> totalByFaction = new Dictionary<Color, int>();

            int totalRounds = 1 + Mathf.Max(0, level.extraPassengerRounds);

            for (int i = 0; i < level.tiles.Count; i++)
            {
                Color faction = level.tiles[i].Faction;

                if (!totalByFaction.ContainsKey(faction))
                {
                    totalByFaction[faction] = 0;
                }

                totalByFaction[faction] += totalRounds;
            }

            List<BusGarageItemConfig> generated = new List<BusGarageItemConfig>();

            foreach (var pair in totalByFaction)
            {
                generated.Add(new BusGarageItemConfig { faction = pair.Key, number = pair.Value });
            }

            level.busGarageQueue = generated;

            EditorUtility.SetDirty(level);
        }

        // Dựng map preview để xem layout khi CHƯA có asset 3D thật:
        // - Hành khách: dùng sprite tile của FactionMapper (đúng cái BaseTile/Tilemap đang hiển thị ở Ants Flow).
        // - Xe bus: dùng thẳng prefab "Ant Box" (WashingMachineItem) tô lại màu, KHÔNG gọi Init()/Setup() vì
        //   2 hàm đó phụ thuộc WashingMachineManager.Ins/GameController.Instance (chỉ có lúc Play mode).
        private void SimulateMap(LevelItem level, Vector2 tileSpacing)
        {
            if (level.tiles == null || level.tiles.Count == 0)
            {
                Debug.LogWarning("SimulateMap: chưa có tiles. Bấm GENERATE LEVEL trước.");
                return;
            }

            FactionMapper factionMapperAsset =
                AssetDatabase.LoadAssetAtPath<GameObject>(factionMapperPrefabPath)?.GetComponent<FactionMapper>();

            if (factionMapperAsset == null)
            {
                Debug.LogError($"SimulateMap: không tìm thấy FactionMapper tại {factionMapperPrefabPath}");
                return;
            }

            GameObject antBoxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(antBoxPrefabPath);

            if (antBoxPrefab == null)
            {
                Debug.LogError($"SimulateMap: không tìm thấy prefab Ant Box tại {antBoxPrefabPath}");
                return;
            }

            ClearMapPreview(level);

            // Đặt level.transform = phép fit (giống runtime) để preview khớp game: khách + spline (đều
            // là con của level) hiện đúng toạ độ đã co, và bake (InverseTransformPoint(level)) tự ra
            // đúng hệ toạ độ runtime dùng.
            ApplyBoardFitToLevelTransform(level, tileSpacing);

            GameObject previewHolder = new GameObject(mapPreviewHolderName);
            Undo.RegisterCreatedObjectUndo(previewHolder, "Simulate Map");
            previewHolder.transform.SetParent(level.transform, false);

            GameObject passengerHolder = new GameObject("Passengers");
            passengerHolder.transform.SetParent(previewHolder.transform, false);

            float minCol = level.tiles.Min(tile => tile.position.x);
            float maxCol = level.tiles.Max(tile => tile.position.x);
            float minRow = level.tiles.Min(tile => tile.position.y);
            float maxRow = level.tiles.Max(tile => tile.position.y);

            float centerCol = (minCol + maxCol) * 0.5f;

            // Hành khách: 1 sprite tile theo faction cho mỗi tile đã generate từ picture
            for (int i = 0; i < level.tiles.Count; i++)
            {
                AntsFlow.BaseTile tile = level.tiles[i];

                Sprite tileSprite = FindTileSprite(factionMapperAsset, tile.Faction);

                GameObject passengerGO = new GameObject($"Passenger_{i}");
                passengerGO.transform.SetParent(passengerHolder.transform, false);
                passengerGO.transform.localPosition = LevelItem.BusTileToLocal(
                    tile.position.x, tile.position.y, centerCol, minRow, tileSpacing);

                SpriteRenderer spriteRenderer = passengerGO.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = tileSprite;

                if (tileSprite == null)
                {
                    spriteRenderer.color = tile.Faction;
                }
                else
                {
                    // Sprite tile vốn để render qua Tilemap (kích thước theo PPU riêng), không phải
                    // qua SpriteRenderer trần nên bị to quá cỡ. Scale lại cho khớp 1 ô tileSpacing,
                    // chừa margin để không đè sát nhau như Tilemap.
                    Vector2 spriteSize = tileSprite.bounds.size;
                    float margin = 0.95f;

                    float scaleX = spriteSize.x > 0.0001f ? (tileSpacing.x * margin) / spriteSize.x : 1f;
                    float scaleY = spriteSize.y > 0.0001f ? (tileSpacing.y * margin) / spriteSize.y : 1f;

                    passengerGO.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                }
            }

            // Vùng path: chỗ trống để bạn vẽ SplineComputer (busPaths) lên đó. Cho phép đè lên map
            // (pathAreaOverlapIntoMap) và kích thước tự chỉnh trên Inspector (pathAreaWidth/Height) vì
            // hình dạng đường mỗi level một khác, không auto-tính đúng cho mọi map được.
            float mapWidth = (maxCol - minCol) * tileSpacing.x;
            float mapBottomY = LevelItem.BusBoardOriginY;

            float pathAreaTopY = mapBottomY + level.pathAreaOverlapIntoMap;
            float pathAreaBottomY = pathAreaTopY - level.pathAreaHeight;

            if (TryGetBusPathBounds(level, out float pathMinY, out float pathMaxY))
            {
                pathAreaTopY = Mathf.Max(pathAreaTopY, pathMaxY + tileSpacing.y * 0.5f);
                pathAreaBottomY = Mathf.Min(pathAreaBottomY, pathMinY - tileSpacing.y * 0.5f);
            }

            float pathAreaHalfWidth = Mathf.Max(level.pathAreaWidth, mapWidth + 1.5f) * 0.5f;
            float pathAreaCenterX = 0f;

            CreatePathAreaPlaceholder(
                previewHolder,
                pathAreaCenterX - pathAreaHalfWidth,
                pathAreaCenterX + pathAreaHalfWidth,
                pathAreaTopY,
                pathAreaBottomY);

            // Xe bus (garage): đặt ở scene-root (KHÔNG dưới level đã fit) tại đúng garage-fit như runtime,
            // để preview garage khớp game (dưới đáy khung, co vừa bề rộng).
            Vector2 slotSpacing = new Vector2(4.6f, 4.2f);
            var garageManager = UnityEngine.Object.FindObjectOfType<BusFlow.BusGarageManager>();
            if (garageManager != null)
            {
                slotSpacing = garageManager.slotSpacing;
            }

            int garageNumColumn = Mathf.Max(1, level.busGarageNumColumns);
            BusFlow.BusBoardFit.Fit garageFit = BusFlow.BusBoardFit.ComputeGarage(
                Camera.main, garageNumColumn, slotSpacing, Vector3.zero,
                level.garageWidthRatio, level.garageViewportY);

            GameObject busQueueHolder = new GameObject(garagePreviewHolderName);
            Undo.RegisterCreatedObjectUndo(busQueueHolder, "Simulate Map Garage");
            busQueueHolder.transform.position = garageFit.origin;
            busQueueHolder.transform.localScale = garageFit.scale * Vector3.one;

            float centerBusCol = (garageNumColumn - 1) * 0.5f;

            if (level.busGarageQueue != null && level.busGarageQueue.Count > 0)
            {
                int numColumn = garageNumColumn;

                for (int i = 0; i < level.busGarageQueue.Count; i++)
                {
                    BusGarageItemConfig item = level.busGarageQueue[i];

                    int column = i % numColumn;
                    int row = i / numColumn;

                    SpawnBusPreview(busQueueHolder, antBoxPrefab, i, item.faction, item.number,
                        new Vector3((column - centerBusCol) * slotSpacing.x, -row * slotSpacing.y, 0));
                }
            }
            else
            {
                List<Color> distinctFactions = level.tiles
                    .Select(tile => tile.Faction)
                    .Distinct()
                    .ToList();

                int numColumn = garageNumColumn;

                for (int i = 0; i < distinctFactions.Count; i++)
                {
                    Color faction = distinctFactions[i];
                    int passengerCount = level.tiles.Count(tile => tile.Faction == faction);

                    int column = i % numColumn;
                    int row = i / numColumn;

                    SpawnBusPreview(busQueueHolder, antBoxPrefab, i, faction, passengerCount,
                        new Vector3((column - centerBusCol) * slotSpacing.x, -row * slotSpacing.y, 0));
                }
            }

            EditorUtility.SetDirty(level);
        }

        // Set level.transform = phép fit (giống runtime) để preview khớp game.
        private void ApplyBoardFitToLevelTransform(LevelItem level, Vector2 tileSpacing)
        {
            BusFlow.BusBoardFit.Fit fit = BusFlow.BusBoardFit.ComputeBoard(
                Camera.main, level.tiles, tileSpacing, Vector3.zero,
                level.fitWidthRatio, level.fitHeightRatio, level.fitCenterViewportY);

            Undo.RecordObject(level.transform, "Apply Board Fit");
            level.transform.position = fit.origin;
            level.transform.localScale = fit.scale * Vector3.one;
        }

        private void SpawnBusPreview(GameObject busQueueHolder, GameObject antBoxPrefab, int index, Color faction, int number, Vector3 localPosition)
        {
            GameObject busGO = (GameObject)PrefabUtility.InstantiatePrefab(antBoxPrefab, busQueueHolder.transform);
            busGO.name = $"Bus_{index}";
            busGO.transform.localPosition = localPosition;

            WashingMachineItem busItem = busGO.GetComponent<WashingMachineItem>();

            if (busItem != null)
            {
                busItem.faction = faction;
                busItem.number = number;
                busItem.remainingNumber = number;

                if (busItem.lid != null)
                {
                    busItem.lid.color = faction;
                }

                if (busItem.numberText != null)
                {
                    busItem.numberText.text = number.ToString();
                }
            }
        }

        private void ClearMapPreview(LevelItem level)
        {
            Transform existing = level.transform.Find(mapPreviewHolderName);

            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            // Garage preview nằm ở scene-root (không dưới level) nên tìm theo tên trong scene.
            GameObject garagePreview = GameObject.Find(garagePreviewHolderName);

            if (garagePreview != null)
            {
                Undo.DestroyObjectImmediate(garagePreview);
            }
        }

        // Đọc bound Y (local space của LevelItem) của các spline đã gán trong busPaths, nếu có.
        // Bake điểm y hệt logic LevelItem.BuildBusPathConfig() để vùng preview khớp path thật.
        private bool TryGetBusPathBounds(LevelItem level, out float minPathY, out float maxPathY)
        {
            minPathY = float.MaxValue;
            maxPathY = float.MinValue;
            bool found = false;

            if (level.busPaths == null)
            {
                return false;
            }

            for (int i = 0; i < level.busPaths.Count; i++)
            {
                BusPathAuthoring authoring = level.busPaths[i];

                if (authoring == null || authoring.spline == null)
                {
                    continue;
                }

                SplinePoint[] points = authoring.spline.GetPoints();

                for (int p = 0; p < points.Length; p++)
                {
                    float localY = level.transform.InverseTransformPoint(points[p].position).y;

                    if (localY < minPathY) minPathY = localY;
                    if (localY > maxPathY) maxPathY = localY;

                    found = true;
                }
            }

            return found;
        }

        // Quad phẳng làm chỗ chừa sẵn để vẽ spline (GameObject > 3D Object > Spline Computer) lên trên.
        // Chỉ là marker preview, không có gameplay, không collider.
        private void CreatePathAreaPlaceholder(GameObject previewHolder, float minX, float maxX, float topY, float bottomY)
        {
            GameObject pathAreaGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            pathAreaGO.name = "Path Area (vẽ spline vào đây)";
            pathAreaGO.transform.SetParent(previewHolder.transform, false);

            Collider pathAreaCollider = pathAreaGO.GetComponent<Collider>();

            if (pathAreaCollider != null)
            {
                DestroyImmediate(pathAreaCollider);
            }

            float width = Mathf.Max(0.1f, maxX - minX) + 1.5f;
            float height = Mathf.Max(0.1f, topY - bottomY);

            pathAreaGO.transform.localPosition = new Vector3((minX + maxX) * 0.5f, (topY + bottomY) * 0.5f, 0.05f);
            pathAreaGO.transform.localScale = new Vector3(width, height, 1f);

            Renderer pathAreaRenderer = pathAreaGO.GetComponent<Renderer>();
            pathAreaRenderer.sharedMaterial = new Material(pathAreaRenderer.sharedMaterial);
            pathAreaRenderer.sharedMaterial.color = new Color(0.55f, 0.55f, 0.6f, 1f);
        }

        // Không dùng FactionMapper.GetTileSprite() vì hàm đó đọc GameController.Instance.useProfile
        // (chỉ có lúc Play mode) -> tự tra thẳng list mapper (an toàn ở Editor/Edit mode).
        private Sprite FindTileSprite(FactionMapper factionMapperAsset, Color faction)
        {
            if (factionMapperAsset.mapper == null)
            {
                return null;
            }

            for (int i = 0; i < factionMapperAsset.mapper.Count; i++)
            {
                if (factionMapperAsset.mapper[i].color == faction)
                {
                    return factionMapperAsset.mapper[i].spriteContainer?.tileSprite;
                }
            }

            int nearestIndex = -1;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < factionMapperAsset.mapper.Count; i++)
            {
                Color color = factionMapperAsset.mapper[i].color;

                float distance =
                    (color.r - faction.r) * (color.r - faction.r) +
                    (color.g - faction.g) * (color.g - faction.g) +
                    (color.b - faction.b) * (color.b - faction.b);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex >= 0 ? factionMapperAsset.mapper[nearestIndex].spriteContainer?.tileSprite : null;
        }

        private void GetPoolFaction(LevelItem levelItem)
        {
            // BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(levelItem.transform);

            levelItem.poolFaction = levelItem.tiles.Select(item => item.Faction).ToList();

            EditorUtility.SetDirty(levelItem);
        }

        void SpawnPixelArt(LevelItem level)
        {
            List<GameObject> toRemoveObjects = new List<GameObject>();

            for (int i = 0; i < level.transform.childCount; i++)
            {
                toRemoveObjects.Add(level.transform.GetChild(i).gameObject);
            }

            foreach (var item in toRemoveObjects)
            {
                DestroyImmediate(item, true);
            }

            level.transform.position = Vector3.zero;
            level.transform.localScale = Vector3.one;

            Color[] pixels = level.picture.texture.GetPixels();

            int width = level.picture.texture.width;
            int height = level.picture.texture.height;

            int index = 0;

            // for (int y = startY; y < startY + size; y++)
            // {
            //     for (int x = startX; x < startX + size; x++)
            //     {
            //         Color pixelColor = pixels[y * width + x];

            //         // Skip transparent pixels if enabled
            //         if (pixelColor.a < 1)
            //             continue;

            //         // Create tile
            //         Vector3 spawnPos = new Vector3(-8 + (x - startX) * tileSpacing.x, (y - startY) * tileSpacing.y, 0.01f * y);

            //         BaseTile tile = (BaseTile)PrefabUtility.InstantiatePrefab(
            //             level.tilePrefab,
            //             level.transform
            //         );
            //         tile.name = $"Tile {index}";
            //         tile.transform.position = spawnPos;

            //         tile.spriteRenderer.color = pixelColor;

            //         index++;
            //     }
            // }

            level.transform.position = level.spawnPosition;
            level.transform.localScale = level.spawnScale;

            EditorUtility.SetDirty(level);
        }

        private void GenerateLevel(LevelItem levelItem)
        {
            ReadLevel(levelItem);
            // GroupColors(levelItem);
            ReassignFactionForTiles(levelItem);
            AssignLevelPriority(levelItem);
            // FindTileBlockages(levelItem);
            GetPoolFaction(levelItem);
            // CenterOnX(levelItem);
            // MoveTilesCloserTogether(levelItem, moveTilesCloserFactor);
        }

        void ReadLevel(LevelItem level)
        {
            List<GameObject> toRemoveObjects = new List<GameObject>();

            for (int i = 0; i < level.transform.childCount; i++)
            {
                toRemoveObjects.Add(level.transform.GetChild(i).gameObject);
            }

            foreach (var item in toRemoveObjects)
            {
                DestroyImmediate(item, true);
            }

            level.transform.position = Vector3.zero;
            level.transform.localScale = Vector3.one;

            Color[] pixels = level.picture.texture.GetPixels();

            int width = level.picture.texture.width;
            int height = level.picture.texture.height;

            int index = 0;

            float pixelStepX = (float)width / level.numColumn;
            float pixelStepY = (float)height / level.numRow;

            tileSpacing.y = tileSpacing.x * (pixelStepY / pixelStepX);

            level.tiles = new List<BaseTile>();

            for (float y = 0.25f * pixelStepY; y < height; y += pixelStepY)
            {
                for (float x = 0.25f * pixelStepX; x < width; x += pixelStepX)
                {
                    // if (x + 0.5f * pixelStepX > width)
                    // {
                    //     continue;
                    // }

                    // if (y + 0.5f * pixelStepY > height)
                    // {
                    //     continue;
                    // }

                    int pixelX = Mathf.FloorToInt(x);
                    int pixelY = Mathf.FloorToInt(y);

                    Color pixelColor = pixels[pixelY * width + pixelX];

                    // Skip transparent pixels if enabled
                    if (pixelColor.a < 1)
                        continue;

                    // Create tile
                    Vector3 spawnPos = new Vector3(pixelX / pixelStepX * tileSpacing.x, pixelY / pixelStepY * tileSpacing.y, 0.01f * y);

                    // BaseTile tile = (BaseTile)PrefabUtility.InstantiatePrefab(
                    //     level.tilePrefab,
                    //     level.transform
                    // );
                    // tile.name = $"Tile {index}";
                    // tile.transform.position = spawnPos;

                    BaseTile tile = new BaseTile();

                    tile.tileName = index;
                    tile.position = new Vector3(Mathf.FloorToInt(x / pixelStepX), Mathf.FloorToInt(y / pixelStepY));

                    // Debug.Log($"Gen Level {tile.position} {x / pixelStepX}");

                    tile.Faction = pixelColor;

                    level.tiles.Add(tile);

                    index++;
                }
            }

            level.transform.position = level.spawnPosition;
            level.transform.localScale = level.spawnScale;

            EditorUtility.SetDirty(level);
        }

        private void FindTileBlockages(LevelItem level)
        {
            // BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(level.transform);

            // for (int i = 0; i < tiles.Length; i++)
            // {
            //     BaseTile tile = tiles[i];

            //     tile.blockages = new List<BaseTile>();

            //     BoxCollider2D boxCollider = tile.GetComponent<BoxCollider2D>();

            //     FindBlockage(tile, boxCollider, tile.transform.up);
            //     FindBlockage(tile, boxCollider, tile.transform.right);
            //     FindBlockage(tile, boxCollider, -tile.transform.up);
            //     FindBlockage(tile, boxCollider, -tile.transform.right);
            // }

            // EditorUtility.SetDirty(level);

            void FindBlockage(BaseTile tile, BoxCollider2D collider, Vector2 direction)
            {
                RaycastHit2D[] hits = new RaycastHit2D[1];

                collider.Raycast(direction, hits);

                if (hits.Length > 0 && hits[0].collider != null)
                {
                    BaseTile blockage = hits[0].collider.GetComponent<BaseTile>();

                    if (blockage != null)
                    {
                        tile.blockages.Add(blockage);
                    }
                }
            }
        }

        private void GenerateLevelFromJson(LevelItem level, string json)
        {
            List<GameObject> toRemoveObjects = new List<GameObject>();

            for (int i = 0; i < level.transform.childCount; i++)
            {
                toRemoveObjects.Add(level.transform.GetChild(i).gameObject);
            }

            foreach (var item in toRemoveObjects)
            {
                DestroyImmediate(item, true);
            }

            level.transform.position = Vector3.zero;
            level.transform.localScale = Vector3.one;

            // Parse JSON
            LevelData data = JsonUtility.FromJson<LevelData>(json);

            Dictionary<string, Color> paletteMap = new Dictionary<string, Color>();

            // Create palette dictionary
            paletteMap.Clear();

            foreach (var p in data.palette)
            {
                paletteMap[p.key] = HexToColor(p.fill);
            }

            // Generate grid
            for (int row = 0; row < data.gridRows; row++)
            {
                for (int col = 0; col < data.gridCols; col++)
                {
                    int index = row * data.gridCols + col;

                    // Safety check
                    if (index >= data.grid.Length)
                        continue;

                    string tileKey = data.grid[index];

                    // "" means empty
                    if (string.IsNullOrEmpty(tileKey))
                        continue;

                    // Create tile
                    Vector3 pos = new Vector3(
                        col * tileSpacing.x,
                        -row * tileSpacing.y,
                        0
                    );

                    // BaseTile tile = (BaseTile)PrefabUtility.InstantiatePrefab(
                    //     level.tilePrefab,
                    //     level.transform
                    // );

                    // // Set tile data
                    // tile.name = $"Tile_{row}_{col}_{tileKey}";
                    // tile.transform.position = pos;

                    // // Apply color from palette
                    // if (paletteMap.TryGetValue(tileKey, out Color color))
                    // {
                    //     tile.spriteRenderer.color = color;
                    // }
                }
            }

            EditorUtility.SetDirty(level);
        }

        private void GroupColors(LevelItem level)
        {
            // BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(level.transform);

            // List<Color> colors = tiles.Select(item => item.spriteRenderer.color).ToList();

            // List<Color> groups = new();

            // float threshold = colorGroupThreshold;

            // foreach (var color in colors)
            // {
            //     bool found = false;

            //     for (int i = 0; i < groups.Count; i++)
            //     {
            //         float distance = Vector3.Distance(
            //             new Vector3(color.r, color.g, color.b),
            //             new Vector3(groups[i].r, groups[i].g, groups[i].b));

            //         if (distance < threshold)
            //         {
            //             found = true;
            //             break;
            //         }
            //     }

            //     if (!found)
            //         groups.Add(color);
            // }


            // level.factions = groups;
        }

        private void ReassignFactionForTiles(LevelItem level)
        {
            BaseTile[] tiles = level.tiles.ToArray();

            foreach (var tile in tiles)
            {
                Color color = FindNearestFaction(tile.Faction, level.factions);
                tile.Faction = color;
                tile.Faction = color;
            }

            Color FindNearestFaction(Color color, List<Color> centers)
            {
                int bestIndex = 0;
                float bestDistance = float.MaxValue;

                Color outputFaction = color;

                for (int i = 0; i < centers.Count; i++)
                {
                    float distance =
                        Mathf.Pow(color.r - centers[i].r, 2) +
                        Mathf.Pow(color.g - centers[i].g, 2) +
                        Mathf.Pow(color.b - centers[i].b, 2);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestIndex = i;

                        outputFaction = centers[i];
                    }
                }

                return outputFaction;
            }

            EditorUtility.SetDirty(level);
        }

        private void AssignLevelPriority(LevelItem level)
        {
            BaseTile[] tiles = level.tiles.ToArray();

            // Gom theo hàng
            Dictionary<float, List<BaseTile>> rows = new();

            foreach (var tile in tiles)
            {
                float y = tile.position.y;

                // Làm tròn để tránh lỗi float
                y = Mathf.Round(y * 1000f) / 1000f;

                if (!rows.ContainsKey(y))
                    rows[y] = new List<BaseTile>();

                rows[y].Add(tile);
            }

            // Sắp xếp từ dưới lên trên
            int numberPriovity = 0;
            var sortedRows = rows.Keys.OrderBy(y => y).ToList();

            for (int rowIndex = 0; rowIndex < sortedRows.Count; rowIndex++)
            {
                float y = sortedRows[rowIndex];

                foreach (var tile in rows[y])
                {
                    tile.ring = rowIndex;
                    tile.priority = numberPriovity;
                    numberPriovity++;
                }
            }

            Dictionary<int, List<BaseTile>> groups = new Dictionary<int, List<BaseTile>>();

            for (int i = 0; i < tiles.Length; i++)
            {
                BaseTile tile = tiles[i];

                if (!groups.ContainsKey(tile.ring))
                {
                    groups.Add(tile.ring, new List<BaseTile>() { tile });
                }
                else
                {
                    groups[tile.ring].Add(tile);
                }
            }

            // foreach (var group in groups)
            // {
            //    int ring = group.Key;
            //    List<BaseTile> tilesInSameRing = group.Value;

            //    int minX = tilesInSameRing.Min(tile => (int)tile.transform.position.x);
            //    int maxX = tilesInSameRing.Max(tile => (int)tile.transform.position.x);
            //    int minY = tilesInSameRing.Min(tile => (int)tile.transform.position.y);
            //    int maxY = tilesInSameRing.Max(tile => (int)tile.transform.position.y);

            //    // int numCol = maxX - minX;
            //    // int numRow = maxY - minY;

            //    // Debug.Log($"AssignLevelPriority {}")

            //    int priority = 0;

            //    // bottom left --> bottom right
            //    List<BaseTile> validTiles = tilesInSameRing.Where(tile => (int)tile.transform.position.y == minY)
            //            .OrderBy(tile => tile.transform.position.x)
            //            .ToList();

            //    for (int j = 0; j < validTiles.Count; j++)
            //    {
            //        BaseTile tile = validTiles[j];

            //        if (tile.priority == 0)
            //        {
            //            tile.priority = priority;
            //            priority++;
            //        }
            //    }

            //    // bottom right --> top right
            //    validTiles = tilesInSameRing.Where(tile => (int)tile.transform.position.x == maxX)
            //            .OrderBy(tile => tile.transform.position.y)
            //            .ToList();

            //    for (int j = 0; j < validTiles.Count; j++)
            //    {
            //        BaseTile tile = validTiles[j];

            //        if (tile.priority == 0)
            //        {
            //            tile.priority = priority;
            //            priority++;
            //        }
            //    }

            //    // top right --> top left
            //    validTiles = tilesInSameRing.Where(tile => (int)tile.transform.position.y == maxY)
            //            .OrderByDescending(tile => tile.transform.position.x)
            //            .ToList();

            //    for (int j = 0; j < validTiles.Count; j++)
            //    {
            //        BaseTile tile = validTiles[j];

            //        if (tile.priority == 0)
            //        {
            //            tile.priority = priority;
            //            priority++;
            //        }
            //    }

            //    // top left --> bottom left
            //    validTiles = tilesInSameRing.Where(tile => (int)tile.transform.position.x == minX)
            //            .OrderByDescending(tile => tile.transform.position.y)
            //            .ToList();

            //    for (int j = 0; j < validTiles.Count; j++)
            //    {
            //        BaseTile tile = validTiles[j];

            //        if (tile.priority == 0)
            //        {
            //            tile.priority = priority;
            //            priority++;
            //        }
            //    }
            // }

            EditorUtility.SetDirty(level);
        }

        public void CenterOnX(LevelItem level)
        {
            float sumX = 0f;

            foreach (BaseTile child in level.tiles)
            {
                sumX += child.position.x;
            }

            float averageX = sumX / level.transform.childCount;

            level.transform.position += Vector3.left * averageX;

            EditorUtility.SetDirty(level);
        }

        public void MoveTilesCloserTogether(LevelItem level, float factor)
        {
            // BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(level.transform);

            // Vector3 center = Vector3.zero;

            // foreach (BaseTile tile in tiles)
            //     center += tile.transform.position;

            // center /= tiles.Length;

            // // find num column
            // float firstRowY = tiles[0].transform.position.y;
            // float tolerance = 0.01f;

            // int numCols = 0;

            // foreach (var tile in tiles)
            // {
            //     if (Mathf.Abs(tile.transform.position.y - firstRowY) < tolerance)
            //         numCols++;
            // }

            // float baseSpacingX = 0.7605271f;
            // float baseSpacingY = 0.760527f;
            // float currentSpacingX = tiles[1].transform.position.x - tiles[0].transform.position.x;
            // float currentSpacingY = tiles[numCols].transform.position.y - tiles[0].transform.position.y;

            // float ratioX = currentSpacingX / baseSpacingX;
            // float ratioY = currentSpacingY / baseSpacingY;

            // foreach (BaseTile tile in tiles)
            // {
            //     Vector3 offset = tile.transform.position - center;

            //     float gridX = offset.x / currentSpacingX * ratioX;
            //     float gridY = offset.y / currentSpacingY * ratioY;

            //     tile.transform.position = center + new Vector3(
            //         gridX * tileSpacing.x,
            //         gridY * tileSpacing.y,
            //         offset.z
            //     );
            // }


            // foreach (BaseTile tile in tiles)
            // {
            //     tile.transform.position = center + (tile.transform.position - center) * factor;
            // }

            EditorUtility.SetDirty(level);
        }

        public void PreviewLevel(LevelItem level)
        {
            // BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(level.transform);

            // foreach (BaseTile tile in tiles)
            // {
            //     tile.spriteRenderer.sprite = factionMapper.GetAntBoxSprite(tile.Faction);
            // }
        }

        #region JSON
        private void ConvertToJson(LevelItem level)
        {
            // factionMapper =
            //     AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ants Flow/Prefabs/Faction Mapper.prefab")?.GetComponent<FactionMapper>();

            // List<BaseTile> tiles = TransformUtil.GetComponentsFromAllChildren<BaseTile>(level.transform);

            // AntLevelData antLevelData = new AntLevelData();

            // antLevelData.antLevelDataItems = new List<AntLevelDataItem>();

            // for (int i = 0; i < tiles.Count; i++)
            // {
            //     BaseTile tile = tiles[i];

            //     AntLevelDataItem dataItem = new AntLevelDataItem();
            //     dataItem.position = tile.transform.position;
            //     dataItem.factionIndex = factionMapper.mapper.FindIndex(item => item.color == tile.Faction);

            //     dataItem.blockageIndexes = new List<int>();

            //     for (int j = 0; j < tile.blockages.Count; j++)
            //     {
            //         dataItem.blockageIndexes.Add(tiles.IndexOf(tile.blockages[j]));
            //     }

            //     dataItem.ring = tile.ring;
            //     dataItem.priority = tile.priority;

            //     antLevelData.antLevelDataItems.Add(dataItem);
            // }

            // List<int> poolFactionIndexes = factionMapper.GetPoolFactionIndexFromPoolFaction(level.poolFaction);

            // antLevelData.levelItemData = new LevelItemData()
            // {
            //     levelScale = level.transform.localScale,
            //     poolFactionIndexes = poolFactionIndexes,
            //     isTutorialLevel = level.isTutorialLevel,
            //     isUseManualFactionSetupForFirstAntBoxes = level.isUseManualFactionSetupForFirstAntBoxes,
            //     levelFactionConfig = level.levelFactionConfig,
            //     difficulty = level.difficulty,
            //     initialDifficulty = level.initialDifficulty,
            //     overrideInitialDifficulty = level.overrideInitialDifficulty,
            //     numStartAntBoxes = level.numStartAntBoxes,
            //     specialElementConfig = level.specialElementConfig
            // };

            // // Use level name as file name
            // string filePath = Path.Combine("Assets/Ants Flow/JSON/Levels", $"{level.name}.json");

            // // Serialize and save
            // string json = JsonUtility.ToJson(antLevelData, false);
            // File.WriteAllText(filePath, json);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        #endregion

        #region Manual Level Setup
        private void FillTheRestOfLevel(LevelItem levelItem)
        {
            List<Color> poolFaction = levelItem.poolFaction;

            Dictionary<Color, int> numAntPerFaction = new Dictionary<Color, int>();

            for (int i = 0; i < poolFaction.Count; i++)
            {
                CommonUtil.SumValueOfDictionary(numAntPerFaction, poolFaction[i], 1);
            }

            for (int i = 0; i < levelItem.levelFactionConfig.items.Count; i++)
            {
                LevelFactionItemConfig item = levelItem.levelFactionConfig.items[i];

                CommonUtil.SumValueOfDictionary(numAntPerFaction, item.faction, item.numAnt);
            }

            foreach (var item in numAntPerFaction)
            {
                levelItem.levelFactionConfig.items.Add(new LevelFactionItemConfig(item.Key, item.Value));
            }

            EditorUtility.SetDirty(levelItem);
        }
        #endregion

        private Color HexToColor(string hex)
        {
            Color color;
            ColorUtility.TryParseHtmlString(hex, out color);
            return color;
        }

        [Serializable]
        public class LevelData
        {
            public int gridRows;
            public int gridCols;

            public PaletteData[] palette;

            public string[] grid;
        }

        [Serializable]
        public class PaletteData
        {
            public string key;

            public string fill;
            public string dark;
            public string light;
        }
    }
#endif
}

[Serializable]
public class LevelFactionConfig
{
    public List<LevelFactionItemConfig> items;
}

[Serializable]
public class LevelFactionItemConfig
{
    public Color faction;
    public int numAnt;

    public LevelFactionItemConfig(Color faction, int numAnt)
    {
        this.faction = faction;
        this.numAnt = numAnt;
    }
}

[Serializable]
public class AntLevelData
{
    public LevelItemData levelItemData;
    public List<AntLevelDataItem> antLevelDataItems;
}

[Serializable]
public class AntLevelDataItem
{
    public Vector3 position;
    public int factionIndex;
    public List<int> blockageIndexes;
    public int ring;
    public int priority;
}

[Serializable]
public class LevelItemData
{
    public Vector3 levelScale;
    public int numColumn;
    public int numRow;
    public bool isLevelHavingTransparentPixels;
    public bool isOverrideLevelScale;
    public float levelScaleXRatio = 18f;
    public float levelScaleYRatio = 15f;
    public float multiplierScaleX;
    public float multiplierScaleY;
    public List<int> poolFactionIndexes;
    public bool isTutorialLevel;
    public bool isUseManualFactionSetupForFirstAntBoxes;
    public LevelFactionConfig levelFactionConfig;
    public int difficulty;
    public int initialDifficulty;
    public bool overrideInitialDifficulty;
    public int numStartAntBoxes = 6;
    public List<int> easySpawnOrder;
    public SpecialElementConfig specialElementConfig;
    public BusPathConfig busPathConfig;
    public int busGarageNumColumns = 4;
    public int busGarageNumRows = 6;
    public int numStartBuses = 8;
    public List<BusGarageItemConfig> busGarageQueue;
    public int extraPassengerRounds = 1;
    public Vector2 busTileSpacing = new Vector2(1.6f, 1.6f);
}

public enum BusPathRole
{
    Pickup = 0, // Đường đón khách (vòng loop quanh bảng khách)
    Exit = 1    // Đường thoát (dẫn xe ra khỏi map khi xong)
}

[Serializable]
public class BusPathAuthoring
{
    public int id;
    public BusPathRole role;
    public SplineComputer spline;
    public bool isLoop;
}

[Serializable]
public class BusPathConfig
{
    public List<BusPathItemConfig> paths;
}

[Serializable]
public class BusPathItemConfig
{
    public int id;
    public BusPathRole role;
    public bool isLoop;
    public List<Vector3> points;
}

[Serializable]
public class BusGarageItemConfig
{
    public Color faction;
    public int number;
}


[Serializable]
public class HardPixelConfig : ISerializationCallbackReceiver
{
    public int id;
    public int startNodeId;
    public int numRows;
    public int numCols;
    public int requiredEatCount;
    public GameObject prefab;
    public int prefabIndex;
    public bool isShowText;
    public float rotationAngle;
    public float scaleWidth = 1.01f;
    public float scaleHeight = 1.055f;
    public float offsetX;
    public float offsetY;

    public HardPixelConfig()
    {
        scaleWidth = 1.01f;
        scaleHeight = 1.055f;
    }

    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
        if (scaleWidth <= 0)
        {
            scaleWidth = 1.01f;
        }
        if (scaleHeight <= 0)
        {
            scaleHeight = 1.055f;
        }
    }
}

[Serializable]
public class SpecialElementConfig
{
    public SpecialElementItemConfig hiddenAntBoxConfig;
    public FrozenAntBoxConfig frozenAntBoxConfig;
    public SpecialElementItemConfig doubleAntBoxConfig;
    public SpecialElementItemConfig magnetAntBoxConfig;
    public SpawningTileBoxConfig spawningTileBoxConfig;
    public BossAreaConfig bossAreaConfig;
    public OpenCloseAntBoxConfig openCloseAntBoxConfig;
    public List<HardPixelConfig> hardPixelConfigs;
    public LockedSlotConfig lockedSlotConfig;
    public SnakeConfig snakeConfig;
    public bool isContainsHiddenTiles;
    public bool isContainsWallTiles;


}


[Serializable]
public class SpecialElementItemConfig
{
    public List<int> spawnOrder;

    public int GetNumLeft()
    {
        return spawnOrder.Count;
    }

    public void RemoveOneSpecialElementInPool()
    {
        spawnOrder.Remove(WashingMachineManager.Ins.numAntBoxSpawned);
    }

    public bool IsCorrectOrder(int numAntBoxesSpawned)
    {
        return spawnOrder.Contains(numAntBoxesSpawned);
    }

    public virtual bool IsValidToSpawn(int numAntBoxesSpawned, bool isOpenClose = false)
    {
        if (spawnOrder == null)
        {
            return false;
        }

        List<int> remainingColumns = new List<int>() { 0, 1, 2 };

        for (int i = 0; i < WashingMachineManager.Ins.washingMachineStartSlots.Count; i++)
        {
            WashingMachineStartSlot antBoxStartSlot = WashingMachineManager.Ins.washingMachineStartSlots[i];

            if (antBoxStartSlot.washingMachine != null && remainingColumns.Contains(antBoxStartSlot.column))
            {
                if (antBoxStartSlot.washingMachine.isDouble || antBoxStartSlot.washingMachine is FreezedAntBox)
                {
                    remainingColumns.Remove(antBoxStartSlot.column);
                }
            }
        }

        if (remainingColumns.Count < 2)
        {
            return false;
        }
        return GetNumLeft() > 0 && IsCorrectOrder(numAntBoxesSpawned);
    }
}

[Serializable]
public class FrozenAntBoxConfig : SpecialElementItemConfig
{
    public List<int> numBoxToUnfreeze;
}

[Serializable]
public class SpawningTileBoxConfig
{
    public List<SpawningTileBoxItemConfig> items;
}

[Serializable]
public class SpawningTileBoxItemConfig
{
    public Color faction;
    public int number;
    public int startNodeId = -1;
    public int numCoverColumn = 4;
    public int numCoverRow = 6;
}

[Serializable]
public class BossAreaConfig : SpecialElementItemConfig
{
    public int startNodeId;
    public int numCoverRow;
    public int numCoverColumn;

    public int bossHealth;

    public override bool IsValidToSpawn(int numAntBoxesSpawned, bool isOpenClose = false)
    {
        if (spawnOrder == null)
        {
            return false;
        }
        return GetNumLeft() > 0 && IsCorrectOrder(numAntBoxesSpawned);
    }
}

[Serializable]
public class SnakeConfig : SpecialElementItemConfig

{
    public List<int> snakeSegmentIndexes;
    public int health;
    public Vector2 tailOffset;
    public float tailAngle;
    public float width;
    public float scaleHead;
    public bool isFlipHead;
}


[Serializable]
public class OpenCloseAntBoxConfig : SpecialElementItemConfig
{
    public List<bool> initialStates;
}

[Serializable]
public class LockedSlotConfig
{
    public List<int> lockedSlotIndices;
    public SpecialElementItemConfig keyAntBoxConfig;
}