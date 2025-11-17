using System;
using DG.Tweening;
using MoreMountains.Tools;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;


namespace NamPhuThuy.UI
{

    public class CoinPanel : MonoBehaviour, MMEventListener<EResourceUpdated>
    {
        #region Private Serializable Fields

        [SerializeField] private TextMeshProUGUI coinText;
        public TextMeshProUGUI CoinText => coinText;

        [SerializeField] private RectTransform coinImage;

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            GalleryPackElement.updateCoinTextEvent += UpdateUI;
        }

        private void OnDestroy()
        {
            GalleryPackElement.updateCoinTextEvent -= UpdateUI;
        }

        void OnEnable()
        {
            UpdateUI();
            MMEventManager.RegisterAllCurrentEvents(this);
        }

        private void OnDisable()
        {
            MMEventManager.UnregisterAllCurrentEvents(this);
        }

        #endregion

        #region Private Methods

        public void UpdateUI()
        {
            // The cost of a null check (if (coinText != null)) is negligible compared to UI updates or string conversions.
            // if (coinText != null)
            //     coinText.text = DataManager.Ins.PlayerData.Coin.ToString();

            if (coinText != null)
            {
                AnimateCoinUpdate();
            }
        }

        private void AnimateCoinUpdate()
        {
            int currentCoin = int.Parse(CoinText.text);
            int currentValue = currentCoin;

            DOTween.To
            (
                () => currentValue
                , x =>
                {
                    currentValue = x;
                    coinText.text = currentValue.ToString();
                },
                (int)DataManager.Ins.UserResourcesObserver.UserResources.CoinQuantity,
                0.3f
            )
            .SetEase(Ease.Linear);
        }

        #endregion

        #region Public Methods

        public RectTransform CoinImage => coinImage;

        #endregion

        #region Events Listen

        public void OnMMEvent(EResourceUpdated eventType)
        {
            if (eventType.ResourceType == ResourceType.COIN)
                UpdateUI();
        }

        #endregion


    }
}