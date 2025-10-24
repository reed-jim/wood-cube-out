using NamPhuThuy;
using NamPhuThuy.GamePlay;
using NamPhuThuy.VFX;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

public class LockedObjectPart : BasicObjectPart
{
    [Header("Chain")]
    [SerializeField] private GameObject chainContainer;

    [Header("UI")]
    [SerializeField] private RectTransform uiContainer;
    [SerializeField] private int requiredValueToUnlock;
    
    public int RequiredValueToUnlock
    {
        get => requiredValueToUnlock;
        set
        {
            requiredValueToUnlock = value;
            SaferioTween.ScaleUpWithPunch(requiredValueText.transform, targetScale:1f, onCompleteAction:(() =>
            {
                requiredValueText.text = $"{requiredValueToUnlock}";
            }));
        }
    }

    [SerializeField] private TMP_Text requiredValueText;

   
    private BaseScrew[] _screws;

    protected override void MoreLogicOnAwake()
    {
        BaseScrew.screwLoosenedEvent += OnScrewLoosed;
    }

    protected override void MoreLogicOnDestroy()
    {
        BaseScrew.screwLoosenedEvent -= OnScrewLoosed;
    }

    private void Start()
    {
        Lock();
    }

    private void Lock()
    {
        _screws = TransformUtil.GetComponentArrayFromAllChildren<BaseScrew>(transform);

        foreach (var screw in _screws)
        {
            screw.GetComponent<MeshRenderer>().enabled = false;
        }

        _pieceMaterialPropertyBlock.SetCondition(PieceCondition.Locked);

        RequiredValueToUnlock = Random.Range(5, 10);
    }

    private void OnScrewLoosed(BaseScrew screw)
    {
        if (screw.IsInScrewPort)
        {
            DebugLogger.LogSimple(message:$"screw on port", context:this);
            return;
        }
        
        
        
        DebugLogger.LogSimple();
        RequiredValueToUnlock--;

        PlayVFX();

        if (requiredValueToUnlock == 0)
        {
            Tween.Scale(uiContainer, 1.1f, duration: 0.3f).OnComplete(() =>
            {
                Tween.Scale(uiContainer, 0f, duration: 0.3f)
                .OnComplete(() =>
                {
                    uiContainer.gameObject.SetActive(false);
                });
            });

            for (int i = 0; i < _screws.Length; i++)
            {
                _screws[i].GetComponent<MeshRenderer>().enabled = true;
            }

            _pieceMaterialPropertyBlock.SetCondition(PieceCondition.Normal);

            // Chains
            ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.BOX_COMPLETED_FX);

            fx.transform.position = chainContainer.transform.position;
            fx.Play();

            SaferioTween.ScaleDownWithPunch(chainContainer.transform, scaleUpVal: 1.5f);
        }
    }

    private void PlayVFX()
    {
        try
        {
            var targetTransform = requiredValueText != null ? requiredValueText.transform : transform;

            ParticleSystem looseScrewFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX);
            ParticleSystem ringFx = looseScrewFx.transform.GetChild(0).GetComponent<ParticleSystem>();

            var looseScrewFxMain = looseScrewFx.main;
            var ringFxMain = ringFx.main;

            Color factionColor = FactionUtility.GetColorForFaction(GameEnum.GameFaction.Blue);
            Color brighterFactionColor = ColorUtil.GetNearWhiteColor(factionColor);

            looseScrewFxMain.startColor = factionColor;
            ringFxMain.startColor = factionColor;
            ringFxMain.startSize = 1.2f * targetTransform.lossyScale.x;

            // Parent to the UI/text transform and position there
            looseScrewFx.gameObject.SetActive(true);
            looseScrewFx.transform.SetParent(targetTransform);
            looseScrewFx.transform.localRotation = Quaternion.identity;
            looseScrewFx.transform.localPosition = Vector3.zero + new Vector3(0, 0, -0.03f); // small offset in local space

            ringFx.transform.SetParent(targetTransform);
            ringFx.transform.localPosition = new Vector3(0, 0, -0.03f);
            ringFx.transform.localRotation = Quaternion.identity;

            looseScrewFx.Play();

            Tween.Delay(1).OnComplete(() =>
            {
                // keep ringFx as child of the particle root (restore original intent)
                ringFx.transform.SetParent(looseScrewFx.transform);
                ringFx.transform.localPosition = new Vector3(0, 0, -0.19f);
                ringFx.transform.localRotation = Quaternion.identity;
            });
        }
        catch (System.Exception e)
        {
            DebugLogger.LogSimple(message:$"Error: {e}", color:Color.red);
        }
    }

        
}
