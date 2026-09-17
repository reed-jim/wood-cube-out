using UnityEngine;
using UnityEngine.UI;

public class BaseTutorial : MonoBehaviour
{
    public Button nextButton;
    public Transform tutorialElementContainer;

    public int maxStep;
    protected int step;

    protected bool isWaitForNextStep;

    void Awake()
    {
        nextButton.onClick.AddListener(NextStep);

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

    protected void NextStep()
    {
        step++;
        isWaitForNextStep = false;
    }
}
