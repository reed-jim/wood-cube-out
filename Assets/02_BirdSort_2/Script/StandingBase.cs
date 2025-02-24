using System.Linq;

public enum TypeStand
{
    Slot_4 = 0,//Có 4 Slot
    Slot_5 = 1,//Có 5 Slot
    Slot_6 = 2,//Có 6 Slot
    Slot_7 = 3,//Có 7 Slot
    Slot_8 = 4//Có 8 Slot
}

namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using DG.Tweening;
   // using MoreMountains.NiceVibrations;
    using UnityEngine.UI;
    using Spine.Unity;
    using Spine;
    using System;
    using Sirenix.OdinInspector;
    using Random = UnityEngine.Random;
    [System.Serializable]
    public class StandData
    {
        public TypeStand type;
        public int numSlot;
    }



    public class StandingBase : SerializedMonoBehaviour
    {
        public StandData data;
        public List<SlotTran> slots;
        public Stack<BirdBase> birdsOn;
        public BirdBase[] birdsOnArray
        {
            get
            {
                return birdsOn.ToArray();
            }
        }

        public Transform parentBird;
        public Transform posTut;
        public int ID;
        [HideInInspector] public bool isTargetHint;
        public DOTweenAnimation animShake;
        public AudioClip completeStandClip;
        public bool isLockStand;
        public RectTransform rectTrans;

        public List<ExtralPlayBase_BirdSort> extrals;
        // public Image icon;
        public SkeletonGraphic icon;
        public Transform frontBird;
        public GameObject btnCick;
        //private SkeletonGraphic branchAnim;
        private BranchBase branchBase;
        private TrackEntry entryAnim;


        public int tempCompare;
        public List<DataBirdInBrand> tempCompare111;
        private BreakRuleController breakRuleController;
        public List<DataBirdInBrand> CompareBirdToId
        {

            get
            {


                tempCompare111 = new List<DataBirdInBrand>();

                if (birdsOnArray.Length > 0)
                {
                    for (int i = 0; i < birdsOnArray.Length; i++)
                    {
                        tempCompare111.Add(new DataBirdInBrand() { id = birdsOnArray[i].id, isLock = birdsOnArray[i].isLockMove });
                    }

                }

                //birdsOnArray[i].id

                return tempCompare111;

            }

        }
        public ParentOutlineStand parentOutlineStand;
        public Transform parent;


        public bool AllBirdIsInStand
        {
            get
            {
                bool temp = true;

                foreach (var item in birdsOnArray)
                {
                    //if (!item.isInStand)
                    //{
                    //    temp = false;
                    //}
                    if (!item.onGround)
                    {
                        temp = false;
                        break;
                    }
                }
                return temp;
            }
        }

        public bool isBlock;
        public bool isBreakBranchCode;

        public List<BirdBase> lsBirdNoExtra
        {
            get
            {
                var temp = new List<BirdBase>();
                foreach (var item in birdsOnArray)
                {
                    if (item.extrals.Count <= 0)
                    {
                        temp.Add(item);
                    }
                }
                return temp;

            }
        }





        public GameObject animBrand;
        public AudioSource audioSource;
        public AudioClip audioClipBreakBranch;
        public int side;

        public virtual IEnumerator Init(List<int> idBirds, int ID, float time = 0.5f)
        {
            isBreakBranchCode = false;
            isBlock = false;
            breakRuleController = GamePlayController_BirdSort.Instance.breakRuleController;
            baseAnchored = icon.GetComponent<RectTransform>().anchoredPosition;
            SetIcon();
            this.ID = ID;
            int numSlot = slots.Count;
            birdsOn = new Stack<BirdBase>();
            extrals = new List<ExtralPlayBase_BirdSort>();
            yield return new WaitForSeconds(time);
            for (int i = 0; i < idBirds.Count; i++)
            {

                int index = i;
                if (index > numSlot)
                    break;
                if (idBirds[index] > GamePlayController_BirdSort.Instance.level.birdsPrefab.Count)
                    yield return null;

                var birdPrefab = GamePlayController_BirdSort.Instance.level.GetBirdWithId(idBirds[index]);
                //var birdPrefab = GamePlayController_BirdSort.Instance.level.birdsPrefab[idBirds[index]];
                var bird = Instantiate(birdPrefab, GamePlayController_BirdSort.Instance.level.birdParent);

                bird.Init(slots[index].posStart.position, slots[index].posTrans.position, () =>
                {
                    bird.transform.SetParent(parentBird);
                    GamePlayController_BirdSort.Instance.level.CheckAllBirdReady();

                    if (index == idBirds.Count - 1)
                    {
                        Shake();
                    }
                });
                birdsOn.Push(bird);
            }
            GamePlayController_BirdSort.Instance.level.CheckAllStandInited();

        }

        /// <summary>
        /// Lấy ra các chim cuối hàng giống nhau
        /// </summary>
        /// <returns></returns>
        public virtual List<BirdBase> GetBirdSameEnd(bool isGetLockMove = true)
        {
            List<BirdBase> result = new List<BirdBase>();

            int numBrid = birdsOn.Count;
            var lst_birdsOn = birdsOn.ToArray();
            if (numBrid > 0)
            {
                var birdTop = birdsOn.Peek();
                int idBirdEnd = birdTop.id;
                if (isGetLockMove)
                    result.Add(birdTop);
                else
                {
                    if (!birdTop.isLockMove)
                        result.Add(birdTop);
                    else
                        return result;
                }
                for (int i = 1; i < lst_birdsOn.Length; i++)
                {
                    int index = i;
                    if (idBirdEnd == lst_birdsOn[index].id)
                    {
                        if (isGetLockMove)
                            result.Add(lst_birdsOn[index]);
                        else
                        {
                            if (!lst_birdsOn[index].isLockMove)
                                result.Add(lst_birdsOn[index]);
                            else
                                break;
                        }
                    }
                    else
                        break;
                }
            }




            return result;
        }

        public virtual void OnChoiceStand()
        {
            //Out Line các chim có thể bay
            List<BirdBase> birdsEnd = GetBirdSameEnd();
            if (birdsEnd.Count > 0)
            {
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.DISABLE_OUTLINE_BIRD);
                GamePlayController_BirdSort.Instance.level.standChosing = this;
                if (!isLockStand)
                {
                    for (int i = 0; i < birdsEnd.Count; i++)
                    {
                        birdsEnd[i].HightLight();
                    }
                }
            }

        }

        private bool IsFreeBrand()
        {
            return (!this.isLockStand && !this.isBreakBranchCode);
        }    

        public virtual IEnumerator OnCheckBirdChangeStand(StandingBase currentStand)
        {

            if (currentStand != null)
            {
                if (birdsOn.Count >= data.numSlot)
                {
                    //Chim đậu full slot
                    //Hiển thị lỗi
                    GamePlayController_BirdSort.Instance.level.standChosing = this;
                    OnChoiceStand();
                    GamePlayController_BirdSort.Instance.tutorialController.ActiveCanHint(this);
                    yield return null;
                }
                else
                {
                    float delayFly = 0.08f;

                    List<BirdBase> birdsEnd_currentStand = currentStand.GetBirdSameEnd(isGetLockMove: false);
                    int numBirdEnd_currentStand = birdsEnd_currentStand.Count;

                    List<BirdBase> birdsEnd_this = this.GetBirdSameEnd(isGetLockMove: false);
                    int numBirdEnd_this = birdsEnd_this.Count;

                    if (numBirdEnd_this > 0)//Cành this đang có chim
                    {
                        if (numBirdEnd_currentStand > 0)//Cành đang chọn có chim mang theo
                        {
                            if (data.numSlot > birdsOn.Count)//Cành this còn chỗ cho chim đậu
                            {
                                //if (IsFreeBrand())
                                {
                                    if (birdsEnd_this[0].id == -100 || birdsEnd_this[0].id == birdsEnd_currentStand[0].id)//2 con chim khớp nhau
                                    {
                                        //Đậu...
                                        int numSlotLeft = data.numSlot - birdsOn.Count;//Số slot còn lại chim có thể đậu
                                        int numBirdFly = 0;
                                        if (numBirdEnd_currentStand >= numSlotLeft)
                                        {
                                            numBirdFly = numSlotLeft;
                                        }
                                        else
                                        {
                                            numBirdFly = numBirdEnd_currentStand;
                                        }

                                        DataReturn dataReturn = new DataReturn();
                                        List<BirdBase> birdsFly = new List<BirdBase>();
                                        for (int i = 0; i < numBirdFly; i++)
                                        {
                                            int index = i;
                                            if (index >= numBirdEnd_currentStand)
                                                yield return null;

                                            var bird = currentStand.birdsOn.Pop();
                                            this.birdsOn.Push(bird);
                                            birdsFly.Add(bird);

                                            DataReturnModle retu = new DataReturnModle();
                                            retu.bird = bird;
                                            retu.oldStand = currentStand;
                                            retu.idOldSlot = currentStand.birdsOn.Count;
                                            retu.newStand = this;
                                            dataReturn.data.Add(retu);
                                        }
                                        GamePlayController_BirdSort.Instance.returnController.AddStack(dataReturn);
                                        GamePlayController_BirdSort.Instance.level.standChosing = null;
                                        GamePlayController_BirdSort.Instance.returnController.IsLockReturn = true;
                                        SetSaveStandSuccess();
                                        for (int i = 0; i < birdsFly.Count; i++)
                                        {
                                            int index = i;
                                            if (i == birdsFly.Count - 1)
                                                birdsFly[index].FlyCome(slots[data.numSlot - numSlotLeft + index].posTrans, () =>
                                                {
                                                    GamePlayController_BirdSort.Instance.level.isBirdFlying = false;
                                                //birdsFly[index].transform.SetParent(parentBird);
                                                for (int u = 0; u < birdsFly.Count; u++)
                                                    {
                                                        birdsFly[u].DisableOutLine();
                                                    }
                                                    StartCoroutine(CheckStandSuccess());
                                                }, this);
                                            else

                                                birdsFly[index].FlyCome(slots[data.numSlot - numSlotLeft + index].posTrans, () => {/* birdsFly[index].transform.SetParent(parentBird); */}, this);
                                            yield return new WaitForSeconds(delayFly);
                                        }
                                    }
                                    else
                                    {
                                        //Hiển thị lỗi không khớp
                                        //..
                                        ErrorSortHandle();
                                        yield return null;
                                    }
                                }
                            }
                            else
                            {
                                //Hiển thị lỗi full chỗ
                                //..
                                ErrorSortHandle();
                                yield return null;
                            }
                        }
                        else
                        {
                            //Hiển thị lỗi cành đang chọn không có chim
                            //..
                            ErrorSortHandle();
                            yield return null;
                        }
                    }
                    else
                    {
                        if (this.birdsOn.Count > 0)//Cành this có chim bị khoá
                        {
                            if (IsFreeBrand())
                            {
                                List<BirdBase> birds_c = currentStand.GetBirdSameEnd();
                                List<BirdBase> birds_t = this.GetBirdSameEnd();
                                bool isCanChangeStand = false;

                                if (birds_c.Count > 0 && birds_t.Count > 0)
                                {
                                    if (birds_t[0].id == -100 || birds_c[0].id == birds_t[0].id)
                                    {
                                        isCanChangeStand = true;
                                    }
                                }

                                if (isCanChangeStand)
                                {
                                    if (numBirdEnd_currentStand > 0)//Cành đang chọn có chim mang theo
                                    {
                                        int numLeftThis = data.numSlot - this.birdsOn.Count;
                                        if (numLeftThis >= numBirdEnd_currentStand)//Số chim ít mang đi hơn số slot còn lại của cành bị khoá
                                        {
                                            //Đậu hết lên cành
                                            //Đậu...
                                            DataReturn dataReturn = new DataReturn();
                                            List<BirdBase> birdsFly = new List<BirdBase>();
                                            for (int i = 0; i < numBirdEnd_currentStand; i++)
                                            {
                                                int index = i;
                                                var bird = currentStand.birdsOn.Pop();
                                                this.birdsOn.Push(bird);
                                                birdsFly.Add(bird);

                                                DataReturnModle retu = new DataReturnModle();
                                                retu.bird = bird;
                                                retu.oldStand = currentStand;
                                                retu.idOldSlot = currentStand.birdsOn.Count;
                                                retu.newStand = this;
                                                dataReturn.data.Add(retu);
                                            }
                                            GamePlayController_BirdSort.Instance.returnController.AddStack(dataReturn);
                                            GamePlayController_BirdSort.Instance.level.standChosing = null;
                                            GamePlayController_BirdSort.Instance.returnController.IsLockReturn = true;
                                            SetSaveStandSuccess();
                                            for (int i = 0; i < birdsFly.Count; i++)
                                            {
                                                int index = i;
                                                if (i == numBirdEnd_currentStand - 1)
                                                    birdsFly[index].FlyCome(slots[slots.Count - numLeftThis + index].posTrans, () =>
                                                    {
                                                        GamePlayController_BirdSort.Instance.level.isBirdFlying = false;
                                                    //birdsFly[index].transform.SetParent(parentBird);
                                                    for (int u = 0; u < birdsFly.Count; u++)
                                                        {
                                                            birdsFly[u].DisableOutLine();
                                                        }
                                                        StartCoroutine(CheckStandSuccess());
                                                    }, this);
                                                else
                                                    birdsFly[index].FlyCome(slots[slots.Count - numLeftThis + index].posTrans, () => { /*birdsFly[index].transform.SetParent(parentBird);*/ }, this);
                                                yield return new WaitForSeconds(delayFly);
                                            }
                                        }
                                        else//Ko đủ chỗ chứa hết chim => chỉ chứa tối đa là data.numSlot (con chim) --- Số chim nhiều hơn số slot
                                        {
                                            //Đậu...
                                            DataReturn dataReturn = new DataReturn();
                                            List<BirdBase> birdsFly = new List<BirdBase>();
                                            for (int i = 0; i < numLeftThis; i++)
                                            {
                                                int index = i;
                                                var bird = currentStand.birdsOn.Pop();
                                                this.birdsOn.Push(bird);
                                                birdsFly.Add(bird);

                                                DataReturnModle retu = new DataReturnModle();
                                                retu.bird = bird;
                                                retu.oldStand = currentStand;
                                                retu.idOldSlot = currentStand.birdsOn.Count;
                                                retu.newStand = this;
                                                dataReturn.data.Add(retu);
                                            }
                                            GamePlayController_BirdSort.Instance.returnController.AddStack(dataReturn);
                                            GamePlayController_BirdSort.Instance.level.standChosing = null;
                                            GamePlayController_BirdSort.Instance.returnController.IsLockReturn = true;
                                            SetSaveStandSuccess();
                                            for (int i = 0; i < birdsFly.Count; i++)
                                            {
                                                int index = i;
                                                if (i == numLeftThis - 1)
                                                    birdsFly[index].FlyCome(slots[slots.Count - numLeftThis + index].posTrans, () =>
                                                    {
                                                        GamePlayController_BirdSort.Instance.level.isBirdFlying = false;
                                                    //birdsFly[index].transform.SetParent(parentBird);
                                                    for (int u = 0; u < birdsFly.Count; u++)
                                                        {
                                                            birdsFly[u].DisableOutLine();
                                                        }
                                                        StartCoroutine(CheckStandSuccess());
                                                    }, this);
                                                else
                                                    birdsFly[index].FlyCome(slots[slots.Count - numLeftThis + index].posTrans, () => { /*birdsFly[index].transform.SetParent(parentBird); */}, this);
                                                yield return new WaitForSeconds(delayFly);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Hiển thị lỗi cành đang chọn không có chim
                                        //..
                                        ErrorSortHandle();
                                        yield return null;
                                    }
                                }
                                else
                                {
                                    ErrorSortHandle();
                                    yield return null;
                                }
                            }
                        }
                        else//Cành this đang ko có chim
                        {
                            if (IsFreeBrand())
                            {
                                if (numBirdEnd_currentStand > 0)//Cành đang chọn có chim mang theo
                                {
                                    if (data.numSlot >= numBirdEnd_currentStand)//Số chim ít hơn số slot
                                    {
                                        //Đậu hết lên cành
                                        //Đậu...
                                        DataReturn dataReturn = new DataReturn();
                                        List<BirdBase> birdsFly = new List<BirdBase>();
                                        for (int i = 0; i < numBirdEnd_currentStand; i++)
                                        {
                                            int index = i;
                                            var bird = currentStand.birdsOn.Pop();
                                            this.birdsOn.Push(bird);
                                            birdsFly.Add(bird);

                                            DataReturnModle retu = new DataReturnModle();
                                            retu.bird = bird;
                                            retu.oldStand = currentStand;
                                            retu.idOldSlot = currentStand.birdsOn.Count;
                                            retu.newStand = this;
                                            dataReturn.data.Add(retu);
                                        }
                                        GamePlayController_BirdSort.Instance.returnController.AddStack(dataReturn);
                                        GamePlayController_BirdSort.Instance.level.standChosing = null;
                                        GamePlayController_BirdSort.Instance.returnController.IsLockReturn = true;
                                        SetSaveStandSuccess();
                                        for (int i = 0; i < birdsFly.Count; i++)
                                        {
                                            int index = i;
                                            if (i == numBirdEnd_currentStand - 1)
                                                birdsFly[index].FlyCome(slots[index].posTrans, () =>
                                                {
                                                    GamePlayController_BirdSort.Instance.level.isBirdFlying = false;
                                                //birdsFly[index].transform.SetParent(parentBird);
                                                for (int u = 0; u < birdsFly.Count; u++)
                                                    {
                                                        birdsFly[u].DisableOutLine();
                                                    }
                                                    StartCoroutine(CheckStandSuccess());
                                                    GamePlayController_BirdSort.Instance.tutorialController.ActiveCanStandEmpty(this);
                                                }, this);
                                            else
                                                birdsFly[index].FlyCome(slots[index].posTrans, () => { /*birdsFly[index].transform.SetParent(parentBird);*/ }, this);
                                            yield return new WaitForSeconds(delayFly);
                                        }
                                    }
                                    else//Ko đủ chỗ chứa hết chim => chỉ chứa tối đa là data.numSlot (con chim) --- Số chim nhiều hơn số slot
                                    {
                                        //Đậu...
                                        DataReturn dataReturn = new DataReturn();
                                        List<BirdBase> birdsFly = new List<BirdBase>();
                                        for (int i = 0; i < data.numSlot; i++)
                                        {
                                            int index = i;
                                            var bird = currentStand.birdsOn.Pop();
                                            this.birdsOn.Push(bird);
                                            birdsFly.Add(bird);

                                            DataReturnModle retu = new DataReturnModle();
                                            retu.bird = bird;
                                            retu.oldStand = currentStand;
                                            retu.idOldSlot = currentStand.birdsOn.Count;
                                            retu.newStand = this;
                                            dataReturn.data.Add(retu);
                                        }
                                        GamePlayController_BirdSort.Instance.returnController.AddStack(dataReturn);
                                        GamePlayController_BirdSort.Instance.level.standChosing = null;
                                        GamePlayController_BirdSort.Instance.returnController.IsLockReturn = true;
                                        SetSaveStandSuccess();
                                        for (int i = 0; i < birdsFly.Count; i++)
                                        {
                                            int index = i;
                                            if (i == data.numSlot - 1)
                                                birdsFly[index].FlyCome(slots[index].posTrans, () =>
                                                {
                                                    GamePlayController_BirdSort.Instance.level.isBirdFlying = false;
                                                //birdsFly[index].transform.SetParent(parentBird);
                                                for (int u = 0; u < birdsFly.Count; u++)
                                                    {
                                                        birdsFly[u].DisableOutLine();
                                                    }
                                                    StartCoroutine(CheckStandSuccess());

                                                }, this);
                                            else
                                                birdsFly[index].FlyCome(slots[index].posTrans, () => { /*birdsFly[index].transform.SetParent(parentBird); */}, this);
                                            yield return new WaitForSeconds(delayFly);
                                        }
                                    }
                                }
                                else
                                {
                                    //Hiển thị lỗi cành đang chọn không có chim
                                    //..
                                    ErrorSortHandle();

                                    try
                                    {
                                        List<BirdBase> m_birdsEnd_currentStand = currentStand.GetBirdSameEnd();
                                        if (m_birdsEnd_currentStand.Count > 0)
                                        {
                                            if (m_birdsEnd_currentStand[0].isLockMove)
                                            {
                                                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.DISABLE_OUTLINE_BIRD);
                                            }
                                        }
                                    }
                                    catch
                                    {

                                    }
                                    yield return null;
                                }
                            }
                        }

                    }
                }
            }
        }

        public virtual void OnPointDown()
        {
            if (breakRuleController.isUseBooster)
            {
                BreakTheRuleItem_BirdSort it = (BreakTheRuleItem_BirdSort)GamePlayController_BirdSort.Instance.itemController.GetItem(GiftType.BreakTheRule_BirdSort);
                it.EndSuggetTut();
                if (breakRuleController.standCurrentClick != null)
                {
                    if (breakRuleController.standCurrentClick != this)
                    {
                        if (HandleActionBreakRuleBooster())
                        {
                            breakRuleController.OffOutLine();
                            return;
                        }
                        else
                        {
                            breakRuleController.standCurrentClick = this;
                            return;
                        }    
                            
                    }
                    else
                    {
                        breakRuleController.standCurrentClick = null;
                        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.DISABLE_OUTLINE_BIRD);
                    }    
                }
                else
                {
                    breakRuleController.standCurrentClick = this;
                    OnChoiceStandByBreakRuleBooster();
                    return;
                }
                return;
            }

            if (BirdSort2.GamePlayController_BirdSort.Instance.isTempPauseLevel)
                return;

            if (GamePlayController_BirdSort.Instance.state != StateGame.Playing)
                return;

            if (GamePlayController_BirdSort.Instance.level.isHasExtral)
            {

                if (!GamePlayController_BirdSort.Instance.extralPlayController.IsExtralsReady())
                    return;
            }

            var standChosing = GamePlayController_BirdSort.Instance.level.standChosing;

            if (standChosing != null)//Ấn vào 1 cành khác
            {

                if (!standChosing.isBreakBranchCode)
                {

                    if (standChosing != this)
                    {

                        if (!standChosing.isLockStand)
                        {
                            StartCoroutine(OnCheckBirdChangeStand(standChosing));
                            this.parent.transform.SetAsLastSibling();
                            this.transform.SetAsLastSibling();
                        }
                        else
                        {
                            GamePlayController_BirdSort.Instance.level.standChosing = null;
                        }    
                    }
                    else
                    {

                        var birdsEnd = GamePlayController_BirdSort.Instance.level.standChosing.GetBirdSameEnd();
                        if (birdsEnd.Count > 0)
                        {
                            for (int i = 0; i < birdsEnd.Count; i++)
                            {
                                birdsEnd[i].DisableOutLine();
                            }
                        }
                        GamePlayController_BirdSort.Instance.level.standChosing = null;
                        GamePlayController_BirdSort.Instance.tutorialController.ActiveCanHint(this);
                    }
                }
                else
                {
                    GamePlayController_BirdSort.Instance.level.standChosing = null;
                }    
            }
            else//Chọn Stand
            {

                OnChoiceStand();
                GamePlayController_BirdSort.Instance.tutorialController.ActiveTutGamePlay(this);
            }

            GamePlayController_BirdSort.Instance.tutorialController.OnClickStand(this);
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EVENT_CLICK_STANDING, this.ID);






        }

        public virtual void OnPointEnter()
        {
            //if (GamePlayController.Instance.level.isBirdFlying)
            //    return;
            //var standChosing = GamePlayController.Instance.level.standChosing;
            //if (standChosing != null)
            //{
            //    if (standChosing != this)//Kéo vào 1 cành khác
            //    {
            //        StartCoroutine(OnCheckBirdChangeStand(standChosing));
            //    }
            //}
            //  Debug.LogError("OnPointEnter");
        }

        public virtual bool AllBirdOnGround(BirdBase[] birdsOn)
        {
            for (int i = 0; i < birdsOn.Length; i++)
            {
                if (!birdsOn[i].onGround)
                    return false;
            }

            return true;
        }

        public virtual bool IsStandSuccess()
        {
            var lst_birdsOn = birdsOn.ToArray();
            bool isCanFly = true;
            if (lst_birdsOn != null)
            {
                int numBird = lst_birdsOn.Length;
                if (numBird >= data.numSlot)//Cành đã full chim
                {
                    int idBirdOn = -100;
                    for (int i = 0; i < numBird; i++)
                    {
                        if (idBirdOn < 0)
                        {
                            idBirdOn = lst_birdsOn[i].id;
                            continue;
                        }
                        if (idBirdOn != lst_birdsOn[i].id)
                        {
                            //Có 1 chim không giống trên cành
                            isCanFly = false;
                            break;
                        }
                        //if (lst_birdsOn[i].isLockMove)
                        //{
                        //    //Có 1 chim bị lock
                        //    isCanFly = false;
                        //    break;
                        //}
                    }
                }
                else
                    isCanFly = false;
            }
            return isCanFly;
        }

        public virtual void SetSaveStandSuccess()
        {
            bool isCanFly = IsStandSuccess();
            isLockStand = true;
            if (isCanFly)
            {
                //Tất cả chim trên cành đều giống nhau => bay đi
                var nearSave = GamePlayController_BirdSort.Instance.returnController.stackReturn.Pop();
                int numBirds = birdsOn.Count;
                int slot = numBirds - 1;
                var arrayBird = birdsOn.ToArray();
                for (int i = 0; i < numBirds; i++)
                {
                    int index = i;

                    var bird = arrayBird[index];
                    if (!GamePlayController_BirdSort.Instance.returnController.IsHasBirdInSaveElement(bird.gameObject.GetInstanceID(), nearSave))
                    {
                        DataReturnModle retu = new DataReturnModle();
                        retu.oldStand = this;
                        retu.idOldSlot = slot;
                        retu.newStand = null;
                        retu.bird = bird;
                        slot--;
                        nearSave.data.Add(retu);
                    }
                }
                GamePlayController_BirdSort.Instance.returnController.ResetReturn();
                // GamePlayController_BirdSort.Instance.returnController.AddStack(nearSave);
                if (numBirds > 0)
                    GamePlayController_BirdSort.Instance.extralPlayController.CheckExtrals(bird: arrayBird[0], stand: this, isFlyOut: true);
            }
            else
            {
                GamePlayController_BirdSort.Instance.extralPlayController.CheckExtrals(bird: null, stand: this, isFlyOut: false);
            }
        }

        /// <summary>
        /// Check cành cây đã full chim giống nhau => hoàn thành
        /// </summary>
        public virtual IEnumerator CheckStandSuccess()
        {

            if (!IsStandSuccess())
            {
                //  Debug.LogError("isLockStand");
                isLockStand = false;
            }
            else
            {
                isBreakBranchCode = true;
            }    
            //    Debug.LogError("CheckStandSuccess " + this.gameObject.name );
            GamePlayController_BirdSort.Instance.tutorialController.ActiveHintReturnBtn();

            yield return new WaitForSeconds(0.9f);
            var lst_birdsOn = birdsOn.ToArray();
            bool isCanFly = IsStandSuccess();
            if (isCanFly)
            {
                birdsOn.Clear();

                GamePlayController_BirdSort.Instance.returnController.IsLockReturn = true;
                StartCoroutine(Helper.StartAction(() =>
                {
                    StartCoroutine(AllBirdFly(lst_birdsOn));
                    GamePlayController_BirdSort.Instance.tutorialController.ActiveCanHint(this);
                    GamePlayController_BirdSort.Instance.scoreController.AddScore();

                    if (!isBlock)
                    {
                        btnCick.SetActive(false);
                        GamePlayController_BirdSort.Instance.returnController.ResetReturn();
                        icon.gameObject.SetActive(false);
                        var temp = GamePlayController_BirdSort.Instance.level;
                        for (int i = 0; i < temp.standsInLevel_Real.Count; i++)
                        {
                            if (temp.standsInLevel_Real[i].btnCick.activeSelf == false)
                            {
                                temp.standsInLevel[i].gameObject.SetActive(false);
                                animBrand.SetActive(true);
                                if (UseProfile.OnSound)
                                {
                                    GameController.Instance.musicManager.PlayOneShot(NameMusic.FlyOut, audioClipBreakBranch, audioSource);
                                }
                                if (UseProfile.OnVibration)
                                {
                                  //  MMVibrationManager.Haptic(HapticTypes.HeavyImpact);

                                }


                            }

                        }
                        isBlock = true;
                        StartCoroutine(RemoveBrand());
                    }



                }, () => AllBirdOnGround(lst_birdsOn)));

            }
            else
            {
                isLockStand = false;
            }    
            bool isWin = GamePlayController_BirdSort.Instance.level.IsWin();
            if (!isWin)
                GamePlayController_BirdSort.Instance.returnController.IsLockReturn = false;

        }



        public IEnumerator RemoveBrand()
        {
            yield return new WaitForSeconds(0.5f);
            var level = BirdSort2.GamePlayController_BirdSort.Instance.level;
            if (UseProfile.OnSound)
            {
                if (side > 0)
                {
                    if (!level.ListStandRightDone)
                    {
                        GameController.Instance.musicManager.PlayOneShot(NameMusic.BranchDrop, level.audioClipDrop, level.dropAudioRight);
                    }

                }
                else
                {
                    if (!level.ListStandLeftDone)
                    {
                        GameController.Instance.musicManager.PlayOneShot(NameMusic.BranchDrop, level.audioClipDrop,
                            level.dropAudioLeft);
                    }
                }
            }

            //Debug.Log("AAAA====================");
            StartCoroutine(GamePlayController_BirdSort.Instance.level.MoveBrand(0.2f));
        }

        public void RemoveBrandEndGame()
        {
            btnCick.SetActive(false);

            icon.gameObject.SetActive(false);
            var temp = GamePlayController_BirdSort.Instance.level;
            for (int i = 0; i < temp.standsInLevel_Real.Count; i++)
            {
                if (temp.standsInLevel_Real[i].btnCick.activeSelf == false)
                {
                    temp.standsInLevel[i].gameObject.SetActive(false);
                    animBrand.SetActive(true);
                    if (UseProfile.OnSound)
                    {
                        GameController.Instance.musicManager.PlayOneShot(NameMusic.FlyOut, audioClipBreakBranch, audioSource);
                    }
                    if (UseProfile.OnVibration)
                    {
                        //MMVibrationManager.Haptic(HapticTypes.HeavyImpact);

                    }


                }

            }

        }

        public void RemoveBrandEndGame2()
        {
            btnCick.SetActive(false);

            icon.gameObject.SetActive(false);

            if (this.btnCick.activeSelf == false)
            {
                // this.gameObject.SetActive(false);
                animBrand.SetActive(true);
                if (UseProfile.OnSound)
                {
                    GameController.Instance.musicManager.PlayOneShot(NameMusic.FlyOut, audioClipBreakBranch, audioSource);
                }
                if (UseProfile.OnVibration)
                {
                    //MMVibrationManager.Haptic(HapticTypes.HeavyImpact);

                }


            }

        }




        public virtual IEnumerator AllBirdFly(BirdBase[] lst_birdsOn)
        {
            int numBird = lst_birdsOn.Length;
            lst_birdsOn = Helper.DisruptiveList<BirdBase>(new List<BirdBase>(lst_birdsOn)).ToArray();

            for (int i = 0; i < lst_birdsOn.Length; i++)
            {
                int index = i;
                if (i == numBird - 1)
                    lst_birdsOn[index].FlyOut(() =>
                    {
                        GamePlayController_BirdSort.Instance.level.isBirdFlying = false;

                    }, true);
                else
                    lst_birdsOn[index].FlyOut(() => { });

                float timeRandom = Random.Range(0.04f, 0.08f);

                lst_birdsOn[index].HandleClaimItemChallenge();


                lst_birdsOn[i].transform.parent = GamePlayController_BirdSort.Instance.gameScene.birdParent;
                yield return new WaitForSeconds(timeRandom);
            }
            //isLockStand = false;
            bool isWin = GamePlayController_BirdSort.Instance.level.IsWin();

            if (!isWin)
                GamePlayController_BirdSort.Instance.returnController.IsLockReturn = false;

            //   GameController.Instance.musicManager.PlaySingle(completeStandClip, MusicManager.SourceAudio.Effect);
            //MMVibrationManager.Haptic(HapticTypes.Selection, false, true, this);
            if (isWin)
                GamePlayController_BirdSort.Instance.tutorialController.OnWinLevel();

            yield return new WaitForSeconds(1);
            GamePlayController_BirdSort.Instance.level.CheckPassLevel();
        }
        public virtual void Shake()
        {
            animShake.DOPlay();
            animShake.DOComplete(true);
            animShake.onComplete.RemoveAllListeners();
            animShake.onComplete.AddListener(() => { animShake.DORewind(); });
        }

        public virtual void ErrorSortHandle()
        {
            GamePlayController_BirdSort.Instance.level.standChosing = this;
            OnChoiceStand();
            GamePlayController_BirdSort.Instance.tutorialController.ActiveCanHint(this);
        }

        public virtual void BackTurn(BirdBase bird, bool isOldStand)
        {
            if (isOldStand)
            {
                birdsOn.Push(bird);
                bird.FlyCome(slots[birdsOn.Count - 1].posTrans, () =>
                {
                    GamePlayController_BirdSort.Instance.returnController.IsLockReturn = false;
                    //bird.transform.SetParent(parentBird);
                }, this);
            }
            else
            {
                if (birdsOn.Count > 0)
                    birdsOn.Pop();
            }
        }
        public Vector2 baseAnchored;
        public void SetIcon()
        {
            //if (icon.transform.childCount > 0)
            //{
            //    BranchBase branchBase = icon.transform.GetChild(0).GetComponent<BranchBase>();
            //    if (branchBase != null)
            //    {
            //        branchBase.SetParentBird(parentBird);
            //    }
            //}
            //foreach (Transform child in icon.transform)
            //{
            //    Destroy(child.gameObject);
            //}
            //if (GameController.Instance.useProfile.CurrentBranchSkin == 14)
            //{
            //    frontBird.gameObject.SetActive(true);
            //}
            //else
            //{
            //    frontBird.gameObject.SetActive(false);
            //}
            //BranchSkinData branch = GameController.Instance.dataContain.branchSkinDatabase.GetBranchSkinData(GameController.Instance.useProfile.CurrentBranchSkin);
            //if (data.type == TypeStand.Slot_4)
            //{
            //    if (branch.branch4.prefab != null)
            //    {

            //        GameObject newBranch = Instantiate(branch.branch4.prefab);
            //        newBranch.transform.SetParent(icon.transform);
            //        newBranch.transform.localScale = Vector3.one;
            //        newBranch.transform.localPosition = Vector3.zero;
            //        icon.enabled = false;
            //        icon.GetComponent<RectTransform>().anchoredPosition = (baseAnchored + branch.branch4.offset);

            //        branchBase = newBranch.GetComponent<BranchBase>();
            //        branchBase.InitPlay();
            //    }
            //    else
            //    {
            //        icon.sprite = branch.branch4.icon;
            //        icon.SetNativeSize();
            //        icon.transform.localScale = branch.branch4.scale;
            //        icon.GetComponent<RectTransform>().anchoredPosition = (baseAnchored + branch.branch4.offset);
            //        icon.enabled = true;

            //        branchBase = null;
            //    }
            //    //icon.GetComponent<RectTransform>().
            //}
            //else if (data.type == TypeStand.Slot_5)
            //{
            //    if (branch.branch5.prefab != null)
            //    {

            //        GameObject newBranch = Instantiate(branch.branch5.prefab);
            //        newBranch.transform.SetParent(icon.transform);
            //        newBranch.transform.localScale = Vector3.one;
            //        newBranch.transform.localPosition = Vector3.zero;
            //        icon.enabled = false;
            //        icon.GetComponent<RectTransform>().anchoredPosition = (baseAnchored + branch.branch5.offset);

            //        branchBase = newBranch.GetComponent<BranchBase>();
            //        branchBase.InitPlay();
            //    }
            //    else
            //    {
            //        icon.sprite = branch.branch5.icon;
            //        icon.SetNativeSize();
            //        icon.transform.localScale = branch.branch5.scale;
            //        icon.GetComponent<RectTransform>().anchoredPosition = (baseAnchored + branch.branch5.offset);
            //        icon.enabled = true;

            //        branchBase = null;

            //    }


            //}
            //else if (data.type == TypeStand.Slot_6)
            //{
            //    if (branch.branch6.prefab != null)
            //    {

            //        GameObject newBranch = Instantiate(branch.branch6.prefab);
            //        newBranch.transform.SetParent(icon.transform);
            //        newBranch.transform.localScale = Vector3.one;
            //        newBranch.transform.localPosition = Vector3.zero;
            //        icon.enabled = false;
            //        icon.GetComponent<RectTransform>().anchoredPosition = (baseAnchored + branch.branch6.offset);

            //        branchBase = newBranch.GetComponent<BranchBase>();
            //        branchBase.InitPlay();
            //    }
            //    else
            //    {
            //        icon.sprite = branch.branch6.icon;
            //        icon.SetNativeSize();
            //        icon.transform.localScale = branch.branch6.scale;
            //        icon.GetComponent<RectTransform>().anchoredPosition = (baseAnchored + branch.branch6.offset);
            //        icon.enabled = true;

            //        branchBase = null;

            //    }
            //}
        }
        public void HandleOnActionSuggetBooster()
        {
            if (icon.gameObject.activeSelf)
            {
                parentOutlineStand.OnOutLine();
            }

        }
        public void HandleOffActionSuggetBooster()
        {
            if (icon.gameObject.activeSelf)
            {
                parentOutlineStand.OffOutLine();
            }

        }

        #region BlockBrand
        public void HandleBlockBrand()
        {
            isBlock = true;
            icon.SetAnimation("animation", false);



        }

        #endregion

        #region BreakRuleBooster

        public bool HandleActionBreakRuleBooster()
        {
            if (GamePlayController_BirdSort.Instance.level.isHasExtral)
            {
                if (GamePlayController_BirdSort.Instance.state != StateGame.Playing)
                    return false;
                if (!GamePlayController_BirdSort.Instance.extralPlayController.IsExtralsReady())
                    return false;
            }

            bool isComplete = false;
            var standChosing = GamePlayController_BirdSort.Instance.level.standChosing;

            if (standChosing != null)//Ấn vào 1 cành khác
            {
                if (standChosing != this)
                {
                    if (!standChosing.isLockStand)
                    {
                        isComplete = OnCheckBirdChangeStandByBooster(standChosing);
                    }
                    if(isComplete )
                    {
                        breakRuleController.OffPanelAndBool();
                        GameController.Instance.dataContain.giftDatabase.Claim(GiftType.BreakTheRule_BirdSort, -1, Reason.in_campain_level);
                        breakRuleController.standCurrentClick = null;
                        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.BUYED_ITEM, GiftType.BreakTheRule_BirdSort);
                    }    
                    else
                    {
                        OnChoiceStandByBreakRuleBooster();
                    }
                }
                else
                {
                    var birdsEnd = GamePlayController_BirdSort.Instance.level.standChosing.GetBirdSameEnd();
                    if (birdsEnd.Count > 0)
                    {
                        for (int i = 0; i < birdsEnd.Count; i++)
                        {
                            birdsEnd[i].DisableOutLine();
                        }
                    }
                    GamePlayController_BirdSort.Instance.level.standChosing = null;
                    GamePlayController_BirdSort.Instance.tutorialController.ActiveCanHint(this);
                }
            
            }
            else//Chọn Stand
            {
                OnChoiceStandByBreakRuleBooster();
                GamePlayController_BirdSort.Instance.tutorialController.ActiveTutGamePlay(this);
         

            }

            GamePlayController_BirdSort.Instance.tutorialController.OnClickStand(this);
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EVENT_CLICK_STANDING, this.ID);


            return isComplete;
        }
        public virtual bool OnCheckBirdChangeStandByBooster(StandingBase currentStand)
        {

            if (currentStand != null)
            {
                if (birdsOn.Count >= data.numSlot)
                {
                    //Chim đậu full slot
                    //Hiển thị lỗi
                    GamePlayController_BirdSort.Instance.level.standChosing = this;
                    OnChoiceStand();
                    GamePlayController_BirdSort.Instance.tutorialController.ActiveCanHint(this);
                    return false;
                }
                else
                {
                    float delayFly = 0.08f;

                    List<BirdBase> birdsEnd_currentStand = currentStand.GetBirdSameEnd(isGetLockMove: false);
                    int numBirdEnd_currentStand = birdsEnd_currentStand.Count;
                    //BirdBase temp = birdsEnd_currentStand[0];
                    //birdsEnd_currentStand.Clear();
                    //birdsEnd_currentStand.Add(temp);

                    List<BirdBase> birdsEnd_this = this.GetBirdSameEnd(isGetLockMove: false);
                    int numBirdEnd_this = birdsEnd_this.Count;


                    if (numBirdEnd_this > 0)//Cành this đang có chim
                    {
                        if (numBirdEnd_currentStand > 0)//Cành đang chọn có chim mang theo
                        {
                            if (data.numSlot > birdsOn.Count)//Cành this còn chỗ cho chim đậu
                            {

                                //Đậu...
                                int numSlotLeft = data.numSlot - birdsOn.Count;//Số slot còn lại chim có thể đậu
                                int numBirdFly = 0;
                                if (numBirdEnd_currentStand >= numSlotLeft)
                                {
                                    numBirdFly = numSlotLeft;
                                }
                                else
                                {
                                    numBirdFly = numBirdEnd_currentStand;
                                }

                                DataReturn dataReturn = new DataReturn();
                                List<BirdBase> birdsFly = new List<BirdBase>();
                                for (int i = 0; i < numBirdFly; i++)
                                {
                                    int index = i;
                                    if (index >= numBirdEnd_currentStand)
                                        return false;

                                    var bird = currentStand.birdsOn.Pop();
                                    this.birdsOn.Push(bird);
                                    birdsFly.Add(bird);

                                    DataReturnModle retu = new DataReturnModle();
                                    retu.bird = bird;
                                    retu.oldStand = currentStand;
                                    retu.idOldSlot = currentStand.birdsOn.Count;
                                    retu.newStand = this;
                                    dataReturn.data.Add(retu);
                                }
                                //var birdTemp = birdsFly[0];
                                //birdsFly.Clear();
                                //birdsFly.Add(birdTemp);

                                GamePlayController_BirdSort.Instance.returnController.AddStack(dataReturn);
                                GamePlayController_BirdSort.Instance.level.standChosing = null;
                                GamePlayController_BirdSort.Instance.returnController.IsLockReturn = true;

                                SetSaveStandSuccess();
                                for (int i = 0; i < birdsFly.Count; i++)
                                {
                                    int index = i;
                                    if (i == birdsFly.Count - 1)
                                        birdsFly[index].FlyCome(slots[data.numSlot - numSlotLeft + index].posTrans, () =>
                                        {
                                            GamePlayController_BirdSort.Instance.level.isBirdFlying = false;
                                            //birdsFly[index].transform.SetParent(parentBird);
                                            for (int u = 0; u < birdsFly.Count; u++)
                                            {
                                                birdsFly[u].DisableOutLine();
                                            }
                                            StartCoroutine(CheckStandSuccess());
                                        }, this);
                                    else

                                        birdsFly[index].FlyCome(slots[data.numSlot - numSlotLeft + index].posTrans, () => {/* birdsFly[index].transform.SetParent(parentBird); */}, this);
                                    //yield return new WaitForSeconds(delayFly);
                                }


                            }
                            else
                            {
                                //Hiển thị lỗi full chỗ
                                //..
                                ErrorSortHandle();
                                return false;
                            }
                        }
                        else
                        {
                            //Hiển thị lỗi cành đang chọn không có chim
                            //..
                            ErrorSortHandle();
                            return false;
                        }
                    }
                    else
                    {
                        if (this.birdsOn.Count > 0)//Cành this có chim bị khoá
                        {
                            List<BirdBase> birds_c = currentStand.GetBirdSameEnd();
                            List<BirdBase> birds_t = this.GetBirdSameEnd();
                            bool isCanChangeStand = false;

                            if (birds_c.Count > 0 && birds_t.Count > 0)
                            {
                                if (birds_t[0].id == -100 || birds_c[0].id == birds_t[0].id)
                                {
                                    isCanChangeStand = true;
                                }
                            }

                            if (isCanChangeStand)
                            {
                                if (numBirdEnd_currentStand > 0)//Cành đang chọn có chim mang theo
                                {
                                    int numLeftThis = data.numSlot - this.birdsOn.Count;
                                    if (numLeftThis >= numBirdEnd_currentStand)//Số chim ít mang đi hơn số slot còn lại của cành bị khoá
                                    {
                                        //Đậu hết lên cành
                                        //Đậu...
                                        DataReturn dataReturn = new DataReturn();
                                        List<BirdBase> birdsFly = new List<BirdBase>();
                                        for (int i = 0; i < numBirdEnd_currentStand; i++)
                                        {
                                            int index = i;
                                            var bird = currentStand.birdsOn.Pop();
                                            this.birdsOn.Push(bird);
                                            birdsFly.Add(bird);

                                            DataReturnModle retu = new DataReturnModle();
                                            retu.bird = bird;
                                            retu.oldStand = currentStand;
                                            retu.idOldSlot = currentStand.birdsOn.Count;
                                            retu.newStand = this;
                                            dataReturn.data.Add(retu);
                                        }
                                        //var birdTemp = birdsFly[0];
                                        //birdsFly.Clear();
                                        //birdsFly.Add(birdTemp);
                                        GamePlayController_BirdSort.Instance.returnController.AddStack(dataReturn);
                                        GamePlayController_BirdSort.Instance.level.standChosing = null;
                                        GamePlayController_BirdSort.Instance.returnController.IsLockReturn = true;
                                        SetSaveStandSuccess();
                                        for (int i = 0; i < birdsFly.Count; i++)
                                        {
                                            int index = i;
                                            if (i == numBirdEnd_currentStand - 1)
                                                birdsFly[index].FlyCome(slots[slots.Count - numLeftThis + index].posTrans, () =>
                                                {
                                                    GamePlayController_BirdSort.Instance.level.isBirdFlying = false;
                                                    //birdsFly[index].transform.SetParent(parentBird);
                                                    for (int u = 0; u < birdsFly.Count; u++)
                                                    {
                                                        birdsFly[u].DisableOutLine();
                                                    }
                                                    StartCoroutine(CheckStandSuccess());
                                                }, this);
                                            else
                                                birdsFly[index].FlyCome(slots[slots.Count - numLeftThis + index].posTrans, () => { /*birdsFly[index].transform.SetParent(parentBird);*/ }, this);
                                            // yield return new WaitForSeconds(delayFly);
                                        }
                                    }
                                    else//Ko đủ chỗ chứa hết chim => chỉ chứa tối đa là data.numSlot (con chim) --- Số chim nhiều hơn số slot
                                    {
                                        //Đậu...
                                        DataReturn dataReturn = new DataReturn();
                                        List<BirdBase> birdsFly = new List<BirdBase>();
                                        for (int i = 0; i < numLeftThis; i++)
                                        {
                                            int index = i;
                                            var bird = currentStand.birdsOn.Pop();
                                            this.birdsOn.Push(bird);
                                            birdsFly.Add(bird);

                                            DataReturnModle retu = new DataReturnModle();
                                            retu.bird = bird;
                                            retu.oldStand = currentStand;
                                            retu.idOldSlot = currentStand.birdsOn.Count;
                                            retu.newStand = this;
                                            dataReturn.data.Add(retu);
                                        }
                                        //var birdTemp = birdsFly[0];
                                        //birdsFly.Clear();
                                        //birdsFly.Add(birdTemp);
                                        GamePlayController_BirdSort.Instance.returnController.AddStack(dataReturn);
                                        GamePlayController_BirdSort.Instance.level.standChosing = null;
                                        GamePlayController_BirdSort.Instance.returnController.IsLockReturn = true;
                                        SetSaveStandSuccess();
                                        for (int i = 0; i < birdsFly.Count; i++)
                                        {
                                            int index = i;
                                            if (i == numLeftThis - 1)
                                                birdsFly[index].FlyCome(slots[slots.Count - numLeftThis + index].posTrans, () =>
                                                {
                                                    GamePlayController_BirdSort.Instance.level.isBirdFlying = false;
                                                    //birdsFly[index].transform.SetParent(parentBird);
                                                    for (int u = 0; u < birdsFly.Count; u++)
                                                    {
                                                        birdsFly[u].DisableOutLine();
                                                    }
                                                    StartCoroutine(CheckStandSuccess());
                                                }, this);
                                            else
                                                birdsFly[index].FlyCome(slots[slots.Count - numLeftThis + index].posTrans, () => { /*birdsFly[index].transform.SetParent(parentBird); */}, this);
                                            //yield return new WaitForSeconds(delayFly);
                                        }
                                    }
                                }
                                else
                                {
                                    //Hiển thị lỗi cành đang chọn không có chim
                                    //..
                                    ErrorSortHandle();
                                    return false;
                                }
                            }
                            else
                            {
                                ErrorSortHandle();
                                return false;
                            }
                        }
                        else//Cành this đang ko có chim
                        {
                            if (numBirdEnd_currentStand > 0)//Cành đang chọn có chim mang theo
                            {
                                if (data.numSlot >= numBirdEnd_currentStand)//Số chim ít hơn số slot
                                {
                                    //Đậu hết lên cành
                                    //Đậu...
                                    DataReturn dataReturn = new DataReturn();
                                    List<BirdBase> birdsFly = new List<BirdBase>();
                                    for (int i = 0; i < numBirdEnd_currentStand; i++)
                                    {
                                        int index = i;
                                        var bird = currentStand.birdsOn.Pop();
                                        this.birdsOn.Push(bird);
                                        birdsFly.Add(bird);

                                        DataReturnModle retu = new DataReturnModle();
                                        retu.bird = bird;
                                        retu.oldStand = currentStand;
                                        retu.idOldSlot = currentStand.birdsOn.Count;
                                        retu.newStand = this;
                                        dataReturn.data.Add(retu);
                                    }
                                    //var birdTemp = birdsFly[0];
                                    //birdsFly.Clear();
                                    //birdsFly.Add(birdTemp);
                                    GamePlayController_BirdSort.Instance.returnController.AddStack(dataReturn);
                                    GamePlayController_BirdSort.Instance.level.standChosing = null;
                                    GamePlayController_BirdSort.Instance.returnController.IsLockReturn = true;
                                    SetSaveStandSuccess();
                                    StartCoroutine(CheckStandSuccess());

                                    for (int i = 0; i < birdsFly.Count; i++)
                                    {
                                        int index = i;
                                        if (i == numBirdEnd_currentStand - 1)
                                            birdsFly[index].FlyCome(slots[index].posTrans, () =>
                                            {
                                                GamePlayController_BirdSort.Instance.level.isBirdFlying = false;
                                                //birdsFly[index].transform.SetParent(parentBird);
                                                for (int u = 0; u < birdsFly.Count; u++)
                                                {
                                                    birdsFly[u].DisableOutLine();
                                                }


                                                GamePlayController_BirdSort.Instance.tutorialController.ActiveCanStandEmpty(this);
                                            }, this);
                                        else
                                            birdsFly[index].FlyCome(slots[index].posTrans, () => { /*birdsFly[index].transform.SetParent(parentBird);*/ }, this);
                                        //yield return new WaitForSeconds(delayFly);
                                    }
                                }
                                else//Ko đủ chỗ chứa hết chim => chỉ chứa tối đa là data.numSlot (con chim) --- Số chim nhiều hơn số slot
                                {
                                    //Đậu...
                                    DataReturn dataReturn = new DataReturn();
                                    List<BirdBase> birdsFly = new List<BirdBase>();
                                    for (int i = 0; i < data.numSlot; i++)
                                    {
                                        int index = i;
                                        var bird = currentStand.birdsOn.Pop();
                                        this.birdsOn.Push(bird);
                                        birdsFly.Add(bird);

                                        DataReturnModle retu = new DataReturnModle();
                                        retu.bird = bird;
                                        retu.oldStand = currentStand;
                                        retu.idOldSlot = currentStand.birdsOn.Count;
                                        retu.newStand = this;
                                        dataReturn.data.Add(retu);
                                    }
                                    //var birdTemp = birdsFly[0];
                                    //birdsFly.Clear();
                                    //birdsFly.Add(birdTemp);
                                    GamePlayController_BirdSort.Instance.returnController.AddStack(dataReturn);
                                    GamePlayController_BirdSort.Instance.level.standChosing = null;
                                    GamePlayController_BirdSort.Instance.returnController.IsLockReturn = true;
                                    SetSaveStandSuccess();
                                    Debug.LogError("SetSaveStandSuccess");
                                    for (int i = 0; i < birdsFly.Count; i++)
                                    {
                                        int index = i;
                                        if (i == data.numSlot - 1)
                                            birdsFly[index].FlyCome(slots[index].posTrans, () =>
                                            {
                                                GamePlayController_BirdSort.Instance.level.isBirdFlying = false;
                                                //birdsFly[index].transform.SetParent(parentBird);
                                                for (int u = 0; u < birdsFly.Count; u++)
                                                {
                                                    birdsFly[u].DisableOutLine();
                                                }
                                                StartCoroutine(CheckStandSuccess());
                                                Debug.LogError("CheckStandSuccess");

                                            }, this);
                                        else
                                            birdsFly[index].FlyCome(slots[index].posTrans, () => { /*birdsFly[index].transform.SetParent(parentBird); */}, this);
                                        //  yield return new WaitForSeconds(delayFly);
                                    }
                                }
                            }
                            else
                            {
                                //Hiển thị lỗi cành đang chọn không có chim
                                //..
                                ErrorSortHandle();

                                try
                                {
                                    List<BirdBase> m_birdsEnd_currentStand = currentStand.GetBirdSameEnd();
                                    if (m_birdsEnd_currentStand.Count > 0)
                                    {
                                        if (m_birdsEnd_currentStand[0].isLockMove)
                                        {
                                            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.DISABLE_OUTLINE_BIRD);
                                        }
                                    }
                                }
                                catch
                                {

                                }
                                return false;
                            }
                        }

                    }
                }
            }

            return true;
        }
        public virtual void OnChoiceStandByBreakRuleBooster()
        {
            //Out Line các chim có thể bay
            List<BirdBase> birdsEnd = GetBirdSameEnd();
            if (birdsEnd.Count > 0)
            {
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.DISABLE_OUTLINE_BIRD);
                GamePlayController_BirdSort.Instance.level.standChosing = this;
                foreach(var item in birdsEnd)
                {
                    item.DisableOutLine();
                }    
                if (!isLockStand)
                {
                    for (int i = 0; i < birdsEnd.Count; i++)
                    {
                        birdsEnd[i].HightLight();
                    }
                   
                }
            }

        }

        #endregion

        #region ShuffleBooster
        public bool isHasExtraInStandCantShuffle
        {
            get
            {
                bool temp = false;
                foreach (var item in birdsOnArray)
                {
                    if (item.extrals.Count > 0)
                    {
                        if (item.extrals[0].data.type == ExtralPlayType.EggLock)
                        {
                            temp = true;
                            break;
                        }

                        if (item.extrals[0].data.type == ExtralPlayType.SleepBird)
                        {
                            if (item.extrals[0].GetComponent<SleepBirdExtral>().birdsSleep.Contains(item))
                            {
                                temp = true;
                                break;
                            }

                        }

                    }
                }

                return temp;
            }
        }

        public void ShuffleStack()
        {

            if (isHasExtraInStandCantShuffle)
            {
                return;
            }
            List<BirdBase> lsbird = Helper.DisruptiveList(birdsOn.ToArray().ToList()); ;

            birdsOn.Clear();

            for (int i = 0; i < lsbird.Count; i++)
            {
                lsbird[i].FlyCome(slots[i].posTrans, () =>
                {
                    /*birdsFly[index].transform.SetParent(parentBird);*/

                }, this);
                birdsOn.Push(lsbird[i]);
            }
            // BirdBase[] array = birdsOn.ToArray();
            //
            // System.Random random = new System.Random();
            //
            // for (int i = array.Length - 1; i > 0; i--)
            // {
            //     int j = random.Next(i + 1);
            //     BirdBase temp = array[i];
            //     array[i] = array[j];
            //     array[j] = temp;
            // }
            // birdsOn.Clear();
            //
            // for (int i = 0; i < array.Length; i++)
            // {
            //     array[i].FlyCome(slots[i].posTrans, () =>
            //     {
            //         /*birdsFly[index].transform.SetParent(parentBird);*/
            //         
            //        }, this);
            //     birdsOn.Push(array[i]);
            // }




        }
        #endregion
    }
}