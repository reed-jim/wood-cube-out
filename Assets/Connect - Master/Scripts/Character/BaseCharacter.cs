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

    public TMP_Text[] stackingTexts;

    public List<InventoryItem> inventoryItems;
    public List<DamageInflicter> damageInflicters;

    [SerializeField] private InventoryDataContainer inventoryDataContainer;

    public static event Action equipInventoryItemEvent;

    #region Life cycle
    void Awake()
    {
        LoadInventoryData();
    }

    void Update()
    {
        ApplyDamage();
    }

    void OnCollisionEnter(Collision collision)
    {
        DamageInflicter damageInflicter = collision.gameObject.GetComponent<DamageInflicter>();

        damageInflicters.Add(damageInflicter);
    }
    #endregion

    #region Action
    private IEnumerator Attacking()
    {
        float interval = 1 / characterServiceLocator.characterStat.attackSpeed.currentValue;

        WaitForSeconds waitForSeconds = new WaitForSeconds(interval);

        int totalAttack = 0;

        while (true)
        {
            Attack();

            totalAttack++;

            if (totalAttack > 0 && totalAttack % 3 == 0)
            {
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    inventoryItems[i].SpecialEffect();
                }
            }

            yield return waitForSeconds;
        }
    }

    private void Attack()
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
                characterServiceLocator.characterStat.health.currentValue -= damageInflicter.damagePerCycle;

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
}
