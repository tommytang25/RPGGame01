using UnityEngine;
using System.Collections;

public class WorldItem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer itemSpriteRenderer;
    [SerializeField] private SpriteRenderer rarityGlowRenderer;
    [SerializeField] private float bobAmount = 0.2f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float rotateSpeed = 30f;
    [SerializeField] private float attractSpeed = 5f;
    [SerializeField] private float pickupRadius = 1.5f;
    [SerializeField] private float magnetRadius = 3f;
    [SerializeField] private float lifetimeDuration = 60f; // How long before the item despawns
    [SerializeField] private AnimationCurve fadeCurve; // Curve for fading out when about to despawn
    
    private Item item;
    private Transform playerTransform;
    private Vector3 startPosition;
    private float bobTime;
    private bool isAttracting = false;
    private float lifetime = 0f;
    private bool isPickingUp = false;
    
    // Optional particle effect for rarity
    private ParticleSystem rarityParticles;
    
    private void Awake()
    {
        if (itemSpriteRenderer == null)
        {
            itemSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        rarityParticles = GetComponentInChildren<ParticleSystem>();
        
        // Create default fade curve if none assigned
        if (fadeCurve.keys.Length == 0)
        {
            fadeCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.8f, 1f),
                new Keyframe(1f, 0f)
            );
        }
    }
    
    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        startPosition = transform.position;
        bobTime = Random.Range(0f, 2f); // Randomize bob starting position
        
        // Add a small force to make items "pop" out when spawned
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            rb.AddForce(randomDirection * Random.Range(1f, 3f), ForceMode2D.Impulse);
        }
    }
    
    public void SetItem(Item newItem)
    {
        if (newItem == null) return;
        
        item = newItem.Clone(); // Create a deep copy of the item
        
        // Set sprite if available
        if (itemSpriteRenderer != null)
        {
            Sprite itemSprite = Resources.Load<Sprite>($"Items/{item.id}");
            if (itemSprite != null)
            {
                itemSpriteRenderer.sprite = itemSprite;
            }
            else
            {
                // Use default sprite based on item type
                switch (item.type)
                {
                    case ItemType.Equipment:
                        itemSpriteRenderer.sprite = Resources.Load<Sprite>("Items/default_equipment");
                        break;
                    case ItemType.Consumable:
                        itemSpriteRenderer.sprite = Resources.Load<Sprite>("Items/default_consumable");
                        break;
                    case ItemType.Resource:
                        itemSpriteRenderer.sprite = Resources.Load<Sprite>("Items/default_resource");
                        break;
                    default:
                        itemSpriteRenderer.sprite = Resources.Load<Sprite>("Items/default_item");
                        break;
                }
            }
        }
        
        // Set glow color based on rarity
        if (rarityGlowRenderer != null)
        {
            Color rarityColor = item.GetRarityColor();
            rarityColor.a = 0.5f; // Make it semi-transparent
            rarityGlowRenderer.color = rarityColor;
            
            // Scale the glow based on rarity
            float scale = 1.0f;
            switch (item.rarity)
            {
                case ItemRarity.Uncommon:
                    scale = 1.1f;
                    break;
                case ItemRarity.Rare:
                    scale = 1.2f;
                    break;
                case ItemRarity.Epic:
                    scale = 1.3f;
                    break;
                case ItemRarity.Legendary:
                    scale = 1.4f;
                    break;
                case ItemRarity.Mythic:
                    scale = 1.5f;
                    break;
            }
            rarityGlowRenderer.transform.localScale = new Vector3(scale, scale, 1f);
        }
        
        // Set up particles for higher rarity items
        if (rarityParticles != null && item.rarity >= ItemRarity.Rare)
        {
            var main = rarityParticles.main;
            main.startColor = item.GetRarityColor();
            
            // More particles for higher rarities
            var emission = rarityParticles.emission;
            switch (item.rarity)
            {
                case ItemRarity.Rare:
                    emission.rateOverTime = 5;
                    break;
                case ItemRarity.Epic:
                    emission.rateOverTime = 10;
                    break;
                case ItemRarity.Legendary:
                    emission.rateOverTime = 15;
                    break;
                case ItemRarity.Mythic:
                    emission.rateOverTime = 20;
                    break;
            }
            
            rarityParticles.Play();
        }
        else if (rarityParticles != null)
        {
            rarityParticles.Stop();
        }
    }
    
    private void Update()
    {
        if (isPickingUp) return;
        
        lifetime += Time.deltaTime;
        
        // Check for lifetime and apply fading
        if (lifetime > lifetimeDuration)
        {
            Destroy(gameObject);
            return;
        }
        
        // Calculate fade alpha when close to despawning
        if (lifetime > lifetimeDuration * 0.7f)
        {
            float fadeRatio = (lifetime - lifetimeDuration * 0.7f) / (lifetimeDuration * 0.3f);
            float alpha = fadeCurve.Evaluate(fadeRatio);
            
            // Apply alpha to all renderers
            if (itemSpriteRenderer != null)
            {
                Color color = itemSpriteRenderer.color;
                color.a = alpha;
                itemSpriteRenderer.color = color;
            }
            
            if (rarityGlowRenderer != null)
            {
                Color color = rarityGlowRenderer.color;
                color.a = alpha * 0.5f;  // Keep it semi-transparent
                rarityGlowRenderer.color = color;
            }
        }
        
        // Only process player interaction if player exists
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            // Check if player is in pickup radius
            if (distanceToPlayer <= pickupRadius)
            {
                PickupItem();
                return;
            }
            
            // Check if player is in magnet radius
            if (distanceToPlayer <= magnetRadius)
            {
                isAttracting = true;
            }
            
            // Move toward player if attracting
            if (isAttracting)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    playerTransform.position, 
                    attractSpeed * Time.deltaTime
                );
                return;
            }
        }
        
        // Apply bobbing motion
        bobTime += Time.deltaTime;
        float yOffset = Mathf.Sin(bobTime * bobSpeed) * bobAmount;
        transform.position = new Vector3(transform.position.x, startPosition.y + yOffset, transform.position.z);
        
        // Apply rotation
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
    
    private void PickupItem()
    {
        if (isPickingUp) return;
        
        isPickingUp = true;
        
        // Play pickup sound
        AudioManager.Instance?.PlaySound("ItemPickup");
        
        // Add to inventory
        if (InventorySystem.Instance != null && item != null)
        {
            InventorySystem.Instance.AddItem(item);
            
            // Show pickup text
            UIManager.Instance?.ShowItemPickup(item);
        }
        
        // Create pickup effect
        StartCoroutine(PickupEffect());
    }
    
    private IEnumerator PickupEffect()
    {
        // Disable collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // Disable rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }
        
        // Fly toward player with scale reduction
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if (playerTransform != null)
            {
                transform.position = Vector3.Lerp(startPos, playerTransform.position, t);
            }
            
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw pickup radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
        
        // Draw magnet radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
} 