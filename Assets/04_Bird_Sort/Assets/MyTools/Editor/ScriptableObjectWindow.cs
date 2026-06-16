//using UnityEngine;
//using UnityEditor;
//using Object = UnityEngine.Object;
//using System;
//using System.Collections.Generic;

//public class ScriptableObjectWindow : EditorWindow
//{
//    private Editor[] editorArr;
//    [SerializeField]
//    private Object[] settingsArr;
//    private int numObjects = 2;
//    private string soPathsKey = "Assets/01_BirdSort/Data";

//    private static GUIStyle _soContainer;
//    private static GUIStyle soContainer
//    {
//        get
//        {
//            if (_soContainer == null)
//                _soContainer = new GUIStyle(GUI.skin.box);

//            return _soContainer;
//        }
//    }


//    [MenuItem("Tools/Scripstable Object Window")]
//    private static void Init()
//    {
//        Type inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
//        GetWindow<ScriptableObjectWindow>(inspectorType);
//        var window = (ScriptableObjectWindow)GetWindow(typeof(ScriptableObjectWindow));
//        window.Show();
//    }


//    private void OnEnable()
//    {

//        titleContent = new GUIContent("Scriptable Object Window");

//        if (EditorPrefsExist())
//        {
//            LoadObjectsFromEditorPrefs();
//        }
//        else
//        {
//            SetObjectArray();
//        }
//    }

//    private void OnGUI()
//    {
//        if (GUILayout.Button("CHECK NULL"))
//        {
//            CheckNull();
//        }
//        GUILayout.BeginVertical(soContainer);

//        RenderObjectNumberField();
//        RenderObjectFields();

//        GUILayout.EndVertical();

//        for (int i = 0; i < settingsArr.Length; i++)
//        {
//            if (settingsArr[i] != null)
//            {
//                GUILayout.Label(settingsArr[i].name);
//                GUILayout.BeginVertical(soContainer);

//                if (editorArr[i] == null)
//                    editorArr[i] = Editor.CreateEditor(settingsArr[i]);

//                editorArr[i].OnInspectorGUI();
//                GUILayout.EndVertical();
//            }
//        }
//    }

//    private bool EditorPrefsExist()
//    {
//        return EditorPrefs.HasKey(soPathsKey);
//    }

//    private void LoadObjectsFromEditorPrefs()
//    {
//        var soPaths = EditorPrefs.GetString(soPathsKey);
//        string[] paths = soPaths.Split(',');
//        numObjects = paths.Length;

//        settingsArr = new Object[numObjects];
//        editorArr = new Editor[numObjects];

//        for (int i = 0; i < settingsArr.Length; i++)
//        {
//            if (System.IO.File.Exists(paths[i]))
//            {
//                settingsArr[i] = AssetDatabase.LoadAssetAtPath<ScriptableObject>(paths[i]);
//            }

//        }

//    }

//    private void SetEditorPrefs()
//    {
//        string paths = "";
//        for (int i = 0; i < numObjects; i++)
//        {
//            string comma = ",";

//            // Ignore the comma for the last object
//            if (i == numObjects - 1)
//                comma = "";

//            paths += AssetDatabase.GetAssetPath(settingsArr[i]) + comma;
//        }



//        if (!string.IsNullOrEmpty(paths))
//        {
//            EditorPrefs.SetString(soPathsKey, paths);
//            Debug.LogError("Recorded SO paths: " + paths);
//        }
//    }

//    /// <summary>
//    /// Creates arrays to be used in displaying object fields
//    /// </summary>
//    private void SetObjectArray()
//    {
//        settingsArr = new Object[numObjects];
//        editorArr = new Editor[numObjects];

//        for (int i = 0; i < numObjects; i++)
//        {
//            if (settingsArr[i] == null)
//            {
//                settingsArr[i] = new Object();
//                editorArr[i] = null;
//            }

//        }
        
//    }

//    private void RenderObjectNumberField()
//    {
//        EditorGUI.BeginChangeCheck();
//        numObjects = EditorGUILayout.IntField("Number of Objects", numObjects);

//        if (EditorGUI.EndChangeCheck())
//        {
//            SetObjectArray();
//        }
//    }

//    private void RenderObjectFields()
//    {
//        for (int i = 0; i < numObjects; i++)
//        {
//            EditorGUI.BeginChangeCheck();
//            settingsArr[i] = EditorGUILayout.ObjectField("Scriptable Object", settingsArr[i], typeof(Object), false);

//            if (EditorGUI.EndChangeCheck())
//            {
//                SetEditorPrefs();
//            }
//        }
//    }


