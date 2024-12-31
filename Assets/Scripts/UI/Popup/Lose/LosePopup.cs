using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LosePopup : BasePopup
{
    [SerializeField] private Button replayButton;
    [SerializeField] private Button returnHomeButton;

    public static event Action replayLevelEvent;

    protected override void RegisterMoreEvent()
    {
        GameStateLose.loseLevelEvent += OnLevelLose;

        replayButton.onClick.AddListener(Replay);
        returnHomeButton.onClick.AddListener(ReturnHome);
    }

    protected override void UnregisterMoreEvent()
    {
        GameStateLose.loseLevelEvent -= OnLevelLose;
    }

    private void OnLevelLose()
    {
        Show();

        AudioSource loseSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.LOSE_SOUND);

        loseSound.Play();
    }

    private void Replay()
    {
        replayLevelEvent?.Invoke();

        Hide();
    }

    private void ReturnHome()
    {
        Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }
}
