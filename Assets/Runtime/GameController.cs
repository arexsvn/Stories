using UnityEngine;
using VContainer.Unity;

public class GameController : ITickable, ILateTickable, IFixedTickable, IInitializable
{
    private bool _gamePaused = true;
    readonly UIController _uiController;
    readonly SceneController _sceneController;
    readonly SaveStateController _saveStateController;
    readonly DialogueController _dialogueController;
    readonly LocaleManager _localeManager;
    readonly UIViewManager _uiViewManager;
    private const string START_SCENE = "kitchen";
    public const string STRINGS_PATH = "data/strings";

    public GameController(UIController uiController, 
                          SceneController sceneController, 
                          SaveStateController saveStateController, 
                          DialogueController dialogueController, 
                          LocaleManager localeManager,
                          UIViewManager uiViewManager) 
    {
        _uiController = uiController;
        _sceneController = sceneController;
        _saveStateController = saveStateController;
        _dialogueController = dialogueController;
        _localeManager = localeManager;
        _uiViewManager = uiViewManager;

    }

    // for playing memories from the MemoryCreator tool
    public void loadMemory(string memoryId)
    {
        _uiController.showMainMenu(false, false);
        _sceneController.startMemory(memoryId);
    }

    public async void Initialize()
    {
        loadLocale();
        await _uiViewManager.Init();
        showMainMenu();
    }

    private void loadLocale()
    {
        TextAsset strings = (TextAsset)Resources.Load(STRINGS_PATH);
        _localeManager.addBundle(strings.text);
    }

    private void showMainMenu()
    {
        bool newGame = _saveStateController.loadCurrentSave();
        // load latest scene -or- starting scene
        if (_saveStateController.CurrentSave.sceneId == null)
        {
            _saveStateController.CurrentSave.sceneId = START_SCENE;
        }
        _sceneController.loadScene(_saveStateController.CurrentSave.sceneId);

        _uiController.initMainMenu();
        _uiController.showMainMenu();
        _uiController.showResumeButton(!newGame);
        _uiController.showLoadButton(!newGame && _saveStateController.getTotalSaves() > 0);
        _uiController.newGame.Add(handleNewGame);
        _uiController.resumeGame.Add(handleResumeGame);
        _uiController.unpauseGame.Add(handleUnPauseGame);
        _uiController.pauseGame.Add(handlePauseGame);
        _uiController.loadGame.Add(handleLoadGame);
        _uiController.quitGame.Add(handleQuitGame);
    }
       
    private void handleNewGame()
    {
        _dialogueController.stop();
        _uiController.fadeComplete.AddOnce(newGame);
        _uiController.fade(true);
    }

    private void handlePauseGame()
    {
        _dialogueController.pause();
    }

    private void handleUnPauseGame()
    {
        _dialogueController.resume();
    }

    private void handleResumeGame()
    {
        restoreMemories(_saveStateController.CurrentSave);
        _uiController.showMainMenu(false, false);
        _dialogueController.resume();
    }

    private void handleLoadGame()
    {
        _saveStateController.saveGameSelected.AddOnce(handleSaveGameSelected);
        _saveStateController.closeButtonClicked.AddOnce(handleSaveGameSelectionClosed);
        _saveStateController.show();
    }

    private void handleQuitGame()
    {
        Application.Quit();
    }

    private void handleSaveGameSelectionClosed()
    {
        _saveStateController.saveGameSelected.RemoveAll();
        _saveStateController.closeButtonClicked.RemoveAll();
        _saveStateController.show(false);
    }

    private void handleSaveGameSelected(SaveGame saveGame)
    {
        _dialogueController.stop();
        _uiController.fadeComplete.AddOnce(() => loadGame(saveGame));
        _uiController.fade(true);
        handleSaveGameSelectionClosed();
    }

    private void loadGame(SaveGame saveGame)
    {
        restoreMemories(saveGame);
        _uiController.showMainMenu(false, false, 0);
        // load latest scene -or- starting scene
        _sceneController.loadScene(saveGame.sceneId);
    }

    private void restoreMemories(SaveGame saveGame)
    {
        _dialogueController.reset();
        if (saveGame != null && !string.IsNullOrEmpty(saveGame.sceneId))
        {
            // TODO Validate save game
            if (saveGame.dialogueNodes != null && saveGame.dialogueNodes.Count > 0)
            {
                _dialogueController.restore(saveGame.dialogueNodes);
            }
        }
    }

    private void newGame()
    {
        _uiController.showMainMenu(false, true, 0);
        _uiController.showResumeButton();
        _uiController.showLoadButton();

        _dialogueController.reset();
        _saveStateController.createNewSave();
        _sceneController.loadScene(START_SCENE);
        _saveStateController.saveScene(START_SCENE);
        _gamePaused = false;
    }

    public void Tick()
    {
        if (_gamePaused)
        {
            return;
        }

    }

    public void FixedTick()
    {
        if (_gamePaused)
        {
            return;
        }
    }

    public void LateTick()
    {
        if (_gamePaused)
        {
            return;
        }
    }
}
