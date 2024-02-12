using UnityEngine;
using System.Collections.Generic;
using TMPro;
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
    public Button backgroundButton;
    public CanvasGroup backgroundCanvasGroup;
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
        showChoices(false, 0);
        showBackground(false, 0);
        show(false, 0);
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
            Object.Destroy(child.gameObject);
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

    public void showChoices(bool show = true, float fadeTime = -1)
    {
        UITransitions.fade(choiceContainer, choiceCanvasGroup, !show, false, fadeTime);
    }

    public void show(bool show = true, float fadeTime = -1)
    {
        UITransitions.fade(gameObject, canvasGroup, !show, false, fadeTime);
    }

    public void showBackground(bool show = true, float fadeTime = -1)
    {
        if (_showingBackground == show)
        {
            return;
        }
        _showingBackground = show;
        UITransitions.fade(gameObject, backgroundCanvasGroup, !show, false, fadeTime, false);
    }

    private void OnBackgroundClick()
    {
        backgroundClick.Dispatch();
    }
}
