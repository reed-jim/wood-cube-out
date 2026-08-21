using UnityEngine;
using static GameEnum;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using AntsFlow;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AntsFlow
{
    public enum ConfigTool
    {
        Normal,
        GiantChicken,
        Wall,
        FactionConfig
    }

#if UNITY_EDITOR
    public class ColorDropdown : PopupWindowContent
    {
        private readonly List<Color> colors;
        private readonly Color selectedColor;
        private readonly System.Action<Color> onSelected;

        public ColorDropdown(
            List<Color> colors,
            Color selectedColor,
            System.Action<Color> onSelected)
        {
            this.colors = colors;
            this.selectedColor = selectedColor;
            this.onSelected = onSelected;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(220, Mathf.Max(40, colors.Count * 32 + 10));
        }

        public override void OnGUI(Rect rect)
        {
            if (colors == null || colors.Count == 0)
            {
                EditorGUILayout.LabelField("No factions");
                return;
            }

            for (int i = 0; i < colors.Count; i++)
            {
                Color color = colors[i];

                Rect buttonRect = EditorGUILayout.GetControlRect(
                    GUILayout.Height(26)
                );

                if (GUI.Button(buttonRect, GUIContent.none))
                {
                    onSelected?.Invoke(color);
                    editorWindow.Close();
                }

                // Color square
                Rect colorRect = new Rect(
                    buttonRect.x + 4,
                    buttonRect.y + 3,
                    20,
                    20
                );

                EditorGUI.DrawRect(
                    colorRect,
                    color
                );

                // Name
                Rect labelRect = new Rect(
                    buttonRect.x + 30,
                    buttonRect.y,
                    buttonRect.width - 30,
                    buttonRect.height
                );

                EditorGUI.LabelField(
                    labelRect,
                    $"Faction {i}"
                );

                // Selected indicator
                if (color == selectedColor)
                {
                    Rect checkRect = new Rect(
                        buttonRect.xMax - 25,
                        buttonRect.y,
                        20,
                        buttonRect.height
                    );

                    EditorGUI.LabelField(
                        checkRect,
                        "✓"
                    );
                }
            }
        }
    }
#endif

    public class LevelItem : MonoBehaviour
    {
        public Sprite picture;

        public int numColumn;
        public int numRow;
        public int boardShapePresetIndex;
        public bool isLevelHavingTransparentPixels;
        public bool isOverrideLevelScale;
        public float chickenAreaScaleX;
        public float chickenAreaScaleY;
        public List<Color> poolFaction;
        public List<BaseTile> tiles;
        public List<AntLevelDataItem> chickenLevelTiles;
        public bool isTutorialLevel;
        public bool isTutorialSpeedUpLevel;
        public bool isUseManualFactionSetupForFirstAntBoxes;
        public LevelFactionConfig levelFactionConfig;
        [HideInInspector] public int difficulty;
        [HideInInspector] public int initialDifficulty;
        [HideInInspector] public bool overrideInitialDifficulty;
        public int numStartAntBoxes = 6;
        public SpecialElementConfig specialElementConfig;

        public FactionMapper factionMapper;

        public List<Color> factions;

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
            AntLevelData antLevelData = new AntLevelData();

            antLevelData.antLevelDataItems = new List<AntLevelDataItem>();

            for (int i = 0; i < tiles.Count; i++)
            {
                BaseTile tile = tiles[i];

                AntLevelDataItem dataItem = new AntLevelDataItem();
                dataItem.position = tile.position;
                dataItem.factionIndex = factionMapper.mapper.FindIndex(item => item.color == tile.Faction);

                antLevelData.antLevelDataItems.Add(dataItem);
            }

            List<int> poolFactionIndexes = factionMapper.GetPoolFactionIndexFromPoolFaction(poolFaction);

            antLevelData.levelItemData = new LevelItemData()
            {
                levelScale = transform.localScale,
                numColumn = numColumn,
                numRow = numRow,
                boardShapePresetIndex = boardShapePresetIndex,
                chickenLevelTiles = chickenLevelTiles,
                isLevelHavingTransparentPixels = isLevelHavingTransparentPixels,
                isOverrideLevelScale = isOverrideLevelScale,
                chickenAreaScaleX = chickenAreaScaleX,
                chickenAreaScaleY = chickenAreaScaleY,
                poolFactionIndexes = poolFactionIndexes,
                isTutorialLevel = isTutorialLevel,
                isTutorialSpeedUpLevel = isTutorialSpeedUpLevel,
                isUseManualFactionSetupForFirstAntBoxes = isUseManualFactionSetupForFirstAntBoxes,
                levelFactionConfig = levelFactionConfig,
                difficulty = difficulty,
                initialDifficulty = initialDifficulty,
                overrideInitialDifficulty = overrideInitialDifficulty,
                numStartAntBoxes = numStartAntBoxes,
                specialElementConfig = specialElementConfig
            };

            // Use level name as file name
            string filePath = Path.Combine("Assets/Chickens Flow/JSON/Levels", $"{name}.json");

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
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LevelItem))]
    public class LevelItemEditor : Editor
    {
        private string hexInput;

        private Vector2 tileSpacing = 1.6f * Vector2.one;

        private Color selectedColor;

        #region Normal Config
        private int brushRadius = 0;
        private Vector2Int? lastPaintCoord = null;
        private bool isPainting = false;
        private bool undoRecorded = false;
        private bool isErase;
        #endregion

        private ConfigTool configTool;

        #region Faction Config Demo
        private const int FactionConfigColumnCount = 3;
        private int[] factionConfigColumnRows = new int[FactionConfigColumnCount] { 5, 5, 5 };
        private int factionPaintNumber = 10;
        #endregion

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelItem levelItem = (LevelItem)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Chicken Jam Level Tool", EditorStyles.boldLabel);

            Rect rect = EditorGUILayout.GetControlRect();

            // Label
            rect = EditorGUI.PrefixLabel(rect, new GUIContent("Difficulty"));

            // Reserve space for the number
            float numberWidth = 30f;
            Rect sliderRect = new Rect(
                rect.x,
                rect.y,
                rect.width - numberWidth,
                rect.height
            );

            Rect numberRect = new Rect(
                rect.x + rect.width - numberWidth,
                rect.y,
                numberWidth,
                rect.height
            );

            // Slider
            levelItem.difficulty = EditorGUI.IntSlider(
                sliderRect,
                levelItem.difficulty,
                0,
                10
            );

            if (GUILayout.Button("Update Manual Bucket Config"))
            {
                levelItem.levelFactionConfig.ShowDebug(levelItem, levelItem.chickenLevelTiles);
            }

            if (GUILayout.Button("Generate Level From A Picture"))
            {
                ReadLevel(levelItem);
            }

            if (GUILayout.Button("Randomize Level"))
            {
                GenerateChickenLevelTiles(levelItem);
            }

            if (GUILayout.Button("Refresh Slots"))
            {
                RefreshSlots(levelItem);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Add Rows / Columns", EditorStyles.boldLabel);

            if (GUILayout.Button("AddBottomRow"))
            {
                AddBottomRow(levelItem, levelItem.numColumn, levelItem.numRow);
            }

            if (GUILayout.Button("AddTopRow"))
            {
                AddTopRow(levelItem, levelItem.numColumn, levelItem.numRow);
            }

            if (GUILayout.Button("AddLeftColumn"))
            {
                AddLeftColumn(levelItem, levelItem.numColumn, levelItem.numRow);
            }

            if (GUILayout.Button("AddRightColumn"))
            {
                AddRightColumn(levelItem, levelItem.numColumn, levelItem.numRow);
            }

            #region Remove
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Remove Rows / Columns", EditorStyles.boldLabel);

            if (GUILayout.Button("RemoveBottomRow"))
            {
                RemoveBottomRow(levelItem, levelItem.numColumn, levelItem.numRow);
            }

            if (GUILayout.Button("RemoveTopRow"))
            {
                RemoveTopRow(levelItem, levelItem.numColumn, levelItem.numRow);
            }

            if (GUILayout.Button("RemoveLeftColumn"))
            {
                RemoveLeftColumn(levelItem, levelItem.numColumn, levelItem.numRow);
            }

            if (GUILayout.Button("RemoveRightColumn"))
            {
                RemoveRightColumn(levelItem, levelItem.numColumn, levelItem.numRow);
            }
            #endregion

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);

            if (GUILayout.Button("GetPoolFaction"))
            {
                GetPoolFaction(levelItem);
            }

            if (GUILayout.Button("Convert to JSON"))
            {
                levelItem.ConvertToJson();
            }

            if (GUILayout.Button("Clear All Special Elements"))
            {
                levelItem.ClearAllSpecialElements();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Faction Config Demo (3 Columns)", EditorStyles.boldLabel);

            if (GUILayout.Button(configTool == ConfigTool.FactionConfig ? "Disable Faction Config Demo" : "Enable Faction Config Demo (Scene View)"))
            {
                if (configTool == ConfigTool.FactionConfig)
                {
                    configTool = ConfigTool.Normal;
                }
                else
                {
                    configTool = ConfigTool.FactionConfig;
                    InitFactionConfigRows(levelItem);
                }

                SceneView.RepaintAll();
            }

            if (configTool == ConfigTool.FactionConfig)
            {
                DrawFactionConfigControls(levelItem);
            }

            GUILayout.Space(10);
            GUILayout.Label("Scene Config", EditorStyles.boldLabel);

            if (GUILayout.Button(configTool == ConfigTool.Normal ? "Disable Normal Config" : "Normal Config"))
            {
                configTool = ConfigTool.Normal;
                SceneView.RepaintAll();
            }

            if (GUILayout.Button(configTool == ConfigTool.GiantChicken ? "Disable Giant Chicken Config" : "Giant Chicken Config"))
            {
                configTool = ConfigTool.GiantChicken;
                SceneView.RepaintAll();
            }

            if (GUILayout.Button(configTool == ConfigTool.Wall ? "Disable Wall Config" : "Wall Config"))
            {
                configTool = ConfigTool.Wall;
                SceneView.RepaintAll();
            }

            brushRadius = EditorGUILayout.IntField("Brush Size", brushRadius);
            brushRadius = Mathf.Max(brushRadius, 0);

            isErase = EditorGUILayout.Toggle("isErase", isErase);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);

            DrawColorButton(
                levelItem,
                selectedColor
            );

            serializedObject.ApplyModifiedProperties();
        }

        #region Chicken Feed
        private void OnSceneGUI()
        {
            ConfiguringLevel();
        }

        private Color GetDifficultyColor(int difficulty)
        {
            difficulty = Mathf.Clamp(difficulty, 0, 10);

            if (difficulty <= 5)
                return Color.Lerp(Color.green, Color.yellow, difficulty / 5f);

            return Color.Lerp(Color.yellow, Color.red, (difficulty - 5) / 5f);
        }

        private void ConfiguringLevel()
        {
            LevelItem level = (LevelItem)target;

            if (level.factionMapper == null)
                level.factionMapper = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Chickens Flow/Prefabs/Faction Mapper.prefab")?.GetComponent<FactionMapper>();

            if (level.factionMapper == null)
                return;

            if (configTool == ConfigTool.FactionConfig)
            {
                ConfiguringFactionConfig(level);
            }
            else if (configTool == ConfigTool.GiantChicken)
            {
                ConfigureGrid(level, PaintGiantChickenTile, EraseGiantChickenTile);
            }
            else
            {
                ConfigureGrid(level, PaintNormalTile, EraseNormalTile);
            }
        }

        private void PaintNormalTile(LevelItem level, int index)
        {
            level.chickenLevelTiles[index].factionIndex = level.factionMapper.GetFactionIndexEditMode(selectedColor);
        }

        private void EraseNormalTile(LevelItem level, int index)
        {
            level.chickenLevelTiles[index].factionIndex = -1;
        }

        private void ConfigureGrid(LevelItem level, Action<LevelItem, int> paintTile, Action<LevelItem, int> eraseTile)
        {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            Event e = Event.current;

            int numColumn = level.numColumn;
            int numRow = level.numRow;
            float tileDistance = 0.1f;

            float backgroundWidth = 10f;

            Vector3[] background =
            {
                new Vector3(-backgroundWidth, -backgroundWidth, 1),
                new Vector3(-backgroundWidth, +backgroundWidth, 1),
                new Vector3(+backgroundWidth, +backgroundWidth, 1),
                new Vector3(+backgroundWidth, -backgroundWidth, 1),
            };

            Handles.DrawSolidRectangleWithOutline(background, new Color(0, 0, 0, 0.85f), Color.white);

            List<AntLevelDataItem> chickenLevelTiles = level.chickenLevelTiles;
            Camera camera = SceneView.currentDrawingSceneView.camera;

            // Draw Normal Slots
            for (int y = 0; y < numRow; y++)
            {
                for (int x = 0; x < numColumn; x++)
                {
                    int index = x + y * numColumn;
                    Vector3 worldPos = tileDistance * new Vector2(x, y);

                    float size = HandleUtility.GetHandleSize(worldPos) * 0.15f;

                    if (index < chickenLevelTiles.Count)
                    {
                        if (chickenLevelTiles[index].factionIndex == -1)
                        {
                            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
                            {
                                alignment = TextAnchor.MiddleCenter,
                                fontSize = 18
                            };

                            Handles.Label(worldPos, "❌", style);
                        }
                        else
                        {
                            Handles.color = level.factionMapper.GetFaction(chickenLevelTiles[index].factionIndex);
                            Handles.DrawSolidDisc(worldPos, camera.transform.forward, size);
                        }
                    }

                    Handles.color = Color.gray;
                    Handles.DrawWireDisc(worldPos, camera.transform.forward, size);
                }
            }

            // Draw Giant Chicken Slots
            var giantChickenConfigs =
                    level.specialElementConfig.giantChickenConfig.giantChickenItemConfigs;

            if (giantChickenConfigs != null)
            {
                foreach (GiantChickenItemConfig chicken in giantChickenConfigs)
                {
                    DrawGiantChicken(chicken, tileDistance);
                }
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                isPainting = true;
                undoRecorded = false;
                lastPaintCoord = null;
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);

                PaintAtMousePosition(level, e.mousePosition, tileDistance, numColumn, numRow, paintTile, eraseTile);
                e.Use();
            }

            if (e.type == EventType.MouseDrag && e.button == 0 && isPainting)
            {
                PaintAtMousePosition(level, e.mousePosition, tileDistance, numColumn, numRow, paintTile, eraseTile);
                e.Use();
            }

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                isPainting = false;
                undoRecorded = false;
                lastPaintCoord = null;
                GUIUtility.hotControl = 0;
                e.Use();
            }

            if (isPainting || e.type == EventType.MouseMove)
                DrawBrushPreview(e.mousePosition, tileDistance, camera);

            if (GUI.changed)
                SceneView.RepaintAll();
        }

        private void PaintAtMousePosition(LevelItem level, Vector2 mousePosition, float tileDistance, int numColumn, int numRow, Action<LevelItem, int> paintTile, Action<LevelItem, int> eraseTile)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Plane gridPlane = new Plane(Vector3.forward, Vector3.zero);

            if (!gridPlane.Raycast(ray, out float distance))
                return;

            Vector3 worldPosition = ray.GetPoint(distance);

            Vector2Int currentCoord = new Vector2Int(
                Mathf.RoundToInt(worldPosition.x / tileDistance),
                Mathf.RoundToInt(worldPosition.y / tileDistance)
            );

            if (lastPaintCoord.HasValue)
                PaintLine(level, lastPaintCoord.Value, currentCoord, numColumn, numRow, paintTile, eraseTile);
            else
                PaintBrush(level, currentCoord, numColumn, numRow, paintTile, eraseTile);

            lastPaintCoord = currentCoord;

            SceneView.RepaintAll();
        }

        private void PaintLine(LevelItem level, Vector2Int from, Vector2Int to, int numColumn, int numRow, Action<LevelItem, int> paintTile, Action<LevelItem, int> eraseTile)
        {
            int distance = Mathf.Max(Mathf.Abs(to.x - from.x), Mathf.Abs(to.y - from.y));

            if (distance == 0)
            {
                PaintBrush(level, from, numColumn, numRow, paintTile, eraseTile);
                return;
            }

            for (int i = 0; i <= distance; i++)
            {
                float t = i / (float)distance;
                Vector2 interpolated = Vector2.Lerp(from, to, t);
                Vector2Int coord = new Vector2Int(Mathf.RoundToInt(interpolated.x), Mathf.RoundToInt(interpolated.y));
                PaintBrush(level, coord, numColumn, numRow, paintTile, eraseTile);
            }
        }

        private void PaintBrush(LevelItem level, Vector2Int center, int numColumn, int numRow, Action<LevelItem, int> paintTile, Action<LevelItem, int> eraseTile)
        {
            List<AntLevelDataItem> chickenLevelTiles = level.chickenLevelTiles;

            if (!undoRecorded)
            {
                Undo.RecordObject(level, "Paint Grid");
                undoRecorded = true;
            }

            for (int y = -brushRadius; y <= brushRadius; y++)
            {
                for (int x = -brushRadius; x <= brushRadius; x++)
                {
                    if (x * x + y * y > brushRadius * brushRadius)
                        continue;

                    int tileX = center.x + x;
                    int tileY = center.y + y;

                    if (tileX < 0 || tileX >= numColumn || tileY < 0 || tileY >= numRow)
                        continue;

                    int index = tileX + tileY * numColumn;

                    if (index >= chickenLevelTiles.Count)
                        continue;

                    if (isErase)
                        eraseTile(level, index);
                    else
                        paintTile(level, index);
                }
            }

            EditorUtility.SetDirty(level);
        }

        private void DrawBrushPreview(Vector2 mousePosition, float tileDistance, Camera camera)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Plane gridPlane = new Plane(Vector3.forward, Vector3.zero);

            if (!gridPlane.Raycast(ray, out float distance))
                return;

            Vector3 worldPosition = ray.GetPoint(distance);

            Vector2Int coord = new Vector2Int(
                Mathf.RoundToInt(worldPosition.x / tileDistance),
                Mathf.RoundToInt(worldPosition.y / tileDistance)
            );

            Vector3 center = new Vector3(coord.x * tileDistance, coord.y * tileDistance, 0);
            float brushSize = (brushRadius + 0.5f) * tileDistance;

            Handles.color = Event.current.shift ? new Color(1f, 0.2f, 0.2f, 0.8f) : new Color(1f, 1f, 1f, 0.8f);
            Handles.DrawWireDisc(center, camera.transform.forward, brushSize);
        }

        private void ConfiguringChicken()
        {
            LevelItem level = (LevelItem)target;

            if (level.specialElementConfig.giantChickenConfig.giantChickenItemConfigs == null)
                level.specialElementConfig.giantChickenConfig.giantChickenItemConfigs =
                    new List<GiantChickenItemConfig>();

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            Event e = Event.current;

            int numColumn = level.numColumn;
            int numRow = level.numRow;
            float tileDistance = 0.1f;

            // Background
            float backgroundWidth = 10f;

            Vector3[] background =
            {
                new Vector3(-backgroundWidth, -backgroundWidth, 1),
                new Vector3(-backgroundWidth, +backgroundWidth, 1),
                new Vector3(+backgroundWidth, +backgroundWidth, 1),
                new Vector3(+backgroundWidth, -backgroundWidth, 1),
            };

            Handles.DrawSolidRectangleWithOutline(
                background,
                new Color(0, 0, 0, 0.85f),
                Color.white);

            for (int y = 0; y < numRow; y++)
            {
                for (int x = 0; x < numColumn; x++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    Vector3 worldPos = tileDistance * new Vector2(x, y);

                    float size = HandleUtility.GetHandleSize(worldPos) * 0.15f;

                    GiantChickenItemConfig chicken =
                        level.specialElementConfig.giantChickenConfig.giantChickenItemConfigs
                            .Find(c => c.coordinate == coord);

                    if (chicken != null)
                    {
                        Vector3 center = worldPos +
                                         new Vector3(
                                             (chicken.sizeX - 1) * tileDistance * 0.5f,
                                             (chicken.sizeY - 1) * tileDistance * 0.5f);

                        Vector3 rectSize = new Vector3(
                            chicken.sizeX * tileDistance,
                            chicken.sizeY * tileDistance);

                        Handles.color = Color.yellow;

                        Handles.DrawSolidRectangleWithOutline(
                            new Vector3[]
                            {
                        center + new Vector3(-rectSize.x/2,-rectSize.y/2),
                        center + new Vector3(-rectSize.x/2, rectSize.y/2),
                        center + new Vector3( rectSize.x/2, rectSize.y/2),
                        center + new Vector3( rectSize.x/2,-rectSize.y/2),
                            },
                            new Color(1, 1, 0, 0.25f),
                            Color.yellow);

                        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 18
                        };

                        Handles.Label(center, "🐔", style);

                        Handles.color = Color.red;
                    }
                    else
                    {
                        Handles.color = Color.gray;
                    }

                    Handles.DrawWireDisc(
                        worldPos,
                        SceneView.currentDrawingSceneView.camera.transform.forward,
                        size);

                    if (HandleUtility.DistanceToCircle(worldPos, size) == 0)
                    {
                        HandleUtility.AddDefaultControl(
                            GUIUtility.GetControlID(FocusType.Passive));

                        if (e.type == EventType.MouseDown && e.button == 0)
                        {
                            Undo.RecordObject(level, "Chicken Config");

                            if (chicken == null)
                            {
                                chicken = new GiantChickenItemConfig
                                {
                                    coordinate = coord
                                };

                                level.specialElementConfig.giantChickenConfig
                                    .giantChickenItemConfigs.Add(chicken);
                            }
                            else
                            {
                                level.specialElementConfig.giantChickenConfig
                                    .giantChickenItemConfigs.Remove(chicken);
                            }

                            EditorUtility.SetDirty(level);

                            e.Use();
                        }
                    }
                }
            }

            if (GUI.changed)
                SceneView.RepaintAll();
        }

        private void PaintGiantChickenTile(LevelItem level, int index)
        {
            int numColumn = level.numColumn;

            int x = index % numColumn;
            int y = index / numColumn;

            Vector2Int coord = new Vector2Int(x, y);

            var configs = level.specialElementConfig
                .giantChickenConfig
                .giantChickenItemConfigs;

            GiantChickenItemConfig chicken =
                configs.Find(c => c.coordinate == coord);

            if (chicken == null)
            {
                chicken = new GiantChickenItemConfig
                {
                    coordinate = coord
                };

                configs.Add(chicken);
            }
        }

        private void EraseGiantChickenTile(LevelItem level, int index)
        {
            int numColumn = level.numColumn;

            int x = index % numColumn;
            int y = index / numColumn;

            Vector2Int coord = new Vector2Int(x, y);

            var configs = level.specialElementConfig
                .giantChickenConfig
                .giantChickenItemConfigs;

            GiantChickenItemConfig chicken =
                configs.Find(c => c.coordinate == coord);

            if (chicken != null)
            {
                configs.Remove(chicken);
            }
        }

        private void DrawGiantChicken(GiantChickenItemConfig chicken, float tileDistance)
        {
            Vector3 worldPos =
                tileDistance * new Vector2(
                    chicken.coordinate.x,
                    chicken.coordinate.y);

            Vector3 center = worldPos +
                             new Vector3(
                                 (chicken.sizeX - 1) * tileDistance * 0.5f,
                                 (chicken.sizeY - 1) * tileDistance * 0.5f);

            Vector3 rectSize = new Vector3(
                chicken.sizeX * tileDistance,
                chicken.sizeY * tileDistance);

            Handles.color = Color.yellow;

            Handles.DrawAAPolyLine(
                2f,
                new Vector3[]
                {
                    center + new Vector3(-rectSize.x / 2, -rectSize.y / 2),
                    center + new Vector3(-rectSize.x / 2,  rectSize.y / 2),
                    center + new Vector3( rectSize.x / 2,  rectSize.y / 2),
                    center + new Vector3( rectSize.x / 2, -rectSize.y / 2),
                    center + new Vector3(-rectSize.x / 2, -rectSize.y / 2),
                });

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            };

            Handles.Label(center, "🐔", style);
        }

        private void ConfiguringWall()
        {
            LevelItem level = (LevelItem)target;
            WallConfig wall = level.specialElementConfig.wallConfig;

            if (wall.coordinates == null)
                wall.coordinates = new();

            Event e = Event.current;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            int cols = level.numColumn;
            int rows = level.numRow;
            float tileDistance = 0.1f;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Vector2Int coord = new(x, y);
                    Vector3 pos = tileDistance * new Vector2(x, y);
                    float size = HandleUtility.GetHandleSize(pos) * 0.15f;

                    bool hasWall = wall.coordinates.Contains(coord);

                    Handles.color = hasWall ? Color.green : Color.gray;
                    Handles.DrawWireDisc(pos, SceneView.currentDrawingSceneView.camera.transform.forward, size);

                    if (hasWall)
                        Handles.DrawSolidDisc(pos, SceneView.currentDrawingSceneView.camera.transform.forward, size * .5f);

                    if (HandleUtility.DistanceToCircle(pos, size) == 0)
                    {
                        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                        if (e.type == EventType.MouseDown && e.button == 0)
                        {
                            Undo.RecordObject(level, "Wall Config");

                            if (hasWall) wall.coordinates.Remove(coord);
                            else wall.coordinates.Add(coord);

                            EditorUtility.SetDirty(level);
                            e.Use();
                        }
                    }
                }
            }

            if (GUI.changed)
                SceneView.RepaintAll();
        }

        private void GenerateChickenLevelTiles(LevelItem levelItem)
        {
            FactionMapper factionMapper =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Chickens Flow/Prefabs/Faction Mapper.prefab")?.GetComponent<FactionMapper>();

            levelItem.chickenLevelTiles = new List<AntLevelDataItem>();

            List<Color> allFactions = factionMapper.GetRandomFactions(8, levelItem.numRow * levelItem.numColumn);

            for (int y = 0; y < levelItem.numRow; y++)
            {
                for (int x = 0; x < levelItem.numColumn; x++)
                {
                    int index = x + y * levelItem.numColumn;

                    AntLevelDataItem tileData = new AntLevelDataItem();

                    tileData.position = new Vector3(x, y);
                    tileData.factionIndex = factionMapper.allFactions.IndexOf(allFactions[index]);

                    levelItem.chickenLevelTiles.Add(tileData);
                }
            }

            EditorUtility.SetDirty(levelItem);
        }

        private void RefreshSlots(LevelItem levelItem)
        {
            for (int y = 0; y < levelItem.numRow; y++)
            {
                for (int x = 0; x < levelItem.numColumn; x++)
                {
                    int index = x + y * levelItem.numColumn;

                    if (index < levelItem.chickenLevelTiles.Count)
                    {
                        continue;
                    }

                    AntLevelDataItem tileData = new AntLevelDataItem();

                    tileData.position = new Vector3(x, y);
                    tileData.factionIndex = -1;

                    levelItem.chickenLevelTiles.Add(tileData);
                }
            }

            EditorUtility.SetDirty(levelItem);
        }

        public void AddTopRow(LevelItem levelItem, int columns, int rows)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 pos = levelItem.chickenLevelTiles[(rows - 1) * columns + col].position;
                pos.y += 1;

                levelItem.chickenLevelTiles.Add(new AntLevelDataItem
                {
                    position = pos,
                    factionIndex = -1
                });
            }

            levelItem.numRow++;

            EditorUtility.SetDirty(levelItem);
        }

        public void AddBottomRow(LevelItem levelItem, int columns, int rows)
        {
            // Shift all existing tiles up by 1
            for (int i = 0; i < levelItem.chickenLevelTiles.Count; i++)
            {
                Vector3 pos = levelItem.chickenLevelTiles[i].position;
                pos.y += 1;

                levelItem.chickenLevelTiles[i].position = pos;
            }

            // Add new row at y = 0
            var newTiles = new List<AntLevelDataItem>(columns);

            for (int col = 0; col < columns; col++)
            {
                newTiles.Add(new AntLevelDataItem
                {
                    position = new Vector3(col, 0, 0),
                    factionIndex = -1
                });
            }

            levelItem.chickenLevelTiles.InsertRange(0, newTiles);

            levelItem.numRow++;

            EditorUtility.SetDirty(levelItem);
        }

        public void AddLeftColumn(LevelItem levelItem, int columns, int rows)
        {
            var oldTiles = levelItem.chickenLevelTiles;
            var newTiles = new List<AntLevelDataItem>((columns + 1) * rows);

            for (int row = 0; row < rows; row++)
            {
                // New left tile
                Vector3 newPos = oldTiles[row * columns].position;
                newPos.x = 0;

                newTiles.Add(new AntLevelDataItem
                {
                    position = newPos,
                    factionIndex = -1
                });

                // Existing tiles
                for (int col = 0; col < columns; col++)
                {
                    AntLevelDataItem oldTile = oldTiles[row * columns + col];

                    Vector3 pos = oldTile.position;
                    pos.x += 1;

                    oldTile.position = pos;

                    newTiles.Add(oldTile);
                }
            }

            levelItem.chickenLevelTiles = newTiles;
            levelItem.numColumn++;

            EditorUtility.SetDirty(levelItem);
        }

        public void AddRightColumn(LevelItem levelItem, int columns, int rows)
        {
            for (int row = rows - 1; row >= 0; row--)
            {
                int index = row * columns + (columns - 1);

                Vector3 pos = levelItem.chickenLevelTiles[index].position;
                pos.x += 1;

                levelItem.chickenLevelTiles.Insert(index + 1, new AntLevelDataItem
                {
                    position = pos,
                    factionIndex = -1
                });
            }

            levelItem.numColumn++;

            EditorUtility.SetDirty(levelItem);
        }

        public void RemoveTopRow(LevelItem levelItem, int columns, int rows)
        {
            if (rows <= 1)
                return;

            // Remove the last row
            int startIndex = (rows - 1) * columns;

            levelItem.chickenLevelTiles.RemoveRange(startIndex, columns);

            levelItem.numRow--;

            EditorUtility.SetDirty(levelItem);
        }

        public void RemoveBottomRow(LevelItem levelItem, int columns, int rows)
        {
            if (rows <= 1)
                return;

            // Remove the first row
            levelItem.chickenLevelTiles.RemoveRange(0, columns);

            // Shift all remaining tiles down by 1
            for (int i = 0; i < levelItem.chickenLevelTiles.Count; i++)
            {
                Vector3 pos = levelItem.chickenLevelTiles[i].position;
                pos.y -= 1;

                levelItem.chickenLevelTiles[i].position = pos;
            }

            levelItem.numRow--;

            EditorUtility.SetDirty(levelItem);
        }

        public void RemoveLeftColumn(LevelItem levelItem, int columns, int rows)
        {
            if (columns <= 1)
                return;

            var oldTiles = levelItem.chickenLevelTiles;
            var newTiles = new List<AntLevelDataItem>((columns - 1) * rows);

            for (int row = 0; row < rows; row++)
            {
                // Skip/remove the left-most tile
                for (int col = 1; col < columns; col++)
                {
                    AntLevelDataItem tile = oldTiles[row * columns + col];

                    // Shift remaining tiles left
                    Vector3 pos = tile.position;
                    pos.x -= 1;

                    tile.position = pos;

                    newTiles.Add(tile);
                }
            }

            levelItem.chickenLevelTiles = newTiles;
            levelItem.numColumn--;

            EditorUtility.SetDirty(levelItem);
        }

        public void RemoveRightColumn(LevelItem levelItem, int columns, int rows)
        {
            if (columns <= 1)
                return;

            var oldTiles = levelItem.chickenLevelTiles;
            var newTiles = new List<AntLevelDataItem>((columns - 1) * rows);

            for (int row = 0; row < rows; row++)
            {
                // Keep everything except the right-most tile
                for (int col = 0; col < columns - 1; col++)
                {
                    newTiles.Add(oldTiles[row * columns + col]);
                }
            }

            levelItem.chickenLevelTiles = newTiles;
            levelItem.numColumn--;

            EditorUtility.SetDirty(levelItem);
        }

        public void GetPoolFaction(LevelItem levelItem)
        {
            List<int> uniqueFactionIndexes = levelItem.chickenLevelTiles.Select(item => item.factionIndex).Distinct().ToList();

            levelItem.poolFaction = new List<Color>();

            for (int i = 0; i < uniqueFactionIndexes.Count; i++)
            {
                if (uniqueFactionIndexes[i] == -1)
                {
                    continue;
                }

                levelItem.poolFaction.Add(levelItem.factionMapper.allFactions[uniqueFactionIndexes[i]]);
            }

            EditorUtility.SetDirty(levelItem);
        }

        private void DrawColorButton(LevelItem levelItem, Color color)
        {
            // Main button
            GUIContent content = new GUIContent();

            Rect rect = EditorGUILayout.GetControlRect();

            if (GUI.Button(rect, content))
            {
#if UNITY_EDITOR
                PopupWindow.Show(
                    rect,
                    new ColorDropdown(
                        levelItem.factionMapper.allFactions,
                        color,
                        selected =>
                        {
                            selectedColor = selected;
                            Repaint();
                        }
                    )
                );
#endif
            }

            // Draw selected color
            Rect colorRect = new Rect(
                rect.x + 4,
                rect.y + 3,
                24,
                rect.height - 6
            );

            EditorGUI.DrawRect(
                colorRect,
                color
            );

            // Optional label
            Rect labelRect = new Rect(
                rect.x + 34,
                rect.y,
                rect.width - 34,
                rect.height
            );

            EditorGUI.LabelField(
                labelRect,
                "Select Faction"
            );
        }

        #region Faction Config Demo
        private void InitFactionConfigRows(LevelItem levelItem)
        {
            for (int c = 0; c < FactionConfigColumnCount; c++)
            {
                int existingCount = GetFactionColumn(levelItem, c)?.items?.Count ?? 0;

                factionConfigColumnRows[c] = Mathf.Max(factionConfigColumnRows[c], existingCount, 1);
            }
        }

        private void DrawFactionConfigControls(LevelItem levelItem)
        {
            // Faction selector (writes to selectedColor, shared with the Colors section)
            DrawColorButton(levelItem, selectedColor);

            factionPaintNumber = EditorGUILayout.IntField("Paint Number", factionPaintNumber);

            isErase = EditorGUILayout.Toggle("isErase", isErase);

            EditorGUILayout.LabelField("Rows Per Column", EditorStyles.miniBoldLabel);

            for (int c = 0; c < FactionConfigColumnCount; c++)
            {
                factionConfigColumnRows[c] = Mathf.Max(1, EditorGUILayout.IntField($"Col {c}", factionConfigColumnRows[c]));
            }

            EditorGUILayout.HelpBox(
                "Chọn faction, rồi click vào các ô vuông trong Scene view, từ trên xuống (index 0 trước, không bỏ qua ô). "
                + "Bật 'isErase' để xoá ô cuối cùng của cột. Mode giữ nguyên khi chọn liên tục.",
                MessageType.Info);

            DrawFactionConfigElements(levelItem);

            if (GUILayout.Button("Clear Faction Config"))
            {
                if (levelItem.levelFactionConfig != null)
                {
                    Undo.RecordObject(levelItem, "Clear Faction Config");
                    levelItem.levelFactionConfig.items = new List<LevelFactionConfigByColumn>();
                    EditorUtility.SetDirty(levelItem);

                    SceneView.RepaintAll();
                }
            }
        }

        private void DrawFactionConfigElements(LevelItem levelItem)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Elements (edit number)", EditorStyles.miniBoldLabel);

            bool anyElement = false;

            for (int c = 0; c < FactionConfigColumnCount; c++)
            {
                LevelFactionConfigByColumn column = GetFactionColumn(levelItem, c);

                if (column == null || column.items == null || column.items.Count == 0)
                    continue;

                anyElement = true;

                EditorGUILayout.LabelField($"Col {c}", EditorStyles.boldLabel);

                for (int r = 0; r < column.items.Count; r++)
                {
                    LevelFactionItemConfig item = column.items[r];

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField($"#{r}", GUILayout.Width(30));

                    Rect swatch = GUILayoutUtility.GetRect(18, 18, GUILayout.Width(18), GUILayout.ExpandWidth(false));
                    EditorGUI.DrawRect(swatch, item.faction);

                    EditorGUI.BeginChangeCheck();

                    int newNumber = EditorGUILayout.IntField(item.number);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(levelItem, "Edit Faction Number");
                        item.number = newNumber;
                        EditorUtility.SetDirty(levelItem);
                        SceneView.RepaintAll();
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            if (!anyElement)
            {
                EditorGUILayout.LabelField("(chưa có element nào)", EditorStyles.miniLabel);
            }
        }

        private void ConfiguringFactionConfig(LevelItem level)
        {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            Event e = Event.current;

            // Capture the whole scene view so clicking empty space does NOT deselect the
            // object (which would destroy this editor and drop out of the mode).
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            // Background
            float backgroundWidth = 10f;

            Vector3[] background =
            {
                new Vector3(-backgroundWidth, -backgroundWidth, 1),
                new Vector3(-backgroundWidth, +backgroundWidth, 1),
                new Vector3(+backgroundWidth, +backgroundWidth, 1),
                new Vector3(+backgroundWidth, -backgroundWidth, 1),
            };

            Handles.DrawSolidRectangleWithOutline(background, new Color(0, 0, 0, 0.85f), Color.white);

            const float colSpacing = 0.28f;
            const float rowSpacing = 0.24f;
            const float squareHalf = 0.09f;

            GUIStyle numberStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14
            };

            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            for (int c = 0; c < FactionConfigColumnCount; c++)
            {
                // Center the 3 columns around x = 0.
                float x = (c - (FactionConfigColumnCount - 1) * 0.5f) * colSpacing;

                LevelFactionConfigByColumn column = GetFactionColumn(level, c);
                int count = column?.items?.Count ?? 0;

                int displayRows = Mathf.Max(factionConfigColumnRows[c], count);

                // Header above the top-most cell (index 0).
                Handles.Label(new Vector3(x, rowSpacing, 0), $"Col {c}", headerStyle);

                for (int r = 0; r < displayRows; r++)
                {
                    // Index 0 at the top; rows go downward.
                    Vector3 pos = new Vector3(x, -r * rowSpacing, 0);

                    bool filled = r < count;

                    Color fill;
                    Color outline;

                    if (filled)
                    {
                        fill = column.items[r].faction;
                        outline = Color.black;
                    }
                    else
                    {
                        fill = new Color(0.22f, 0.22f, 0.22f, 1f);
                        outline = r == count
                            ? new Color(0.95f, 0.85f, 0.2f, 1f)
                            : new Color(0.45f, 0.45f, 0.45f, 1f);
                    }

                    DrawSceneSquare(pos, squareHalf, fill, outline);

                    if (filled)
                    {
                        numberStyle.normal.textColor = GetContrastingColor(fill);
                        Handles.Label(pos + new Vector3(-squareHalf * 0.6f, squareHalf * 0.7f, 0), column.items[r].number.ToString(), numberStyle);
                    }

                    // Click handling (keeps working continuously while this mode is active).
                    if (e.type == EventType.MouseDown && e.button == 0 && HandleUtility.DistanceToCircle(pos, squareHalf) == 0f)
                    {
                        OnFactionCellClicked(level, c, r);

                        e.Use();

                        SceneView.RepaintAll();
                    }
                }
            }

            if (GUI.changed)
                SceneView.RepaintAll();
        }

        private void DrawSceneSquare(Vector3 center, float half, Color fill, Color outline)
        {
            Vector3[] verts =
            {
                center + new Vector3(-half, -half, 0),
                center + new Vector3(-half, +half, 0),
                center + new Vector3(+half, +half, 0),
                center + new Vector3(+half, -half, 0),
            };

            Handles.DrawSolidRectangleWithOutline(verts, fill, outline);
        }

        private void OnFactionCellClicked(LevelItem levelItem, int column, int row)
        {
            Undo.RecordObject(levelItem, "Edit Faction Config");

            if (isErase)
            {
                LevelFactionConfigByColumn config = GetFactionColumn(levelItem, column);
                int count = config?.items?.Count ?? 0;

                if (count == 0)
                    return;

                // Keep the column contiguous: only the last filled cell can be removed.
                if (row == count - 1)
                {
                    config.items.RemoveAt(row);

                    if (config.items.Count == 0)
                    {
                        levelItem.levelFactionConfig.items.Remove(config);
                    }

                    EditorUtility.SetDirty(levelItem);
                }
                else
                {
                    Debug.LogWarning("Chỉ được xoá ô cuối cùng của cột để giữ index liên tục.");
                }

                return;
            }

            LevelFactionConfigByColumn columnConfig = GetOrCreateFactionColumn(levelItem, column);
            int currentCount = columnConfig.items.Count;

            if (row < currentCount)
            {
                // Update both faction and number for an existing cell.
                columnConfig.items[row].faction = selectedColor;
                columnConfig.items[row].number = factionPaintNumber;
            }
            else if (row == currentCount)
            {
                // Append the next cell in the column.
                columnConfig.items.Add(new LevelFactionItemConfig(selectedColor, factionPaintNumber));
            }
            else
            {
                Debug.LogWarning("Phải chấm tuần tự từ index 0, không được bỏ qua ô ở giữa.");
                return;
            }

            EditorUtility.SetDirty(levelItem);
        }

        private LevelFactionConfigByColumn GetFactionColumn(LevelItem levelItem, int column)
        {
            if (levelItem.levelFactionConfig?.items == null)
                return null;

            return levelItem.levelFactionConfig.items.Find(item => item.column == column);
        }

        private LevelFactionConfigByColumn GetOrCreateFactionColumn(LevelItem levelItem, int column)
        {
            if (levelItem.levelFactionConfig == null)
                levelItem.levelFactionConfig = new LevelFactionConfig();

            if (levelItem.levelFactionConfig.items == null)
                levelItem.levelFactionConfig.items = new List<LevelFactionConfigByColumn>();

            LevelFactionConfigByColumn config = levelItem.levelFactionConfig.items.Find(item => item.column == column);

            if (config == null)
            {
                config = new LevelFactionConfigByColumn
                {
                    column = column,
                    items = new List<LevelFactionItemConfig>()
                };

                levelItem.levelFactionConfig.items.Add(config);
                levelItem.levelFactionConfig.items.Sort((a, b) => a.column.CompareTo(b.column));
            }

            if (config.items == null)
                config.items = new List<LevelFactionItemConfig>();

            return config;
        }

        private Color GetContrastingColor(Color background)
        {
            float luminance = 0.299f * background.r + 0.587f * background.g + 0.114f * background.b;

            return luminance > 0.5f ? Color.black : Color.white;
        }
        #endregion
        #endregion

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

        private void GenerateLevel(LevelItem levelItem)
        {
            ReadLevel(levelItem);
            // GroupColors(levelItem);
            ReassignFactionForTiles(levelItem);
            AssignLevelPriority(levelItem);
            // FindTileBlockages(levelItem);
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

            level.chickenLevelTiles = new List<AntLevelDataItem>();

            for (float y = 0.25f * pixelStepY; y < height; y += pixelStepY)
            {
                for (float x = 0.25f * pixelStepX; x < width; x += pixelStepX)
                {
                    int pixelX = Mathf.FloorToInt(x);
                    int pixelY = Mathf.FloorToInt(y);

                    Color pixelColor = pixels[pixelY * width + pixelX];

                    // Skip transparent pixels if enabled
                    if (pixelColor.a < 1)
                        continue;

                    AntLevelDataItem antLevelDataItem = new AntLevelDataItem();
                    antLevelDataItem.position = new Vector3(Mathf.FloorToInt(x / pixelStepX), Mathf.FloorToInt(y / pixelStepY));
                    antLevelDataItem.factionIndex = level.factionMapper.GetNearestFactionIndexEditMode(pixelColor);
                    level.chickenLevelTiles.Add(antLevelDataItem);

                    index++;
                }
            }

            EditorUtility.SetDirty(level);
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
    public List<LevelFactionConfigByColumn> items;
#if UNITY_EDITOR
    public LevelFactionConfigDebug levelFactionConfigDebug;
#endif

    public void ShowDebug(AntsFlow.LevelItem levelItem, List<AntLevelDataItem> antLevelDataItems)
    {
        Dictionary<Color, int> numberByFaction = new Dictionary<Color, int>();

        foreach (var item in antLevelDataItems)
        {
            if (item.factionIndex >= 0)
            {
                CommonUtil.SumValueOfDictionary(numberByFaction, levelItem.factionMapper.allFactions[item.factionIndex], 10);
            }
        }

        foreach (var column in items)
        {
            for (int i = 0; i < column.items.Count; i++)
            {
                LevelFactionItemConfig levelFactionItemConfig = column.items[i];

                CommonUtil.SumValueOfDictionary(numberByFaction, levelFactionItemConfig.faction, -levelFactionItemConfig.number);
            }
        }

#if UNITY_EDITOR
        levelFactionConfigDebug = new LevelFactionConfigDebug();
        levelFactionConfigDebug.items = new List<LevelFactionConfigDebugItem>();

        foreach (var faction in numberByFaction.Keys)
        {
            LevelFactionConfigDebugItem item = new LevelFactionConfigDebugItem();

            item.faction = faction;
            item.remaning = numberByFaction[faction];

            levelFactionConfigDebug.items.Add(item);
        }
#endif
    }
}

