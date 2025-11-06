using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NamPhuThuy.Data;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{
    public class GUIReward : GUIBase
    {
        [Header("Components")]
        [SerializeField] private RectTransform canvas;
        [SerializeField] private RectTransform container;
        [SerializeField] private RectTransform[] rewardItemRTs;
       
        [SerializeField] private /*CoinPanel*/GameObject coinPanel;
        [SerializeField] private Transform rewardListPivot;
        [SerializeField] private ElementReward elementRewardPrefab;
        [SerializeField] private List<ElementReward> elementRewards;

        private GUIRewardItem[] _rewardItems;

        #region Serializable Fields

        [SerializeField] private List<ResourceReward> currentRewardList;
        
        [SerializeField] private Button okButton;
        [SerializeField] private Button closeButton;

        #endregion
        
        #region Private Fields

        private void SetupRewards(List<ResourceReward> rewardList)
        {
            List<RectTransform> validResourceGroups = new List<RectTransform>();

            foreach (ResourceReward reward in rewardList)
            {
                switch (reward.resourceType)
                {
                    case ResourceType.NO_ADS:
                        validResourceGroups.Add(rewardItemRTs[0]);
                        break;
                    
                    case ResourceType.COIN:
                        if (reward.amount <= 0) break;
                        
                        _rewardItems[1].SetQuantity(reward.amount);
                        validResourceGroups.Add(rewardItemRTs[1]);
                        break;
                    
                    case ResourceType.BOOSTER:
                        if (reward.boosterType == BoosterType.UNDO)
                        {
                            _rewardItems[2].SetQuantity(reward.amount);
                            validResourceGroups.Add(rewardItemRTs[2]);
                        }
                        else if (reward.boosterType == BoosterType.SHUFFLE)
                        {
                            _rewardItems[3].SetQuantity(reward.amount);
                            validResourceGroups.Add(rewardItemRTs[3]);
                        }
                        else if (reward.boosterType == BoosterType.MAGIC_PICK)
                        {
                            _rewardItems[4].SetQuantity(reward.amount);
                            validResourceGroups.Add(rewardItemRTs[4]);
                        }
                        break;
                }
            }

            Vector3 position = Vector3.zero;
            position.y = validResourceGroups[0].localPosition.y;

            for (int i = 0; i < rewardItemRTs.Length; i++)
            {
                rewardItemRTs[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < validResourceGroups.Count; i++)
            {
                position.x = (-(validResourceGroups.Count - 1) / 2f + i) * 0.17f * container.sizeDelta.x;

                validResourceGroups[i].gameObject.SetActive(true);
                validResourceGroups[i].localPosition = position;
            }
        }
        
        private void SetupRewards2(List<ResourceReward> resourceRewards)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            _rewardItems = new GUIRewardItem[rewardItemRTs.Length];

            for (int i = 0; i < rewardItemRTs.Length; i++)
            {
                _rewardItems[i] = rewardItemRTs[i].GetComponent<GUIRewardItem>();
            }
        }

        private void OnEnable()
        {
            okButton.onClick.AddListener(OnClickConfirm);
            closeButton.onClick.AddListener(OnClickConfirm);
        }

        private void OnDisable()
        {
            okButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
        }

        #endregion

        

        #region Button Events

        private void OnClickConfirm()
        {
            Hide();
            
            foreach (ResourceReward reward in currentRewardList)
            {
                if (reward.resourceType == ResourceType.COIN)
                {
                    /*VFXManager.Ins.PlayAt(
                        VFXType.COIN_FLY, 
                        amount: reward.amount,
                        initialParent: VFXManager.Ins.transform);*/
                    break;
                }
            }
        }

        #endregion

        #region Override Methods

        public override void Show(params object[] parameters)
        {
            base.Show(parameters);

            if (parameters != null)
            {
                currentRewardList = (List<ResourceReward>) parameters[0];
            }
            SetupRewards(currentRewardList);
            // SetupRewards2(currentRewardList);
        }

        

        public override void Hide(params object[] parameters)
        {
            base.Hide(parameters);
            // DataManager.Ins.PlayerData.TryApplyRewards(currentRewardList);
            
        }

        #endregion

        
    }
}