//    private void CheckNull()
//    {
//        for (int i = 0; i < settingsArr.Length; i++)
//        {
//            if (settingsArr[i] != null)
//            {
//                if (settingsArr[i].GetType() == typeof(GiftDatabase))
//                {
//                    Dictionary<GiftType, Gift> listGift = ((GiftDatabase)settingsArr[i]).giftList;
//                    foreach (KeyValuePair<GiftType, Gift> item in listGift)
//                    {
//                        if (item.Value == null)
//                        {
//                            Debug.Log("<color=yellow>*GiftDatabase*</color> Gift của <color=yellow>" + item.Key + "</color> is null");
//                        }
//                        else
//                        {
//                            if (item.Value.getGiftSprite == null)
//                            {
//                                Debug.Log("<color=yellow>*GiftDatabase*</color> Gift Sprite của <color=yellow>" + item.Key + "</color> là NULL");
//                            }
//                            if (item.Value.getGiftAnim == null)
//                            {
//                                Debug.Log("<color=yellow>*GiftDatabase*</color> Gift Animation của <color=yellow>" + item.Key + "</color> là NULL");
//                            }
//                        }
//                    }
//                }
//                else if (settingsArr[i].GetType() == typeof(BirdSkinDatabase))
//                {
//                    List<BirdSkinData> listBirdSkinData = ((BirdSkinDatabase)settingsArr[i]).listBirdSkinData;
//                    for (int j = 0; j < listBirdSkinData.Count; j++)
//                    {
//                        if (listBirdSkinData[j].birdSkin.idBird == 0 && listBirdSkinData[j].birdSkin.idSkin == 0)
//                        {
//                            Debug.Log("<color=cyan>*BirdSkinDatabase*</color> <color=yellow>Chim thứ " + (j + 1) + "</color> chưa set id skin");
//                        }
//                        if (listBirdSkinData[j].animation == null)
//                        {
//                            Debug.Log("<color=cyan>*BirdSkinDatabase*</color> <color=yellow>Chim thứ " + (j + 1) + "</color> chưa có prefab animation");
//                        }
//                        if (listBirdSkinData[j].icon == null)
//                        {
//                            Debug.Log("<color=cyan>*BirdSkinDatabase*</color> <color=yellow>Chim thứ " + (j + 1) + "</color> chưa có icon");
//                        }
//                    }
//                }
//                else if (settingsArr[i].GetType() == typeof(BranchSkinDatabase))
//                {
//                    List<BranchSkinData> listBranchSkinData = ((BranchSkinDatabase)settingsArr[i]).listBranchSkinData;
//                    for (int j = 0; j < listBranchSkinData.Count; j++)
//                    {
//                        if (listBranchSkinData[j].branch4.icon == null)
//                        {
//                            Debug.Log("<color=orange>*BranchSkinDatabase*</color> <color=yellow>Cành 4 " + listBranchSkinData[j].branchType + "</color> chưa có icon");
//                        }
//                        if (listBranchSkinData[j].branch4.prefab == null)
//                        {
//                            Debug.Log("<color=orange>*BranchSkinDatabase*</color> <color=yellow>Cành 4 " + listBranchSkinData[j].branchType + "</color> chưa có prefab");
//                        }
//                        if (listBranchSkinData[j].branch5.icon == null)
//                        {
//                            Debug.Log("<color=orange>*BranchSkinDatabase*</color> <color=yellow>Cành 5 " + listBranchSkinData[j].branchType + "</color> chưa có icon");
//                        }
//                        if (listBranchSkinData[j].branch5.prefab == null)
//                        {
//                            Debug.Log("<color=orange>*BranchSkinDatabase*</color> <color=yellow>Cành 5 " + listBranchSkinData[j].branchType + "</color> chưa có prefab");
//                        }
//                        if (listBranchSkinData[j].branch6.icon == null)
//                        {
//                            Debug.Log("<color=orange>*BranchSkinDatabase*</color> <color=yellow>Cành 6 " + listBranchSkinData[j].branchType + "</color> chưa có icon");
//                        }
//                        if (listBranchSkinData[j].branch6.prefab == null)
//                        {
//                            Debug.Log("<color=orange>*BranchSkinDatabase*</color> <color=yellow>Cành 6 " + listBranchSkinData[j].branchType + "</color> chưa có prefab");
//                        }
//                    }

//                }
//                else if (settingsArr[i].GetType() == typeof(ThemeSkinDatabase))
//                {
//                    List<ThemeSkinData> listThemeData = ((ThemeSkinDatabase)settingsArr[i]).listThemeSkinData;
//                    for (int j = 0; j < listThemeData.Count; j++)
//                    {
//                        if (listThemeData[j].prefab == null)
//                        {
//                            Debug.Log("<color=#ff6347>*ThemeSkinDatabase*</color> <color=yellow>Theme " + listThemeData[j].themeType + "</color> chưa có prefab");
//                        }
//                        if (listThemeData[j].icon == null)
//                        {
//                            Debug.Log("<color=#ff6347>*ThemeSkinDatabase*</color> <color=yellow>Theme " + listThemeData[j].themeType + "</color> chưa có icon");
//                        }
//                    }
//                }
//                else if (settingsArr[i].GetType() == typeof(SalePackDatabase))
//                {
//                    List<SalePackDataModel> listPack = ((SalePackDatabase)settingsArr[i]).packs;
//                    for (int j = 0; j < listPack.Count; j++)
//                    {
//                        if (listPack[j].icon == null)
//                        {
//                            Debug.Log("<color=#90EE90>*SalePackDatabase*</color> <color=yellow>Gói " + listPack[j].typePackIAP + "</color> chưa có icon");
//                        }
//                        if (listPack[j].shopBanner == null)
//                        {
//                            Debug.Log("<color=#90EE90>*SalePackDatabase*</color> <color=yellow>Gói " + listPack[j].typePackIAP + "</color> chưa có prefab banner trong shop");
//                        }
//                    }
//                }
//                else if (settingsArr[i].GetType() == typeof(IAPDatabase))
//                {

//                }
//            }
//        }
//    }
//}