[Serializable]
public class LevelFactionConfigByColumn
{
    public int column;
    public List<LevelFactionItemConfig> items;
}

[Serializable]
public class LevelFactionItemConfig
{
    public Color faction;
    public int number;

    public LevelFactionItemConfig()
    {
    }

    public LevelFactionItemConfig(Color faction, int number)
    {
        this.faction = faction;
        this.number = number;
    }
}

#if UNITY_EDITOR
[Serializable]
public class LevelFactionConfigDebug
{
    [ReadOnly] public List<LevelFactionConfigDebugItem> items;
}

[Serializable]
public class LevelFactionConfigDebugItem
{
    public Color faction;
    public int remaning;
}
#endif

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
}

[Serializable]
public class LevelItemData
{
    public Vector3 levelScale;
    public int numColumn;
    public int numRow;
    public int boardShapePresetIndex;
    public List<AntLevelDataItem> chickenLevelTiles;
    public bool isLevelHavingTransparentPixels;
    public bool isOverrideLevelScale;
    public float chickenAreaScaleX;
    public float chickenAreaScaleY;
    public List<int> poolFactionIndexes;
    public bool isTutorialLevel;
    public bool isTutorialSpeedUpLevel;
    public bool isUseManualFactionSetupForFirstAntBoxes;
    public LevelFactionConfig levelFactionConfig;
    public int difficulty;
    public int initialDifficulty;
    public bool overrideInitialDifficulty;
    public int numStartAntBoxes = 6;
    public SpecialElementConfig specialElementConfig;
}


