using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BasicListView : BasicDialogView
{
    public void Set(string title, List<DialogButtonData> listItems, bool showCloseButton = true, UnityAction closeAction = null)
    {
        base.Set(title, null, listItems, showCloseButton, closeAction);
    }

    public void ClearList()
    {
        foreach (Transform child in base.buttonContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateList(List<DialogButtonData> listItems)
    {
        UIUtils.RecycleOrCreateItems(base.buttonContainer, base.buttonPrefab, listItems, setupButton);
    }
}