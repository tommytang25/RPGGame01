using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ResourceType
{
    Plant,
    Ore,
    Wood,
    Fish,
    Crystal,
    Special
}

[System.Serializable]
public class PossibleDrop
{
    public int itemId;
    public string itemName;
    public float dropChance; // 0.0 to 1.0
    public ItemRarity rarity = ItemRarity.Common;
}

public class ResourceNode : MonoBehaviour
{
    [Header("Resource Settings")]
    public string nodeName = "Resource Node";
    public ResourceType resourceType = ResourceType.Plant;
    public List<PossibleDrop> possibleDrops = new List<PossibleDrop>();
    public int minDrops = 1;
    public int maxDrops = 3;
    
    [Header("Respawn Settings")]
    public bool canRespawn = true;
    public float respawnTime = 300f; // 5 minutes by default
    public bool isHarvested = false;
    private float respawnTimer = 0f;
    
    [Header("Interaction Settings")]
    public float harvestRadius = 2f;
    public string requiredTool = "none"; // none, pickaxe, axe, fishingrod
    public int requiredToolTier = 1; // 1-5, higher tier nodes need better tools
    
    [Header("Visual Settings")]
    public GameObject activeVisual; // When resource is available
    public GameObject harvestedVisual; // When resource has been harvested
    public GameObject interactionPrompt; // UI prompt for interaction
    
    private bool playerInRange = false;
    private Transform player;
    
