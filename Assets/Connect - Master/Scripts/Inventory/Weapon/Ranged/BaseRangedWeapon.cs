using TMPro;
using UnityEngine;

public class BaseRangedWeapon : MonoBehaviour
{
    public float bulletVelocity;
    public int totalRound;
    public int maxBulletPerRound;
    public int remainingBullet;

    [SerializeField] private Transform head;
    public BaseBullet bullet;

    public TMP_Text bulletText;

    void Awake()
    {
        remainingBullet = totalRound;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Reload()
    {

    }

    public void Shoot()
    {
        // BaseBullet spawnedBullet = ObjectPoolingEverything.GetFromPool<BaseBullet>(bullet.name);

        BaseBullet spawnedBullet = Instantiate(bullet, transform);

        spawnedBullet.transform.position = head.transform.position;

        spawnedBullet.Shoot();

        remainingBullet--;
        bulletText.text = $"{remainingBullet}/{maxBulletPerRound}";
    }
}
