using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
public class TutorialConnectBirdBox : BaseBox
{
    private static TutorialConnectBirdBox instance;
    public static TutorialConnectBirdBox Setup(bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<TutorialConnectBirdBox>("UI/Popups/TutorialConnectBirdBox"));
            instance.Init();
        }
        instance.InitState();


        return instance;
    }
    public Button btnClose;
    public GameObject aura1;
    public GameObject aura2;
    public GameObject aura3;
    public List<GameObject> lsTut1;
   
    public List<GameObject> lsTut2;
    public List<GameObject> lsTut3;
    public GameObject changeTut1;
    public GameObject changeTut2;
    private bool limitClickClose;

    [SerializeField] private AudioClip appearClip;

    public void Init()
    {
        btnClose.onClick.AddListener(Close);
    }
    public void InitState()
    {
        SetUp();
        limitClickClose = false;
    }



    private void Update()
    {
        //aura1.transform.localEulerAngles += new Vector3(0, 0, 30) * Time.deltaTime;
        //aura2.transform.localEulerAngles += new Vector3(0, 0, 30) * Time.deltaTime;
        //aura3.transform.localEulerAngles += new Vector3(0, 0, 30) * Time.deltaTime;

    }
    private void SetUp()
    {
        foreach (GameObject item in lsTut1)
        {
            //item.transform.localScale = new Vector3(0, 0, 0);
            item.gameObject.SetActive(false);

        }
        foreach (GameObject item in lsTut2)
        {
            item.transform.localScale = new Vector3(0, 0, 0);
            item.gameObject.SetActive(false);

        }
        foreach (GameObject item in lsTut3)
        {
            //item.transform.localScale = new Vector3(0, 0, 0);
            item.gameObject.SetActive(false);
        }
        changeTut1.transform.localScale = new Vector3(0, 0, 0);
        changeTut2.transform.localScale = new Vector3(0, 0, 0);
        Invoke("ZoomLsTut1",0.25f);
    }    
    private void ZoomLsTut1()
    {
        //foreach(GameObject item in lsTut1)
        //{
        //    
        //}
        for(int i = 0; i < lsTut1.Count; i ++)
        {
            int index = i;
            Vector3 originalScale = lsTut1[index].transform.localScale;
            lsTut1[index].transform.localScale = new Vector3(0, 0, 0);
            lsTut1[index].gameObject.SetActive(true);
            if (i != lsTut1.Count -1)
            {
                lsTut1[index].transform.DOScale(originalScale, 0.25f);

                //if (i == 0)
                //{
                //    lsTut1[i].transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.25f);
                //}
                //else
                //{
                //    lsTut1[i].transform.DOScale(new Vector3(1, 1, 1), 0.25f);
                //}
         
            }
            else
            {
                lsTut1[index].transform.DOScale(originalScale, 0.25f).OnComplete(delegate 
                { 
                    changeTut1.transform.DOScale(new Vector3(1, 1, 1), 0.25f).OnComplete(delegate { ZoomLsTut2(); }); 
                });
            }
        
        }
        //Anhtusz GameController.Instance.musicManager.PlayOneShot(appearClip, MusicManager.SourceAudio.Effect);
    }
    private void ZoomLsTut2()
    {
        for (int i = 0; i < lsTut2.Count; i++)
        {
            int index = i;
            lsTut2[index].gameObject.SetActive(true);
            if (i != lsTut2.Count - 1)
            {
                if (i == 0)
                {
                    lsTut2[i].transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.25f);
                }
                else
                {
                    lsTut2[i].transform.DOScale(new Vector3(1, 1, 1), 0.25f);
                }
            }
            else
            {
                lsTut2[i].transform.DOScale(new Vector3(1, 1, 1), 0.25f).OnComplete(delegate { changeTut2.transform.DOScale(new Vector3(1, 1, 1), 0.25f).OnComplete(delegate { ZoomLsTut3(); }); });
            }

        }
        //Anhtusz GameController.Instance.musicManager.PlayOneShot(appearClip, MusicManager.SourceAudio.Effect);
    }
    private void ZoomLsTut3()
    {
        for (int i = 0; i < lsTut3.Count; i++)
        {
            int index = i;
            Vector3 originalScale = lsTut3[index].transform.localScale;
            lsTut3[index].transform.localScale = new Vector3(0, 0, 0);
            lsTut3[index].gameObject.SetActive(true);
            lsTut3[index].transform.DOScale(originalScale, 0.25f);
            //if (i == 0)
            //{
            //    lsTut3[index].transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.25f);
            //}
            //else
            //{
            //    lsTut3[i].transform.DOScale(new Vector3(1, 1, 1), 0.25f);
            //}
        }
        //Anhtusz GameController.Instance.musicManager.PlayOneShot(appearClip, MusicManager.SourceAudio.Effect);
    }

    public override void Close()
    {
        if(limitClickClose == false)
        {
            base.Close();
            limitClickClose = true;
            GameController.Instance.musicManager.PlayClickSound();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (GameObject item in lsTut1)
        {
            item.transform.DOKill();
        }
        foreach (GameObject item in lsTut2)
        {
            item.transform.DOKill();
        }
        foreach (GameObject item in lsTut3)
        {
            item.transform.DOKill();
        }
        changeTut1.transform.DOKill();
        changeTut2.transform.DOKill();
        CancelInvoke();

    }

}
