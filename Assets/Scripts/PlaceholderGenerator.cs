using UnityEngine;

public class PlaceholderGenerator : MonoBehaviour
{
    [Header("Player Placeholder")]
    public Color playerColor = Color.blue;
    public float playerSize = 1f;
    
    [Header("Enemy Placeholder")]
    public Color enemyColor = Color.red;
    public float enemySize = 1f;
    
    [Header("NPC Placeholder")]
    public Color npcColor = Color.green;
    public float npcSize = 1f;
    
    [Header("Environment")]
    public Color groundColor = new Color(0.3f, 0.5f, 0.2f);
    public Color wallColor = new Color(0.5f, 0.3f, 0.1f);
    public Vector2 mapSize = new Vector2(50, 50);
    public float tileSize = 1f;
    
    [Header("Items")]
    public Color itemColor = Color.yellow;
    public float itemSize = 0.5f;
    
    private void Awake()
    {
        // Generate placeholder objects based on what's needed in the scene
        GeneratePlaceholderPlayer();
        GeneratePlaceholderEnvironment();
        GeneratePlaceholderEnemies();
        GeneratePlaceholderNPCs();
        GeneratePlaceholderItems();
    }
    
    private void GeneratePlaceholderPlayer()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
            return; // Player already exists
        
        GameObject player = CreatePrimitive(PrimitiveType.Capsule, playerColor, Vector3.zero, playerSize);
        player.name = "Player";
        player.tag = "Player";
        
        // Add required components
        player.AddComponent<Rigidbody2D>().gravityScale = 0;
        player.AddComponent<CircleCollider2D>();
        
        // Add player controller script
        player.AddComponent<PlayerController>();
        
        // Create a child object for the attack point
        GameObject attackPoint = new GameObject("AttackPoint");
        attackPoint.transform.SetParent(player.transform);
        attackPoint.transform.localPosition = new Vector3(0, 1, 0);
        
