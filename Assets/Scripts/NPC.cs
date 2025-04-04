using UnityEngine;
using System.Collections.Generic;

public class NPC : MonoBehaviour
{
    [Header("NPC Identity")]
    public string npcName;
    public NPCType npcType;
    public Sprite portrait;
    
    [Header("Dialogue")]
    public Dialogue dialogue;
    public float interactionRadius = 2f;
    
    [Header("Movement")]
    public bool canMove = true;
    public float moveSpeed = 1f;
    public Transform[] waypoints;
    private int currentWaypoint = 0;
    private Rigidbody2D rb;
    
    [Header("Vendor")]
    public bool isVendor = false;
    public List<Item> itemsForSale = new List<Item>();
    public float priceMultiplier = 1.0f; // For negotiation/reputation effects
    
    // Interaction state
    private bool playerInRange = false;
    private Transform player;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void Start()
    {
        if (dialogue == null)
        {
            // Create a default dialogue if none is assigned
            dialogue = new Dialogue();
            dialogue.id = "dialogue_" + npcName.ToLower().Replace(" ", "_");
            
            DialogueNode greeting = new DialogueNode
            {
                speakerName = npcName,
                speakerPortrait = portrait,
                text = "Hello there, traveler."
            };
            
            dialogue.nodes.Add(greeting);
        }
    }
    
    private void Update()
    {
        // Check for player interaction
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            InteractWithPlayer();
        }
        
        // Handle NPC movement if they can move and have waypoints
        if (canMove && waypoints != null && waypoints.Length > 0)
        {
            MoveToWaypoint();
        }
    }
    
    private void MoveToWaypoint()
    {
        // Skip if no valid waypoint
        if (currentWaypoint >= waypoints.Length || waypoints[currentWaypoint] == null)
            return;
            
        // Get direction to waypoint
        Vector2 direction = (waypoints[currentWaypoint].position - transform.position).normalized;
        
        // Move towards waypoint
        rb.velocity = direction * moveSpeed;
        
        // Rotate to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
        
        // Check if reached waypoint
        float distanceToWaypoint = Vector2.Distance(transform.position, waypoints[currentWaypoint].position);
        if (distanceToWaypoint < 0.1f)
        {
            // Move to next waypoint
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }
    
    private void InteractWithPlayer()
    {
        // Stop NPC movement during interaction
        StopMoving();
        
        // Face the player
        if (player != null)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }
        
        if (isVendor)
        {
            // Open shop UI
            OpenShop();
        }
        else
        {
            // Start dialogue
            if (DialogueSystem.Instance != null && dialogue != null)
            {
                DialogueSystem.Instance.StartDialogue(dialogue);
                
                // Subscribe to dialogue end event to resume movement
                DialogueSystem.Instance.OnDialogueEnd += OnDialogueEnd;
            }
        }
    }
    
    private void OnDialogueEnd()
    {
        // Unsubscribe from event
        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.OnDialogueEnd -= OnDialogueEnd;
        }
        
        // Resume movement if applicable
        if (canMove)
        {
            ResumeMoving();
        }
        
        // Check for quest updates or other effects from the dialogue
        CheckDialogueEffects();
    }
    
    private void OpenShop()
    {
        // TODO: Implement shop UI and interaction
        Debug.Log(npcName + " opens their shop. " + itemsForSale.Count + " items for sale.");
        
        // This would be replaced with actual UI showing/populating logic
    }
    
    private void StopMoving()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    private void ResumeMoving()
    {
        // Movement will resume on next Update
    }
    
    private void CheckDialogueEffects()
    {
        // TODO: Implement effects from dialogue like quest triggers, reputation changes, etc.
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.transform;
            
            // Show interaction prompt
            ShowInteractionPrompt();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            
            // Hide interaction prompt
            HideInteractionPrompt();
        }
    }
    
    private void ShowInteractionPrompt()
    {
        // TODO: Show a UI prompt to interact with the NPC
        Debug.Log("Press E to talk to " + npcName);
    }
    
    private void HideInteractionPrompt()
    {
        // TODO: Hide the UI prompt
    }
    
    // Add items to the shop inventory
    public void AddItemForSale(Item item)
    {
        if (!itemsForSale.Contains(item))
        {
            itemsForSale.Add(item);
        }
    }
}

public enum NPCType
{
    Villager,
    Merchant,
    Guard,
    Nobility,
    Scholar,
    Enemy
} 