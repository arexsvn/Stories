using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

public class BasicInputDialogView : BasicDialogView
{
    public TMP_InputField inputText;
    public TextMeshProUGUI placeholderText;
        
    public void Set(string title, string message = null, string placeholderMessage = null, List<DialogButtonData> buttons = null, bool allowClose = true, IUITransitions uiTransitions = null)
    {
        if (!string.IsNullOrEmpty(placeholderMessage))
        {
            placeholderText.text = placeholderMessage;
        }

        base.Set(title, message, buttons, allowClose, uiTransitions);
    }

    public string GetInput()
    {
        return inputText.text;
    }
}