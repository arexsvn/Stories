using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class InboxView : MonoBehaviour
{
    public Transform inboxItemContainer;
    public Transform inboxListItemContainer;
    public InboxItemView inboxItemPrefab;
    public InboxListItemView inboxListItemPrefab;
    public TextMeshProUGUI titleText;
    public Button deleteReadMessages;
    public Button closeButton;
    public CanvasGroup canvasGroup;

    public void show(bool show = true, float fadeTime = -1)
    {
        UITransitions.fade(gameObject, canvasGroup, !show, false, fadeTime);
    }

    public void removeInboxItems()
    {
        foreach (Transform child in inboxItemContainer.transform)
        {
            Object.Destroy(child.gameObject);
        }
    }

    public void removeInboxListItems()
    {
        foreach (Transform child in inboxListItemContainer.transform)
        {
            Object.Destroy(child.gameObject);
        }
    }
}
