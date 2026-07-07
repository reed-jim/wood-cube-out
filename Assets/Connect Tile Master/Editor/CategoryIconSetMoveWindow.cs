using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool để di chuyển (move) 1 phần tử trong mảng "categoryIconSets" của
/// TileIconContainer đến vị trí nằm giữa 2 phần tử khác, mà KHÔNG làm thay đổi
/// dữ liệu đã fill (itemCategory, icons, text, textOrder, spriteOrder,
/// iconCdnKeys, spriteOrderCdnKey, isOnServer) của các phần tử còn lại — các
/// phần tử khác chỉ bị dịch chỉ số (index), dữ liệu bên trong giữ nguyên.
///
/// Cách dùng:
/// 1. Trong Unity: Tools > Tile Icon Container > Move Category Icon Set
/// 2. Kéo file TileIconContainer.asset vào ô "Tile Icon Container Asset"
/// 3. Nhập Index N (phần tử muốn di chuyển)
/// 4. Nhập Index M (phần tử sẽ đứng ngay TRƯỚC vị trí mới của N,
///    tức N sẽ nằm giữa M và M+1 sau khi di chuyển)
/// 5. Bấm "Move Element"
/// </summary>
public class CategoryIconSetMoveWindow : EditorWindow
{
    private Object targetAsset;
    private int indexN;
    private int indexM = 1;

    // Danh sách các field được backup/restore cho mỗi element của categoryIconSets
    private static readonly string[] FieldsToBackup =
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

    // Backup gần nhất của TOÀN BỘ mảng categoryIconSets, chụp ngay TRƯỚC lần move gần nhất
    private List<Dictionary<string, object>> lastBackup;
    private Object lastBackupAsset;

    [MenuItem("Tools/Tile Icon Container/Move Category Icon Set")]
    public static void ShowWindow()
    {
        var window = GetWindow<CategoryIconSetMoveWindow>("Move Category Icon Set");
        window.minSize = new Vector2(420, 240);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Move 1 Category Icon Set vào giữa 2 phần tử khác", EditorStyles.boldLabel);
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

        indexN = EditorGUILayout.IntField("Index N (phần tử cần di chuyển)", indexN);
        indexM = EditorGUILayout.IntField("Index M (sẽ chèn N vào giữa M và M+1)", indexM);

        bool validRange = indexN >= 0 && indexN < count && indexM >= 0 && indexM < count;

        if (!validRange)
        {
            EditorGUILayout.HelpBox($"Index phải trong khoảng 0 - {count - 1}.", MessageType.Warning);
            return;
        }

        int dstIndex = ComputeDestinationIndex(indexN, indexM);
        bool isNoOp = dstIndex == indexN;

        SerializedProperty elemN = listProp.GetArrayElementAtIndex(indexN);
        SerializedProperty catN = elemN.FindPropertyRelative("itemCategory");
        string catNInfo = catN != null ? $" (itemCategory={catN.intValue})" : "";

        if (isNoOp)
        {
            EditorGUILayout.HelpBox(
                $"[{indexN}]{catNInfo} đã ở đúng vị trí giữa [{indexM}] và [{indexM + 1}], không cần di chuyển.",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                $"Sẽ di chuyển [{indexN}]{catNInfo} đến giữa [{indexM}] và [{indexM + 1}]\n" +
                $"(vị trí cuối cùng trong mảng: index {dstIndex}).\n" +
                "Các phần tử khác giữ nguyên dữ liệu, chỉ dịch index.",
                MessageType.None);
        }

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(isNoOp))
        {
            if (GUILayout.Button("Move Element", GUILayout.Height(32)))
            {
                // Chụp lại toàn bộ dữ liệu hiện tại TRƯỚC khi move, để có thể Back lại
                lastBackup = CaptureArray(listProp);
                lastBackupAsset = targetAsset;

                Undo.RecordObject(targetAsset, "Move Category Icon Set");
                listProp.MoveArrayElement(indexN, dstIndex);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(targetAsset);
                AssetDatabase.SaveAssets();
                Debug.Log($"Đã di chuyển categoryIconSets[{indexN}] đến vị trí giữa [{indexM}] và [{indexM + 1}] (index cuối: {dstIndex})");
            }
        }

        EditorGUILayout.Space();

        bool hasBackup = lastBackup != null && lastBackupAsset == targetAsset;

