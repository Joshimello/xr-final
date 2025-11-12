using UnityEngine;
using System.Collections.Generic;

public class CollectObjectsQuest : Quest
{
    [Header("Collect Objects Settings")]
    [SerializeField] private GameObject[] objectsToCollect;
    [SerializeField] private int requiredCollectionCount = 1;
    [SerializeField] private string collectibleTag = "Collectible";
    [SerializeField] private bool useSpecificObjects = true;
    [SerializeField] private bool destroyOnCollect = true;

    [Header("Collection Detection")]
    [SerializeField] private float collectionRange = 2f;
    [SerializeField] private GameObject collector;
    [SerializeField] private bool usePlayerAsCollector = true;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.green;

    private List<GameObject> collectedObjects = new List<GameObject>();
    private Transform collectorTransform;
    private int currentCollectionCount = 0;

    protected override void Start()
    {
        base.Start();

        // Find collector object
        if (collector == null && usePlayerAsCollector)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                player = Camera.main?.gameObject;
            }
            collector = player;
        }

        if (collector != null)
            collectorTransform = collector.transform;

        // Get audio source if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Initialize collection list
        collectedObjects.Clear();
        currentCollectionCount = 0;

        // Validate setup
        if (useSpecificObjects && (objectsToCollect == null || objectsToCollect.Length == 0))
        {
            Debug.LogError($"CollectObjectsQuest '{questName}': No specific objects assigned to collect!");
        }

        if (collector == null)
        {
            Debug.LogError($"CollectObjectsQuest '{questName}': No collector object found!");
        }
    }

    protected override bool CheckQuestCompletion()
    {
        if (collectorTransform == null) return false;

        // Check for new collectibles in range
        if (useSpecificObjects)
        {
            CheckSpecificObjects();
        }
        else
        {
            CheckObjectsByTag();
        }

        return currentCollectionCount >= requiredCollectionCount;
    }

    private void CheckSpecificObjects()
    {
        if (objectsToCollect == null) return;

        foreach (GameObject obj in objectsToCollect)
        {
            if (obj == null || collectedObjects.Contains(obj)) continue;

            float distance = Vector3.Distance(collectorTransform.position, obj.transform.position);
            if (distance <= collectionRange)
            {
                CollectObject(obj);
            }
        }
    }

    private void CheckObjectsByTag()
    {
        GameObject[] collectibles = GameObject.FindGameObjectsWithTag(collectibleTag);

        foreach (GameObject obj in collectibles)
        {
            if (obj == null || collectedObjects.Contains(obj)) continue;

            float distance = Vector3.Distance(collectorTransform.position, obj.transform.position);
            if (distance <= collectionRange)
            {
                CollectObject(obj);
            }
        }
    }

    private void CollectObject(GameObject obj)
    {
        if (obj == null || collectedObjects.Contains(obj)) return;

        collectedObjects.Add(obj);
        currentCollectionCount++;

        // Play collection sound
        if (collectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        // Visual feedback
        Debug.Log($"Collected: {obj.name} ({currentCollectionCount}/{requiredCollectionCount})");

        // Optional: Add particle effect here
        CreateCollectionEffect(obj.transform.position);

        // Handle object after collection
        if (destroyOnCollect)
        {
            Destroy(obj);
        }
        else
        {
            // Disable the object instead of destroying it
            obj.SetActive(false);
        }
    }

    private void CreateCollectionEffect(Vector3 position)
    {
        // Optional: Create particle effect or other visual feedback
        // You can implement this with particle systems, UI animations, etc.
    }

    protected override void OnQuestStart()
    {
        base.OnQuestStart();
        Debug.Log($"Quest Started: {questName} - Collect {requiredCollectionCount} objects");
    }

    protected override void OnQuestComplete()
    {
        base.OnQuestComplete();
        Debug.Log($"Quest Completed: {questName} - All objects collected!");
    }

    public override float GetQuestProgress()
    {
        if (IsCompleted) return 1f;
        if (requiredCollectionCount == 0) return 0f;

        return (float)currentCollectionCount / requiredCollectionCount;
    }

    public override void ResetQuest()
    {
        base.ResetQuest();

        // Reset collection state
        collectedObjects.Clear();
        currentCollectionCount = 0;

        // Re-enable any objects that were disabled
        if (!destroyOnCollect)
        {
            if (useSpecificObjects && objectsToCollect != null)
            {
                foreach (GameObject obj in objectsToCollect)
                {
                    if (obj != null)
                        obj.SetActive(true);
                }
            }
            else
            {
                GameObject[] collectibles = GameObject.FindGameObjectsWithTag(collectibleTag);
                foreach (GameObject obj in collectibles)
                {
                    if (obj != null)
                        obj.SetActive(true);
                }
            }
        }
    }

    // Public methods for external access
    public int GetCurrentCollectionCount() => currentCollectionCount;
    public int GetRequiredCollectionCount() => requiredCollectionCount;
    public List<GameObject> GetCollectedObjects() => new List<GameObject>(collectedObjects);

    void OnDrawGizmos()
    {
        if (!showGizmos || collectorTransform == null) return;

        Gizmos.color = gizmoColor;

        // Draw collection range around collector
        Gizmos.DrawWireSphere(collectorTransform.position, collectionRange);

        // Draw lines to uncollected objects
        if (useSpecificObjects && objectsToCollect != null)
        {
            foreach (GameObject obj in objectsToCollect)
            {
                if (obj != null && !collectedObjects.Contains(obj))
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(collectorTransform.position, obj.transform.position);

                    float distance = Vector3.Distance(collectorTransform.position, obj.transform.position);
                    bool inRange = distance <= collectionRange;

                    Gizmos.color = inRange ? Color.green : Color.red;
                    Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.5f);
                }
            }
        }

#if UNITY_EDITOR
        // Show collection info in scene view
        Vector3 labelPos = collectorTransform.position + Vector3.up * 2f;
        UnityEditor.Handles.Label(labelPos, $"Collected: {currentCollectionCount}/{requiredCollectionCount}");
#endif
    }

    void OnDrawGizmosSelected()
    {
        OnDrawGizmos();
    }
}
