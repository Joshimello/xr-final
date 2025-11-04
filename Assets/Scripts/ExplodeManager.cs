using UnityEngine;

public class ExplodeManager : MonoBehaviour
{
    [Header("Explosion Settings")]
    public GameObject explosionPrefab;
    public GameObject[] explosionPoints;

    [Header("Effect Settings")]
    public float shakeMagnitude = 0.5f;
    public float shakeDuration = 0.5f;
    public float destroyDelay = 5f;

    [Header("Periodic Explosion Settings")]
    public bool periodicExplosions = false;
    public float explosionIntervalMin = 3f;
    public float explosionIntervalMax = 10f;

    [Header("Component References")]
    public VignetteShaker vignetteShaker;
    public CameraShaker cameraShaker;

    private float nextExplosionTime;

    void Start()
    {
        if (periodicExplosions)
        {
            nextExplosionTime = Time.time + Random.Range(explosionIntervalMin, explosionIntervalMax);
        }
    }

    void Update()
    {
        if (periodicExplosions && Time.time >= nextExplosionTime)
        {
            SpawnExplosionRandom();
            nextExplosionTime = Time.time + Random.Range(explosionIntervalMin, explosionIntervalMax);
        }
    }

    public void SpawnExplosion(int index)
    {
        if (explosionPrefab == null)
        {
            Debug.LogError("Explosion prefab is not assigned!");
            return;
        }

        if (explosionPoints == null || explosionPoints.Length == 0)
        {
            Debug.LogError("No explosion points assigned!");
            return;
        }

        if (index < 0 || index >= explosionPoints.Length)
        {
            Debug.LogError($"Invalid explosion point index: {index}. Valid range: 0 to {explosionPoints.Length - 1}");
            return;
        }

        if (explosionPoints[index] == null)
        {
            Debug.LogError($"Explosion point at index {index} is null!");
            return;
        }

        Vector3 position = explosionPoints[index].transform.position;

        if (vignetteShaker != null)
            vignetteShaker.StartShake(shakeDuration, shakeMagnitude);

        if (cameraShaker != null)
            cameraShaker.StartShake();

        GameObject explosionInstance = Instantiate(explosionPrefab, position, Quaternion.identity);
        Destroy(explosionInstance, destroyDelay);
    }

    public void SpawnExplosionRandom()
    {
        if (explosionPoints == null || explosionPoints.Length == 0)
        {
            Debug.LogError("No explosion points assigned for random spawning!");
            return;
        }

        int randomIndex = Random.Range(0, explosionPoints.Length);
        SpawnExplosion(randomIndex);
    }
}
