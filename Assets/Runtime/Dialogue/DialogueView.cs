using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using signals;

public class DialogueView : MonoBehaviour
{
    public Signal backgroundClick = new Signal();
    public PortraitView portrait;
    public TextOverlayView npcTextDisplay;
    public TextOverlayView playerTextDisplay;
    public TextOverlayView descriptiveTextDisplay;
    public GameObject choiceContainer;
    public CanvasGroup canvasGroup;
    public CanvasGroup choiceCanvasGroup;
    public CanvasGroup backgroundCanvasGroup;
    public Button backgroundButton;
    public bool _showingBackground = true;
    private List<TextOverlayView> _textOverlays;
    private Vector3 playerTextPositionStandard;
    private Vector3 playerTextPositionQuestion;

    void Awake()
    {
        backgroundButton.onClick.AddListener(OnBackgroundClick);
        _textOverlays = new List<TextOverlayView> { npcTextDisplay, playerTextDisplay, descriptiveTextDisplay };
        playerTextPositionStandard = playerTextDisplay.transform.position;
        playerTextPositionQuestion = descriptiveTextDisplay.transform.position;

        hideAll();
    }

    public void hideAll()
    {
        portrait.imageContainer.show(false, 0);
        showNpcText(false, 0);
        showDescriptiveText(false, 0);

        choiceCanvasGroup.alpha = 0f;
        choiceContainer.SetActive(false);

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        backgroundButton.onClick.RemoveListener(OnBackgroundClick);
        backgroundClick.RemoveAll();
    }

    public void clearChoices()
    {
        foreach (Transform child in choiceContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void showNpcText(bool show = true, float fadeTime = -1)
    {
        npcTextDisplay.show(show, fadeTime);
    }

    public void showPlayerText(bool show = true, float fadeTime = -1)
    {
        playerTextDisplay.show(show, fadeTime);
    }

    public void showDescriptiveText(bool show = true, float fadeTime = -1)
    {
        descriptiveTextDisplay.show(show, fadeTime);
    }

    public void displayDescriptiveText(string text)
    {
        descriptiveTextDisplay.textField.text = text;
        showOnlyTextOverlay(descriptiveTextDisplay);
    }

    public void displayNpcText(string text, string color = null)
    {
        Color characterTextColor;
        if (ColorUtility.TryParseHtmlString(color, out characterTextColor))
        {
            npcTextDisplay.textField.color = characterTextColor;
        }
        else
        {
            npcTextDisplay.restoreDefaultTextColor();
        }

        npcTextDisplay.textField.text = text;
        showOnlyTextOverlay(npcTextDisplay);
    }

    public void displayPlayerText(string text, bool question = false)
    {
        if(question)
        {
            playerTextDisplay.transform.position = playerTextPositionQuestion;
        }
        else
        {
            playerTextDisplay.transform.position = playerTextPositionStandard;
        }
        playerTextDisplay.textField.text = text;
        showOnlyTextOverlay(playerTextDisplay);
    }

    public void showOnlyTextOverlay(TextOverlayView textOverlay)
    {
        foreach (TextOverlayView nextTextOverlay in _textOverlays)
        {
            if (textOverlay == nextTextOverlay)
            {
                if (!nextTextOverlay.showing)
                {
                    nextTextOverlay.show(true);
                }
            }
            else
            {
                nextTextOverlay.show(false);
            }
        }
    }

    public void showChoices(bool show = true)
    {
        if (show)
        {
            UITransitions.fadeIn(choiceCanvasGroup, choiceContainer);
        }
        else
        {
            UITransitions.fadeOut(choiceCanvasGroup, choiceContainer, null, true);
        }
    }

    public void show(bool show = true)
    {
        if (show)
        {
            UITransitions.fadeIn(canvasGroup, gameObject);
        }
        else
        {
            UITransitions.fadeOut(canvasGroup, gameObject, null, true);
        }
    }

    public void showBackground(bool show = true)
    {
        if (_showingBackground == show)
        {
            return;
        }

        _showingBackground = show;

        if (show)
        {
            UITransitions.fadeIn(backgroundCanvasGroup);
        }
        else
        {
            UITransitions.fadeOut(backgroundCanvasGroup, backgroundCanvasGroup.gameObject, null, false);
        }
    }

    private void OnBackgroundClick()
    {
        backgroundClick.Dispatch();
    }
}
