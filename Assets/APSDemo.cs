using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class APSDemo : MonoBehaviour
{
    public void LoadAdMobScene()
    {
        SceneManager.LoadScene(2);
    }

    public void LoadMaxScene()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadIronSourceScene()
    {
        SceneManager.LoadScene(3);
    }
}
