using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UniRx;

public class IQPlayerBadgeData
{
    public string playerName;
    public string linkAvatar;
    public string linkFlag;
    public int score;
}
public class IQScorePlayerBadge : MonoBehaviour
{
    protected IDisposable _downloadAvatarDispose;
    protected IDisposable _downloadFlagDispose;

    public Text playerName;
    public RawImage ava;
    public RawImage flag;
    public Text iqScore;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Scale(float t)
    {
        this.transform.localScale = new Vector3(t, t, t);
    }
    public void SetUI(IQPlayerBadgeData data)
    {
        playerName.text = data.playerName;
        SetUpAvatar(data.linkAvatar);
        SetUpFlag(data.linkFlag);
        iqScore.text = "IQ: " + data.score.ToString();
    }
    public void SetFakeUI()
    {
        var a = (Texture)Resources.Load("UI/Sprite/Avatars/Avatar0", typeof(Texture));
        if (a != null)
        {
            ava.texture = a;
        }
        SetUpFlag(GameController.Instance.playerData.flagLink);
        iqScore.text = "IQ: 0";
    }
    public void SetCurrPlayer()
    {
        iqScore.text = "IQ: "+ UseProfile.IQScore.ToString();
        SetUpAvatar(GameController.Instance.playerData.linkAvatar);
        SetUpFlag(GameController.Instance.playerData.flagLink);
    }
    private Tween tween1;
    private int tempPercent;
    public void DOAnimScore(object value, Action onComplete = null)
    {
        tween1.Kill(true);
        tween1 = DOTween.To(() => tempPercent, x => tempPercent = x, (int)value, 1f).OnUpdate(delegate
        {
            iqScore.text = "IQ: " + $"{tempPercent}";
        }).OnComplete(delegate
        {
            tempPercent = (int)value;
            onComplete?.Invoke();
        });
    }


    public void SetUpAvatar(string linkAvatar)
    {
        var a = (Texture)Resources.Load("UI/Sprite/Avatars/Avatar0", typeof(Texture));
        if(a != null)
        {
            ava.texture = a;
        }
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
