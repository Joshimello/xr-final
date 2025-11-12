using UnityEngine;
using UnityEngine.Events;

public class FunctionCallQuest : Quest
{
    [Header("Function Call Settings")]
    [SerializeField] private string functionCallDescription = "Complete the required action";
    [SerializeField] private bool allowMultipleCalls = false;
    [SerializeField] private int requiredCallCount = 1;

    [Header("Optional Parameters")]
    [SerializeField] private bool requireSpecificCaller = false;
    [SerializeField] private GameObject requiredCaller;
    [SerializeField] private string requiredTag = "Player";

    [Header("Events")]
    public UnityEvent OnFunctionCalled;
    public UnityEvent<GameObject> OnFunctionCalledWithCaller;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField, ReadOnly, Tooltip("Number of times the function has been called")]
    private int currentCallCount = 0;
    [SerializeField, ReadOnly, Tooltip("Last caller that triggered the function")]
    private string lastCallerName = "None";

    // Custom ReadOnly attribute for inspector
    public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            UnityEditor.EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif

    protected override void Start()
    {
        base.Start();

        // Initialize call count
        currentCallCount = 0;
        lastCallerName = "None";

        // Update quest description if not manually set
        if (string.IsNullOrEmpty(questDescription))
        {
            questDescription = functionCallDescription;
        }

        LogDebug($"FunctionCallQuest '{questName}': Initialized. Required calls: {requiredCallCount}, Allow multiple: {allowMultipleCalls}");
    }

    protected override bool CheckQuestCompletion()
    {
        // This quest completes when the function is called, not through continuous checking
        return currentCallCount >= requiredCallCount;
    }

    /// <summary>
    /// Main function to be called by external modules to complete/progress the quest
    /// </summary>
    public void CallFunction()
    {
        CallFunction(null);
    }

    /// <summary>
    /// Function to be called with a specific caller object
    /// </summary>
    /// <param name="caller">The GameObject that is calling this function</param>
    public void CallFunction(GameObject caller)
    {
        if (!IsActive)
        {
            LogDebug($"FunctionCallQuest '{questName}': Function called but quest is not active");
            return;
        }

        if (IsCompleted && !allowMultipleCalls)
        {
            LogDebug($"FunctionCallQuest '{questName}': Function called but quest is already completed");
            return;
        }

        // Check if specific caller is required
        if (requireSpecificCaller)
        {
            if (requiredCaller != null && caller != requiredCaller)
            {
                LogDebug($"FunctionCallQuest '{questName}': Wrong caller. Expected: {requiredCaller.name}, Got: {caller?.name ?? "null"}");
                return;
            }

            if (!string.IsNullOrEmpty(requiredTag) && caller != null && !caller.CompareTag(requiredTag))
            {
                LogDebug($"FunctionCallQuest '{questName}': Wrong caller tag. Expected: {requiredTag}, Got: {caller.tag}");
                return;
            }
        }

        // Valid function call
        currentCallCount++;
        lastCallerName = caller?.name ?? "Unknown";

        LogDebug($"FunctionCallQuest '{questName}': Function called by {lastCallerName}. Call count: {currentCallCount}/{requiredCallCount}");

        // Trigger events
        OnFunctionCalled?.Invoke();
        OnFunctionCalledWithCaller?.Invoke(caller);

        // Check if quest should complete
        if (currentCallCount >= requiredCallCount)
        {
            // Quest will be completed automatically by the base class Update method
            LogDebug($"FunctionCallQuest '{questName}': Required call count reached, quest will complete");
        }
    }

    /// <summary>
    /// Alternative function names for better semantics
    /// </summary>
    public void TriggerQuest() => CallFunction();
    public void TriggerQuest(GameObject caller) => CallFunction(caller);
    public void CompleteAction() => CallFunction();
    public void CompleteAction(GameObject caller) => CallFunction(caller);

    protected override void OnQuestStart()
    {
        base.OnQuestStart();
        LogDebug($"Quest Started: {questName} - {functionCallDescription}");
    }

    protected override void OnQuestComplete()
    {
        base.OnQuestComplete();
        LogDebug($"Quest Completed: {questName} - Function was called {currentCallCount} times. Final caller: {lastCallerName}");
    }

    public override float GetQuestProgress()
    {
        if (IsCompleted) return 1f;
        if (requiredCallCount == 0) return 0f;

        return (float)currentCallCount / requiredCallCount;
    }

    public override void ResetQuest()
    {
        base.ResetQuest();
        currentCallCount = 0;
        lastCallerName = "None";
        LogDebug($"FunctionCallQuest '{questName}': Quest reset");
    }

    // Public getter methods for external access
    public int GetCurrentCallCount() => currentCallCount;
    public int GetRequiredCallCount() => requiredCallCount;
    public string GetLastCallerName() => lastCallerName;
    public bool IsCallValid(GameObject caller)
    {
        if (!requireSpecificCaller) return true;

        if (requiredCaller != null && caller != requiredCaller) return false;
        if (!string.IsNullOrEmpty(requiredTag) && caller != null && !caller.CompareTag(requiredTag)) return false;

        return true;
    }

    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[FunctionCallQuest] {message}");
        }
    }

    // Context menu helpers for testing in editor
#if UNITY_EDITOR
    [ContextMenu("Test Function Call")]
    private void EditorTestFunctionCall()
    {
        CallFunction(GameObject.FindWithTag("Player"));
    }

    [ContextMenu("Reset Call Count")]
    private void EditorResetCallCount()
    {
        currentCallCount = 0;
        lastCallerName = "None";
        Debug.Log($"FunctionCallQuest '{questName}': Call count reset via editor");
    }
#endif
}
