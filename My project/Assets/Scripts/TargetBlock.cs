using UnityEngine;

public class TargetBlock : MonoBehaviour
{
    public int scoreValue = 10;

    void OnTriggerEnter(Collider other)
    {
        // Handle Compound Colliders (Child objects hitting this trigger)
        GameObject hitObject = other.gameObject;
        if (other.attachedRigidbody != null)
        {
            hitObject = other.attachedRigidbody.gameObject;
        }

        // Check if the object we collided with is the player
        if (hitObject.CompareTag("Player") || hitObject.name == "Player")
        {
            // Add score
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            // Visual Effect: Spawn Debris
            SpawnDebris();

            // Audio Effect: Play Sound
            PlayExplosionSound();
            
            // Destroy this block
            Destroy(gameObject);
        }
    }

    void SpawnDebris()
    {
        int debrisCount = 10;
        for (int i = 0; i < debrisCount; i++)
        {
            GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debris.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
            debris.transform.localScale = Vector3.one * 0.3f;
            
            // Color same as this block (Blue)
            Renderer r = debris.GetComponent<Renderer>();
            if (r != null) r.material.color = Color.blue;

            Rigidbody rb = debris.AddComponent<Rigidbody>();
            rb.AddExplosionForce(500f, transform.position, 3f);

            Destroy(debris, 2.0f); // Cleanup debris after 2 seconds
        }
    }

    void PlayExplosionSound()
    {
        AudioClip clip = Resources.Load<AudioClip>("Explosion");
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
        else
        {
            Debug.LogWarning("Explosion sound not found in Resources. Run Agentic > Generate Audio to create it.");
        }
    }
}
