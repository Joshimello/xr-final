using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private TextMeshProUGUI questProgressText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject questPanel;

    [Header("Quest Manager")]
    [SerializeField] private QuestManager questManager;

    [Header("Display Settings")]
    [SerializeField] private bool showQuestTitle = true;
    [SerializeField] private bool showQuestDescription = true;
    [SerializeField] private bool showProgressText = true;
    [SerializeField] private bool showProgressBar = true;
    [SerializeField] private bool hideWhenNoQuests = true;

    [Header("Text Formatting")]
    [SerializeField] private string titlePrefix = "Current Mission: ";
    [SerializeField] private string progressFormat = "Progress: {0:P0}";
    [SerializeField] private string completedText = "Mission Complete!";
    [SerializeField] private string noQuestsText = "No active missions";

    [Header("Animation")]
    [SerializeField] private bool animateTextChanges = true;
    [SerializeField] private float fadeSpeed = 2f;

    private Quest currentDisplayedQuest;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        InitializeComponents();
        SetupQuestManager();
        UpdateDisplay();
    }

    private void Update()
    {
        UpdateDisplay();
    }

    /// <summary>
    /// Initialize UI components and cache references
    /// </summary>
    private void InitializeComponents()
    {
        // Get CanvasGroup for fade animations
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && animateTextChanges)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Find QuestManager if not assigned
        if (questManager == null)
        {
            questManager = FindObjectOfType<QuestManager>();
            if (questManager == null)
            {
                Debug.LogWarning("QuestDisplay: No QuestManager found in scene!");
            }
        }

        // Hide/show UI elements based on settings
        SetUIElementVisibility();
    }

    /// <summary>
    /// Setup quest manager event listeners
    /// </summary>
    private void SetupQuestManager()
    {
        if (questManager != null)
        {
            questManager.OnQuestStarted.AddListener(OnQuestStarted);
            questManager.OnQuestCompleted.AddListener(OnQuestCompleted);
            questManager.OnAllQuestsCompleted.AddListener(OnAllQuestsCompleted);
        }
    }

    /// <summary>
    /// Set visibility of UI elements based on settings
    /// </summary>
    private void SetUIElementVisibility()
    {
        if (questTitleText != null)
            questTitleText.gameObject.SetActive(showQuestTitle);

        if (questDescriptionText != null)
            questDescriptionText.gameObject.SetActive(showQuestDescription);

        if (questProgressText != null)
            questProgressText.gameObject.SetActive(showProgressText);

        if (progressBar != null)
            progressBar.gameObject.SetActive(showProgressBar);
    }

    /// <summary>
    /// Update the quest display
    /// </summary>
    private void UpdateDisplay()
    {
        if (questManager == null) return;

        Quest currentQuest = questManager.CurrentQuest;

        // Check if we need to update the display
        if (currentDisplayedQuest != currentQuest)
        {
            currentDisplayedQuest = currentQuest;
            RefreshDisplay();
        }

        // Update progress for active quest
        if (currentQuest != null && currentQuest.IsActive)
        {
            UpdateProgressDisplay(currentQuest);
        }
    }

    /// <summary>
    /// Refresh the entire display
    /// </summary>
    private void RefreshDisplay()
    {
        if (questManager.AllQuestsCompleted)
        {
            ShowAllQuestsCompleted();
        }
        else if (currentDisplayedQuest == null)
        {
            ShowNoQuests();
        }
        else
        {
            ShowCurrentQuest(currentDisplayedQuest);
        }
    }

    /// <summary>
    /// Show the current active quest
    /// </summary>
    /// <param name="quest">Quest to display</param>
    private void ShowCurrentQuest(Quest quest)
    {
        if (quest == null) return;

        ShowQuestPanel();

        // Update title
        if (questTitleText != null && showQuestTitle)
        {
            questTitleText.text = titlePrefix + quest.questName;
        }

        // Update description
        if (questDescriptionText != null && showQuestDescription)
        {
            questDescriptionText.text = quest.questDescription;
        }

        // Update progress
        UpdateProgressDisplay(quest);
    }

    /// <summary>
    /// Update progress-related UI elements
    /// </summary>
    /// <param name="quest">Quest to get progress from</param>
    private void UpdateProgressDisplay(Quest quest)
    {
        if (quest == null) return;

        float progress = quest.GetQuestProgress();

        // Update progress text
        if (questProgressText != null && showProgressText)
        {
            if (quest.IsCompleted)
            {
                questProgressText.text = completedText;
            }
            else
            {
                questProgressText.text = string.Format(progressFormat, progress);
            }
        }

        // Update progress bar
        if (progressBar != null && showProgressBar)
        {
            progressBar.value = progress;
        }
    }

    /// <summary>
    /// Show message when no quests are available
    /// </summary>
    private void ShowNoQuests()
    {
        if (hideWhenNoQuests)
        {
            HideQuestPanel();
        }
        else
        {
            ShowQuestPanel();

            if (questTitleText != null)
                questTitleText.text = noQuestsText;

            if (questDescriptionText != null)
                questDescriptionText.text = "";

            if (questProgressText != null)
                questProgressText.text = "";

            if (progressBar != null)
                progressBar.value = 0f;
        }
    }

    /// <summary>
    /// Show message when all quests are completed
    /// </summary>
    private void ShowAllQuestsCompleted()
    {
        ShowQuestPanel();

        if (questTitleText != null)
            questTitleText.text = "All Missions Complete!";

        if (questDescriptionText != null)
            questDescriptionText.text = "Great job completing all your missions!";

        if (questProgressText != null)
            questProgressText.text = string.Format(progressFormat, 1f);

        if (progressBar != null)
            progressBar.value = 1f;
    }

    /// <summary>
    /// Show the quest panel
    /// </summary>
    private void ShowQuestPanel()
    {
        if (questPanel != null)
            questPanel.SetActive(true);

        if (canvasGroup != null && animateTextChanges)
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, fadeSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Hide the quest panel
    /// </summary>
    private void HideQuestPanel()
    {
        if (canvasGroup != null && animateTextChanges)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
            if (canvasGroup.alpha <= 0.01f && questPanel != null)
                questPanel.SetActive(false);
        }
        else if (questPanel != null)
        {
            questPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Event handler for when a quest starts
    /// </summary>
    /// <param name="quest">Started quest</param>
    private void OnQuestStarted(Quest quest)
    {
        // Optional: Add quest start animation/sound effect here
        Debug.Log($"QuestDisplay: Quest started - {quest.questName}");
    }

    /// <summary>
    /// Event handler for when a quest is completed
    /// </summary>
    /// <param name="quest">Completed quest</param>
    private void OnQuestCompleted(Quest quest)
    {
        // Optional: Add quest completion animation/sound effect here
        Debug.Log($"QuestDisplay: Quest completed - {quest.questName}");
    }

    /// <summary>
    /// Event handler for when all quests are completed
    /// </summary>
    private void OnAllQuestsCompleted()
    {
        // Optional: Add celebration animation/sound effect here
        Debug.Log("QuestDisplay: All quests completed!");
    }

    /// <summary>
    /// Manually refresh the display (useful for editor testing)
    /// </summary>
    [ContextMenu("Refresh Display")]
    public void ManualRefresh()
    {
        currentDisplayedQuest = null;
        UpdateDisplay();
    }

    /// <summary>
    /// Set the quest manager reference
    /// </summary>
    /// <param name="manager">Quest manager to use</param>
    public void SetQuestManager(QuestManager manager)
    {
        questManager = manager;
        SetupQuestManager();
        UpdateDisplay();
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (questManager != null)
        {
            questManager.OnQuestStarted.RemoveListener(OnQuestStarted);
            questManager.OnQuestCompleted.RemoveListener(OnQuestCompleted);
            questManager.OnAllQuestsCompleted.RemoveListener(OnAllQuestsCompleted);
        }
    }
}
