using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Enemy : MonoBehaviour
{
    [Header("Monster Info")]
    [SerializeField] private int monsterId;
    [SerializeField] private string monsterName;
    [SerializeField] private MonsterRarity monsterRarity;
    [SerializeField] private int monsterLevel = 1;
    
    [Header("Stats")]
    public int maxHealth = 50;
    private int currentHealth;
    public int damage = 10;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public int experienceValue = 10;
    
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectionRange = 8f;
    public float stoppingDistance = 1.2f;
    private Transform player;
    private Rigidbody2D rb;
    private bool canAttack = true;
    
    [Header("Respawn Settings")]
    public bool canRespawn = true;
    public float respawnTime = 60f; // Default 1 minute
    [HideInInspector] public SpawnArea respawnArea;
    private Vector3 originalPosition;
    private bool isDead = false;
    private float deathTimer = 0f;
    private bool isRespawnQueued = false;
    
    // cached monster type
    private MonsterType monsterType;
    
    // Events
    public delegate void EnemyDeathHandler(Enemy enemy);
    public event EnemyDeathHandler OnEnemyDeath;
    
    // Properties
    public int MonsterId => monsterId;
    public string MonsterName => monsterName;
    public MonsterRarity MonsterRarity => monsterRarity;
    public int MonsterLevel => monsterLevel;
    public bool IsRespawnable => canRespawn;
    public bool IsDead => isDead;
    
    // Get monster area type from cached monster type
    public AreaType GetMonsterAreaType()
    {
        if (monsterType != null && monsterType.spawnAreas.Count > 0)
        {
            return monsterType.spawnAreas[0];
        }
        return AreaType.Forest; // Default area type
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
    }
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player == null)
        {
            Debug.LogWarning("Enemy: Player not found");
        }
        
        // Initialize from database if ID is set but monsterType is null
        if (monsterId != 0 && monsterType == null && MonsterDatabase.Instance != null)
        {
            MonsterType type = MonsterDatabase.Instance.GetMonsterById(monsterId);
            if (type != null)
            {
                Initialize(type, monsterLevel);
            }
        }
        
        // Make sure health is set
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }
    }
    
    // Initialize with monster type from database
    public void Initialize(MonsterType type, int level = 1, MonsterRarity overrideRarity = MonsterRarity.Common)
    {
        if (type == null)
        {
            Debug.LogError("Enemy: Tried to initialize with null monster type");
            return;
        }
        
        monsterType = type;
        monsterId = type.id;
        monsterName = type.name;
        
        // Use override rarity if specified, otherwise use the monster's default rarity
        monsterRarity = (overrideRarity != MonsterRarity.Common) ? overrideRarity : type.rarity;
        monsterLevel = level;
        
        // Set stats based on monster type, level and rarity
        maxHealth = type.GetHealthWithRarity(level, monsterRarity);
        currentHealth = maxHealth;
        damage = type.GetDamageWithRarity(level, monsterRarity);
        experienceValue = type.GetExperienceWithRarity(level, monsterRarity);
        
        // Set movement parameters
        moveSpeed = type.baseSpeed;
        detectionRange = 8f; // Default value
        attackRange = 1.5f; // Default value
        attackCooldown = 1.5f; // Default value
        
        // Set respawn settings
        canRespawn = true; // Default to respawnable
        respawnTime = 60f; // Default 1 minute
        
        // Set appearance if applicable
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && type.icon != null)
        {
            spriteRenderer.sprite = type.icon;
        }
        
        // Add visual indicator for special monsters
        if (monsterRarity != MonsterRarity.Common && monsterRarity != MonsterRarity.Uncommon)
        {
            AddRarityVisualIndicator();
        }
        
        isDead = false;
        isRespawnQueued = false;
        
        // Enable all components
        SetEnemyActive(true);
        
        // Debug log
        Debug.Log($"Initialized {monsterName} (Level {monsterLevel}, {monsterRarity}) with {maxHealth} HP");
    }
    
    private void AddRarityVisualIndicator()
    {
        // Remove any existing indicator
        Transform existingIndicator = transform.Find("RarityIndicator");
        if (existingIndicator != null)
        {
            Destroy(existingIndicator.gameObject);
        }
        
        // Add a visual indicator for special monsters (glow, aura, etc.)
        GameObject indicator = new GameObject("RarityIndicator");
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = Vector3.zero;
        
        SpriteRenderer indicatorRenderer = indicator.AddComponent<SpriteRenderer>();
        indicatorRenderer.sprite = Resources.Load<Sprite>("Indicators/glow"); // You would need to create this sprite
        
        // Set color based on rarity
        switch (monsterRarity)
        {
            case MonsterRarity.Uncommon:
                indicatorRenderer.color = new Color(0, 0.8f, 0, 0.3f); // Green
                indicatorRenderer.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                break;
            case MonsterRarity.Rare:
                indicatorRenderer.color = new Color(0, 0.3f, 0.8f, 0.3f); // Blue
                indicatorRenderer.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                break;
            case MonsterRarity.Elite:
                indicatorRenderer.color = new Color(0.8f, 0, 0.8f, 0.4f); // Purple
                indicatorRenderer.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
                break;
            case MonsterRarity.Boss:
                indicatorRenderer.color = new Color(0.8f, 0.1f, 0.1f, 0.5f); // Red
                indicatorRenderer.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
                break;
        }
        
        // Make sure it renders behind the monster
        indicatorRenderer.sortingOrder = -1;
    }
    
    private void Update()
    {
        if (isDead)
        {
            // Handle respawning if not queued to the spawn manager
            if (canRespawn && !isRespawnQueued)
            {
                deathTimer += Time.deltaTime;
                if (deathTimer >= respawnTime)
                {
                    Respawn();
                }
            }
            return;
        }
        
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Check if player is within detection range
        if (distanceToPlayer <= detectionRange)
        {
            // Move towards player if not within stopping distance
            if (distanceToPlayer > stoppingDistance)
            {
                ChasePlayer();
            }
            else
            {
                // Stop moving
                rb.velocity = Vector2.zero;
                
                // Attack if in range and off cooldown
                if (canAttack && distanceToPlayer <= attackRange)
                {
                    StartCoroutine(Attack());
                }
            }
        }
        else
        {
            // Player out of range, stop moving
            rb.velocity = Vector2.zero;
        }
    }
    
    private void ChasePlayer()
    {
        if (player == null) return;
        
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
        
        // Flip sprite based on direction if no rotation is used
        if (Mathf.Abs(rb.rotation) < 0.01f)
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.flipX = direction.x < 0;
            }
        }
        else
        {
            // Rotate towards player
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }
    }
    
    private IEnumerator Attack()
    {
        canAttack = false;
        
        // Check if player is still in range
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                // Play attack animation if available
                Animator animator = GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger("Attack");
                }
                
                // Apply damage to player
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(damage);
                }
            }
        }
        
        yield return new WaitForSeconds(attackCooldown);
        
        canAttack = true;
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // Play hurt animation if available
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
        
        // Visual feedback
        StartCoroutine(FlashRed());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator FlashRed()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sprite.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }
    
    private void Die()
    {
        isDead = true;
        
        // Play death animation if available
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // Grant experience to player
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddExperience(experienceValue);
        }
        
        // Generate loot
        GenerateLoot();
        
        // Trigger death event
        OnEnemyDeath?.Invoke(this);
        
        // Disable monster components (but leave GameObject active for death animation)
        SetEnemyActive(false);
        
        // Queue for respawn if applicable
        if (canRespawn && SpawnManager.Instance != null)
        {
            isRespawnQueued = true;
            SpawnManager.Instance.QueueEnemyForRespawn(this, respawnTime);
        }
        else if (!canRespawn)
        {
            // If not respawning, destroy after animation
            Destroy(gameObject, 1.5f); // Adjust based on death animation length
        }
    }
    
    private void GenerateLoot()
    {
        // Use LootSystem to generate loot based on monster rarity and level
        if (LootSystem.Instance != null)
        {
            LootSystem.Instance.GenerateLootAtPosition(transform.position, monsterRarity, monsterLevel);
        }
    }
    
    private void SetEnemyActive(bool active)
    {
        // Disable components but keep GameObject active for death animation
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = active;
        }
        
        // Disable rigidbody physics
        if (rb != null)
        {
            rb.simulated = active;
        }
        
        // Disable this script's functionality (but not the component itself)
        enabled = active;
    }
    
    public void Respawn()
    {
        if (isDead && canRespawn)
        {
            // Reset health and state
            currentHealth = maxHealth;
            isDead = false;
            deathTimer = 0f;
            isRespawnQueued = false;
            
            // Re-enable components
            SetEnemyActive(true);
            
            // Re-enable GameObject if disabled
            gameObject.SetActive(true);
            
            Debug.Log($"Respawned {monsterName} (Level {monsterLevel})");
        }
    }
    
    public void QueueForRespawn()
    {
        if (!isDead && canRespawn)
        {
            isDead = true;
            isRespawnQueued = true;
            
            // Disable without death animation
            SetEnemyActive(false);
            
            // Queue for respawn
            if (SpawnManager.Instance != null)
            {
                SpawnManager.Instance.QueueEnemyForRespawn(this, respawnTime);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    // For debug purposes
    public string GetDebugInfo()
    {
        string info = $"{monsterName} (Level {monsterLevel}) [{monsterRarity}]\n";
        info += $"HP: {currentHealth}/{maxHealth}\n";
        info += $"DMG: {damage}\n";
        info += $"EXP: {experienceValue}\n";
        return info;
    }
} 