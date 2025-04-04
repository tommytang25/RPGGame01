using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterRarity
{
    Common,
    Uncommon,
    Rare,
    Elite,
    Boss
}

public enum AreaType
{
    Forest,
    Cave,
    Mountain,
    Swamp,
    Desert,
    Ruins,
    Village,
    Beach,
    Volcano,
    Dungeon
}

[System.Serializable]
public class MonsterType
{
    public int id;
    public string name;
    public string description;
    public MonsterRarity rarity;
    public List<AreaType> spawnAreas;
    
    [Header("Stats")]
    public int baseHealth;
    public int baseDamage;
    public float baseSpeed;
    public int baseExperience;
    public float aggroRange;
    public float attackRange;
    
    [Header("Scaling")]
    public float healthScaling = 1.2f;
    public float damageScaling = 1.15f;
    public float experienceScaling = 1.1f;
    
    [Header("Visuals")]
    public GameObject prefab;
    public Sprite icon;
    public RuntimeAnimatorController animatorController;
    
    [Header("Behavior")]
    public bool isAggressive;
    public bool isRanged;
    public float patrolRadius = 5f;
    public bool canFly;
    public bool canSwim;
    
    [Header("Spawning")]
    public float spawnWeight = 1f;
    public float respawnTime = 60f;
    public bool isUnique = false;
    public int maxGroupSize = 1; // For monsters that spawn in groups
    
    [Header("Drops")]
    public int goldMin;
    public int goldMax;
    public List<LootDrop> guaranteedDrops = new List<LootDrop>();
    public List<LootDrop> possibleDrops = new List<LootDrop>();

    // Calculate stats for the given level
    public int GetHealthForLevel(int level)
    {
        return Mathf.RoundToInt(baseHealth * Mathf.Pow(healthScaling, level - 1));
    }
    
    public int GetDamageForLevel(int level)
    {
        return Mathf.RoundToInt(baseDamage * Mathf.Pow(damageScaling, level - 1));
    }
    
    public int GetExperienceForLevel(int level)
    {
        return Mathf.RoundToInt(baseExperience * Mathf.Pow(experienceScaling, level - 1));
    }
    
    // Constructor for creating a new monster type
    public MonsterType(string id, string name, string description)
    {
        this.id = int.Parse(id);
        this.name = name;
        this.description = description;
        this.rarity = MonsterRarity.Common;
        this.spawnAreas = new List<AreaType>();
        this.guaranteedDrops = new List<LootDrop>();
        this.possibleDrops = new List<LootDrop>();
    }
    
    // Clone method
    public MonsterType Clone()
    {
        MonsterType clone = new MonsterType(id.ToString(), name, description);
        
        clone.rarity = rarity;
        clone.spawnAreas = new List<AreaType>(spawnAreas);
        
        clone.baseHealth = baseHealth;
        clone.baseDamage = baseDamage;
        clone.baseSpeed = baseSpeed;
        clone.baseExperience = baseExperience;
        clone.aggroRange = aggroRange;
        clone.attackRange = attackRange;
        
        clone.healthScaling = healthScaling;
        clone.damageScaling = damageScaling;
        clone.experienceScaling = experienceScaling;
        
        clone.prefab = prefab;
        clone.icon = icon;
        clone.animatorController = animatorController;
        
        clone.isAggressive = isAggressive;
        clone.isRanged = isRanged;
        clone.patrolRadius = patrolRadius;
        clone.canFly = canFly;
        clone.canSwim = canSwim;
        
        clone.spawnWeight = spawnWeight;
        clone.respawnTime = respawnTime;
        clone.isUnique = isUnique;
        clone.maxGroupSize = maxGroupSize;
        
        clone.goldMin = goldMin;
        clone.goldMax = goldMax;
        clone.guaranteedDrops = new List<LootDrop>(guaranteedDrops);
        clone.possibleDrops = new List<LootDrop>(possibleDrops);
        
        return clone;
    }
}

[System.Serializable]
public class LootDrop
{
    public string itemId;
    public float dropChance; // 0-1 probability
    public int minQuantity = 1;
    public int maxQuantity = 1;
    
