using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

namespace GPlayUI
{
    public class ToggleUIHandle : MonoBehaviour
    {
        [SerializeField] private Image farmeBG;
        [SerializeField] private Sprite onFarmeSpr;
        [SerializeField] private Sprite offFarmeSpr;

        [SerializeField] private Image choiceImg;
        [SerializeField] private Sprite onChoiceSpr;
        [SerializeField] private Sprite offChoiceSpr;
        [SerializeField] private Transform onPosTrans;
        [SerializeField] private Transform offPosTrans;

        [SerializeField] private Button clickBtn;
        private UnityAction actionOn, actionOff;
        private bool isOn;
        public void Init(UnityAction actionOn, UnityAction actionOff, bool isOn)
        {
            this.actionOn = actionOn;
            this.actionOff = actionOff;
            this.isOn = isOn;
            clickBtn.onClick.AddListener(OnClickChoice);
            IsOnHandleUI(this.isOn, false);
        }    

        public void OnClickChoice()
        {
            this.isOn = !this.isOn;

            if(this.isOn)
            {
                IsOnHandleUI(true, true);
                this.actionOn?.Invoke();
               
            }   
            else
            {
                IsOnHandleUI(false, true);
                this.actionOff?.Invoke();
                
            }    
        } 
        
        public void IsOnHandleUI(bool isOn, bool isAnim = false)
        {
            if (isOn)
            {
                farmeBG.sprite = onFarmeSpr;
                choiceImg.sprite = onChoiceSpr;

                choiceImg.transform.DOKill();
                if (isAnim)
                {
                    choiceImg.transform.DOMove(onPosTrans.position, 0.3f);
                }   
                else
                {
                    choiceImg.transform.position = onPosTrans.position;
                }    
            }
            else
            {
                farmeBG.sprite = offFarmeSpr;
                choiceImg.sprite = offChoiceSpr;

                choiceImg.transform.DOKill();
                if (isAnim)
                {
                    choiceImg.transform.DOMove(offPosTrans.position, 0.3f);
                }
                else
                {
                    choiceImg.transform.position = offPosTrans.position;
                }
            }    
        }    
    }
}