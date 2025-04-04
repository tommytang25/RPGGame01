using UnityEngine;
using System.Collections.Generic;

// Enum for equipment sets
public enum EquipmentSet
{
    None,
    Warrior,    // Strength-focused set
    Ranger,     // Dexterity-focused set
    Mage,       // Intelligence-focused set
    Guardian,   // Defense-focused set
    Shadow,     // Critical-focused set
    Dragon,     // Fire resistance and damage
    Nature,     // Regeneration and resource gathering
    Abyssal     // Dark magic and lifesteal
}

[System.Serializable]
public class Item
{
    public string id;
    public string name;
    public string description;
    public ItemType type;
    public ItemRarity rarity = ItemRarity.Common;
    public Sprite icon;
    
    // Stats for equipment
    public int strengthBonus;
    public int dexterityBonus;
    public int intelligenceBonus;
    public int defenseBonus;
    public float attackSpeedModifier = 0f;
    public EquipmentSlot equipmentSlot = EquipmentSlot.Weapon;
    public EquipmentSet equipmentSet = EquipmentSet.None; // New property for equipment sets
    
    // For weapons
    public float damageModifier = 0f;
    public WeaponType weaponType = WeaponType.Sword;
    
    // For armor
    public float movementSpeedModifier = 0f;
    
    // For accessories
    public List<ItemEffect> itemEffects = new List<ItemEffect>();
    
    // For consumables
    public int healthRestoreAmount;
    public int staminaRestoreAmount;
    public float buffDuration;
    
    // For crafting
    public bool isModernTechnology;
    public List<string> craftingRequirements;
    public int craftingLevel = 1;
    
    // For farming/grinding
    public float dropRate = 0.1f;
    public string dropsFrom = "";
    public bool isRespawnable = false;
    public float respawnTime = 300f; // 5 minutes default
    
    // For quest items
    public string relatedQuestId;
    
    // For selling/buying
    public int value = 10;
    
