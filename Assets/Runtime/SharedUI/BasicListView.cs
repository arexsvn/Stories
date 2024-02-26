using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BasicListView : MonoBehaviour, IUIView
{
    public TextMeshProUGUI titleText;
    public Button closeButton;
    public Button background;
    public RectTransform listContainer;
    public BasicButtonView listItemPrefab;
    public CanvasGroup canvasGroup;

    public event System.Action<IUIView> OnShown;
    public event System.Action<IUIView> OnHidden;

    public void Set(string title, List<DialogButtonData> listItems, bool showCloseButton = true, UnityAction closeAction = null)
    {
        titleText.text = title;

        UpdateList(listItems);

        closeButton.gameObject.SetActive(showCloseButton);

        if (closeAction != null)
        {
            closeButton.onClick.AddListener(closeAction);
            background.onClick.AddListener(closeAction);
        }
        else
        {
            closeButton.onClick.AddListener(() => Hide());
            background.onClick.AddListener(() => Hide());
        }

        Show();
    }

    public void Show(bool animate = true)
    {
        gameObject.SetActive(true);
        if (animate)
        {
            UITransitions.fade(gameObject, canvasGroup, false, false, UITransitions.FADE_TIME, false, go => { OnShown?.Invoke(this);  });
        }
        else
        {
            canvasGroup.alpha = 1f;
            OnShown?.Invoke(this);
        }
    }

    public void Hide(bool animate = true)
    {
        if (animate)
        {
            UITransitions.fade(gameObject, canvasGroup, true, false, UITransitions.FADE_TIME, true, go => { OnHidden?.Invoke(this); });
        }
        else
        {
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            OnHidden?.Invoke(this);
        }
    }

    public void Init()
    {
        gameObject.SetActive(false);
    }

    public void ClearList()
    {
        foreach (Transform child in listContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateList(List<DialogButtonData> listItems)
    {
        UIUtils.RecycleOrCreateItems(listContainer, listItemPrefab, listItems, setupListItem);
    }

    private void setupListItem(BasicButtonView view, DialogButtonData data)
    {
        view.button.onClick.RemoveAllListeners();
        view.labelText.text = data.label;

        if (data.action != null)
        {
            view.button.onClick.AddListener(data.action);
        }

        if (data.closeOnAction)
        {
            view.button.onClick.AddListener(() => Hide());
        }
    }

    private void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    public bool IsFullScreen { get => false; set { } }
}