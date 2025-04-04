using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnArea))]
public class SpawnAreaEditor : Editor
{
    SerializedProperty areaType;
    SerializedProperty areaName;
    SerializedProperty isActive;
    SerializedProperty maxActiveMonsters;
    SerializedProperty spawnInterval;
    SerializedProperty minSpawnDistance;
    SerializedProperty maxSpawnRetries;
    SerializedProperty areaSize;
    SerializedProperty useBoxCollider;
    SerializedProperty visualizeArea;
    SerializedProperty areaLevel;
    SerializedProperty eliteChance;
    SerializedProperty rareChance;
    SerializedProperty allowBosses;
    
    private void OnEnable()
    {
        areaType = serializedObject.FindProperty("areaType");
        areaName = serializedObject.FindProperty("areaName");
        isActive = serializedObject.FindProperty("isActive");
        maxActiveMonsters = serializedObject.FindProperty("maxActiveMonsters");
        spawnInterval = serializedObject.FindProperty("spawnInterval");
        minSpawnDistance = serializedObject.FindProperty("minSpawnDistance");
        maxSpawnRetries = serializedObject.FindProperty("maxSpawnRetries");
        areaSize = serializedObject.FindProperty("areaSize");
        useBoxCollider = serializedObject.FindProperty("useBoxCollider");
        visualizeArea = serializedObject.FindProperty("visualizeArea");
        areaLevel = serializedObject.FindProperty("areaLevel");
        eliteChance = serializedObject.FindProperty("eliteChance");
        rareChance = serializedObject.FindProperty("rareChance");
        allowBosses = serializedObject.FindProperty("allowBosses");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("Area Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(areaType);
        EditorGUILayout.PropertyField(areaName);
        EditorGUILayout.PropertyField(isActive);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spawn Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(maxActiveMonsters);
        EditorGUILayout.PropertyField(spawnInterval);
        EditorGUILayout.PropertyField(minSpawnDistance);
        EditorGUILayout.PropertyField(maxSpawnRetries);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spawn Boundaries", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(areaSize);
        EditorGUILayout.PropertyField(useBoxCollider);
        EditorGUILayout.PropertyField(visualizeArea);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Difficulty Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(areaLevel);
        EditorGUILayout.PropertyField(eliteChance);
        EditorGUILayout.PropertyField(rareChance);
        EditorGUILayout.PropertyField(allowBosses);
        
        EditorGUILayout.Space();
        
        // Monster Database integration
        EditorGUILayout.LabelField("Monster Database Preview", EditorStyles.boldLabel);
        
        SpawnArea spawnArea = (SpawnArea)target;
        DisplayMonsterPreview(spawnArea);
        
        EditorGUILayout.Space();
        
        // Tools
        EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Update Box Collider"))
        {
            UpdateBoxCollider(spawnArea);
        }
        
        if (GUILayout.Button("Create Test Monsters"))
        {
            CreateTestMonsters(spawnArea);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DisplayMonsterPreview(SpawnArea spawnArea)
    {
        if (MonsterDatabase.Instance == null)
        {
            EditorGUILayout.HelpBox("Monster Database not found in scene. Add MonsterDatabase to your scene to see available monsters.", MessageType.Info);
            return;
        }
        
        List<MonsterType> monsters = MonsterDatabase.Instance.GetMonstersByArea(spawnArea.areaType);
        
        if (monsters.Count == 0)
        {
            EditorGUILayout.HelpBox($"No monsters defined for area type: {spawnArea.areaType}", MessageType.Warning);
            return;
        }
        
        EditorGUILayout.LabelField($"Available Monsters for {spawnArea.areaType}: {monsters.Count}");
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        foreach (MonsterType monster in monsters)
        {
            string rarityColor = GetRarityColor(monster.rarity);
            EditorGUILayout.BeginHorizontal();
            
            // Display monster icon if available
            if (monster.icon != null)
            {
                GUILayout.Label(AssetPreview.GetAssetPreview(monster.icon), GUILayout.Width(32), GUILayout.Height(32));
            }
            else
            {
                GUILayout.Label("", GUILayout.Width(32), GUILayout.Height(32));
            }
            
            // Display monster name and info
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"<color={rarityColor}>{monster.name}</color> ({monster.rarity})", new GUIStyle(EditorStyles.label) { richText = true });
            EditorGUILayout.LabelField($"HP: {monster.baseHealth}, DMG: {monster.baseDamage}, XP: {monster.baseExperience}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(2);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private string GetRarityColor(MonsterRarity rarity)
    {
        switch (rarity)
        {
            case MonsterRarity.Common:
                return "#FFFFFF"; // White
            case MonsterRarity.Uncommon:
                return "#00FF00"; // Green
            case MonsterRarity.Rare:
                return "#0080FF"; // Blue
            case MonsterRarity.Elite:
                return "#FF00FF"; // Magenta
            case MonsterRarity.Boss:
                return "#FF0000"; // Red
            default:
                return "#FFFFFF";
        }
    }
    
    private void UpdateBoxCollider(SpawnArea spawnArea)
    {
        if (spawnArea.useBoxCollider)
        {
            BoxCollider2D boxCollider = spawnArea.GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = Undo.AddComponent<BoxCollider2D>(spawnArea.gameObject);
            }
            
            Undo.RecordObject(boxCollider, "Update Box Collider");
            boxCollider.size = spawnArea.areaSize;
            boxCollider.isTrigger = true;
            EditorUtility.SetDirty(boxCollider);
        }
    }
    
    private void CreateTestMonsters(SpawnArea spawnArea)
    {
        if (MonsterDatabase.Instance == null)
        {
            EditorUtility.DisplayDialog("Error", "Monster Database not found in scene. Add MonsterDatabase to your scene first.", "OK");
            return;
        }
        
        List<MonsterType> monsters = MonsterDatabase.Instance.GetMonstersByArea(spawnArea.areaType);
        
        if (monsters.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", $"No monsters defined for area type: {spawnArea.areaType}", "OK");
            return;
        }
        
        // Create a container for test monsters
        GameObject container = new GameObject($"TestMonsters_{spawnArea.areaType}");
        Undo.RegisterCreatedObjectUndo(container, "Create Test Monsters");
        container.transform.position = spawnArea.transform.position;
        
        // Spawn one of each monster type
        foreach (MonsterType monster in monsters)
        {
            if (monster.prefab == null)
            {
                Debug.LogWarning($"Monster {monster.name} has no prefab assigned");
                continue;
            }
            
            // Calculate random position within spawn area
            Vector2 randomOffset = new Vector2(
                Random.Range(-spawnArea.areaSize.x/2, spawnArea.areaSize.x/2),
                Random.Range(-spawnArea.areaSize.y/2, spawnArea.areaSize.y/2)
            );
            
            Vector3 spawnPos = spawnArea.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            
            // Instantiate monster
            GameObject monsterObj = (GameObject)PrefabUtility.InstantiatePrefab(monster.prefab);
            Undo.RegisterCreatedObjectUndo(monsterObj, "Create Test Monster");
            monsterObj.transform.position = spawnPos;
            monsterObj.transform.SetParent(container.transform);
            monsterObj.name = monster.name;
            
            // Initialize the monster
            Enemy enemy = monsterObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Initialize(monster, spawnArea.areaLevel);
            }
        }
        
        Selection.activeGameObject = container;
    }
    
    private void OnSceneGUI()
    {
        SpawnArea spawnArea = (SpawnArea)target;
        
        if (!spawnArea.visualizeArea)
            return;
            
        // Draw spawn area in scene view
        Handles.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(
            spawnArea.transform.position,
            spawnArea.transform.rotation,
            Vector3.one
        );
        
        using (new Handles.DrawingScope(rotationMatrix))
        {
            Handles.DrawSolidRectangleWithOutline(
                new Rect(-spawnArea.areaSize.x/2, -spawnArea.areaSize.y/2, spawnArea.areaSize.x, spawnArea.areaSize.y),
                new Color(0.2f, 0.8f, 0.2f, 0.1f),
                new Color(0.2f, 0.8f, 0.2f, 0.8f)
            );
        }
        
        // Draw the minimum spawn distance from player
        Handles.color = new Color(0.8f, 0.2f, 0.2f, 0.5f);
        Handles.DrawWireDisc(spawnArea.transform.position, Vector3.forward, spawnArea.minSpawnDistance);
        
        // Add handles to resize the spawn area
        EditorGUI.BeginChangeCheck();
        Vector2 newSize = Handles.ScaleHandle(
            spawnArea.areaSize,
            spawnArea.transform.position,
            spawnArea.transform.rotation,
            HandleUtility.GetHandleSize(spawnArea.transform.position) * 0.5f
        );
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spawnArea, "Resize Spawn Area");
            spawnArea.areaSize = newSize;
            
            // Update box collider if used
            if (spawnArea.useBoxCollider)
            {
                BoxCollider2D boxCollider = spawnArea.GetComponent<BoxCollider2D>();
                if (boxCollider != null)
                {
                    Undo.RecordObject(boxCollider, "Resize Box Collider");
                    boxCollider.size = newSize;
                }
            }
            
            EditorUtility.SetDirty(spawnArea);
        }
    }
} 