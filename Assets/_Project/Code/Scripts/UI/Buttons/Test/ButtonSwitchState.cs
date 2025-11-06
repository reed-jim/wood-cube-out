using System;
using NamPhuThuy.ComponentSM;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{
    /// <summary>
    /// Multi-state button switch that can either:
    ///  - Drive a ComponentStateMachine (children are states), OR
    ///  - Manually toggle a list of GameObjects as states.
    /// </summary>
    public class ButtonSwitchState : MonoBehaviour
    {
        public enum Mode
        {
            UseComponentStateMachine,
            ManualGameObjects
        }

        [Header("Mode")]
        [SerializeField] private Mode mode = Mode.UseComponentStateMachine;

        [Header("StateMachine Mode")]
        [Tooltip("If empty, will try to find a ComponentStateMachine on this or child objects.")]
        [SerializeField] private ComponentStateMachine stateMachine;

        [Header("Manual Mode")]
        [Tooltip("Manual list of state GameObjects (only one active at a time).")]
        [SerializeField] private GameObject[] manualStates;

        [Header("State")]
        [Tooltip("Current state index (0..N-1).")]
        [SerializeField] private int currentIndex = 0;

        [Header("Events")]
        public UnityEvent<int> OnStateChangedIndex;
        public UnityEvent<string> OnStateChangedName;

        // Optional convenience actions for legacy two-state callbacks
        public Action OnSwitchFirst;  // fired when index == 0
        public Action OnSwitchLast;   // fired when index == last

        // ------------- Lifecycle -------------

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            EnsureSetup();
            ApplyState(currentIndex, invokeEvents: false); // Initialize visuals without spam
        }

        private void OnValidate()
        {
            // Keep currentIndex in range if modified in inspector
            int count = GetStateCount();
            if (count > 0)
            {
                if (currentIndex < 0) currentIndex = 0;
                if (currentIndex >= count) currentIndex = count - 1;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Cycle to next state (wrap around).
        /// Hook this to your UI Button onClick.
        /// </summary>
        public void SwitchState()
        {
            int count = GetStateCount();
            if (count == 0) return;

            int next = (currentIndex + 1) % count;
            SetState(next);
        }

        public void Next()
        {
            int count = GetStateCount();
            if (count == 0) return;

            int next = Mathf.Min(currentIndex + 1, count - 1);
            SetState(next);
        }

        public void Previous()
        {
            int count = GetStateCount();
            if (count == 0) return;

            int prev = Mathf.Max(currentIndex - 1, 0);
            SetState(prev);
        }

        public void SetState(int index)
        {
            // Debug.Log($"ButtonSwitchState.SetState()");
            if (index < 0 || index >= GetStateCount()) return;

            if (index == currentIndex)
            {
                // Debug.Log($"ButtonSwitchState.SetState() index = currentIndex");
                
                switch (mode)
                {
                    case Mode.UseComponentStateMachine:
                    {
                        if (stateMachine != null)
                            ApplyState(index, invokeEvents: true);
                        break;
                    }
                    case Mode.ManualGameObjects:
                    {
                        ApplyState(index, invokeEvents: false);
                        break;
                    }
                }
                
                return;
            }

            currentIndex = index;
            ApplyState(currentIndex, invokeEvents: true);
        }

        /// <summary>
        /// For StateMachine mode: set by child name. For Manual: set by GameObject name.
        /// </summary>
        public void SetState(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            switch (mode)
            {
                case Mode.UseComponentStateMachine:
                    {
                        // Try find child with this name
                        var t = (stateMachine ? stateMachine.transform : transform).Find(name);
                        if (!t) return;
                        SetState(t.GetSiblingIndex());
                        break;
                    }
                case Mode.ManualGameObjects:
                    {
                        if (manualStates == null) return;
                        for (int i = 0; i < manualStates.Length; i++)
                        {
                            if (manualStates[i] != null && manualStates[i].name == name)
                            {
                                SetState(i);
                                return;
                            }
                        }
                        break;
                    }
            }
        }

        #endregion

        #region Private Methods

        private void EnsureSetup()
        {
            if (mode == Mode.UseComponentStateMachine)
            {
                if (stateMachine == null)
                {
                    stateMachine = GetComponent<ComponentStateMachine>();
                    if (stateMachine == null)
                        stateMachine = GetComponentInChildren<ComponentStateMachine>(true);
                }
            }
        }

        private int GetStateCount()
        {
            int res = 0;
            switch (mode)
            {
                case Mode.UseComponentStateMachine:
                {
                    var root = stateMachine ? stateMachine.transform : transform;
                    res = root.childCount;
                    break;
                }
                case Mode.ManualGameObjects:
                    res =  manualStates?.Length ?? 0;
                    break;
            }
            
            // Debug.Log($"ButtonSwitchState.GetStateCount() - {res}");
            return res;
        }

        private string GetStateName(int index)
        {
            string res = string.Empty;
            
            switch (mode)
            {
                case Mode.UseComponentStateMachine:
                {
                    var root = stateMachine ? stateMachine.transform : transform;
                    if (index < 0 || index >= root.childCount)
                    {
                        res = string.Empty;
                        break;
                    }
                    res = root.GetChild(index).name;
                    break;
                }
                case Mode.ManualGameObjects:
                {
                    if (manualStates == null || index < 0 || index >= manualStates.Length || manualStates[index] == null)
                    {
                        res = string.Empty;
                        break;
                    }
                    res = manualStates[index].name;
                    break;
                }
            }
            return res;
        }

        private void ApplyState(int index, bool invokeEvents)
        {
            switch (mode)
            {
                case Mode.UseComponentStateMachine:
                {
                    if (stateMachine == null)
                    {
                        Debug.LogWarning($"{name}: No ComponentStateMachine found.");
                        return;
                    }
                    stateMachine.ChangeState(index);
                    break;
                }

                case Mode.ManualGameObjects:
                {
                    if (manualStates == null)
                    {
                        // Debug.Log($"ButtonSwitchState.ApplyState() - manualStates is null");
                        return;
                    }

                    if (manualStates[index] == null)
                    {
                        // Debug.Log($"ButtonSwitchState.ApplyState() - manualStates is null at {index}");
                        return;
                    }
                    // Debug.Log($"ButtonSwitchState.ApplyState() at {index}");
                    
                    for (int i = 0; i < manualStates.Length; i++)
                    {
                        manualStates[i].SetActive(i == index);    
                    }
                    
                    
                    break;
                }
            }

            if (invokeEvents)
            {
                OnStateChangedIndex?.Invoke(index);
                OnStateChangedName?.Invoke(GetStateName(index));

                // Optional legacy helpers:
                if (index == 0) OnSwitchFirst?.Invoke();
                if (index == GetStateCount() - 1) OnSwitchLast?.Invoke();
            }
        }

        #endregion

        // ------------- Editor Helpers -------------

#if UNITY_EDITOR
        [ContextMenu("Collect Manual States From Children")]
        private void CollectManualStatesFromChildren()
        {
            var list = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in transform)
                list.Add(child.gameObject);
            manualStates = list.ToArray();

            EditorUtility.SetDirty(this);
            Debug.Log($"{name}: Collected {manualStates.Length} manual states from children.");
        }
#endif
    }
}
