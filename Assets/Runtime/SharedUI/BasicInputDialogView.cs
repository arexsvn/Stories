using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

public class BasicInputDialogView : BasicDialogView
{
    public TMP_InputField inputText;
    public TextMeshProUGUI placeholderText;
        
    public void Set(string title, string message = null, string placeholderMessage = null, List<DialogButtonData> buttons = null, bool showCloseButton = true, UnityAction closeAction = null)
    {
        titleText.text = title;

        if (!string.IsNullOrEmpty(message))
        {
            messageText.text = message;
        }

        if (!string.IsNullOrEmpty(placeholderMessage))
        {
            placeholderText.text = placeholderMessage;
        }

        if (buttons != null)
        {
            UIUtils.RecycleOrCreateItems(buttonContainer, buttonPrefab, buttons, setupButton);
        }

        closeButton.gameObject.SetActive(showCloseButton);

        if (closeAction != null)
        {
            closeButton.onClick.AddListener(closeAction);
        }
        else
        {
            closeButton.onClick.AddListener(()=> Hide());
        }

        Show();
    }

    public string GetInput()
    {
        return inputText.text;
    }
}