    public LootDrop(string itemId, float dropChance)
    {
        this.itemId = itemId;
        this.dropChance = dropChance;
    }
}

public class MonsterDatabase : MonoBehaviour
{
    // Singleton instance
    public static MonsterDatabase Instance { get; private set; }
    
    // List of all monster types
    [SerializeField] private List<MonsterType> allMonsters = new List<MonsterType>();
    
    // Dictionary for quick lookups by ID
    private Dictionary<int, MonsterType> monsterDictionary = new Dictionary<int, MonsterType>();
    
    // Dictionary for area-specific monster lists
    private Dictionary<AreaType, List<MonsterType>> areaMonsters = new Dictionary<AreaType, List<MonsterType>>();
    
    // Dictionary for rarity-specific monster lists
    private Dictionary<MonsterRarity, List<MonsterType>> rarityMonsters = new Dictionary<MonsterRarity, List<MonsterType>>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeDatabase()
    {
        // Clear existing dictionaries
        monsterDictionary.Clear();
        areaMonsters.Clear();
        rarityMonsters.Clear();
        
        // Initialize dictionaries for each area and rarity
        foreach (AreaType area in System.Enum.GetValues(typeof(AreaType)))
        {
            areaMonsters[area] = new List<MonsterType>();
        }
        
        foreach (MonsterRarity rarity in System.Enum.GetValues(typeof(MonsterRarity)))
        {
            rarityMonsters[rarity] = new List<MonsterType>();
        }
        
        // Populate dictionaries
        foreach (MonsterType monster in allMonsters)
        {
            // Add to ID dictionary
            monsterDictionary[monster.id] = monster;
            
            // Add to rarity dictionary
            rarityMonsters[monster.rarity].Add(monster);
            
            // Add to each relevant area dictionary
            foreach (AreaType area in monster.spawnAreas)
            {
                areaMonsters[area].Add(monster);
            }
        }
        
        Debug.Log($"Monster Database initialized with {allMonsters.Count} monster types");
    }
    
    // Get a monster type by ID
    public MonsterType GetMonsterById(int id)
    {
        if (monsterDictionary.TryGetValue(id, out MonsterType monster))
        {
            return monster.Clone();
        }
        
        Debug.LogWarning($"Monster with ID '{id}' not found in database");
        return null;
    }
    
    // Get all monsters that can spawn in a specific area
    public List<MonsterType> GetMonstersByArea(AreaType area)
    {
        if (areaMonsters.TryGetValue(area, out List<MonsterType> monsters))
        {
            return new List<MonsterType>(monsters);
        }
        
        return new List<MonsterType>();
    }
    
    // Get monsters of a specific rarity
    public List<MonsterType> GetMonstersByRarity(MonsterRarity rarity)
    {
        if (rarityMonsters.TryGetValue(rarity, out List<MonsterType> monsters))
        {
            return new List<MonsterType>(monsters);
        }
        
        return new List<MonsterType>();
    }
    
    // Get monsters that can spawn in a specific area with a specific rarity
    public List<MonsterType> GetMonstersByAreaAndRarity(AreaType area, MonsterRarity rarity)
    {
        List<MonsterType> result = new List<MonsterType>();
        
        if (areaMonsters.TryGetValue(area, out List<MonsterType> areaList))
        {
            foreach (MonsterType monster in areaList)
            {
                if (monster.rarity == rarity)
                {
                    result.Add(monster);
                }
            }
        }
        
        return result;
    }
    
    // Get a random monster for an area, weighted by spawn weight
    public MonsterType GetRandomMonsterForArea(AreaType area)
    {
        if (!areaMonsters.TryGetValue(area, out List<MonsterType> monsters) || monsters.Count == 0)
        {
            Debug.LogWarning($"No monsters defined for area {area}");
            return null;
        }
        
        // Calculate total weight
        float totalWeight = 0f;
        foreach (MonsterType monster in monsters)
        {
            totalWeight += monster.spawnWeight;
        }
        
        // Get a random weighted monster
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (MonsterType monster in monsters)
        {
            currentWeight += monster.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return monster.Clone();
            }
        }
        
