using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;

public class BaseRangedWeapon : BaseWeapon
{
    public float bulletVelocity;
    public int totalRound;
    public int maxBulletPerRound;
    public int remainingBullet;
    public int roundPerSecond;

    [SerializeField] private Transform head;
    public BaseBullet bullet;

    public TMP_Text bulletText;

    private float _delayPerRound;
    private float _lastShootTime;

    void Awake()
    {
        remainingBullet = totalRound;
        _delayPerRound = 1f / roundPerSecond;

        StartCoroutine(Rotating());
    }

    private void Reload()
    {

    }

    public override void Attack(BaseCharacter attacker)
    {
        float currentTime = Time.time;

        if (currentTime > _lastShootTime + _delayPerRound)
        {
            // BaseBullet spawnedBullet = ObjectPoolingEverything.GetFromPool<BaseBullet>(bullet.name);
            BaseBullet spawnedBullet = Instantiate(bullet, transform);
            spawnedBullet.transform.position = head.transform.position;

            spawnedBullet.Shoot(transform.forward);

            remainingBullet--;
            bulletText.text = $"{remainingBullet}/{maxBulletPerRound}";

            _lastShootTime = currentTime;
        }
    }

    private IEnumerator Rotating()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 30);

        while (true)
        {
            Vector3 aimingPoint = GetAimingPoint();
            transform.LookAt(aimingPoint);

            yield return waitForSeconds;
        }
    }

    private Vector3 GetAimingPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        return targetPoint;
    }
}
