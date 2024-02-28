using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// UIViewManager
///
/// Handles the instantiation of views through an IAssetService.
/// Will manage the lifecycle of views including back button handling via UIViewStack.
/// </summary>
public class UIViewManager : IDisposable
{
	public event Action<string> ViewLoadError;
	public event Action<IUIView> ViewShown;
	public event Action<IUIView> ViewHidden;
	public IUICanvas UiCanvas { get => _uiCanvas; }

	private const string UI_CANVAS_KEY = "MainCanvas";
	private IUICanvas _uiCanvas;
	private Dictionary<string, IUIView> _viewCache;
	private Dictionary<IUIView, List<GameObject>> _viewChildAssets;
	
    private readonly IAssetService _assetService;
    private readonly UIViewStack _uiViewStack;

    public UIViewManager(IAssetService assetService, UIViewStack uiViewStack)
    {
		_assetService = assetService;
		_assetService.LoadError += ViewLoadError;
		_uiViewStack = uiViewStack;
    }

	public async Task Init(IUICanvas uiCanvas = null)
    {
	    if (uiCanvas == null)
	    {
			_uiCanvas = await _assetService.Instantiate<IUICanvas>(null, UI_CANVAS_KEY);
	    }
	    else
	    {
		    _uiCanvas = uiCanvas;
	    }

		_viewCache = new Dictionary<string, IUIView>();
		_viewChildAssets = new Dictionary<IUIView, List<GameObject>>();
    }

	/// <summary>
	/// Get a cached view.
	/// <param name="assetName">Can be used to override the default asset name that is derived from Type name.</param>
	/// </summary>
	public IUIView GetCachedView(string assetName)
	{
		if (_viewCache.ContainsKey(assetName))
		{
			return _viewCache[assetName];
		}

		return null;
	}

	public T GetCachedView<T>()
	{
		return (T)GetCachedView(typeof(T).Name);
	}

	public T GetViewInHierarchy<T>(string assetName = null)
	{
		IUIView[] views = _uiCanvas.BaseContainer.GetComponentsInChildren<IUIView>(true);
		foreach (IUIView view in views)
		{
			if (view.GetType() == typeof(T) && (string.IsNullOrEmpty(assetName) || assetName == GetViewName(view)))
			{
				return (T)view;
			}
		}

		return default;
	}
	
	/// <summary>
	/// Show a view. Will instantiate and parent to the correct canvas transform. 'UIView.ifFullScreen=true' can be used for
	///    'fullscreen' Screens and 'UIView.ifFullScreen=false' will treat the view as a dialog.
	/// <param name="assetName">Asset name of view, will be used to lookup the asset.</param>
	/// <param name="animate">(optional, default 'true') Animate the view opening.</param>
	/// <param name="cache">(optional, default 'true') Should the view stay cached or destroyed when closed.</param>
	/// </summary>
	public async Task<IUIView> ShowViewAsync(string assetName, bool animate = true, bool cache = true, CancellationToken cancellationToken = default)
	{
		Debug.Log($"UIViewManager::ShowViewAsync : Showing view {assetName}");
		
		if (string.IsNullOrEmpty(assetName))
		{
			Debug.LogError("UIViewManager::ShowViewAsync: Cannot show view, null or empty assetName.");
			return null;
		}
		
		IUIView newView = null;
		if (cache)
        {
			newView = GetCachedView(assetName);
		}

		if (newView == null)
		{
			GameObject viewGameObject = await _assetService.Instantiate(assetName, null, cancellationToken);
            newView = viewGameObject.GetComponent<IUIView>();

			if (newView == null)
			{
				Debug.LogError($"UIViewManager :: View '{assetName}' does not have a component that implements IUIView so will not be added to the canvas.");
                _assetService.DisposeAsset(viewGameObject);
				return null;
			}

			Transform layer = _uiCanvas.DialogLayer;
			if (newView.IsFullScreen)
			{
				layer = _uiCanvas.ScreenLayer;
			}
			viewGameObject.transform.SetParent(layer, false);

            newView.OnShown += HandleViewShown;

            if (cache)
			{
                newView.OnHidden += HandleViewHidden;
				_viewCache[assetName] = newView;
			}
			else
			{
				newView.OnHidden += HandleTemporaryViewClosed;
			}
		}

		ShowView(newView, animate, cache);

		return newView;
	}
	
