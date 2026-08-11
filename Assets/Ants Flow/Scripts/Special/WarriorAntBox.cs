using System;
using System.Collections.Generic;
using System.Linq;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace AntsFlow
{
    public class WarriorAntBox : WashingMachineItem
    {
        public override void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number, Action onCompleteAction = null)
        {
            base.Init(washingMachineStartSlot, faction, number, onCompleteAction);

            this.number = SpecialElementManager.Ins.bossArea.GetNextAntNumberOfBox();
            remainingNumber = this.number;

            numberText.text = $"{this.number}";
        }
    }
}
