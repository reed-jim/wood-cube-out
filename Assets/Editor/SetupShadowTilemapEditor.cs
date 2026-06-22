using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using NamPhuThuy.GamePlay;
using AntsFlow;

public class SetupShadowTilemapEditor : EditorWindow
{
    [MenuItem("Tools/Ants Flow/Setup Shadow Tilemap in Scene")]
    static void SetupShadowTilemap()
    {
        // Find LevelLoader in current scene
        LevelLoader levelLoader = Object.FindObjectOfType<LevelLoader>();
        if (levelLoader == null)
        {
            EditorUtility.DisplayDialog("Error", "Không tìm thấy LevelLoader trong scene hiện tại!", "OK");
            return;
        }

        AntsFlow.TileManager tileManager = Object.FindObjectOfType<AntsFlow.TileManager>();

        // --- Handle existing Shadow Grid ---
        GameObject shadowGridGO = GameObject.Find("Shadow Grid");
        Tilemap shadowTilemap = null;

        if (shadowGridGO == null)
        {
            // Create Shadow Grid (Grid component)
            shadowGridGO = new GameObject("Shadow Grid");
            Undo.RegisterCreatedObjectUndo(shadowGridGO, "Create Shadow Grid");
            shadowGridGO.AddComponent<UnityEngine.Grid>();

            // Create Shadow Tilemap child
            GameObject shadowTilemapGO = new GameObject("Shadow Tilemap");
            Undo.RegisterCreatedObjectUndo(shadowTilemapGO, "Create Shadow Tilemap");
            shadowTilemapGO.transform.SetParent(shadowGridGO.transform, false);

            shadowTilemap = shadowTilemapGO.AddComponent<Tilemap>();
            TilemapRenderer shadowRenderer = shadowTilemapGO.AddComponent<TilemapRenderer>();
            shadowRenderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
            shadowRenderer.mode = TilemapRenderer.Mode.Individual;

            Debug.Log("[Setup] Đã tạo mới 'Shadow Grid' và 'Shadow Tilemap'.");
        }
        else
        {
            // Already exists — just grab the Tilemap component from child
            shadowTilemap = shadowGridGO.GetComponentInChildren<Tilemap>();
            if (shadowTilemap == null)
            {
                GameObject shadowTilemapGO = new GameObject("Shadow Tilemap");
                Undo.RegisterCreatedObjectUndo(shadowTilemapGO, "Create Shadow Tilemap child");
                shadowTilemapGO.transform.SetParent(shadowGridGO.transform, false);
                shadowTilemap = shadowTilemapGO.AddComponent<Tilemap>();
                shadowTilemapGO.AddComponent<TilemapRenderer>();
            }
            Debug.Log("[Setup] 'Shadow Grid' đã tồn tại — dùng lại.");
        }

        // --- Load shadow sprite ---
        Sprite shadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Ants Flow/Sprites/gameplay/shadow_pixel.png"
        );
        if (shadowSprite == null)
        {
            // Fallback: load by GUID
            string path = AssetDatabase.GUIDToAssetPath("c169d82cae7894044869e7f993bddf2f");
            if (!string.IsNullOrEmpty(path))
                shadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // --- Assign to LevelLoader ---
        SerializedObject soLoader = new SerializedObject(levelLoader);
        soLoader.FindProperty("shadowTilemap").objectReferenceValue = shadowTilemap;
        soLoader.FindProperty("shadowTilemapGrid").objectReferenceValue = shadowGridGO.transform;
        if (shadowSprite != null)
            soLoader.FindProperty("shadowSprite").objectReferenceValue = shadowSprite;
        soLoader.FindProperty("scaleFactor").floatValue = 0.55f;
        soLoader.FindProperty("shadowTilemapOffset").vector3Value = new Vector3(-0.3f, 0.3f, 0f);
        soLoader.ApplyModifiedProperties();

        // --- Assign to TileManager ---
        if (tileManager != null)
        {
            SerializedObject soTM = new SerializedObject(tileManager);
            var prop = soTM.FindProperty("shadowTilemap");
            if (prop != null)
            {
                prop.objectReferenceValue = shadowTilemap;
                soTM.ApplyModifiedProperties();
                Debug.Log("[Setup] ✅ Đã gán shadowTilemap vào AntsFlow.TileManager.");
            }
            else
            {
                Debug.LogWarning("[Setup] ⚠️ AntsFlow.TileManager không có serialized property 'shadowTilemap' — kiểm tra field có [SerializeField] không.");
            }
        }
        else
        {
            Debug.LogWarning("[Setup] Không tìm thấy TileManager trong scene.");
        }

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        string spriteStatus = shadowSprite != null ? "✅ Shadow Sprite: shadow_pixel" : "⚠️ Shadow Sprite: Cần gán thủ công";
        string tmStatus = tileManager != null ? "✅ AntsFlow.TileManager.shadowTilemap" : "⚠️ AntsFlow.TileManager: không tìm thấy trong scene";

        Debug.Log("[Setup] ✅ Hoàn tất! Đừng quên Save Scene (Ctrl+S).");

        EditorUtility.DisplayDialog("Thành công!",
            "Đã tạo và gán đầy đủ vào scene:\n\n" +
            "✅ LevelLoader.shadowTilemapGrid\n" +
            "✅ LevelLoader.shadowTilemap\n" +
            spriteStatus + "\n" +
            "✅ scaleFactor = 0.55\n" +
            "✅ shadowTilemapOffset = (-0.3, 0.3, 0)\n" +
            tmStatus + "\n\n" +
            "Đừng quên Save Scene (Ctrl+S)!",
            "OK");
    }
}
