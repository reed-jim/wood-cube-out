#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Globalization;

public class HexColorImporter : EditorWindow
{
    private string input;
    private PieceMaterialContainer _pieceMaterialContainer;

    [MenuItem("Tools/Hex Color Importer")]
    public static void ShowWindow()
    {
        GetWindow<HexColorImporter>("Hex Color Importer");
    }

    void OnGUI()
    {
        _pieceMaterialContainer = (PieceMaterialContainer)EditorGUILayout.ObjectField("pieceMaterialContainer",
            _pieceMaterialContainer, typeof(PieceMaterialContainer), true);

        GUILayout.Label("Paste Hex Colors (Name = #HEX)", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Format:\nYarn Blue = #0057FF\nYarn Brown = #7A3E1D\n\nOne per line.",
            MessageType.Info
        );

        input = EditorGUILayout.TextArea(input, GUILayout.Height(200));

        if (GUILayout.Button("Import Colors"))
        {
            Import();
        }
    }

    void Import()
    {
        var lines = input.Split('\n');

        int index = 0;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split('=');
            if (parts.Length != 2) continue;

            string name = parts[0].Trim();
            string hex = parts[1].Trim();

            if (!hex.StartsWith("#")) continue;

            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                _pieceMaterialContainer.materials[index].SetColor("_Color", color);

                index++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif
