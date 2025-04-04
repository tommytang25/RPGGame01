using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum for area types 
public enum AreaType
{
    Forest,
    Meadow,
    Swamp,
    Desert,
    Tundra,
    Mountain,
    Cave,
    Dungeon,
    Village,
    Castle,
    Cemetery
}

[ExecuteInEditMode]
public class SpawnArea : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] public AreaType areaType = AreaType.Forest;
    [SerializeField] public string areaName = "Spawn Area";
    [SerializeField] public bool isActive = true;
    
    [Header("Spawn Settings")]
    [SerializeField] public int maxActiveMonsters = 10;
    [SerializeField] public float spawnInterval = 5f; // Average seconds between spawn attempts
    [SerializeField] public float minSpawnDistance = 10f; // Minimum distance from player to spawn
    [SerializeField] public int maxSpawnRetries = 10; // Max attempts to find a valid spawn position
    
    [Header("Spawn Boundaries")]
    [SerializeField] public Vector2 areaSize = new Vector2(30f, 30f);
    [SerializeField] public bool useBoxCollider = true;
    [SerializeField] public bool visualizeArea = true;
    
    [Header("Difficulty Settings")]
    [SerializeField] public int areaLevel = 1; // Base level for monsters in this area
    [SerializeField] [Range(0f, 100f)] public float eliteChance = 5f; // % chance for elite monsters
    [SerializeField] [Range(0f, 100f)] public float rareChance = 15f; // % chance for rare monsters
    [SerializeField] public bool allowBosses = true; // Can boss monsters spawn in this area
    
    // Runtime variables
    private List<Enemy> activeMonsters = new List<Enemy>();
    private BoxCollider2D boxCollider;
    private SpawnManager spawnManager;
    private float nextSpawnTime;
    private int spawnedCount = 0;
    
    private void Awake()
    {
        // Register with spawn manager
        if (Application.isPlaying)
        {
            if (SpawnManager.Instance != null)
            {
                SpawnManager.Instance.RegisterSpawnArea(this);
            }
            else
            {
                Debug.LogWarning($"SpawnArea {name}: SpawnManager instance not found");
            }
        }
        
        // Setup collider if needed
        if (useBoxCollider)
        {
            boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            
            boxCollider.size = areaSize;
            boxCollider.isTrigger = true;
        }
    }
    
    private void Start()
    {
        spawnManager = SpawnManager.Instance;
        
        if (spawnManager == null)
        {
            Debug.LogError("SpawnArea: SpawnManager instance not found!");
            isActive = false;
            return;
        }
        
        // Set initial spawn time
        nextSpawnTime = Time.time + Random.Range(0f, spawnInterval);
    }
    
    private void OnDestroy()
    {
        // Unregister from spawn manager when destroyed
        if (Application.isPlaying)
        {
            if (SpawnManager.Instance != null)
            {
                SpawnManager.Instance.UnregisterSpawnArea(this);
            }
        }
    }
    
    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            // Re-register if enabled
            if (SpawnManager.Instance != null)
            {
                SpawnManager.Instance.RegisterSpawnArea(this);
            }
        }
    }
    
    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            // Unregister if disabled
            if (SpawnManager.Instance != null)
            {
                SpawnManager.Instance.UnregisterSpawnArea(this);
            }
        }
    }
    
    private void OnValidate()
    {
        // Update collider when properties change in editor
        if (useBoxCollider)
        {
            boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                boxCollider.size = areaSize;
                boxCollider.isTrigger = true;
            }
        }
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        // Check if it's time to spawn and we have room for more monsters
        if (Time.time >= nextSpawnTime && activeMonsters.Count < maxActiveMonsters)
        {
            SpawnMonster();
            nextSpawnTime = Time.time + spawnInterval;
        }
        
        // Clean up destroyed monsters
        CleanupMonsterList();
    }
    
    private void SpawnMonster()
    {
        if (spawnManager == null || MonsterDatabase.Instance == null) return;
        
        // Get a random monster type for this area
        MonsterType monsterType = DetermineMonsterToSpawn();
        
        if (monsterType == null)
        {
            Debug.LogWarning($"SpawnArea '{areaName}': No valid monster type found for area {areaType}");
            return;
        }
        
        // Find a valid spawn position
        Vector2 spawnPosition = GetRandomSpawnPosition();
        
        if (spawnPosition == Vector2.zero)
        {
            // Couldn't find a valid spawn position
            return;
        }
        
        // Request monster spawn from the spawn manager
        Enemy spawnedEnemy = spawnManager.SpawnMonster(monsterType, spawnPosition, areaLevel);
        
        if (spawnedEnemy != null)
        {
            // Add to active monsters list
            activeMonsters.Add(spawnedEnemy);
            spawnedCount++;
            
            // Set this as the respawn area for the monster
            spawnedEnemy.respawnArea = this;
            
            Debug.Log($"SpawnArea '{areaName}': Spawned {monsterType.name} at {spawnPosition}");
        }
    }
    
    private MonsterType DetermineMonsterToSpawn()
    {
        MonsterDatabase database = MonsterDatabase.Instance;
        
        // Determine rarity based on chances
        float rarityRoll = Random.value;
        MonsterRarity targetRarity;
        
        if (allowBosses && rarityRoll < 0.01f) // 1% chance for boss if allowed
        {
            targetRarity = MonsterRarity.Boss;
        }
        else if (rarityRoll < eliteChance)
        {
            targetRarity = MonsterRarity.Elite;
        }
        else if (rarityRoll < (eliteChance + rareChance))
        {
            targetRarity = MonsterRarity.Rare;
        }
        else if (rarityRoll < (eliteChance + rareChance + 0.25f))
        {
            targetRarity = MonsterRarity.Uncommon;
        }
        else
        {
            targetRarity = MonsterRarity.Common;
        }
        
        // Try to get a monster of the target rarity
        List<MonsterType> candidates = database.GetMonstersByAreaAndRarity(areaType, targetRarity);
        
        // If no monsters of target rarity, fall back to any monster for this area
        if (candidates.Count == 0)
        {
            candidates = database.GetMonstersByArea(areaType);
            
            // If still no monsters, return null
            if (candidates.Count == 0)
            {
                return null;
            }
        }
        
        // Select a random monster from candidates, weighted by spawn weight
        float totalWeight = 0f;
        foreach (MonsterType monster in candidates)
        {
            totalWeight += monster.spawnWeight;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (MonsterType monster in candidates)
        {
            currentWeight += monster.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return monster;
            }
        }
        
        // Fallback
        return candidates[0];
    }
    
    private Vector2 GetRandomSpawnPosition()
    {
        Transform playerTransform = FindObjectOfType<PlayerController>()?.transform;
        Vector2 playerPosition = playerTransform != null ? (Vector2)playerTransform.position : Vector2.zero;
        
        for (int i = 0; i < maxSpawnRetries; i++)
        {
            // Generate random position inside area
            Vector2 randomOffset = new Vector2(
                Random.Range(-areaSize.x/2, areaSize.x/2),
                Random.Range(-areaSize.y/2, areaSize.y/2)
            );
            
            Vector2 potentialPosition = (Vector2)transform.position + randomOffset;
            
            // Check distance from player
            if (playerTransform != null && Vector2.Distance(potentialPosition, playerPosition) < minSpawnDistance)
            {
                continue; // Too close to player, try again
            }
            
            // Check if position is valid (not colliding with obstacles, etc.)
            if (IsValidSpawnPosition(potentialPosition))
            {
                return potentialPosition;
            }
        }
        
        // Couldn't find valid position after max retries
        return Vector2.zero;
    }
    
    private bool IsValidSpawnPosition(Vector2 position)
    {
        // Check for obstacles or other reasons why we can't spawn here
        
        // Basic check: cast a small circle to see if there's an obstacle
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.5f);
        
        foreach (Collider2D collider in colliders)
        {
            // Skip triggers
            if (collider.isTrigger)
                continue;
                
            // Check if it's an obstacle layer
            if (collider.gameObject.layer == LayerMask.NameToLayer("Obstacle") ||
                collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                return false;
            }
            
            // Check if it's another enemy
            if (collider.GetComponent<Enemy>() != null)
            {
                return false;
            }
        }
        
        return true;
    }
    
    private void CleanupMonsterList()
    {
        for (int i = activeMonsters.Count - 1; i >= 0; i--)
        {
            if (activeMonsters[i] == null || !activeMonsters[i].gameObject.activeInHierarchy)
            {
                activeMonsters.RemoveAt(i);
            }
        }
    }
    
    // Called when a monster from this area is killed or despawned
    public void OnMonsterRemoved(Enemy monster)
    {
        activeMonsters.Remove(monster);
    }
    
    // Called when a monster from this area is respawned
    public void OnMonsterRespawned(Enemy monster)
    {
        if (!activeMonsters.Contains(monster))
        {
            activeMonsters.Add(monster);
        }
    }
    
    // Visualize the spawn area in the editor
    private void OnDrawGizmos()
    {
        if (!visualizeArea)
            return;
            
        // Draw area bounds in editor
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(
            transform.position,
            transform.rotation,
            Vector3.one
        );
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawCube(Vector3.zero, new Vector3(areaSize.x, areaSize.y, 0.1f));
        
        // Draw area outline
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(areaSize.x, areaSize.y, 0.1f));
        
        // Draw minimum spawn distance from player
        Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.5f);
        Gizmos.DrawWireSphere(Vector3.zero, minSpawnDistance);
        
        // Show area name
        Gizmos.color = Color.white;
        Vector3 textPosition = transform.position + Vector3.up * (areaSize.y / 2 + 1f);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(textPosition, $"{areaName} (Lv. {areaLevel})");
