using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;

public class NodeGroupUI : MonoBehaviour
{
    public NodeGroupServiceLocator serviceLocator;

    [Header("UI")]
    public Button selectButton;

    [Header("Highlight")]
    public Image highlight;
    private Sequence highlightSequence;
    private Tween highlightTween;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public void Select()
    {
        if (IdleGameInputManager.inputMode == IdleGameInputMode.SelectNodeGroup)
        {
            AugmentManager.Ins.ApplyAugmentForNodeGroup(serviceLocator.nodeGroup);

            MultiverseIdleGameplayScreen.Ins.HideSelectAugmentUI();

            IdleGameInputManager.inputMode = IdleGameInputMode.Normal;

            EndHighlight();
        }
    }

    #region Highlight
    public void Highlight()
    {
        if (highlightSequence.isAlive)
        {
            highlightSequence.Stop();
        }

        if (highlightTween.isAlive)
        {
            highlightTween.Stop();
        }

        highlight.gameObject.SetActive(true);
        highlight.color = ColorUtil.WithAlpha(Color.white, 0f);

        highlightSequence = SaferioTween.ShowWithPunch(highlight.rectTransform, onCompleteAction: () =>
        {
            highlightTween = Tween.Alpha(highlight, 1f, duration: 0.6f, cycles: -1, cycleMode: CycleMode.Yoyo);
        });
    }

    public void EndHighlight()
    {
        if (highlightTween.isAlive)
        {
            highlightTween.Stop();

            SaferioTween.ScaleDownWithPunch(highlight.rectTransform);
        }
    }
    #endregion
}
