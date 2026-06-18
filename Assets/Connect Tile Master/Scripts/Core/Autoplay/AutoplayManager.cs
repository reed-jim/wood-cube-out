using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using UnityEngine;

namespace AntsFlow
{
    public class AutoplayManager : Singleton<AutoplayManager>
    {
        // Scene-specific - khong DontDestroyOnLoad
        protected override bool ShouldPersist() => false;

        public float delayPerTap;

        public void StartAutoplay()
        {
            StartCoroutine(AutoplayLevel());
        }

        public void StartAutoplayFail()
        {
            StartCoroutine(AutoplayLevelFail());
        }

        private IEnumerator AutoplayLevelFail()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(delayPerTap);

            List<WashingMachineItem> poolAntBox;

            while (true)
            {
                poolAntBox = new List<WashingMachineItem>();

                for (int i = 0; i < WashingMachineManager.Ins.washingMachineStartSlots.Count; i++)
                {
                    WashingMachineStartSlot washingMachineStartSlot = WashingMachineManager.Ins.washingMachineStartSlots[i];

                    if (washingMachineStartSlot.row == 0 && washingMachineStartSlot.washingMachine != null)
                    {
                        poolAntBox.Add(washingMachineStartSlot.washingMachine);
                    }
                }

                if (poolAntBox.Count == 0)
                {
                    break;
                }

                List<Color> activeFactions = new List<Color>();
                if (WashingMachineManager.Ins.washingMachineSlots != null)
                {
                    foreach (var slot in WashingMachineManager.Ins.washingMachineSlots)
                    {
                        if (slot.washingMachineItem != null)
                        {
                            activeFactions.Add(slot.washingMachineItem.faction);
                        }
                    }
                }

                List<WashingMachineItem> failBoxes = poolAntBox.Where(box => !activeFactions.Contains(box.faction)).ToList();
                WashingMachineItem selectedBox = null;

                if (failBoxes.Count > 0)
                {
                    failBoxes.Shuffle();
                    selectedBox = failBoxes.First();
                }
                else
                {
                    poolAntBox.Shuffle();
                    selectedBox = poolAntBox.First();
                }

                if (selectedBox != null)
                {
                    selectedBox.Select();
                }

                GamePersistentVariable.isAutoTestingLevel = true;

                yield return waitForSeconds;
            }
        }

        private IEnumerator AutoplayLevel()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(delayPerTap);

            List<WashingMachineItem> poolAntBox;

            while (true)
            {
                poolAntBox = new List<WashingMachineItem>();

                for (int i = 0; i < WashingMachineManager.Ins.washingMachineStartSlots.Count; i++)
                {
                    WashingMachineStartSlot washingMachineStartSlot = WashingMachineManager.Ins.washingMachineStartSlots[i];

                    if (washingMachineStartSlot.row == 0 && washingMachineStartSlot.washingMachine != null)
                    {
                        poolAntBox.Add(washingMachineStartSlot.washingMachine);
                    }
                }

                poolAntBox.Shuffle();

                if (poolAntBox.Count == 0)
                {
                    break;
                }

                WashingMachineItem selectedAntBox = poolAntBox.First();

                selectedAntBox.Select();

                GamePersistentVariable.isAutoTestingLevel = true;

                yield return waitForSeconds;
            }
        }
    }
}
