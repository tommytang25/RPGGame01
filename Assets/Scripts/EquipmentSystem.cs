using UnityEngine;
using System.Collections.Generic;

public class EquipmentSystem : MonoBehaviour
{
    // Singleton instance
    public static EquipmentSystem Instance { get; private set; }
    
    [Header("Equipment Slots")]
    public EquippedItem weaponSlot;
    public EquippedItem armorSlot;
    public EquippedItem helmetSlot;
    public EquippedItem accessory1Slot;
    public EquippedItem accessory2Slot;
    
    [Header("UI References")]
    public GameObject equipmentUI;
    public Transform equipmentSlotsParent;
    
    // Events
    public delegate void EquipmentChangedEvent();
    public event EquipmentChangedEvent OnEquipmentChanged;
    
    // Stats bonuses from all equipment
    private int strengthBonus = 0;
    private int dexterityBonus = 0;
    private int intelligenceBonus = 0;
    private int defenseBonus = 0;
    
    // New set bonus stats
    private float damageModifierBonus = 0f;
    private float attackSpeedBonus = 0f;
    private float criticalChanceBonus = 0f;
    private float healthRegenBonus = 0f;
    private float manaRegenBonus = 0f;
    
    // Track active equipment sets
    private Dictionary<EquipmentSet, int> equippedSetPieces = new Dictionary<EquipmentSet, int>();
    
    private PlayerController playerController;
    
    // Event for set bonus changes
    public delegate void SetBonusChangedEvent(EquipmentSet set, int pieceCount);
    public event SetBonusChangedEvent OnSetBonusChanged;
    
    private void Awake()
    {
        // Singleton pattern
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
    }
    
    private void Start()
    {
        // Initialize UI
        if (equipmentUI != null)
        {
            equipmentUI.SetActive(false);
        }
        
        // Find player controller
        playerController = FindObjectOfType<PlayerController>();
        
        // Initialize equipment slots
        weaponSlot = new EquippedItem(EquipmentSlot.Weapon);
        armorSlot = new EquippedItem(EquipmentSlot.Armor);
        helmetSlot = new EquippedItem(EquipmentSlot.Helmet);
        accessory1Slot = new EquippedItem(EquipmentSlot.Accessory);
        accessory2Slot = new EquippedItem(EquipmentSlot.Accessory);
        
        // Initialize set piece tracking
        InitializeSetPieceTracking();
    }
    
    private void Update()
    {
        // Toggle equipment UI
        if (Input.GetKeyDown(KeyCode.E) && Input.GetKey(KeyCode.LeftShift))
        {
            ToggleEquipmentUI();
        }
    }
    
    public void ToggleEquipmentUI()
    {
        if (equipmentUI != null)
        {
            bool isActive = equipmentUI.activeSelf;
            equipmentUI.SetActive(!isActive);
            
            if (!isActive)
            {
                // Opening equipment UI
                RefreshEquipmentUI();
            }
        }
    }
    
    private void RefreshEquipmentUI()
    {
        // TODO: Update equipment UI with current equipped items
        Debug.Log("Refreshing equipment UI");
    }
    
