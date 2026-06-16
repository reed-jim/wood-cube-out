using UnityEngine;

public class AutoFlickLightning : AutoTargetLazerBehaviour, IResize
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private AudioClip shotsound;
    [SerializeField] private Material[] lazerMaterials;
    private float alphaValue = 1f;
    private float xPosValue = 1f;
    private MaterialPropertyBlock _block;
    private float _lineSize;


    public void Resize(float size)
    {
        if (line == null)
            line = GetComponent<LineRenderer>();
        _lineSize = size;
        UpdateFromMaterialChange();
    }

    protected void OnEnable()
    {
        _timeSinceLastShoot = Time.time;
        alphaValue = 1.2f;
    }

    protected override void Start()
    {
        base.Start();
        _block = new MaterialPropertyBlock();
        Resize(0.75f);
    }

    protected override void UpdateObjectTexture(float range)
    {
    }

    protected override void UpdateObjectBoxCollider(float range)
    {
        //_collider.enabled = false;
    }

    protected override void Update()
    {
        base.Update();
        _currentTip = MaxRange();
    }

    protected override Transform CastTargetHit(float maxRange, out Vector2 position)
    {
        position = endTransform.position;
        return endTransform;
    }

    protected override void InteractHitObject(Transform hitObject, Vector3 point)
    {

        alphaValue = 1.2f;
        xPosValue = Random.Range(0, 5);
        _timeSinceLastShoot = Time.time;
        if (hitObject)
        {
            if (hitParticle != null)
            {
                var effect = SimplePool.Spawn(hitParticle.gameObject);
                effect.transform.position = point;
                effect.transform.parent = hitObject;
            }

            //var bird = hitObject.GetComponent<BirdBase>();
            //if(bird != null)
            //{
            //    bird.LightingHit();
            //}    
        }
        //if (shotsound != null)
        //    MusicManager.Instance.PlaySingleOnShotSource(shotsound);
    }

    protected override void HitCheck(Transform colTrans)
    {

    }

    public override void SetEffectSkinBullet(int skinIndex)
    {
        line.material = lazerMaterials[skinIndex];
        UpdateFromMaterialChange();
    }

    public override void UpdateFromMaterialChange()
    {
        base.UpdateFromMaterialChange();
        line.startWidth = line.endWidth = _lineSize;
    }
}

public enum LightningBoltAnimationMode
{
    /// <summary>
    /// No animation
    /// </summary>
    None,

    /// <summary>
    /// Pick a random frame
    /// </summary>
    Random,

    /// <summary>
    /// Loop through each frame and restart at the beginning
    /// </summary>
    Loop,

    /// <summary>
    /// Loop through each frame then go backwards to the beginning then forward, etc.
    /// </summary>
    PingPong
}