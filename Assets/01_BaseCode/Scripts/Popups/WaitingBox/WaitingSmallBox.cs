using UnityEngine;
using UniRx;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Sirenix.OdinInspector;

public class WaitingSmallBox : MonoBehaviour
{
    public Action action;
    [SerializeField] Text msgText;
    private static WaitingSmallBox instance;
    private IDisposable _waitDispose;
    [SerializeField] private Button cancelButton;
    private Action onClickCancel;
    private int _showQueue;

#if UNITY_EDITOR
    [Button]
    private void ShowQueueNow()
    {
        Debug.Log(_showQueue);
    }
#endif

    public static WaitingSmallBox Setup()
    {
        if (instance == null)
        {
            // Create popup and attach it to UI
            instance = Instantiate(Resources.Load<WaitingSmallBox>(PathPrefabs.WAITING_BOX_SMALL));
            // Configure popup
            instance.cancelButton.onClick.AddListener(() => instance.onClickCancel?.Invoke());
            instance._showQueue = 0;
        }

        instance.gameObject.SetActive(true);
        return instance;
    }


    private void Start()
    {
    }

    private void OnDestroy()
    {
        if (_waitDispose != null)
            _waitDispose.Dispose();
    }

    public void ShowWaiting(bool useTimeout = true, string status = "", bool isShowCancelButton = false)
    {
        msgText.text = status;
        BoxController.Instance.isLockEscape = true;
        gameObject.SetActive(true);
        GetComponent<RectTransform>().rect.Set(0, 0, 0, 0);
        msgText.DOKill();
        if (isShowCancelButton) ShowCancelButton();
        else HideCancelButton();
        if (useTimeout)
            TimeOut(40);
    }

    public void UpdateTextStatus(string status)
    {
        if (instance == null)
        {
            return;
        }

        instance.msgText.text = status;
    }

    public void SetCancelButton(Action onClick)
    {
        onClickCancel = onClick;
    }

    public void ShowCancelButton()
    {
        cancelButton.interactable = true;
        cancelButton.gameObject.SetActive(true);
        cancelButton.transform.localScale = Vector3.zero;
        cancelButton.transform.DOScale(1, 0.25f);
    }

    public void HideCancelButton()
    {
        cancelButton.interactable = false;
        cancelButton.transform.DOScale(0, 0.25f);
        cancelButton.gameObject.SetActive(false);
    }

    public void ShowQueue()
    {
        _showQueue++;
        if (_showQueue == 1)
        {
            ShowWaiting(false, "", false);
        }
    }

    public void HideQueue()
    {
        _showQueue--;
        if (_showQueue == 0)
        {
            HideWaiting();
        }
    }


    public void HideWaiting(bool isLockEscape = false)
    {
        if (_waitDispose != null)
            _waitDispose.Dispose();
        if (_showQueue != 0)
        {
            return;
        }

        BoxController.Instance.isLockEscape = isLockEscape;
        //Debug.LogError("===========================HideWaiting==========================");
        if (gameObject != null && gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    public void ShowWaiting(float time)
    {
        // Show va Hide, ko lam gi ca
        action = null;
        ShowWaiting();
        TimeOut(time);
    }

    public void ShowWaiting(float time, Action action)
    {
        // Show va Hide, ko lam gi ca
        ShowWaiting();
        this.action = action;
        TimeOut(time);
    }

    private void TimeOut(float time)
    {
        if (_waitDispose != null)
            _waitDispose.Dispose();
        _waitDispose = Observable.Timer(TimeSpan.FromSeconds(time), Scheduler.MainThreadIgnoreTimeScale)
            .Subscribe(_ =>
            {
                if (gameObject.activeSelf)
                {
                    Debug.Log("TimeOut");
                    HideWaiting();
                    if (action != null)
                        action();
                }
            });
    }
}