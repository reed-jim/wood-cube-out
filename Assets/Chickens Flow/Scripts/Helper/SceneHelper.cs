#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

public class SceneHelper : MonoBehaviour
{
    public GameObject[] uiElementsToHide;
    public GameObject bar;
    [ReadOnly] public bool isHidingUI;
    [ReadOnly] public bool isHidingBar;
}
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(SceneHelper))]
public class SceneHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SceneHelper sceneHelper = (SceneHelper)target;

        if (sceneHelper.isHidingUI)
        {
            if (GUILayout.Button("Show UI"))
            {
                ShowUI(sceneHelper);
            }
        }
        else
        {
            if (GUILayout.Button("Hide UI"))
            {
                HideUI(sceneHelper);
            }
        }

        if (sceneHelper.isHidingBar)
        {
            if (GUILayout.Button("Show Bar"))
            {
                ShowBar(sceneHelper, true);
            }
        }
        else
        {
            if (GUILayout.Button("Hide Bar"))
            {
                ShowBar(sceneHelper, false);
            }
        }
    }

    public void ShowUI(SceneHelper sceneHelper)
    {
        for (int i = 0; i < sceneHelper.uiElementsToHide.Length; i++)
        {
            sceneHelper.uiElementsToHide[i].SetActive(true);
        }

        sceneHelper.isHidingUI = false;
    }

    public void HideUI(SceneHelper sceneHelper)
    {
        for (int i = 0; i < sceneHelper.uiElementsToHide.Length; i++)
        {
            sceneHelper.uiElementsToHide[i].SetActive(false);
        }

        sceneHelper.isHidingUI = true;
    }

    public void ShowBar(SceneHelper sceneHelper, bool isShow)
    {
        sceneHelper.bar.SetActive(isShow);

        sceneHelper.isHidingBar = !isShow;
    }
}
#endif
