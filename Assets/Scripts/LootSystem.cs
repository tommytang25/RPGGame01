using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LootItem
{
    public string itemId; // Reference to ItemDatabase
    public float dropChance; // 0.0 to 1.0
    public ItemRarity rarityOverride = ItemRarity.Common; // Optional override for item's default rarity
    public bool useRarityOverride = false;
}

[System.Serializable]
public class LootTable
{
    public string tableId;
    public string tableName;
    public List<LootItem> lootItems = new List<LootItem>();
}

public class LootSystem : MonoBehaviour
{
    // Singleton pattern
    public static LootSystem Instance { get; private set; }
    
    [Header("Loot Settings")]
    public float globalDropRateModifier = 1.0f;
    public float rarityModifier = 1.0f;
    public AnimationCurve luckCurve; // Curve to determine how luck affects drop rates
    
    [Header("Loot Display")]
    public GameObject worldItemPrefab; // Prefab for items in the world
    public GameObject lootPopupPrefab; // For showing loot that dropped
    public float lootPopupDuration = 3f;
    
    // Loot tables
    private Dictionary<string, LootTable> lootTables = new Dictionary<string, LootTable>();
    
    // Temporary boost timers
    private float dropRateBoostTimeRemaining = 0f;
    private float dropRateBoostMultiplier = 1f;
    private float rarityBoostTimeRemaining = 0f;
    private float rarityBoostMultiplier = 1f;
    
    private void Awake()
    {
        // Singleton implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Default luck curve if none is set
        if (luckCurve.keys.Length == 0)
        {
            // Create a curve where higher luck gives better chances
            Keyframe[] keys = new Keyframe[3]
            {
                new Keyframe(0, 1), // 0 luck = normal drop rate
                new Keyframe(10, 1.5f), // 10 luck = 50% more drops
                new Keyframe(20, 2f) // 20 luck = double drops
            };
            luckCurve = new AnimationCurve(keys);
        }
    }
    
    private void Start()
    {
        // Wait until ItemDatabase is initialized before creating loot tables
        if (ItemDatabase.Instance != null)
        {
            InitializeLootTables();
        }
        else
        {
            StartCoroutine(WaitForItemDatabase());
        }
    }
    
