using UnityEngine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    [Header("References")]
    public Camera targetCamera;
    public Transform cameraTransform;

    [Header("Dolly Zoom Settings")]
    public float shakeDuration = 2f;
    public float initialIntensity = 1f;
    public float shakeSpeed = 8f;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Field of View Range")]
    public float fovVariation = 15f;
    public float minFOV = 40f;
    public float maxFOV = 100f;

    [Header("Distance Compensation")]
    public bool compensateDistance = true;
    public float distanceVariation = 2f;

    private float originalFOV;
    private Vector3 originalPosition;
    private bool isShaking = false;
    private float noiseOffsetFOV;
    private float noiseOffsetDistance;

    void Start()
    {
        // Try to find camera if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }
        }

        // Get camera transform if not assigned
        if (cameraTransform == null && targetCamera != null)
        {
            cameraTransform = targetCamera.transform;
        }

        if (targetCamera != null)
        {
            originalFOV = targetCamera.fieldOfView;
            originalPosition = cameraTransform.position;

            // Clamp original FOV to our limits
            originalFOV = Mathf.Clamp(originalFOV, minFOV, maxFOV);
        }
        else
        {
            Debug.LogError("CameraShaker: No camera found!");
        }

        // Get random noise offsets for unique shake patterns
        noiseOffsetFOV = Random.Range(0f, 1000f);
        noiseOffsetDistance = Random.Range(0f, 1000f);
    }

    public void StartShake()
    {
        if (targetCamera != null && !isShaking)
        {
            StartCoroutine(DollyZoomShake());
        }
    }

    public void StartShake(float duration)
    {
        shakeDuration = duration;
        StartShake();
    }

    public void StartShake(float duration, float intensity)
    {
        shakeDuration = duration;
        initialIntensity = intensity;
        StartShake();
    }

    public void StartShake(float duration, float intensity, float fovVar)
    {
        shakeDuration = duration;
        initialIntensity = intensity;
        fovVariation = fovVar;
        StartShake();
    }

    private IEnumerator DollyZoomShake()
    {
        isShaking = true;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float normalizedTime = elapsedTime / shakeDuration;
            float currentIntensity = initialIntensity * intensityCurve.Evaluate(normalizedTime);

            // Generate Perlin noise for smooth FOV changes
            float noiseFOV = Mathf.PerlinNoise((Time.time * shakeSpeed) + noiseOffsetFOV, 0f);

            // Convert noise from 0-1 to -1 to 1 range
            noiseFOV = (noiseFOV - 0.5f) * 2f;

            // Calculate FOV change
            float fovDelta = noiseFOV * currentIntensity * fovVariation;
            float targetFOV = Mathf.Clamp(originalFOV + fovDelta, minFOV, maxFOV);

            // Apply FOV change
            targetCamera.fieldOfView = targetFOV;

            // Compensate camera distance to maintain relative object sizes (dolly zoom effect)
            if (compensateDistance && cameraTransform != null)
            {
                // Generate separate noise for distance to avoid perfect correlation
                float noiseDistance = Mathf.PerlinNoise(0f, (Time.time * shakeSpeed * 0.7f) + noiseOffsetDistance);
                noiseDistance = (noiseDistance - 0.5f) * 2f;

                // Calculate distance compensation based on FOV change
                float fovRatio = targetFOV / originalFOV;
                float distanceCompensation = (1f - fovRatio) * distanceVariation;

                // Add some noise to the distance compensation for more organic feel
                distanceCompensation += noiseDistance * currentIntensity * (distanceVariation * 0.3f);

                // Apply distance change along the forward direction
                Vector3 targetPosition = originalPosition + (cameraTransform.forward * distanceCompensation);
                cameraTransform.position = targetPosition;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset to original values
        targetCamera.fieldOfView = originalFOV;
        if (compensateDistance && cameraTransform != null)
        {
            cameraTransform.position = originalPosition;
        }

        isShaking = false;
    }

    public void StopShake()
    {
        if (isShaking)
        {
            StopAllCoroutines();
            if (targetCamera != null)
            {
                targetCamera.fieldOfView = originalFOV;
            }
            if (compensateDistance && cameraTransform != null)
            {
                cameraTransform.position = originalPosition;
            }
            isShaking = false;
        }
    }

    void OnDestroy()
    {
        // Ensure camera is reset when object is destroyed
        if (targetCamera != null)
        {
            targetCamera.fieldOfView = originalFOV;
        }
        if (compensateDistance && cameraTransform != null)
        {
            cameraTransform.position = originalPosition;
        }
    }

    // Helper method to test the effect in editor
    [ContextMenu("Test Shake")]
    private void TestShake()
    {
        if (Application.isPlaying)
        {
            StartShake();
        }
    }

    public bool IsShaking => isShaking;

    // Public properties for runtime adjustment
    public float CurrentFOV => targetCamera != null ? targetCamera.fieldOfView : 0f;
    public float OriginalFOV => originalFOV;
}
