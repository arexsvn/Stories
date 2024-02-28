using UnityEngine;
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
		UITransitions.fadeIn(canvasGroup, gameObject);
	}

	public void fadeOut(float fadeTime = -1f, bool destroyOnComplete = true)
	{
		UITransitions.fade(canvasGroup, 0f, fadeTime, gameObject, gameObject =>
		{
			if (destroyOnComplete)
			{
                Destroy(gameObject);
            }
		});
	}

    public void clearButtons()
    {
        foreach (Transform child in buttonContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
