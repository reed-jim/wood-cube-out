using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LA.AssetBundle
{
    
    public class AssetBundleEditor: EditorWindow
    {
        //field
        private string _selectedBundle="ExportBundle";
        private List<(string,bool)> _spritePathList = new List<(string, bool)>();
        private List<string> _guidList = new List<string>();
        private string _trimPath="";
        private string _searchPath = "";
        private Vector2 _scrollPos;
        private Action onDone;
        [MenuItem("LA/Asset Bundle/Open Window")] 
        private static void Init()
        {
            var box = GetWindow<AssetBundleEditor>(false,"Asset Bundle Tool");

        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 100;
            EditorGUILayout.LabelField("Asset Bundle Tool");
            ShowGUI();
        }

        private void ShowGUI()
        {
            _selectedBundle = EditorGUILayout.TextField("Select Bundle:", _selectedBundle).ToLower();
            if (GUILayout.Button("Generate Field") && _selectedBundle!="")
            {
                GenerateSpriteList();
                Debug.Log(_spritePathList.Count);
            }
            ShowListGUI();
            if (GUILayout.Button("Create Asset Bundle"))
            {
                _guidList.Clear();
                foreach (var sTmp in _spritePathList)
                {
                    Debug.Log("-----");
                    string path = sTmp.Item1;
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    _guidList.Add(guid);
                    Debug.Log(path);
                    string oldPath = path;
                    int last = path.LastIndexOf('/');
                    string basePath = path.Substring(0, last+1);
                    string fName = path.Substring(last+1,path.Length-last-5);
                    string extend = path.Substring(path.LastIndexOf(".")+1);
                    string newPath = basePath + guid + "." + extend;
                    Debug.Log(basePath);
                    Debug.Log(fName);
                    Debug.Log(extend);
                    Debug.Log(newPath);
                    AssetDatabase.RenameAsset(oldPath, guid);
                    AssetDatabase.SaveAssets();
                    var importer = AssetImporter.GetAtPath(newPath);
                    importer.assetBundleName = guid;
                    onDone += () =>
                    {
                        AssetDatabase.RenameAsset(newPath, fName);
                        AssetDatabase.SaveAssets();
                    };
                }

                AssetBundleTool.BuildAllAssetBundle();
                onDone.Invoke();
            }

            if (GUILayout.Button("Clear All Asset Bundle"))
            {
                var aa = AssetDatabase.GetAllAssetBundleNames();
                foreach (var VARIABLE in aa)
                {
                    AssetDatabase.RemoveAssetBundleName(VARIABLE, true);
                }
            }
        }

        private void ShowListGUI()
        {
            if (_spritePathList.Count==0)
            {
                return;
            }
            _searchPath = EditorGUILayout.TextField("Search Path:", _searchPath);
            _trimPath = EditorGUILayout.TextField("Trim Path:", _trimPath);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUILayout.TextField("Eg: ", _spritePathList[0].Item1);
            /*EditorGUIUtility.labelWidth = position.width-50;
            EditorGUIUtility.fieldWidth = 30;*/
            for(int i=0;i<_spritePathList.Count;i++)
            {
                var pTmp = _spritePathList[i];
                string text = "";
                if (_searchPath=="" || pTmp.Item1.Contains(_searchPath))
                {
                    if (_trimPath!="")
                    {
                        text = pTmp.Item1.Replace(_trimPath,"");
                    }
                    else
                    {
                        text = pTmp.Item1;

                    }
                }

                pTmp.Item2 = EditorGUILayout.Toggle(text,pTmp.Item2,new GUIStyle(GUI.skin.toggle));
                _spritePathList[i] = pTmp;
            }
            EditorGUILayout.EndScrollView();
        }

        private void GenerateSpriteList()
        {
            _spritePathList.Clear();
            var array = AssetDatabase.GetAssetPathsFromAssetBundle(_selectedBundle);
            foreach (var sTmp in array)
            {
                _spritePathList.Add(new ValueTuple<string,bool>(sTmp,true));
            }
            _trimPath = "";
            _searchPath = "";
        }

    }
}
