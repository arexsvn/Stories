public class ApplicationMessage
{
    public ApplicationAction ApplicationAction;
    public ApplicationMessage(ApplicationAction action)
    {
        ApplicationAction = action;
    }
}

public enum ApplicationAction
{
    Undefined,
    Pause,
    UnPause,
    Restart,
    Quit,
    ShowMainMenu
}