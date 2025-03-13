namespace BirdSort2
{
    using System.Collections;
    using UnityEngine;

    public enum ExtralPlayType
    {
        None = 0,
        Boom = 1,
        LockBird = 2,
        EggLock = 3,
        SleepBird = 4,
        LockStand = 5
    }

    public enum ExtralPlayPosition
    {
        InBird = 0,
        InStand = 1,
        InGamePlay = 2
    }

    [System.Serializable]
    public class ExtralPositionData
    {
        public ExtralPlayPosition posType;
        public int indexStand;
        public int indexBird;
        public Vector3 localPosition;
    }

    [System.Serializable]
    public class ExtralPlayData
    {
        public ExtralPlayType type;
        public ExtralPositionData positionData;
        public float[] extralValues;


        public BirdBase birdParent;
        [HideInInspector] public StandingBase standParent;
    }

    public enum StateExtral
    {
        Appear = 0,
        Life = 1,
        Success = 2,
        Fail = 3
    }

    public abstract class ExtralPlayBase_BirdSort : MonoBehaviour
    {
        
    }
}