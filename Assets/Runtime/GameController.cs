using FlowCanvas.Nodes;
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
    readonly ISubscriber<ApplicationMessage> _applicationMessageSubscriber;
    readonly MainMenuController _mainMenuController;
    private const string START_SCENE = "kitchen";
    public const string STRINGS_PATH = "data/strings";

    public GameController(UIController uiController, 
                          SceneController sceneController, 
                          SaveStateController saveStateController, 
                          DialogueController dialogueController, 
                          LocaleManager localeManager,
                          MainMenuController mainMenuController,
                          ISubscriber<ApplicationMessage> applicationMessageSubscriber) 
    {
        _uiController = uiController;
        _sceneController = sceneController;
        _saveStateController = saveStateController;
        _dialogueController = dialogueController;
        _localeManager = localeManager;
        _mainMenuController = mainMenuController;
        _applicationMessageSubscriber = applicationMessageSubscriber;
    }

    // for playing memories from the MemoryCreator tool
    public void loadMemory(string memoryId)
    {
        showMainMenu(false, false);
        _sceneController.startMemory(memoryId);
    }

    public void Initialize()
    {
        _applicationMessageSubscriber.Subscribe(OnApplicationMessage);

        loadLocale();
        initMainMenu();
    }

    private void loadLocale()
    {
        TextAsset strings = (TextAsset)Resources.Load(STRINGS_PATH);
        _localeManager.addBundle(strings.text);
    }

    private void initMainMenu()
    {
        bool newGame = _saveStateController.loadCurrentSave();
        // load latest scene -or- starting scene
        if (_saveStateController.CurrentSave.sceneId == null)
        {
            _saveStateController.CurrentSave.sceneId = START_SCENE;
        }
        _sceneController.loadScene(_saveStateController.CurrentSave.sceneId);

        _mainMenuController.init();
        _mainMenuController.newGame.Add(handleNewGame);
        _mainMenuController.resumeGame.Add(handleResumeGame);
        _mainMenuController.loadGame.Add(handleLoadGame);
        _mainMenuController.quitGame.Add(handleQuit);

        _mainMenuController.allowResume(!newGame);
        _mainMenuController.allowLoad(!newGame && _saveStateController.getTotalSaves() > 0);
        showMainMenu();
    }

    public void showMainMenu(bool show = true, bool fadeFromBlack = true, float fadeTime = -1)
    {
        if (show)
        {
            if (fadeFromBlack)
            {
                _uiController.fade(false, default(Color), -1, 1f);
            }
            handlePause();
        }

        _mainMenuController.show(show, fadeTime);
    }

    private void OnApplicationMessage(ApplicationMessage applicationMessage)
    {
        switch (applicationMessage.ApplicationAction)
        {
            case ApplicationAction.Pause:
                handlePause();
                break;
            case ApplicationAction.UnPause:
                handleUnPause();
                break;
            case ApplicationAction.Quit:
                handleQuit();
                break;
            case ApplicationAction.ShowMainMenu:
                showMainMenu(true, false);
                break;
        }
    }

    private void handlePause()
    {
        _gamePaused = true;
        _dialogueController.pause();
    }

    private void handleUnPause()
    {
        _gamePaused = false;
        _dialogueController.resume();
    }

    private void handleQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void handleNewGame()
    {
        _dialogueController.stop();
        _uiController.fadeComplete.AddOnce(newGame);
        _uiController.fade(true);
    }

    private void handleResumeGame()
    {
        restoreMemories(_saveStateController.CurrentSave);
        showMainMenu(false, false);
        _dialogueController.resume();
    }

    private void handleLoadGame()
    {
        _saveStateController.saveGameSelected.AddOnce(handleSaveGameSelected);
        _saveStateController.closeButtonClicked.AddOnce(handleSaveGameSelectionClosed);
        _saveStateController.show();
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
        showMainMenu(false, false, 0);
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
        showMainMenu(false, true, 0);
        _mainMenuController.allowResume(true);
        _mainMenuController.allowLoad(true);

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
