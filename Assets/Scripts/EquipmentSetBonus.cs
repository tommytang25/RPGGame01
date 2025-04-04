using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SetBonusEffect
{
    public string description;
    public int piecesRequired;
    public int strengthBonus;
    public int dexterityBonus;
    public int intelligenceBonus;
    public int defenseBonus;
    public float damageModifier;
    public float speedModifier;
    public float criticalChanceBonus;
    public float healthRegenBonus;
    public float manaRegenBonus;
    public bool specialEffect; // If true, this set has a unique effect handled in code
    
    public SetBonusEffect(string description, int piecesRequired)
    {
        this.description = description;
        this.piecesRequired = piecesRequired;
    }
}

// Define all equipment set bonuses
public static class EquipmentSetBonuses
{
    // Dictionary of all set bonuses by equipment set
    private static Dictionary<EquipmentSet, List<SetBonusEffect>> allSetBonuses;
    
    // Initialize all set bonuses
    static EquipmentSetBonuses()
    {
        allSetBonuses = new Dictionary<EquipmentSet, List<SetBonusEffect>>();
        InitializeSetBonuses();
    }
    
    // Set up all the different set bonuses
    private static void InitializeSetBonuses()
    {
        // Warrior Set - Strength focused
        List<SetBonusEffect> warriorBonuses = new List<SetBonusEffect>
        {
            new SetBonusEffect("Might of the Warrior: +5 Strength", 2) 
            { 
                strengthBonus = 5 
            },
            new SetBonusEffect("Warrior's Focus: +12 Strength, +10% Damage", 4) 
            { 
                strengthBonus = 12, 
                damageModifier = 0.1f
            },
            new SetBonusEffect("Warrior's Fury: +20 Strength, +20% Damage, Special Attack", 6) 
            { 
                strengthBonus = 20, 
                damageModifier = 0.2f,
                specialEffect = true 
            }
        };
        allSetBonuses[EquipmentSet.Warrior] = warriorBonuses;
        
        // Ranger Set - Dexterity focused
        List<SetBonusEffect> rangerBonuses = new List<SetBonusEffect>
        {
            new SetBonusEffect("Ranger's Precision: +5 Dexterity", 2) 
            { 
                dexterityBonus = 5 
            },
            new SetBonusEffect("Ranger's Speed: +12 Dexterity, +15% Attack Speed", 4) 
            { 
                dexterityBonus = 12, 
                speedModifier = 0.15f
            },
            new SetBonusEffect("Ranger's Mastery: +20 Dexterity, +25% Attack Speed, Special Shot", 6) 
            { 
                dexterityBonus = 20, 
                speedModifier = 0.25f,
                specialEffect = true
            }
        };
        allSetBonuses[EquipmentSet.Ranger] = rangerBonuses;
        
        // Mage Set - Intelligence focused
        List<SetBonusEffect> mageBonuses = new List<SetBonusEffect>
        {
            new SetBonusEffect("Arcane Mind: +5 Intelligence", 2) 
            { 
                intelligenceBonus = 5 
            },
            new SetBonusEffect("Arcane Power: +12 Intelligence, +3 Mana Regen", 4) 
            { 
                intelligenceBonus = 12, 
                manaRegenBonus = 3f
            },
            new SetBonusEffect("Arcane Mastery: +20 Intelligence, +5 Mana Regen, Spell Echo", 6) 
            { 
                intelligenceBonus = 20, 
                manaRegenBonus = 5f,
                specialEffect = true
            }
        };
        allSetBonuses[EquipmentSet.Mage] = mageBonuses;
        
        // Guardian Set - Defense focused
        List<SetBonusEffect> guardianBonuses = new List<SetBonusEffect>
        {
            new SetBonusEffect("Guardian's Protection: +8 Defense", 2) 
            { 
                defenseBonus = 8 
            },
            new SetBonusEffect("Guardian's Resilience: +18 Defense, +2 Health Regen", 4) 
            { 
                defenseBonus = 18, 
                healthRegenBonus = 2f
            },
            new SetBonusEffect("Guardian's Fortitude: +30 Defense, +5 Health Regen, Damage Reflection", 6) 
            { 
                defenseBonus = 30, 
                healthRegenBonus = 5f,
                specialEffect = true
            }
        };
        allSetBonuses[EquipmentSet.Guardian] = guardianBonuses;
        
        // Shadow Set - Critical hit focused
        List<SetBonusEffect> shadowBonuses = new List<SetBonusEffect>
        {
            new SetBonusEffect("Shadow's Edge: +5% Critical Chance", 2) 
            { 
                criticalChanceBonus = 0.05f 
            },
            new SetBonusEffect("Shadow's Precision: +10% Critical Chance, +5 Dexterity", 4) 
            { 
                criticalChanceBonus = 0.1f, 
                dexterityBonus = 5
            },
            new SetBonusEffect("Shadow's Mastery: +15% Critical Chance, +10 Dexterity, Backstab", 6) 
            { 
                criticalChanceBonus = 0.15f, 
                dexterityBonus = 10,
                specialEffect = true
            }
        };
        allSetBonuses[EquipmentSet.Shadow] = shadowBonuses;
        
        // Dragon Set - Fire themed
        List<SetBonusEffect> dragonBonuses = new List<SetBonusEffect>
        {
            new SetBonusEffect("Dragon's Scales: Fire Resistance, +5 Defense", 2) 
            { 
                defenseBonus = 5, 
                specialEffect = true 
            },
            new SetBonusEffect("Dragon's Might: Fire Resistance, +10 Defense, +10% Damage", 4) 
            { 
                defenseBonus = 10, 
                damageModifier = 0.1f, 
                specialEffect = true 
            },
            new SetBonusEffect("Dragon's Fury: Fire Resistance, +15 Defense, +20% Damage, Flame Breath", 6) 
            { 
                defenseBonus = 15, 
                damageModifier = 0.2f, 
                specialEffect = true 
            }
        };
        allSetBonuses[EquipmentSet.Dragon] = dragonBonuses;
        
        // Nature Set - Regeneration themed
        List<SetBonusEffect> natureBonuses = new List<SetBonusEffect>
        {
            new SetBonusEffect("Nature's Blessing: +2 Health Regen", 2) 
            { 
                healthRegenBonus = 2f 
            },
            new SetBonusEffect("Nature's Embrace: +4 Health Regen, +5 All Stats", 4) 
            { 
                healthRegenBonus = 4f, 
                strengthBonus = 5,
                dexterityBonus = 5,
                intelligenceBonus = 5,
                defenseBonus = 5
            },
            new SetBonusEffect("Nature's Harmony: +6 Health Regen, +10 All Stats, Nature's Wrath", 6) 
            { 
                healthRegenBonus = 6f, 
                strengthBonus = 10,
                dexterityBonus = 10,
                intelligenceBonus = 10,
                defenseBonus = 10,
                specialEffect = true
            }
        };
        allSetBonuses[EquipmentSet.Nature] = natureBonuses;
        
        // Abyssal Set - Dark magic themed
        List<SetBonusEffect> abyssalBonuses = new List<SetBonusEffect>
        {
            new SetBonusEffect("Abyssal Touch: +5% Life Steal", 2) 
            { 
                specialEffect = true 
            },
            new SetBonusEffect("Abyssal Embrace: +10% Life Steal, +8 Intelligence", 4) 
            { 
                specialEffect = true, 
                intelligenceBonus = 8 
            },
            new SetBonusEffect("Abyssal Mastery: +15% Life Steal, +15 Intelligence, Void Nova", 6) 
            { 
                specialEffect = true, 
                intelligenceBonus = 15 
            }
        };
        allSetBonuses[EquipmentSet.Abyssal] = abyssalBonuses;
    }
    
