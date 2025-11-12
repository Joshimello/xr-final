# Quest System Documentation

A flexible and extensible quest management system for Unity that allows developers to create sequential quests with automatic progression and UI display.

## System Overview

The quest system consists of four main components:

1. **Quest** (Base Class) - Abstract base class for all quests
2. **QuestManager** - Manages quest progression and state
3. **QuestDisplay** - UI component for showing current quest information
4. **Quest Implementations** - Concrete quest classes (GoNearPointQuest, CollectObjectsQuest, etc.)

## Quick Setup

### 1. Create Quest Manager
1. Create an empty GameObject in your scene
2. Add the `QuestManager` component
3. Configure the quest array and settings

### 2. Create Quest Objects
1. Create GameObjects for each quest you want
2. Add quest components (e.g., `GoNearPointQuest`, `CollectObjectsQuest`)
3. Configure quest parameters in the inspector
4. Drag quest objects into the QuestManager's quest array

### 3. Setup UI Display
1. Create a UI Canvas if you don't have one
2. Create UI elements for quest display (Text, Slider, etc.)
3. Add the `QuestDisplay` component to a GameObject
4. Assign UI references and the QuestManager

## Quest Manager Configuration

### Inspector Settings
- **Quests**: Array of Quest objects to manage
- **Current Quest Index**: Starting quest index (usually 0)
- **Auto Start Next Quest**: Automatically start the next quest when current completes
- **Enable Debug Logs**: Show debug information in console

### Events
- **OnQuestStarted**: Triggered when a quest begins
- **OnQuestCompleted**: Triggered when a quest is completed
- **OnAllQuestsCompleted**: Triggered when all quests are finished

## Creating Custom Quests

### Basic Quest Structure
```csharp
public class MyCustomQuest : Quest
{
    [Header("Custom Quest Settings")]
    [SerializeField] private float someParameter = 1f;

    protected override bool CheckQuestCompletion()
    {
        // Return true when quest should be completed
        return /* your completion logic */;
    }

    protected override void OnQuestStart()
    {
        base.OnQuestStart();
        // Custom start behavior
    }

    protected override void OnQuestComplete()
    {
        base.OnQuestComplete();
        // Custom completion behavior
    }

    public override float GetQuestProgress()
    {
        if (IsCompleted) return 1f;
        // Return progress between 0f and 1f
        return /* your progress calculation */;
    }
}
```

### Required Overrides
- `CheckQuestCompletion()`: Return true when quest should complete
- Optional overrides:
  - `OnQuestStart()`: Custom start behavior
  - `OnQuestComplete()`: Custom completion behavior  
  - `GetQuestProgress()`: Return progress (0-1) for UI display

## Built-in Quest Types

### GoNearPointQuest
Move within a specified distance of a target point.

**Inspector Settings:**
- **Target Point**: GameObject to move near
- **Object To Test**: GameObject to check distance from (defaults to Player)
- **Required Distance**: How close you need to get
- **Use Player As Test Object**: Automatically find player object

**Example Use Cases:**
- Navigate to waypoints
- Reach specific locations
- Proximity-based triggers

### CollectObjectsQuest
Collect a specified number of objects.

**Inspector Settings:**
- **Objects To Collect**: Specific objects to collect (if using specific objects)
- **Required Collection Count**: How many objects to collect
- **Collectible Tag**: Tag to search for (if not using specific objects)
- **Use Specific Objects**: Use assigned objects vs. objects with tag
- **Collection Range**: How close player needs to be to collect
- **Destroy On Collect**: Whether to destroy or just disable objects

**Example Use Cases:**
- Gather items or resources
- Pick up quest objects
- Collectible hunting

## Quest Display Configuration

### UI References
- **Quest Title Text**: TextMeshPro component for quest name
- **Quest Description Text**: TextMeshPro component for quest description
- **Quest Progress Text**: TextMeshPro component for progress text
- **Progress Bar**: Slider component for visual progress
- **Quest Panel**: GameObject containing the UI elements

### Display Settings
- **Show Quest Title**: Enable/disable title display
- **Show Quest Description**: Enable/disable description display
- **Show Progress Text**: Enable/disable progress text
- **Show Progress Bar**: Enable/disable progress slider
- **Hide When No Quests**: Hide UI when no active quests

### Text Formatting
- **Title Prefix**: Text before quest name (e.g., "Current Mission: ")
- **Progress Format**: String format for progress (e.g., "Progress: {0:P0}")
- **Completed Text**: Text when quest is done
- **No Quests Text**: Text when no quests available

## Usage Examples

### Basic Setup
1. Create QuestManager GameObject
2. Create quest objects with quest components
3. Assign quests to QuestManager array
4. Configure UI with QuestDisplay component

### Scripting Integration
```csharp
// Get quest manager reference
QuestManager questManager = FindObjectOfType<QuestManager>();

// Check if specific quest is completed
bool isQuestDone = questManager.IsQuestCompleted(0);

// Get current quest
Quest currentQuest = questManager.CurrentQuest;

// Jump to specific quest (for testing)
questManager.JumpToQuest(2);

// Reset all quests
questManager.ResetAllQuests();

// Get overall progress
float overallProgress = questManager.GetOverallProgress();
```

## Best Practices

### Quest Design
- Keep quests simple and focused
- Provide clear objectives in quest descriptions
- Use meaningful quest names
- Test quest completion logic thoroughly

### Performance
- Use efficient collision detection for proximity quests
- Cache Transform references in Start()
- Avoid expensive operations in CheckQuestCompletion()

### User Experience
- Provide clear visual feedback for quest progress
- Use audio cues for quest completion
- Show helpful guidance (arrows, markers, etc.)

### Debugging
- Enable debug logs during development
- Use Scene view gizmos to visualize quest areas
- Test quest progression in various scenarios

## Common Issues & Solutions

### Quest Not Starting
- Check if previous quest is completed
- Verify quest is in QuestManager array
- Ensure QuestManager has correct starting index

### Quest Not Completing
- Check CheckQuestCompletion() logic
- Verify required objects/parameters are assigned
- Use debug logs to track completion conditions

### UI Not Updating
- Ensure QuestDisplay has QuestManager reference
- Check if UI elements are properly assigned
- Verify Canvas is set up correctly

### Performance Issues
- Optimize CheckQuestCompletion() methods
- Use object pooling for collectibles
- Limit quest checking frequency if needed

## Extension Ideas

### Additional Quest Types
- **TimeBasedQuest**: Complete within time limit
- **KillEnemiesQuest**: Defeat specific number of enemies  
- **UseItemQuest**: Use specific items or abilities
- **DialogueQuest**: Talk to NPCs
- **CraftingQuest**: Create specific items

### Advanced Features
- Quest prerequisites and branching
- Optional/side quests
- Quest rewards and experience
- Save/load quest progress
- Localization support
