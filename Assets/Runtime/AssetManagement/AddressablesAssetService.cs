using System;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;

public class AddressablesAssetService : IDisposable
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
    public async Task<T> InstantiateAsset<T>(Transform container = null, string assetName = null, CancellationToken cancellationToken = default)
    {
        if (assetName == null)
        {
            assetName = typeof(T).Name;
        }

        GameObject viewGameObject = await InstantiateAsset(assetName, container, cancellationToken); ;

        return viewGameObject.GetComponent<T>();
    }

    /// <summary>
    /// Instantiate an asset that needs dependency injection.
    /// <param name="assetName">Asset name.</param>
    /// <param name="parentView"> The parent view associated with this asset. If parent is destroyed, this asset will be as well.</param>
    /// <param name="container"> (optional) Transform we should load into. If left 'null' will NOT be added to any transform.</param>
    /// <param name="cancellationToken">(optional) Token to cancel the load.</param>
    /// </summary>
    public async Task<GameObject> InstantiateAsset(string assetName, Transform container = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            Debug.LogError("AssetService::InstantiateAndInjectAsset: Cannot instantiate, null or empty assetName.");
            return null;
        }

        AsyncOperationHandle<GameObject> handle = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(assetName, container);
        GameObject viewGameObject = await InstantiateAsync(handle, cancellationToken);

        return viewGameObject;
    }

    public void DisposeAsset(GameObject gameObject)
    {
        // If a GameObject is loaded via Addressables, ReleaseInstance returns 'true' and will destroy it and decrement the reference count.
        //   If it returns 'false' it needs to be Destroyed manually.
        if (!UnityEngine.AddressableAssets.Addressables.ReleaseInstance(gameObject))
        {
            Debug.LogWarning($"[AssetService] DisposeGameObject is being called on a non-addressable asset. {gameObject.name}.");
            GameObject.Destroy(gameObject);
        }
    }

    private async Task<GameObject> InstantiateAsync(AsyncOperationHandle<GameObject> handle, CancellationToken cancellationToken = default)
    {
        // It is possible this task is already done due to the asset being loaded previously. If so, do not attempt to run it again as
        //   it causes an assert.
        if (handle.Task.Status != TaskStatus.RanToCompletion)
        {
            try
            {
                await Task.Run(() => handle.Task, cancellationToken);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AssetService] LoadAssetAsync : Exception while instantiating view '{e.Message}'");
                return null;
            }
        }

        if (handle.Status == AsyncOperationStatus.Failed || handle.Result == null)
        {
            Debug.LogError($"[AssetService] Failed to instantiate asset {handle.DebugName}.");
            LoadError?.Invoke($"[AssetService] Failed to instantiate asset {handle.DebugName}.");
            return null;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            UnityEngine.AddressableAssets.Addressables.ReleaseInstance(handle);
            return null;
        }

        InjectDependencies(handle.Result);

        return handle.Result;
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
