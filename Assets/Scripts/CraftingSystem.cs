using UnityEngine;
using System.Collections.Generic;

public class CraftingSystem : MonoBehaviour
{
    // Singleton instance
    public static CraftingSystem Instance { get; private set; }
    
    [Header("UI References")]
    public GameObject craftingUI;
    public Transform recipeContainer;
    public GameObject recipeButtonPrefab;
    
    [Header("Crafting Settings")]
    public float craftingTime = 2f;
    
    // List of all available recipes
    public List<CraftingRecipe> availableRecipes = new List<CraftingRecipe>();
    
    // List of recipes the player has unlocked
    private List<CraftingRecipe> unlockedRecipes = new List<CraftingRecipe>();
    
    // Currently selected recipe
    private CraftingRecipe selectedRecipe;
    
    // Skills that affect crafting
    private int craftingSkillLevel = 1;
    private int modernKnowledgeLevel = 1;
    
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
        if (craftingUI != null)
        {
            craftingUI.SetActive(false);
        }
        
        // Add basic crafting recipes that are available from the start
        UnlockStartingRecipes();
    }
    
    private void Update()
    {
        // Toggle crafting UI
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCraftingUI();
        }
    }
    
    private void UnlockStartingRecipes()
    {
        // Find recipes with no level requirements
        foreach (CraftingRecipe recipe in availableRecipes)
        {
            if (recipe.requiredCraftingLevel <= 1 && recipe.requiredModernKnowledgeLevel <= 1)
            {
                UnlockRecipe(recipe);
            }
        }
    }
    
    public void UnlockRecipe(CraftingRecipe recipe)
    {
        if (!unlockedRecipes.Contains(recipe))
        {
            unlockedRecipes.Add(recipe);
            Debug.Log("New recipe unlocked: " + recipe.itemName);
        }
    }
    
    public void ToggleCraftingUI()
    {
        if (craftingUI != null)
        {
            bool isActive = craftingUI.activeSelf;
            craftingUI.SetActive(!isActive);
            
            if (!isActive)
            {
                // Opening UI
                Time.timeScale = 0f;
                RefreshCraftingUI();
            }
            else
            {
                // Closing UI
                Time.timeScale = 1f;
            }
        }
    }
    
    private void RefreshCraftingUI()
    {
        if (recipeContainer == null || recipeButtonPrefab == null)
            return;
            
        // Clear existing recipe buttons
        foreach (Transform child in recipeContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create buttons for each unlocked recipe
        foreach (CraftingRecipe recipe in unlockedRecipes)
        {
            GameObject recipeButton = Instantiate(recipeButtonPrefab, recipeContainer);
            
            // Set button text/icon (assuming button has a text component)
            UnityEngine.UI.Text buttonText = recipeButton.GetComponentInChildren<UnityEngine.UI.Text>();
            if (buttonText != null)
            {
                buttonText.text = recipe.itemName;
            }
            
            // Set button icon if available
            if (recipe.resultItemIcon != null)
            {
                UnityEngine.UI.Image buttonImage = recipeButton.GetComponentInChildren<UnityEngine.UI.Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = recipe.resultItemIcon;
                }
            }
            
            // Add button click handler
            UnityEngine.UI.Button button = recipeButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                CraftingRecipe currentRecipe = recipe; // Store in local variable for closure
                button.onClick.AddListener(() => SelectRecipe(currentRecipe));
            }
        }
    }
    
    private void SelectRecipe(CraftingRecipe recipe)
    {
        selectedRecipe = recipe;
        
        // TODO: Update UI to show selected recipe details and materials
        Debug.Log("Selected recipe: " + recipe.itemName);
        
        // Check if player has the required materials
        bool canCraft = CanCraftRecipe(recipe);
        
        // Enable/disable craft button based on available materials
        // TODO: Update craft button UI
        Debug.Log("Can craft: " + canCraft);
    }
    
    public bool CanCraftRecipe(CraftingRecipe recipe)
    {
        // Check if player has the required skill levels
        if (craftingSkillLevel < recipe.requiredCraftingLevel ||
            modernKnowledgeLevel < recipe.requiredModernKnowledgeLevel)
        {
            return false;
        }
        
        // Check if player has all required items
        foreach (CraftingIngredient ingredient in recipe.ingredients)
        {
            // Count how many of this ingredient the player has
            int count = CountItemsInInventory(ingredient.itemId);
            
            if (count < ingredient.amount)
            {
                return false;
            }
        }
        
        return true;
    }
    
    private int CountItemsInInventory(string itemId)
    {
        // Get count of items with this ID from inventory
        int count = 0;
        
        foreach (Item item in InventorySystem.Instance.items)
        {
            if (item.id == itemId)
            {
                count++;
            }
        }
        
        return count;
    }
    
    public void CraftSelectedRecipe()
    {
        if (selectedRecipe == null)
            return;
            
        if (CanCraftRecipe(selectedRecipe))
        {
            // Remove ingredients from inventory
            foreach (CraftingIngredient ingredient in selectedRecipe.ingredients)
            {
                for (int i = 0; i < ingredient.amount; i++)
                {
                    // Find and remove the item
                    RemoveItemFromInventory(ingredient.itemId);
                }
            }
            
            // Create the crafted item
            Item craftedItem = CreateItemFromRecipe(selectedRecipe);
            
            // Add to inventory
            if (craftedItem != null)
            {
                InventorySystem.Instance.AddItem(craftedItem);
                Debug.Log("Crafted: " + craftedItem.name);
                
                // Apply any special modern technology effects
                if (selectedRecipe.isModernTechnology)
                {
                    ApplyModernTechnologyEffect(selectedRecipe, craftedItem);
                }
            }
            
            // Refresh crafting UI
            RefreshCraftingUI();
        }
        else
        {
            Debug.Log("Cannot craft item: missing ingredients or skills");
        }
    }
    
    private void RemoveItemFromInventory(string itemId)
    {
        // Find and remove a single item with this ID
        foreach (Item item in InventorySystem.Instance.items)
        {
            if (item.id == itemId)
            {
                InventorySystem.Instance.RemoveItem(item);
                break;
            }
        }
    }
    
    private Item CreateItemFromRecipe(CraftingRecipe recipe)
    {
        // Create a new item based on the recipe
        Item newItem = new Item(
            recipe.resultItemId,
            recipe.itemName,
            recipe.itemDescription,
            recipe.resultItemType
        );
        
        // Set additional properties based on item type
        if (recipe.resultItemType == ItemType.Equipment)
        {
            newItem.strengthBonus = recipe.strengthBonus;
            newItem.dexterityBonus = recipe.dexterityBonus;
            newItem.intelligenceBonus = recipe.intelligenceBonus;
        }
        else if (recipe.resultItemType == ItemType.Consumable)
        {
            newItem.healthRestoreAmount = recipe.healthRestoreAmount;
        }
        
        newItem.isModernTechnology = recipe.isModernTechnology;
        newItem.icon = recipe.resultItemIcon;
        
        return newItem;
    }
    
    private void ApplyModernTechnologyEffect(CraftingRecipe recipe, Item craftedItem)
    {
        // Apply special effects for modern technology items
        switch (recipe.resultItemId)
        {
            case "compass":
                // Unlock minimap or enhance navigation
                Debug.Log("Modern technology effect: Compass enhances navigation");
                break;
                
            case "simple_explosive":
                // Add special use ability for combat or puzzles
                Debug.Log("Modern technology effect: Explosive can be used for combat or puzzles");
                break;
                
            case "makeshift_telescope":
                // Enhance vision range or reveal distant locations
                Debug.Log("Modern technology effect: Telescope reveals distant locations");
                break;
                
            default:
                Debug.Log("Modern technology item created");
                break;
        }
    }
    
    public void SetCraftingSkillLevel(int level)
    {
        craftingSkillLevel = level;
        
        // Check for newly unlockable recipes
        foreach (CraftingRecipe recipe in availableRecipes)
        {
            if (!unlockedRecipes.Contains(recipe) && 
                recipe.requiredCraftingLevel <= craftingSkillLevel &&
                recipe.requiredModernKnowledgeLevel <= modernKnowledgeLevel)
            {
                UnlockRecipe(recipe);
            }
        }
    }
    
    public void SetModernKnowledgeLevel(int level)
    {
        modernKnowledgeLevel = level;
        
        // Check for newly unlockable recipes
        foreach (CraftingRecipe recipe in availableRecipes)
        {
            if (!unlockedRecipes.Contains(recipe) && 
                recipe.requiredCraftingLevel <= craftingSkillLevel &&
                recipe.requiredModernKnowledgeLevel <= modernKnowledgeLevel)
            {
                UnlockRecipe(recipe);
            }
        }
    }
}

[System.Serializable]
public class CraftingRecipe
{
    public string resultItemId;
    public string itemName;
    public string itemDescription;
    public ItemType resultItemType;
    public Sprite resultItemIcon;
    public List<CraftingIngredient> ingredients = new List<CraftingIngredient>();
    
    // Skill requirements
    public int requiredCraftingLevel = 1;
    public int requiredModernKnowledgeLevel = 1;
    
    // Modern technology flag
    public bool isModernTechnology = false;
    
    // Stats for equipment items
    public int strengthBonus = 0;
    public int dexterityBonus = 0;
    public int intelligenceBonus = 0;
    
    // Stats for consumable items
    public int healthRestoreAmount = 0;
}

[System.Serializable]
public class CraftingIngredient
{
    public string itemId;
    public string itemName; // For display purposes
    public int amount = 1;
    public Sprite icon;
} 