using System;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;

public class AddressablesAssetService : IAssetService, IDisposable
{
    public event Action<string> LoadError;
    private readonly IObjectResolver _dependencyInjectionContainer;

    public AddressablesAssetService(IObjectResolver container)
    {
        _dependencyInjectionContainer = container;
    }

    /// <summary>
    /// Instantiate an asset that needs dependency injection.
    /// <param name="container"></param>
    /// <param name="parentView"></param>
    /// <param name="assetName"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// </summary>
    public async Task<T> Instantiate<T>(Transform container = null, string assetName = null, CancellationToken cancellationToken = default)
    {
        if (assetName == null)
        {
            assetName = typeof(T).Name;
        }

        GameObject viewGameObject = await Instantiate(assetName, container, cancellationToken);

        if (viewGameObject == null)
        {
            return default;
        }

        return viewGameObject.GetComponent<T>();
    }

    /// <summary>
    /// Instantiate an asset that needs dependency injection.
    /// <param name="assetName">Asset name / Addressable key.</param>
    /// <param name="parentView">The parent view associated with this asset. If parent is destroyed, this asset will be as well.</param>
    /// <param name="container">(optional) Transform we should load into. If left 'null' will NOT be added to any transform.</param>
    /// <param name="cancellationToken">(optional) Token to cancel the load.</param>
    /// </summary>
    public async Task<GameObject> Instantiate(string assetName, Transform container = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            Debug.LogError("AssetService::InstantiateAndInjectAsset: Cannot instantiate, null or empty assetName.");
            return null;
        }

        AsyncOperationHandle<GameObject> handle = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(assetName, container);
        try
        {
            while (!cancellationToken.IsCancellationRequested && handle.Task.Status != TaskStatus.RanToCompletion && !handle.IsDone)
            {
                await handle.Task;
            }
        }
        catch (Exception e)
        {
            LoadError?.Invoke($"[AssetService] LoadAssetAsync : Exception while instantiating view '{e.Message}'");
            return null;
        }

        if (handle.Status == AsyncOperationStatus.Failed || handle.Result == null)
        {
            LoadError?.Invoke($"[AssetService] Failed to instantiate asset {handle.DebugName}.");
            return null;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            UnityEngine.AddressableAssets.Addressables.ReleaseInstance(handle);
            return null;
        }

        GameObject viewGameObject = handle.Result;

        InjectDependencies(viewGameObject);
        
        return viewGameObject;
    }

    public void DisposeAsset(GameObject gameObject)
    {
        // If a GameObject is loaded via Addressables, ReleaseInstance returns 'true' and will destroy it and decrement the reference count.
        //   If it returns 'false' it needs to be Destroyed manually.
        if (!UnityEngine.AddressableAssets.Addressables.ReleaseInstance(gameObject))
        {
            GameObject.Destroy(gameObject);
        }
    }

    private void InjectDependencies<T>(T target)
    {
        if (_dependencyInjectionContainer != null)
        {
            try
            {
                if (target is GameObject gameObject)
                {
                    _dependencyInjectionContainer.InjectGameObject(gameObject);
                }
                else
                {
                    _dependencyInjectionContainer.Inject(target);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AssetService] InjectDependencies : Exception while injecting dependencies '{e.Message}'");
            }
        }
        else
        {
            Debug.LogError("[AssetService] InjectDependencies : Container not available for injection.");
        }
    }

    /// <summary>
    /// Implementation of disposable pattern
    /// https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
    /// </summary>
    private bool _disposed = false;

    ~AddressablesAssetService() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            LoadError = null;

            _disposed = true;
        }
    }
}
