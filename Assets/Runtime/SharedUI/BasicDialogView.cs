using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BasicDialogView : MonoBehaviour, IUIView
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button closeButton;
    public Button background;
    public RectTransform buttonContainer;
    public BasicButtonView buttonPrefab;
    public CanvasGroup canvasGroup;

    public event System.Action<IUIView> OnShown;
    public event System.Action<IUIView> OnHidden;

    public void Set(string title, string message = null, List<DialogButtonData> buttons = null, bool showCloseButton = true, UnityAction closeAction = null)
    {
        titleText.text = title;

        if (!string.IsNullOrEmpty(message))
        {
            messageText.text = message;
        }

        if (buttons != null)
        {
            UIUtils.RecycleOrCreateItems(buttonContainer, buttonPrefab, buttons, setupButton);
        }

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
            UITransitions.fade(gameObject, canvasGroup, false, false, UITransitions.FADE_TIME, false, go => { OnShown?.Invoke(this); });
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

    protected void setupButton(BasicButtonView view, DialogButtonData data)
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