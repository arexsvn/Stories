using UnityEngine;
using System.Collections.Generic;
using signals;
using Ink.Runtime;
using System;

public class InkDialogueController
{
    public Signal<Dialogue> dialogueComplete;
    private Dictionary<string, Dialogues> _dialogues;
    private DialogueView _dialogueView;
    private LeafNode _currentNode;
    private Dialogue _currentDialogue;
    private CharacterBio _currentCharacter;
    private string _currentEmotion;
    private string _currentStoryId;
    private List<DialogueNode> _dialoguesToSave;
    public static string DIALOGUE_PATH = "data/dialogues/";
    private static float TEXT_TIME_MULTIPLIER = 0.065f;
    private static float MIN_TEXT_SECONDS = 2f;
    private bool _autoAdvanceDialogue = false;
    private Story _currentStory;
    private bool _storyFinished = false;
    readonly CoroutineRunner _coroutineRunner;
    readonly DialogueParser _dialogueParser;
    readonly CharacterManager _characterManager;
    readonly MemoryController _memoryManager;
    readonly SaveStateController _saveGameController;
    readonly AddressablesAssetService _assetService;

    public InkDialogueController(CoroutineRunner coroutineRunner, 
                              DialogueParser dialogueParser, 
                              CharacterManager characterManager, 
                              MemoryController memoryManager, 
                              SaveStateController saveGameController,
                              AddressablesAssetService assetService)
    {
        _coroutineRunner = coroutineRunner;
        _dialogueParser = dialogueParser;
        _characterManager = characterManager;
        _memoryManager = memoryManager;
        _saveGameController = saveGameController;
        _assetService = assetService;

        init();
    }

    private async void init()
    {
        dialogueComplete = new Signal<Dialogue>();
        _dialoguesToSave = new List<DialogueNode>();

        _dialogueView = await _assetService.InstantiateAsync<DialogueView>();
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

    public void Reset()
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

    public async void Start(string storyId)
    {
        /*
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
        */
        TextAsset storyText = await _assetService.LoadAsync<TextAsset>(storyId);
        if (storyText == null)
        {
            Debug.LogError($"InkDialogController :: start : storyId '{storyId}' TextAsset is null!");
            return;
        }

        _storyFinished = false;
        _currentStoryId = storyId;
        _currentStory = new Story(storyText.text);

        //_currentDialogue = _dialogues[dialoguesId].start;
        _dialogueView.hideAll();
        _dialogueView.clearChoices();
        //displayNode(_currentDialogue.tree.root);
        continueStory();
        _dialogueView.show();
    }

    private void continueStory()
    {
        string descriptiveText = "";
        while (_currentStory.canContinue)
        {
            // Continue gets the next line of the story
            string text = _currentStory.Continue();
            // This removes any white space from the text.
            descriptiveText += text.Trim() + "\n";
        }

        // Display the text on screen!
        _dialogueView.displayDescriptiveText(descriptiveText);

        // Display all the choices, if there are any!
        if (_currentStory.currentChoices.Count > 0)
        {
            for (int i = 0; i < _currentStory.currentChoices.Count; i++)
            {
                Choice choice = _currentStory.currentChoices[i];
                displayChoice(choice);
            }
        }
        // If we've read all the content and there's no choices, the story is finished!
        else
        {
            _storyFinished = true;
            /*
            Button choice = CreateChoiceView("End of story.\nRestart?");
            choice.onClick.AddListener(delegate {
                StartStory();
            });
            */
        }
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
                dialogueNode.memoryId = _currentStoryId;
                _memoryManager.addOutcome(new MemoryOutcome(_currentCharacter.characterId, dialogueNode.outcome, _currentStoryId));
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

    private void displayText(DialogueNode dialogueNode, Action advanceDialogue)
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

    private async void displayChoice(LeafNode node)
    {
        ChoiceView choiceView = await _assetService.InstantiateAsync<ChoiceView>(_dialogueView.choiceContainer.transform);
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

    private async void displayChoice(Choice choice)
    {
        ChoiceView choiceView = await _assetService.InstantiateAsync<ChoiceView>(_dialogueView.choiceContainer.transform);
        choiceView.textArea.text = choice.text.Trim();

        choiceView.click.AddOnce(() => handleMakeChoice(choice));

        _dialogueView.showChoices();
    }

    private void handleMakeChoice(Choice choice)
    {
        _dialogueView.showChoices(false);
        _dialogueView.clearChoices();
        _currentStory.ChooseChoiceIndex(choice.index);
        continueStory();
        //displayNode(node);
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

        if (_currentStoryId != null)
        {
            _saveGameController.saveMemoryId(_currentStoryId);
            _currentStoryId = null;
        }
    }

    private void handleBackgroundClicked()
    {
        if (_coroutineRunner.RunningDelayedUpdateAction)
        {
            _coroutineRunner.RunDelayedUpdateActionNow();
        }

        if (_storyFinished)
        {
            _dialogueView.hideAll();
        }
    }
}
