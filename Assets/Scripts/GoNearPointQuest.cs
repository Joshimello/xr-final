using UnityEngine;

public class GoNearPointQuest : Quest
{
    [Header("Go Near Point Settings")]
    [SerializeField] private GameObject targetPoint;
    [SerializeField] private GameObject objectToTest;
    [SerializeField] private float requiredDistance = 3f;
    [SerializeField] private bool usePlayerAsTestObject = true;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.yellow;
    [Space(5)]
    [SerializeField, ReadOnly, Tooltip("Real-time distance between test object and target point")]
    private float debugCurrentDistance = 0f;

    private Transform testTransform;
    private Transform targetTransform;
    private float lastDebugLogTime = 0f;

    // Custom ReadOnly attribute for inspector
    public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;

            // Custom label for debug distance
            if (property.name == "debugCurrentDistance")
            {
                float distance = property.floatValue;
                string displayText = distance < 0f ? "N/A (Invalid Objects)" : $"{distance:F2}m";
                label.text = $"Current Distance: {displayText}";
            }

            UnityEditor.EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif

    protected override void Update()
    {
        base.Update();
        UpdateDebugDistance();
    }

    private void UpdateDebugDistance()
    {
        if (testTransform == null || targetTransform == null)
        {
            debugCurrentDistance = -1f; // Use -1 to indicate invalid state
        }
        else
        {
            debugCurrentDistance = Vector3.Distance(testTransform.position, targetTransform.position);
        }
    }

    protected override void Start()
    {
        base.Start();

        // If no test object is specified and usePlayerAsTestObject is true, find the player
        if (objectToTest == null && usePlayerAsTestObject)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                // Try to find main camera if no player tag found
                player = Camera.main?.gameObject;
            }
            objectToTest = player;
        }

        // Cache transforms for performance
        if (objectToTest != null)
            testTransform = objectToTest.transform;

        if (targetPoint != null)
            targetTransform = targetPoint.transform;

        // Validate setup
        if (targetPoint == null)
        {
            Debug.LogError($"GoNearPointQuest '{questName}': Target Point is not assigned!");
        }

        if (objectToTest == null)
        {
            Debug.LogError($"GoNearPointQuest '{questName}': Object To Test is not assigned and no Player found!");
        }
        else
        {
            Debug.Log($"GoNearPointQuest '{questName}': Setup complete. Target: {targetPoint?.name}, Test Object: {objectToTest?.name}, Required Distance: {requiredDistance}m");
        }
    }

    protected override bool CheckQuestCompletion()
    {
        if (testTransform == null || targetTransform == null)
        {
            Debug.LogWarning($"GoNearPointQuest '{questName}': Missing transform references! testTransform: {testTransform}, targetTransform: {targetTransform}");
            return false;
        }

        float currentDistance = Vector3.Distance(testTransform.position, targetTransform.position);
        bool withinRange = currentDistance <= requiredDistance;

        // Debug logging for quest completion checking (limited to once per second)
        if (IsActive && !IsCompleted && Time.time - lastDebugLogTime >= 1f)
        {
            Debug.Log($"GoNearPointQuest '{questName}': Distance check - Current: {currentDistance:F2}m, Required: {requiredDistance:F2}m, Within range: {withinRange}");
            lastDebugLogTime = Time.time;
        }

        return withinRange;
    }

    protected override void OnQuestStart()
    {
        base.OnQuestStart();
        Debug.Log($"Quest Started: {questName} - Go within {requiredDistance}m of {targetPoint?.name}");

        // Additional debug info at quest start
        if (testTransform != null && targetTransform != null)
        {
            float startDistance = Vector3.Distance(testTransform.position, targetTransform.position);
            Debug.Log($"GoNearPointQuest '{questName}': Starting distance is {startDistance:F2}m");
        }
    }

    protected override void OnQuestComplete()
    {
        base.OnQuestComplete();
        float finalDistance = testTransform != null && targetTransform != null ?
            Vector3.Distance(testTransform.position, targetTransform.position) : 0f;
        Debug.Log($"Quest Completed: {questName} - Reached target point! Final distance: {finalDistance:F2}m");
    }

    public override float GetQuestProgress()
    {
        if (IsCompleted) return 1f;
        if (testTransform == null || targetTransform == null) return 0f;

        float currentDistance = debugCurrentDistance != 0f ? debugCurrentDistance : Vector3.Distance(testTransform.position, targetTransform.position);
        float progress = Mathf.Clamp01(1f - (currentDistance / (requiredDistance * 2f)));
        return progress;
    }

    // Get current distance for UI display
    public float GetCurrentDistance()
    {
        return debugCurrentDistance >= 0f ? debugCurrentDistance : float.MaxValue;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || targetPoint == null) return;

        Gizmos.color = gizmoColor;

        // Draw target point
        Gizmos.DrawWireSphere(targetPoint.transform.position, requiredDistance);

        // Draw line to test object if available
        if (objectToTest != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(targetPoint.transform.position, objectToTest.transform.position);

            // Show current distance in scene view
            float currentDistance = Vector3.Distance(objectToTest.transform.position, targetPoint.transform.position);
            Vector3 midPoint = (targetPoint.transform.position + objectToTest.transform.position) / 2f;

#if UNITY_EDITOR
            UnityEditor.Handles.Label(midPoint, $"Distance: {currentDistance:F1}m");
#endif
        }
    }

    void OnDrawGizmosSelected()
    {
        OnDrawGizmos();
    }
}
