using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class LootPopup : MonoBehaviour
{
    [SerializeField] private GameObject itemEntryPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private float entrySpacing = 25f;
    [SerializeField] private float fadeInTime = 0.25f;
    [SerializeField] private float showTime = 2.5f;
    [SerializeField] private float fadeOutTime = 0.5f;
    [SerializeField] private Vector2 startOffset = new Vector2(0, -50);
    [SerializeField] private AnimationCurve moveCurve;
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private List<GameObject> itemEntries = new List<GameObject>();
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("LootPopup must be attached to a UI element with a RectTransform!");
        }
        
        // Initialize default fade in/out curve if not set
        if (moveCurve.keys.Length == 0)
        {
            moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        
        // Start invisible
        canvasGroup.alpha = 0;
    }
    
    public void SetItems(List<Item> items)
    {
        if (items == null || items.Count == 0 || contentParent == null || itemEntryPrefab == null)
            return;
            
        // Clear any existing entries
        ClearItemEntries();
        
        // Group items by ID for stacking
        Dictionary<string, int> itemCounts = new Dictionary<string, int>();
        Dictionary<string, Item> itemReferences = new Dictionary<string, Item>();
        
        foreach (Item item in items)
        {
            if (item == null) continue;
            
            if (itemCounts.ContainsKey(item.id))
            {
                itemCounts[item.id]++;
            }
            else
            {
                itemCounts[item.id] = 1;
                itemReferences[item.id] = item;
            }
        }
        
        // Create UI elements for each item type
        float yOffset = 0;
        foreach (var kvp in itemCounts)
        {
            string itemId = kvp.Key;
            int count = kvp.Value;
            Item item = itemReferences[itemId];
            
            // Create item entry
            GameObject entryObj = Instantiate(itemEntryPrefab, contentParent);
            itemEntries.Add(entryObj);
            
            // Position entry
            RectTransform entryRect = entryObj.GetComponent<RectTransform>();
            if (entryRect != null)
            {
                entryRect.anchoredPosition = new Vector2(0, yOffset);
                yOffset -= entrySpacing;
            }
            
            // Set entry content
            SetupItemEntry(entryObj, item, count);
        }
        
        // Resize content parent if needed
        if (contentParent.TryGetComponent<RectTransform>(out RectTransform contentRect))
        {
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, Mathf.Abs(yOffset));
        }
        
        // Animate the popup
        StartCoroutine(AnimatePopup());
    }
    
    private void SetupItemEntry(GameObject entryObj, Item item, int count)
    {
        if (item == null) return;
        
        // Set icon if available
        Image iconImage = entryObj.transform.Find("ItemIcon")?.GetComponent<Image>();
        if (iconImage != null)
        {
            Sprite itemSprite = Resources.Load<Sprite>($"Items/{item.id}");
            if (itemSprite != null)
            {
                iconImage.sprite = itemSprite;
            }
            else
            {
                // Use default sprite based on item type
                iconImage.sprite = Resources.Load<Sprite>($"Items/default_{item.type.ToString().ToLower()}");
            }
        }
        
        // Set item name with rarity color
        TextMeshProUGUI nameText = entryObj.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = item.GetColoredName();
        }
        
        // Set quantity if more than 1
        TextMeshProUGUI quantityText = entryObj.transform.Find("ItemQuantity")?.GetComponent<TextMeshProUGUI>();
        if (quantityText != null)
        {
            quantityText.text = count > 1 ? $"x{count}" : "";
        }
        
        // Set rarity background if available
        Image rarityBg = entryObj.transform.Find("RarityBG")?.GetComponent<Image>();
        if (rarityBg != null)
        {
            Color rarityColor = item.GetRarityColor();
            rarityColor.a = 0.2f; // Make it semi-transparent
            rarityBg.color = rarityColor;
        }
    }
    
    private void ClearItemEntries()
    {
        foreach (GameObject entry in itemEntries)
        {
            Destroy(entry);
        }
        itemEntries.Clear();
    }
    
    private IEnumerator AnimatePopup()
    {
        // Store original position
        Vector2 targetPosition = rectTransform.anchoredPosition;
        Vector2 startPosition = targetPosition + startOffset;
        
        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInTime);
            float curveT = moveCurve.Evaluate(t);
            
            canvasGroup.alpha = t;
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveT);
            
            yield return null;
        }
        
        // Ensure we reach target values
        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = targetPosition;
        
        // Wait for display duration
        yield return new WaitForSeconds(showTime);
        
        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutTime);
            
            canvasGroup.alpha = 1f - t;
            
            yield return null;
        }
        
        // Ensure we reach target values
        canvasGroup.alpha = 0f;
    }
} 