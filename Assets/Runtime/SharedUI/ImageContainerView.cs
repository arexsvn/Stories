using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageContainerView : MonoBehaviour
{
    public Image image;
    public CanvasRenderer imageRenderer;
    public CanvasGroup canvasGroup;
    private Texture _currentTexture;
    private bool _showing = true;

    public void loadRemoteImage(string assetId, RemoteAssetLoader remoteAssetLoader)
    {
        RemoteAssetRequest request = new RemoteAssetRequest();
        request.assetId = assetId;
        request.complete.AddOnce((UnityWebRequest assetRequest) => addTexture(assetRequest));

        remoteAssetLoader.sendRequest(request);
    }

    public void loadImageResource(string localResourcePath)
    {
        image.sprite = Resources.Load<Sprite>(localResourcePath);
    }

    public void clearImage()
    {
        _currentTexture = null;
        imageRenderer.Clear();
    }

    public void show(bool show = true, float fadeTime = -1)
    {
        _showing = show;

        float alpha = 1f;

        if (!show)
        {
            alpha = 0f;
        }

        if (canvasGroup != null)
        {
            UITransitions.fade(canvasGroup, alpha, fadeTime, gameObject, null, !show);
        }
        else
        {
            UITransitions.fade(image, alpha, fadeTime, gameObject, null, !show);
        }
    }

    private void addTexture(UnityWebRequest request)
    {
        setTexture(DownloadHandlerTexture.GetContent(request));
    }

    public void setTexture(Texture texture)
    {
        _currentTexture = texture;
        imageRenderer.SetTexture(_currentTexture);
    }

    public bool showing
    {
        get
        {
            return _showing;
        }
    }

    /***/
    // Some code to deal with canvas renderers losing their textures when disabled/enabled.
    /*
    private bool _textureRefresh = false;

    private void refreshTexture()
    {
		if (_currentTexture != null)
		{
			imageRenderer.SetTexture(_currentTexture);
		}
    }

    void OnEnable()
    {
		_textureRefresh = true;
    }

    void LateUpdate()
    {
		if (_textureRefresh)
		{
			refreshTexture();
			_textureRefresh = false;
		}
    }
    */
    /***/
}