    private void Start()
    {
        // Setup initial state
        ShowAppropriateVisual();
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Handle respawning
        if (isHarvested && canRespawn)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime)
            {
                Respawn();
            }
        }
        
        // Show interaction prompt when player is in range and node is available
        if (playerInRange && !isHarvested)
        {
            if (interactionPrompt != null && !interactionPrompt.activeSelf)
            {
                interactionPrompt.SetActive(true);
                
                // Update prompt text based on required tool
                TMPro.TextMeshProUGUI promptText = interactionPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (promptText != null)
                {
                    string toolText = (requiredTool == "none") ? "hand" : requiredTool;
                    promptText.text = $"Press [E] to harvest {nodeName} (Requires: {toolText} tier {requiredToolTier})";
                }
            }
        }
        else if (interactionPrompt != null && interactionPrompt.activeSelf)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    public void TryHarvest(PlayerController player)
    {
        if (isHarvested)
        {
            Debug.Log($"{nodeName} has already been harvested. Come back later!");
            return;
        }
        
        // Check if player has the required tool
        if (player.HasRequiredTool(requiredTool, requiredToolTier))
        {
            HarvestResource(player);
        }
        else
        {
            // Show message about required tool
            Debug.Log($"You need a {requiredTool} (tier {requiredToolTier}) to harvest this {nodeName}!");
        }
    }
    
    private void HarvestResource(PlayerController player)
    {
        // Generate drops
        List<Item> harvestedItems = GenerateDrops();
        
        // Add items to inventory
        if (InventorySystem.Instance != null)
        {
            foreach (Item item in harvestedItems)
            {
                InventorySystem.Instance.AddItem(item);
            }
        }
        
        // Calculate experience for farming
        int experienceGained = CalculateExperience(player.toolTier);
        
        // Grant experience
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddExperience(experienceGained);
            GameManager.Instance.RecordResourceHarvested(resourceType.ToString());
        }
        
        // Visual and state updates
        isHarvested = true;
        respawnTimer = 0f;
        ShowAppropriateVisual();
        
        // Hide interaction prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Sound effect and particles can be added here
        Debug.Log($"Harvested {nodeName}! Gained {experienceGained} XP.");
    }
    
    private List<Item> GenerateDrops()
    {
        List<Item> drops = new List<Item>();
        
        // Determine how many items to drop
        int numDrops = Random.Range(minDrops, maxDrops + 1);
        
        // Get luck from player if possible
        int playerLuck = 0;
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            playerLuck = player.GetLuckStat();
        }
        
        // Get any active event bonuses
        float eventBonus = 0f;
        if (GameManager.Instance != null && GameManager.Instance.specialEvent)
        {
            eventBonus = GameManager.Instance.eventDropRateBonus;
        }
        
        // Roll for each possible drop
        for (int i = 0; i < numDrops; i++)
        {
            foreach (PossibleDrop possibleDrop in possibleDrops)
            {
                // Apply luck and event bonuses
                float modifiedChance = possibleDrop.dropChance * (1 + (playerLuck * 0.02f)) * (1 + eventBonus);
                
                if (Random.value <= modifiedChance)
                {
                    // Create item
                    Item newItem = new Item
                    {
                        id = possibleDrop.itemId,
                        name = possibleDrop.itemName,
                        rarity = possibleDrop.rarity,
                        type = ItemType.Resource
                    };
                    
                    // Set resource-specific properties
                    newItem.resourceType = resourceType;
                    
                    // Add to drops
                    drops.Add(newItem);
                    
                    // Show message
                    Debug.Log($"Found {newItem.GetColoredName()}!");
                }
            }
        }
        
        // Ensure at least one item drops if we were supposed to get something
        if (drops.Count == 0 && possibleDrops.Count > 0 && numDrops > 0)
        {
            // Select a random item from possible drops
            PossibleDrop guaranteedDrop = possibleDrops[Random.Range(0, possibleDrops.Count)];
            
            Item newItem = new Item
            {
                id = guaranteedDrop.itemId,
                name = guaranteedDrop.itemName,
                rarity = guaranteedDrop.rarity,
                type = ItemType.Resource,
                resourceType = resourceType
            };
            
            drops.Add(newItem);
            Debug.Log($"Found {newItem.GetColoredName()}!");
        }
        
        return drops;
    }
    
    private int CalculateExperience(int playerToolTier)
    {
        // Base experience depends on resource type
        int baseExp = 0;
        switch (resourceType)
        {
            case ResourceType.Plant:
                baseExp = 5;
                break;
            case ResourceType.Wood:
                baseExp = 8;
                break;
            case ResourceType.Ore:
                baseExp = 10;
                break;
            case ResourceType.Fish:
                baseExp = 12;
                break;
            case ResourceType.Crystal:
                baseExp = 15;
                break;
            case ResourceType.Special:
                baseExp = 20;
                break;
        }
        
        // Adjust based on node tier and player's tool tier
        float tierMultiplier = requiredToolTier * 0.5f; // Higher tier nodes give more exp
        
        // Bonus for using higher tier tools than required
        float toolBonusMultiplier = 1.0f;
        if (playerToolTier > requiredToolTier)
        {
            toolBonusMultiplier += (playerToolTier - requiredToolTier) * 0.1f; // 10% per tier above requirement
        }
        
        return Mathf.RoundToInt(baseExp * tierMultiplier * toolBonusMultiplier);
    }
    
    private void Respawn()
    {
        isHarvested = false;
        respawnTimer = 0f;
        ShowAppropriateVisual();
        
        Debug.Log($"{nodeName} has respawned!");
    }
    
    // Call this from GameManager to force respawn (e.g., day change)
    public void ForceRespawn()
    {
        if (isHarvested)
        {
            Respawn();
        }
    }
    
    private void ShowAppropriateVisual()
    {
        if (activeVisual != null)
        {
            activeVisual.SetActive(!isHarvested);
        }
        
        if (harvestedVisual != null)
        {
            harvestedVisual.SetActive(isHarvested);
        }
    }
    
    // When player enters range
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.transform;
        }
    }
    
    // When player leaves range
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            
            // Hide interaction prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw harvest radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, harvestRadius);
        
        // Draw required tool tier as text
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, 
            $"{nodeName} (Tier {requiredToolTier} {requiredTool})");
    }
} 