using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using UnityEngine;

namespace AntsFlow
{
    public class AutoplayManager : Singleton<AutoplayManager>
    {
        public float delayPerTap;

        public void StartAutoplay()
        {
            StartCoroutine(AutoplayLevel());
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
