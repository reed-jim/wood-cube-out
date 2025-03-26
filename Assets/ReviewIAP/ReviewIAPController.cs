using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;

public class ReviewIAPController : MonoBehaviour
{
    [System.Serializable]
    public class ReviewData
    {
        public string shortID;
        public string IDIAP
        {
            get
            {
                return string.Format("{0}.{1}", Config.package_name, shortID);
            }
        }
        public string namePack;
        public Sprite iconPack;
        public TypePackIAP typePack;
    }

    public Button reviewBtn;

    public List<ReviewData> packs;
    [SerializeField] private ReviewIAPElement reviewElement;

    [Header("UI")]
    [SerializeField] private Button preBtn;
    [SerializeField] private Button nextBtn;
    [SerializeField] private GameObject content;
    [SerializeField] private Button buyBtn;
    [SerializeField] private Text priceTxt;

    private int currentIndex;

    private bool isInit;

    void Start()
    {
        GameUtils.AddHandler<GameEvent.OnIAPSucsses>(OnIAPSuccesEventReal);
        reviewBtn.gameObject.SetActive(false);
#if UNITY_ANDROID
        reviewBtn.gameObject.SetActive(false);
        reviewBtn.onClick.RemoveAllListeners();
        reviewBtn.onClick.AddListener(Show);
#else
       if (RemoteConfigController.GetIntConfig(FirebaseConfig.REVIEW_IAP_VERSION, Config.versionCode + 1) <= Config.versionCode)
        {
            reviewBtn.gameObject.SetActive(true);
            reviewBtn.onClick.RemoveAllListeners();
            reviewBtn.onClick.AddListener(Show);
        }
       else
        {
           reviewBtn.gameObject.SetActive(false);
        }
#endif
    }

    public void Show()
    {
        content.SetActive(true);
        Init();
        SetIndex(0);
    }

    public void Hide()
    {
        content.SetActive(false);
    }


    public void Init()
    {
        if (isInit)
            return;

        currentIndex = 0;

        if (packs == null)
        {
            return;
        }

        preBtn.onClick.RemoveAllListeners();
        preBtn.onClick.AddListener(PreviouHandle);
        nextBtn.onClick.RemoveAllListeners();
        nextBtn.onClick.AddListener(NextHandle);

        if (packs.Count <= 1)
        {
            preBtn.gameObject.SetActive(false);
            nextBtn.gameObject.SetActive(false);
        }
        else
        {
            preBtn.gameObject.SetActive(false);
            nextBtn.gameObject.SetActive(true);
        }

        if (packs.Count > 0)
        {
            var packData = GameController.Instance.iapController.inappDatabase.GetPack(packs[currentIndex].typePack);
            if (packData.productType == ProductType.NonConsumable)
            {
                if (GameController.Instance.iapController.inappDatabase.GetPack(packs[currentIndex].typePack).IsBought)
                {
                    buyBtn.interactable = false;
                }
                else
                {
                    reviewElement.Init(packs[currentIndex]);
                    SetBuy(packs[currentIndex].typePack);
                    buyBtn.interactable = true;
                }
            }
            else
            {
                reviewElement.Init(packs[currentIndex]);
                SetBuy(packs[currentIndex].typePack);
                buyBtn.interactable = true;
            }
        }

        isInit = true;
    }

    public void SetBuy(TypePackIAP typePack)
    {
        priceTxt.text = GameController.Instance.iapController.GetPrice(typePack);
        buyBtn.onClick.RemoveAllListeners();
        buyBtn.onClick.AddListener(() => GameController.Instance.iapController.BuyProduct(typePack));
    }

    public void SetIndex(int index)
    {
        currentIndex = index;
        reviewElement.Init(packs[currentIndex]);
        SetBuy(packs[currentIndex].typePack);
    }

    public void PreviouHandle()
    {
        if (currentIndex < 0)
            return;

        var index = currentIndex;

        SetIndex(index - 1);

        if (currentIndex <= 0)
        {
            preBtn.gameObject.SetActive(false);
            nextBtn.gameObject.SetActive(true);
        }
        else
        {
            preBtn.gameObject.SetActive(true);
            nextBtn.gameObject.SetActive(true);
        }

        // var packData = GameController

        var packData = GameController.Instance.iapController.inappDatabase.GetPack(packs[currentIndex].typePack);
        if (packData.productType == ProductType.NonConsumable)
        {
            var statePackage = GameController.Instance.iapController.inappDatabase.GetPack(packs[currentIndex].typePack).IsBought;
            if (statePackage)
            {
                buyBtn.interactable = false;
            }
            else
            {
                buyBtn.interactable = true;
            }
        }
        else
        {
            buyBtn.interactable = true;
        }
    }

    public void NextHandle()
    {
        int numPack = packs.Count;

        if (currentIndex >= numPack)
            return;

        var index = currentIndex;

        SetIndex(index + 1);

        if (currentIndex >= numPack - 1)
        {
            preBtn.gameObject.SetActive(true);
            nextBtn.gameObject.SetActive(false);
        }
        else
        {
            preBtn.gameObject.SetActive(true);
            nextBtn.gameObject.SetActive(true);
        }

        var packData = GameController.Instance.iapController.inappDatabase.GetPack(packs[currentIndex].typePack);
        if (packData.productType == ProductType.NonConsumable)
        {
            var statePackage = GameController.Instance.iapController.inappDatabase.GetPack(packs[currentIndex].typePack).IsBought;
            if (statePackage)
            {
                buyBtn.interactable = false;
            }
            else
            {
                buyBtn.interactable = true;
            }
        }
        else
        {
            buyBtn.interactable = true;
        }
    }

    protected void OnIAPSuccesEventReal(GameEvent.OnIAPSucsses obj)
    {
        var packData = GameController.Instance.iapController.inappDatabase.GetPack(packs[currentIndex].typePack);
        if (packData.productType == ProductType.NonConsumable)
        {
            if (packs != null && packs.Count > 0)
            {
                var statePackage = GameController.Instance.iapController.inappDatabase.GetPack(packs[currentIndex].typePack).IsBought;
                if (statePackage)
                {
                    buyBtn.interactable = false;
                }
                else
                {
                    buyBtn.interactable = true;
                }
            }
        }
        else
        {
            buyBtn.interactable = true;
        }
    }

    protected void OnDestroy()
    {
        GameUtils.RemoveHandler<GameEvent.OnIAPSucsses>(OnIAPSuccesEventReal);
    }
}