[Serializable]
public class SpecialElementConfig
{
    public GiantChickenConfig giantChickenConfig;
    public WallConfig wallConfig;

    public SpecialElementItemConfig hiddenAntBoxConfig;
    public FrozenAntBoxConfig frozenAntBoxConfig;
    public SpecialElementItemConfig doubleAntBoxConfig;
    public SpawningTileBoxConfig spawningTileBoxConfig;
    public BossAreaConfig bossAreaConfig;
    public SnakeConfig snakeConfig;
    public bool isContainsHiddenTiles;
    public bool isContainsWallTiles;
}

[Serializable]
public class GiantChickenConfig
{
    public List<GiantChickenItemConfig> giantChickenItemConfigs;
}

[Serializable]
public class GiantChickenItemConfig
{
    public Vector2Int coordinate;
    public int sizeX = 2;
    public int sizeY = 2;
}

[Serializable]
public class WallConfig
{
    public List<Vector2Int> coordinates;
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

    public bool IsValidToSpawn(int numAntBoxesSpawned)
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
}

[Serializable]
public class BossAreaConfig : SpecialElementItemConfig
{
    public Vector2Int startTileCoordinate;
    public int numCoverColumn;
    public int numCoverRow;
    public int bossHealth;
}

[Serializable]
public class SnakeConfig : SpecialElementItemConfig

{
    public List<int> snakeSegmentIndexes;
    public int health;
}