	public async Task<T> ShowViewAsync<T>(bool animate = true, bool cache = true, CancellationToken cancellationToken = default, string assetName = null)
	{
		if (assetName == null)
		{
			assetName = typeof(T).Name;
		}

		return (T)await ShowViewAsync(assetName, animate, cache, cancellationToken);
	}

	/// <summary>
	/// Show an already instantiated view. Will hide existing views as needed and move the view to the top of the stack.
	/// <param name="view">Instantiated view to show.</param>
	/// <param name="animate">(optional, default 'true') Animate the view hiding.</param>
	/// <param name="addToStack">(optional, default 'true') Add this to the stack for back navigation.</param>
	/// </summary>
	public void ShowView(IUIView view, bool animate = true, bool addToStack = true)
	{
        if (addToStack)
        {
            _uiViewStack.AddViewToStack(view);
        }
        else if (view.IsFullScreen)
		{
            // If the new view is full screen, hide all the views beneath it.
            HideAllViews(_uiCanvas.DialogLayer);
			HideAllViews(_uiCanvas.ScreenLayer);
		}
		// Ensure the gameobject is at the top of the heirarchy in the view container.
		GameObject viewGameObject = (view as MonoBehaviour).gameObject;
		viewGameObject.transform.SetAsLastSibling();

		view.Show(animate);
	}
	
	/// <summary>
	/// Hide a cached view.
	/// <param name="assetName">Asset name of view, will be used to lookup the asset from the cache.</param>
	/// <param name="animate">(optional, default 'true') Animate the view hiding.</param>
	/// </summary>
	public void HideView(string assetName, bool animate = true)
	{
		IUIView uiView = GetCachedView(assetName);

		if (uiView != null)
		{
			uiView.Hide(animate);
		}
	}

	public void HideView<T>(bool animate = true, string assetName = null)
    {
		if (assetName == null)
		{
			assetName = typeof(T).Name;
		}

		HideView(assetName, animate);
	}

	/// <summary>
	/// Load a view. Will not be added to view stack or added to one of the standard layer transforms.
	/// <param name="assetName">Asset name.</param>
	/// <param name="parentView"> The parent view associated with this asset. If parent is destroyed, this asset will be as well.</param>
	/// <param name="container"> (optional) Transform we should load into. If left 'null' will NOT be added to any transform.</param>
	/// <param name="cancellationToken">(optional) Token to cancel the load.</param>
	/// </summary>
	public async Task<GameObject> InstantiateAsset(string assetName, Transform container = null, IUIView parentView = null, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(assetName))
        {
			Debug.LogError("UIViewManager::InstantiateAndInjectAsset: Cannot instantiate, null or empty assetName.");
			return null;
        }
		
		GameObject viewGameObject = await _assetService.Instantiate(assetName, container, cancellationToken);

        if (parentView != null)
		{
			if (!_viewChildAssets.ContainsKey(parentView))
			{
				_viewChildAssets[parentView] = new List<GameObject>();
			}
			_viewChildAssets[parentView].Add(viewGameObject);
		}
		
