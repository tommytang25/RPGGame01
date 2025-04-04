using UnityEngine;
using System.Collections;

public class GameInitializer : MonoBehaviour
{
    [Header("Manager Prefabs")]
    public GameObject gameManagerPrefab;
    public GameObject inventorySystemPrefab;
    public GameObject dialogueSystemPrefab;
    public GameObject craftingSystemPrefab;
    public GameObject timeAbilityControllerPrefab;
    
    [Header("Player Setup")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;
    
    [Header("Enemy Setup")]
    public GameObject[] enemyPrefabs;
    public Transform[] enemySpawnPoints;
    
    [Header("Environment")]
    public GameObject placeholderEnvironmentPrefab;
    
    private void Awake()
    {
        // Initialize managers first
        InitializeManagers();
    }
    
    private void Start()
    {
        // Set up the scene
        StartCoroutine(SetupScene());
    }
    
    private void InitializeManagers()
    {
        // Check if managers already exist from previous scene
        if (GameManager.Instance == null && gameManagerPrefab != null)
        {
            Instantiate(gameManagerPrefab);
        }
        
        if (InventorySystem.Instance == null && inventorySystemPrefab != null)
        {
            Instantiate(inventorySystemPrefab);
        }
        
        if (DialogueSystem.Instance == null && dialogueSystemPrefab != null)
        {
            Instantiate(dialogueSystemPrefab);
        }
        
        if (CraftingSystem.Instance == null && craftingSystemPrefab != null)
        {
            Instantiate(craftingSystemPrefab);
        }
        
        if (TimeAbilityController.Instance == null && timeAbilityControllerPrefab != null)
        {
            Instantiate(timeAbilityControllerPrefab);
        }
    }
    
    private IEnumerator SetupScene()
    {
        // Small delay to ensure all managers are initialized
        yield return new WaitForSeconds(0.1f);
        
        // Spawn placeholder environment if no real environment exists
        if (placeholderEnvironmentPrefab != null)
        {
            Instantiate(placeholderEnvironmentPrefab, Vector3.zero, Quaternion.identity);
        }
        
        // Spawn player
        SpawnPlayer();
        
        // Spawn enemies
        SpawnEnemies();
        
        // Setup initial quests
        SetupInitialQuests();
        
        // Setup initial crafting recipes
        SetupInitialCraftingRecipes();
        
        // Add test items to inventory
        AddTestItems();
        
        Debug.Log("Game initialized successfully!");
    }
    
    private void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            player.name = "Player";
            
            // Set up camera to follow player if needed
            // CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
            // if (cameraFollow != null)
            // {
            //     cameraFollow.target = player.transform;
            // }
        }
        else
        {
            Debug.LogError("Player prefab is not assigned!");
        }
    }
    
    private void SpawnEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("No enemy prefabs assigned!");
            return;
        }
        
        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            Debug.LogWarning("No enemy spawn points assigned!");
            return;
        }
        
        for (int i = 0; i < enemySpawnPoints.Length; i++)
        {
            if (enemySpawnPoints[i] != null)
            {
                // Choose a random enemy prefab
                GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                
                if (enemyPrefab != null)
                {
                    GameObject enemy = Instantiate(enemyPrefab, enemySpawnPoints[i].position, Quaternion.identity);
                    enemy.name = "Enemy_" + i;
                }
            }
        }
    }
    
    private void SetupInitialQuests()
    {
        // Skip if GameManager is not initialized
        if (GameManager.Instance == null)
            return;
            
        // Create and add initial quests
        Quest mainQuest = new Quest
        {
            title = "The Way Home",
            description = "Investigate the artifact that brought you to this world and find a way back home.",
            experienceReward = 500,
            type = QuestType.CollectItems,
            target = 1,
            progress = 0
        };
        
        Quest sideQuest = new Quest
        {
            title = "Village Troubles",
            description = "Help the village by defeating bandits in the nearby forest.",
            experienceReward = 100,
            type = QuestType.KillEnemies,
            target = 5,
            progress = 0
        };
        
        GameManager.Instance.AddQuest(mainQuest);
        GameManager.Instance.AddQuest(sideQuest);
    }
    
    private void SetupInitialCraftingRecipes()
    {
        // Skip if CraftingSystem is not initialized
        if (CraftingSystem.Instance == null)
            return;
            
        // Create and add some basic recipes
        
        // Example: Simple health potion recipe
        CraftingRecipe healthPotionRecipe = new CraftingRecipe
        {
            resultItemId = "health_potion",
            itemName = "Health Potion",
            itemDescription = "Restores 25 health points.",
            resultItemType = ItemType.Consumable,
            healthRestoreAmount = 25,
            requiredCraftingLevel = 1,
            requiredModernKnowledgeLevel = 1,
            isModernTechnology = false
        };
        
        // Add ingredients
        CraftingIngredient herbIngredient = new CraftingIngredient
        {
            itemId = "herb",
            itemName = "Healing Herb",
            amount = 2
        };
        
        CraftingIngredient waterIngredient = new CraftingIngredient
        {
            itemId = "water",
            itemName = "Clean Water",
            amount = 1
        };
        
        healthPotionRecipe.ingredients.Add(herbIngredient);
        healthPotionRecipe.ingredients.Add(waterIngredient);
        
        // Example: Compass (modern technology recipe)
        CraftingRecipe compassRecipe = new CraftingRecipe
        {
            resultItemId = "compass",
            itemName = "Makeshift Compass",
            itemDescription = "A compass made with your modern knowledge. Helps with navigation.",
            resultItemType = ItemType.QuestItem,
            requiredCraftingLevel = 1,
            requiredModernKnowledgeLevel = 2,
            isModernTechnology = true
        };
        
        // Add ingredients
        CraftingIngredient metalIngredient = new CraftingIngredient
        {
            itemId = "metal_scrap",
            itemName = "Metal Scrap",
            amount = 1
        };
        
        CraftingIngredient magnetiteIngredient = new CraftingIngredient
        {
            itemId = "magnetite",
            itemName = "Magnetite Ore",
            amount = 1
        };
        
        compassRecipe.ingredients.Add(metalIngredient);
        compassRecipe.ingredients.Add(magnetiteIngredient);
        
        // Add recipes to the crafting system
        CraftingSystem.Instance.availableRecipes.Add(healthPotionRecipe);
        CraftingSystem.Instance.availableRecipes.Add(compassRecipe);
    }
    
    private void AddTestItems()
    {
        // Skip if InventorySystem is not initialized
        if (InventorySystem.Instance == null)
            return;
            
        // Add some test items to player's inventory
        Item herb = new Item("herb", "Healing Herb", "A common herb with mild healing properties.", ItemType.CraftingMaterial);
        Item water = new Item("water", "Clean Water", "A flask of clean water.", ItemType.CraftingMaterial);
        Item sword = new Item("sword", "Iron Sword", "A basic iron sword.", ItemType.Equipment);
        sword.strengthBonus = 5;
        
        InventorySystem.Instance.AddItem(herb);
        InventorySystem.Instance.AddItem(herb); // Add two herbs
        InventorySystem.Instance.AddItem(water);
        InventorySystem.Instance.AddItem(sword);
    }
} 