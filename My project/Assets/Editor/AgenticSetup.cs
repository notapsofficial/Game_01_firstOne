using UnityEngine;
using UnityEditor;

public class AgenticSetup
{
    [MenuItem("Agentic/Setup Game")]
    public static void SetupGame()
    {
        // 0. Cleanup Old Objects
        DestroyIfExists("Ground");
        DestroyIfExists("Player");
        DestroyIfExists("GameManager");
        
        GameObject[] oldTargets = GameObject.FindGameObjectsWithTag("Respawn"); // We'll use Respawn tag or checking name for cleanup
        // Note: Finding by name pattern "TargetBlock" is safer locally
        foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
        {
            if (obj.name.StartsWith("TargetBlock")) Object.DestroyImmediate(obj);
        }

        // Ensure Audio exists
        AgenticAudioGenerator.GenerateExplosionSound();

        // 1. Create Ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(5, 1, 5); // Made text larger for 3rd person

        // 2. Create Player (Humanoid)
        GameObject player = CreateHumanoid("Player", Color.red);
        player.tag = "Player"; 
        player.transform.position = new Vector3(0, 0, 0); 

        player.AddComponent<PlayerController>();

        // Add Rigidbody
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.useGravity = false; // Still arcade style
        rb.isKinematic = true; 
        
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // 3. Create GameManager
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();

        // 4. Create Target Blocks (Blue Humanoids)
        int targetCount = 15;
        for (int i = 0; i < targetCount; i++)
        {
            GameObject target = CreateHumanoid("TargetBlock_" + i, Color.blue);
            
            // Random Position
            float x = Random.Range(-20f, 20f);
            float z = Random.Range(-20f, 20f);
            target.transform.position = new Vector3(x, 0, z);
            
            Debug.Log($"Created TargetBlock_{i} at {target.transform.position}");

            TargetBlock tb = target.AddComponent<TargetBlock>();
            
            // Note: CreateHumanoid adds colliders to children. 
            // We need a Trigger collider for the TargetBlock script to work effectively on the Parent,
            // OR the child colliders need to forward events.
            // Simpler: Add a big Trigger BoxCollider to the parent that covers the human.
            BoxCollider col = target.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0, 1.0f, 0);
            col.size = new Vector3(1f, 2f, 1f);
        }

        // 5. Setup Camera (Third Person)
        Camera cam = Camera.main;
        ThirdPersonCamera tpc = cam.gameObject.AddComponent<ThirdPersonCamera>();
        tpc.target = player.transform;
        
