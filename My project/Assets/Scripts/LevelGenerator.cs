using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Environment")]
    public int levelLength = 100;
    public int platformCount = 10;
    public int bridgeCount = 3;
    
    [Header("Enemies")]
    public int enemyCount = 5;
    public GameObject enemyPrefab; // Optional

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        // 1. Create Ground (Long strip at Y=0, Z=0)
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube); // Cube is better for platformer floor than Plane typically
        floor.name = "Level_Ground";
        floor.transform.localScale = new Vector3(levelLength, 1, 2); // Long X, Thin Y, Narrow Z
        floor.transform.position = new Vector3(0, -0.5f, 0);
        // Make floor dark grey
        floor.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);

        // 2. Create Platforms (Above ground)
        for (int i = 0; i < platformCount; i++)
        {
            CreatePlatform();
        }

        // 3. Create Bridges (Just longer platforms)
        for (int i = 0; i < bridgeCount; i++)
        {
            CreateBridge();
        }

        // 4. Spawn Enemies
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    void CreatePlatform()
    {
        GameObject plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plat.name = "Platform";
        
        // Random Position along X
        float x = Random.Range(-levelLength / 2f, levelLength / 2f);
        float z = 0;
        float y = Random.Range(2f, 5f); // Height
        
        // Scale
        float scaleX = Random.Range(2f, 5f);
        
        plat.transform.position = new Vector3(x, y, z);
        plat.transform.localScale = new Vector3(scaleX, 0.5f, 1.5f); // Z thickness 1.5
        plat.GetComponent<Renderer>().material.color = new Color(0.4f, 0.6f, 1.0f); // Light blue
    }

    void CreateBridge()
    {
        GameObject bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge.name = "Bridge";

        // Random Position
        float x = Random.Range(-levelLength / 2f, levelLength / 2f);
        float z = 0;
        float y = Random.Range(3f, 7f); 

        // Long
        bridge.transform.position = new Vector3(x, y, z);
        bridge.transform.localScale = new Vector3(8f, 0.2f, 1.5f); 
        // No rotation for 2D platformer typically
        
        bridge.GetComponent<Renderer>().material.color = new Color(0.6f, 0.4f, 0.2f); // Brown
    }

    void SpawnEnemy()
    {
        GameObject enemyObj;
        
        if (enemyPrefab != null)
        {
            enemyObj = Instantiate(enemyPrefab);
        }
        else
        {
            enemyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemyObj.AddComponent<Enemy>();
        }

        enemyObj.name = "Enemy_Generated";
        
        // Random X on the ground or low height
        float x = Random.Range(-levelLength / 2f, levelLength / 2f);
        float z = 0;
        
        enemyObj.transform.position = new Vector3(x, 1f, z);
        
        // If we created a primitive, the Enemy script's Start() will color it red
    }
}
