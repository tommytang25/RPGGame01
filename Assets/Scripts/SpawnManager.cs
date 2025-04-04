using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // Singleton instance
    public static SpawnManager Instance { get; private set; }
    
    [Header("Global Settings")]
    [SerializeField] private bool enableSpawning = true;
    [SerializeField] private int totalMaxMonsters = 100;
    [SerializeField] private float playerDetectionRadius = 30f;
    
    [Header("Boss Settings")]
    [SerializeField] private float bossCooldownTime = 300f; // 5 minutes
    [SerializeField] private float bossAnnouncementTime = 10f; // 10 seconds warning before boss spawn
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    // Private variables
    private List<SpawnArea> spawnAreas = new List<SpawnArea>();
    private Dictionary<SpawnArea, List<Enemy>> activeEnemiesByArea = new Dictionary<SpawnArea, List<Enemy>>();
    private Queue<Enemy> enemyRespawnQueue = new Queue<Enemy>();
    private float nextRespawnTime = 0f;
    private Dictionary<AreaType, float> bossTimers = new Dictionary<AreaType, float>();
    private Dictionary<AreaType, bool> bossAnnounced = new Dictionary<AreaType, bool>();
    
    private Transform playerTransform;
    private int activeMonsterCount = 0;
    
    public delegate void BossAnnouncementHandler(AreaType areaType, float timeUntilSpawn);
    public event BossAnnouncementHandler OnBossAnnounced;
    
    public delegate void BossSpawnedHandler(AreaType areaType, Enemy bossEnemy);
    public event BossSpawnedHandler OnBossSpawned;

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
        // Find player
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogWarning("SpawnManager: Player not found! Using Camera position for spawning logic.");
        }
        
        // Find all spawn areas in scene
        FindAllSpawnAreas();
        
        // Initialize boss timers for all area types
        foreach (AreaType areaType in System.Enum.GetValues(typeof(AreaType)))
        {
            bossTimers[areaType] = bossCooldownTime;
            bossAnnounced[areaType] = false;
        }
        
        // Start spawning
        StartCoroutine(SpawnRoutine());
        StartCoroutine(ProcessRespawnQueue());
    }
    
    private void Update()
    {
        if (!enableSpawning)
            return;
            
        UpdateBossTimers();
    }
    
    public void FindAllSpawnAreas()
    {
        SpawnArea[] areas = FindObjectsOfType<SpawnArea>();
        spawnAreas.Clear();
        
        foreach (SpawnArea area in areas)
        {
            RegisterSpawnArea(area);
        }
        
        Debug.Log($"SpawnManager: Found {spawnAreas.Count} spawn areas");
    }
    
    public void RegisterSpawnArea(SpawnArea area)
    {
        if (!spawnAreas.Contains(area))
        {
            spawnAreas.Add(area);
            activeEnemiesByArea[area] = new List<Enemy>();
        }
    }
    
    public void UnregisterSpawnArea(SpawnArea area)
    {
        if (spawnAreas.Contains(area))
        {
            spawnAreas.Remove(area);
            activeEnemiesByArea.Remove(area);
        }
    }
    
    public void QueueEnemyForRespawn(Enemy enemy, float respawnDelay)
    {
        if (enemy == null)
            return;
            
        StartCoroutine(DelayedRespawnEnqueue(enemy, respawnDelay));
    }
    
    private IEnumerator DelayedRespawnEnqueue(Enemy enemy, float respawnDelay)
    {
        yield return new WaitForSeconds(respawnDelay);
        enemyRespawnQueue.Enqueue(enemy);
    }
    
    private void UpdateBossTimers()
    {
        foreach (AreaType areaType in bossTimers.Keys)
        {
            // Decrement timer
            bossTimers[areaType] -= Time.deltaTime;
            
            // Announce boss when approaching spawn time
            if (!bossAnnounced[areaType] && bossTimers[areaType] <= bossAnnouncementTime)
            {
                AnnounceBoss(areaType, bossTimers[areaType]);
                bossAnnounced[areaType] = true;
            }
            
            // Spawn boss when timer reaches zero
            if (bossTimers[areaType] <= 0)
            {
                SpawnBoss(areaType);
                bossTimers[areaType] = bossCooldownTime;
                bossAnnounced[areaType] = false;
            }
        }
    }
    
    private void AnnounceBoss(AreaType areaType, float timeUntilSpawn)
    {
        OnBossAnnounced?.Invoke(areaType, timeUntilSpawn);
        
        // Show announcement UI or play sound
        UIManager.Instance?.ShowBossAnnouncement(areaType, timeUntilSpawn);
    }
    
    private void SpawnBoss(AreaType areaType)
    {
        // Find eligible spawn areas for this boss
        List<SpawnArea> eligibleAreas = spawnAreas.FindAll(area => 
            area.areaType == areaType && 
            area.isActive && 
            area.allowBosses &&
            IsAreaWithinPlayerRange(area)
        );
        
        if (eligibleAreas.Count == 0)
        {
            Debug.LogWarning($"SpawnManager: No eligible spawn areas found for boss in {areaType}");
            return;
        }
        
        // Pick random eligible area
        SpawnArea selectedArea = eligibleAreas[Random.Range(0, eligibleAreas.Count)];
        
        // Get boss monster from database
        MonsterType bossMonster = MonsterDatabase.Instance.GetRandomBossForArea(areaType);
        
        if (bossMonster == null || bossMonster.prefab == null)
        {
            Debug.LogError($"SpawnManager: No boss monster defined for area {areaType}");
            return;
        }
        
        // Spawn the boss
        Vector3 spawnPosition = GetValidSpawnPosition(selectedArea);
        Enemy bossEnemy = SpawnMonster(bossMonster, spawnPosition, selectedArea.areaLevel, MonsterRarity.Boss);
        
        if (bossEnemy != null)
        {
            // Trigger event
            OnBossSpawned?.Invoke(areaType, bossEnemy);
            
            // Show special effects or notifications
            GameObject spawnVFX = Instantiate(Resources.Load<GameObject>("Effects/BossSpawnVFX"), spawnPosition, Quaternion.identity);
            Destroy(spawnVFX, 3f);
            
            // Play boss spawn sound
            AudioManager.Instance?.PlaySound("BossSpawn");
        }
    }
    
    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (enableSpawning && activeMonsterCount < totalMaxMonsters)
            {
                // Process each active spawn area
                foreach (SpawnArea area in spawnAreas)
                {
                    if (!area.isActive)
                        continue;
                        
                    // Check if area is within range of player
                    if (!IsAreaWithinPlayerRange(area))
                        continue;
                        
                    // Check if area has reached its monster limit
                    if (GetAreaActiveMonsterCount(area) >= area.maxActiveMonsters)
                        continue;
                        
                    // Random chance to spawn based on area's spawn interval
                    if (Random.value > Time.deltaTime / area.spawnInterval)
                        continue;
                        
                    // Try to spawn a monster
                    TrySpawnMonsterInArea(area);
                }
            }
            
            yield return null;
        }
    }
    
    private IEnumerator ProcessRespawnQueue()
    {
        while (true)
        {
            if (enableSpawning && enemyRespawnQueue.Count > 0 && Time.time >= nextRespawnTime)
            {
                Enemy enemy = enemyRespawnQueue.Dequeue();
                
                // Find appropriate spawn area for this enemy
                SpawnArea spawnArea = FindAppropriateSpawnArea(enemy);
                
                if (spawnArea != null)
                {
                    // Reset enemy position and state
                    enemy.gameObject.SetActive(true);
                    enemy.transform.position = GetValidSpawnPosition(spawnArea);
                    
                    // Track the respawned enemy
                    TrackEnemy(enemy, spawnArea);
                    
                    // Add small delay between respawns
                    nextRespawnTime = Time.time + 0.1f;
                }
                else
                {
                    // No appropriate area found, re-queue for later
                    enemyRespawnQueue.Enqueue(enemy);
                    
                    // Add longer delay before trying again
                    nextRespawnTime = Time.time + 1f;
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private void TrySpawnMonsterInArea(SpawnArea area)
    {
        // Determine monster rarity based on area settings
        MonsterRarity rarity = DetermineMonsterRarity(area);
        
        // Get random monster of this rarity for this area
        MonsterType monsterType = MonsterDatabase.Instance.GetRandomMonsterForArea(area.areaType, rarity);
        
        if (monsterType == null || monsterType.prefab == null)
        {
            // Fallback to any monster for this area if specific rarity not found
            monsterType = MonsterDatabase.Instance.GetRandomMonsterForArea(area.areaType);
            
            if (monsterType == null || monsterType.prefab == null)
            {
                Debug.LogWarning($"SpawnManager: No monsters defined for area {area.areaType}");
                return;
            }
        }
        
        // Get valid spawn position
        Vector3 spawnPosition = GetValidSpawnPosition(area);
        
        // Spawn the monster
        SpawnMonster(monsterType, spawnPosition, area.areaLevel, rarity, area);
    }
    
    private Enemy SpawnMonster(MonsterType monsterType, Vector3 position, int areaLevel, MonsterRarity rarity, SpawnArea area = null)
    {
        if (monsterType == null || monsterType.prefab == null)
        {
            Debug.LogError("SpawnManager: Attempted to spawn null monster");
            return null;
        }
        
        // Instantiate monster prefab
        GameObject monsterObj = Instantiate(monsterType.prefab, position, Quaternion.identity);
        
        // Get Enemy component
        Enemy enemy = monsterObj.GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError($"SpawnManager: Monster prefab {monsterType.name} does not have Enemy component");
            Destroy(monsterObj);
            return null;
        }
        
        // Initialize enemy with monster type and level
        enemy.Initialize(monsterType, areaLevel, rarity);
        
        // Track the spawned monster
        if (area != null)
        {
            TrackEnemy(enemy, area);
        }
        
        return enemy;
    }
    
    private void TrackEnemy(Enemy enemy, SpawnArea area)
    {
        // Add to active enemies list
        if (activeEnemiesByArea.ContainsKey(area))
        {
            activeEnemiesByArea[area].Add(enemy);
            activeMonsterCount++;
            
            // Subscribe to enemy's death event
            enemy.OnEnemyDeath += HandleEnemyDeath;
        }
    }
    
    private void HandleEnemyDeath(Enemy enemy)
    {
        // Unsubscribe from event
        enemy.OnEnemyDeath -= HandleEnemyDeath;
        
        // Remove from active enemies list
        foreach (SpawnArea area in activeEnemiesByArea.Keys)
        {
            if (activeEnemiesByArea[area].Contains(enemy))
            {
                activeEnemiesByArea[area].Remove(enemy);
                activeMonsterCount--;
                break;
            }
        }
    }
    
    private SpawnArea FindAppropriateSpawnArea(Enemy enemy)
    {
        // First check if there's a spawn area matching the enemy's area type and level
        List<SpawnArea> matchingAreas = spawnAreas.FindAll(area => 
            area.isActive && 
            area.areaType == enemy.GetMonsterAreaType() &&
            IsAreaWithinPlayerRange(area) &&
            GetAreaActiveMonsterCount(area) < area.maxActiveMonsters
        );
        
        if (matchingAreas.Count > 0)
        {
            return matchingAreas[Random.Range(0, matchingAreas.Count)];
        }
        
        // If no exact match, find any active area
        List<SpawnArea> anyActiveAreas = spawnAreas.FindAll(area => 
            area.isActive && 
            IsAreaWithinPlayerRange(area) &&
            GetAreaActiveMonsterCount(area) < area.maxActiveMonsters
        );
        
        if (anyActiveAreas.Count > 0)
        {
            return anyActiveAreas[Random.Range(0, anyActiveAreas.Count)];
        }
        
        return null;
    }
    
    private Vector3 GetValidSpawnPosition(SpawnArea area)
    {
        Vector3 result = Vector3.zero;
        bool foundValidPosition = false;
        int attempts = 0;
        
        while (!foundValidPosition && attempts < area.maxSpawnRetries)
        {
            // Calculate random position within spawn area
            Vector2 randomOffset = new Vector2(
                Random.Range(-area.areaSize.x/2, area.areaSize.x/2),
                Random.Range(-area.areaSize.y/2, area.areaSize.y/2)
            );
            
            Vector3 candidatePosition = area.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            
            // Check if position is within minimum spawn distance from player
            if (IsPositionValidForSpawn(candidatePosition, area.minSpawnDistance))
            {
                result = candidatePosition;
                foundValidPosition = true;
            }
            
            attempts++;
        }
        
        if (!foundValidPosition)
        {
            Debug.LogWarning($"SpawnManager: Failed to find valid spawn position in area {area.name} after {attempts} attempts");
            // Fallback to area center if no valid position found
            result = area.transform.position;
        }
        
        return result;
    }
    
    private bool IsPositionValidForSpawn(Vector3 position, float minDistanceFromPlayer)
    {
        if (playerTransform == null)
            return true;
            
        return Vector3.Distance(position, playerTransform.position) >= minDistanceFromPlayer;
    }
    
    private bool IsAreaWithinPlayerRange(SpawnArea area)
    {
        if (playerTransform == null)
            return true;
            
        return Vector3.Distance(area.transform.position, playerTransform.position) <= playerDetectionRadius;
    }
    
    private int GetAreaActiveMonsterCount(SpawnArea area)
    {
        if (activeEnemiesByArea.ContainsKey(area))
        {
            return activeEnemiesByArea[area].Count;
        }
        return 0;
    }
    
    private MonsterRarity DetermineMonsterRarity(SpawnArea area)
    {
        float rarityRoll = Random.value * 100f;
        
        // Boss chance is handled separately by the boss timer system
        
        // Check for elite
        if (rarityRoll <= area.eliteChance)
        {
            return MonsterRarity.Elite;
        }
        
        // Check for rare
        if (rarityRoll <= area.eliteChance + area.rareChance)
        {
            return MonsterRarity.Rare;
        }
        
        // Check for uncommon (30% chance)
        if (rarityRoll <= area.eliteChance + area.rareChance + 30f)
        {
            return MonsterRarity.Uncommon;
        }
        
        // Default to common
        return MonsterRarity.Common;
    }
    
    public int GetTotalActiveMonsters()
    {
        return activeMonsterCount;
    }
    
    public float GetBossTimerForArea(AreaType areaType)
    {
        if (bossTimers.ContainsKey(areaType))
        {
            return bossTimers[areaType];
        }
        return bossCooldownTime;
    }
    
    public void ForceBossSpawn(AreaType areaType)
    {
        SpawnBoss(areaType);
        bossTimers[areaType] = bossCooldownTime;
        bossAnnounced[areaType] = false;
    }
    
    public void ClearAllMonsters()
    {
        foreach (var areaEnemies in activeEnemiesByArea.Values)
        {
            foreach (Enemy enemy in new List<Enemy>(areaEnemies))
            {
                if (enemy != null)
                {
                    enemy.gameObject.SetActive(false);
                    HandleEnemyDeath(enemy);
                }
            }
        }
        
        // Clear respawn queue
        enemyRespawnQueue.Clear();
        activeMonsterCount = 0;
    }
    
    // Debug information
    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 200, 150));
        GUILayout.Label($"Active Monsters: {activeMonsterCount}/{totalMaxMonsters}");
        GUILayout.Label($"Spawn Areas: {spawnAreas.Count}");
        
        GUILayout.EndArea();
    }
} 
} 