    public bool EquipItem(Item item)
    {
        if (item.type != ItemType.Equipment)
        {
            Debug.LogWarning("Attempted to equip non-equipment item: " + item.name);
            return false;
        }
        
        EquipmentSlot slot = item.equipmentSlot;
        EquippedItem targetSlot = GetEquipmentSlotByType(slot);
        
        // Track set piece changes
        EquipmentSet oldSetType = EquipmentSet.None;
        
        // If there's already an item in this slot, add it back to inventory
        if (!targetSlot.IsEmpty())
        {
            // Track the old set type before removing
            oldSetType = targetSlot.equippedItem.equipmentSet;
            
            // Return the current equipped item to inventory
            InventorySystem.Instance.AddItem(targetSlot.equippedItem);
        }
        
        // Equip the new item
        targetSlot.equippedItem = item;
        Debug.Log("Equipped: " + item.name + " in " + slot.ToString() + " slot");
        
        // Update set piece counts
        if (oldSetType != EquipmentSet.None)
        {
            equippedSetPieces[oldSetType]--;
            TriggerSetBonusChanged(oldSetType, equippedSetPieces[oldSetType]);
        }
        
        if (item.equipmentSet != EquipmentSet.None)
        {
            equippedSetPieces[item.equipmentSet]++;
            TriggerSetBonusChanged(item.equipmentSet, equippedSetPieces[item.equipmentSet]);
        }
        
        // Remove the item from inventory
        InventorySystem.Instance.RemoveItem(item);
        
        // Recalculate stat bonuses
        RecalculateStatBonuses();
        
        // Update UI
        RefreshEquipmentUI();
        
        // Trigger event
        if (OnEquipmentChanged != null)
        {
            OnEquipmentChanged.Invoke();
        }
        
        return true;
    }
    
    public Item UnequipItem(EquipmentSlot slot, int accessoryIndex = 0)
    {
        EquippedItem targetSlot;
        
        if (slot == EquipmentSlot.Accessory)
        {
            // Handle accessory slots using the index
            targetSlot = (accessoryIndex == 0) ? accessory1Slot : accessory2Slot;
        }
        else
        {
            targetSlot = GetEquipmentSlotByType(slot);
        }
        
        if (targetSlot.IsEmpty())
        {
            Debug.LogWarning("Attempted to unequip from empty slot: " + slot.ToString());
            return null;
        }
        
        // Get the item
        Item unequippedItem = targetSlot.equippedItem;
        
        // Update set piece tracking
        if (unequippedItem.equipmentSet != EquipmentSet.None)
        {
            equippedSetPieces[unequippedItem.equipmentSet]--;
            TriggerSetBonusChanged(unequippedItem.equipmentSet, equippedSetPieces[unequippedItem.equipmentSet]);
        }
        
        // Add it to inventory
        InventorySystem.Instance.AddItem(unequippedItem);
        
        // Clear the slot
        targetSlot.equippedItem = null;
        Debug.Log("Unequipped: " + unequippedItem.name + " from " + slot.ToString() + " slot");
        
        // Recalculate stat bonuses
        RecalculateStatBonuses();
        
        // Update UI
        RefreshEquipmentUI();
        
        // Trigger event
        if (OnEquipmentChanged != null)
        {
            OnEquipmentChanged.Invoke();
        }
        
        return unequippedItem;
    }
    
    private EquippedItem GetEquipmentSlotByType(EquipmentSlot slotType)
    {
        switch (slotType)
        {
            case EquipmentSlot.Weapon:
                return weaponSlot;
            case EquipmentSlot.Armor:
                return armorSlot;
            case EquipmentSlot.Helmet:
                return helmetSlot;
            case EquipmentSlot.Accessory:
                // Default to first accessory slot
                return accessory1Slot;
            default:
                Debug.LogError("Unknown equipment slot type: " + slotType.ToString());
                return null;
        }
    }
    
    public void RecalculateStatBonuses()
    {
        // Reset bonuses
        strengthBonus = 0;
        dexterityBonus = 0;
        intelligenceBonus = 0;
        defenseBonus = 0;
        
        // Reset set bonus stats
        damageModifierBonus = 0f;
        attackSpeedBonus = 0f;
        criticalChanceBonus = 0f;
        healthRegenBonus = 0f;
        manaRegenBonus = 0f;
        
        // Add bonuses from each equipment slot
        AddStatBonusesFromSlot(weaponSlot);
        AddStatBonusesFromSlot(armorSlot);
        AddStatBonusesFromSlot(helmetSlot);
        AddStatBonusesFromSlot(accessory1Slot);
        AddStatBonusesFromSlot(accessory2Slot);
        
        // Apply set bonuses
        ApplySetBonuses();
        
        // Apply stat bonuses to the player
        if (playerController != null)
        {
            playerController.ApplyEquipmentBonuses(
                strengthBonus, 
                dexterityBonus, 
                intelligenceBonus, 
                defenseBonus,
                damageModifierBonus,
                attackSpeedBonus,
                criticalChanceBonus,
                healthRegenBonus,
                manaRegenBonus
            );
        }
    }
    
