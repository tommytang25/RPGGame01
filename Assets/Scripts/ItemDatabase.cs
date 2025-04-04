using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    // Singleton pattern
    public static ItemDatabase Instance { get; private set; }
    
    // Lists to store all items in the game
    [Header("Item Collections")]
    public List<Item> allItems = new List<Item>();
    public List<Item> weapons = new List<Item>();
    public List<Item> armor = new List<Item>();
    public List<Item> helmets = new List<Item>();
    public List<Item> accessories = new List<Item>();
    public List<Item> consumables = new List<Item>();
    public List<Item> craftingMaterials = new List<Item>();
    public List<Item> resources = new List<Item>();
    public List<Item> questItems = new List<Item>();
    
    // Dictionary for quick item lookup by ID
    private Dictionary<string, Item> itemsById = new Dictionary<string, Item>();
    
    // For editor use - create new items
    [Header("Item Creation")]
    public Sprite defaultItemIcon;
    
    [Header("Editor-Defined Items")]
    [Tooltip("Define custom weapons directly in the inspector")]
    public WeaponDefinition[] customWeapons;
    
    [Tooltip("Define custom armor directly in the inspector")]
    public ArmorDefinition[] customArmor;
    
    [Tooltip("Define custom helmets directly in the inspector")]
    public HelmetDefinition[] customHelmets;
    
    [Tooltip("Define custom accessories directly in the inspector")]
    public AccessoryDefinition[] customAccessories;
    
    [Tooltip("Define custom consumables directly in the inspector")]
    public ConsumableDefinition[] customConsumables;
    
    [Tooltip("Define custom resources directly in the inspector")]
    public ResourceDefinition[] customResources;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeItemDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeItemDatabase()
    {
        // Clear lists in case they had editor data
        ClearLists();
        
        // Initialize with predefined items
        CreateDefaultItems();
        
        // Add editor-defined custom items
        CreateCustomItems();
        
        // Populate the dictionary for fast lookup
        foreach (Item item in allItems)
        {
            itemsById[item.id] = item;
        }
        
        Debug.Log($"Item Database initialized with {allItems.Count} items");
    }
    
    private void ClearLists()
    {
        allItems.Clear();
        weapons.Clear();
        armor.Clear();
        helmets.Clear();
        accessories.Clear();
        consumables.Clear();
        craftingMaterials.Clear();
        resources.Clear();
        questItems.Clear();
        itemsById.Clear();
    }
    
    // Create all the default items in the game
    private void CreateDefaultItems()
    {
        // WEAPONS
        CreateWeapon("sword_wood", "Wooden Sword", "A basic wooden training sword.", 
            2, 0, 0, 0, WeaponType.Sword, 0.0f, ItemRarity.Common, 10);
            
        CreateWeapon("sword_iron", "Iron Sword", "Standard issue iron sword.", 
            5, 1, 0, 0, WeaponType.Sword, 0.0f, ItemRarity.Common, 50);
            
        CreateWeapon("sword_steel", "Steel Sword", "A well-crafted steel sword.", 
            8, 2, 0, 0, WeaponType.Sword, 0.1f, ItemRarity.Uncommon, 150);
            
        CreateWeapon("sword_enchanted", "Enchanted Blade", "A blade with magical properties.", 
            10, 3, 2, 0, WeaponType.Sword, 0.15f, ItemRarity.Rare, 500);
            
        CreateWeapon("axe_iron", "Iron Axe", "A heavy iron axe.", 
            7, 0, 0, 0, WeaponType.Axe, -0.1f, ItemRarity.Common, 60);
            
        CreateWeapon("bow_hunting", "Hunting Bow", "A simple hunting bow.", 
            3, 7, 0, 0, WeaponType.Bow, 0.2f, ItemRarity.Common, 70);
            
        CreateWeapon("staff_apprentice", "Apprentice Staff", "A staff for novice mages.", 
            0, 2, 8, 0, WeaponType.Staff, 0.0f, ItemRarity.Common, 80);
            
        CreateWeapon("dagger_assassin", "Assassin's Dagger", "A quick and deadly dagger.", 
            4, 8, 0, 0, WeaponType.Dagger, 0.3f, ItemRarity.Uncommon, 120);
        
        // ARMOR
        CreateArmor("armor_cloth", "Cloth Tunic", "Simple cloth protection.", 
            0, 1, 1, 2, 0.1f, ItemRarity.Common, 30);
            
        CreateArmor("armor_leather", "Leather Armor", "Lightweight leather protection.", 
            1, 2, 0, 5, 0.05f, ItemRarity.Common, 80);
            
        CreateArmor("armor_chain", "Chain Mail", "Flexible metal protection.", 
            2, 0, 0, 10, 0.0f, ItemRarity.Uncommon, 200);
            
        CreateArmor("armor_plate", "Plate Armor", "Heavy but strong protection.", 
            3, -1, 0, 15, -0.1f, ItemRarity.Uncommon, 350);
            
        CreateArmor("armor_mage", "Mage Robes", "Enchanted robes for spellcasters.", 
            0, 1, 5, 7, 0.0f, ItemRarity.Rare, 400);
        
        // HELMETS
        CreateHelmet("helmet_leather", "Leather Cap", "A simple leather cap.", 
            0, 1, 0, 2, ItemRarity.Common, 40);
            
        CreateHelmet("helmet_iron", "Iron Helmet", "Standard metal protection for your head.", 
            1, 0, 0, 5, ItemRarity.Common, 100);
            
        CreateHelmet("helmet_full", "Full Helmet", "Complete head protection.", 
            2, -1, 0, 8, ItemRarity.Uncommon, 200);
            
        CreateHelmet("helmet_mage", "Wizard Hat", "Enhances magical abilities.", 
            0, 0, 3, 3, ItemRarity.Uncommon, 180);
        
        // ACCESSORIES
        CreateAccessory("ring_strength", "Ring of Strength", "Enhances physical power.", 
            3, 0, 0, 0, ItemRarity.Uncommon, 200);
            
        CreateAccessory("ring_dexterity", "Ring of Dexterity", "Improves agility and reflexes.", 
            0, 3, 0, 0, ItemRarity.Uncommon, 200);
            
        CreateAccessory("ring_intelligence", "Ring of Intelligence", "Boosts magical potency.", 
            0, 0, 3, 0, ItemRarity.Uncommon, 200);
            
        CreateAccessory("amulet_protection", "Amulet of Protection", "Provides defensive energy.", 
            0, 0, 0, 5, ItemRarity.Uncommon, 250);
            
        CreateAccessory("bracelet_luck", "Lucky Bracelet", "Improves fortune in all endeavors.", 
            1, 1, 1, 1, ItemRarity.Rare, 400);
        
        // Add more items as needed
        
        // CONSUMABLES
        CreateConsumable("potion_health_small", "Minor Health Potion", "Restores a small amount of health.", 
            20, 0, 0, ItemRarity.Common, 15);
            
        CreateConsumable("potion_health_medium", "Health Potion", "Restores a moderate amount of health.", 
            50, 0, 0, ItemRarity.Common, 40);
            
        CreateConsumable("potion_stamina", "Stamina Potion", "Restores stamina.", 
            0, 30, 0, ItemRarity.Common, 35);
            
        CreateConsumable("potion_strength", "Strength Elixir", "Temporarily increases strength.", 
            0, 0, 30, ItemRarity.Uncommon, 75);
        
        // RESOURCES & MATERIALS
        CreateResource("ore_iron", "Iron Ore", "Raw iron ore for smelting.", ItemRarity.Common, 5);
        CreateResource("ore_gold", "Gold Ore", "Precious gold ore.", ItemRarity.Uncommon, 20);
        CreateResource("wood_oak", "Oak Wood", "Sturdy oak wood.", ItemRarity.Common, 3);
        CreateResource("herb_healing", "Healing Herb", "A plant with medicinal properties.", ItemRarity.Common, 8);
        CreateResource("crystal_mana", "Mana Crystal", "A crystal infused with magical energy.", ItemRarity.Rare, 50);
    }
    
    // Create all the custom items defined in the inspector
    private void CreateCustomItems()
    {
        // Create custom weapons
        if (customWeapons != null)
        {
            foreach (WeaponDefinition weaponDef in customWeapons)
            {
                CreateWeapon(
                    weaponDef.id,
                    weaponDef.name,
                    weaponDef.description,
                    weaponDef.strengthBonus,
                    weaponDef.dexterityBonus,
                    weaponDef.intelligenceBonus,
                    weaponDef.defenseBonus,
                    weaponDef.weaponType,
                    weaponDef.attackSpeedModifier,
                    weaponDef.rarity,
                    weaponDef.value
                );
            }
        }
        
        // Create custom armor
        if (customArmor != null)
        {
            foreach (ArmorDefinition armorDef in customArmor)
            {
                CreateArmor(
                    armorDef.id,
                    armorDef.name,
                    armorDef.description,
                    armorDef.strengthBonus,
                    armorDef.dexterityBonus,
                    armorDef.intelligenceBonus,
                    armorDef.defenseBonus,
                    armorDef.movementSpeedModifier,
                    armorDef.rarity,
                    armorDef.value
                );
            }
        }
        
        // Create custom helmets
        if (customHelmets != null)
        {
            foreach (HelmetDefinition helmetDef in customHelmets)
            {
                CreateHelmet(
                    helmetDef.id,
                    helmetDef.name,
                    helmetDef.description,
                    helmetDef.strengthBonus,
                    helmetDef.dexterityBonus,
                    helmetDef.intelligenceBonus,
                    helmetDef.defenseBonus,
                    helmetDef.rarity,
                    helmetDef.value
                );
            }
        }
        
        // Create custom accessories
        if (customAccessories != null)
        {
            foreach (AccessoryDefinition accessoryDef in customAccessories)
            {
                CreateAccessory(
                    accessoryDef.id,
                    accessoryDef.name,
                    accessoryDef.description,
                    accessoryDef.strengthBonus,
                    accessoryDef.dexterityBonus,
                    accessoryDef.intelligenceBonus,
                    accessoryDef.defenseBonus,
                    accessoryDef.rarity,
                    accessoryDef.value
                );
            }
        }
        
        // Create custom consumables
        if (customConsumables != null)
        {
            foreach (ConsumableDefinition consumableDef in customConsumables)
            {
                CreateConsumable(
                    consumableDef.id,
                    consumableDef.name,
                    consumableDef.description,
                    consumableDef.healthRestoreAmount,
                    consumableDef.staminaRestoreAmount,
                    consumableDef.buffDuration,
                    consumableDef.rarity,
                    consumableDef.value
                );
            }
        }
        
        // Create custom resources
        if (customResources != null)
        {
            foreach (ResourceDefinition resourceDef in customResources)
            {
                CreateResource(
                    resourceDef.id,
                    resourceDef.name,
                    resourceDef.description,
                    resourceDef.rarity,
                    resourceDef.value
                );
            }
        }
    }
    
    #region Item Creation Methods
    
    public Item CreateWeapon(string id, string name, string description, 
                           int strength, int dexterity, int intelligence, int defense,
                           WeaponType weaponType, float attackSpeedMod, ItemRarity rarity, int value)
    {
        Item weapon = new Item(id, name, description, ItemType.Equipment);
        weapon.equipmentSlot = EquipmentSlot.Weapon;
        weapon.strengthBonus = strength;
        weapon.dexterityBonus = dexterity;
        weapon.intelligenceBonus = intelligence;
        weapon.defenseBonus = defense;
        weapon.weaponType = weaponType;
        weapon.attackSpeedModifier = attackSpeedMod;
        weapon.rarity = rarity;
        weapon.value = value;
        weapon.icon = defaultItemIcon; // Set default icon, can be updated later
        
        // Add to appropriate lists
        weapons.Add(weapon);
        allItems.Add(weapon);
        
        return weapon;
    }
    
    public Item CreateArmor(string id, string name, string description, 
                          int strength, int dexterity, int intelligence, int defense,
                          float movementSpeedMod, ItemRarity rarity, int value)
    {
        Item armor = new Item(id, name, description, ItemType.Equipment);
        armor.equipmentSlot = EquipmentSlot.Armor;
        armor.strengthBonus = strength;
        armor.dexterityBonus = dexterity;
        armor.intelligenceBonus = intelligence;
        armor.defenseBonus = defense;
        armor.movementSpeedModifier = movementSpeedMod;
        armor.rarity = rarity;
        armor.value = value;
        armor.icon = defaultItemIcon;
        
        // Add to appropriate lists
        this.armor.Add(armor);
        allItems.Add(armor);
        
        return armor;
    }
    
    public Item CreateHelmet(string id, string name, string description, 
                           int strength, int dexterity, int intelligence, int defense,
                           ItemRarity rarity, int value)
    {
        Item helmet = new Item(id, name, description, ItemType.Equipment);
        helmet.equipmentSlot = EquipmentSlot.Helmet;
        helmet.strengthBonus = strength;
        helmet.dexterityBonus = dexterity;
        helmet.intelligenceBonus = intelligence;
        helmet.defenseBonus = defense;
        helmet.rarity = rarity;
        helmet.value = value;
        helmet.icon = defaultItemIcon;
        
        // Add to appropriate lists
        helmets.Add(helmet);
        allItems.Add(helmet);
        
        return helmet;
    }
    
    public Item CreateAccessory(string id, string name, string description, 
                              int strength, int dexterity, int intelligence, int defense,
                              ItemRarity rarity, int value)
    {
        Item accessory = new Item(id, name, description, ItemType.Equipment);
        accessory.equipmentSlot = EquipmentSlot.Accessory;
        accessory.strengthBonus = strength;
        accessory.dexterityBonus = dexterity;
        accessory.intelligenceBonus = intelligence;
        accessory.defenseBonus = defense;
        accessory.rarity = rarity;
        accessory.value = value;
        accessory.icon = defaultItemIcon;
        
        // Add to appropriate lists
        accessories.Add(accessory);
        allItems.Add(accessory);
        
        return accessory;
    }
    
    public Item CreateConsumable(string id, string name, string description, 
                               int healthRestore, int staminaRestore, float buffDuration,
                               ItemRarity rarity, int value)
    {
        Item consumable = new Item(id, name, description, ItemType.Consumable);
        consumable.healthRestoreAmount = healthRestore;
        consumable.staminaRestoreAmount = staminaRestore;
        consumable.buffDuration = buffDuration;
        consumable.rarity = rarity;
        consumable.value = value;
        consumable.icon = defaultItemIcon;
        
        // Add to appropriate lists
        consumables.Add(consumable);
        allItems.Add(consumable);
        
        return consumable;
    }
    
    public Item CreateResource(string id, string name, string description, 
                             ItemRarity rarity, int value)
    {
        Item resource = new Item(id, name, description, ItemType.Resource);
        resource.rarity = rarity;
        resource.value = value;
        resource.icon = defaultItemIcon;
        
        // Add to appropriate lists
        resources.Add(resource);
        allItems.Add(resource);
        
        return resource;
    }
    
    #endregion
    
    #region Item Retrieval Methods
    
    public Item GetItemById(string id)
    {
        if (itemsById.TryGetValue(id, out Item item))
        {
            return item.Clone(); // Return a clone to prevent modifying the template
        }
        
        Debug.LogWarning($"Item with ID '{id}' not found in database!");
        return null;
    }
    
    public List<Item> GetItemsByType(ItemType type)
    {
        List<Item> result = new List<Item>();
        
        foreach (Item item in allItems)
        {
            if (item.type == type)
            {
                result.Add(item.Clone());
            }
        }
        
        return result;
    }
    
    public List<Item> GetEquipmentBySlot(EquipmentSlot slot)
    {
        List<Item> result = new List<Item>();
        
        foreach (Item item in allItems)
        {
            if (item.type == ItemType.Equipment && item.equipmentSlot == slot)
            {
                result.Add(item.Clone());
            }
        }
        
        return result;
    }
    
    public List<Item> GetItemsByRarity(ItemRarity rarity)
    {
        List<Item> result = new List<Item>();
        
        foreach (Item item in allItems)
        {
            if (item.rarity == rarity)
            {
                result.Add(item.Clone());
            }
        }
        
        return result;
    }
    
    #endregion
    
    #region Editor Methods
    
    // Add a new item to the database (useful for editor)
    public void AddItemToDatabase(Item item)
    {
        if (string.IsNullOrEmpty(item.id))
        {
            Debug.LogError("Cannot add item without ID to database");
            return;
        }
        
        if (itemsById.ContainsKey(item.id))
        {
            Debug.LogWarning($"Item with ID '{item.id}' already exists in database, updating it");
            itemsById[item.id] = item; // Update existing
            
            // Update in appropriate lists
            UpdateItemInLists(item);
        }
        else
        {
            // Add to lookup dictionary
            itemsById[item.id] = item;
            
            // Add to all items list
            allItems.Add(item);
            
            // Add to appropriate type list
            AddItemToTypeList(item);
        }
    }
    
    private void AddItemToTypeList(Item item)
    {
        switch (item.type)
        {
            case ItemType.Equipment:
                switch (item.equipmentSlot)
                {
                    case EquipmentSlot.Weapon:
                        weapons.Add(item);
                        break;
                    case EquipmentSlot.Armor:
                        armor.Add(item);
                        break;
                    case EquipmentSlot.Helmet:
                        helmets.Add(item);
                        break;
                    case EquipmentSlot.Accessory:
                        accessories.Add(item);
                        break;
                }
                break;
            case ItemType.Consumable:
                consumables.Add(item);
                break;
            case ItemType.QuestItem:
                questItems.Add(item);
                break;
            case ItemType.CraftingMaterial:
                craftingMaterials.Add(item);
                break;
            case ItemType.Resource:
                resources.Add(item);
                break;
        }
    }
    
    private void UpdateItemInLists(Item item)
    {
        // First remove from existing lists
        weapons.RemoveAll(x => x.id == item.id);
        armor.RemoveAll(x => x.id == item.id);
        helmets.RemoveAll(x => x.id == item.id);
        accessories.RemoveAll(x => x.id == item.id);
        consumables.RemoveAll(x => x.id == item.id);
        questItems.RemoveAll(x => x.id == item.id);
        craftingMaterials.RemoveAll(x => x.id == item.id);
        resources.RemoveAll(x => x.id == item.id);
        
        // Replace in allItems
        for (int i = 0; i < allItems.Count; i++)
        {
            if (allItems[i].id == item.id)
            {
                allItems[i] = item;
                break;
            }
        }
        
        // Add to appropriate list
        AddItemToTypeList(item);
    }
    
    // Remove an item from the database
    public void RemoveItemFromDatabase(string id)
    {
        if (!itemsById.ContainsKey(id))
        {
            Debug.LogWarning($"Item with ID '{id}' not found in database, cannot remove");
            return;
        }
        
        Item item = itemsById[id];
        
        // Remove from all lists
        weapons.RemoveAll(x => x.id == id);
        armor.RemoveAll(x => x.id == id);
        helmets.RemoveAll(x => x.id == id);
        accessories.RemoveAll(x => x.id == id);
        consumables.RemoveAll(x => x.id == id);
        questItems.RemoveAll(x => x.id == id);
        craftingMaterials.RemoveAll(x => x.id == id);
        resources.RemoveAll(x => x.id == id);
        allItems.RemoveAll(x => x.id == id);
        
        // Remove from dictionary
        itemsById.Remove(id);
        
        Debug.Log($"Item '{item.name}' (ID: {id}) removed from database");
    }
    
    #endregion
    
    #region Debug Methods
    
    // Debug method to list all items in the database
    public void ListAllItems()
    {
        Debug.Log("====== ITEM DATABASE CONTENTS ======");
        Debug.Log($"Total Items: {allItems.Count}");
        Debug.Log($"Weapons: {weapons.Count}");
        Debug.Log($"Armor: {armor.Count}");
        Debug.Log($"Helmets: {helmets.Count}");
        Debug.Log($"Accessories: {accessories.Count}");
        Debug.Log($"Consumables: {consumables.Count}");
        Debug.Log($"Crafting Materials: {craftingMaterials.Count}");
        Debug.Log($"Resources: {resources.Count}");
        Debug.Log($"Quest Items: {questItems.Count}");
        
        // List all weapons
        if (weapons.Count > 0)
        {
            Debug.Log("\n-- WEAPONS --");
            foreach (Item weapon in weapons)
            {
                ListItemDetails(weapon);
            }
        }
        
        // List all armor
        if (armor.Count > 0)
        {
            Debug.Log("\n-- ARMOR --");
            foreach (Item armorItem in armor)
            {
                ListItemDetails(armorItem);
            }
        }
        
        // List all helmets
        if (helmets.Count > 0)
        {
            Debug.Log("\n-- HELMETS --");
            foreach (Item helmet in helmets)
            {
                ListItemDetails(helmet);
            }
        }
        
        // List all accessories
        if (accessories.Count > 0)
        {
            Debug.Log("\n-- ACCESSORIES --");
            foreach (Item accessory in accessories)
            {
                ListItemDetails(accessory);
            }
        }
    }
    
    // Debug method to display item details
    private void ListItemDetails(Item item)
    {
        string rarityStr = item.rarity.ToString();
        string stats = "";
        
        if (item.strengthBonus != 0) stats += $"STR: {item.strengthBonus} ";
        if (item.dexterityBonus != 0) stats += $"DEX: {item.dexterityBonus} ";
        if (item.intelligenceBonus != 0) stats += $"INT: {item.intelligenceBonus} ";
        if (item.defenseBonus != 0) stats += $"DEF: {item.defenseBonus} ";
        
        if (item.type == ItemType.Equipment && item.equipmentSlot == EquipmentSlot.Weapon)
        {
            stats += $"Type: {item.weaponType} ";
            if (item.attackSpeedModifier != 0) stats += $"Speed: {item.attackSpeedModifier:+0.##;-0.##;0} ";
            if (item.damageModifier != 0) stats += $"Dmg: {item.damageModifier:+0.##;-0.##;0} ";
        }
        
        if (item.movementSpeedModifier != 0) stats += $"Move: {item.movementSpeedModifier:+0.##;-0.##;0} ";
        
        Debug.Log($"[{rarityStr}] {item.name} (ID: {item.id}) - {stats} - Value: {item.CalculateValue()}");
        Debug.Log($"  > {item.description}");
    }
    
    // Show details of a specific item by ID
    public void PrintItemDetails(string itemId)
    {
        Item item = GetItemById(itemId);
        if (item != null)
        {
            Debug.Log("====== ITEM DETAILS ======");
            ListItemDetails(item);
        }
        else
        {
            Debug.LogWarning($"Item with ID '{itemId}' not found in database!");
        }
    }
    
    #endregion
}

