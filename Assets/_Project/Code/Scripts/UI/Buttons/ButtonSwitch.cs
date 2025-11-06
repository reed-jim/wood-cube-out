using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{
    
    public class ButtonSwitch : MonoBehaviour
    {
        #region Private Serializable Fields

        [SerializeField] private GameObject onState;
        [SerializeField] private GameObject offState;
        public ButtonSwitchState currentState;
        

        #endregion

        #region Public Fields
        
        public enum ButtonSwitchState
        {
            ON = 0,
            OFF = 1
        }
        
        public Action OnSwitchOn;
        public Action OnSwitchOff;

        #endregion

        #region MonoBehaviour Callbacks


        #endregion

        #region Private Methods
        #endregion

        #region Public Methods

        public void SetState(ButtonSwitchState state)
        {
            currentState = state;
            switch (state)
            {
                case ButtonSwitchState.ON:
                    onState?.SetActive(true);
                    offState?.SetActive(false);
                    break;
                case ButtonSwitchState.OFF:
                    onState?.SetActive(false);
                    offState?.SetActive(true);
                    break;
            }
        }

        public void SwitchState()
        {
            switch (currentState)
            {
                case ButtonSwitchState.OFF:
                    SetState(ButtonSwitchState.ON);
                    break;
                    
                case ButtonSwitchState.ON:
                    SetState(ButtonSwitchState.OFF);    
                    break;
            }
        }
        
        #endregion

    }
}