		return viewGameObject;
	}

    /// <summary>
    /// Show a non-screen/non-dialog view.. Will not be added to view stack to added to one of the standard layer transforms.
    /// <param name="container"></param>
    /// <param name="parentView"></param>
    /// <param name="assetName"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// </summary>
    public async Task<T> InstantiateAsset<T>(Transform container = null, IUIView parentView = null, string assetName = null, CancellationToken cancellationToken = default)
	{
		if (assetName == null)
		{
			assetName = typeof(T).Name;
		}

		GameObject viewGameObject = await InstantiateAsset(assetName, container, parentView, cancellationToken); ;

		return viewGameObject.GetComponent<T>();
	}

	/// <summary>
	/// Destroy all views on the dialog layer.
	/// </summary>
	public void DestroyAllDialogs()
    {
		HideAllViews(_uiCanvas.DialogLayer, true);
    }

	/// <summary>
	/// Hide all views.
	/// <param name="layer">(optional, defaults to 'all' layers) A transform indicating the layer to hide views on.</param>
	/// <param name="destroy">(optional, defaults to 'false') Should the view be destroyed or left in the cache?</param>
	/// </summary>
	public void HideAllViews(Transform layer = null, bool destroy = false)
	{
		if (layer == null)
		{
			if (_uiCanvas != null && _uiCanvas.BaseContainer != null)
			{
				layer = _uiCanvas.BaseContainer.transform;
			}
			else
			{
				return;
			}
		}

		IUIView[] views = layer.GetComponentsInChildren<IUIView>(true);
		foreach (IUIView view in views)
		{
			if (destroy)
            {
				DisposeView(view);
			}
			else
            {
				view.Hide(false);
            }
		}
	}

	/// <summary>
	/// Destroy a specific view based on type T.
	/// </summary>
	public void DisposeView<T>()
	{
		IUIView cachedView = GetCachedView<T>() as IUIView;
		if (cachedView != null)
        {
			DisposeView(cachedView);
			return;
		}

		IUIView[] views = _uiCanvas.BaseContainer.GetComponentsInChildren<IUIView>(true);
		foreach (IUIView view in views)
		{
			if (view.GetType() == typeof(T))
            {
				DisposeView(view);
			}
		}
	}

	/// <summary>
	/// Cleans up and disposes a specific view.
	/// </summary>
	public void DisposeView(IUIView view)
    {
		view.OnHidden -= HandleViewHidden;
		view.OnHidden -= HandleTemporaryViewClosed;
        view.OnShown -= HandleViewShown;

        _uiViewStack.RemoveViewFromStack(view);

        string assetName = view.GetType().Name;
        if (_viewCache.ContainsKey(assetName))
		{
			_viewCache.Remove(assetName);
		}
		
		if (view is MonoBehaviour behaviour)
		{
			if (_viewChildAssets.ContainsKey(view))
			{
				foreach (GameObject childAsset in _viewChildAssets[view])
				{
                    _assetService.DisposeAsset(childAsset);
				}
				_viewChildAssets.Remove(view);
			}
            _assetService.DisposeAsset(behaviour.gameObject);
		}
    }

	/// <summary>
	/// Check if any views are active on the dialog layer.
	/// </summary>
	public bool IsDialogActive()
	{
		for (int n = 0; n < _uiCanvas.DialogLayer.childCount; n++)
		{
			Transform transform = _uiCanvas.ScreenLayer.GetChild(n);
			if (transform.gameObject.activeInHierarchy)
            {
				return true;
            }
		}

		return false;
	}

    private void HandleViewShown(IUIView view)
    {
        ViewShown?.Invoke(view);
    }

    private void HandleViewHidden(IUIView view)
    {
		ViewHidden?.Invoke(view);
	}

	private void HandleTemporaryViewClosed(IUIView view)
	{
		ViewHidden?.Invoke(view);
		DisposeView(view);
	}

    /// <summary>
    /// Implementation of disposable pattern
    /// https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
    /// </summary>
    private bool _disposed = false;

    ~UIViewManager() => Dispose(false);

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
				HideAllViews();
				
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

			ViewLoadError = null;
			ViewShown = null;
			ViewHidden = null;

			_disposed = true;
		}
	}

	private string GetViewName(IUIView view)
	{
		string viewName = null;
		
		MonoBehaviour comp = view as MonoBehaviour;
		if (comp != null && comp.gameObject != null)
		{
			string removeString = "(Clone)";
			string sourceString = comp.gameObject.name;
			int index = sourceString.IndexOf(removeString, StringComparison.Ordinal);
			viewName = index < 0 ? sourceString : sourceString.Remove(index, removeString.Length);
		}

		return viewName;
	}
}

public class UICanvasWrapper : IUICanvas
{
	public RectTransform ScreenLayer { get; set; }
	public RectTransform DialogLayer { get; set; }
	public Transform BaseContainer { get; set;}
}
