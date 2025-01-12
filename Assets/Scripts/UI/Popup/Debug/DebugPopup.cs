using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugPopup : BasePopup
{
    [SerializeField] private TMP_InputField levelInput;

    [SerializeField] private Button debugPlayLevelButton;
    [SerializeField] private Button debugNextLevelButton;
    [SerializeField] private Button debugPrevLevelButton;
    [SerializeField] private Button debugWinLevelButton;

    public static event Action<int> toLevelEvent;
    public static event Action nextLevelEvent;
    public static event Action prevLevelEvent;
    public static event Action winLevelEvent;

    protected override void RegisterMoreEvent()
    {
        debugPlayLevelButton.onClick.AddListener(DebugPlayLevel);
        debugNextLevelButton.onClick.AddListener(NextLevel);
        debugPrevLevelButton.onClick.AddListener(PrevLevel);
        debugWinLevelButton.onClick.AddListener(DebugWinLevel);
    }

    private void DebugPlayLevel()
    {
        int level = int.Parse(levelInput.text);

        toLevelEvent?.Invoke(level);

        Hide();
    }

    private void NextLevel()
    {
        nextLevelEvent?.Invoke();

        Hide();
    }

    private void PrevLevel()
    {
        prevLevelEvent?.Invoke();

        Hide();
    }

    private async void DebugWinLevel()
    {
        Hide();

        await Task.Delay(500);

        winLevelEvent?.Invoke();
    }
}
