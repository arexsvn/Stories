using UnityEngine;
using System.Collections.Generic;
using signals;

public class DialogueController
{
    public Signal<Dialogue> dialogueComplete;
    private Dictionary<string, Dialogues> _dialogues;
    private DialogueView _dialogueView;
    private LeafNode _currentNode;
    private Dialogue _currentDialogue;
    private CharacterBio _currentCharacter;
    private string _currentEmotion;
    private string _currentMemoryId;
    private List<DialogueNode> _dialoguesToSave;
    private static string DIALOGUE_PREFAB = "UI/DialogueOverlay";
    private static string CHOICE_PREFAB = "UI/ChoiceEntry";
    public static string DIALOGUE_PATH = "data/dialogues/";
    private static float TEXT_TIME_MULTIPLIER = 0.065f;
    private static float MIN_TEXT_SECONDS = 2f;
    private bool _autoAdvanceDialogue = false;
    readonly CoroutineRunner _coroutineRunner;
    readonly DialogueParser _dialogueParser;
    readonly CharacterManager _characterManager;
    readonly MemoryController _memoryManager;
    readonly SaveStateController _saveGameController;

    public DialogueController(CoroutineRunner coroutineRunner, 
                              DialogueParser dialogueParser, 
                              CharacterManager characterManager, 
                              MemoryController memoryManager, 
                              SaveStateController saveGameController)
    {
        _coroutineRunner = coroutineRunner;
        _dialogueParser = dialogueParser;
        _characterManager = characterManager;
        _memoryManager = memoryManager;
        _saveGameController = saveGameController;

        init();
    }

    private void init()
    {
        dialogueComplete = new Signal<Dialogue>();
        _dialoguesToSave = new List<DialogueNode>();

        GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(DIALOGUE_PREFAB));
        _dialogueView = prefab.GetComponent<DialogueView>();
        _dialogueView.gameObject.SetActive(false);
        _dialogueView.backgroundClick.Add(handleBackgroundClicked);