    private IEnumerator WaitForItemDatabase()
    {
        while (ItemDatabase.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        InitializeLootTables();
    }
    
    private void Update()
    {
        // Handle temporary boosts
        if (dropRateBoostTimeRemaining > 0)
        {
            dropRateBoostTimeRemaining -= Time.deltaTime;
            if (dropRateBoostTimeRemaining <= 0)
            {
                dropRateBoostMultiplier = 1f;
            }
        }
        
        if (rarityBoostTimeRemaining > 0)
        {
            rarityBoostTimeRemaining -= Time.deltaTime;
            if (rarityBoostTimeRemaining <= 0)
            {
                rarityBoostMultiplier = 1f;
            }
        }
    }
    
    private void InitializeLootTables()
    {
        // Create common monster loot table
        LootTable commonMonsterTable = new LootTable
        {
            tableId = "monster_common",
            tableName = "Common Monster Drops",
            lootItems = new List<LootItem>
            {
                new LootItem { itemId = "potion_health_small", dropChance = 0.2f },
                new LootItem { itemId = "ore_iron", dropChance = 0.3f },
                new LootItem { itemId = "wood_oak", dropChance = 0.3f }
            }
        };
        
        // Create uncommon monster loot table
        LootTable uncommonMonsterTable = new LootTable
        {
            tableId = "monster_uncommon",
            tableName = "Uncommon Monster Drops",
            lootItems = new List<LootItem>
            {
                new LootItem { itemId = "potion_health_small", dropChance = 0.3f },
                new LootItem { itemId = "ore_iron", dropChance = 0.4f },
                new LootItem { itemId = "ore_silver", dropChance = 0.2f },
                new LootItem { itemId = "herb_healing", dropChance = 0.3f }
            }
        };
        
        // Create rare monster loot table
        LootTable rareMonsterTable = new LootTable
        {
            tableId = "monster_rare",
            tableName = "Rare Monster Drops",
            lootItems = new List<LootItem>
            {
                new LootItem { itemId = "potion_health_medium", dropChance = 0.4f },
                new LootItem { itemId = "ore_gold", dropChance = 0.3f },
                new LootItem { itemId = "crystal_small", dropChance = 0.2f },
                new LootItem { itemId = "ring_dexterity", dropChance = 0.1f }
            }
        };
        
        // Create elite monster loot table
        LootTable eliteMonsterTable = new LootTable
        {
            tableId = "monster_elite",
            tableName = "Elite Monster Drops",
            lootItems = new List<LootItem>
            {
                new LootItem { itemId = "potion_health_large", dropChance = 0.5f },
                new LootItem { itemId = "crystal_medium", dropChance = 0.3f },
                new LootItem { itemId = "ore_platinum", dropChance = 0.2f },
                new LootItem { itemId = "amulet_protection", dropChance = 0.15f, useRarityOverride = true, rarityOverride = ItemRarity.Rare }
            }
        };
        
        // Create boss loot table
        LootTable bossTable = new LootTable
        {
            tableId = "monster_boss",
            tableName = "Boss Drops",
            lootItems = new List<LootItem>
            {
                new LootItem { itemId = "potion_strength", dropChance = 0.6f },
                new LootItem { itemId = "crystal_large", dropChance = 0.4f },
                new LootItem { itemId = "ore_mithril", dropChance = 0.3f },
                new LootItem { itemId = "staff_arcane", dropChance = 0.2f, useRarityOverride = true, rarityOverride = ItemRarity.Epic },
                new LootItem { itemId = "helm_dragon", dropChance = 0.1f, useRarityOverride = true, rarityOverride = ItemRarity.Legendary }
            }
        };
        
        // Register the loot tables
        RegisterLootTable(commonMonsterTable);
        RegisterLootTable(uncommonMonsterTable);
        RegisterLootTable(rareMonsterTable);
        RegisterLootTable(eliteMonsterTable);
        RegisterLootTable(bossTable);
        
        Debug.Log($"Loot System initialized with {lootTables.Count} loot tables");
    }
    
    public void RegisterLootTable(LootTable table)
    {
        if (!lootTables.ContainsKey(table.tableId))
        {
            lootTables.Add(table.tableId, table);
        }
        else
        {
            Debug.LogWarning($"Loot table with ID {table.tableId} already exists!");
        }
    }
    
    // Legacy method for backward compatibility
    public void GenerateLoot(string enemyType, Vector3 dropPosition, int playerLuck = 0)
    {
        if (!lootTables.ContainsKey(enemyType))
        {
            Debug.LogWarning($"No loot table found for enemy type: {enemyType}");
            return;
        }
        
        // Make sure ItemDatabase is available
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("ItemDatabase not found! Cannot generate loot.");
            return;
        }
        
        LootTable table = lootTables[enemyType];
        
        // Calculate luck bonus using the curve
        float luckMultiplier = luckCurve.Evaluate(playerLuck);
        
        // Apply temporary drop rate boost if active
        float finalDropRateModifier = globalDropRateModifier * dropRateBoostMultiplier * luckMultiplier;
        
        List<Item> droppedItems = new List<Item>();
        
        foreach (LootItem lootItem in table.lootItems)
        {
            // Calculate final drop chance
            float finalDropChance = lootItem.dropChance * finalDropRateModifier;
            
            // Roll for loot
            if (Random.value <= finalDropChance)
            {
                // Create the item and add to world
                Item item = CreateItemFromLootItem(lootItem);
                if (item != null)
                {
                    droppedItems.Add(item);
                    SpawnItemInWorld(item, dropPosition);
                }
            }
        }
        
        // Show loot popup if items were dropped
        if (droppedItems.Count > 0)
        {
            ShowLootPopup(droppedItems, dropPosition);
        }
    }
    
