using System.Threading.Tasks;

public class TextOverlayController
{
    private TextOverlayView _view;
    private bool _showing = false;
    private string _currentText = "";
    private readonly AddressablesAssetService _assetService;

    public TextOverlayController(AddressablesAssetService assetService)
    {
        _assetService = assetService;

        Init();
    }

    public async Task Init()
    {
        _view = await _assetService.InstantiateAsync<TextOverlayView>();
        _view.show(_showing, 0f);
        setText(_currentText);
    }

    public void setText(string text)
    {
        _currentText = text;

        if (_view == null)
        {
            return;
        }

        _view.textField.text = text;
    }

    public void clearText()
    {
        setText("");
    }

    public void show(bool show = true)
    {
        _showing = show;

        if (_view == null) 
        {
            return;
        }

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
