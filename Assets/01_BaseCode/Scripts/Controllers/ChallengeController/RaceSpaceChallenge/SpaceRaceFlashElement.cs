using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SpaceRaceFlashElement : MonoBehaviour
{
    [SerializeField] protected Text nameMemberTxt;
    [SerializeField] protected Text rankTxt;
    [SerializeField] protected Image farmeRankImg;
    [SerializeField] protected RawImage flag;
    [SerializeField] protected Image farmePanelImg;

    protected IDisposable _downloadFlagDispose;

    [SerializeField] protected Sprite[] farmeRankSprs;
    [SerializeField] protected Sprite[] farmePanelSprs;

    public void InitState(int rank, MemberRace memberData)
    {
        if (memberData.isMe)
        {
            nameMemberTxt.text = Localization.Get("s_me");
            farmePanelImg.sprite = farmePanelSprs[0];
        }
        else
        {
            nameMemberTxt.text = memberData.nameMember;
            farmePanelImg.sprite = farmePanelSprs[1];
        }
        SetUpFlag(memberData.linkFlag);
        rankTxt.text = (rank + 1).ToString();
        farmeRankImg.sprite = farmeRankSprs[rank];
    }

    private void SetUpFlag(string linkFlag)
    {
        if (!string.IsNullOrEmpty(linkFlag))
        {
            _downloadFlagDispose = Context.DownloadOrCache(linkFlag, false)
                .CatchIgnore()
                .Subscribe(www => flag.texture = www).AddTo(this);
        }
        else
        {
            flag.gameObject.SetActive(false);
        }
    }
}
