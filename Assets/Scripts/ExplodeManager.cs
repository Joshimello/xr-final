using UnityEngine;

public class ExplodeManager : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float shakeMagnitude = 0.5f;
    public float shakeDuration = 0.5f;
    public float destroyDelay = 5f;
    public VignetteShaker vignetteShaker;
    public CameraShaker cameraShaker;

    public void SpawnExplosion()
    {
        if (explosionPrefab == null)
        {
            Debug.LogError("Explosion prefab is not assigned!");
            return;
        }

        Vector3 position = transform.position;
        vignetteShaker.StartShake(shakeDuration, shakeMagnitude);
        cameraShaker.StartShake();
        GameObject explosionInstance = Instantiate(explosionPrefab, position, Quaternion.identity);
        Destroy(explosionInstance, destroyDelay);
    }
}
