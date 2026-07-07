using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool để swap (đổi chỗ) 2 phần tử trong mảng "categoryIconSets" của
/// TileIconContainer, bao gồm toàn bộ dữ liệu đã fill:
/// itemCategory, icons, text, textOrder, spriteOrder, iconCdnKeys, spriteOrderCdnKey, isOnServer.
///
/// Cách dùng:
/// 1. Trong Unity: Tools > Tile Icon Container > Swap Category Icon Sets
/// 2. Kéo file TileIconContainer.asset vào ô "Tile Icon Container Asset"
/// 3. Nhập Index A và Index B
/// 4. Bấm "Swap 2 Elements"
/// </summary>
public class CategoryIconSetSwapWindow : EditorWindow
{
    private Object targetAsset;
    private int indexA;
    private int indexB = 1;

    // Danh sách các field sẽ được swap trong mỗi element của categoryIconSets
    private static readonly string[] FieldsToSwap =
    {
        "itemCategory",
        "icons",
        "text",
        "textOrder",
        "spriteOrder",
        "iconCdnKeys",
        "spriteOrderCdnKey",
        "isOnServer"
    };

    [MenuItem("Tools/Tile Icon Container/Swap Category Icon Sets")]
    public static void ShowWindow()
    {
        var window = GetWindow<CategoryIconSetSwapWindow>("Swap Category Icon Sets");
        window.minSize = new Vector2(380, 220);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Swap 2 Category Icon Sets", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetAsset = EditorGUILayout.ObjectField(
            "Tile Icon Container Asset", targetAsset, typeof(Object), false);

        EditorGUILayout.Space();

        if (targetAsset == null)
        {
            EditorGUILayout.HelpBox("Kéo file TileIconContainer.asset vào ô trên.", MessageType.Info);
            return;
        }

        var so = new SerializedObject(targetAsset);
        SerializedProperty listProp = so.FindProperty("categoryIconSets");

        if (listProp == null || !listProp.isArray)
        {
            EditorGUILayout.HelpBox(
                "Không tìm thấy field 'categoryIconSets' trên asset này.", MessageType.Error);
            return;
        }

        int count = listProp.arraySize;
        EditorGUILayout.LabelField($"Số lượng phần tử: {count}");
        EditorGUILayout.Space();

        indexA = EditorGUILayout.IntField("Index A", indexA);
        indexB = EditorGUILayout.IntField("Index B", indexB);

        bool validRange = indexA >= 0 && indexA < count && indexB >= 0 && indexB < count;

