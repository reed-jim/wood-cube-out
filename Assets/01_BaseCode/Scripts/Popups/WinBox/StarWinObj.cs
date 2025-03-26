using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class StarWinObj : MonoBehaviour
{
    [SerializeField] private Image star;
    [SerializeField] private GameObject starFX;
    [SerializeField] private Image disable;
    [SerializeField] protected AudioClip appearClip;
    [SerializeField] protected AudioSource appearSource;

    public void Init()
    {
        star.transform.DOKill();
        starFX.gameObject.SetActive(false);
   
        star.color = new Color32(255, 255, 255, 0);
       
        if (Helper.Rateofscreen > 2)
        {
            disable.rectTransform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void ShowStar(UnityAction showDone = null)
    {
        //star(1, 0.25f);
        starFX.gameObject.SetActive(true);
        //star.color = new Color32(255, 255, 255, 255);
        
      
        star.transform.localScale = new Vector3(1.1f, 1.1f, 1);
        star.transform.DOScale(1, 0.5f).OnComplete(()=> 
        {
           
            showDone?.Invoke(); 
        });
        GameController.Instance.musicManager.PlayOneShot(appearClip, appearSource, 0.3f);
    }
}
