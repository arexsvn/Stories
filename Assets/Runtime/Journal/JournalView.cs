using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class JournalView : MonoBehaviour, IUIView
{
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI memoryOutcomesText;
    public Transform memoryListEntries;
    public Transform characterListEntries;
    public Transform characterDetailsEntries;
    public Transform noEntriesOverlay;
    public CanvasGroup canvasGroup;
    public CharacterDetailView characterDetailEntry;
    public MemoryEntryView memoryEntry;
    public PortraitView characterPortraitEntry;
    public Button closeButton;

    public bool IsFullScreen { get => true; }

    public event Action<IUIView> OnReady;
    public event Action<IUIView> OnClosed;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    public void clearCharacters()
    {
        foreach (Transform child in characterListEntries.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void selectCharacter(string characterId)
    {
        foreach (Transform child in characterListEntries.transform)
        {
            PortraitView portrait = child.GetComponent<PortraitView>();
            portrait.darken(portrait.characterId != characterId);
        }
    }

    public void clearMemories()
    {
        foreach (Transform child in memoryListEntries.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void clearCharacterDetails()
    {
        characterNameText.text = "";
        foreach (Transform child in characterDetailsEntries.transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    public void clearMemoryOutcomes()
    {
        memoryOutcomesText.text = "";
    }

    public void Show(bool animate = true)
    {
        float fadeTime = -1;
        if (!animate)
        {
            fadeTime = 0;
        }
        UITransitions.fade(gameObject, canvasGroup, false, false, fadeTime);
    }

    public void Hide(bool animate = true)
    {
        float fadeTime = -1;
        if (!animate)
        {
            fadeTime = 0;
        }
        UITransitions.fade(gameObject, canvasGroup, true, false, fadeTime);
    }

    public void Init()
    {
        //
    }
}
