using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class VignetteShaker : MonoBehaviour
{
    [Header("References")]
    public Volume postProcessVolume;

    [Header("Shake Settings")]
    public float shakeDuration = 2f;
    public float initialIntensity = 1f;
    public float shakeSpeed = 10f;
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Vignette Shake Range")]
    public float maxOffset = 0.5f;

    [Header("Vignette Intensity")]
    public float vignetteIntensity = 0.5f;

    private Vignette vignette;
    private Vector2 originalCenter;
    private float originalIntensity;
    private bool isShaking = false;
    private float noiseOffsetX;
    private float noiseOffsetY;

    void Start()
    {
        // Get random noise offsets to make each shake unique
        noiseOffsetX = Random.Range(0f, 1000f);
        noiseOffsetY = Random.Range(0f, 1000f);

        // Try to find post process volume if not assigned
        if (postProcessVolume == null)
        {
            postProcessVolume = FindObjectOfType<Volume>();
        }

        // Get the vignette component
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            if (postProcessVolume.profile.TryGet<Vignette>(out vignette))
            {
                originalCenter = vignette.center.value;
                originalIntensity = vignette.intensity.value;
            }
            else
            {
                Debug.LogError("VignetteShaker: No Vignette found in the post-process profile!");
            }
        }
        else
        {
            Debug.LogError("VignetteShaker: No post-process volume or profile assigned!");
        }
    }

    public void StartShake()
    {
        if (vignette != null && !isShaking)
        {
            StartCoroutine(ShakeVignette());
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

    private IEnumerator ShakeVignette()
    {
        isShaking = true;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float normalizedTime = elapsedTime / shakeDuration;
            float currentIntensity = initialIntensity * intensityCurve.Evaluate(normalizedTime);

            // Generate Perlin noise for smooth random movement
            float noiseX = Mathf.PerlinNoise((Time.time * shakeSpeed) + noiseOffsetX, 0f);
            float noiseY = Mathf.PerlinNoise(0f, (Time.time * shakeSpeed) + noiseOffsetY);

            // Convert noise from 0-1 to -1 to 1 range
            noiseX = (noiseX - 0.5f) * 2f;
            noiseY = (noiseY - 0.5f) * 2f;

            // Apply intensity and max offset
            Vector2 shakeOffset = new Vector2(
                noiseX * currentIntensity * maxOffset,
                noiseY * currentIntensity * maxOffset
            );

            // Apply the shake to vignette center
            vignette.center.value = originalCenter + shakeOffset;

            // Apply intensity animation (stronger at the beginning, fading back to original)
            float targetIntensity = originalIntensity + (vignetteIntensity * currentIntensity);
            vignette.intensity.value = targetIntensity;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset to original position and intensity
        vignette.center.value = originalCenter;
        vignette.intensity.value = originalIntensity;
        isShaking = false;
    }

    public void StopShake()
    {
        if (isShaking)
        {
            StopAllCoroutines();
            if (vignette != null)
            {
                vignette.center.value = originalCenter;
                vignette.intensity.value = originalIntensity;
            }
            isShaking = false;
        }
    }

    void OnDestroy()
    {
        // Ensure vignette is reset when object is destroyed
        if (vignette != null)
        {
            vignette.center.value = originalCenter;
            vignette.intensity.value = originalIntensity;
        }
    }

    public bool IsShaking => isShaking;
}
