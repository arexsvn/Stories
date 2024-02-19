
public interface IUIView
{
    event System.Action<IUIView> OnShown;
    event System.Action<IUIView> OnHidden;
    void Show(bool animate = true);
    void Hide(bool animate = true);
    bool IsFullScreen { get; }
}
