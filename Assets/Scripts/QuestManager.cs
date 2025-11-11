using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    [Header("Quest Management")]
    [SerializeField] private Quest[] quests;
    [SerializeField] private int currentQuestIndex = 0;
    [SerializeField] private bool autoStartNextQuest = true;

    [Header("Events")]
    public UnityEvent<Quest> OnQuestStarted;
    public UnityEvent<Quest> OnQuestCompleted;
    public UnityEvent OnAllQuestsCompleted;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    public Quest CurrentQuest => (currentQuestIndex >= 0 && currentQuestIndex < quests.Length) ? quests[currentQuestIndex] : null;
    public Quest[] AllQuests => quests;
    public int CurrentQuestIndex => currentQuestIndex;
    public int TotalQuestsCount => quests?.Length ?? 0;
    public bool AllQuestsCompleted => currentQuestIndex >= TotalQuestsCount;

    private void Start()
    {
        InitializeQuests();
        StartCurrentQuest();
    }

    private void Update()
    {
        CheckCurrentQuestCompletion();
    }

    /// <summary>
    /// Initialize all quests and set up their completion callbacks
    /// </summary>
    private void InitializeQuests()
    {
        if (quests == null || quests.Length == 0)
        {
            LogDebug("No quests assigned to QuestManager!");
            return;
        }

        // Set up completion callbacks for all quests
        for (int i = 0; i < quests.Length; i++)
        {
            if (quests[i] != null)
            {
                int questIndex = i; // Capture for closure
                quests[i].OnQuestCompleted.AddListener(() => OnQuestCompletedCallback(questIndex));
            }
            else
            {
                Debug.LogError($"Quest at index {i} is null!");
            }
        }

        LogDebug($"Initialized {quests.Length} quests");
    }

    /// <summary>
    /// Start the current quest if it exists and hasn't been completed
    /// </summary>
    private void StartCurrentQuest()
    {
        if (AllQuestsCompleted)
        {
            LogDebug("All quests completed!");
            OnAllQuestsCompleted?.Invoke();
            return;
        }

        Quest currentQuest = CurrentQuest;
        if (currentQuest != null && !currentQuest.IsCompleted && !currentQuest.IsActive)
        {
            currentQuest.StartQuest();
            OnQuestStarted?.Invoke(currentQuest);
            LogDebug($"Started quest: {currentQuest.questName}");
        }
    }

    /// <summary>
    /// Check if the current quest is completed and handle progression
    /// </summary>
    private void CheckCurrentQuestCompletion()
    {
        Quest currentQuest = CurrentQuest;
        if (currentQuest != null && currentQuest.IsActive && currentQuest.IsCompleted)
        {
            // Quest was just completed, handle it
            LogDebug($"Quest completion detected: {currentQuest.questName}");
            OnQuestCompletedCallback(currentQuestIndex);
        }
    }

    /// <summary>
    /// Callback when a quest is completed
    /// </summary>
    /// <param name="questIndex">Index of the completed quest</param>
    private void OnQuestCompletedCallback(int questIndex)
    {
        if (questIndex < 0 || questIndex >= quests.Length) return;

        Quest completedQuest = quests[questIndex];
        LogDebug($"Quest completed callback: {completedQuest.questName} (Index: {questIndex})");

        OnQuestCompleted?.Invoke(completedQuest);

        // If this is the current quest, progress to next
        if (questIndex == currentQuestIndex)
        {
            LogDebug($"Progressing from quest {questIndex} to next quest");
            ProgressToNextQuest();
        }
    }

    /// <summary>
    /// Progress to the next quest in sequence
    /// </summary>
    private void ProgressToNextQuest()
    {
        currentQuestIndex++;

        if (AllQuestsCompleted)
        {
            LogDebug("All quests completed!");
            OnAllQuestsCompleted?.Invoke();
        }
        else if (autoStartNextQuest)
        {
            StartCurrentQuest();
        }
    }

    /// <summary>
    /// Manually start the next quest
    /// </summary>
    public void StartNextQuest()
    {
        if (!AllQuestsCompleted)
        {
            StartCurrentQuest();
        }
    }

    /// <summary>
    /// Jump to a specific quest (for testing/debugging)
    /// </summary>
    /// <param name="questIndex">Index of quest to jump to</param>
    public void JumpToQuest(int questIndex)
    {
        if (questIndex < 0 || questIndex >= quests.Length)
        {
            LogDebug($"Invalid quest index: {questIndex}");
            return;
        }

        // Stop current quest
        Quest currentQuest = CurrentQuest;
        if (currentQuest != null && currentQuest.IsActive)
        {
            currentQuest.ResetQuest();
        }

        currentQuestIndex = questIndex;
        StartCurrentQuest();
    }

    /// <summary>
    /// Reset all quests to initial state
    /// </summary>
    public void ResetAllQuests()
    {
        currentQuestIndex = 0;

        if (quests != null)
        {
            foreach (Quest quest in quests)
            {
                if (quest != null)
                {
                    quest.ResetQuest();
                }
            }
        }

        LogDebug("All quests reset");
        StartCurrentQuest();
    }

    /// <summary>
    /// Get quest by index
    /// </summary>
    /// <param name="index">Quest index</param>
    /// <returns>Quest at index, or null if invalid</returns>
    public Quest GetQuest(int index)
    {
        if (index < 0 || index >= TotalQuestsCount) return null;
        return quests[index];
    }

    /// <summary>
    /// Get quest by name
    /// </summary>
    /// <param name="questName">Name of quest to find</param>
    /// <returns>First quest with matching name, or null if not found</returns>
    public Quest GetQuestByName(string questName)
    {
        if (quests == null) return null;

        foreach (Quest quest in quests)
        {
            if (quest != null && quest.questName == questName)
            {
                return quest;
            }
        }

        return null;
    }

    /// <summary>
    /// Check if a specific quest is completed
    /// </summary>
    /// <param name="questIndex">Index of quest to check</param>
    /// <returns>True if quest is completed</returns>
    public bool IsQuestCompleted(int questIndex)
    {
        Quest quest = GetQuest(questIndex);
        return quest != null && quest.IsCompleted;
    }

    /// <summary>
    /// Get overall quest completion progress (0 to 1)
    /// </summary>
    /// <returns>Progress as float between 0 and 1</returns>
    public float GetOverallProgress()
    {
        if (TotalQuestsCount == 0) return 0f;

        int completedQuests = 0;
        float currentQuestProgress = 0f;

        for (int i = 0; i < TotalQuestsCount; i++)
        {
            Quest quest = GetQuest(i);
            if (quest != null)
            {
                if (quest.IsCompleted)
                {
                    completedQuests++;
                }
                else if (i == currentQuestIndex && quest.IsActive)
                {
                    currentQuestProgress = quest.GetQuestProgress();
                }
            }
        }

        return (completedQuests + currentQuestProgress) / TotalQuestsCount;
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] {message}");
        }
    }

    // Editor helper methods
#if UNITY_EDITOR
    [ContextMenu("Reset All Quests")]
    private void EditorResetAllQuests()
    {
        ResetAllQuests();
    }

    [ContextMenu("Complete Current Quest")]
    private void EditorCompleteCurrentQuest()
    {
        if (CurrentQuest != null)
        {
            CurrentQuest.CompleteQuest();
        }
    }

    [ContextMenu("Start Next Quest")]
    private void EditorStartNextQuest()
    {
        StartNextQuest();
    }
#endif
}