        _dialogues = new Dictionary<string, Dialogues>();
    }

    public void restore(List<DialogueNode> dialogues)
    {
        foreach(DialogueNode dialogueNode in dialogues)
        {
            if (!Mathf.Approximately(dialogueNode.status, 0f))
            {
                _characterManager.addStatus(new CharacterStatus(dialogueNode.character, dialogueNode.id, dialogueNode.status));
            }
            // track any memory outcomes that result from this node.
            if (!string.IsNullOrEmpty(dialogueNode.outcome))
            {
                _memoryManager.addOutcome(new MemoryOutcome(dialogueNode.character, dialogueNode.outcome, dialogueNode.memoryId));
            }
        }
    }

    public void reset()
    {
        _characterManager.clearStatus();
        _memoryManager.clearMemories();
    }

    public bool showingDialog
    {
        get
        {
            return _dialogueView.isActiveAndEnabled;
        }
    }

    public void start(string dialoguesId, string memoryId = null)
    {
        if (!_dialogues.ContainsKey(dialoguesId))
        {
            loadDialogues(dialoguesId);
        }
        _currentDialogue = _dialogues[dialoguesId].start;
        _currentMemoryId = memoryId;
        _dialogueView.hideAll();
        _dialogueView.clearChoices();
        displayNode(_currentDialogue.tree.root);
        _dialogueView.show();
    }

    public void stop()
    {
        _dialogueView.hideAll();
        _coroutineRunner.StopDelayedUpdateAction();
    }

    public void pause()
    {
        _coroutineRunner.PauseDelayedUpdateAction = true;
    }

    public void resume()
    {
        _coroutineRunner.PauseDelayedUpdateAction = false;
    }

    private void loadDialogues(string id)
    {
        TextAsset xml = (TextAsset)Resources.Load(DIALOGUE_PATH + id);
        Dialogues dialogues = _dialogueParser.parse(xml.text);
        _dialogues[dialogues.id] = dialogues;
    }

    private void displayNode(LeafNode node)
    {
        _currentNode = node;
        DialogueNode dialogueNode = _currentNode.nodeData as DialogueNode;

        if (dialogueNode.character != null && (_currentCharacter == null || _currentCharacter != null && dialogueNode.character != _currentCharacter.characterId))
        {
            _currentCharacter = _characterManager.getBio(dialogueNode.character);

            if (_currentCharacter == null)
            {
                Debug.LogError("Character Bio null for character '" + dialogueNode.character + "' in node '" + dialogueNode.id + "'.");
            }
        }

        // Update / show a character portrait for eligible dialogue nodes.
        if (dialogueNode.type != DialogueNode.Type.Description)
        {
            if (dialogueNode.emotion != _currentEmotion)
            {
                _currentEmotion = dialogueNode.emotion;
            }

            bool isRig = _currentCharacter != null && _currentCharacter.rig;

            if (!string.IsNullOrEmpty(dialogueNode.character))
            {
                if (isRig)
                {
                    _dialogueView.portrait.displayRig(dialogueNode.character, dialogueNode.emotion);
                }
                else
                {
                    _dialogueView.portrait.display(dialogueNode.character, dialogueNode.emotion);

                    if (!_dialogueView.portrait.imageContainer.showing && _dialogueView.portrait.imageContainer.image.sprite != null)
                    {
                        _dialogueView.portrait.imageContainer.show(true);
                    }
                }
            }
            else if (!isRig)
            {
                _dialogueView.portrait.imageContainer.show(false);
            }
        }

        bool saveDialogueNode = false;

        // If this node results in a character status change record it
        if (!Mathf.Approximately(dialogueNode.status, 0f))
        {
            _characterManager.addStatus(new CharacterStatus(_currentCharacter.characterId, dialogueNode.id, dialogueNode.status));
            saveDialogueNode = true;
        }
        // track any memory outcomes that result from this node.
        if (!string.IsNullOrEmpty(dialogueNode.outcome))
        {
            if (_currentCharacter != null)
            {
                dialogueNode.memoryId = _currentMemoryId;
                _memoryManager.addOutcome(new MemoryOutcome(_currentCharacter.characterId, dialogueNode.outcome, _currentMemoryId));
                saveDialogueNode = true;
            }
            else
            {
                Debug.LogError("Not saving outcome, no player specified : " + dialogueNode.outcome);
            }
        }

        if (dialogueNode.type == DialogueNode.Type.Choice)
        {
            saveDialogueNode = true;
        }

        if (dialogueNode.type == DialogueNode.Type.Statement || dialogueNode.type == DialogueNode.Type.Description || dialogueNode.type == DialogueNode.Type.Choice)
        {
            if (dialogueNode.type == DialogueNode.Type.Description && _dialogueView.portrait.isActiveAndEnabled)
            {
                _dialogueView.portrait.imageContainer.show(false);
            }

            displayText(dialogueNode, nextNode);
        }
        else if (dialogueNode.type == DialogueNode.Type.Question)
        {
            displayText(_currentNode.nodeData as DialogueNode, () => displayChoices(_currentNode.children));
        }

        _dialogueView.showBackground(dialogueNode.type == DialogueNode.Type.Description);

        if (saveDialogueNode)
        {
            _dialoguesToSave.Add(dialogueNode);
        }
    }

    private void displayText(DialogueNode dialogueNode, System.Action advanceDialogue)
    {
        if (dialogueNode.type == DialogueNode.Type.Description)
        {
            _dialogueView.displayDescriptiveText(dialogueNode.text);
        }
        else
        {
            if (!string.IsNullOrEmpty(dialogueNode.character))
            {
                string textColor = null;
                if (_currentCharacter != null)
                {
                    textColor = _currentCharacter.textColor;
                }
                _dialogueView.displayNpcText(dialogueNode.text, textColor);
            } 
            else
            {
                // for now, assume an empty character field means it is the 'player' speaking.
                _dialogueView.displayPlayerText(dialogueNode.text, dialogueNode.type == DialogueNode.Type.Question);
            }
        }

        float displayTime = int.MaxValue;
        if (_autoAdvanceDialogue)
        {
            displayTime = Mathf.Max(dialogueNode.text.Length * TEXT_TIME_MULTIPLIER, MIN_TEXT_SECONDS);
        }
        
        _coroutineRunner.DelayUpdateAction(advanceDialogue, displayTime);
    }

    private void displayChoices(List<LeafNode> responses)
    {
        foreach (LeafNode node in responses)
        {
            if ((node.nodeData as DialogueNode).type == DialogueNode.Type.Choice)
            {
                displayChoice(node);
            }
        }

        _dialogueView.showChoices();
    }

    private void displayChoice(LeafNode node)
    {
        GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(CHOICE_PREFAB), _dialogueView.choiceContainer.transform);
        ChoiceView choiceView = prefab.GetComponent<ChoiceView>();
        DialogueNode dialogueNode = node.nodeData as DialogueNode;

        // Use the full text of the choice if a label isn't specified.
        if (string.IsNullOrEmpty(dialogueNode.label))
        {
            choiceView.textArea.text = dialogueNode.text;
        }
        else
        {
            choiceView.textArea.text = dialogueNode.label;
        }
        
        choiceView.click.AddOnce(() => handleMakeChoice(node));
    }

    private void handleMakeChoice(LeafNode node)
    {
        _dialogueView.showChoices(false);
        _dialogueView.clearChoices();
        displayNode(node);
    }

    private void nextNode()
    {
        DialogueNode dialogueNode = _currentNode.nodeData as DialogueNode;

        if (_currentNode.children != null && _currentNode.children.Count > 0 && dialogueNode.type != DialogueNode.Type.Action)
        {
            displayNode(_currentNode.children[0]);
        }
        else
        {
            saveDialogues();
            _dialogueView.show(false);
            dialogueComplete.Dispatch(_currentDialogue);
        }
    }

    private void saveDialogues()
    {
        if (_dialoguesToSave.Count > 0)
        {
            _saveGameController.saveDialogues(_dialoguesToSave);
            _dialoguesToSave.Clear();
        }

        if (_currentMemoryId != null)
        {
            _saveGameController.saveMemoryId(_currentMemoryId);
            _currentMemoryId = null;
        }
    }

    private void handleBackgroundClicked()
    {
        if (_coroutineRunner.RunningDelayedUpdateAction)
        {
            _coroutineRunner.RunDelayedUpdateActionNow();
        }
    }
}

public class Dialogues
{
    public string id;
    public string startDialogueId;
    public Dictionary<string, Dialogue> dialogues;

    public Dialogues(string id, string startDialogueId)
    {
        this.id = id;
        this.startDialogueId = startDialogueId;
    }

    public Dialogue start
    {
        get
        {
            return dialogues[startDialogueId];
        }
    }
}
