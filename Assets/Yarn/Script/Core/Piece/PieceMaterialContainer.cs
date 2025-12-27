using System.IO;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static GameEnum;

[CreateAssetMenu(fileName = "PieceMaterialContainer", menuName = "ScriptableObjects/PieceMaterialContainer")]
public class PieceMaterialContainer : ScriptableObject
{
    [SerializeField] public Material[] materials;

    
    public static GameFaction ValidateFaction(GameFaction faction)
    {
        if (faction == GameFaction.YarnSky)
        {
            return GameFaction.YarnSnow;
        }
        else if (faction == GameFaction.YarnCyan)
        {
            return GameFaction.YarnDeep;
        }
        
        return faction;
    }
    
    public Color GetColor(GameFaction faction)
    {
        faction = ValidateFaction(faction);
        
        Material mat = materials[(int)faction];

        if (mat != null)
        {
            if (mat.HasProperty("_Color"))
                return mat.GetColor("_Color");
            else if (mat.HasProperty("_BaseColor")) // URP or HDRP
                return mat.GetColor("_BaseColor");
            else
                Debug.LogWarning($"Material {mat.name} has no _Color or _BaseColor property.");
        }
        return Color.white; // default fallback
    }

 
}

#if UNITY_EDITOR
[CustomEditor(typeof(PieceMaterialContainer))]
public class MaterialCollectionEditor : Editor
{
    string folderPath = "Assets/Yarn/Material/Yarn"; // Default folder

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PieceMaterialContainer collection = (PieceMaterialContainer)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Auto-Fill Settings", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        if (GUILayout.Button("Load All Materials from Folder"))
        {
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });
            var materials = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(m => m != null)
                .ToArray();

            collection.materials = materials;
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();

            Debug.Log($"Loaded {materials.Length} materials from {folderPath}");
        }

        if (GUILayout.Button("GenerateEnum"))
        {
            GenerateEnum(collection);
        }
    }

    void GenerateEnum(PieceMaterialContainer collection)
    {
        string enumName = collection.name + "Enum";
        string savePath = $"Assets/{enumName}.cs";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Auto-generated enum of materials from MaterialCollection");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public enum {enumName}");
        sb.AppendLine("{");

        foreach (var mat in collection.materials)
        {
            if (mat == null) continue;

            string cleanName = CleanEnumName(mat.name);
            sb.AppendLine($"    {cleanName},");
        }

        sb.AppendLine("}");

        File.WriteAllText(savePath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();

        Debug.Log($"✅ Generated enum '{enumName}' at {savePath}");
    }

    string CleanEnumName(string name)
    {
        // Remove spaces
        string clean = name.Replace(" ", "");

        // Remove invalid characters, keep letters, digits, and underscores
        clean = new string(clean.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

        // If the first char is a digit, prefix with underscore (enums can't start with digit)
        if (clean.Length > 0 && char.IsDigit(clean[0]))
            clean = "_" + clean;

        return clean;
    }
}
#endif