    // Basic constructor
    public Item(string id, string name, string description, ItemType type)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.type = type;
    }
    
    // Clone method for creating duplicates
    public Item Clone()
    {
        Item clone = new Item(id, name, description, type);
        clone.rarity = rarity;
        clone.icon = icon;
        
        // Copy equipment stats
        clone.strengthBonus = strengthBonus;
        clone.dexterityBonus = dexterityBonus;
        clone.intelligenceBonus = intelligenceBonus;
        clone.defenseBonus = defenseBonus;
        clone.attackSpeedModifier = attackSpeedModifier;
        clone.equipmentSlot = equipmentSlot;
        clone.equipmentSet = equipmentSet; // Copy the equipment set
        
        // Copy weapon stats
        clone.damageModifier = damageModifier;
        clone.weaponType = weaponType;
        
        // Copy armor stats
        clone.movementSpeedModifier = movementSpeedModifier;
        
        // Copy accessory effects
        clone.itemEffects = new List<ItemEffect>(itemEffects);
        
        // Copy consumable stats
        clone.healthRestoreAmount = healthRestoreAmount;
        clone.staminaRestoreAmount = staminaRestoreAmount;
        clone.buffDuration = buffDuration;
        
        // Copy crafting info
        clone.isModernTechnology = isModernTechnology;
        clone.craftingRequirements = new List<string>(craftingRequirements);
        clone.craftingLevel = craftingLevel;
        
        // Copy farming/grinding info
        clone.dropRate = dropRate;
        clone.dropsFrom = dropsFrom;
        clone.isRespawnable = isRespawnable;
        clone.respawnTime = respawnTime;
        
        // Copy quest info
        clone.relatedQuestId = relatedQuestId;
        
        // Copy value
        clone.value = value;
        
        return clone;
    }
    
    // Calculate item value based on rarity
    public int CalculateValue()
    {
        float rarityMultiplier = 1f;
        
        switch (rarity)
        {
            case ItemRarity.Common:
                rarityMultiplier = 1f;
                break;
            case ItemRarity.Uncommon:
                rarityMultiplier = 2f;
                break;
            case ItemRarity.Rare:
                rarityMultiplier = 5f;
                break;
            case ItemRarity.Epic:
                rarityMultiplier = 10f;
                break;
            case ItemRarity.Legendary:
                rarityMultiplier = 25f;
                break;
        }
        
        return Mathf.RoundToInt(value * rarityMultiplier);
    }
    
    // Get color based on item rarity
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return new Color(0.8f, 0.8f, 0.8f); // Light gray
            case ItemRarity.Uncommon:
                return new Color(0.0f, 0.8f, 0.0f); // Bright green
            case ItemRarity.Rare:
                return new Color(0.0f, 0.5f, 1.0f); // Vibrant blue
            case ItemRarity.Epic:
                return new Color(0.7f, 0.2f, 1.0f); // Bright purple
            case ItemRarity.Legendary:
                return new Color(1.0f, 0.7f, 0.0f); // Golden orange
            default:
                return Color.white;
        }
    }
    
    // Get name with rarity color for UI display
    public string GetColoredName()
    {
        string colorHex = ColorUtility.ToHtmlStringRGB(GetRarityColor());
        return $"<color=#{colorHex}>{name}</color>";
    }
    
    // Get colorful rarity text with symbol
    public string GetRarityDisplay()
    {
        string colorHex = ColorUtility.ToHtmlStringRGB(GetRarityColor());
        string symbol = "";
        
        switch (rarity)
        {
            case ItemRarity.Common:
                symbol = "●";
                break;
            case ItemRarity.Uncommon:
                symbol = "★";
                break;
            case ItemRarity.Rare:
                symbol = "★★";
                break;
            case ItemRarity.Epic:
                symbol = "★★★";
                break;
            case ItemRarity.Legendary:
                symbol = "✦✦✦";
                break;
        }
        
        return $"<color=#{colorHex}>{symbol} {rarity} {symbol}</color>";
    }
    
    // Check if this item can be stacked (for inventory purposes)
    public bool IsStackable()
    {
        return type == ItemType.Consumable || 
               type == ItemType.CraftingMaterial ||
               type == ItemType.Resource;
    }
    
    // Get equipment set display name with color
    public string GetSetName()
    {
        if (equipmentSet == EquipmentSet.None)
            return "";
            
        Color setColor = GetSetColor();
        string colorHex = ColorUtility.ToHtmlStringRGB(setColor);
        return $"<color=#{colorHex}>Set: {equipmentSet}</color>";
    }
    
    // Get color for the equipment set
    public Color GetSetColor()
    {
        switch (equipmentSet)
        {
            case EquipmentSet.Warrior:
                return new Color(0.8f, 0.2f, 0.2f); // Red
            case EquipmentSet.Ranger:
                return new Color(0.2f, 0.8f, 0.2f); // Green
            case EquipmentSet.Mage:
                return new Color(0.2f, 0.2f, 0.8f); // Blue
            case EquipmentSet.Guardian:
                return new Color(0.8f, 0.8f, 0.2f); // Yellow
            case EquipmentSet.Shadow:
                return new Color(0.5f, 0.0f, 0.5f); // Purple
            case EquipmentSet.Dragon:
                return new Color(1.0f, 0.4f, 0.0f); // Orange
            case EquipmentSet.Nature:
                return new Color(0.0f, 0.8f, 0.4f); // Teal
            case EquipmentSet.Abyssal:
                return new Color(0.4f, 0.0f, 0.8f); // Deep Purple
            default:
                return Color.white;
        }
    }
}

public enum ItemType
{
    Consumable,
    Equipment,
    QuestItem,
    CraftingMaterial,
    Resource   // New type for farmable resources
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum WeaponType
{
    Sword,
    Axe,
    Bow,
    Staff,
    Dagger
}

[System.Serializable]
public class ItemEffect
{
    public EffectType type;
    public float value;
    public float duration;
    
    public ItemEffect(EffectType type, float value, float duration = 0f)
    {
        this.type = type;
        this.value = value;
        this.duration = duration;
    }
}

public enum EffectType
{
    ExperienceBoost,
    HealthRegen,
    StaminaRegen,
    CriticalChance,
    ResourceFindChance,
    GoldFind,
    DamageReflect,
    ThornsEffect
} 