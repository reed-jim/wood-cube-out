using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
#if UNITY_EDITOR
public class BirdCategorizerTool : EditorWindow
{
    private List<string> txtFiles = new List<string>();
    private string outputFilePath = "Assets/Output/Statistics.json";
    private Vector2 scrollPosition;

    [MenuItem("Tools/Bird Categorizer")]
    public static void ShowWindow()
    {
        GetWindow<BirdCategorizerTool>("Bird Categorizer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Drag and Drop TXT Files Here", EditorStyles.boldLabel);
        var dropArea = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));

        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        }

        if (evt.type == EventType.DragPerform && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.AcceptDrag();
            foreach (var draggedFile in DragAndDrop.paths)
            {
                if (Path.GetExtension(draggedFile) == ".txt")
                {
                    txtFiles.Add(draggedFile);
                }
            }
            evt.Use();
        }

        GUILayout.Label($"Files Added: {txtFiles.Count}");

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));

        for (int i = 0; i < txtFiles.Count; i++)
        {
            GUILayout.Label(txtFiles[i]);
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Update IDs and Save"))
        {
            UpdateIdsAndSave();
        }

        if (GUILayout.Button("Categorize Birds and Generate Statistics"))
        {
            CategorizeBirdsAndGenerateStatistics();
        }

        if (GUILayout.Button("Clear List"))
        {
            txtFiles.Clear();
        }

        GUILayout.Label("Output File Path");
        outputFilePath = EditorGUILayout.TextField(outputFilePath);
    }

    private void UpdateIdsAndSave()
    {
        foreach (var file in txtFiles)
        {
            string json = File.ReadAllText(file);
            var data = JsonConvert.DeserializeObject<RootObject>(json);

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
            int newId = ExtractIdFromFileName(fileNameWithoutExtension);
            if (newId != -1)
            {
                data.id = newId;
            }

            if (newId > 1000)
            {
                if (newId % 10 == 0)
                {
                    data.idHardMode = 2;
                    data.min = 10;
                    data.second = 45;
                }
                else
                if (newId % 5 == 0)
                {
                    data.idHardMode = 1;
                    data.min = 15;
                    data.second = 30;
                }
                else
                {
                    data.min = 20;
                    data.second = 0;
                }
            }
            else
            {
                data.min = 10;
                data.second = 0;
            }

            string outputJson = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(file, outputJson);
            Debug.Log($"Updated file saved to {file}");
            AssetDatabase.Refresh();
        }
    }

    private int ExtractIdFromFileName(string fileName)
    {
        var match = Regex.Match(fileName, @"\d+");
        if (match.Success)
        {
            return int.Parse(match.Value);
        }
        return -1;
    }

    private void CategorizeBirdsAndGenerateStatistics()
    {
        var levelStatistics = new Dictionary<int, Dictionary<string, Dictionary<string, int>>>();
        var extralsInfo = new Dictionary<int, Dictionary<string, int>>();
        var difficultyScores = new Dictionary<int, int>();
        var birdInBranch = new Dictionary<int, int>();

        foreach (var file in txtFiles)
        {
            string json = File.ReadAllText(file);
            var data = JsonConvert.DeserializeObject<RootObject>(json);

            foreach (var config in data.standConfig)
            {
                int level = data.id;
                if (!levelStatistics.ContainsKey(level))
                {
                    levelStatistics[level] = new Dictionary<string, Dictionary<string, int>>();
                }

                foreach (var bird in config.idBirds)
                {
                    string birdName = GetNameOfBird(bird);
                    string typeKey = GetTypeKey(bird);

                    if (!levelStatistics[level].ContainsKey(typeKey))
                    {
                        levelStatistics[level][typeKey] = new Dictionary<string, int>();
                    }

                    if (levelStatistics[level][typeKey].ContainsKey(birdName))
                    {
                        levelStatistics[level][typeKey][birdName]++;
                    }
                    else
                    {
                        levelStatistics[level][typeKey][birdName] = 1;
                    }
                }
                birdInBranch[level] = config.standData.numSlot;
            }
            int levelId = data.id;

            if (!extralsInfo.ContainsKey(levelId))
            {
                extralsInfo[levelId] = new Dictionary<string, int>();
            }

            foreach (var extral in data.extralsConfig)
            {
                string typeName = GetExtralTypeName(extral.type);
                if (extralsInfo[levelId].ContainsKey(typeName))
                {
                    extralsInfo[levelId][typeName]++;
                }
                else
                {
                    extralsInfo[levelId][typeName] = 1;
                }
            }
        }

        // Tính toán độ khó
        foreach (var level in levelStatistics.Keys)
        {
            int birdTypesCount = levelStatistics[level].Count;
            int stylesCount = 0;
            int totalBirdCount = 0;

            // Đếm số kiểu chim
            foreach (var type in levelStatistics[level])
            {
                if (stylesCount < type.Value.Count)
                {
                    stylesCount = type.Value.Count;
                }
                totalBirdCount += type.Value.Count;
            }

            // Tính điểm độ khó từ số loại chim
            int difficulty = (int)Math.Ceiling(CalculateDifficulty(birdTypesCount, stylesCount, totalBirdCount, birdInBranch[level]));

            // Tính độ khó từ extralConfig
            foreach (var extral in extralsInfo[level])
            {
                int extralTypeScore = GetExtralTypeScore(extral.Key);
                difficulty += extral.Value * extralTypeScore; // Nhân số lần xuất hiện với độ khó của loại
            }

            difficultyScores[level] = difficulty;
        }

        // Tạo kết quả cuối cùng
        var finalOutput = new Dictionary<int, object>();
        foreach (var level in levelStatistics.Keys)
        {
            finalOutput[level] = new
            {
                Statistics = levelStatistics[level],
                ExtralTypes = extralsInfo[level],
                DifficultyScore = difficultyScores[level],
                NumSlot = birdInBranch[level]
            };
        }

        string outputJson = JsonConvert.SerializeObject(finalOutput, Formatting.Indented);
        File.WriteAllText(outputFilePath, outputJson);
        UploadToGoogleSheet(outputJson);
        Debug.Log($"Statistics saved to {outputFilePath}");
    }

    private float CalculateDifficulty(int birdTypesCount, int stylesCount, int totalBird, int birdInbranch)
    {
        float difficulty = 0;

        difficulty += totalBird * 10;

        switch (birdTypesCount)
        {
            case 1: difficulty += 7.5f; break;
            case 2: difficulty += 15; break;
            case 3: difficulty += 22.5f; break;
            case 4: difficulty += 30; break;
            case 5: difficulty += 37.5f; break;
            case 6: difficulty += 45; break;
            case 7: difficulty += 52.5f; break;
            case 8: difficulty += 60; break;
            case 9: difficulty += 67.5f; break;
            case 10: difficulty += 75; break;
            default: break;  // Trường hợp không xác định
        }

        switch (stylesCount)
        {
            case 2: difficulty += 20; break;
            case 3: difficulty += 40; break;
            case 4: difficulty += 60; break;
            case 5: difficulty += 80; break;
            case 6: difficulty += 100; break;
            case 7: difficulty += 120; break;
            default: break;  // Trường hợp không xác định
        }
        difficulty += (totalBird * 1.0f / birdTypesCount) * 50;
        
        difficulty += birdInbranch * 30;

        return difficulty;
    }

    private int GetExtralTypeScore(string typeName)
    {
        switch (typeName)
        {
            case "bomb": return 20; // Điểm cho loại bomb
            case "cage": return 20; // Điểm cho loại cage
            case "egglock": return 30; // Điểm cho loại egglock
            case "sleepbird": return 40; // Điểm cho loại sleepbird
            case "lockstand": return 60; // Điểm cho loại lockstand
            default: return 0; // Trường hợp không xác định
        }
    }

    private string GetExtralTypeName(int type)
    {
        switch (type)
        {
            case 1: return "bomb";
            case 2: return "cage";
            case 3: return "egglock";
            case 4: return "sleepbird";
            case 5: return "lockstand";
            default: return "unknown";
        }
    }

    private string GetTypeKey(int bird)
    {
        int segment = bird / 10 * 10;
        return GetBirdName(segment);
    }

    private string GetBirdName(int birdCode)
    {
        switch (birdCode)
        {
            case 0: return $"{birdCode}_chim đỏ";
            case 10: return $"{birdCode}_chim nâu";
            case 20: return $"{birdCode}_chim xanh biển";
            case 30: return $"{birdCode}_chim xám mào đỏ";
            case 40: return $"{birdCode}_chim vàng mào cam";
            case 50: return $"{birdCode}_chim xanh lá mỏ cam";
            case 60: return $"{birdCode}_chim tím mỏ vàng";
            case 70: return $"{birdCode}_chim hồng mỏ đỏ";
            case 80: return $"{birdCode}_chim đen mỏ vàng";
            case 90: return $"{birdCode}_chim vàng cánh xanh";
            default: return "không xác định";
        }
    }

    private string GetNameOfBird(int birdCode)
    {
        int segment = birdCode % 10;
        switch (segment)
        {
            case 0: return $"{birdCode}_Mặc định";
            default: return $"{birdCode}_Kiểu {segment}";
        }
    }

    public class StandConfig
    {
        public List<int> idBirds { get; set; }
        public StandData standData { get; set; }
        public int side { get; set; }
    }

    public class StandData
    {
        public int type { get; set; }
        public int numSlot { get; set; }
    }

    public class ExtralConfig
    {
        public int type { get; set; }
        public PositionData positionData { get; set; }
        public List<string> extralValues { get; set; }
        public ParentData birdParent { get; set; }
        public ParentData standParent { get; set; }
    }

    public class PositionData
    {
        public int posType { get; set; }
        public int indexStand { get; set; }
        public int indexBird { get; set; }
        public LocalPosition localPosition { get; set; }
    }

    public class LocalPosition
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class ParentData
    {
        public int instanceID { get; set; }
    }

    public class RootObject
    {
        public int id { get; set; }
        public int idHardMode { get; set; }
        public List<StandConfig> standConfig { get; set; }
        public List<ExtralConfig> extralsConfig { get; set; }
        public int min { get; set; }
        public int second { get; set; }
    }

    public class LevelData
    {
        public Dictionary<string, Dictionary<string, int>> Statistics { get; set; }
        public Dictionary<string, int> ExtralTypes { get; set; }
        public int DifficultyScore { get; set; }

        public int NumSlot { get; set; }
    }

    public async Task UploadToGoogleSheet(string json, string spreadsheetId = "1zewCkrFoA887ZQaJ4yx2Hy_ElrR5y5LJV31Se_jESVI")
    {
        var levels = JsonConvert.DeserializeObject<Dictionary<string, LevelData>>(json);

        var credential = GoogleCredential.FromFile(@"Assets/LongTH/birdsort2-ecbf7ebc01bd.json")
            .CreateScoped(SheetsService.Scope.Spreadsheets);

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Birdsort2",
        });

        var range = "Level!A:G"; // Đặt tên sheet và ô bắt đầu
        var valueRange = new ValueRange();

        // Đặt tiêu đề cột
        var header = new List<object> { "Level", "Total", "Bird Category", "Bird Style", "Bird In Branch", "Special", "Difficulty Point" };
        valueRange.Values = new List<IList<object>> { header };

        foreach (var level in levels)
        {
            var levelId = level.Key;
            var levelData = level.Value;

            // Chuyển đổi các loại extral thành chuỗi
            string special = "[" + string.Join(" ; ", levelData.ExtralTypes.Select(e => $"{e.Key}: {e.Value}")) + "]";

            // Tính toán
            var birdCategories = levelData.Statistics.Keys.ToList();
            int birdCategoryCount = birdCategories.Count;
            int totalStylesCount = levelData.Statistics.Sum(bird => bird.Value.Count);
            int maxBirdStyle = levelData.Statistics.Values
                .SelectMany(bird => bird.Values)
                .DefaultIfEmpty(0)
                .Max();

            int numSlot = levelData.NumSlot;

            // Tạo hàng dữ liệu
            var row = new List<object> { levelId, totalStylesCount, birdCategoryCount, maxBirdStyle, numSlot, special, levelData.DifficultyScore };

            valueRange.Values.Add(row);
        }

        // Ghi dữ liệu lên Google Sheets
        var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        try
        {
            await updateRequest.ExecuteAsync();
            Debug.Log("Data uploaded successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error uploading data: {ex.Message}");
        }
    }
}
#endif