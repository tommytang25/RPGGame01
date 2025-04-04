using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SetBonusUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject setBonusPanel;
    public Transform setBonusContainer;
    public GameObject setBonusEntryPrefab;
    
    [Header("Settings")]
    public float updateInterval = 0.5f;
    
    private Dictionary<EquipmentSet, GameObject> setBonusEntries = new Dictionary<EquipmentSet, GameObject>();
    private EquipmentSystem equipmentSystem;
    private float updateTimer;
    
    private void Start()
    {
        equipmentSystem = EquipmentSystem.Instance;
        
        if (equipmentSystem == null)
        {
            Debug.LogError("SetBonusUI couldn't find EquipmentSystem instance!");
            return;
        }
        
        // Subscribe to equipment change events
        equipmentSystem.OnEquipmentChanged += UpdateSetBonusDisplay;
        equipmentSystem.OnSetBonusChanged += HandleSetBonusChanged;
        
        // Initialize UI
        if (setBonusPanel != null)
        {
            setBonusPanel.SetActive(false);
        }
        
        updateTimer = updateInterval;
    }
    
    private void Update()
    {
        // Periodically update display in case external changes occurred
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0)
        {
            updateTimer = updateInterval;
            UpdateSetBonusDisplay();
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (equipmentSystem != null)
        {
            equipmentSystem.OnEquipmentChanged -= UpdateSetBonusDisplay;
            equipmentSystem.OnSetBonusChanged -= HandleSetBonusChanged;
        }
    }
    
    public void ToggleSetBonusPanel()
    {
        if (setBonusPanel != null)
        {
            setBonusPanel.SetActive(!setBonusPanel.activeSelf);
            
            if (setBonusPanel.activeSelf)
            {
                UpdateSetBonusDisplay();
            }
        }
    }
    
    private void HandleSetBonusChanged(EquipmentSet set, int pieceCount)
    {
        // Update only the changed set display
        UpdateSetDisplay(set, pieceCount);
    }
    
    public void UpdateSetBonusDisplay()
    {
        if (equipmentSystem == null || setBonusContainer == null)
            return;
            
        // Check all equipment sets
        foreach (EquipmentSet set in System.Enum.GetValues(typeof(EquipmentSet)))
        {
            if (set == EquipmentSet.None)
                continue;
                
            int pieceCount = equipmentSystem.GetEquippedSetPieceCount(set);
            UpdateSetDisplay(set, pieceCount);
        }
    }
    
    private void UpdateSetDisplay(EquipmentSet set, int pieceCount)
    {
        if (set == EquipmentSet.None)
            return;
            
        GameObject entryObject;
        bool isNewEntry = false;
        
        // Get or create entry for this set
        if (!setBonusEntries.TryGetValue(set, out entryObject) || entryObject == null)
        {
            // Only create display for sets with at least 1 piece
            if (pieceCount <= 0)
                return;
                
            // Create new entry
            entryObject = Instantiate(setBonusEntryPrefab, setBonusContainer);
            setBonusEntries[set] = entryObject;
            isNewEntry = true;
        }
        
        // Remove entries with 0 pieces
        if (pieceCount <= 0)
        {
            Destroy(entryObject);
            setBonusEntries.Remove(set);
            return;
        }
        
        // Update entry UI
        SetBonusEntry entry = entryObject.GetComponent<SetBonusEntry>();
        if (entry != null)
        {
            entry.UpdateSetBonusDisplay(set, pieceCount);
        }
        
        // Otherwise fall back to manual update
        if (entry == null || isNewEntry)
        {
            ManuallyUpdateEntryUI(entryObject, set, pieceCount);
        }
    }
    
    // Fallback manual update if the SetBonusEntry component doesn't exist
    private void ManuallyUpdateEntryUI(GameObject entryObject, EquipmentSet set, int pieceCount)
    {
        // Get the set color
        Color setColor = GetSetColor(set);
        
        // Find UI elements by name
        Transform titleTransform = entryObject.transform.Find("SetTitle");
        Transform countTransform = entryObject.transform.Find("PieceCount");
        Transform bonusesTransform = entryObject.transform.Find("BonusesList");
        
        // Update title
        if (titleTransform != null)
        {
            TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
            {
                titleText.text = set.ToString();
                titleText.color = setColor;
            }
        }
        
        // Update piece count
        if (countTransform != null)
        {
            TextMeshProUGUI countText = countTransform.GetComponent<TextMeshProUGUI>();
            if (countText != null)
            {
                countText.text = $"{pieceCount} piece{(pieceCount > 1 ? "s" : "")} equipped";
            }
        }
        
        // Update bonuses list
        if (bonusesTransform != null)
        {
            TextMeshProUGUI bonusesText = bonusesTransform.GetComponent<TextMeshProUGUI>();
            if (bonusesText != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                
                // Get all bonus descriptions for this set
                List<string> bonusDescriptions = EquipmentSetBonuses.GetSetBonusDescriptions(set, pieceCount);
                
                foreach (string description in bonusDescriptions)
                {
                    sb.AppendLine("• " + description);
                }
                
                // Add information about upcoming tier bonuses
                List<SetBonusEffect> allBonuses = EquipmentSetBonuses.GetAllSetBonuses(set);
                if (allBonuses != null)
                {
                    foreach (SetBonusEffect effect in allBonuses)
                    {
                        if (effect.piecesRequired > pieceCount)
                        {
                            string colorHex = ColorUtility.ToHtmlStringRGB(Color.grey);
                            sb.AppendLine($"<color=#{colorHex}>• ({effect.piecesRequired} pieces) {effect.description}</color>");
                        }
                    }
                }
                
                bonusesText.text = sb.ToString();
            }
        }
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

// Component for individual set bonus display entries
public class SetBonusEntry : MonoBehaviour
{
    public TextMeshProUGUI setTitleText;
    public TextMeshProUGUI pieceCountText;
    public TextMeshProUGUI bonusesText;
    
    public void UpdateSetBonusDisplay(EquipmentSet set, int pieceCount)
    {
        // Skip if none
        if (set == EquipmentSet.None)
            return;
            
        // Get color for this set
        Color setColor = GetSetColor(set);
        
        // Update set title
        if (setTitleText != null)
        {
            setTitleText.text = set.ToString();
            setTitleText.color = setColor;
        }
        
        // Update piece count
        if (pieceCountText != null)
        {
            pieceCountText.text = $"{pieceCount} piece{(pieceCount > 1 ? "s" : "")} equipped";
        }
        
        // Update bonuses description
        if (bonusesText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // Get all active bonus descriptions for this set
            List<string> bonusDescriptions = EquipmentSetBonuses.GetSetBonusDescriptions(set, pieceCount);
            
            foreach (string description in bonusDescriptions)
            {
                sb.AppendLine("• " + description);
            }
            
            // Add information about upcoming tier bonuses
            List<SetBonusEffect> allBonuses = EquipmentSetBonuses.GetAllSetBonuses(set);
            if (allBonuses != null)
            {
                foreach (SetBonusEffect effect in allBonuses)
                {
                    if (effect.piecesRequired > pieceCount)
                    {
                        string colorHex = ColorUtility.ToHtmlStringRGB(Color.grey);
                        sb.AppendLine($"<color=#{colorHex}>• ({effect.piecesRequired} pieces) {effect.description}</color>");
                    }
                }
            }
            
            bonusesText.text = sb.ToString();
        }
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