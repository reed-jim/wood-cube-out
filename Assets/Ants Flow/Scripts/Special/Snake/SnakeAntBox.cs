using System;
using System.Collections.Generic;
using System.Linq;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace AntsFlow
{
    public class SnakeAntBox : WashingMachineItem
    {
        public override void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number, Action onCompleteAction = null)
        {
            base.Init(washingMachineStartSlot, faction, number, onCompleteAction);

            this.number = SpecialElementManager.Ins.snake.GetNextAntNumberOfBox();
            remainingNumber = this.number;

            numberText.text = $"{this.number}";
        }
    }
}
