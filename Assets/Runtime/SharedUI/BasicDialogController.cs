using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class BasicDialogController
{
    private const string UI_CANVAS_KEY = "MainCanvas";
    private IUICanvas _uiCanvas;
    readonly IAssetService _assetService;
    private Dictionary<string, IUIView> _viewCache;

    public BasicDialogController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    public async Task Init(IUICanvas uiCanvas = null)
    {
        if (uiCanvas == null)
        {
            _uiCanvas = await _assetService.InstantiateAsset<IUICanvas>(null, UI_CANVAS_KEY);
        }
        else
        {
            _uiCanvas = uiCanvas;
        }

        _viewCache = new Dictionary<string, IUIView>();
    }

    public async Task<BasicDialogView> ShowDialog(string title, string message = null, List<DialogButtonData> buttons = null, string assetName = null, bool showCloseButton = true, UnityAction closeAction = null)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            assetName = typeof(BasicDialogView).Name;
        }

        BasicDialogView dialog = await GetOrInstantiateView<BasicDialogView>(assetName);
        dialog.Set(title, message, buttons, showCloseButton, closeAction);

        return dialog;
    }

    public async Task<BasicDialogView> ShowInputDialog(string title, string message = null, string placeholderInputMessage = null, List<DialogButtonData> buttons = null, string assetName = null, bool showCloseButton = true, UnityAction closeAction = null)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            assetName = typeof(BasicInputDialogView).Name;
        }

        BasicInputDialogView dialog = await GetOrInstantiateView<BasicInputDialogView>(assetName);
        dialog.Set(title, message, placeholderInputMessage, buttons, showCloseButton, closeAction);

        return dialog;
    }

    public async Task<BasicListView> ShowList(string title, List<DialogButtonData> buttons = null, string assetName = null, bool showCloseButton = true, UnityAction closeAction = null)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            assetName = typeof(BasicListView).Name;
        }

        BasicListView view = await GetOrInstantiateView<BasicListView>(assetName);
        view.Set(title, buttons, showCloseButton, closeAction);

        return view;
    }

    public void HideView<T>(string assetName = null, bool animate = true)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            assetName = typeof(T).Name;
        }

        IUIView uiView = GetCachedView(assetName);

        if (uiView != null)
        {
            uiView.Hide(animate);
        }
    }

    public async Task<T> GetOrInstantiateView<T>(string assetName) where T : IUIView
    {
        IUIView view;
        if (_viewCache.ContainsKey(assetName))
        {
            view = GetCachedView(assetName);
        }
        else
        {
            view = await _assetService.InstantiateAsset<T>(_uiCanvas.DialogLayer, assetName);
            _viewCache[assetName] = view;
        }
        return (T)view;
    }

    public IUIView GetCachedView(string assetName)
    {
        if (_viewCache.ContainsKey(assetName))
        {
            return _viewCache[assetName];
        }

        return null;
    }

    private bool _disposed = false;

    ~BasicDialogController() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _uiCanvas != null)
            {
                MonoBehaviour canvasComponent = _uiCanvas as MonoBehaviour;
                if (canvasComponent != null && canvasComponent.gameObject != null)
                {
                    _assetService.DisposeAsset(canvasComponent.gameObject);
                }
                _uiCanvas = null;
            }

            // unmanaged resources and fields that should be nulled
            if (_viewCache != null)
            {
                _viewCache.Clear();
                _viewCache = null;

            }

            _disposed = true;
        }
    }
}

public class DialogButtonData : ButtonData
{
    public bool closeOnAction = true;

    public DialogButtonData(UnityAction action, string label = null, bool closeOnAction = true, string iconResourceKey = null, string prefabResourceKey = null) : base(action, label, iconResourceKey, prefabResourceKey)
    {
        this.closeOnAction = closeOnAction;
    }
}

public class ButtonData
{
    public UnityAction action;
    public string label;
    public string prefabResourceKey;
    public string iconResourceKey;
    public bool interactable = true;
    private bool _grayOut = false;
    public string textColor;
    public string backgroundColor;
    public const string GOLD_TEXT_COLOR = "#724B1AFF";
    public const string GOLD_BG_COLOR = "#FFD96FFF";
    public const string DISABLED_TEXT_COLOR = "#808080FF";
    public const string DISABLED_BG_COLOR = "#2F3C45FF";
    public const string BLUE_BG_COLOR = "#445F80C8";
    public const string WHITE_BG_COLOR = "#FDFDFDFF";
    public const string BLUE_TEXT_COLOR = "#284461FF";
    public const string WHITE_TEXT_COLOR = "#FDFDFDFF";

    public ButtonData(UnityAction action, string label = null, string iconResourceKey = null, string prefabResourceKey = null)
    {
        this.action = action;
        this.label = label;
        this.iconResourceKey = iconResourceKey;
        this.prefabResourceKey = prefabResourceKey;
    }

    public bool disable
    {
        set
        {
            grayOut = value;
            interactable = !value;
        }
    }

    public bool grayOut
    {
        set
        {
            _grayOut = value;

            if (_grayOut)
            {
                textColor = DISABLED_TEXT_COLOR;
                backgroundColor = DISABLED_BG_COLOR;
            }
        }

        get
        {
            return _grayOut;
        }
    }

    public void highlight()
    {
        textColor = GOLD_TEXT_COLOR;
        backgroundColor = GOLD_BG_COLOR;
    }
}