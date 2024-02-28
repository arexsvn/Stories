public class TextOverlayController
{
    private TextOverlayView _view;
    private bool _showing = false;
    private readonly AddressablesAssetService _assetService;

    public TextOverlayController(AddressablesAssetService assetService)
    {
        _assetService = assetService;
        init();
    }

    public async void init()
    {
        _view = await _assetService.InstantiateAsync<TextOverlayView>();
        _view.canvasGroup.alpha = 0f;
        setText("");
    }

    public void setText(string text)
    {
        _view.textField.text = text;
    }

    public void clearText()
    {
        _view.textField.text = "";
    }

    public void show(bool show = true)
    {
        _showing = show;
        _view.show(show);
    }

    public bool showing
    {
        get
        {
            return _showing;
        }
    }
}
