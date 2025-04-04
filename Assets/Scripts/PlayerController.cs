using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float sprintStaminaCost = 10f;
    private float currentMoveSpeed;
    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isSprinting = false;
    
    [Header("Combat")]
    public int damage = 25;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;
    public Transform attackPoint;
    public LayerMask enemyLayer;
    private bool canAttack = true;
    
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int maxStamina = 100;
    public float currentStamina;
    public float staminaRegenRate = 5f;
    public int strength = 5;
    public int dexterity = 5;
    public int intelligence = 5;
    public int luck = 5;
    
    [Header("Tools")]
    public bool hasPickaxe = false;
    public bool hasAxe = false;
    public bool hasFishingRod = false;
    public int toolTier = 1; // 1-5, higher tier = better resource gathering
    
    [Header("Grinding Mechanics")]
    public int killCombo = 0;
    public float comboTimer = 0f;
    public float maxComboTime = 10f;
    public float comboExpMultiplier = 0.1f; // 10% bonus per combo level
    private bool isHarvesting = false;
    
    [Header("Equipment Bonuses")]
    private int equipStrengthBonus = 0;
    private int equipDexterityBonus = 0;
    private int equipIntelligenceBonus = 0;
    private int equipDefenseBonus = 0;
    private float equipDamageModifier = 0f;
    private float equipAttackSpeedModifier = 0f;
    private float equipCriticalChanceBonus = 0f;
    private float equipHealthRegenBonus = 0f;
    private float equipManaRegenBonus = 0f;
    
    // Special set bonus effects
    private bool hasFireResistance = false;
    private bool hasLifeSteal = false;
    private float lifeStealPercent = 0f;
    
    // Cached components
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentMoveSpeed = moveSpeed;
    }
    
    private void Update()
    {
        // Get input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        // Normalize diagonal movement
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }
        
        // Sprint handling
        HandleSprinting();
        
        // Attack
        if (Input.GetMouseButtonDown(0) && canAttack && !isHarvesting)
        {
            StartCoroutine(AttackCoroutine());
        }
        
        // Harvest resources (right mouse button)
        if (Input.GetMouseButtonDown(1) && canAttack)
        {
            StartCoroutine(HarvestCoroutine());
        }
        
        // Use tool (E key)
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseTool();
        }
        
        // Stamina regeneration
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }
        }
        
        // Health regeneration from equipment bonuses
        if (currentHealth < maxHealth && equipHealthRegenBonus > 0)
        {
            currentHealth += equipHealthRegenBonus * Time.deltaTime;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }
        
        // Handle combo timer
        if (killCombo > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }
    
    private void FixedUpdate()
    {
        // Move the player
        rb.velocity = movement * currentMoveSpeed;
        
        // Update animator if it exists
        if (animator != null)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);
        }
        
        // Flip sprite based on movement direction
        if (movement.x != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = movement.x < 0;
        }
    }
    
    private void HandleSprinting()
    {
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
        {
            isSprinting = true;
            currentMoveSpeed = moveSpeed * sprintMultiplier;
            currentStamina -= sprintStaminaCost * Time.deltaTime;
        }
        else
        {
            isSprinting = false;
            currentMoveSpeed = moveSpeed;
        }
        
        if (currentStamina <= 0)
        {
            isSprinting = false;
            currentMoveSpeed = moveSpeed;
        }
    }
    
    private IEnumerator AttackCoroutine()
    {
        canAttack = false;
        
        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Apply damage
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        int totalDamageDealt = 0;
        
        foreach (Collider2D enemy in hitEnemies)
        {
            int damageToEnemy = CalculateDamage();
            enemy.GetComponent<Enemy>().TakeDamage(damageToEnemy);
            totalDamageDealt += damageToEnemy;
        }
        
        // Process life steal if applicable
        if (totalDamageDealt > 0)
        {
            ProcessLifeSteal(totalDamageDealt);
        }
        
        yield return new WaitForSeconds(attackCooldown);
        
        canAttack = true;
    }
    
    private IEnumerator HarvestCoroutine()
    {
        isHarvesting = true;
        canAttack = false;
        
        // Play harvest animation
        if (animator != null)
        {
            animator.SetTrigger("Harvest");
        }
        
        // Check for resource nodes
        Collider2D[] hitNodes = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, LayerMask.GetMask("ResourceNode"));
        foreach (Collider2D nodeCollider in hitNodes)
        {
            ResourceNode node = nodeCollider.GetComponent<ResourceNode>();
            if (node != null)
            {
                node.TryHarvest(this);
            }
        }
        
        yield return new WaitForSeconds(attackCooldown);
        
        isHarvesting = false;
        canAttack = true;
    }
    
    private void UseTool()
    {
        // This can be expanded based on the context (mining, fishing, etc.)
        Debug.Log("Using tool: " + (hasPickaxe ? "Pickaxe" : hasAxe ? "Axe" : hasFishingRod ? "Fishing Rod" : "None"));
        
        // Play tool animation
        if (animator != null)
        {
            animator.SetTrigger("UseTool");
        }
    }
    
    private int CalculateDamage()
    {
        // Base damage + strength bonus + random variance
        int finalDamage = damage + (strength / 2);
        
        // Apply equipment bonuses (if equipment system exists)
        if (EquipmentSystem.Instance != null)
        {
            finalDamage += EquipmentSystem.Instance.GetTotalStrengthBonus() / 2;
            
            // Apply damage modifier bonus from sets
            finalDamage = Mathf.RoundToInt(finalDamage * (1f + equipDamageModifier));
        }
        
        // Critical hit chance based on dexterity + equipment bonus
        float critChance = (dexterity + equipDexterityBonus) * 0.5f + equipCriticalChanceBonus * 100f;
        if (Random.Range(0, 100) < critChance)
        {
            finalDamage = (int)(finalDamage * 1.5f);
            Debug.Log("Critical Hit! Damage: " + finalDamage);
        }
        
        return finalDamage;
    }
    
    public void TakeDamage(int damage)
    {
        // Apply defense from equipment
        int actualDamage = damage;
        int totalDefense = equipDefenseBonus;
        
        if (EquipmentSystem.Instance != null)
        {
            totalDefense += EquipmentSystem.Instance.GetTotalDefenseBonus();
        }
        
        actualDamage = Mathf.Max(1, damage - totalDefense);
        
        currentHealth -= actualDamage;
        
        // Play hurt animation
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
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }
    
    private void Die()
    {
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }
        
        // Disable player input and physics
        enabled = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        
        // Disable collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }
    
    public void IncreaseCombo()
    {
        killCombo++;
        comboTimer = maxComboTime;
        
        // Notify UI
        Debug.Log("Combo: " + killCombo + "x");
    }
    
    public void ResetCombo()
    {
        killCombo = 0;
        comboTimer = 0;
    }
    
    public float GetComboMultiplier()
    {
        return 1 + (killCombo * comboExpMultiplier);
    }
    
    public void AddToolToInventory(string toolType, int tier)
    {
        switch (toolType.ToLower())
        {
            case "pickaxe":
                hasPickaxe = true;
                break;
            case "axe":
                hasAxe = true;
                break;
            case "fishingrod":
                hasFishingRod = true;
                break;
        }
        
        // Update tool tier if the new tool is better
        if (tier > toolTier)
        {
            toolTier = tier;
            Debug.Log("Tool upgraded to tier " + toolTier);
        }
    }
    
    public bool HasRequiredTool(string requiredTool, int requiredTier)
    {
        bool hasTool = false;
        
        switch (requiredTool.ToLower())
        {
            case "pickaxe":
                hasTool = hasPickaxe;
                break;
            case "axe":
                hasTool = hasAxe;
                break;
            case "fishingrod":
                hasTool = hasFishingRod;
                break;
            case "none":
                hasTool = true;
                break;
        }
        
        return hasTool && toolTier >= requiredTier;
    }
    
    // Method for the luck stat used in loot calculations
    public int GetLuckStat()
    {
        return luck;
    }
    
    // Apply equipment bonuses calculated by the EquipmentSystem
    public void ApplyEquipmentBonuses(
        int strengthBonus, 
        int dexterityBonus, 
        int intelligenceBonus, 
        int defenseBonus,
        float damageModifier,
        float attackSpeedModifier,
        float criticalChanceBonus,
        float healthRegenBonus,
        float manaRegenBonus)
    {
        this.equipStrengthBonus = strengthBonus;
        this.equipDexterityBonus = dexterityBonus;
        this.equipIntelligenceBonus = intelligenceBonus;
        this.equipDefenseBonus = defenseBonus;
        this.equipDamageModifier = damageModifier;
        this.equipAttackSpeedModifier = attackSpeedModifier;
        this.equipCriticalChanceBonus = criticalChanceBonus;
        this.equipHealthRegenBonus = healthRegenBonus;
        this.equipManaRegenBonus = manaRegenBonus;
        
        // Apply attack speed modifier to cooldown
        float baseAttackCooldown = 0.5f; // Store the base cooldown somewhere
        attackCooldown = baseAttackCooldown / (1f + equipAttackSpeedModifier);
        
        Debug.Log($"Applied equipment bonuses: STR+{strengthBonus}, DEX+{dexterityBonus}, INT+{intelligenceBonus}, DEF+{defenseBonus}");
        if (damageModifier > 0 || attackSpeedModifier > 0 || criticalChanceBonus > 0)
        {
            Debug.Log($"Combat bonuses: DMG+{damageModifier*100}%, SPD+{attackSpeedModifier*100}%, CRIT+{criticalChanceBonus*100}%");
        }
    }
    
    // Handle special set bonus effects
    public void ApplySpecialSetEffect(EquipmentSet set, int piecesRequired)
    {
        switch (set)
        {
            case EquipmentSet.Warrior:
                if (piecesRequired >= 6)
                {
                    // Special Attack - Enable warrior's fury
                    Debug.Log("Warrior's Fury special effect applied!");
                }
                break;
                
            case EquipmentSet.Ranger:
                if (piecesRequired >= 6)
                {
                    // Special Shot - Enable ranger's special attack
                    Debug.Log("Ranger's Special Shot effect applied!");
                }
                break;
                
            case EquipmentSet.Mage:
                if (piecesRequired >= 6)
                {
                    // Spell Echo - Chance to cast spells twice
                    Debug.Log("Mage's Spell Echo effect applied!");
                }
                break;
                
            case EquipmentSet.Guardian:
                if (piecesRequired >= 6)
                {
                    // Damage Reflection
                    Debug.Log("Guardian's Damage Reflection effect applied!");
                }
                break;
                
            case EquipmentSet.Shadow:
                if (piecesRequired >= 6)
                {
                    // Backstab - Extra damage from behind
                    Debug.Log("Shadow's Backstab effect applied!");
                }
                break;
                
            case EquipmentSet.Dragon:
                // Fire Resistance at any tier
                hasFireResistance = true;
                Debug.Log("Dragon's Fire Resistance effect applied!");
                
                if (piecesRequired >= 6)
                {
                    // Flame Breath attack
                    Debug.Log("Dragon's Flame Breath attack unlocked!");
                }
                break;
                
            case EquipmentSet.Nature:
                if (piecesRequired >= 6)
                {
                    // Nature's Wrath
                    Debug.Log("Nature's Wrath effect applied!");
                }
                break;
                
            case EquipmentSet.Abyssal:
                // Life Steal at all tiers
                hasLifeSteal = true;
                // Tier 1: 5%, Tier 2: 10%, Tier 3: 15%
                lifeStealPercent = 0.05f * (piecesRequired / 2);
                Debug.Log($"Abyssal Life Steal effect applied: {lifeStealPercent * 100}%");
                
                if (piecesRequired >= 6)
                {
                    // Void Nova attack
                    Debug.Log("Abyssal Void Nova attack unlocked!");
                }
                break;
        }
    }
    
    // Process life steal from attacks
    private void ProcessLifeSteal(int damageDealt)
    {
        if (hasLifeSteal && lifeStealPercent > 0)
        {
            int healthRestored = Mathf.RoundToInt(damageDealt * lifeStealPercent);
            if (healthRestored > 0)
            {
                currentHealth += healthRestored;
                if (currentHealth > maxHealth)
                {
                    currentHealth = maxHealth;
                }
                Debug.Log($"Life Steal: +{healthRestored} HP");
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
} 