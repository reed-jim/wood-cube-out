using System;
using System.Collections;
using DG.Tweening;
using Spine.Unity;
//using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public static class SceneLoader
{
    public static string nextScene;
    public static Action onLoaded;
    public static bool showMrec;
}
public class LoadingPanel : MonoBehaviour
{
    private void Awake()
    {
    }

    private void Start()
    {
    }

    public void GotoScene(string scene, bool showMrec, Action action = null)
    {
        SceneLoader.nextScene = scene;
        SceneLoader.showMrec = showMrec;
        SceneLoader.onLoaded = action;
        Initiate.Fade("LoadNewScene", Color.black, 3f);
    }

    
}
