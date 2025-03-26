using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScoreIQView : MonoBehaviour
{
    [SerializeField] protected Text iqTxt;
    [SerializeField] protected Image progessComboImg;
    [SerializeField] protected Text comboTxt;

    private Tweener _textTween;
    private int _valueCache;

    private ScoreIQController controller;

    public void Init(ScoreIQController controller)
    {
        iqTxt.text = "IQ: 0";
        progessComboImg.fillAmount = 0;
        comboTxt.text = "X1 IQ";

        this.controller = controller;
    }

    public void AddCombo(ComboData combo)
    {
        comboTxt.text = "X" + combo.score + " IQ";
        progessComboImg.DOKill();
        progessComboImg.fillAmount = 1;
        UpdateIQValue();

        comboTxt.transform.DOKill();
        comboTxt.transform.localScale = Vector3.one;
        comboTxt.transform.DOScale(1.3f, 0.2f).OnComplete(() =>
        {
            comboTxt.transform.DOScale(1f, 0.2f);
        });
    }

    public void ResetCombo()
    {
        progessComboImg.fillAmount = 0;
        comboTxt.text = "X1 IQ";
    }

    public void CoolDownCombo(float percent)
    {
        progessComboImg.fillAmount = percent;
    }

    public virtual void UpdateIQValue()
    {
        _textTween = _textTween ??
                         DOTween.To(() => _valueCache, value =>
                         {
                             _valueCache = value;
                             iqTxt.text = "IQ: " +  value.ToString();
                         }, this.controller.currentIQScore, 1f)
                                .SetUpdate(true)
                                .SetAutoKill(false).OnComplete(() =>
                                {
                                    iqTxt.text = "IQ: " + this.controller.currentIQScore.ToString();
                                });
        _textTween.ChangeStartValue(_valueCache);
        _textTween.ChangeEndValue(this.controller.currentIQScore);
        _textTween.Play();

        iqTxt.transform.DOKill();
        iqTxt.transform.localScale = Vector3.one;
        iqTxt.transform.DOScale(1.3f, 0.2f).OnComplete(() =>
        {
            iqTxt.transform.DOScale(1f, 0.2f);
        });

    }
}
