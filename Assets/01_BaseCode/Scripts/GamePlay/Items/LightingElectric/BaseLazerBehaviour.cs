using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LineRenderer))]
public abstract class BaseLazerBehaviour : MonoBehaviour
{
    public float offsetRange = 1;
    public float PixelPerUnit = 100f;
    public LayerMask layermask;
    public float speed = 0.5f;
    public float damage;
    public bool isFake;
    public bool isPiercing;
    public float tickRate = 0.5f;
    public float offsetSize;
    [SerializeField]
    protected Transform head;
    [SerializeField]
    public ParticleSystem hitParticle;
    protected LineRenderer line;
    protected float _cacheRange;

    protected Vector2 offsetVector;
    protected Vector2 _max;
    protected BoxCollider2D _collider;
    protected float _currentTip;
    protected float _timeSinceLastShoot;
    protected bool isTickDamage;

    protected float attackSpeedModifier = 0f;

    public Vector3 TestUp;

    [SerializeField] protected bool isInMiniShip;

    protected abstract void UpdateObjectTexture(float range);

    private void Awake()
    {
        //_collider = GetComponent<BoxCollider2D>();
        line = GetComponent<LineRenderer>();
        piercingSetup = isPiercing;
    }

    protected virtual void Start()
    {
        line.positionCount = 2;
        offsetVector = new Vector2(0, offsetRange);
        _max = Camera.main.ViewportToWorldPoint(GameUtils.VIEW_PORT_RIGHT_TOP);
        _currentTip = 0f;
        _timeSinceLastShoot = Time.time;
    }

    protected virtual void Update()
    {
        float maxRange = Range();
        var position = transform.position;
        TestUp = transform.up;
        var hit = Physics2D.Raycast(position, transform.up, maxRange, layermask); // transform.position + (transform.right * (float)offset) can be used for casting not from center.
        if (hit && !isPiercing)
        {
            line.SetPosition(0, position);
            line.SetPosition(1, hit.point + offsetVector);
            _cacheRange = Vector2.Distance(hit.point, position) + offsetVector.y;

            //			Collider2D collider = hit.collider;
            if (head != null)
            {
                head.gameObject.SetActive(true);
                head.position = hit.point + offsetVector;
            }
        }
        else
        {
            line.SetPosition(0, position);
            line.SetPosition(1, position + (transform.up * (maxRange + offsetVector.y))); // (transform.right * ((float)offset + range)) can be used for casting not from center.
            _cacheRange = maxRange + offsetVector.y;
            if (head != null)
            {
                head.gameObject.SetActive(false);
            }
        }
        InteractHitObject(hit.transform, hit.point);
        UpdateObjectTexture(_cacheRange);
        UpdateObjectBoxCollider(_cacheRange);

        _currentTip = _cacheRange;
        _currentTip += speed;

        isTickDamage = false;
    }

    public virtual void SetEffectSkinBullet(int skinIndex)
    { }

    protected virtual float Range()
    {
        return Mathf.Min((_max.y + 10f - transform.position.y) / Mathf.Cos(Mathf.Deg2Rad * Vector3.Angle(Vector3.up, transform.up)), _currentTip);

    }

    protected virtual float MaxRange()
    {
        return (_max.y - transform.position.y) / Mathf.Cos(Mathf.Deg2Rad * Vector3.Angle(Vector3.up, transform.up));
    }

    protected virtual void UpdateObjectBoxCollider(float range)
    {
        //_collider.size = new Vector2(_collider.size.x, range);
        //_collider.offset = new Vector2(_collider.offset.x, range / 2f);
    }

    protected virtual void InteractHitObject(Transform hitTransform, Vector3 point)
    {
    }

    protected void OnTriggerEnter2D(Collider2D c)
    {
        HitCheck(c.transform);
    }

    protected void OnTriggerEnter(Collider c)
    {
        HitCheck(c.transform);
    }

    protected void OnTriggerStay2D(Collider2D c)
    {
        if (c.gameObject.activeInHierarchy)
            HitCheck(c.transform);
    }

    protected virtual void HitCheck(Transform colTrans)
    {

        if ((layermask.value & (1 << colTrans.gameObject.layer)) == 0)
        {
            //  OnNotCollideWith(collider);
            return;
        }
        if (!isTickDamage)
            return;

        var heath = colTrans.GetComponent<BirdBase>();
        if (heath != null)
        {
            DealDamage(colTrans);
            if (head != null)
            {
                var effect = SimplePool.Spawn(hitParticle.gameObject);
                effect.transform.position = head.gameObject.activeInHierarchy ? head.position : colTrans.position;
                    effect.transform.parent = heath.transform;
            }
            if (!isPiercing)
                isTickDamage = false;
        }

    }

    protected void DealDamage(Transform healthObject)
    {
       
    }


    private bool piercingSetup;
    public void SetPiercing(bool isPiercing)
    {
        //Không thay đổi giá trị piercing nếu giá trị mặc định  = true
        if (piercingSetup)
            return;
        this.isPiercing = isPiercing;
    }

    public void SetSkinSprites(Sprite newSprite)
    {
        head.GetComponent<SpriteRenderer>().sprite = newSprite;
    }

    public void SetSkinMaterialsLazer(Material newMaterial)
    {
        line.material = newMaterial;
    }

    public void SetAttackSpeedModifierLevel(float attackSpeedModifier)
    {
        this.attackSpeedModifier = attackSpeedModifier;
    }
}