// Item definition classes for inspector use
[System.Serializable]
public class WeaponDefinition
{
    public string id;
    public string name;
    public string description;
    public int strengthBonus;
    public int dexterityBonus;
    public int intelligenceBonus;
    public int defenseBonus;
    public WeaponType weaponType = WeaponType.Sword;
    public float attackSpeedModifier;
    public ItemRarity rarity = ItemRarity.Common;
    public int value;
    public Sprite icon;
}

[System.Serializable]
public class ArmorDefinition
{
    public string id;
    public string name;
    public string description;
    public int strengthBonus;
    public int dexterityBonus;
    public int intelligenceBonus;
    public int defenseBonus;
    public float movementSpeedModifier;
    public ItemRarity rarity = ItemRarity.Common;
    public int value;
    public Sprite icon;
}

[System.Serializable]
public class HelmetDefinition
{
    public string id;
    public string name;
    public string description;
    public int strengthBonus;
    public int dexterityBonus;
    public int intelligenceBonus;
    public int defenseBonus;
    public ItemRarity rarity = ItemRarity.Common;
    public int value;
    public Sprite icon;
}

[System.Serializable]
public class AccessoryDefinition
{
    public string id;
    public string name;
    public string description;
    public int strengthBonus;
    public int dexterityBonus;
    public int intelligenceBonus;
    public int defenseBonus;
    public ItemRarity rarity = ItemRarity.Common;
    public int value;
    public Sprite icon;
}

[System.Serializable]
public class ConsumableDefinition
{
    public string id;
    public string name;
    public string description;
    public int healthRestoreAmount;
    public int staminaRestoreAmount;
    public float buffDuration;
    public ItemRarity rarity = ItemRarity.Common;
    public int value;
    public Sprite icon;
}

[System.Serializable]
public class ResourceDefinition
{
    public string id;
    public string name;
    public string description;
    public ItemRarity rarity = ItemRarity.Common;
    public int value;
    public Sprite icon;
} 