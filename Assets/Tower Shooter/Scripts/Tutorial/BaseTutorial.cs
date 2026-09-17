using UnityEngine;
using UnityEngine.UI;

public class BaseTutorial : MonoBehaviour
{
    public Button nextButton;

    public int maxStep;
    protected int step;

    protected bool isWaitForNextStep;

    void Awake()
    {
        nextButton.onClick.AddListener(NextStep);
    }

    protected void NextStep()
    {
        step++;
        isWaitForNextStep = false;
    }
}
