using UnityEngine;
using System.Collections.Generic;

public class SceneController
{
    private Dictionary<string, Scene> _loadedScenes;
    private string _currentSceneId;
    private string _previousSceneId;
    private static string SCENES_PATH = "Scenes/";
    private Memory _currentMemory;
    private Scene _currentScene;
    private bool _endOfDayActive = false;
    readonly UIController _uiController;
    readonly DialogueController _dialogueController;
    readonly MemoryController _memoryController;
    readonly LocaleManager _localeManager;
    readonly SaveStateController _saveGameManager;
    readonly CameraController _cameraController;
    readonly ClockController _clockController;

    public SceneController(UIController uiController,
                           DialogueController dialogueController,
                           MemoryController memoryController,
                           LocaleManager localeManager,
                           SaveStateController saveGameManager,
                           CameraController cameraController,
                           ClockController clockController)
    {
        _uiController = uiController;
        _dialogueController = dialogueController;
        _memoryController = memoryController;
        _localeManager = localeManager;
        _saveGameManager = saveGameManager;
        _cameraController = cameraController;
        _clockController = clockController;

        init();
    }

    private void init()
    {
        _loadedScenes = new Dictionary<string, Scene>();
        _dialogueController.dialogueComplete.Add(handleDialogueComplete);
    }

    private void handleDialogueComplete(Dialogue dialogue)
    {
        refreshHotspots();

        if (_currentScene.customSceneController != null && _currentMemory != null)
        {
            _currentScene.customSceneController.handleDialogueComplete(_currentMemory.dialoguesId);
        }

        if (_endOfDayActive)
        {
            _endOfDayActive = false;
            _clockController.beginDay();
        }
        else if (_currentMemory != null)
        {
            Memory lastMemory = _currentMemory;
            _currentMemory = null;

            _clockController.advanceTime(lastMemory.timeCostInMinutes);

            System.Action endOfDaySequence = null;

            if (_clockController.isEndOfDay)
            {
                endOfDaySequence = () =>
                {
                    _endOfDayActive = true;
                    // TEMP - TODO : this should be per-day in data.
                    startMemory("end_of_day");
                };
            }

            if (lastMemory.sceneId != null && lastMemory.sceneId != _previousSceneId)
            {
                moveToScene(_previousSceneId, endOfDaySequence);
            }
            else if (endOfDaySequence != null)
            {
                endOfDaySequence();
            }
            else
            {

            }
        }
    }

    public void loadScene(string sceneId, System.Action roomLoadedAction = null)
    {
        if (_currentSceneId != null)
        {
            //_loadedScenes[_currentSceneId].SetActive(false);
            GameObject.Destroy(_loadedScenes[_currentSceneId].gameObject);
            _loadedScenes.Remove(_currentSceneId);
        }
        _previousSceneId = _currentSceneId;
        _currentSceneId = sceneId;
        if (!_loadedScenes.ContainsKey(sceneId))
        {
            GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(SCENES_PATH + sceneId));  //Container.InstantiatePrefabResource(SCENES_PATH + sceneId);
            _loadedScenes[sceneId] = prefab.GetComponent<Scene>();
            _loadedScenes[sceneId].view.canvas.worldCamera = _cameraController.camera;
            setupScene(_loadedScenes[sceneId]);
        }
        else
        {
            _loadedScenes[_currentSceneId].gameObject.SetActive(true);
            _currentScene = _loadedScenes[_currentSceneId];
        }

        refreshHotspots();

