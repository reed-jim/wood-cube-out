using TMPro;
using UnityEngine;

public class BaseRangedWeapon : MonoBehaviour
{
    public float bulletVelocity;
    public int totalRound;
    public int maxBulletPerRound;
    public int remainingBullet;

    public TMP_Text bulletText;

    private void Reload()
    {

    }
}