        // Set the attack point reference
        player.GetComponent<PlayerController>().attackPoint = attackPoint.transform;
    }
    
    private void GeneratePlaceholderEnvironment()
    {
        // Create parent object for environment
        GameObject environment = new GameObject("Environment");
        
        // Create ground plane
        GameObject ground = CreatePrimitive(PrimitiveType.Cube, groundColor, new Vector3(0, 0, 0), new Vector3(mapSize.x, mapSize.y, 0.1f));
        ground.name = "Ground";
        ground.transform.SetParent(environment.transform);
        
        // Create walls around the map
        float wallHeight = 1f;
        
        // Bottom wall
        GameObject wallBottom = CreatePrimitive(PrimitiveType.Cube, wallColor, new Vector3(0, -mapSize.y/2 - 0.5f, 0), new Vector3(mapSize.x + 2, 1, wallHeight));
        wallBottom.name = "WallBottom";
        wallBottom.transform.SetParent(environment.transform);
        wallBottom.AddComponent<BoxCollider2D>();
        
        // Top wall
        GameObject wallTop = CreatePrimitive(PrimitiveType.Cube, wallColor, new Vector3(0, mapSize.y/2 + 0.5f, 0), new Vector3(mapSize.x + 2, 1, wallHeight));
        wallTop.name = "WallTop";
        wallTop.transform.SetParent(environment.transform);
        wallTop.AddComponent<BoxCollider2D>();
        
        // Left wall
        GameObject wallLeft = CreatePrimitive(PrimitiveType.Cube, wallColor, new Vector3(-mapSize.x/2 - 0.5f, 0, 0), new Vector3(1, mapSize.y, wallHeight));
        wallLeft.name = "WallLeft";
        wallLeft.transform.SetParent(environment.transform);
        wallLeft.AddComponent<BoxCollider2D>();
        
        // Right wall
        GameObject wallRight = CreatePrimitive(PrimitiveType.Cube, wallColor, new Vector3(mapSize.x/2 + 0.5f, 0, 0), new Vector3(1, mapSize.y, wallHeight));
        wallRight.name = "WallRight";
        wallRight.transform.SetParent(environment.transform);
        wallRight.AddComponent<BoxCollider2D>();
        
        // Add some random obstacles
        int obstacleCount = 20;
        for (int i = 0; i < obstacleCount; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-mapSize.x/2 + 2, mapSize.x/2 - 2),
                Random.Range(-mapSize.y/2 + 2, mapSize.y/2 - 2),
                0
            );
            
            Vector3 randomSize = new Vector3(
                Random.Range(1f, 3f),
                Random.Range(1f, 3f),
                wallHeight
            );
            
            GameObject obstacle = CreatePrimitive(PrimitiveType.Cube, wallColor, randomPosition, randomSize);
            obstacle.name = "Obstacle_" + i;
            obstacle.transform.SetParent(environment.transform);
            obstacle.AddComponent<BoxCollider2D>();
        }
    }
    
    private void GeneratePlaceholderEnemies()
    {
        // Create some placeholder enemies at random positions
        int enemyCount = 5;
        
        // Create parent object for enemies
        GameObject enemiesParent = new GameObject("Enemies");
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-mapSize.x/2 + 5, mapSize.x/2 - 5),
                Random.Range(-mapSize.y/2 + 5, mapSize.y/2 - 5),
                0
            );
            
            GameObject enemy = CreatePrimitive(PrimitiveType.Capsule, enemyColor, randomPosition, enemySize);
            enemy.name = "Enemy_" + i;
            enemy.tag = "Enemy";
            enemy.layer = LayerMask.NameToLayer("Enemy");
            enemy.transform.SetParent(enemiesParent.transform);
            
            // Add required components
            enemy.AddComponent<Rigidbody2D>().gravityScale = 0;
            enemy.AddComponent<CircleCollider2D>();
            
            // Add enemy script
            enemy.AddComponent<Enemy>();
        }
    }
    
    private void GeneratePlaceholderNPCs()
    {
        // Create some placeholder NPCs at random positions
        int npcCount = 3;
        
        // Create parent object for NPCs
        GameObject npcsParent = new GameObject("NPCs");
        
        for (int i = 0; i < npcCount; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-mapSize.x/2 + 5, mapSize.x/2 - 5),
                Random.Range(-mapSize.y/2 + 5, mapSize.y/2 - 5),
                0
            );
            
            GameObject npc = CreatePrimitive(PrimitiveType.Capsule, npcColor, randomPosition, npcSize);
            npc.name = "NPC_" + i;
            npc.tag = "NPC";
            npc.transform.SetParent(npcsParent.transform);
            
            // Add required components
            npc.AddComponent<Rigidbody2D>().gravityScale = 0;
            CircleCollider2D col = npc.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            
            // Add NPC script
            NPC npcScript = npc.AddComponent<NPC>();
            npcScript.npcName = "Villager " + (i + 1);
            npcScript.npcType = NPCType.Villager;
            
            // Create waypoints for the NPC
            GameObject waypointsParent = new GameObject("Waypoints_NPC_" + i);
            waypointsParent.transform.SetParent(npc.transform);
            
            Transform[] waypoints = new Transform[3];
            for (int j = 0; j < 3; j++)
            {
                GameObject waypoint = new GameObject("Waypoint_" + j);
                waypoint.transform.SetParent(waypointsParent.transform);
                
                Vector3 waypointPosition = randomPosition + new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(-5f, 5f),
                    0
                );
                
                waypoint.transform.position = waypointPosition;
                waypoints[j] = waypoint.transform;
            }
            
            npcScript.waypoints = waypoints;
        }
    }
    
    private void GeneratePlaceholderItems()
    {
        // Create some placeholder items at random positions
        int itemCount = 8;
        
        // Create parent object for items
        GameObject itemsParent = new GameObject("Items");
        
        for (int i = 0; i < itemCount; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-mapSize.x/2 + 5, mapSize.x/2 - 5),
                Random.Range(-mapSize.y/2 + 5, mapSize.y/2 - 5),
                0
            );
            
            GameObject item = CreatePrimitive(PrimitiveType.Cube, itemColor, randomPosition, new Vector3(itemSize, itemSize, itemSize));
            item.name = "Item_" + i;
            item.tag = "Item";
            item.transform.SetParent(itemsParent.transform);
            
            // Add required components
            CircleCollider2D col = item.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            
            // Make items spin for visibility
            item.AddComponent<RotateObject>();
        }
    }
    
    private GameObject CreatePrimitive(PrimitiveType type, Color color, Vector3 position, float size)
    {
        return CreatePrimitive(type, color, position, new Vector3(size, size, size));
    }
    
    private GameObject CreatePrimitive(PrimitiveType type, Color color, Vector3 position, Vector3 scale)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        
        // Set the color
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;
        }
        
        // Remove the 3D collider to avoid conflicts with 2D physics
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
        
        return obj;
    }
}

// Simple component to make objects rotate (for pickups/items visibility)
public class RotateObject : MonoBehaviour
{
    public float rotateSpeed = 100f;
    
    private void Update()
    {
        transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
    }
}