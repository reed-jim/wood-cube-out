using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AdBreakBox : MonoBehaviour
{
    private static AdBreakBox instance;
    public static AdBreakBox Setup()
    {
        if (instance == null)
        {
            // Create popup and attach it to UI
            instance = Instantiate(Resources.Load<AdBreakBox>(PathPrefabs.AD_BREAK_BOX));
            // Configure popup
        }
        instance.gameObject.SetActive(true);
        return instance;
    }

    private UnityAction actionShowAds;
    private bool isCoolDownShowAds;
    private float timer;

    public void Show(UnityAction actionShowAds)
    {
        instance.gameObject.SetActive(true);
        this.actionShowAds = actionShowAds;

        timer = 0;
        isCoolDownShowAds = true;
    }


    public void Close()
    {
        instance.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(isCoolDownShowAds)
        {
            timer += Time.deltaTime;
            if(timer >= 0.5f)
            {
                isCoolDownShowAds = false;
                Close();

                this.actionShowAds?.Invoke();
            }    
        }    
    }
}
