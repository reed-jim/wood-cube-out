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

            List<WashingMachineItem> poolAntBox = new List<WashingMachineItem>();

            while (true)
            {
                for (int i = 0; i < WashingMachineManager.Ins.washingMachineStartSlots.Count; i++)
                {
                    WashingMachineStartSlot washingMachineStartSlot = WashingMachineManager.Ins.washingMachineStartSlots[i];

                    if (washingMachineStartSlot.washingMachine != null)
                    {
                        poolAntBox.Add(washingMachineStartSlot.washingMachine);
                    }
                }

                poolAntBox.Shuffle();

                WashingMachineItem selectedAntBox = poolAntBox.First();

                selectedAntBox.Select();

                GamePersistentVariable.isAutoTestingLevel = true;

                yield return waitForSeconds;
            }
        }
    }
}
