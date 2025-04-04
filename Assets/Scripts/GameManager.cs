using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    [Header("Player Progression")]
    public int playerLevel = 1;
    public int playerExperience = 0;
    public int[] experienceRequirements;
    public int availableSkillPoints = 0;
    
    [Header("Game State")]
    public bool isPaused = false;
    public bool isPlayerDead = false;
    public float gameTime = 0f;
    public float dayLength = 300f; // 5 minutes per in-game day
    public int currentDay = 1;
    
    [Header("Grinding Mechanics")]
    public float globalExpMultiplier = 1.0f;
    public float globalDropRateMultiplier = 1.0f;
    public float timeSinceLastDeath = 0f;
    public int enemiesKilledThisSession = 0;
    public int resourcesHarvestedThisSession = 0;
    
    [Header("Events")]
    public bool specialEvent = false;
    public string currentEventName = "";
    public float eventTimeRemaining = 0f;
    public float eventExpBonus = 0f;
    public float eventDropRateBonus = 0f;
    
    [Header("UI References")]
    public GameObject pauseMenuUI;
    public GameObject gameOverUI;
    public GameObject levelUpUI;
    
    // Quest system
    public List<string> activeQuests = new List<string>();
    public List<string> completedQuests = new List<string>();
    
    // Time travel elements
    public bool hasDiscoveredArtifact = false;
    public bool canUseTimeSlowAbility = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeExperienceTable();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Initialize UI elements
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (levelUpUI != null) levelUpUI.SetActive(false);
        
        // Add a method to show a tip about accessing the item encyclopedia
        ShowItemEncyclopediaTip();
    }
    
    private void Update()
    {
        // Track game time
        gameTime += Time.deltaTime;
        
        // Track time since player's last death
        if (!isPlayerDead)
        {
            timeSinceLastDeath += Time.deltaTime;
        }
        
        // Day/Night cycle
        float dayProgress = gameTime % dayLength;
        float dayPercentage = dayProgress / dayLength;
        
        // Update current day
        int newDay = Mathf.FloorToInt(gameTime / dayLength) + 1;
        if (newDay != currentDay)
        {
            DayChanged(newDay);
        }
        
        // Handle special events
        if (specialEvent && eventTimeRemaining > 0)
        {
            eventTimeRemaining -= Time.deltaTime;
            if (eventTimeRemaining <= 0)
            {
                EndSpecialEvent();
            }
        }
        
        // Check for random events (1% chance per minute)
        if (!specialEvent && Random.value < (Time.deltaTime / 60f) * 0.01f)
        {
            StartRandomEvent();
        }
    }
    
    private void InitializeExperienceTable()
    {
        // Initialize the experience table with increasing requirements
        experienceRequirements = new int[100]; // Support up to level 100
        experienceRequirements[0] = 0; // Level 1 starts at 0 exp
        
        for (int i = 1; i < experienceRequirements.Length; i++)
        {
            // Exponential growth formula: baseExp * level^1.5
            experienceRequirements[i] = Mathf.RoundToInt(100 * Mathf.Pow(i, 1.5f));
        }
    }
    
    public void AddExperience(int amount)
    {
        // Apply combo multiplier if player has a combo
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            float comboMultiplier = player.GetComboMultiplier();
            amount = Mathf.RoundToInt(amount * comboMultiplier);
        }
        
        // Apply global and event multipliers
        amount = Mathf.RoundToInt(amount * globalExpMultiplier * (1 + eventExpBonus));
        
        playerExperience += amount;
        Debug.Log($"Gained {amount} XP. Total: {playerExperience}");
        
        // Check for level up
        CheckLevelUp();
    }
    
    private void CheckLevelUp()
    {
        int nextLevelExp = experienceRequirements[playerLevel];
        
        while (playerExperience >= nextLevelExp && playerLevel < experienceRequirements.Length - 1)
        {
            LevelUp();
            nextLevelExp = experienceRequirements[playerLevel];
        }
    }
    
    private void LevelUp()
    {
        playerLevel++;
        availableSkillPoints++;
        
        Debug.Log($"LEVEL UP! Now level {playerLevel}");
        
        // Notify UI about level up
        // UIManager.Instance.ShowLevelUp(playerLevel);
        
        // Play level up sound
        // AudioManager.Instance.PlaySound("level_up");
    }
    
    public void PlayerDied()
    {
        isPlayerDead = true;
        
        // Reset combo
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ResetCombo();
        }
        
        // Statistics
        timeSinceLastDeath = 0f;
        
        // Show game over screen after a delay
        StartCoroutine(GameOverSequence());
    }
    
    private IEnumerator GameOverSequence()
    {
        // Wait for death animation
        yield return new WaitForSeconds(2f);
        
        // Show game over UI
        // UIManager.Instance.ShowGameOverScreen();
        
        Debug.Log("Game Over!");
    }
    
    public void RestartGame()
    {
        isPlayerDead = false;
        // Reload current scene or respawn player
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }
    
    public void RecordEnemyKill(string enemyType)
    {
        enemiesKilledThisSession++;
        
        // Increase player's kill combo
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.IncreaseCombo();
        }
        
        // Check for streak achievements
        CheckKillAchievements();
    }
    
    public void RecordResourceHarvested(string resourceType)
    {
        resourcesHarvestedThisSession++;
        
        // Check for harvesting achievements
        CheckHarvestAchievements();
    }
    
    private void CheckKillAchievements()
    {
        // This could be expanded based on game requirements
        if (enemiesKilledThisSession == 10)
        {
            Debug.Log("Achievement: Novice Hunter");
        }
        else if (enemiesKilledThisSession == 100)
        {
            Debug.Log("Achievement: Experienced Hunter");
        }
        else if (enemiesKilledThisSession == 1000)
        {
            Debug.Log("Achievement: Master Hunter");
        }
    }
    
    private void CheckHarvestAchievements()
    {
        // This could be expanded based on game requirements
        if (resourcesHarvestedThisSession == 10)
        {
            Debug.Log("Achievement: Novice Gatherer");
        }
        else if (resourcesHarvestedThisSession == 100)
        {
            Debug.Log("Achievement: Experienced Gatherer");
        }
        else if (resourcesHarvestedThisSession == 1000)
        {
            Debug.Log("Achievement: Master Gatherer");
        }
    }
    
    private void DayChanged(int newDay)
    {
        currentDay = newDay;
        Debug.Log($"Day {currentDay} has begun!");
        
        // Daily events or resets could happen here
        RefreshResourceNodes();
    }
    
    private void RefreshResourceNodes()
    {
        // Find all resource nodes and refresh them
        ResourceNode[] nodes = FindObjectsOfType<ResourceNode>();
        foreach (ResourceNode node in nodes)
        {
            node.ForceRespawn();
        }
        
        Debug.Log("All resource nodes have been refreshed for the new day!");
    }
    
    private void StartRandomEvent()
    {
        // Random event types
        string[] eventTypes = {
            "Double XP",
            "Better Drops",
            "Resource Rush",
            "Monster Hunt"
        };
        
        // Select random event
        int eventIndex = Random.Range(0, eventTypes.Length);
        string eventType = eventTypes[eventIndex];
        
        // Set event duration (10-20 minutes)
        float eventDuration = Random.Range(600f, 1200f);
        
        StartSpecialEvent(eventType, eventDuration);
    }
    
    public void StartSpecialEvent(string eventName, float duration)
    {
        specialEvent = true;
        currentEventName = eventName;
        eventTimeRemaining = duration;
        
        // Set event bonuses based on type
        switch (eventName)
        {
            case "Double XP":
                eventExpBonus = 1.0f; // 100% extra exp
                eventDropRateBonus = 0f;
                break;
            case "Better Drops":
                eventExpBonus = 0f;
                eventDropRateBonus = 0.5f; // 50% better drop rates
                break;
            case "Resource Rush":
                eventExpBonus = 0.5f; // 50% extra exp
                eventDropRateBonus = 0.5f; // 50% better drop rates
                // Refresh all resource nodes
                RefreshResourceNodes();
                break;
            case "Monster Hunt":
                eventExpBonus = 0.75f; // 75% extra exp
                eventDropRateBonus = 0.25f; // 25% better drop rates
                // Potentially spawn more enemies here
                break;
        }
        
        Debug.Log($"Special event started: {eventName} for {duration/60f:F1} minutes!");
        // UIManager.Instance.ShowEventNotification(eventName, duration);
    }
    
    private void EndSpecialEvent()
    {
        specialEvent = false;
        currentEventName = "";
        eventExpBonus = 0f;
        eventDropRateBonus = 0f;
        
        Debug.Log("Special event ended!");
        // UIManager.Instance.HideEventNotification();
    }
    
    // Adjust global drop and exp rates for balancing
    public void SetGlobalExpMultiplier(float multiplier)
    {
        globalExpMultiplier = multiplier;
    }
    
    public void SetGlobalDropRateMultiplier(float multiplier)
    {
        globalDropRateMultiplier = multiplier;
    }
    
    // Add a method to show a tip about accessing the item encyclopedia
    public void ShowItemEncyclopediaTip()
    {
        // Only show this tip once
        if (PlayerPrefs.GetInt("EncyclopediaTipShown", 0) == 0)
        {
            // Show UI message or tooltip explaining how to access the item encyclopedia
            Debug.Log("TIP: Press Shift+I to access the Item Encyclopedia and learn about items in the game world!");
            
            // You would typically display this in the UI rather than just a debug log
            // For example:
            // uiManager.ShowTip("Press Shift+I to access the Item Encyclopedia!");
            
            // Mark as shown
            PlayerPrefs.SetInt("EncyclopediaTipShown", 1);
            PlayerPrefs.Save();
        }
    }
}

// Simple Quest class for basic quest system
[System.Serializable]
public class Quest
{
    public string title;
    public string description;
    public int experienceReward;
    public QuestType type;
    public int target;
    public int progress;
    
    public bool IsComplete()
    {
        return progress >= target;
    }
}

public enum QuestType
{
    KillEnemies,
    CollectItems,
    TalkToNPC,
    ExploreArea,
    CraftItem
} 