        Debug.Log("Game Setup Completed! Player (Capsule), Targets (Capsule), and Camera Setup.");
    }

    static GameObject CreateHumanoid(string name, Color color)
    {
        // Root Object
        GameObject root = new GameObject(name);

        // Materials
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;

        // 1. Body (Cube)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.parent = root.transform;
        body.transform.localPosition = new Vector3(0, 1.0f, 0);
        body.transform.localScale = new Vector3(0.8f, 1.0f, 0.4f);
        ApplyMaterial(body, mat);

        // 2. Head (Sphere)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.parent = root.transform;
        head.transform.localPosition = new Vector3(0, 1.8f, 0);
        head.transform.localScale = Vector3.one * 0.6f;
        ApplyMaterial(head, mat);

        // 3. Limbs (Cylinders)
        // Arms
        CreateLimb(root.transform, new Vector3(-0.6f, 1.0f, 0), new Vector3(0.3f, 0.8f, 0.3f), mat); // Left Arm
        CreateLimb(root.transform, new Vector3(0.6f, 1.0f, 0), new Vector3(0.3f, 0.8f, 0.3f), mat);  // Right Arm

        // Legs
        CreateLimb(root.transform, new Vector3(-0.3f, 0.0f, 0), new Vector3(0.3f, 1.0f, 0.3f), mat, 0.5f); // Left Leg
        CreateLimb(root.transform, new Vector3(0.3f, 0.0f, 0), new Vector3(0.3f, 1.0f, 0.3f), mat, 0.5f);  // Right Leg

        return root;
    }

    static void CreateLimb(Transform parent, Vector3 localPos, Vector3 scale, Material mat, float yOffset = 0f)
    {
        GameObject limb = GameObject.CreatePrimitive(PrimitiveType.Capsule); // Capsule is better for limbs
        limb.transform.parent = parent;
        // Adjust for pivot (Capsule pivot is center)
        limb.transform.localPosition = localPos + new Vector3(0, yOffset, 0); 
        limb.transform.localScale = scale;
        ApplyMaterial(limb, mat);
    }

    static void ApplyMaterial(GameObject obj, Material mat)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (r != null) r.material = mat;
    }

    static void DestroyIfExists(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null) Object.DestroyImmediate(obj);
    }

    [MenuItem("Agentic/Helpers/Create Player Character")]
    public static void CreatePlayerCharacter()
    {
        // 1. Cleanup
        DestroyIfExists("PlayerArmature");
        DestroyIfExists("PlayerCube");
        DestroyIfExists("Player"); // Cleanup potential old manual objects

        string prefabPath = "Assets/StarterAssets/ThirdPersonController/Prefabs/PlayerArmature.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject character = null;

        if (prefab != null)
        {
            // 2. Instantiate Prefab
            character = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            character.name = "PlayerArmature";
            character.transform.position = new Vector3(0, 0, 0); // Ground level usually 0 for this prefab
            Debug.Log($"Created 'PlayerArmature' from prefab: {prefabPath}");
        }
        else
        {
            // Fallback
            Debug.LogWarning($"Prefab not found at {prefabPath}. Creating placeholder Cube.");
            character = GameObject.CreatePrimitive(PrimitiveType.Cube);
            character.name = "PlayerCube";
            character.transform.position = new Vector3(0, 0.5f, 0);
        }

        // 3. Setup Tag & Camera
        if (character != null)
        {
            character.tag = "Player";

            Camera cam = Camera.main;
            if (cam != null)
            {
                ThirdPersonCamera tpc = cam.GetComponent<ThirdPersonCamera>();
                if (tpc == null) tpc = cam.gameObject.AddComponent<ThirdPersonCamera>();
                
                tpc.target = character.transform;
                Debug.Log("Assigned Player to ThirdPersonCamera target.");
            }
            else
            {
                Debug.LogError("Main Camera not found! Cannot assign camera target.");
            }

            // 4. Configure Dash (Speed)
            var controller = character.GetComponent<StarterAssets.ThirdPersonController>();
            if (controller != null)
            {
                controller.MoveSpeed = 4.0f;     // Faster walk (Default ~2)
                controller.SprintSpeed = 12.0f;  // Dash speed (Default ~5.3)
                Debug.Log("Player Controls Updated: Dash Speed set to 12.0f");
            }
        }
    }
    [MenuItem("Agentic/Helpers/Create Environment")]
    public static void CreateEnvironment()
    {
        // 1. Cleanup
        DestroyIfExists("Ground");
        
        // Cleanup old buildings
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Building_")) Object.DestroyImmediate(obj);
        }

        // Materials
        Material groundMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/StarterAssets/Environment/Art/Materials/GridWhite_01_Mat.mat");
        Material buildingMat1 = AssetDatabase.LoadAssetAtPath<Material>("Assets/StarterAssets/Environment/Art/Materials/GridBlue_01_Mat.mat");
        Material buildingMat2 = AssetDatabase.LoadAssetAtPath<Material>("Assets/StarterAssets/Environment/Art/Materials/GridOrange_01_Mat.mat");

        // Fallback if materials not found (though they should be there)
        if (groundMat == null) { groundMat = new Material(Shader.Find("Standard")); groundMat.color = Color.white; }
        if (buildingMat1 == null) { buildingMat1 = new Material(Shader.Find("Standard")); buildingMat1.color = Color.blue; }
        if (buildingMat2 == null) { buildingMat2 = new Material(Shader.Find("Standard")); buildingMat2.color = new Color(1, 0.5f, 0); }

        // 2. Create Ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(10, 1, 10); // 100x100 area
        ground.transform.position = Vector3.zero;
        
        Renderer gr = ground.GetComponent<Renderer>();
        if (gr != null) 
        {
            gr.material = groundMat;
            // Adjust Tiling (Plane primitives are 10x10 units, so scale 10 = 100 units. We want 1 tile per unit generally, or similar)
            gr.material.mainTextureScale = new Vector2(100, 100); 
        }

        // 3. Create Buildings
        int buildingCount = 25;

        for (int i = 0; i < buildingCount; i++)
        {
            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = "Building_" + i;
            
            // Random Position (Excluding Center)
            float x = 0, z = 0;
            do
            {
                x = Random.Range(-45f, 45f);
                z = Random.Range(-45f, 45f);
            } while (Mathf.Abs(x) < 8f && Mathf.Abs(z) < 8f); // Keep 8 units clear around 0,0

            float width = Random.Range(3f, 10f);
            float depth = Random.Range(3f, 10f);
            float height = Random.Range(5f, 25f);

            building.transform.position = new Vector3(x, height / 2, z);
            building.transform.localScale = new Vector3(width, height, depth);
            
            Renderer br = building.GetComponent<Renderer>();
            if (br != null) 
            {
                // Randomly pick blue or orange
                br.material = (Random.value > 0.5f) ? buildingMat1 : buildingMat2;
                
                // Adjust Tiling for Cubes
                // For walls (sides), we roughly want tiling to match width/depth and height
                // Since Unity Standard shader tiles globally, we can't easily tile perfectly per face without custom shader or UVs.
                // But setting it to match the largest scale usually looks okay-ish for prototypes.
                // BETTER: Just use world-space or tri-planar shader if available, but for now standard tiling:
                br.material.mainTextureScale = new Vector2(width + depth, height);
            }
        }

        Debug.Log($"Environment Created: Ground (100x100) and {buildingCount} Buildings with Grid materials.");
    }
}
