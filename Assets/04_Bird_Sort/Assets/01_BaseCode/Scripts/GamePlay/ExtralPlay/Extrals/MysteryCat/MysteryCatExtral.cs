namespace BirdSort2
{
    using Spine;
    using Spine.Unity;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MysteryCatExtral : ExtralPlayBase_BirdSort
    {
        public const string DIE = "die";
        public const string FLY = "fly";// go up or equal
        public const string FLY_2 = "fly2"; // go down
        public const string GROUNDING = "grounding";
        public const string IDLE = "idle";
        public const string IDLE_2 = "idle2";
        public const string IDLE_3 = "idle3";
        public const string PRE_SLEEP = "pre-sleep";
        public const string SLEEP = "sleep";
        public const string SELECT = "select";
        public const string END = "end";
        [SerializeField] private SkeletonGraphic skeGrade;
        [SerializeField] protected AudioClip hidenCatClip;
        [SerializeField] protected AudioClip revealCatClip;
        [SerializeField] protected AudioSource audioMysterySource;

        private int remeberIDBirdParent;

        private int side;
        public override void Init(ExtralPlayData data, int id)
        {
            base.Init(data, id);
            data.birdParent.DisableBody();
            data.birdParent.isLockMove = true;
            remeberIDBirdParent = data.birdParent.id;
            data.birdParent.id = -5;
            side = data.standParent.side;
            skeGrade.AnimationState.ClearTracks();
            if (side != 1)
            {
                skeGrade.transform.localScale = new Vector3(1, 1, 1) * 1.15f;
            }
            else
            {
                skeGrade.transform.localScale = new Vector3(-1, 1, 1) * 1.15f;
            }

            var track = skeGrade.AnimationState.SetAnimation(0, FLY, true);
            //Skin newSkin = new Skin("new-skin"); // 1. Create a new empty skin
            //newSkin.AddSkin(skeGrade.SkeletonData.FindSkin(string.Format("skin-{0}", (int)data.extralValues[1] + 1))); // 2. Add items
            //skeGrade.Skeleton.SetSkin(newSkin); // 1. Set the active skin

            //body.gameObject.SetActive(false);
        }
        public override void ReAppearExtral(int idBird, int idStand)
        {

        }

        public override void ResetExtral()
        {
            Destroy(this.gameObject);
        }

        protected override void CheckExtralHandle(BirdBase bird, StandingBase stand, bool isFlyOut)
        {
            CheckExtral();
        }

        public void CheckExtral()
        {
            BirdBase bird = data.standParent.birdsOn.Pop();
            if (bird == data.birdParent)
            {
                state = StateExtral.Success;
                if (m_ExtralSuccess != null)
                    StopCoroutine(m_ExtralSuccess);
                m_ExtralSuccess = ExtralSuccessHandle();
                StartCoroutine(m_ExtralSuccess);

                GamePlayController_BirdSort.Instance.tutorialController.DoneTutExtral();
            }
            data.standParent.birdsOn.Push(bird);
        }
        private IEnumerator ExtralSuccessHandle()
        {
            yield return new WaitForSeconds(0.05f);
            PlayAnimEnd();
            //Post Event check lại StandSuccess cả map
        }
        protected override void ReturnExtralHandle()
        {
            //BirdBase bird = data.standParent.birdsOn.Pop();
            //if (bird == data.birdParent)
            //{
            //    state = StateExtral.Appear;
            //    data.birdParent.DisableBody();
            //    data.birdParent.isInStand = true;
            //    data.birdParent.isLockMove = true;
            //    remeberIDBirdParent = data.birdParent.id;
            //    data.birdParent.id = -5;
            //    side = data.standParent.sidePosStand;
            //    skeGrade.AnimationState.ClearTracks();
            //    var track = skeGrade.AnimationState.SetAnimation(0, side != 1 ? RIGHT_ANIM : LEFT_ANIM, true);
            //    body.gameObject.SetActive(true);
            //}
            //data.standParent.birdsOn.Push(bird);
        }

        protected override void PlayAnimAppear()
        {
            base.PlayAnimAppear();
            StartCoroutine(Helper.StartAction(() =>
            {
                body.gameObject.SetActive(true);
                skeGrade.AnimationState.SetAnimation(0, IDLE, true);
                GameController.Instance.musicManager.PlayOneShot(hidenCatClip, audioMysterySource);
                transform.SetAsFirstSibling();
                GamePlayController_BirdSort.Instance.tutorialController.ActiveTutExtral(this);
                GamePlayController_BirdSort.Instance.tutorialController.ActiveHintLockBird();
                GamePlayController_BirdSort.Instance.extralPlayController.InitDestroyExtralButton(data.type, this);
                isReady = true;
                data.birdParent.isInStand = true;
                GamePlayController_BirdSort.Instance.returnController.IsLockReturn = false;
            }, () => this.data.birdParent.onGround));
        }

        protected override void PlayAnimEnd()
        {
            //=> anim chìa khoá bay mở cửa
            skeGrade.AnimationState.ClearTracks();
            GameController.Instance.musicManager.PlayOneShot(revealCatClip, audioMysterySource);
            float scale = skeGrade.transform.localScale.y;
            if (side != 1)
            {
                skeGrade.transform.localScale = new Vector3(1, 1, 1) * scale;
            }
            else
            {
                skeGrade.transform.localScale = new Vector3(-1, 1, 1) * scale;
            }
            var track = skeGrade.AnimationState.SetAnimation(0,END , false);
            track.Complete += (s) =>
            {
                body.gameObject.SetActive(false);
                data.birdParent.id = remeberIDBirdParent;
                data.birdParent.ActiveBody();
                data.birdParent.isLockMove = false;
                StartCoroutine(Helper.StartAction(() => { Destroy(this); }, 0.5f));
            };
            base.PlayAnimEnd();
        }

        public override void DestroyExtrals()
        {
            base.DestroyExtrals();

        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (Input.GetMouseButtonDown(0))
                GamePlayController_BirdSort.Instance.tutorialController.DeActiveTutExtral(true);

            //if (this.data.birdParent.body == null || skeGrade == null) return;

            //// 1. Lấy thông tin track hiện tại của Object gốc (thường là Track 0)
            //var originalTrack = this.data.birdParent.body.AnimationState.GetCurrent(0);

            //if (originalTrack != null)
            //{
            //    // 2. Lấy thông tin track hiện tại của Object bắt chước
            //    var targetTrack = skeGrade.AnimationState.GetCurrent(0);

            //    // Kiểm tra nếu Object bắt chước đang chơi sai Animation hoặc đã kết thúc
            //    if (targetTrack == null || targetTrack.Animation != originalTrack.Animation)
            //    {
            //        skeGrade.AnimationState.SetAnimation(0, originalTrack.Animation, originalTrack.Loop);
            //        targetTrack = skeGrade.AnimationState.GetCurrent(0);
            //    }

            //    // 3. ĐỒNG BỘ QUAN TRỌNG: Ép thời gian của Target bằng đúng Original
            //    targetTrack.TrackTime = originalTrack.TrackTime;
            //}
            //else
            //{
            //    // Nếu gốc không có anim, Target cũng nên về trạng thái trống
            //    skeGrade.AnimationState.ClearTrack(0);
            //}
        }

    }

};
