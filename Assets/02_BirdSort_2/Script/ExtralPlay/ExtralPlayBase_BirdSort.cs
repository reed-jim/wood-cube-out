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
        public int id;
        public ExtralPlayData data;
        public StateExtral state;
        protected IEnumerator m_ExtralSuccess;
        [SerializeField] protected GameObject body;
        public Sprite iconItem;
        public bool isReady;

        public virtual void Init(ExtralPlayData data, int id)
        {
            this.data = data;
            this.id = id;
            state = StateExtral.Appear;

            if (data.birdParent != null)
                data.birdParent.extrals.Add(this);

            if (data.standParent != null)
                data.standParent.extrals.Add(this);
        }

        public virtual void ApearExtral()
        {
            GamePlayController_BirdSort.Instance.returnController.IsLockReturn = true;
            PlayAnimAppear();
        }

        public void CheckExtral(BirdBase bird, StandingBase stand, bool isFlyOut)
        {
            if (state != StateExtral.Life)
                return;

            CheckExtralHandle(bird, stand, isFlyOut);
            if (state == StateExtral.Success)
            {
                // GamePlayController.Instance.gameScene.extralBtn.interactable = false;
            }

        }
        protected abstract void CheckExtralHandle(BirdBase bird, StandingBase stand, bool isFlyOut);

        public void ReturnExtral()
        {
            if (state != StateExtral.Life)
                return;

            ReturnExtralHandle();
        }
        protected abstract void ReturnExtralHandle();

        public abstract void ReAppearExtral(int idBird, int idStand);

        public virtual void EndExtral(bool isDone)
        {
            state = isDone ? StateExtral.Success : StateExtral.Fail;
        }

        public virtual void DestroyExtrals()
        {
            state = StateExtral.Success;
            PlayAnimEnd();
            // GamePlayController.Instance.gameScene.extralBtn.interactable = false;
        }

        protected virtual void OnUpdate()
        {

        }

        private void Update()
        {
            if (state != StateExtral.Life)
                return;

            OnUpdate();
        }

        protected virtual void PlayAnimAppear()
        {
            state = StateExtral.Life;
        }

        protected virtual void PlayAnimEnd()
        {
            GamePlayController_BirdSort.Instance.extralPlayController.CheckAllExtralSuccess();
        }

        protected virtual void OnDestroy()
        {
            if (m_ExtralSuccess != null)
                StopCoroutine(m_ExtralSuccess);

            StopAllCoroutines();
        }
        public abstract void ResetExtral();
    }
}