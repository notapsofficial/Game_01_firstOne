using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f;
    public Vector3 offset = new Vector3(0, 2, -10);
    public float orthographicSize = 5f;

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
        }
        
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Only follow X and Y
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, 0) + offset;
            
            // Keep Z constant at offset.z
            targetPosition.z = offset.z; 

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}
