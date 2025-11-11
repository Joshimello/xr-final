using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public abstract class Quest : MonoBehaviour
{
    [Header("Quest Info")]
    public string questName = "New Quest";
    public string questDescription = "Complete this quest";

    [Header("Quest State")]
    [SerializeField] private bool isCompleted = false;
    [SerializeField] private bool isActive = false;

    [Header("Events")]
    public UnityEvent OnQuestCompleted;
    public UnityEvent OnQuestStarted;

    public bool IsCompleted => isCompleted;
    public bool IsActive => isActive;

    protected virtual void Start()
    {
        // Initialize quest-specific logic
    }

    protected virtual void Update()
    {
        if (isActive && !isCompleted)
        {
            if (CheckQuestCompletion())
            {
                CompleteQuest();
            }
        }
    }

    /// <summary>
    /// Override this method to implement quest-specific completion logic
    /// </summary>
    /// <returns>True if quest should be marked as complete</returns>
    protected abstract bool CheckQuestCompletion();

    /// <summary>
    /// Called by QuestManager to start this quest
    /// </summary>
    public virtual void StartQuest()
    {
        if (!isCompleted)
        {
            isActive = true;
            OnQuestStarted?.Invoke();
            OnQuestStart();
        }
    }

    /// <summary>
    /// Called by QuestManager to complete this quest
    /// </summary>
    public virtual void CompleteQuest()
    {
        if (!isCompleted)
        {
            isCompleted = true;
            isActive = false;
            OnQuestCompleted?.Invoke();
            OnQuestComplete();
        }
    }

    /// <summary>
    /// Override this for custom quest start behavior
    /// </summary>
    protected virtual void OnQuestStart()
    {
        // Override in derived classes for custom start behavior
    }

    /// <summary>
    /// Override this for custom quest completion behavior
    /// </summary>
    protected virtual void OnQuestComplete()
    {
        // Override in derived classes for custom completion behavior
    }

    /// <summary>
    /// Reset quest to initial state
    /// </summary>
    public virtual void ResetQuest()
    {
        isCompleted = false;
        isActive = false;
    }

    /// <summary>
    /// Get quest progress as a value between 0 and 1
    /// </summary>
    public virtual float GetQuestProgress()
    {
        return isCompleted ? 1f : 0f;
    }
}
