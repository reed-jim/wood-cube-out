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
            
            
            if (onState == null)
                Debug.LogWarning($"[ButtonSwitch] `onState` is null or destroyed on {name}", this);
            if (offState == null)
                Debug.LogWarning($"[ButtonSwitch] `offState` is null or destroyed on {name}", this);
            
            switch (state)
            {
                case ButtonSwitchState.ON:
                    onState?.SetActive(true);
                    offState?.SetActive(false);
                    break;
                case ButtonSwitchState.OFF:
                    // a destroyed object is not null for C# ?. checks, so onState?.SetActive(false); still calls SetActive on a destroyed GameObject.
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