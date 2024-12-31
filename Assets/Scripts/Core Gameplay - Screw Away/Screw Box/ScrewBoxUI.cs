using System;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class ScrewBoxUI : MonoBehaviour
{
    [SerializeField] private RectTransform unlockByAdsButtonRT;
    [SerializeField] private Button unlockByAdsButton;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private ScrewBoxCameraObserver screwBoxCameraObserver;
    [SerializeField] private Vector2Variable canvasSize;

    private Camera _screwBoxCamera;

    #region EVENT
    public static event Action<int> unlockScrewBox;
    #endregion

    private void Awake()
    {
        ScrewBoxCameraManager.setCameraEvent += OnCameraSet;

        unlockByAdsButton.gameObject.SetActive(false);

        unlockByAdsButton.onClick.AddListener(Unlock);
    }

    void OnDestroy()
    {
        ScrewBoxCameraManager.setCameraEvent -= OnCameraSet;
    }

    private void OnCameraSet(Camera camera)
    {
        _screwBoxCamera = camera;
    }

    public void SetUnlockByAdsButtonPosition()
    {
        unlockByAdsButtonRT.localPosition = _screwBoxCamera.WorldToScreenPoint(transform.position) - 0.5f * (Vector3)canvasSize.Value;
    }

    public void Lock()
    {
        unlockByAdsButtonRT.localScale = Vector3.one;

        unlockByAdsButton.gameObject.SetActive(true);
    }

    private void Unlock()
    {
        Tween.LocalPositionY(unlockByAdsButtonRT, unlockByAdsButtonRT.localPosition.y + 0.5f * canvasSize.Value.y, duration: 0.3f).OnComplete(() =>
        {
            unlockByAdsButton.gameObject.SetActive(false);
        });

        // Tween.Scale(unlockByAdsButtonRT, 0, duration: 0.3f).OnComplete(() =>
        // {
        //     unlockByAdsButton.gameObject.SetActive(false);
        // });

        unlockScrewBox?.Invoke(gameObject.GetInstanceID());

        PlayUnlockSound();
    }

    private void PlayUnlockSound()
    {
        AudioSource sound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.UNLOCK_ADS_SCREW_BOX_SOUND);

        sound.Play();
    }
}
