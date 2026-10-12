using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class BaseTutorial : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Button nextButton;
    public Button doAgainButton;
    public Transform tutorialElementContainer;

    public Image tutorialHand;

    public int maxStep;
    protected int step;

    protected bool isWaitForNextStep;

    void Awake()
    {
        nextButton.onClick.AddListener(NextStep);
        doAgainButton.onClick.AddListener(DoAgain);

        MoreActionOnAwake();
    }

    void OnDestroy()
    {
        MoreActionOnDestroy();
    }

    protected virtual void MoreActionOnAwake()
    {

    }

    protected virtual void MoreActionOnDestroy()
    {

    }

    public virtual void StartTutorial()
    {

    }

    protected void NextStep()
    {
        step++;
        isWaitForNextStep = false;
    }

    protected virtual void EndTutorial()
    {
        Tween.Alpha(canvasGroup, 0, duration: 0.3f).OnComplete(() =>
        {
            canvasGroup.gameObject.SetActive(false);
        });
    }

    protected virtual void DoAgain()
    {
        step = 0;
        isWaitForNextStep = false;
    }
}
