using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System;

public interface IAssetService
{
    Task<T> Instantiate<T>(Transform container = null, string assetName = null, CancellationToken cancellationToken = default);
    Task<GameObject> Instantiate(string assetName, Transform container = null, CancellationToken cancellationToken = default);
    void DisposeAsset(GameObject gameObject);
    event Action<string> LoadError;
}
