using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class JournalView : MonoBehaviour
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
        if (!animate)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            UITransitions.fadeIn(canvasGroup, gameObject);
        }
    }

    public void Hide(bool animate = true)
    {
        if (!animate)
        {
            canvasGroup.alpha = 0f;
        }
        else
        {
            UITransitions.fadeOut(canvasGroup, gameObject);
        }
    }
}