    // Get set bonus effects for a specific set and number of pieces
    public static List<SetBonusEffect> GetActiveSetBonuses(EquipmentSet set, int pieceCount)
    {
        if (set == EquipmentSet.None || pieceCount <= 0)
            return new List<SetBonusEffect>();
            
        if (!allSetBonuses.ContainsKey(set))
            return new List<SetBonusEffect>();
            
        List<SetBonusEffect> result = new List<SetBonusEffect>();
        List<SetBonusEffect> setBonuses = allSetBonuses[set];
        
        foreach (SetBonusEffect bonus in setBonuses)
        {
            if (pieceCount >= bonus.piecesRequired)
            {
                result.Add(bonus);
            }
        }
        
        return result;
    }
    
    // Get all set bonus descriptions for a set and piece count
    public static List<string> GetSetBonusDescriptions(EquipmentSet set, int pieceCount)
    {
        List<string> descriptions = new List<string>();
        
        if (set == EquipmentSet.None || !allSetBonuses.ContainsKey(set))
            return descriptions;
            
        List<SetBonusEffect> allBonuses = allSetBonuses[set];
        
        foreach (SetBonusEffect bonus in allBonuses)
        {
            string status = pieceCount >= bonus.piecesRequired ? "Active" : "Inactive";
            string colorHex = pieceCount >= bonus.piecesRequired ? 
                ColorUtility.ToHtmlStringRGB(Color.green) : 
                ColorUtility.ToHtmlStringRGB(Color.gray);
                
            string description = $"<color=#{colorHex}>{bonus.description} [{status}]</color>";
            descriptions.Add(description);
        }
        
        return descriptions;
    }
    
    // Get all potential set bonuses for a given set, regardless of piece count
    public static List<SetBonusEffect> GetAllSetBonuses(EquipmentSet set)
    {
        if (set == EquipmentSet.None || !allSetBonuses.ContainsKey(set))
            return new List<SetBonusEffect>();
            
        return new List<SetBonusEffect>(allSetBonuses[set]);
    }
} 