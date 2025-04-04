using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    // Singleton instance
    public static InventorySystem Instance { get; private set; }
    
    [Header("Inventory Settings")]
    public int maxInventorySize = 20;
    
    // The list of items in the inventory
    public List<Item> items = new List<Item>();
    
    // UI reference
    public GameObject inventoryUI;
    public Transform itemsParent; // Parent transform for item slots in UI
    
    // Currently selected item
    private Item selectedItem;
    
    // New variables for equipment integration
    private EquipmentSlot currentEquipmentSlotFilter = EquipmentSlot.Weapon;
    private int currentAccessoryIndex = 0;
    private bool isEquippingMode = false;
    
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
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Toggle inventory
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }
    
    public void ToggleInventory()
    {
        if (inventoryUI != null)
        {
            bool isActive = !inventoryUI.activeSelf;
            inventoryUI.SetActive(isActive);
            
            if (isActive)
            {
                RefreshInventoryUI();
            }
        }
    }
    
    // New method for equipment integration
    public void OpenInventoryForEquipping(EquipmentSlot targetSlot, int accessoryIndex = 0)
    {
        if (inventoryUI != null && !inventoryUI.activeSelf)
        {
            inventoryUI.SetActive(true);
        }
        
        // Set a filter for the inventory to only show relevant equipment
        currentEquipmentSlotFilter = targetSlot;
        currentAccessoryIndex = accessoryIndex;
        isEquippingMode = true;
        
        // Show only equipment of the relevant type
        RefreshInventoryUI(true);
        
        // Display a message to the player
        Debug.Log($"Select an item to equip in the {targetSlot} slot");
    }
    
    public bool AddItem(Item item)
    {
        if (items.Count >= maxInventorySize)
        {
            Debug.Log("Inventory is full!");
            return false;
        }
        
        items.Add(item);
        
        // Refresh UI if inventory is open
        if (inventoryUI != null && inventoryUI.activeSelf)
        {
            RefreshInventoryUI();
        }
        
        return true;
    }
    
    public void RemoveItem(Item item)
    {
        items.Remove(item);
        
        // Refresh UI if inventory is open
        if (inventoryUI != null && inventoryUI.activeSelf)
        {
            RefreshInventoryUI();
        }
    }
    
    public void UseItem(Item item)
    {
        // Handle different item types
        switch (item.type)
        {
            case ItemType.Consumable:
                // Apply consumable effect
                ApplyConsumableEffect(item);
                // Remove item after use
                RemoveItem(item);
                break;
                
            case ItemType.Equipment:
                // Equip/unequip item
                EquipItem(item);
                break;
                
            case ItemType.QuestItem:
                // Quest items just display info, they can't be "used"
                Debug.Log("This is a quest item: " + item.description);
                break;
                
            case ItemType.CraftingMaterial:
                // Crafting materials can be used in crafting UI
                Debug.Log("This is a crafting material: " + item.description);
                break;
        }
    }
    
    private void ApplyConsumableEffect(Item item)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return;
        
        // Apply effects based on item properties
        // For example, healing potions restore health
        if (item.healthRestoreAmount > 0)
        {
            player.currentHealth = Mathf.Min(player.currentHealth + item.healthRestoreAmount, player.maxHealth);
            Debug.Log("Restored " + item.healthRestoreAmount + " health!");
        }
        
        // TODO: Implement other consumable effects like temporary stat boosts
    }
    
    private void EquipItem(Item item)
    {
        // TODO: Implement equipment system
        Debug.Log("Equipped: " + item.name);
    }
    
    // Modified RefreshInventoryUI to handle equipment filtering
    public void RefreshInventoryUI(bool filterForEquipping = false)
    {
        if (inventoryUI == null || itemsParent == null)
            return;
        
        // Clear existing items
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }
        
        // Populate UI with filtered items
        List<Item> filteredItems = new List<Item>();
        if (filterForEquipping)
        {
            foreach (Item item in items)
            {
                if (item.type == ItemType.Equipment && item.equipmentSlot == currentEquipmentSlotFilter)
                {
                    filteredItems.Add(item);
                }
            }
        }
        else
        {
            filteredItems = items;
        }
        
        // Create UI elements for each item
        foreach (Item item in filteredItems)
        {
            GameObject itemSlot = Instantiate(itemPrefab, itemsParent);
            ItemSlot itemSlotScript = itemSlot.GetComponent<ItemSlot>();
            if (itemSlotScript != null)
            {
                itemSlotScript.Setup(item);
            }
        }
    }
    
    public void SelectItem(Item item)
    {
        selectedItem = item;
        // TODO: Show item details in UI
    }

    public void UseSelectedItem()
    {
        if (selectedItem == null)
            return;
            
        // Handle equipment items differently if in equipping mode
        if (isEquippingMode && selectedItem.type == ItemType.Equipment)
        {
            if (selectedItem.equipmentSlot == currentEquipmentSlotFilter || 
                (currentEquipmentSlotFilter == EquipmentSlot.Accessory && selectedItem.equipmentSlot == EquipmentSlot.Accessory))
            {
                // Equip the item
                if (EquipmentSystem.Instance != null)
                {
                    EquipmentSystem.Instance.EquipItem(selectedItem);
                    
                    // Exit equipping mode
                    isEquippingMode = false;
                    currentEquipmentSlotFilter = EquipmentSlot.Weapon; // Reset to default
                    
                    // Refresh the UI
                    RefreshInventoryUI();
                }
            }
            else
            {
                Debug.LogWarning($"This item cannot be equipped in the {currentEquipmentSlotFilter} slot");
            }
            
            return;
        }
        
        // Normal item usage
        switch (selectedItem.type)
        {
            case ItemType.Consumable:
                // Apply consumable effect
                ApplyConsumableEffect(selectedItem);
                // Remove item after use
                RemoveItem(selectedItem);
                break;
                
            case ItemType.Equipment:
                // Equip/unequip item
                EquipItem(selectedItem);
                break;
                
            case ItemType.QuestItem:
                // Quest items just display info, they can't be "used"
                Debug.Log("This is a quest item: " + selectedItem.description);
                break;
                
            case ItemType.CraftingMaterial:
                // Crafting materials can be used in crafting UI
                Debug.Log("This is a crafting material: " + selectedItem.description);
                break;
        }
    }
}

[System.Serializable]
public class Item
{
    public string id;
    public string name;
    public string description;
    public ItemType type;
    public Sprite icon;
    
    // Stats for equipment
    public int strengthBonus;
    public int dexterityBonus;
    public int intelligenceBonus;
    
    // For consumables
    public int healthRestoreAmount;
    
    // For crafting
    public bool isModernTechnology;
    public List<string> craftingRequirements;
    
    // For quest items
    public string relatedQuestId;
    
    // New variable for equipment slot
    public EquipmentSlot equipmentSlot;
    
    public Item(string id, string name, string description, ItemType type)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.type = type;
    }
}

public enum ItemType
{
    Consumable,
    Equipment,
    QuestItem,
    CraftingMaterial
} 