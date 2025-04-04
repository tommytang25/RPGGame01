using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueSystem : MonoBehaviour
{
    // Singleton instance
    public static DialogueSystem Instance { get; private set; }
    
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public Text dialogueText;
    public Text speakerNameText;
    public Image speakerPortrait;
    public GameObject choicePanel;
    public GameObject choiceButtonPrefab;
    public Transform choiceContainer;
    
    [Header("Dialogue Settings")]
    public float typingSpeed = 0.05f;
    
    // Current dialogue data
    private Dialogue currentDialogue;
    private int currentNodeIndex;
    private bool isDisplayingText;
    private Coroutine typingCoroutine;
    
    // Events
    public delegate void DialogueEndEvent();
    public event DialogueEndEvent OnDialogueEnd;
    
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
        // Initialize UI
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Skip typing animation or proceed to next dialogue when player presses space/click
        if (isDisplayingText && Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (dialogueText.text != currentDialogue.nodes[currentNodeIndex].text)
            {
                // Skip typing animation
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentDialogue.nodes[currentNodeIndex].text;
                isDisplayingText = false;
                
                // Show choices if this node has them
                if (currentDialogue.nodes[currentNodeIndex].choices.Count > 0)
                {
                    ShowChoices();
                }
            }
            else if (currentDialogue.nodes[currentNodeIndex].choices.Count == 0)
            {
                // No choices, just go to next node
                ProceedToNextNode();
            }
        }
    }
    
    public void StartDialogue(Dialogue dialogue)
    {
        currentDialogue = dialogue;
        currentNodeIndex = 0;
        
        // Show dialogue panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        // Pause the game during dialogue
        Time.timeScale = 0f;
        
        // Display first node
        DisplayNode(currentNodeIndex);
    }
    
    private void DisplayNode(int nodeIndex)
    {
        if (nodeIndex >= currentDialogue.nodes.Count)
        {
            EndDialogue();
            return;
        }
        
        DialogueNode node = currentDialogue.nodes[nodeIndex];
        
        // Set speaker name and portrait
        if (speakerNameText != null)
        {
            speakerNameText.text = node.speakerName;
        }
        
        if (speakerPortrait != null && node.speakerPortrait != null)
        {
            speakerPortrait.sprite = node.speakerPortrait;
        }
        
        // Start typing text
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        typingCoroutine = StartCoroutine(TypeText(node.text));
    }
    
    private IEnumerator TypeText(string text)
    {
        isDisplayingText = true;
        dialogueText.text = "";
        
        foreach (char c in text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        
        isDisplayingText = false;
        
        // Show choices if this node has them
        if (currentDialogue.nodes[currentNodeIndex].choices.Count > 0)
        {
            ShowChoices();
        }
    }
    
    private void ShowChoices()
    {
        if (choicePanel == null || choiceButtonPrefab == null || choiceContainer == null)
            return;
            
        // Clear existing choices
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Show choice panel
        choicePanel.SetActive(true);
        
        // Create buttons for each choice
        List<DialogueChoice> choices = currentDialogue.nodes[currentNodeIndex].choices;
        
        for (int i = 0; i < choices.Count; i++)
        {
            GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
            
            int choiceIndex = i; // Store index for button callback
            
            // Set button text
            Text buttonText = choiceButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = choices[i].text;
            }
            
            // Add button click handler
            Button button = choiceButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
            }
        }
    }
    
    private void OnChoiceSelected(int choiceIndex)
    {
        // Hide choice panel
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
        
        // Get the next node index from the selected choice
        DialogueChoice choice = currentDialogue.nodes[currentNodeIndex].choices[choiceIndex];
        
        // Handle any effects of this choice
        HandleChoiceEffects(choice);
        
        // Go to the next node
        currentNodeIndex = choice.nextNodeIndex;
        DisplayNode(currentNodeIndex);
    }
    
    private void HandleChoiceEffects(DialogueChoice choice)
    {
        // TODO: Implement choice effects (e.g., changing reputation, triggering quests, etc.)
        if (!string.IsNullOrEmpty(choice.questId))
        {
            // Trigger a quest
            Debug.Log("Triggering quest: " + choice.questId);
            // QuestManager.Instance.StartQuest(choice.questId);
        }
        
        if (choice.changePlayerStats)
        {
            // Modify player stats
            Debug.Log("Changing player stats");
            // PlayerStats.Instance.ModifyStats(choice.statChanges);
        }
    }
    
    private void ProceedToNextNode()
    {
        currentNodeIndex++;
        
        if (currentNodeIndex < currentDialogue.nodes.Count)
        {
            DisplayNode(currentNodeIndex);
        }
        else
        {
            EndDialogue();
        }
    }
    
    private void EndDialogue()
    {
        // Hide dialogue panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        // Hide choice panel
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
        
        // Resume game
        Time.timeScale = 1f;
        
        // Trigger event
        if (OnDialogueEnd != null)
        {
            OnDialogueEnd.Invoke();
        }
    }
}

[System.Serializable]
public class Dialogue
{
    public string id;
    public List<DialogueNode> nodes = new List<DialogueNode>();
}

[System.Serializable]
public class DialogueNode
{
    public string speakerName;
    public Sprite speakerPortrait;
    public string text;
    public List<DialogueChoice> choices = new List<DialogueChoice>();
}

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public int nextNodeIndex;
    
    // Optional effects
    public string questId;
    public bool changePlayerStats;
    public Dictionary<string, int> statChanges = new Dictionary<string, int>();
} 