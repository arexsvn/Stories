using System.Collections.Generic;

public class BasicListView : BasicDialogView
{
    public void Set(string title, List<DialogButtonData> listItems, bool allowClose = true, IUITransitions uiTransitions = null)
    {
        base.Set(title, null, listItems, allowClose, uiTransitions);
    }

    public void UpdateList(List<DialogButtonData> listItems)
    {
        UIUtils.RecycleOrCreateItems(base.buttonContainer, base.buttonPrefab, listItems, setupButton);
    }
}