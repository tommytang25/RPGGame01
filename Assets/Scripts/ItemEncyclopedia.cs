using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

// This component provides a read-only view of all game items as an in-game encyclopedia/wiki
public class ItemEncyclopedia : MonoBehaviour
{
    [Header("UI References")]
    public GameObject itemDatabasePanel;
    public Transform itemListContent;
    public GameObject itemEntryPrefab;
    public TMP_Dropdown categoryDropdown;
    public TMP_Dropdown rarityDropdown;
    public Button refreshButton;
    public GameObject itemDetailPanel;
    public TextMeshProUGUI encyclopediaHeaderText;
    public TextMeshProUGUI encyclopediaDescriptionText;
    
    [Header("Item Detail UI")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemStatsText;
    public Image itemIconImage;
    public Button closeDetailButton;
    
    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.I;
    public KeyCode modifierKey = KeyCode.LeftShift;
    public string panelTitle = "Item Encyclopedia";
    
    private ItemDatabase itemDatabase;
    private Item selectedItem;
    
    private void Start()
    {
        // Initialize UI
        if (itemDatabasePanel != null)
        {
            itemDatabasePanel.SetActive(false);
        }
        
        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(false);
        }
        
        // Set encyclopedia header text
        if (encyclopediaHeaderText != null)
        {
            encyclopediaHeaderText.text = panelTitle;
        }
        
        // Set encyclopedia description
        if (encyclopediaDescriptionText != null)
        {
            encyclopediaDescriptionText.text = "This encyclopedia contains information about all items in the game world. Use this reference to learn about items, their properties, and where they can be found.";
        }
        
        // Set up button listeners
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshItemList);
        }
        
        if (closeDetailButton != null)
        {
            closeDetailButton.onClick.AddListener(CloseItemDetail);
        }
        
        // Set up dropdown listeners
        if (categoryDropdown != null)
        {
            categoryDropdown.onValueChanged.AddListener(delegate { RefreshItemList(); });
        }
        
        if (rarityDropdown != null)
        {
            rarityDropdown.onValueChanged.AddListener(delegate { RefreshItemList(); });
        }
        
        // Find database reference
        itemDatabase = FindObjectOfType<ItemDatabase>();
        
        // Initialize dropdowns
        InitializeDropdowns();
    }
    
    private void Update()
    {
        // Toggle item database view with configured keys
        if (Input.GetKeyDown(toggleKey) && Input.GetKey(modifierKey))
        {
            ToggleItemDatabaseView();
        }
    }
    
    private void InitializeDropdowns()
    {
        // Initialize category dropdown
        if (categoryDropdown != null)
        {
            categoryDropdown.ClearOptions();
            
            List<TMP_Dropdown.OptionData> categoryOptions = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("All Items"),
                new TMP_Dropdown.OptionData("Weapons"),
                new TMP_Dropdown.OptionData("Armor"),
                new TMP_Dropdown.OptionData("Helmets"),
                new TMP_Dropdown.OptionData("Accessories"),
                new TMP_Dropdown.OptionData("Consumables"),
                new TMP_Dropdown.OptionData("Resources")
            };
            
            categoryDropdown.AddOptions(categoryOptions);
        }
        
        // Initialize rarity dropdown
        if (rarityDropdown != null)
        {
            rarityDropdown.ClearOptions();
            
            List<TMP_Dropdown.OptionData> rarityOptions = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("All Rarities"),
                new TMP_Dropdown.OptionData("Common"),
                new TMP_Dropdown.OptionData("Uncommon"),
                new TMP_Dropdown.OptionData("Rare"),
                new TMP_Dropdown.OptionData("Epic"),
                new TMP_Dropdown.OptionData("Legendary")
            };
            
            rarityDropdown.AddOptions(rarityOptions);
        }
    }
    
    public void ToggleItemDatabaseView()
    {
        if (itemDatabasePanel != null)
        {
            bool isActive = itemDatabasePanel.activeSelf;
            itemDatabasePanel.SetActive(!isActive);
            
            if (!isActive)
            {
                // Refresh the item list when showing the panel
                RefreshItemList();
            }
            else
            {
                // Close detail panel if we're closing the main panel
                if (itemDetailPanel != null && itemDetailPanel.activeSelf)
                {
                    itemDetailPanel.SetActive(false);
                }
            }
        }
    }
    
    public void RefreshItemList()
    {
        if (itemListContent == null || itemEntryPrefab == null || itemDatabase == null)
        {
            Debug.LogWarning("Missing references for ItemEncyclopedia.RefreshItemList()");
            return;
        }
        
        // Clear existing items
        foreach (Transform child in itemListContent)
        {
            Destroy(child.gameObject);
        }
        
        // Get filtered items based on selected category and rarity
        List<Item> filteredItems = GetFilteredItems();
        
        // Display item count
        if (encyclopediaDescriptionText != null)
        {
            encyclopediaDescriptionText.text = $"Displaying {filteredItems.Count} items. Use the dropdowns to filter by category or rarity.";
        }
        
        // Create item entries
        foreach (Item item in filteredItems)
        {
            GameObject entryObj = Instantiate(itemEntryPrefab, itemListContent);
            
            // Set up the entry UI
            TextMeshProUGUI nameText = entryObj.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                // Use the enhanced colored name with rarity
                string colorHex = ColorUtility.ToHtmlStringRGB(item.GetRarityColor());
                string raritySymbol = GetRaritySymbol(item.rarity);
                nameText.text = $"<color=#{colorHex}>{raritySymbol} {item.name}</color>";
                
                // The color is already set in the rich text tag, so we don't need to set it again
                // nameText.color = item.GetRarityColor();
            }
            
            // Add click listener
            Button entryButton = entryObj.GetComponent<Button>();
            if (entryButton != null)
            {
                // Need to create a local copy for the closure
                Item itemCopy = item;
                entryButton.onClick.AddListener(delegate { ShowItemDetail(itemCopy); });
            }
            
            // Set item icon if it exists
            Image iconImage = entryObj.GetComponentInChildren<Image>();
            if (iconImage != null && item.icon != null)
            {
                iconImage.sprite = item.icon;
            }
        }
    }
    
    private List<Item> GetFilteredItems()
    {
        List<Item> result = new List<Item>();
        
        // Get items based on category selection
        int categoryIndex = categoryDropdown != null ? categoryDropdown.value : 0;
        List<Item> categoryItems = new List<Item>();
        
        switch (categoryIndex)
        {
            case 0: // All Items
                categoryItems = itemDatabase.allItems;
                break;
            case 1: // Weapons
                categoryItems = itemDatabase.weapons;
                break;
            case 2: // Armor
                categoryItems = itemDatabase.armor;
                break;
            case 3: // Helmets
                categoryItems = itemDatabase.helmets;
                break;
            case 4: // Accessories
                categoryItems = itemDatabase.accessories;
                break;
            case 5: // Consumables
                categoryItems = itemDatabase.consumables;
                break;
            case 6: // Resources
                categoryItems = itemDatabase.resources;
                break;
            default:
                categoryItems = itemDatabase.allItems;
                break;
        }
        
        // Filter by rarity if a specific rarity is selected
        int rarityIndex = rarityDropdown != null ? rarityDropdown.value : 0;
        
        if (rarityIndex == 0) // All Rarities
        {
            result = new List<Item>(categoryItems);
        }
        else
        {
            ItemRarity selectedRarity = (ItemRarity)(rarityIndex - 1); // -1 because "All Rarities" is at index 0
            
            foreach (Item item in categoryItems)
            {
                if (item.rarity == selectedRarity)
                {
                    result.Add(item);
                }
            }
        }
        
        return result;
    }
    
    public void ShowItemDetail(Item item)
    {
        if (itemDetailPanel == null || item == null)
        {
            return;
        }
        
        selectedItem = item;
        
        // Show the detail panel
        itemDetailPanel.SetActive(true);
        
        // Update UI with item details
        if (itemNameText != null)
        {
            // Enhanced colored name
            itemNameText.text = item.GetColoredName();
        }
        
        if (itemDescriptionText != null)
        {
            // Add rarity at the top of the description in its appropriate color
            string rarityText = item.GetRarityDisplay();
            itemDescriptionText.text = $"{rarityText}\n\n{item.description}";
        }
        
        if (itemStatsText != null)
        {
            string stats = GetItemStatsText(item);
            itemStatsText.text = stats;
        }
        
        if (itemIconImage != null && item.icon != null)
        {
            itemIconImage.sprite = item.icon;
        }
    }
    
    private string GetItemStatsText(Item item)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        // Item type with color
        string typeColor = "#CCCCCC"; // Gray
        sb.AppendLine($"Type: <color={typeColor}>{item.type}</color>");
        
        // Rarity is now shown at the top of the description instead
        
        // Value with color based on amount
        string valueColor = GetValueColor(item.CalculateValue());
        sb.AppendLine($"Value: <color={valueColor}>{item.CalculateValue():N0}</color> gold");
        
        if (item.type == ItemType.Equipment)
        {
            sb.AppendLine($"Slot: <color=#CCCCCC>{item.equipmentSlot}</color>");
            
            if (item.strengthBonus != 0)
                sb.AppendLine($"Strength: {GetColoredStatValue(item.strengthBonus)}");
                
            if (item.dexterityBonus != 0)
                sb.AppendLine($"Dexterity: {GetColoredStatValue(item.dexterityBonus)}");
                
            if (item.intelligenceBonus != 0)
                sb.AppendLine($"Intelligence: {GetColoredStatValue(item.intelligenceBonus)}");
                
            if (item.defenseBonus != 0)
                sb.AppendLine($"Defense: {GetColoredStatValue(item.defenseBonus)}");
            
            if (item.equipmentSlot == EquipmentSlot.Weapon)
            {
                sb.AppendLine($"Weapon Type: <color=#CCCCCC>{item.weaponType}</color>");
                
                if (item.attackSpeedModifier != 0)
                    sb.AppendLine($"Attack Speed: {GetColoredStatValue(item.attackSpeedModifier, true)}");
                    
                if (item.damageModifier != 0)
                    sb.AppendLine($"Damage Modifier: {GetColoredStatValue(item.damageModifier, true)}");
            }
            
            if (item.movementSpeedModifier != 0)
                sb.AppendLine($"Movement Speed: {GetColoredStatValue(item.movementSpeedModifier, true)}");
        }
        else if (item.type == ItemType.Consumable)
        {
            if (item.healthRestoreAmount > 0)
                sb.AppendLine($"Health Restore: <color=#00CC00>{item.healthRestoreAmount}</color>");
                
            if (item.staminaRestoreAmount > 0)
                sb.AppendLine($"Stamina Restore: <color=#00AAFF>{item.staminaRestoreAmount}</color>");
                
            if (item.buffDuration > 0)
                sb.AppendLine($"Buff Duration: <color=#FFAA00>{item.buffDuration}s</color>");
        }
        
        // Add drop information if available
        if (!string.IsNullOrEmpty(item.dropsFrom))
        {
            sb.AppendLine();
            sb.AppendLine($"<color=#CCCCCC>SOURCE INFORMATION</color>");
            sb.AppendLine($"Drops From: <color=#AADDFF>{item.dropsFrom}</color>");
            
            // Color the drop rate based on rarity (lower drop rate = rarer = more vibrant color)
            string dropRateColor = GetDropRateColor(item.dropRate);
            sb.AppendLine($"Drop Rate: <color={dropRateColor}>{(item.dropRate * 100):F1}%</color>");
        }
        
        return sb.ToString();
    }
    
    // Helper method to color stat values
    private string GetColoredStatValue(float value, bool isPercentage = false)
    {
        string valueText;
        string color;
        
        if (isPercentage)
        {
            // Format as percentage
            valueText = $"{value:+0.00;-0.00;0}";
            
            if (value > 0.3f)
                color = "#00FF00"; // Strong positive: bright green
            else if (value > 0)
                color = "#88FF88"; // Moderate positive: light green
            else if (value < -0.3f)
                color = "#FF0000"; // Strong negative: bright red
            else if (value < 0)
                color = "#FF8888"; // Moderate negative: light red
            else
                color = "#FFFFFF"; // Zero: white
        }
        else
        {
            // Format as integer
            valueText = $"{value:+0;-0;0}";
            
            if (value > 10)
                color = "#00FF00"; // Strong positive: bright green
            else if (value > 0)
                color = "#88FF88"; // Moderate positive: light green
            else if (value < -10)
                color = "#FF0000"; // Strong negative: bright red
            else if (value < 0)
                color = "#FF8888"; // Moderate negative: light red
            else
                color = "#FFFFFF"; // Zero: white
        }
        
        return $"<color={color}>{valueText}</color>";
    }
    
    // Helper method to color drop rates
    private string GetDropRateColor(float dropRate)
    {
        if (dropRate <= 0.01f)       // 1% or less
            return "#FF00FF";        // Pink/purple for extremely rare
        else if (dropRate <= 0.05f)  // 5% or less
            return "#AA00FF";        // Purple for very rare
        else if (dropRate <= 0.15f)  // 15% or less
            return "#0088FF";        // Blue for rare
        else if (dropRate <= 0.30f)  // 30% or less
            return "#00AA00";        // Green for uncommon
        else
            return "#AAAAAA";        // Gray for common
    }
    
    // Helper method to color value amounts
    private string GetValueColor(int value)
    {
        if (value >= 1000)
            return "#FFD700"; // Gold for valuable items
        else if (value >= 500)
            return "#FFA500"; // Orange for moderate value
        else if (value >= 100)
            return "#FFFF00"; // Yellow for some value
        else
            return "#AAAAAA"; // Gray for low value
    }
    
    // Helper method to get the rarity symbol
    private string GetRaritySymbol(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return "●";
            case ItemRarity.Uncommon:
                return "★";
            case ItemRarity.Rare:
                return "★★";
            case ItemRarity.Epic:
                return "★★★";
            case ItemRarity.Legendary:
                return "✦✦✦";
            default:
                return "";
        }
    }
    
    public void CloseItemDetail()
    {
        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(false);
            selectedItem = null;
        }
    }
} 