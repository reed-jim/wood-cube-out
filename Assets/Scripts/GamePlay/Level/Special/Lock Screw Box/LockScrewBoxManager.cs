using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static GameEnum;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.GamePlay
{

    public class LockScrewBoxManager : BaseSpecialLevelManager
    {
        #region Private Serializable Fields

        [SerializeField] private List<ScrewBoxLocked> lockedScrewBoxes;
        [SerializeField] private int lockedBoxIndex = 1;
        [SerializeField] private int screwBoxCount = 0;

        [SerializeField] private SpecialLevelTaskRecord currentRecord;
        #endregion

        public GameFaction LockedScrewFaction => _specialLevelTaskRecord?.requiredComponents?
            .Where(component => component.componentType == GameComponentType.Screw)
            .FirstOrDefault()?.faction ?? GameFaction.None;

        public static event Action<ScrewBox, SpecialLevelTaskRecord> requestLockedScrewBox;
        public static event Action<LockScrewBoxManager> lockScrewBoxManagerInitEvent;

        #region MonoBehaviour Callbacks

        protected override void Awake()
        {
            base.Awake();

            ScrewBoxManager.screwBoxSpawnedEvent += OnScrewBoxSpawned;
            ScrewBoxManager.screwBoxLockSpawnedEvent += OnScewBoxLockSpawned;

            StartCoroutine(IETriggerInitEvent());
            IEnumerator IETriggerInitEvent()
            {
                yield return new WaitForSeconds(0.2f);
                lockScrewBoxManagerInitEvent?.Invoke(this);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ScrewBoxManager.screwBoxSpawnedEvent -= OnScrewBoxSpawned;
            ScrewBoxManager.screwBoxLockSpawnedEvent -= OnScewBoxLockSpawned;
        }

        #endregion

        #region Private Methods

        private void LockBoxes(ScrewBox refScrewBox)
        {
            _specialLevelTaskRecord = SpecialLevelTaskRecord.GenerateTask(SpecialLevelTaskType.ScrewComplete);

            _specialLevelTaskRecord.requiredComponents[0].faction = refScrewBox.Faction;

            requestLockedScrewBox?.Invoke(refScrewBox, _specialLevelTaskRecord);
            // ScrewBoxLocked screwBoxLocked = CommonUtil.ReplaceAndCopy<ScrewBox, ScrewBoxLocked>(screwBoxes[1].gameObject);
        }

        #endregion

        #region Override Methods

        public override bool IsFactionLocked(GameEnum.GameFaction faction)
        {
            return base.IsFactionLocked(faction);
        }

        protected override void OnScrewLoosed(BaseScrew screw)
        {

        }

        protected override void Reset()
        {
            base.Reset();

            foreach (ScrewBoxLocked screwBox in lockedScrewBoxes)
            {
                ObjectPoolingEverything.ReturnToPool(GameConstants.SCREW_BOX_LOCKED, screwBox.gameObject);
            }

            lockedScrewBoxes.Clear();

            screwBoxCount = 0;
        }

        #endregion

        #region Event Methods

        private void OnScewBoxLockSpawned(ScrewBoxLocked screwBoxLocked)
        {
            lockedScrewBoxes.Add(screwBoxLocked);
        }

        private void OnScrewBoxSpawned(ScrewBox screwbox)
        {
            if (screwBoxCount == lockedBoxIndex)
            {
                LockBoxes(screwbox);
            }

            screwBoxCount++;
        }
        #endregion
    }
}
