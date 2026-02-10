using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SimpleRPG;
using System.Linq;

public class SimpleRPGSceneSetup : EditorWindow
{
    [MenuItem("SimpleRPG/Create Demo Scene")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Error", "Cannot create scene while in Play Mode. Please stop the game first.", "OK");
            return;
        }

        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        Sprite LoadSprite(string path)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var sprite = assets.OfType<Sprite>().FirstOrDefault();
            if (sprite == null) Debug.LogWarning($"Sprite not found at {path}");
            return sprite;
        }

        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<SimpleRPGGameManager>();

        // Environment
        GameObject ground = new GameObject("Ground");
        SpriteRenderer groundSr = ground.AddComponent<SpriteRenderer>();
        Sprite groundSprite = LoadSprite("Assets/Brackeys/2D Mega Pack/Environment/Gold Miner/Ground.png");
        groundSr.sprite = groundSprite;
        groundSr.sortingOrder = 0; 
        
        float groundHalfHeight = groundSprite != null ? groundSprite.bounds.extents.y : 1f;
        ground.transform.position = new Vector3(0, -2f - groundHalfHeight, 0); 
        ground.transform.localScale = new Vector3(100f, 1f, 1f); 
        BoxCollider2D groundCol = ground.AddComponent<BoxCollider2D>();
        ground.layer = LayerMask.NameToLayer("Default");

        // Player
        GameObject player = new GameObject("Player");
        player.name = "Player";
        player.tag = "Player"; // IMPORTANT: Fix for Enemy collision check
        
        SpriteRenderer playerSr = player.AddComponent<SpriteRenderer>();
        Sprite playerSprite = LoadSprite("Assets/Brackeys/2D Mega Pack/Characters/Astronaut.png"); 
        playerSr.sprite = playerSprite;
        playerSr.sortingOrder = 10; 

        float playerHalfHeight = playerSprite != null ? playerSprite.bounds.extents.y : 1f;
        player.transform.position = new Vector3(-5, -2f + playerHalfHeight + 0.1f, 0); 
        
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 
        rb.gravityScale = 3f; 
        
        player.AddComponent<CapsuleCollider2D>();
        SimpleRPGPlayer playerScript = player.AddComponent<SimpleRPGPlayer>();
        playerScript.groundLayer = LayerMask.GetMask("Default");
        
        // Camera
        GameObject cameraObj = new GameObject("Main Camera");
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.53f, 0.81f, 0.98f); // Sky Blue (Fallback)
        cam.orthographic = true;
        cam.orthographicSize = 8f; 
        cameraObj.tag = "MainCamera";
        
        cameraObj.transform.SetParent(player.transform);
        cameraObj.transform.localPosition = new Vector3(0, 2, -10);

        // Background (Sky Sprite)
        GameObject bgSky = new GameObject("SkyBackground");
        SpriteRenderer bgSkySr = bgSky.AddComponent<SpriteRenderer>();
        Sprite skySprite = LoadSprite("Assets/Brackeys/2D Mega Pack/Backgrounds/SkyBackground.png");
        if (skySprite != null)
        {
            bgSkySr.sprite = skySprite;
            // Scale to cover checking typical 16:9 aspect ratio at size 8
            // Size 8 orthographic means height is 16 units. Width is ~28. 
            // Sprite is likely small/pixel art or large. Let's set a safe large scale.
            bgSky.transform.localScale = new Vector3(2f, 2f, 1f); 
        }
        else
        {
             Debug.LogWarning("SkyBackground sprite not found!");
        }
        bgSky.transform.SetParent(cameraObj.transform);
        bgSky.transform.localPosition = new Vector3(0, 0, 10); // Behind everything relative to camera
        
        // Poop Asset Assignment - Removed from Player as per new pivot
        // Sprite poopSprite = LoadSprite("Assets/Brackeys/2D Mega Pack/Items & Icons/Pixel Art/Rock.png");
        // if (poopSprite != null) playerScript.poopSprite = poopSprite;

        // Enemy (Cannon Enemy) - With Turret Logic
        // Enemies (Cannon Enemy) - With Turret Logic
        for (int i = 0; i < 3; i++)
        {
            GameObject enemy = new GameObject($"Cannon Enemy {i}");
            // Root components
            Rigidbody2D enemyRb = enemy.AddComponent<Rigidbody2D>();
            enemyRb.bodyType = RigidbodyType2D.Kinematic; 
            enemy.AddComponent<BoxCollider2D>();

            // Turret (Child)
            GameObject turretObj = new GameObject("Turret");
            turretObj.transform.SetParent(enemy.transform);
            turretObj.transform.localPosition = Vector3.zero;
            
            SpriteRenderer turretSr = turretObj.AddComponent<SpriteRenderer>();
            Sprite cannonSprite = LoadSprite("Assets/Brackeys/2D Mega Pack/Enemies/Canon.png");
            turretSr.sprite = cannonSprite;
            turretSr.sortingOrder = 5; 
            turretSr.color = Color.white; 

            // Use Rock.png (Poop) for Projectile
            Sprite poopRockSprite = LoadSprite("Assets/Brackeys/2D Mega Pack/Items & Icons/Pixel Art/Rock.png");

            // Position: 5, 13, 21, 29, 37, 45 (Spaced out along the path)
            float xPos = 5 + (i * 8);
            enemy.transform.position = new Vector3(xPos, -2f + (cannonSprite != null ? cannonSprite.bounds.extents.y : 0.5f), 0);
            
            SimpleRPGEnemy enemyScript = enemy.AddComponent<SimpleRPGEnemy>();
            enemyScript.damage = 10; 
            enemyScript.moveSpeed = 0f; 
            enemyScript.projectileSprite = poopRockSprite; 
            enemyScript.turret = turretObj.transform; // Assign Rotating Part 
        } 

        // UI Setup
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Health Bar (Slider)
        GameObject sliderObj = new GameObject("HealthBar");
        sliderObj.transform.SetParent(canvasObj.transform, false);
        Slider slider = sliderObj.AddComponent<Slider>();
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 1); // Top Left
        sliderRect.anchorMax = new Vector2(0, 1);
        sliderRect.pivot = new Vector2(0, 1);
        sliderRect.anchoredPosition = new Vector2(20, -20);
        sliderRect.sizeDelta = new Vector2(200, 20);

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.gray;
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, 0); // Padding

        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        slider.targetGraphic = bgImage;
        slider.fillRect = fillRect;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;

        // Attach UI Script
        SimpleRPGUI uiScript = sliderObj.AddComponent<SimpleRPGUI>();
        uiScript.healthSlider = slider;
        uiScript.player = playerScript;

        Debug.Log("Simple RPG Demo Scene Created! Please save the scene.");
    }
}