        if (!validRange)
        {
            EditorGUILayout.HelpBox($"Index phải trong khoảng 0 - {count - 1}.", MessageType.Warning);
        }
        else
        {
            var catA = listProp.GetArrayElementAtIndex(indexA).FindPropertyRelative("itemCategory");
            var catB = listProp.GetArrayElementAtIndex(indexB).FindPropertyRelative("itemCategory");
            if (catA != null && catB != null)
            {
                EditorGUILayout.HelpBox(
                    $"Sẽ swap: [{indexA}] (itemCategory={catA.intValue})  <->  [{indexB}] (itemCategory={catB.intValue})",
                    MessageType.None);
            }
        }

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(!validRange || indexA == indexB))
        {
            if (GUILayout.Button("Swap 2 Elements", GUILayout.Height(32)))
            {
                Undo.RecordObject(targetAsset, "Swap Category Icon Sets");
                SwapElements(listProp, indexA, indexB);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(targetAsset);
                AssetDatabase.SaveAssets();
                Debug.Log($"Đã swap categoryIconSets[{indexA}] <-> categoryIconSets[{indexB}]");
            }
        }
    }

    private static void SwapElements(SerializedProperty listProp, int a, int b)
    {
        SerializedProperty elementA = listProp.GetArrayElementAtIndex(a);
        SerializedProperty elementB = listProp.GetArrayElementAtIndex(b);

        foreach (string fieldName in FieldsToSwap)
        {
            SerializedProperty propA = elementA.FindPropertyRelative(fieldName);
            SerializedProperty propB = elementB.FindPropertyRelative(fieldName);

            if (propA == null || propB == null)
            {
                Debug.LogWarning($"Không tìm thấy field '{fieldName}', bỏ qua field này.");
                continue;
            }

            SwapProperty(propA, propB);
        }
    }

    private static void SwapProperty(SerializedProperty a, SerializedProperty b)
    {
        switch (a.propertyType)
        {
            case SerializedPropertyType.Integer:
                (a.intValue, b.intValue) = (b.intValue, a.intValue);
                break;

            case SerializedPropertyType.Boolean:
                (a.boolValue, b.boolValue) = (b.boolValue, a.boolValue);
                break;

            case SerializedPropertyType.Float:
                (a.floatValue, b.floatValue) = (b.floatValue, a.floatValue);
                break;

            case SerializedPropertyType.String:
                (a.stringValue, b.stringValue) = (b.stringValue, a.stringValue);
                break;

            case SerializedPropertyType.ObjectReference:
                (a.objectReferenceValue, b.objectReferenceValue) = (b.objectReferenceValue, a.objectReferenceValue);
                break;

            case SerializedPropertyType.Enum:
                (a.enumValueIndex, b.enumValueIndex) = (b.enumValueIndex, a.enumValueIndex);
                break;

            case SerializedPropertyType.Generic:
                // Trường hợp List<>/Array (icons, text, iconCdnKeys...)
                if (a.isArray && b.isArray)
                {
                    SwapArrayProperty(a, b);
                }
                else
                {
                    Debug.LogWarning($"Field '{a.name}' kiểu Generic không phải array, bỏ qua.");
                }
                break;

            default:
                Debug.LogWarning($"Kiểu '{a.propertyType}' của field '{a.name}' chưa được hỗ trợ swap.");
                break;
        }
    }

    private static void SwapArrayProperty(SerializedProperty a, SerializedProperty b)
    {
        List<object> valuesA = ExtractArrayValues(a);
        List<object> valuesB = ExtractArrayValues(b);

        ApplyArrayValues(a, valuesB);
        ApplyArrayValues(b, valuesA);
    }

    private static List<object> ExtractArrayValues(SerializedProperty arrayProp)
    {
        var result = new List<object>(arrayProp.arraySize);
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            SerializedProperty elem = arrayProp.GetArrayElementAtIndex(i);
            switch (elem.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    result.Add(elem.objectReferenceValue);
                    break;
                case SerializedPropertyType.String:
                    result.Add(elem.stringValue);
                    break;
                case SerializedPropertyType.Integer:
                    result.Add(elem.intValue);
                    break;
                case SerializedPropertyType.Boolean:
                    result.Add(elem.boolValue);
                    break;
                case SerializedPropertyType.Float:
                    result.Add(elem.floatValue);
                    break;
                default:
                    result.Add(null);
                    Debug.LogWarning($"Kiểu phần tử '{elem.propertyType}' trong array '{arrayProp.name}' chưa được hỗ trợ đầy đủ.");
                    break;
            }
        }
        return result;
    }

    private static void ApplyArrayValues(SerializedProperty arrayProp, List<object> values)
    {
        arrayProp.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++)
        {
            SerializedProperty elem = arrayProp.GetArrayElementAtIndex(i);
            object val = values[i];
            switch (elem.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    elem.objectReferenceValue = val as Object;
                    break;
                case SerializedPropertyType.String:
                    elem.stringValue = val as string ?? string.Empty;
                    break;
                case SerializedPropertyType.Integer:
                    elem.intValue = val is int intVal ? intVal : 0;
                    break;
                case SerializedPropertyType.Boolean:
                    elem.boolValue = val is bool boolVal && boolVal;
                    break;
                case SerializedPropertyType.Float:
                    elem.floatValue = val is float floatVal ? floatVal : 0f;
                    break;
            }
        }
    }
}