        if (roomLoadedAction != null)
        {
            _uiController.fadeComplete.Add(roomLoadedAction);
        }
        _uiController.fade(false);
    }

    public void moveToScene(string sceneId, System.Action roomLoadedAction = null)
    {
        _uiController.fadeComplete.AddOnce(() => loadScene(sceneId, roomLoadedAction));
        _uiController.fade(true);
    }

    private void setupScene(Scene scene)
    {
        _currentScene = scene;
        setupHotspots(scene);
    }

    public void startMemory(string memoryId)
    {
        _uiController.showClock(false);
        _uiController.showText(false);
        _currentMemory = _memoryController.getMemory(memoryId);

        if (_currentMemory.sceneId != null && _currentMemory.sceneId != _currentSceneId)
        {
            moveToScene(_currentMemory.sceneId, () => startDialogue(_currentMemory.dialoguesId, memoryId));
        }
        else
        {
            startDialogue(_currentMemory.dialoguesId, memoryId);
        }
    }

    private void startDialogue(string dialoguesId, string memoryId = null)
    {
        _uiController.showClock(false);
        _uiController.showText(false);
        _uiController.fadeComplete.RemoveAll();
        _dialogueController.start(dialoguesId, memoryId);
    }

    private void setupHotspots(Scene scene)
    {
        if (scene.view.hotspotsContainer == null)
        {
            return;
        }
        foreach (Hotspot hotspot in scene.view.hotspotsContainer.GetComponentsInChildren<Hotspot>())
        {
            hotspot.view.click.Add(() => handleHotspotClick(hotspot));
            hotspot.view.over.Add(() => handleHotspotOver(hotspot));
            hotspot.view.off.Add(() => handleHotspotOff(hotspot));
        }
    }

    private bool shouldShowHotspot(Hotspot hotspot)
    {
        return  !(!string.IsNullOrEmpty(hotspot.memory) && _saveGameManager.CurrentSave.memoryIds.Contains(hotspot.memory));
    }

    private void refreshHotspots()
    {
        Hotspot[] hotspots = _currentScene.view.hotspotsContainer.GetComponentsInChildren<Hotspot>(true);

        if (hotspots != null)
        {
            foreach(Hotspot hotspot in hotspots)
            {
                hotspot.gameObject.SetActive(!shouldHideForOutcomes(hotspot) && shouldShowHotspot(hotspot));
            }
        }
    }

    private bool shouldHideForOutcomes(Hotspot hotspot)
    {
        foreach (string outcome in hotspot.hideForOutcomes)
        {
            if (_saveGameManager.hasOutcome(outcome))
            {
                return true;
            }
        }

        bool doHide = false;

        if (hotspot.showForOutcomes.Count > 0)
        {
            doHide = true;

            foreach (string outcome in hotspot.showForOutcomes)
            {
                if (_saveGameManager.hasOutcome(outcome))
                {
                    return false;
                }
            }
        }

        return doHide;
    }

    private bool hotspotsDisabled()
    {
        return _dialogueController.showingDialog;
    }

    private void handleHotspotClick(Hotspot hotspot)
    {
        if (hotspotsDisabled())
        {
            return;
        }
        
        if (_currentScene.customSceneController != null)
        {
            _currentScene.customSceneController.handleHotspotClick(hotspot);
        }

        switch (hotspot.type)
        {
            case Hotspot.Type.Move:
                _saveGameManager.saveScene(hotspot.destination.id);
                moveToScene(hotspot.destination.id);
                break;

            case Hotspot.Type.Item:
                if (!string.IsNullOrEmpty(hotspot.dialogue))
                {
                    startDialogue(hotspot.dialogue);
                }
                else
                {
                    _uiController.setText("You examine the " + hotspot.item.id + ".");
                }
                break;

            case Hotspot.Type.Memory:
                startMemory(hotspot.memory);
                break;

            case Hotspot.Type.Dialogue:
                startDialogue(hotspot.dialogue);
                break;
        }
    }

    private void handleHotspotOver(Hotspot hotspot)
    {
        if (hotspotsDisabled())
        {
            return;
        }

        if (_currentScene.customSceneController != null)
        {
            _currentScene.customSceneController.handleHotspotOver(hotspot);
        }

        string labelText = null;

        switch (hotspot.type)
        {
            case Hotspot.Type.Move:
                labelText = _localeManager.lookup(hotspot.destination.id + "_hotspot_label");

                if (labelText == null)
                {
                    labelText = _localeManager.lookup("hotspot_destination_label", new string[] { hotspot.destination.id });
                }
                break;

            case Hotspot.Type.Item:
                labelText = _localeManager.lookup("hotspot_item_label", new string[] { hotspot.item.id });
                break;

            case Hotspot.Type.Memory:
                labelText = _localeManager.lookup(hotspot.memory + "_hotspot_label");

                if (labelText == null)
                {
                    labelText = _localeManager.lookup("hotspot_memory_label", new string[] { hotspot.memory });
                }
                Memory selectedMemory = _memoryController.getMemory(hotspot.memory);
                _uiController.showClock(true, selectedMemory.timeCostInMinutes);
                break;
            case Hotspot.Type.Dialogue:
                labelText = _localeManager.lookup(hotspot.dialogue + "_hotspot_label");

                if (labelText == null)
                {
                    labelText = _localeManager.lookup("hotspot_dialogue_label", new string[] { hotspot.dialogue });
                }
                break;
        }

        if (!string.IsNullOrEmpty(labelText))
        {
            _uiController.setText(labelText);
        }
    }

    private void handleHotspotOff(Hotspot hotspot)
    {
        if (hotspotsDisabled())
        {
            return;
        }

        if (_currentScene.customSceneController != null)
        {
            _currentScene.customSceneController.handleHotspotOff(hotspot);
        }

        _uiController.showText(false);
        _uiController.showClock(false);
    }
}