        // Fallback
        return monsters[0].Clone();
    }
    
    // Add a new monster type at runtime
    public void AddMonsterType(MonsterType monster)
    {
        // Add to main list
        allMonsters.Add(monster);
        
        // Add to ID dictionary
        monsterDictionary[monster.id] = monster;
        
        // Add to rarity dictionary
        rarityMonsters[monster.rarity].Add(monster);
        
        // Add to each relevant area dictionary
        foreach (AreaType area in monster.spawnAreas)
        {
            areaMonsters[area].Add(monster);
        }
        
        Debug.Log($"Added new monster type: {monster.name}");
    }
    
    // Get all monsters
    public List<MonsterType> GetAllMonsters()
    {
        return new List<MonsterType>(allMonsters);
    }
    
    // Create and populate default monsters (placeholder examples)
    public void CreateDefaultMonsters()
    {
        // This would be called if the database is empty and needs default data
        
        // Example: Forest monsters
        CreateForestMonsters();
        
        // Example: Cave monsters
        CreateCaveMonsters();
        
        // Refresh the database with the new monsters
        InitializeDatabase();
    }
    
    private void CreateForestMonsters()
    {
        // Wolf
        MonsterType wolf = new MonsterType("forest_wolf", "Forest Wolf", "A common predator found in forest areas.");
        wolf.rarity = MonsterRarity.Common;
        wolf.spawnAreas.Add(AreaType.Forest);
        wolf.baseHealth = 50;
        wolf.baseDamage = 12;
        wolf.baseSpeed = 3.5f;
        wolf.baseExperience = 15;
        wolf.aggroRange = 6f;
        wolf.attackRange = 1.5f;
        wolf.isAggressive = true;
        wolf.spawnWeight = 10f;
        wolf.goldMin = 1;
        wolf.goldMax = 5;
        wolf.possibleDrops.Add(new LootDrop("wolf_pelt", 0.5f));
        wolf.possibleDrops.Add(new LootDrop("wolf_tooth", 0.3f));
        allMonsters.Add(wolf);
        
        // Deer
        MonsterType deer = new MonsterType("forest_deer", "Forest Deer", "A peaceful herbivore that flees when threatened.");
        deer.rarity = MonsterRarity.Common;
        deer.spawnAreas.Add(AreaType.Forest);
        deer.baseHealth = 30;
        deer.baseDamage = 0;
        deer.baseSpeed = 4f;
        deer.baseExperience = 10;
        deer.aggroRange = 8f;
        deer.attackRange = 0f;
        deer.isAggressive = false;
        deer.spawnWeight = 15f;
        deer.goldMin = 0;
        deer.goldMax = 3;
        deer.possibleDrops.Add(new LootDrop("deer_hide", 0.8f));
        deer.possibleDrops.Add(new LootDrop("venison", 0.6f));
        allMonsters.Add(deer);
        
        // Forest Troll
        MonsterType troll = new MonsterType("forest_troll", "Forest Troll", "A dangerous troll that guards its territory fiercely.");
        troll.rarity = MonsterRarity.Uncommon;
        troll.spawnAreas.Add(AreaType.Forest);
        troll.baseHealth = 150;
        troll.baseDamage = 25;
        troll.baseSpeed = 2f;
        troll.baseExperience = 40;
        troll.aggroRange = 5f;
        troll.attackRange = 2f;
        troll.isAggressive = true;
        troll.spawnWeight = 5f;
        troll.goldMin = 5;
        troll.goldMax = 15;
        troll.possibleDrops.Add(new LootDrop("troll_hide", 0.4f));
        troll.possibleDrops.Add(new LootDrop("troll_tooth", 0.3f));
        allMonsters.Add(troll);
        
        // Forest Spirit (Rare)
        MonsterType spirit = new MonsterType("forest_spirit", "Ancient Forest Spirit", "A magical guardian of the forest. Rarely seen.");
        spirit.rarity = MonsterRarity.Rare;
        spirit.spawnAreas.Add(AreaType.Forest);
        spirit.baseHealth = 300;
        spirit.baseDamage = 40;
        spirit.baseSpeed = 3f;
        spirit.baseExperience = 100;
        spirit.aggroRange = 7f;
        spirit.attackRange = 3f;
        spirit.isAggressive = false;
        spirit.spawnWeight = 1f;
        spirit.goldMin = 20;
        spirit.goldMax = 50;
        spirit.possibleDrops.Add(new LootDrop("spirit_essence", 0.6f));
        spirit.possibleDrops.Add(new LootDrop("magic_wood", 0.4f));
        allMonsters.Add(spirit);
    }
    
    private void CreateCaveMonsters()
    {
        // Bat
        MonsterType bat = new MonsterType("cave_bat", "Cave Bat", "Small flying creatures that hunt in the darkness.");
        bat.rarity = MonsterRarity.Common;
        bat.spawnAreas.Add(AreaType.Cave);
        bat.baseHealth = 20;
        bat.baseDamage = 8;
        bat.baseSpeed = 4f;
        bat.baseExperience = 8;
        bat.aggroRange = 5f;
        bat.attackRange = 1f;
        bat.isAggressive = true;
        bat.canFly = true;
        bat.spawnWeight = 20f;
        bat.maxGroupSize = 5;
        bat.goldMin = 0;
        bat.goldMax = 2;
        bat.possibleDrops.Add(new LootDrop("bat_wing", 0.4f));
        allMonsters.Add(bat);
        
        // Cave Spider
        MonsterType spider = new MonsterType("cave_spider", "Cave Spider", "Venomous spiders that lurk in the shadows.");
        spider.rarity = MonsterRarity.Common;
        spider.spawnAreas.Add(AreaType.Cave);
        spider.baseHealth = 35;
        spider.baseDamage = 15;
        spider.baseSpeed = 3f;
        spider.baseExperience = 12;
        spider.aggroRange = 4f;
        spider.attackRange = 1.5f;
        spider.isAggressive = true;
        spider.spawnWeight = 15f;
        spider.goldMin = 1;
        spider.goldMax = 4;
        spider.possibleDrops.Add(new LootDrop("spider_silk", 0.5f));
        spider.possibleDrops.Add(new LootDrop("venom_sac", 0.3f));
        allMonsters.Add(spider);
        
        // Cave Troll
        MonsterType caveTroll = new MonsterType("cave_troll", "Cave Troll", "A lumbering troll adapted to cave life.");
        caveTroll.rarity = MonsterRarity.Uncommon;
        caveTroll.spawnAreas.Add(AreaType.Cave);
        caveTroll.baseHealth = 180;
        caveTroll.baseDamage = 30;
        caveTroll.baseSpeed = 1.8f;
        caveTroll.baseExperience = 45;
        caveTroll.aggroRange = 4f;
        caveTroll.attackRange = 2.2f;
        caveTroll.isAggressive = true;
        caveTroll.spawnWeight = 5f;
        caveTroll.goldMin = 5;
        caveTroll.goldMax = 18;
        caveTroll.guaranteedDrops.Add(new LootDrop("troll_club", 0.1f));
        caveTroll.possibleDrops.Add(new LootDrop("cave_crystal", 0.2f));
        allMonsters.Add(caveTroll);
        
        // Ancient Golem (Elite)
        MonsterType golem = new MonsterType("cave_golem", "Ancient Stone Golem", "A powerful construct that has guarded the caves for centuries.");
        golem.rarity = MonsterRarity.Elite;
        golem.spawnAreas.Add(AreaType.Cave);
        golem.baseHealth = 500;
        golem.baseDamage = 50;
        golem.baseSpeed = 1.5f;
        golem.baseExperience = 150;
        golem.aggroRange = 6f;
        golem.attackRange = 2.5f;
        golem.isAggressive = true;
        golem.spawnWeight = 1f;
        golem.respawnTime = 300f; // 5 minutes
        golem.goldMin = 50;
        golem.goldMax = 100;
        golem.guaranteedDrops.Add(new LootDrop("ancient_core", 0.5f));
        golem.possibleDrops.Add(new LootDrop("golem_heart", 0.2f));
        allMonsters.Add(golem);
    }
} 