    private void AddStatBonusesFromSlot(EquippedItem slotItem)
    {
        if (slotItem == null || slotItem.IsEmpty())
            return;
            
        Item item = slotItem.equippedItem;
        strengthBonus += item.strengthBonus;
        dexterityBonus += item.dexterityBonus;
        intelligenceBonus += item.intelligenceBonus;
        defenseBonus += item.defenseBonus;
    }
    
    // Get equipped item by slot
    public Item GetEquippedItem(EquipmentSlot slot, int accessoryIndex = 0)
    {
        EquippedItem targetSlot;
        
        if (slot == EquipmentSlot.Accessory)
        {
            // Handle accessory slots using the index
            targetSlot = (accessoryIndex == 0) ? accessory1Slot : accessory2Slot;
        }
        else
        {
            targetSlot = GetEquipmentSlotByType(slot);
        }
        
        return targetSlot.equippedItem;
    }
    
    // Check if an equipment slot is empty
    public bool IsSlotEmpty(EquipmentSlot slot, int accessoryIndex = 0)
    {
        EquippedItem targetSlot;
        
        if (slot == EquipmentSlot.Accessory)
        {
            // Handle accessory slots using the index
            targetSlot = (accessoryIndex == 0) ? accessory1Slot : accessory2Slot;
        }
        else
        {
            targetSlot = GetEquipmentSlotByType(slot);
        }
        
        return targetSlot.IsEmpty();
    }
    
    // Get total bonuses for display in UI
    public int GetTotalStrengthBonus() { return strengthBonus; }
    public int GetTotalDexterityBonus() { return dexterityBonus; }
    public int GetTotalIntelligenceBonus() { return intelligenceBonus; }
    public int GetTotalDefenseBonus() { return defenseBonus; }
    
