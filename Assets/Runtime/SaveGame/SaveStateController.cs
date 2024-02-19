using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using signals;

public class SaveStateController
{
    public Signal<SaveGame> saveGameSelected;
    public Signal closeButtonClicked;
    private SaveGame _currentSave;
    private GameConfig _gameConfig;
    private string _gameConfigFileName = "config.txt";
    private string _saveSuffix = "_save.txt";
    private string _saveFolderPath = "saveGames/";
    private string _sceneThumbnailPath = "images/sceneThumbnails/";
    private SaveGameSelectionView _view;
    private static string SAVE_GAME_SELECTOR_PREFAB = "UI/SaveGameSelection";
    readonly LocaleManager _localeManager;

    public SaveStateController(LocaleManager localeManager)
    {
        _localeManager = localeManager;
        init();
    }

    public void show(bool show = true)
    {
        _view.show(show);

        if (show)
        {
            displaySaveGames();
        }
    }

    public void saveDialogues(List<DialogueNode> dialogueNodes)
    {
        _currentSave.dialogueNodes.AddRange(dialogueNodes);
        save();
    }

    public void createNewSave()
    {
        resetCurrentSave();
        _currentSave.id = getTotalSaves().ToString();
        _gameConfig.currentSaveId = _currentSave.id;
        _gameConfig.lastUpdateTime = TimeUtils.currentTime;
        saveConfig();
    }

    public void saveScene(string sceneId)
    {
        _currentSave.sceneId = sceneId;
        _currentSave.thumbnailPath = Path.Combine(_sceneThumbnailPath, sceneId);
        save();
    }

    public void saveMemoryId(string memoryId)
    {
        _currentSave.memoryIds.Add(memoryId);
        save();
    }

    public bool loadCurrentSave()
    {
        string configPath = Path.Combine(Application.persistentDataPath, _gameConfigFileName);
        bool newGame = true;

        if (File.Exists(configPath))
        {
            using (StreamReader streamReader = File.OpenText(configPath))
            {
                string jsonString = streamReader.ReadToEnd();
                _gameConfig = JsonUtility.FromJson<GameConfig>(jsonString);
            }
            load(_gameConfig.currentSaveId);
            newGame = !isSaveGameValid(_currentSave);
        }

        if (newGame)
        {
            createNewGameConfig();
        }
        
        return newGame;
    }

    private void createNewGameConfig()
    {
        _gameConfig = new GameConfig();
    }

    private bool isSaveGameValid(SaveGame saveGame)
    {
        return saveGame != null && !string.IsNullOrEmpty(saveGame.sceneId);
    }

    private void saveConfig()
    {
        string configPath = Path.Combine(Application.persistentDataPath, _gameConfigFileName);
        string jsonString = JsonUtility.ToJson(_gameConfig);
        using (StreamWriter streamWriter = File.CreateText(configPath))
        {
            streamWriter.Write(jsonString);
        }
    }

