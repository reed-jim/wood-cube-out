using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UMPManager : MonoBehaviour
{
    void Start()
    {
        
    }

    private void HandleConsentComplete()
    {
        SceneManager.LoadScene("Loading");
    }
}
