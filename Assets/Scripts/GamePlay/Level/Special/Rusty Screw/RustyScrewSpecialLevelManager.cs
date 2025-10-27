using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using NamPhuThuy.VFX;
using UnityEngine;
using static GameEnum;
using Random = UnityEngine.Random;

namespace NamPhuThuy.GamePlay
{
    public class RustyScrewSpecialLevelManager : BaseSpecialLevelManager
    {
        [Header("Resources")] [Tooltip("Mesh: oc_low")] [SerializeField]
        private Mesh normalScrewMesh;

        [Tooltip("screw_rusty_broken")] [SerializeField]
        private Mesh rustyScrewMesh;

        private BaseScrew[] _rustyScrew;

        [Header("Flags")] [SerializeField] private bool useCustomRustyScrewMesh = true;

        /// <summary>
        /// Current faction that is locked by rusty screws, O(n) based on number of requiredComponents
        /// </summary>
        public GameFaction LockedScrewFaction => _specialLevelTaskRecord?.requiredComponents?
            .Where(component => component.componentType == GameComponentType.Screw)
            .FirstOrDefault()?.faction ?? GameFaction.None;

        public static event Action switchToRustyScrewsEvent;
        public static event Action<RustyScrewSpecialLevelManager> rustyScrewManagerInitEvent;
        public static event Action rustyBreakALayer;

        #region MonoBehaviour Callbacks

        protected override void Awake()
        {
            base.Awake();

            StartCoroutine(IETriggerInitEvent());

            IEnumerator IETriggerInitEvent()
            {
                yield return new WaitForSeconds(0.2f);
                rustyScrewManagerInitEvent?.Invoke(this);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region Override Methods

        public override bool IsFactionLocked(GameFaction faction)
        {
            if (_specialLevelTaskRecord == null)
            {
                return false;
            }

            return _specialLevelTaskRecord.requiredComponents
                .Any(item => item.componentType == GameComponentType.Screw && item.faction == faction);
        }

        protected override void LockSingleColor()
        {
            BaseScrew[] validScrews = _screws.Where(item => !item.IsDone && !item.IsInScrewPort).ToArray();

            if (_isDoneCompletely)
            {
                return;
            }
            else
            {
                if (validScrews.Length < 0.3f * _screws.Length)
                {
                    UpdateUI(SpecialLevelUIState.Hide);
                    VFXManager.Ins.PlayAt(VFXType.POPUP_TEXT,
                        message: VFXPopupTextMessage.NO_MORE_CHALLENGES,
                        initialParent: VFXManager.Ins.transform);

                    _isDoneCompletely = true;

                    return;
                }
            }

            BaseScrew targetScrew = CommonUtil.GetRandomValueFromArray(validScrews);

            if (targetScrew == null)
            {
                return;
            }

            _specialLevelTaskRecord = SpecialLevelTaskRecord.GenerateTask(SpecialLevelTaskType.ScrewComplete);

            _specialLevelTaskRecord.requiredComponents[0].faction = targetScrew.Faction;
            _specialLevelTaskRecord.requiredComponents[0].Amount = Random.Range(3, 9);

            UpdateUI(SpecialLevelUIState.UpdateProgress, new object[1] { _specialLevelTaskRecord });

            // Update screw shader
            _rustyScrew = validScrews.Where(item => item.Faction == targetScrew.Faction).ToArray();

            foreach (var item in _rustyScrew)
            {
                item.ScrewServiceLocator.screwMaterialPropertyBlock.SetScrewCondition(ScrewCondition.Rusty);

                if (rustyScrewMesh != null)
                {
                    ChangeMeshToRustyScrew(item);
                }
            }
        }

        protected override void OnLevelLost()
        {
            if (_delayedLockNextScrew != null)
            {
                StopCoroutine(_delayedLockNextScrew);
            }
        }

        protected override void OnScrewsSet(BaseScrew[] screws)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            base.OnScrewsSet(screws);

            switchToRustyScrewsEvent?.Invoke();
        }

        protected override void HandleTaskCompleted()
        {
            base.HandleTaskCompleted();

            foreach (var item in _rustyScrew)
            {
                item.ScrewServiceLocator.screwMaterialPropertyBlock.SetScrewCondition(ScrewCondition.Normal);
            }

            ReapplyNormalMesh();
        }

        protected override void OnScrewLoosed(BaseScrew screw)
        {
            base.OnScrewLoosed(screw);
            if (_specialLevelTaskRecord.taskType == SpecialLevelTaskType.ScrewComplete)
            {
                if (_specialLevelTaskRecord.IsTaskCompleted())
                {
                    SoundManager.Instance.PlaySoundRustyBreak();
                }
            }
        }

        protected override void OnCompleteTaskStep()
        {
            base.OnCompleteTaskStep();
            rustyBreakALayer?.Invoke();
        }

        #endregion

        #region ScrewManipulation

        void ChangeMeshToRustyScrew(BaseScrew screw)
        {

            if (screw == null || rustyScrewMesh == null) return;
            if (!useCustomRustyScrewMesh) return;


            try
            {
                var mf = screw.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    DebugLogger.LogSimple(message: $"change to rusty mesh");
                    mf.sharedMesh = rustyScrewMesh;
                }
            }
            catch (Exception e)
            {
                DebugLogger.LogSimple(message: $"Error: {e}", color: Color.red);
            }

        }

        public void ReapplyNormalMesh()
        {

            if (normalScrewMesh == null) return;
            if (!useCustomRustyScrewMesh) return;

            foreach (var screw in _rustyScrew)
            {
                try
                {
                    if (screw == null) continue;

                    DebugLogger.LogSimple(message: $"revert mesh");

                    // Restore mesh on whichever renderer exists
                    var mf = screw.GetComponentInChildren<MeshFilter>(true);
                    if (mf != null) mf.sharedMesh = normalScrewMesh;
                }
                catch (Exception e)
                {
                    DebugLogger.LogSimple(message: $"Error: {e}", color: Color.red);
                }
            }
        }

        #endregion


    }
}
