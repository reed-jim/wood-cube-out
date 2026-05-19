using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    public CharacterServiceLocator characterServiceLocator;

    public BaseRangedWeapon weapon;
    public DamageText damageText;
    [SerializeField] private LazyHealthBar lazyHealthBar;

    public TMP_Text[] stackingTexts;

    public List<InventoryItem> inventoryItems;
    public List<DamageInflicter> damageInflicters;
    public List<AugmentItem> augmentItems;

    public bool isInvisible;

    [SerializeField] private InventoryDataContainer inventoryDataContainer;

    public static event Action equipInventoryItemEvent;

    #region Properties
    public int attackTime;
    #endregion

    #region Life cycle
    void Awake()
    {
        LoadInventoryData();

        lazyHealthBar.gameObject.SetActive(false);

        StartCoroutine(Attacking());
    }

    void Update()
    {
        ApplyDamage();
    }

    void OnCollisionEnter(Collision collision)
    {
        DamageInflicter damageInflicter = collision.gameObject.GetComponent<DamageInflicter>();

        damageInflicters.Add(damageInflicter);

        DamageText spawnedDamageText = Instantiate(damageText, damageText.transform.parent);
        spawnedDamageText.gameObject.SetActive(true);
        spawnedDamageText.transform.position = collision.contacts[0].point;
        spawnedDamageText.Show(damageInflicter.damagePerCycle);

        collision.gameObject.SetActive(false);
    }
    #endregion

    #region Action
    private IEnumerator Attacking()
    {
        float interval = 1 / characterServiceLocator.characterStat.attackSpeed.currentValue;

        WaitForSeconds waitForSeconds = new WaitForSeconds(interval);

        while (true)
        {
            if (Input.GetMouseButton(0))
            {
                Attack();

                attackTime++;

                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    inventoryItems[i].SpecialEffect();
                }

                for (int i = 0; i < augmentItems.Count; i++)
                {
                    augmentItems[i].ApplySpecialEffect(this);
                }
            }

            yield return waitForSeconds;
        }
    }

    public void Attack()
    {
        DamageInflicter damageInflicter = new DamageInflicter();

        weapon.Shoot();
    }
    #endregion

    private void LoadInventoryData()
    {
        int[] inventoryItemIndexes = DataUtility.Load<int[]>(gameObject.name, null);

        if (inventoryItemIndexes == null)
        {
            return;
        }

        for (int i = 0; i < inventoryItemIndexes.Length; i++)
        {
            int index = inventoryItemIndexes[i];

            inventoryItems.Add(inventoryDataContainer.items[index]);
        }

        UpdateStats();

        equipInventoryItemEvent?.Invoke();
    }

    private void UpdateStats()
    {
        characterServiceLocator.characterStat.health.currentValue = inventoryItems.Sum(i => i.health.currentValue);
        characterServiceLocator.characterStat.armor.currentValue = inventoryItems.Sum(i => i.armor.currentValue);
    }

    public bool IsInventoryItemEquipped(InventoryItem inventoryItem)
    {
        return inventoryItems.Contains(inventoryItem);
    }

    public void EquipInventoryItem(InventoryItem inventoryItem)
    {
        inventoryItems.Add(inventoryItem);

        UpdateStats();

        DataUtility.Save(gameObject.name, inventoryItems.Select(i => i.id).ToArray());

        equipInventoryItemEvent?.Invoke();
    }

    public void UnequipInventoryItem(InventoryItem inventoryItem)
    {
        inventoryItems.Remove(inventoryItem);

        UpdateStats();

        DataUtility.Save(gameObject.name, inventoryItems.Select(i => i.id).ToArray());

        equipInventoryItemEvent?.Invoke();
    }

    private void ApplyDamage()
    {
        float currentTime = Time.time;

        for (int i = 0; i < damageInflicters.Count; i++)
        {
            DamageInflicter damageInflicter = damageInflicters[i];

            if (damageInflicter.IsNewCycleToApplyDamage(currentTime))
            {
                BaseStat health = characterServiceLocator.characterStat.health;

                health.currentValue -= damageInflicter.damagePerCycle;

                lazyHealthBar.SetProgress(health.currentValue / health.initialValue);

                damageInflicter.UpdateNewDamageCycle(currentTime);
            }
        }

        damageInflicters = damageInflicters.Where(item => !item.IsExpire()).ToList();
    }

    private float CalculateFinalDamage(float flatDamage, ArmorPenetration armorPenetration)
    {
        float effectiveArmor = (characterServiceLocator.characterStat.armor.currentValue - armorPenetration.flatAmount) * (1 - armorPenetration.percent);
        float finalDamage = flatDamage * (100 / (100 + effectiveArmor));

        return finalDamage;
    }

    private void ShowDamageStatus()
    {
        Dictionary<DamageType, int> stackingPerDamageType = new Dictionary<DamageType, int>();

        for (int i = 0; i < damageInflicters.Count; i++)
        {
            DamageInflicter damageInflicter = damageInflicters[i];

            CommonUtil.SumValueOfDictionary(stackingPerDamageType, damageInflicter.damageType, 1);
        }

        for (int i = 0; i < stackingTexts.Length; i++)
        {
            stackingTexts[i].text = $"{stackingPerDamageType.ElementAt(i)}";
        }
    }

    #region Augment
    private void ApplyAugments()
    {
        CharacterStat totalBonusStat = new CharacterStat();
        CharacterStat totalMultiplyStat = new CharacterStat();

        for (int i = 0; i < augmentItems.Count; i++)
        {
            AugmentItem augmentItem = augmentItems[i];

            if (!augmentItem.isApplied)
            {
                CharacterStat bonusStat = augmentItem.GetBonusStat();

                totalBonusStat += bonusStat;

                augmentItem.ApplySpecialEffect(this);
            }
        }

        characterServiceLocator.characterStat += totalBonusStat;
    }
    #endregion
}
