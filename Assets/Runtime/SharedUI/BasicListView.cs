using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BasicListView : BasicDialogView
{
    public RectTransform listContainer;
    public BasicButtonView listItemPrefab;

    public void Set(string title, List<DialogButtonData> listItems, bool showCloseButton = true, UnityAction closeAction = null)
    {
        base.Set(title, null, listItems, showCloseButton, closeAction);
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
        UIUtils.RecycleOrCreateItems(listContainer, listItemPrefab, listItems, setupButton);
    }
}