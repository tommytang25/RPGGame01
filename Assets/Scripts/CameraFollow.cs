using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    
    [Header("Follow Settings")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);
    
    [Header("Bounds")]
    public bool useBounds = false;
    public float minX, maxX, minY, maxY;
    
    [Header("Camera Shake")]
    public float shakeDuration = 0f;
    public float shakeAmount = 0.5f;
    public float decreaseFactor = 1.0f;
    private Vector3 originalPosition;
    
    private void Start()
    {
        if (target == null)
        {
            // Try to find the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        originalPosition = transform.position;
    }
    
    private void FixedUpdate()
    {
        if (target == null)
            return;
            
        // Get the desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Apply smoothing
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Apply bounds if needed
        if (useBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);
        }
        
        // Apply shake if active
        if (shakeDuration > 0)
        {
            transform.position = smoothedPosition + Random.insideUnitSphere * shakeAmount;
            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            transform.position = smoothedPosition;
        }
    }
    
    public void ShakeCamera(float duration, float amount = 0.5f)
    {
        originalPosition = transform.position;
        shakeDuration = duration;
        shakeAmount = amount;
    }
    
    // If we need to set bounds dynamically, e.g. when changing scenes
    public void SetBounds(float minXValue, float maxXValue, float minYValue, float maxYValue)
    {
        useBounds = true;
        minX = minXValue;
        maxX = maxXValue;
        minY = minYValue;
        maxY = maxYValue;
    }
} 