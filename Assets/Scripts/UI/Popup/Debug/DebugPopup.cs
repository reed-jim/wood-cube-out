using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugPopup : BasePopup
{
    [SerializeField] private TMP_InputField levelInput;

    [SerializeField] private Button debugPlayLevelButton;
    [SerializeField] private Button debugWinLevelButton;

    public static event Action<int> toLevelEvent;
    public static event Action winLevelEvent;

    protected override void RegisterMoreEvent()
    {
        debugPlayLevelButton.onClick.AddListener(DebugPlayLevel);
        debugWinLevelButton.onClick.AddListener(DebugWinLevel);
    }

    private void DebugPlayLevel()
    {
        int level = int.Parse(levelInput.text);

        toLevelEvent?.Invoke(level);

        Hide();
    }

    private async void DebugWinLevel()
    {
        Hide();

        await Task.Delay(500);

        winLevelEvent?.Invoke();
    }
}
