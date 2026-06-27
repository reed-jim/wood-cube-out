using CandyCoded.HapticFeedback;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

public class HiddenAntBox : WashingMachineItem
{
    [SerializeField] private SpriteRenderer hiddenContainer;
    [SerializeField] private TMP_Text questionMark;

    public override void Setup(WashingMachineStartSlot washingMachineStartSlot)
    {
        base.Setup(washingMachineStartSlot);

        if (washingMachineStartSlot.row == 0)
        {
            Reveal();
        }
        else
        {
            Hide();
        }
    }

    public override void Enable()
    {
        base.Enable();

        Reveal();
    }

    private void Hide()
    {
        questionMark.gameObject.SetActive(true);
        hiddenContainer.gameObject.SetActive(true);
        numberText.gameObject.SetActive(false);

        questionMark.color = Color.white;
        hiddenContainer.color = Color.white;
    }

    public void Reveal()
    {
        SaferioTween.AlphaHide(questionMark);
        SaferioTween.AlphaHide(hiddenContainer);
        SaferioTween.AlphaShow(numberText);
    }

    protected override void PlayInvalidSelectAnimation()
    {
        if (isInSlot)
        {
            return;
        }

        Tween.ShakeScale(transform, strength: 0.2f * Vector3.one, duration: 0.3f);

        if (GamePersistentVariable.IsVibrate)
        {
            HapticFeedback.MediumFeedback();
        }
    }
}
