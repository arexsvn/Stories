using UnityEngine;
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

    private void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    public void show(bool show = true)
    {
        if (show)
        {
            UITransitions.fadeIn(canvasGroup, gameObject);
        }
        else
        {
            UITransitions.fadeOut(canvasGroup, gameObject);
        }
    }

    public void removeInboxItems()
    {
        foreach (Transform child in inboxItemContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void removeInboxListItems()
    {
        foreach (Transform child in inboxListItemContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