    private Item CreateItemFromLootItem(LootItem lootItem)
    {
        // Get item from database
        Item itemTemplate = ItemDatabase.Instance.GetItemById(lootItem.itemId);
        if (itemTemplate == null)
        {
            Debug.LogWarning($"Item with ID '{lootItem.itemId}' not found in database!");
            return null;
        }
        
        // Clone the item
        Item newItem = itemTemplate.Clone();
        
        // Apply rarity override if specified
        if (lootItem.useRarityOverride)
        {
            newItem.rarity = lootItem.rarityOverride;
        }
        else
        {
            // Calculate rarity chances - potentially upgrade rarity based on luck and boosts
            float rarityUpgradeChance = 0.05f * rarityModifier * rarityBoostMultiplier;
            
            // Chance to upgrade rarity by one level
            if (Random.value < rarityUpgradeChance)
            {
                UpgradeItemRarity(newItem);
            }
        }
        
        // Recalculate the item's value based on final rarity
        newItem.value = newItem.CalculateValue();
        
        return newItem;
    }
    
    // Method for generating loot based on monster rarity and level
    public void GenerateLootAtPosition(Vector3 position, MonsterRarity rarity, int monsterLevel, int playerLuck = 0)
    {
        // Make sure ItemDatabase is available
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("ItemDatabase not found! Cannot generate loot.");
            return;
        }
        
        List<Item> droppedItems = new List<Item>();
        
        // Get the appropriate loot table based on monster rarity
        string tableId = GetLootTableForRarity(rarity);
        
        if (lootTables.ContainsKey(tableId))
        {
            LootTable table = lootTables[tableId];
            
            // Calculate final drop rate modifier
            float finalDropModifier = CalculateDropRateModifier(playerLuck, monsterLevel);
            
            foreach (LootItem lootItem in table.lootItems)
            {
                // Calculate final drop chance
                float finalDropChance = lootItem.dropChance * finalDropModifier;
                
                // Roll for loot
                if (Random.value <= finalDropChance)
                {
                    // Create the item and add to world
                    Item item = CreateItemFromLootItem(lootItem);
                    if (item != null)
                    {
                        // Scale item rarity based on monster level
                        TryUpgradeItemBasedOnLevel(item, monsterLevel);
                        
                        droppedItems.Add(item);
                        SpawnItemInWorld(item, position);
                    }
                }
            }
            
            // Always try to add a level-appropriate resource drop
            TryAddResourceDrop(droppedItems, position, monsterLevel);
        }
        else
        {
            // Fallback if no specific table exists
            GenerateGenericLoot(droppedItems, position, rarity, monsterLevel);
        }
        
