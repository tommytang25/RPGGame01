using UnityEngine;
using System.Collections;

public class TimeAbilityController : MonoBehaviour
{
    // Singleton instance
    public static TimeAbilityController Instance { get; private set; }
    
    [Header("Time Slow Ability")]
    public bool timeSlowUnlocked = false;
    public float timeSlowFactor = 0.5f; // How much to slow time (0.5 = half speed)
    public float timeSlowDuration = 5f; // In seconds
    public float timeSlowCooldown = 15f; // In seconds
    private bool timeSlowOnCooldown = false;
    private float timeSlowCooldownRemaining = 0f;
    
    [Header("UI Elements")]
    public GameObject timeAbilityUI; // UI panel showing ability cooldown
    public UnityEngine.UI.Image cooldownFillImage; // Fill image showing cooldown progress
    
    [Header("VFX")]
    public GameObject timeSlowVFX; // Visual effect for time slow
    
    // Audio
    private AudioSource audioSource;
    public AudioClip timeSlowStartSound;
    public AudioClip timeSlowEndSound;
    
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
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (timeSlowStartSound != null || timeSlowEndSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void Start()
    {
        // Initialize UI
        if (timeAbilityUI != null)
        {
            timeAbilityUI.SetActive(timeSlowUnlocked);
        }
        
        if (timeSlowVFX != null)
        {
            timeSlowVFX.SetActive(false);
        }
        
        UpdateCooldownUI();
    }
    
    private void Update()
    {
        // Check for activation of time slow ability
        if (timeSlowUnlocked && !timeSlowOnCooldown && Input.GetKeyDown(KeyCode.F))
        {
            ActivateTimeSlow();
        }
        
        // Update cooldown timer
        if (timeSlowOnCooldown)
        {
            timeSlowCooldownRemaining -= Time.unscaledDeltaTime;
            
            if (timeSlowCooldownRemaining <= 0f)
            {
                timeSlowOnCooldown = false;
                timeSlowCooldownRemaining = 0f;
            }
            
            UpdateCooldownUI();
        }
    }
    
    public void UnlockTimeSlowAbility()
    {
        timeSlowUnlocked = true;
        
        // Show ability UI
        if (timeAbilityUI != null)
        {
            timeAbilityUI.SetActive(true);
        }
        
        Debug.Log("Time slow ability unlocked!");
    }
    
    private void ActivateTimeSlow()
    {
        StartCoroutine(TimeSlowSequence());
    }
    
    private IEnumerator TimeSlowSequence()
    {
        // Begin cooldown
        timeSlowOnCooldown = true;
        timeSlowCooldownRemaining = timeSlowCooldown;
        
        // Play activation sound
        if (audioSource != null && timeSlowStartSound != null)
        {
            audioSource.PlayOneShot(timeSlowStartSound);
        }
        
        // Show VFX
        if (timeSlowVFX != null)
        {
            timeSlowVFX.SetActive(true);
        }
        
        // Slow down time
        Time.timeScale = timeSlowFactor;
        
        // Keep physics consistent
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        
        Debug.Log("Time slow activated!");
        
        // Wait for the duration (use unscaled time since time is slowed)
        yield return new WaitForSecondsRealtime(timeSlowDuration);
        
        // Return to normal time
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        
        // Play end sound
        if (audioSource != null && timeSlowEndSound != null)
        {
            audioSource.PlayOneShot(timeSlowEndSound);
        }
        
        // Hide VFX
        if (timeSlowVFX != null)
        {
            timeSlowVFX.SetActive(false);
        }
        
        Debug.Log("Time slow ended!");
    }
    
    private void UpdateCooldownUI()
    {
        if (cooldownFillImage != null)
        {
            if (timeSlowOnCooldown)
            {
                // Show cooldown as a fill amount (1 = full, 0 = empty)
                float fillAmount = 1f - (timeSlowCooldownRemaining / timeSlowCooldown);
                cooldownFillImage.fillAmount = fillAmount;
            }
            else
            {
                // Ability is ready
                cooldownFillImage.fillAmount = 1f;
            }
        }
    }
    
    // Method to upgrade the time slow ability as the player progresses
    public void UpgradeTimeSlowAbility(float newDuration, float newCooldown)
    {
        timeSlowDuration = newDuration;
        timeSlowCooldown = newCooldown;
        
        Debug.Log("Time slow ability upgraded! New duration: " + timeSlowDuration + "s, New cooldown: " + timeSlowCooldown + "s");
    }
    
    // Future time-related abilities could be added here
    // For example: time rewind, time freeze, etc.
} 