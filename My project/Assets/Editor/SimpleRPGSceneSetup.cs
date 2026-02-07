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
        ground.transform.localScale = Vector3.one; 
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
        cam.backgroundColor = new Color(0.2f, 0.2f, 0.2f); 
        cam.orthographic = true;
        cam.orthographicSize = 8f; 
        cameraObj.tag = "MainCamera";
        
        cameraObj.transform.SetParent(player.transform);
        cameraObj.transform.localPosition = new Vector3(0, 2, -10);

        // Enemy (Red Stone)
        GameObject enemy = new GameObject("Deadly Red Stone");
        SpriteRenderer enemySr = enemy.AddComponent<SpriteRenderer>();
        Sprite enemySprite = LoadSprite("Assets/Brackeys/2D Mega Pack/Enemies/SpikyBall.png");
        enemySr.sprite = enemySprite;
        enemySr.sortingOrder = 5; 
        enemySr.color = Color.red; 

        enemy.transform.position = new Vector3(5, -2f + (enemySprite != null ? enemySprite.bounds.extents.y : 1f), 0);
        Rigidbody2D enemyRb = enemy.AddComponent<Rigidbody2D>();
        enemyRb.bodyType = RigidbodyType2D.Kinematic; 
        enemy.AddComponent<BoxCollider2D>();
        
        SimpleRPGEnemy enemyScript = enemy.AddComponent<SimpleRPGEnemy>();
        enemyScript.damage = 20; 
        enemyScript.moveSpeed = 0f; 

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