        // Show loot popup if items were dropped
        if (droppedItems.Count > 0)
        {
            ShowLootPopup(droppedItems, position);
        }
    }
    
    // Helper function to calculate drop rate modifier
    private float CalculateDropRateModifier(int playerLuck, int monsterLevel)
    {
        // Calculate luck bonus using the curve
        float luckMultiplier = luckCurve.Evaluate(playerLuck);
        
        // Apply temporary drop rate boost if active
        float dropRateModifier = globalDropRateModifier * dropRateBoostMultiplier * luckMultiplier;
        
        // Apply level-based modifier (higher level = better drops)
        float levelModifier = 1f + (monsterLevel - 1) * 0.05f; // 5% better drops per level
        
        return dropRateModifier * levelModifier;
    }
    
    // Try to upgrade item rarity based on monster level
    private void TryUpgradeItemBasedOnLevel(Item item, int monsterLevel)
    {
        // Higher level monsters have a chance to drop higher rarity items
        if (monsterLevel > 10)
        {
            float upgradeChance = 0.05f * (monsterLevel / 10f);
            if (Random.value < upgradeChance)
            {
                UpgradeItemRarity(item);
            }
        }
        
        if (monsterLevel > 20)
        {
            float doubleUpgradeChance = 0.02f * (monsterLevel / 20f);
            if (Random.value < doubleUpgradeChance)
            {
                UpgradeItemRarity(item);
            }
        }
    }
    
    // Create a world item from an Item object
    public void SpawnItemInWorld(Item item, Vector3 position)
    {
        if (item == null) return;
        
        // Add some randomness to position so items don't stack perfectly
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f),
            0
        );
        
        Vector3 spawnPosition = position + randomOffset;
        
        if (worldItemPrefab != null)
        {
            // Instantiate world item
            GameObject worldItem = Instantiate(worldItemPrefab, spawnPosition, Quaternion.identity);
            
            // Set the item data
            WorldItem worldItemComponent = worldItem.GetComponent<WorldItem>();
            if (worldItemComponent != null)
            {
                worldItemComponent.SetItem(item);
            }
            else
            {
                Debug.LogWarning("WorldItem component not found on worldItemPrefab!");
                Destroy(worldItem);
            }
        }
        else
        {
            // If no prefab, just add to inventory directly (temporary solution)
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem(item);
            }
        }
    }
    
    private void ShowLootPopup(List<Item> items, Vector3 position)
    {
        if (lootPopupPrefab != null && items.Count > 0)
        {
            // In a real implementation, you'd instantiate the popup and populate it with items
            GameObject popup = Instantiate(lootPopupPrefab, position + Vector3.up, Quaternion.identity);
            LootPopup lootPopup = popup.GetComponent<LootPopup>();
            
            if (lootPopup != null)
            {
                lootPopup.SetItems(items);
                Destroy(popup, lootPopupDuration);
            }
            else
            {
                Debug.LogWarning("LootPopup component not found on lootPopupPrefab!");
                Destroy(popup);
            }
        }
    }
    
    // Methods for temporary boosts - can be activated from items, quests, etc.
    public void TemporaryDropRateBoost(float multiplier, float duration)
    {
        dropRateBoostMultiplier = multiplier;
        dropRateBoostTimeRemaining = duration;
        Debug.Log($"Drop rate boosted by {multiplier}x for {duration} seconds!");
    }
    
    public void TemporaryRarityBoost(float multiplier, float duration)
    {
        rarityBoostMultiplier = multiplier;
        rarityBoostTimeRemaining = duration;
        Debug.Log($"Rarity chances boosted by {multiplier}x for {duration} seconds!");
    }
    
    // Helper to get appropriate loot table based on monster rarity
    private string GetLootTableForRarity(MonsterRarity rarity)
    {
        switch (rarity)
        {
            case MonsterRarity.Common:
                return "monster_common";
            case MonsterRarity.Uncommon:
                return "monster_uncommon";
            case MonsterRarity.Rare:
                return "monster_rare";
            case MonsterRarity.Elite:
                return "monster_elite";
            case MonsterRarity.Boss:
                return "monster_boss";
            default:
                return "monster_common";
        }
    }
    
    // Generate generic loot for monsters without specific loot tables
    private void GenerateGenericLoot(List<Item> droppedItems, Vector3 position, MonsterRarity rarity, int monsterLevel)
    {
        if (ItemDatabase.Instance == null) return;
        
        // Define base chances and expected number of drops based on rarity
        float resourceChance, equipmentChance, consumableChance;
        int minDrops, maxDrops;
        
        SetLootChancesForRarity(rarity, out resourceChance, out equipmentChance, out consumableChance, out minDrops, out maxDrops);
        
        // Scale by level
        float levelMultiplier = 1f + (monsterLevel - 1) * 0.03f;
        resourceChance *= levelMultiplier;
        equipmentChance *= levelMultiplier;
        consumableChance *= levelMultiplier;
        
        // Determine number of drops
        int numDrops = Random.Range(minDrops, maxDrops + 1);
        
        for (int i = 0; i < numDrops; i++)
        {
            // Decide what type of item to drop
            float roll = Random.value;
            
            if (roll < resourceChance)
            {
                // Drop a resource
                Item resource = GetRandomResource(monsterLevel);
                if (resource != null)
                {
                    droppedItems.Add(resource);
                    SpawnItemInWorld(resource, position);
                }
            }
            else if (roll < resourceChance + equipmentChance)
            {
                // Drop equipment
                Item equipment = GetRandomEquipment(rarity, monsterLevel);
                if (equipment != null)
                {
                    droppedItems.Add(equipment);
                    SpawnItemInWorld(equipment, position);
                }
            }
            else if (roll < resourceChance + equipmentChance + consumableChance)
            {
                // Drop consumable
                Item consumable = GetRandomConsumable(monsterLevel);
                if (consumable != null)
                {
                    droppedItems.Add(consumable);
                    SpawnItemInWorld(consumable, position);
                }
            }
        }
    }
    
    // Helper to set drop chances based on monster rarity
    private void SetLootChancesForRarity(MonsterRarity rarity, out float resourceChance, out float equipmentChance, 
                                        out float consumableChance, out int minDrops, out int maxDrops)
    {
        switch (rarity)
        {
            case MonsterRarity.Common:
                resourceChance = 0.4f;
                equipmentChance = 0.05f;
                consumableChance = 0.1f;
                minDrops = 0;
                maxDrops = 1;
                break;
            case MonsterRarity.Uncommon:
                resourceChance = 0.5f;
                equipmentChance = 0.1f;
                consumableChance = 0.2f;
                minDrops = 1;
                maxDrops = 2;
                break;
            case MonsterRarity.Rare:
                resourceChance = 0.6f;
                equipmentChance = 0.2f;
                consumableChance = 0.3f;
                minDrops = 1;
                maxDrops = 3;
                break;
            case MonsterRarity.Elite:
                resourceChance = 0.7f;
                equipmentChance = 0.4f;
                consumableChance = 0.5f;
                minDrops = 2;
                maxDrops = 4;
                break;
            case MonsterRarity.Boss:
                resourceChance = 0.8f;
                equipmentChance = 0.6f;
                consumableChance = 0.7f;
                minDrops = 3;
                maxDrops = 6;
                break;
            default:
                resourceChance = 0.3f;
                equipmentChance = 0.05f;
                consumableChance = 0.1f;
                minDrops = 0;
                maxDrops = 1;
                break;
        }
    }
    
    // Helper to get a random resource based on level
    private Item GetRandomResource(int level)
    {
        if (ItemDatabase.Instance == null) return null;
        
        List<Item> resources = ItemDatabase.Instance.GetItemsByType(ItemType.Resource);
        if (resources.Count == 0) return null;
        
        // Select a random resource, but favor higher tier resources for higher levels
        resources.Sort((a, b) => a.value.CompareTo(b.value));
        
        int index;
        if (level > 20)
        {
            // High level - favor valuable resources
            index = Mathf.Min(resources.Count - 1, resources.Count - 1 - Random.Range(0, 3));
        }
        else if (level > 10)
        {
            // Mid level - mixed resources
            index = Random.Range(resources.Count / 3, resources.Count - 1);
        }
        else
        {
            // Low level - basic resources
            index = Random.Range(0, Mathf.Min(resources.Count, 3));
        }
        
        return resources[index].Clone();
    }
    
    // Helper to get random equipment based on rarity and level
    private Item GetRandomEquipment(MonsterRarity monsterRarity, int level)
    {
        if (ItemDatabase.Instance == null) return null;
        
        List<Item> equipment = ItemDatabase.Instance.GetItemsByType(ItemType.Equipment);
        if (equipment.Count == 0) return null;
        
        // Filter by appropriate rarity
        ItemRarity targetRarity = ItemRarity.Common;
        
        switch (monsterRarity)
        {
            case MonsterRarity.Common:
                targetRarity = Random.value < 0.9f ? ItemRarity.Common : ItemRarity.Uncommon;
                break;
            case MonsterRarity.Uncommon:
                targetRarity = Random.value < 0.7f ? ItemRarity.Uncommon : ItemRarity.Rare;
                break;
            case MonsterRarity.Rare:
                targetRarity = Random.value < 0.6f ? ItemRarity.Rare : ItemRarity.Epic;
                break;
            case MonsterRarity.Elite:
                targetRarity = Random.value < 0.7f ? ItemRarity.Rare : ItemRarity.Epic;
                break;
            case MonsterRarity.Boss:
                targetRarity = Random.value < 0.5f ? ItemRarity.Epic : ItemRarity.Legendary;
                break;
        }
        
        // Filter by appropriate level
        List<Item> appropriateEquipment = new List<Item>();
        
        foreach (Item item in equipment)
        {
            if (item.rarity == targetRarity)
            {
                appropriateEquipment.Add(item);
            }
        }
        
        if (appropriateEquipment.Count == 0)
        {
            // Fallback if no items of target rarity
            return equipment[Random.Range(0, equipment.Count)].Clone();
        }
        
        return appropriateEquipment[Random.Range(0, appropriateEquipment.Count)].Clone();
    }
    
    // Helper to get random consumable based on level
    private Item GetRandomConsumable(int level)
    {
        if (ItemDatabase.Instance == null) return null;
        
        List<Item> consumables = ItemDatabase.Instance.GetItemsByType(ItemType.Consumable);
        if (consumables.Count == 0) return null;
        
        // Sort by value (assumed to correlate with power)
        consumables.Sort((a, b) => a.value.CompareTo(b.value));
        
        int index;
        if (level > 20)
        {
            // High level - better consumables
            index = Mathf.Min(consumables.Count - 1, consumables.Count - 1 - Random.Range(0, 2));
        }
        else if (level > 10)
        {
            // Mid level
            index = Random.Range(consumables.Count / 3, consumables.Count - 1);
        }
        else
        {
            // Low level - basic consumables
            index = Random.Range(0, Mathf.Min(consumables.Count, 3));
        }
        
        return consumables[index].Clone();
    }
    
    // Add a level-appropriate resource drop
    private void TryAddResourceDrop(List<Item> droppedItems, Vector3 position, int level)
    {
        if (Random.value < 0.3f) // 30% chance
        {
            Item resource = GetRandomResource(level);
            if (resource != null)
            {
                int quantity = Random.Range(1, Mathf.Max(2, level / 5));
                droppedItems.Add(resource);
                
                for (int i = 0; i < quantity; i++)
                {
                    SpawnItemInWorld(resource.Clone(), position);
                }
            }
        }
    }
    
    // Upgrade an item's rarity
    private void UpgradeItemRarity(Item item)
    {
        if (item == null) return;
        
        // Get next rarity level
        if (item.rarity == ItemRarity.Mythic)
            return; // Can't upgrade beyond mythic
            
        ItemRarity nextRarity = (ItemRarity)((int)item.rarity + 1);
        
        // Apply the new rarity
        item.rarity = nextRarity;
        
        // Adjust stats based on new rarity
        float statMultiplier = 1.0f;
        switch (nextRarity)
        {
            case ItemRarity.Uncommon:
                statMultiplier = 1.2f;
                break;
            case ItemRarity.Rare:
                statMultiplier = 1.5f;
                break;
            case ItemRarity.Epic:
                statMultiplier = 2.0f;
                break;
            case ItemRarity.Legendary:
                statMultiplier = 3.0f;
                break;
            case ItemRarity.Mythic:
                statMultiplier = 5.0f;
                break;
        }
        
        // Apply stat multiplier if it's equipment
        if (item.type == ItemType.Equipment)
        {
            item.strengthBonus = Mathf.RoundToInt(item.strengthBonus * statMultiplier);
            item.dexterityBonus = Mathf.RoundToInt(item.dexterityBonus * statMultiplier);
            item.intelligenceBonus = Mathf.RoundToInt(item.intelligenceBonus * statMultiplier);
            item.defenseBonus = Mathf.RoundToInt(item.defenseBonus * statMultiplier);
            item.damageModifier *= statMultiplier;
        }
    }
} 