using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;
public class IQTableSubItem : MonoBehaviour
{
    public Text numText;
    public RawImage ava;
    public RawImage flag;
    public Text nameText;
    public Text iqNumText;

    protected IDisposable _downloadAvatarDispose;
    protected IDisposable _downloadFlagDispose;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void InitState(RankingItemDataIQ data)
    {
        this.gameObject.SetActive(true);
        numText.text = (data.rank + 1) > 100? "100+" : (data.rank + 1).ToString();
        nameText.text = data.name;
        iqNumText.text = data.score.ToString();
        SetUpAvatar(data.linkAvatar);
        SetUpFlag(data.linkFlag);
    }

    public void SetUpAvatar(string linkAvatar)
    {
        var a = (Texture)Resources.Load("UI/Sprite/Avatars/Avatar0", typeof(Texture));
        ava.texture = a;

        if (!string.IsNullOrEmpty(linkAvatar))
        {
            _downloadAvatarDispose = Context.DownloadOrCache(linkAvatar, true)
                              .CatchIgnore()
                              .Subscribe(www => ava.texture = www);
        }
    }

    public void SetUpFlag(string linkFlag)
    {
        if (!string.IsNullOrEmpty(linkFlag))
        {
            _downloadFlagDispose = Context.DownloadOrCache(linkFlag, false)
                              .CatchIgnore()
                              .Subscribe(www => flag.texture = www);
        }
        else
        {
            flag.gameObject.SetActive(false);
        }
    }
}