#endif
    }
    
    // Get total count of monsters that have been spawned in this area
    public int GetSpawnedCount()
    {
        return spawnedCount;
    }
    
    // Get current active monster count
    public int GetActiveMonsterCount()
    {
        CleanupMonsterList();
        return activeMonsters.Count;
    }
    
    // Force spawn a specific monster type at a specific position
    public Enemy ForceSpawnMonster(string monsterId, Vector2 position)
    {
        if (spawnManager == null || MonsterDatabase.Instance == null) return null;
        
        MonsterType monsterType = MonsterDatabase.Instance.GetMonsterById(monsterId);
        if (monsterType == null) return null;
        
        Enemy spawnedEnemy = spawnManager.SpawnMonster(monsterType, position, areaLevel);
        
        if (spawnedEnemy != null)
        {
            activeMonsters.Add(spawnedEnemy);
            spawnedCount++;
            spawnedEnemy.respawnArea = this;
        }
        
        return spawnedEnemy;
    }
    
    // Check if position is inside spawn area
    public bool IsPositionInArea(Vector2 position)
    {
        Vector2 localPosition = transform.InverseTransformPoint(position);
        
        return Mathf.Abs(localPosition.x) <= areaSize.x / 2 &&
               Mathf.Abs(localPosition.y) <= areaSize.y / 2;
    }
    
    public bool IsPointInArea(Vector3 point)
    {
        // Convert to local space
        Vector3 localPoint = transform.InverseTransformPoint(point);
        
        // Check if within bounds
        return localPoint.x >= -areaSize.x / 2 && localPoint.x <= areaSize.x / 2 &&
               localPoint.y >= -areaSize.y / 2 && localPoint.y <= areaSize.y / 2;
    }
    
    public Vector3 GetRandomPointInArea()
    {
        // Calculate random position within spawn area
        Vector2 randomOffset = new Vector2(
            Random.Range(-areaSize.x/2, areaSize.x/2),
            Random.Range(-areaSize.y/2, areaSize.y/2)
        );
        
        return transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
    }
    
    public string GetAreaDisplayName()
    {
        return $"{areaName} <size=80%>(Lv. {areaLevel})</size>";
    }
} 