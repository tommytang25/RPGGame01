using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(MonsterDatabase))]
public class MonsterDatabaseEditor : Editor
{
    private bool[] foldoutStates;
    private bool showAreaFilter = false;
    private bool showRarityFilter = false;
    private Dictionary<AreaType, bool> areaFilters = new Dictionary<AreaType, bool>();
    private Dictionary<MonsterRarity, bool> rarityFilters = new Dictionary<MonsterRarity, bool>();
    
    private SerializedProperty monsterTypesProperty;
    private Vector2 scrollPosition;
    
    private void OnEnable()
    {
        monsterTypesProperty = serializedObject.FindProperty("monsterTypes");
        
        // Initialize foldout states
        foldoutStates = new bool[monsterTypesProperty.arraySize];
        
        // Initialize filters
        InitializeFilters();
    }
    
    private void InitializeFilters()
    {
        // Area filters
        foreach (AreaType areaType in System.Enum.GetValues(typeof(AreaType)))
        {
            if (!areaFilters.ContainsKey(areaType))
            {
                areaFilters[areaType] = false;
            }
        }
        
        // Rarity filters
        foreach (MonsterRarity rarity in System.Enum.GetValues(typeof(MonsterRarity)))
        {
            if (!rarityFilters.ContainsKey(rarity))
            {
                rarityFilters[rarity] = false;
            }
        }
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        MonsterDatabase database = (MonsterDatabase)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Monster Database", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Assign Unique IDs", GUILayout.Width(120)))
        {
            #if UNITY_EDITOR
            database.AssignUniqueIds();
            #endif
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        // Display database stats
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Total Monsters: {monsterTypesProperty.arraySize}", EditorStyles.boldLabel);
        
        // Count by rarity
        Dictionary<MonsterRarity, int> rarityCount = new Dictionary<MonsterRarity, int>();
        foreach (MonsterRarity rarity in System.Enum.GetValues(typeof(MonsterRarity)))
        {
            rarityCount[rarity] = 0;
        }
        
        // Count by area
        Dictionary<AreaType, int> areaCount = new Dictionary<AreaType, int>();
        foreach (AreaType area in System.Enum.GetValues(typeof(AreaType)))
        {
            areaCount[area] = 0;
        }
        
        // Populate counts
        for (int i = 0; i < monsterTypesProperty.arraySize; i++)
        {
            SerializedProperty monsterProperty = monsterTypesProperty.GetArrayElementAtIndex(i);
            SerializedProperty rarityProperty = monsterProperty.FindPropertyRelative("rarity");
            SerializedProperty areasProperty = monsterProperty.FindPropertyRelative("spawnAreas");
            
            MonsterRarity rarity = (MonsterRarity)rarityProperty.enumValueIndex;
            rarityCount[rarity]++;
            
            for (int j = 0; j < areasProperty.arraySize; j++)
            {
                SerializedProperty areaProperty = areasProperty.GetArrayElementAtIndex(j);
                AreaType area = (AreaType)areaProperty.enumValueIndex;
                areaCount[area]++;
            }
        }
        
        // Display rarity counts
        EditorGUILayout.BeginHorizontal();
        foreach (var kv in rarityCount)
        {
            EditorGUILayout.LabelField($"{kv.Key}: {kv.Value}", EditorStyles.miniLabel, GUILayout.Width(100));
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        // Filters
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // Area filter
        showAreaFilter = EditorGUILayout.Foldout(showAreaFilter, "Filter by Area", true);
        
        // Rarity filter
        showRarityFilter = EditorGUILayout.Foldout(showRarityFilter, "Filter by Rarity", true);
        
        // Clear filters button
        if (GUILayout.Button("Clear Filters", GUILayout.Width(100)))
        {
            ClearFilters();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Display area filters if expanded
        if (showAreaFilter)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All", EditorStyles.miniButton))
            {
                foreach (AreaType area in areaFilters.Keys.ToList())
                {
                    areaFilters[area] = true;
                }
            }
            
            if (GUILayout.Button("Select None", EditorStyles.miniButton))
            {
                foreach (AreaType area in areaFilters.Keys.ToList())
                {
                    areaFilters[area] = false;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            int areaIndex = 0;
            foreach (AreaType area in System.Enum.GetValues(typeof(AreaType)))
            {
                areaIndex++;
                
                // Create a new row every 3 areas
                if (areaIndex % 4 == 1 && areaIndex > 1)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                
                areaFilters[area] = EditorGUILayout.ToggleLeft(area.ToString(), areaFilters[area], GUILayout.Width(100));
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        // Display rarity filters if expanded
        if (showRarityFilter)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All", EditorStyles.miniButton))
            {
                foreach (MonsterRarity rarity in rarityFilters.Keys.ToList())
                {
                    rarityFilters[rarity] = true;
                }
            }
            
            if (GUILayout.Button("Select None", EditorStyles.miniButton))
            {
                foreach (MonsterRarity rarity in rarityFilters.Keys.ToList())
                {
                    rarityFilters[rarity] = false;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            foreach (MonsterRarity rarity in System.Enum.GetValues(typeof(MonsterRarity)))
            {
                rarityFilters[rarity] = EditorGUILayout.ToggleLeft(rarity.ToString(), rarityFilters[rarity], GUILayout.Width(100));
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.EndVertical();
        
        // Add button
        if (GUILayout.Button("Add New Monster"))
        {
            AddNewMonster();
        }
        
        EditorGUILayout.Space();
        
        // Scroll view for monsters
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Display each monster
        for (int i = 0; i < monsterTypesProperty.arraySize; i++)
        {
            SerializedProperty monsterProperty = monsterTypesProperty.GetArrayElementAtIndex(i);
            
            // Check if this monster passes the filters
            if (!PassesFilters(monsterProperty))
                continue;
                
            DisplayMonsterField(monsterProperty, i);
        }
        
        EditorGUILayout.EndScrollView();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void ClearFilters()
    {
        foreach (AreaType area in areaFilters.Keys.ToList())
        {
            areaFilters[area] = false;
        }
        
        foreach (MonsterRarity rarity in rarityFilters.Keys.ToList())
        {
            rarityFilters[rarity] = false;
        }
    }
    
    private bool PassesFilters(SerializedProperty monsterProperty)
    {
        // If no filters are active, show all monsters
        bool anyAreaFilterActive = areaFilters.Values.Any(v => v);
        bool anyRarityFilterActive = rarityFilters.Values.Any(v => v);
        
        if (!anyAreaFilterActive && !anyRarityFilterActive)
            return true;
            
        // Check rarity filter
        if (anyRarityFilterActive)
        {
            SerializedProperty rarityProperty = monsterProperty.FindPropertyRelative("rarity");
            MonsterRarity rarity = (MonsterRarity)rarityProperty.enumValueIndex;
            
            if (!rarityFilters[rarity])
                return false;
        }
        
        // Check area filter
        if (anyAreaFilterActive)
        {
            SerializedProperty areasProperty = monsterProperty.FindPropertyRelative("spawnAreas");
            bool matchesAnySelectedArea = false;
            
            for (int j = 0; j < areasProperty.arraySize; j++)
            {
                SerializedProperty areaProperty = areasProperty.GetArrayElementAtIndex(j);
                AreaType area = (AreaType)areaProperty.enumValueIndex;
                
                if (areaFilters[area])
                {
                    matchesAnySelectedArea = true;
                    break;
                }
            }
            
            if (!matchesAnySelectedArea)
                return false;
        }
        
        return true;
    }
    
    private void DisplayMonsterField(SerializedProperty monsterProperty, int index)
    {
        if (foldoutStates.Length <= index)
        {
            System.Array.Resize(ref foldoutStates, index + 1);
        }
        
        SerializedProperty idProperty = monsterProperty.FindPropertyRelative("id");
        SerializedProperty nameProperty = monsterProperty.FindPropertyRelative("name");
        SerializedProperty rarityProperty = monsterProperty.FindPropertyRelative("rarity");
        SerializedProperty iconProperty = monsterProperty.FindPropertyRelative("icon");
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Header with basic info
        EditorGUILayout.BeginHorizontal();
        
        // Monster icon if available
        if (iconProperty.objectReferenceValue != null)
        {
            GUILayout.Label(AssetPreview.GetAssetPreview(iconProperty.objectReferenceValue), GUILayout.Width(32), GUILayout.Height(32));
        }
        else
        {
            GUILayout.Label(EditorGUIUtility.IconContent("d_VisualEffect Gizmo"), GUILayout.Width(32), GUILayout.Height(32));
        }
        
        // Foldout with ID and name
        foldoutStates[index] = EditorGUILayout.Foldout(foldoutStates[index], $"#{idProperty.intValue}: {nameProperty.stringValue}", true);
        
        // Rarity display
        MonsterRarity rarity = (MonsterRarity)rarityProperty.enumValueIndex;
        GUI.color = GetRarityColor(rarity);
        EditorGUILayout.LabelField(rarity.ToString(), EditorStyles.boldLabel, GUILayout.Width(100));
        GUI.color = Color.white;
        
        // Delete button
        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            if (EditorUtility.DisplayDialog("Delete Monster", 
                $"Are you sure you want to delete {nameProperty.stringValue}?", 
                "Delete", "Cancel"))
            {
                monsterTypesProperty.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
                return;
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Display fields if expanded
        if (foldoutStates[index])
        {
            EditorGUILayout.PropertyField(idProperty);
            EditorGUILayout.PropertyField(nameProperty);
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("prefab"));
            EditorGUILayout.PropertyField(iconProperty);
            EditorGUILayout.PropertyField(rarityProperty);
            
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("spawnAreas"), true);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("baseHealth"));
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("baseDamage"));
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("baseSpeed"));
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("baseExperience"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scaling", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("healthScaling"));
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("damageScaling"));
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("experienceScaling"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Loot Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("guaranteedLoot"), true);
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("goldDropBase"));
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("goldDropVariance"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Special Abilities", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("canTeleport"));
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("canHeal"));
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("hasRangedAttack"));
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("canSummonMinions"));
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void AddNewMonster()
    {
        // Add a new element to the array
        monsterTypesProperty.arraySize++;
        
        // Get the newly added element
        SerializedProperty newMonster = monsterTypesProperty.GetArrayElementAtIndex(monsterTypesProperty.arraySize - 1);
        
        // Set default values
        newMonster.FindPropertyRelative("id").intValue = 0; // Will be assigned properly with AssignUniqueIds
        newMonster.FindPropertyRelative("name").stringValue = "New Monster";
        newMonster.FindPropertyRelative("rarity").enumValueIndex = (int)MonsterRarity.Common;
        newMonster.FindPropertyRelative("baseHealth").intValue = 100;
        newMonster.FindPropertyRelative("baseDamage").intValue = 10;
        newMonster.FindPropertyRelative("baseSpeed").floatValue = 3f;
        newMonster.FindPropertyRelative("baseExperience").intValue = 50;
        newMonster.FindPropertyRelative("healthScaling").intValue = 20;
        newMonster.FindPropertyRelative("damageScaling").intValue = 2;
        newMonster.FindPropertyRelative("experienceScaling").floatValue = 15f;
        newMonster.FindPropertyRelative("goldDropBase").floatValue = 5f;
        newMonster.FindPropertyRelative("goldDropVariance").floatValue = 3f;
        
        // Expand the new monster in the editor
        System.Array.Resize(ref foldoutStates, monsterTypesProperty.arraySize);
        foldoutStates[monsterTypesProperty.arraySize - 1] = true;
        
        serializedObject.ApplyModifiedProperties();
        
        // Auto-assign IDs
        MonsterDatabase database = (MonsterDatabase)target;
        #if UNITY_EDITOR
        database.AssignUniqueIds();
        #endif
    }
    
    private Color GetRarityColor(MonsterRarity rarity)
    {
        switch (rarity)
        {
            case MonsterRarity.Common:
                return Color.white;
            case MonsterRarity.Uncommon:
                return new Color(0.0f, 1.0f, 0.0f); // Green
            case MonsterRarity.Rare:
                return new Color(0.0f, 0.5f, 1.0f); // Blue
            case MonsterRarity.Elite:
                return new Color(1.0f, 0.0f, 1.0f); // Magenta
            case MonsterRarity.Boss:
                return new Color(1.0f, 0.0f, 0.0f); // Red
            default:
                return Color.white;
        }
    }
} 