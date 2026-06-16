using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseTopButtonGUI : MonoBehaviour
{
    [SerializeField] protected Text valueText;
    [SerializeField] protected Image plusImage;
    [SerializeField] protected Button plusButton;
    [SerializeField] protected bool canClickIAP = true;
    
    protected readonly CompositeDisposable disposables = new CompositeDisposable();
    
    private int _valueCache;
    private Tweener _textTween;
    protected bool isInited;

    public void Init() {
        if (canClickIAP) {
            plusButton.onClick.AddListener(OnClick);
            plusButton.interactable = true;
        }
        else {
            plusImage.gameObject.SetActive(false);
            plusButton.interactable = false;
        }

        InitState();
        isInited = true;
    }

    private void InitState() {
        OnStart();
        AddingEvent();
    }

    protected virtual void OnDestroy() {
        disposables.Dispose();
    }

    private void OnEnable() {
        UpdateValue(true);
    }

    protected virtual void OnStart() {
        
    }

    protected abstract void AddingEvent();
    protected abstract void OnClick();
    protected abstract int GetValue { get; }

    protected virtual void UpdateValue(bool isEnable = false) {

        if(isEnable)
        {
            valueText.text = GetValue.ToString();
            return;
        }
        if(!isInited)
        {
            valueText.text = GetValue.ToString();
            return;
        }    

        if (!gameObject.activeInHierarchy) {
            valueText.text = GetValue.ToString();
        }
        else {
           
            _textTween = _textTween ??
                         DOTween.To(() => _valueCache, value => {
                                    _valueCache = value;
                                    valueText.text = value.ToString();
                                }, GetValue, 1f)
                                .SetUpdate(true)
                                .SetAutoKill(false).OnComplete(()=> {
                                    valueText.text = GetValue.ToString();
                                });
            _textTween.ChangeStartValue(_valueCache);
            _textTween.ChangeEndValue(GetValue);
            _textTween.Play();
        }
    }

    protected virtual void OnDisable()
    {
        _textTween.Complete();
    }

    public void SetCanClickIAP(bool isCan)
    {
        plusButton.interactable = isCan;
    }
}