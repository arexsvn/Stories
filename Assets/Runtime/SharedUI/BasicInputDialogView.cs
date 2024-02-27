using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

public class BasicInputDialogView : BasicDialogView
{
    public TMP_InputField inputText;
    public TextMeshProUGUI placeholderText;
        
    public void Set(string title, string message = null, string placeholderMessage = null, List<DialogButtonData> buttons = null, bool showCloseButton = true, UnityAction closeAction = null)
    {
        if (!string.IsNullOrEmpty(placeholderMessage))
        {
            placeholderText.text = placeholderMessage;
        }

        base.Set(title, message, buttons, showCloseButton, closeAction);
    }

    public string GetInput()
    {
        return inputText.text;
    }
}