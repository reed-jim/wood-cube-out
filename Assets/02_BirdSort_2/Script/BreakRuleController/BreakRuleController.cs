namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using DG.Tweening;
    using UnityEngine.UI;

    public class BreakRuleController : MonoBehaviour
    {
        public bool isUseBooster;
        private Level_BirdSort_2 level;
        public CanvasGroup panelTut;
        public GameObject hand;
        [SerializeField] private Image BGItem;

        [HideInInspector] public StandingBase standCurrentClick;

        public void Init()
        {
            isUseBooster = false;
            level = GamePlayController_BirdSort.Instance.level;
            standCurrentClick = null;
        }
        public void HandleBreakRuleBooster()
        {
            isUseBooster = true;
            panelTut.gameObject.SetActive(true);
            panelTut.DOKill();
            panelTut.alpha = 0;
            panelTut.DOFade(1, 0.3f);

            BGItem.gameObject.SetActive(true);
            BGItem.color = new Color(BGItem.color.r, BGItem.color.g, BGItem.color.b, 0);
            BGItem.DOKill();
            BGItem.DOFade(0.65f, 0.25f);
        }

        public void OffOutLine()
        {

            foreach (var item in level.standsInLevel_Real)
            {
                if (item.birdsOnArray.Length > 0)
                {
                    for (int i = 0; i < item.birdsOnArray.Length; i++)
                    {
                        item.birdsOnArray[i].HandleOffActionSuggetBooster();
                    }
                    
                }
            }
        }
        public void OffPanelAndBool()
        {
            isUseBooster = false;
            panelTut.DOKill();
            panelTut.DOFade(0, 0.2f).OnComplete(() => { panelTut.gameObject.SetActive(false); });
            BGItem.DOKill();
            BGItem.DOFade(0, 0.15f);

            if (hand.activeSelf)
            {
                hand.SetActive(false);
                // EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.EVENT_CLICK_STANDING, tempAction);
            }
            standCurrentClick = null;
            BreakTheRuleItem_BirdSort it = (BreakTheRuleItem_BirdSort)GamePlayController_BirdSort.Instance.itemController.GetItem(GiftType.BreakTheRule_BirdSort);
            it.EndSuggetTut();
        }

        public void OnClickBrand(int currentStand)
        {
            var standChosing = BirdSort2.GamePlayController_BirdSort.Instance.level.standChosing;
            var listBrand = BirdSort2.GamePlayController_BirdSort.Instance.level.standsInLevel_Real;
            if (standChosing != null)
            {
                foreach (var item in listBrand)
                {
                    if (item.birdsOnArray.Length == 1)
                    {

                        hand.transform.DOMove(item.slots[3].transform.position, 0.3f);
                        break;
                    }
                }
            }

        }
        private void HandeleTut()
        {

        }
    }
}