using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class TextureAtlasCreatorWindow : EditorWindow
{
    private List<Texture2D> textures = new();

    private string outputPath = "Assets/texture_atlas.png";


    [MenuItem("Tools/Texture Atlas Creator")]
    static void Open()
    {
        GetWindow<TextureAtlasCreatorWindow>(
            "Texture Atlas Creator");
    }


    void OnGUI()
    {
        GUILayout.Label(
            "Texture Atlas Creator",
            EditorStyles.boldLabel);


        GUILayout.Space(10);


        DrawDropArea();


        GUILayout.Space(10);


        EditorGUILayout.LabelField(
            $"Frames: {textures.Count}");


        GUILayout.Space(10);


        outputPath =
            EditorGUILayout.TextField(
                "Output",
                outputPath);


        GUILayout.Space(10);


        GUI.enabled = textures.Count > 0;

        if (GUILayout.Button("Create Atlas"))
        {
            CreateAtlas();
        }

        GUI.enabled = true;
    }


    void DrawDropArea()
    {
        Rect dropArea =
            GUILayoutUtility.GetRect(
                0,
                100,
                GUILayout.ExpandWidth(true));


        GUI.Box(
            dropArea,
            "Drag PNG files here");


        Event evt = Event.current;


        if (!dropArea.Contains(evt.mousePosition))
            return;


        if (evt.type == EventType.DragUpdated ||
            evt.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode =
                DragAndDropVisualMode.Copy;


            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();


                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    Texture2D tex =
                        obj as Texture2D;


                    if (tex != null &&
                       !textures.Contains(tex))
                    {
                        textures.Add(tex);
                    }
                }


                textures =
                    textures
                    .OrderBy(x => x.name)
                    .ToList();


                Repaint();
            }


            evt.Use();
        }
    }


    void CreateAtlas()
    {
        if (textures.Count == 0)
            return;


        int count = textures.Count;

        int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt((float)count / columns);


        int width = textures[0].width;
        int height = textures[0].height;


        Texture2D atlas = new Texture2D(
            width * columns,
            height * rows,
            TextureFormat.RGBA32,
            false);


        for (int i = 0; i < count; i++)
        {
            Texture2D tex = textures[i];

            // Make sure texture is readable
            string path = AssetDatabase.GetAssetPath(tex);

            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }


            int x = (i % columns) * width;
            int y = (rows - 1 - i / columns) * height;


            atlas.SetPixels(
                x,
                y,
                width,
                height,
                tex.GetPixels());
        }


        atlas.Apply();


        File.WriteAllBytes(
            outputPath,
            atlas.EncodeToPNG());


        AssetDatabase.Refresh();


        Debug.Log(
            $"Created atlas: {outputPath}\n" +
            $"Frames: {count}\n" +
            $"Grid: {columns}x{rows}");
    }
}