    private void init()
    {
        _currentSave = new SaveGame();
        _currentSave.dialogueNodes = new List<DialogueNode>();
        _currentSave.memoryIds = new List<string>();
        saveGameSelected = new Signal<SaveGame>();
        closeButtonClicked = new Signal();

        GameObject prefab = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(SAVE_GAME_SELECTOR_PREFAB));
        _view = prefab.GetComponent<SaveGameSelectionView>();
        _view.show(false, 0);
        _view.closeButton.onClick.AddListener(closeButtonClicked.Dispatch);

        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, _saveFolderPath);
        if (!Directory.Exists(saveDirectoryPath))
        {
            Directory.CreateDirectory(saveDirectoryPath);
        }
    }

    private void save()
    {
        _currentSave.lastSaveTime = TimeUtils.currentTime;
        
        string saveFileName = Path.Combine(_saveFolderPath, _currentSave.id + _saveSuffix);
        string currentSavePath = Path.Combine(Application.persistentDataPath, saveFileName);
        string jsonString = JsonUtility.ToJson(_currentSave);
        using (StreamWriter streamWriter = File.CreateText(currentSavePath))
        {
            streamWriter.Write(jsonString);
        }
    }

    public int getTotalSaves()
    {
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, _saveFolderPath);

        if (string.IsNullOrEmpty(saveDirectoryPath)) { return 0; }

        string[] filePaths = Directory.GetFiles(saveDirectoryPath);

        return filePaths.Length;
    }

    private void displaySaveGames()
    {
        _view.clearSaveGames();

        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, _saveFolderPath);
        string[] filePaths = Directory.GetFiles(saveDirectoryPath);

        foreach(string filePath in filePaths)
        {
            using (StreamReader streamReader = File.OpenText(filePath))
            {
                string jsonString = streamReader.ReadToEnd();
                SaveGame saveGame = JsonUtility.FromJson<SaveGame>(jsonString);
                SaveGameEntryView entry = _view.addSaveGame(saveGame);
                entry.sceneLabel.text = _localeManager.lookup(saveGame.sceneId + "_scene_label");
                entry.saveTime.text = saveGame.lastSaveTime.ToString();
                entry.imageContainerView.loadImageResource(saveGame.thumbnailPath);
                entry.mainButton.onClick.AddListener(() => handleSaveGameSelected(saveGame));

                if (saveGame.id == _currentSave.id)
                {
                    entry.deleteButton.gameObject.SetActive(false);
                    entry.saveTime.text += " (Current)";
                }
                else
                {
                    entry.deleteButton.onClick.AddListener(() => delete(saveGame.id));
                }
            }
        }
    }

    private void handleSaveGameSelected(SaveGame saveGame)
    {
        _currentSave = saveGame;
        _gameConfig.currentSaveId = _currentSave.id;
        saveConfig();
        saveGameSelected.Dispatch(saveGame);
    }

    public SaveGame load(string id)
    {
        string saveFileName = Path.Combine(_saveFolderPath, id + _saveSuffix);
        string currentSavePath = Path.Combine(Application.persistentDataPath, saveFileName);

        if (!File.Exists(currentSavePath))
        {
            return null;
        }

        using (StreamReader streamReader = File.OpenText(currentSavePath))
        {
            string jsonString = streamReader.ReadToEnd();
            _currentSave = JsonUtility.FromJson<SaveGame>(jsonString);
            return _currentSave;
        }
    }

    public void delete(string id)
    {
        if (_currentSave.id == id)
        {
            resetCurrentSave();
        }

        string saveFileName = Path.Combine(_saveFolderPath, id + _saveSuffix);
        string savePathToDelete = Path.Combine(Application.persistentDataPath, saveFileName);

        if (File.Exists(savePathToDelete))
        {
            File.Delete(savePathToDelete);
            displaySaveGames();
        }
    }

    public bool hasOutcome(string outcome)
    {
        foreach(DialogueNode node in _currentSave.dialogueNodes)
        {
            if (node.outcome == outcome)
            {
                return true;
            }
        }

        return false;
    }

    private void resetCurrentSave()
    {
        _currentSave.dialogueNodes.Clear();
        _currentSave.memoryIds.Clear();
        _currentSave.sceneId = null;
        _currentSave.thumbnailPath = null;
        _currentSave.lastSaveTime = 0;
        _currentSave.id = null;
    }

    public SaveGame CurrentSave
    {
        get
        {
            return _currentSave;
        }
    }
}

[Serializable]
public class SaveGame
{
    public string id;
    public double lastSaveTime;
    public string sceneId;
    public string thumbnailPath;
    public List<DialogueNode> dialogueNodes;
    public List<string> memoryIds;
    public float musicVolume;
    public float sfxVolume;
}

[Serializable]
public class GameConfig
{
    public double lastUpdateTime;
    public string currentSaveId;
}
