using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogView : MonoBehaviour 
{
	public TextMeshProUGUI titleText;
	public TextMeshProUGUI descriptionText;
	public GameObject buttonContainer;
	public CanvasGroup canvasGroup;
    public GameObject entryContainer;

	public void fadeIn()
	{
		UITransitions.fade(gameObject, canvasGroup, false);
	}

	public void fadeOut(float fadeTime = -1f, bool destroyOnComplete = true)
	{
		UITransitions.fade(gameObject, canvasGroup, true, destroyOnComplete, fadeTime);
	}

    public void clearButtons()
    {
        foreach (Transform child in buttonContainer.transform)
        {
            Object.Destroy(child.gameObject);
        }
    }
}
