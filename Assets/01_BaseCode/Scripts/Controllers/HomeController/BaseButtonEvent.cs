using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


[System.Serializable]
public class BaseDataButtonEvent
{
    public Sprite icon;
    public Vector3 scaleIcon;
    public long timeLife;
    public string titleIconShow;
}

public class BaseButtonEvent : MonoBehaviour
{
    [SerializeField] protected Image iconImg;
    public Text timeTxt;
    [SerializeField] protected Button showBtn;
    protected float timeLife;
    private bool isCoolDown;

    protected UnityAction actionClick;
    protected BaseDataButtonEvent data;

    public virtual void Init(BaseDataButtonEvent data, UnityAction actionClick, bool isReplaceIcon = true)
    {
        this.actionClick = actionClick;
        this.data = data;
        if (isReplaceIcon)
        {
            if (iconImg != null)
                iconImg.sprite = data.icon;
            if (data.scaleIcon != Vector3.zero)
            {
                if (iconImg != null)
                    iconImg.gameObject.transform.localScale = data.scaleIcon;
            }
        }

        showBtn.onClick.AddListener(OnClick);
        InitState(data);
    }

    public virtual void InitState(BaseDataButtonEvent data)
    {
        timeLife = data.timeLife;
        if (timeLife <= -1000)
        {
            timeTxt.gameObject.SetActive(false);
            if (data.titleIconShow != null && data.titleIconShow != "")
            {
                timeTxt.gameObject.SetActive(true);
                timeTxt.text = Localization.Get(data.titleIconShow);
            }
            isCoolDown = false;
        }
        else
        {
            timeTxt.gameObject.SetActive(true);
            isCoolDown = true;
        }
    }

    protected void OnClick()
    {
        this.actionClick?.Invoke();
        GameController.Instance.musicManager.PlayClickSound();
    }

    protected virtual void Update()
    {
        if (isCoolDown)
        {
            timeLife -= Time.unscaledDeltaTime;
            timeTxt.text = TimeManager.ShowTime((long)timeLife);
            if (timeLife <= 0)
            {
                OnCoolDownDone();
            }
        }
    }

    protected virtual void OnCoolDownDone()
    {
        isCoolDown = false;
    }
}
