using VContainer;

class Kitchen : CustomSceneController
{
    [Inject]
    readonly UIController _uiController;

    public override void handleDialogueComplete(string id)
    {
        if (id == "evening_decision_leave" || id == "evening_decision_stay")
        {
            _uiController.showJournal(true);
        }
    }
}
