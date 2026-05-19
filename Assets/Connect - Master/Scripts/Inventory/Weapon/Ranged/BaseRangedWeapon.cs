using TMPro;
using UnityEngine;

public class BaseRangedWeapon : MonoBehaviour
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
    }

    private void Reload()
    {

    }

    public void Shoot()
    {
        float currentTime = Time.time;

        if (currentTime > _lastShootTime + _delayPerRound)
        {
            // BaseBullet spawnedBullet = ObjectPoolingEverything.GetFromPool<BaseBullet>(bullet.name);
            BaseBullet spawnedBullet = Instantiate(bullet, transform);
            spawnedBullet.transform.position = head.transform.position;
            spawnedBullet.Shoot();

            remainingBullet--;
            bulletText.text = $"{remainingBullet}/{maxBulletPerRound}";

            _lastShootTime = currentTime;
        }
    }
}