        using (new EditorGUI.DisabledScope(!hasBackup))
        {
            if (GUILayout.Button("⟲ Back (khôi phục dữ liệu trước lần di chuyển gần nhất)", GUILayout.Height(28)))
            {
                Undo.RecordObject(targetAsset, "Undo Move Category Icon Set");
                RestoreArray(listProp, lastBackup);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(targetAsset);
                AssetDatabase.SaveAssets();

                // Xoá backup sau khi đã restore, vì đây chỉ là 1 bước lùi gần nhất
                lastBackup = null;
                lastBackupAsset = null;

                Debug.Log("Đã khôi phục categoryIconSets về trạng thái trước lần di chuyển gần nhất.");
            }
        }

        if (!hasBackup)
        {
            EditorGUILayout.HelpBox("Chưa có thao tác Move nào để khôi phục.", MessageType.Info);
        }
    }

    // ---------------- Backup / Restore helpers ----------------

    private static List<Dictionary<string, object>> CaptureArray(SerializedProperty listProp)
    {
        var snapshot = new List<Dictionary<string, object>>(listProp.arraySize);

        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);
            var fieldValues = new Dictionary<string, object>();

            foreach (string fieldName in FieldsToBackup)
            {
                SerializedProperty prop = element.FindPropertyRelative(fieldName);
                if (prop == null) continue;
                fieldValues[fieldName] = ExtractValue(prop);
            }

            snapshot.Add(fieldValues);
        }

        return snapshot;
    }

    private static void RestoreArray(SerializedProperty listProp, List<Dictionary<string, object>> snapshot)
    {
        listProp.arraySize = snapshot.Count;

        for (int i = 0; i < snapshot.Count; i++)
        {
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);

            foreach (var kvp in snapshot[i])
            {
                SerializedProperty prop = element.FindPropertyRelative(kvp.Key);
                if (prop == null) continue;
                ApplyValue(prop, kvp.Value);
            }
        }
    }

    private static object ExtractValue(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer: return prop.intValue;
            case SerializedPropertyType.Boolean: return prop.boolValue;
            case SerializedPropertyType.Float: return prop.floatValue;
            case SerializedPropertyType.String: return prop.stringValue;
            case SerializedPropertyType.ObjectReference: return prop.objectReferenceValue;
            case SerializedPropertyType.Enum: return prop.enumValueIndex;
            case SerializedPropertyType.Generic:
                if (prop.isArray)
                {
                    var list = new List<object>(prop.arraySize);
                    for (int i = 0; i < prop.arraySize; i++)
                        list.Add(ExtractValue(prop.GetArrayElementAtIndex(i)));
                    return list;
                }
                return null;
            default:
                return null;
        }
    }

    private static void ApplyValue(SerializedProperty prop, object value)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                prop.intValue = value is int i ? i : 0;
                break;
            case SerializedPropertyType.Boolean:
                prop.boolValue = value is bool b && b;
                break;
            case SerializedPropertyType.Float:
                prop.floatValue = value is float f ? f : 0f;
                break;
            case SerializedPropertyType.String:
                prop.stringValue = value as string ?? string.Empty;
                break;
            case SerializedPropertyType.ObjectReference:
                prop.objectReferenceValue = value as Object;
                break;
            case SerializedPropertyType.Enum:
                prop.enumValueIndex = value is int e ? e : 0;
                break;
            case SerializedPropertyType.Generic:
                if (prop.isArray && value is List<object> list)
                {
                    prop.arraySize = list.Count;
                    for (int idx = 0; idx < list.Count; idx++)
                        ApplyValue(prop.GetArrayElementAtIndex(idx), list[idx]);
                }
                break;
        }
    }

    /// <summary>
    /// Tính index đích cuối cùng trong mảng để phần tử N nằm ngay sau phần tử M
    /// (tức giữa M và M+1), dựa trên vị trí tương đối của N so với M.
    /// </summary>
    private static int ComputeDestinationIndex(int n, int m)
    {
        // Nếu N đang đứng trước M: sau khi rút N ra, mọi phần tử sau N (bao gồm M)
        // dịch lên 1, nên M mới = M - 1 => vị trí chèn (ngay sau M) = (M - 1) + 1 = M.
        // Nếu N đang đứng sau M: M không bị ảnh hưởng khi rút N ra,
        // nên vị trí chèn (ngay sau M) = M + 1.
        return n <= m ? m : m + 1;
    }
}