using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject equipmentPanel;
    public Transform equipmentSlotsContainer;
    public GameObject setBonusesPanel;
    public Button toggleSetBonusesButton;
    public TextMeshProUGUI statBonusesText;
    
    [Header("Slot UI References")]
    public Image weaponSlotImage;
    public Image armorSlotImage;
    public Image helmetSlotImage;
    public Image accessory1SlotImage;
    public Image accessory2SlotImage;
    
    private EquipmentSystem equipmentSystem;
    private SetBonusUI setBonusUI;
    
    private void Start()
    {
        equipmentSystem = EquipmentSystem.Instance;
        setBonusUI = GetComponent<SetBonusUI>();
        
        if (equipmentSystem == null)
        {
            Debug.LogError("EquipmentUIManager: EquipmentSystem instance not found!");
            return;
        }
        
        // Initialize UI
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
        }
        
        // Set up button events
        if (toggleSetBonusesButton != null)
        {
            toggleSetBonusesButton.onClick.AddListener(ToggleSetBonusesPanel);
        }
        
        // Subscribe to equipment change events
        equipmentSystem.OnEquipmentChanged += RefreshUI;
        equipmentSystem.OnSetBonusChanged += (set, count) => RefreshUI();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (equipmentSystem != null)
        {
            equipmentSystem.OnEquipmentChanged -= RefreshUI;
        }
        
        // Remove button listeners
        if (toggleSetBonusesButton != null)
        {
            toggleSetBonusesButton.onClick.RemoveListener(ToggleSetBonusesPanel);
        }
    }
    
    public void ToggleEquipmentPanel()
    {
        if (equipmentPanel != null)
        {
            bool isActive = !equipmentPanel.activeSelf;
            equipmentPanel.SetActive(isActive);
            
            if (isActive)
            {
                RefreshUI();
            }
        }
    }
    
    public void ToggleSetBonusesPanel()
    {
        if (setBonusUI != null)
        {
            setBonusUI.ToggleSetBonusPanel();
        }
        else if (setBonusesPanel != null)
        {
            setBonusesPanel.SetActive(!setBonusesPanel.activeSelf);
        }
    }
    
    public void RefreshUI()
    {
        if (equipmentSystem == null)
            return;
            
        // Update slot images
        UpdateSlotImage(weaponSlotImage, EquipmentSlot.Weapon);
        UpdateSlotImage(armorSlotImage, EquipmentSlot.Armor);
        UpdateSlotImage(helmetSlotImage, EquipmentSlot.Helmet);
        UpdateSlotImage(accessory1SlotImage, EquipmentSlot.Accessory, 0);
        UpdateSlotImage(accessory2SlotImage, EquipmentSlot.Accessory, 1);
        
        // Update stat bonuses text
        UpdateStatBonusesText();
        
        // Update set bonuses if SetBonusUI is attached
        if (setBonusUI != null)
        {
            setBonusUI.UpdateSetBonusDisplay();
        }
    }
    
    private void UpdateSlotImage(Image slotImage, EquipmentSlot slot, int accessoryIndex = 0)
    {
        if (slotImage == null)
            return;
            
        Item equippedItem = equipmentSystem.GetEquippedItem(slot, accessoryIndex);
        
        if (equippedItem != null && equippedItem.icon != null)
        {
            slotImage.sprite = equippedItem.icon;
            slotImage.color = Color.white;
            
            // Add set coloring indicator
            if (equippedItem.equipmentSet != EquipmentSet.None)
            {
                // Add a colored border or indicator
                Image borderImage = slotImage.transform.Find("SetBorder")?.GetComponent<Image>();
                if (borderImage != null)
                {
                    borderImage.gameObject.SetActive(true);
                    borderImage.color = equippedItem.GetSetColor();
                }
            }
        }
        else
        {
            // Empty slot
            slotImage.sprite = null;
            slotImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            
            // Hide set border if any
            Image borderImage = slotImage.transform.Find("SetBorder")?.GetComponent<Image>();
            if (borderImage != null)
            {
                borderImage.gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateStatBonusesText()
    {
        if (statBonusesText == null || equipmentSystem == null)
            return;
            
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        // Add main stat bonuses
        sb.AppendLine($"Strength: +{equipmentSystem.GetTotalStrengthBonus()}");
        sb.AppendLine($"Dexterity: +{equipmentSystem.GetTotalDexterityBonus()}");
        sb.AppendLine($"Intelligence: +{equipmentSystem.GetTotalIntelligenceBonus()}");
        sb.AppendLine($"Defense: +{equipmentSystem.GetTotalDefenseBonus()}");
        
        // Add special stat bonuses (these would need new getter methods in EquipmentSystem)
        // sb.AppendLine($"Critical Chance: +{equipmentSystem.GetCriticalChanceBonus():P1}");
        // sb.AppendLine($"Attack Speed: +{equipmentSystem.GetAttackSpeedBonus():P1}");
        
        statBonusesText.text = sb.ToString();
    }
    
    // Button handlers for equipment slots
    public void OnWeaponSlotClicked()
    {
        HandleEquipmentSlotClicked(EquipmentSlot.Weapon);
    }
    
    public void OnArmorSlotClicked()
    {
        HandleEquipmentSlotClicked(EquipmentSlot.Armor);
    }
    
    public void OnHelmetSlotClicked()
    {
        HandleEquipmentSlotClicked(EquipmentSlot.Helmet);
    }
    
    public void OnAccessory1SlotClicked()
    {
        HandleEquipmentSlotClicked(EquipmentSlot.Accessory, 0);
    }
    
    public void OnAccessory2SlotClicked()
    {
        HandleEquipmentSlotClicked(EquipmentSlot.Accessory, 1);
    }
    
    private void HandleEquipmentSlotClicked(EquipmentSlot slot, int accessoryIndex = 0)
    {
        if (equipmentSystem == null)
            return;
            
        // Check if there's an item in the slot
        if (!equipmentSystem.IsSlotEmpty(slot, accessoryIndex))
        {
            // Unequip the item
            equipmentSystem.UnequipItem(slot, accessoryIndex);
        }
        else
        {
            // Open inventory to equip an item
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OpenInventoryForEquipping(slot, accessoryIndex);
            }
        }
    }
} 