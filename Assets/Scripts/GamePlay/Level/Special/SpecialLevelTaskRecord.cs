using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace NamPhuThuy.GamePlay
{
    [Serializable]
    public class SpecialLevelTaskRecord
    {
        public SpecialLevelTaskType taskType;
        public List<SpecialLevelTaskComponentAmount> requiredComponents;

        #region Generate Task

        public static SpecialLevelTaskRecord GenerateTask(SpecialLevelTaskType taskType)
        {
            var record = new SpecialLevelTaskRecord
            {
                taskType = taskType,
                requiredComponents = new List<SpecialLevelTaskComponentAmount>()
            };

            switch (taskType)
            {
                case SpecialLevelTaskType.ScrewComplete:
                    record.requiredComponents.Add(new SpecialLevelTaskComponentAmount
                    {
                        componentType = GameComponentType.Screw,
                        faction = GameEnum.GameFaction.Green,
                        amount = 3,
                        remainingAmount = 3
                    });
                    break;

                case SpecialLevelTaskType.ScrewBoxComplete:
                    record.requiredComponents.Add(new SpecialLevelTaskComponentAmount
                    {
                        componentType = GameComponentType.ScrewBox,
                        faction = GameEnum.GameFaction.Green,
                        amount = 1,
                        remainingAmount = 1
                    });
                    break;

                case SpecialLevelTaskType.ItemCollect:
                    record.requiredComponents.Add(new SpecialLevelTaskComponentAmount
                    {
                        componentType = GameComponentType.Item,
                        faction = GameEnum.GameFaction.Green,
                        amount = 1,
                        remainingAmount = 1
                    });
                    break;

                case SpecialLevelTaskType.None:
                default:
                    // Return empty record for None or invalid types
                    break;
            }

            return record;
        }

        #endregion

        public bool IsTaskCompleted()
        {
            return requiredComponents.All(item => item.remainingAmount <= 0);
        }
    }

    [Serializable]
    public class SpecialLevelTaskComponentAmount : GameComponentAmount
    {
        public int remainingAmount;

        public int Amount
        {
            set
            {
                amount = value;
                remainingAmount = amount;
            }
        }
    }

    [Serializable]
    public class GameComponentAmount
    {
        public GameComponentType componentType;
        public GameEnum.GameFaction faction;
        public int amount;
    }

    public enum SpecialLevelTaskType
    {
        None = 0,
        ScrewComplete = 1,
        ScrewBoxComplete = 2,
        ItemCollect = 3,
    }

    public enum GameComponentType
    {
        None = 0,
        Screw = 1,
        ScrewBox = 2,
        Item = 3,
    }
}