    // New method to retrieve and equip an item from the database by ID
    public bool EquipItemById(string itemId)
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("ItemDatabase instance not found!");
            return false;
        }
        
        Item item = ItemDatabase.Instance.GetItemById(itemId);
        if (item == null)
        {
            Debug.LogWarning($"Item with ID '{itemId}' not found in the database!");
            return false;
        }
        
        return EquipItem(item);
    }
    
    // Method to equip a random item of a specific rarity and slot
    public bool EquipRandomItem(EquipmentSlot slot, ItemRarity minimumRarity = ItemRarity.Common)
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("ItemDatabase instance not found!");
            return false;
        }
        
        // Get all equipment for the specified slot
        List<Item> slotItems = ItemDatabase.Instance.GetEquipmentBySlot(slot);
        
        // Filter by minimum rarity
        List<Item> eligibleItems = slotItems.FindAll(item => item.rarity >= minimumRarity);
        
        if (eligibleItems.Count == 0)
        {
            Debug.LogWarning($"No items found for slot {slot} with minimum rarity {minimumRarity}");
            return false;
        }
        
        // Select a random item
        int randomIndex = Random.Range(0, eligibleItems.Count);
        Item selectedItem = eligibleItems[randomIndex];
        
        // Equip the selected item
        return EquipItem(selectedItem);
    }
    
    // Initialize set piece counts
    private void InitializeSetPieceTracking()
    {
        equippedSetPieces.Clear();
        
        // Initialize with all sets having 0 pieces
        foreach (EquipmentSet setType in System.Enum.GetValues(typeof(EquipmentSet)))
        {
            equippedSetPieces[setType] = 0;
        }
    }
    
    // Trigger set bonus changed event
    private void TriggerSetBonusChanged(EquipmentSet set, int pieceCount)
    {
        if (OnSetBonusChanged != null)
        {
            OnSetBonusChanged.Invoke(set, pieceCount);
        }
        
        // Debug log active set bonuses
        List<string> bonusDescriptions = EquipmentSetBonuses.GetSetBonusDescriptions(set, pieceCount);
        
        if (bonusDescriptions.Count > 0)
        {
            Debug.Log($"Set Bonus Status for {set} ({pieceCount} pieces):");
            foreach (string description in bonusDescriptions)
            {
                Debug.Log($"- {description}");
            }
        }
    }
    
    // Apply set bonuses based on equipped set pieces
    private void ApplySetBonuses()
    {
        foreach (var setPieces in equippedSetPieces)
        {
            EquipmentSet set = setPieces.Key;
            int pieceCount = setPieces.Value;
            
            if (set == EquipmentSet.None || pieceCount < 2)
                continue;
                
            // Get active set bonuses for this set and piece count
            List<SetBonusEffect> activeBonuses = EquipmentSetBonuses.GetActiveSetBonuses(set, pieceCount);
            
            foreach (SetBonusEffect bonus in activeBonuses)
            {
                // Apply stat bonuses
                strengthBonus += bonus.strengthBonus;
                dexterityBonus += bonus.dexterityBonus;
                intelligenceBonus += bonus.intelligenceBonus;
                defenseBonus += bonus.defenseBonus;
                
                // Apply other bonuses
                damageModifierBonus += bonus.damageModifier;
                attackSpeedBonus += bonus.speedModifier;
                criticalChanceBonus += bonus.criticalChanceBonus;
                healthRegenBonus += bonus.healthRegenBonus;
                manaRegenBonus += bonus.manaRegenBonus;
                
                // Special effects are handled separately by the player controller
                if (bonus.specialEffect && playerController != null)
                {
                    playerController.ApplySpecialSetEffect(set, bonus.piecesRequired);
                }
            }
        }
    }
    
    // Get the count of equipped pieces for a specific set
    public int GetEquippedSetPieceCount(EquipmentSet set)
    {
        if (equippedSetPieces.ContainsKey(set))
        {
            return equippedSetPieces[set];
        }
        return 0;
    }
    
    // Get all active set bonuses as a formatted string for UI display
    public string GetActiveSetBonusesText()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        foreach (var setPieces in equippedSetPieces)
        {
            EquipmentSet set = setPieces.Key;
            int pieceCount = setPieces.Value;
            
            if (set != EquipmentSet.None && pieceCount >= 2)
            {
                // Get the set color
                Color setColor = GetSetColor(set);
                string colorHex = ColorUtility.ToHtmlStringRGB(setColor);
                
                sb.AppendLine($"<color=#{colorHex}>{set} Set ({pieceCount} pieces)</color>");
                
                List<string> bonusDescriptions = EquipmentSetBonuses.GetSetBonusDescriptions(set, pieceCount);
                foreach (string description in bonusDescriptions)
                {
                    sb.AppendLine($"  {description}");
                }
                
                sb.AppendLine();
            }
        }
        
        if (sb.Length == 0)
        {
            sb.AppendLine("No active set bonuses");
        }
        
        return sb.ToString();
    }
    
    // Helper method to get set color
    private Color GetSetColor(EquipmentSet set)
    {
        // We need a dummy item to get the set color
        Item dummyItem = new Item("", "", "", ItemType.Equipment);
        dummyItem.equipmentSet = set;
        return dummyItem.GetSetColor();
    }
}

[System.Serializable]
public class EquippedItem
{
    public EquipmentSlot slotType;
    public Item equippedItem;
    
    public EquippedItem(EquipmentSlot slotType)
    {
        this.slotType = slotType;
        this.equippedItem = null;
    }
    
    public bool IsEmpty()
    {
        return equippedItem == null;
    }
}

public enum EquipmentSlot
{
    Weapon,
    Armor,
    Helmet,
    Accessory
} 