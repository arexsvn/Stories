using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class UICreator
{
	private const string BUTTON_PATH = "UI/Button";
	private const string TEXT_AREA_PATH = "UI/TextArea";
	private const string DIALOG_PATH = "UI/DialogBox";
    private const string FADE_SCREEN_PREFAB = "UI/ScreenFade";
    private const string TEXT_OVERLAY_PREFAB = "UI/TextOverlay";
    private const string LIST_VIEW_PATH = "UI/SimpleListView";

    public ButtonView addButton(GameObject container, ButtonData buttonData)
    {
        string buttonPrefabResourceKey = BUTTON_PATH;

        if (buttonData.prefabResourceKey != null)
        {
            buttonPrefabResourceKey = buttonData.prefabResourceKey;
        }

        GameObject buttonPrefab = (GameObject)Object.Instantiate(Resources.Load(buttonPrefabResourceKey), container.transform);
        ButtonView buttonView = buttonPrefab.GetComponent<ButtonView>();
        string textColor = null;
        buttonView.button.onClick.RemoveAllListeners();
        buttonView.button.onClick.AddListener(buttonData.action);
        buttonView.labelText.text = buttonData.label;

        if (buttonData.textColor != null)
        {
            textColor = buttonData.textColor;
        }

        Color newColor;

        if (textColor != null)
        {
            ColorUtility.TryParseHtmlString(textColor, out newColor);
            buttonView.labelText.color = newColor;
        }

        if (buttonData.backgroundColor != null)
        {
            ColorUtility.TryParseHtmlString(buttonData.backgroundColor, out newColor);
            buttonView.buttonImage.color = newColor;
        }

        buttonView.button.interactable = buttonData.interactable;

        return buttonView;
    }

    public DialogView showDialog(string title, string description, List<ButtonData> buttons = null, string prefabResourceKey = DIALOG_PATH)
	{
		GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(prefabResourceKey));
		DialogView dialogView = prefab.GetComponent<DialogView>();

        updateDialog(dialogView, title, description, buttons);
			
		dialogView.fadeIn();
        return dialogView;
    }

    public BasicListView showBasicListView(string title, List<DialogButtonData> buttons = null, string prefabResourceKey = LIST_VIEW_PATH)
    {
        BasicListView view = Object.Instantiate(Resources.Load<BasicListView>(prefabResourceKey));
        view.Set(title, buttons);

        return view;
    }

    public void updateDialog(DialogView dialogView, string title, string description, List<ButtonData> buttons = null)
    {
        if (dialogView.titleText != null)
        {
            dialogView.titleText.text = title;
        }

        if (dialogView.descriptionText != null)
        {
            dialogView.descriptionText.text = description;
        }

        dialogView.clearButtons();

        if (buttons != null)
        {
            int buttonIndex = 0;

            foreach (ButtonData vo in buttons)
            {
                addButton(dialogView.buttonContainer, vo);
                buttonIndex++;
            }
        }
    }

    public DialogView showErrorDialog(string errorString, System.Action actionToRetry = null, string buttonText = null, bool allowCancel = false, System.Action actionOnClose = null)
	{
		DialogView dialog = null;
		List<ButtonData> actions = new List<ButtonData>();

        UnityAction closeDialog = delegate
		{
            if (actionOnClose != null)
            {
                actionOnClose();
            }
            Object.Destroy(dialog.gameObject);
		};

		if (actionToRetry != null)
		{
			if (buttonText == null)
			{
				buttonText = "TRY AGAIN";
			}

			UnityAction retryAction = delegate
			{
				Object.Destroy(dialog.gameObject);
				actionToRetry();
			};
			actions.Add(new ButtonData(retryAction, buttonText));
		}
		else
		{
			if (buttonText == null)
			{
				buttonText = "OK";
			}

			actions.Add(new ButtonData(closeDialog, buttonText));
		}

		if (allowCancel)
		{
			actions.Add(new ButtonData(closeDialog, "CANCEL"));
		}

		dialog = showDialog("ERROR", errorString, actions);

		return dialog;
	}

	public DialogView showConfirmationDialog(string titleString, string messageString, 
											 System.Action confirmButtonAction = null, string confirmButtonText = null, 
		                                     System.Action cancelButtonAction = null, string cancelButtonText = null)
	{
		DialogView dialog = null;
		List<ButtonData> actions = new List<ButtonData>();

		if (confirmButtonText == null)
		{
			confirmButtonText = "OK";
		}

		if (confirmButtonAction != null)
		{
            UnityAction doOkAction = delegate
			{
				Object.Destroy(dialog.gameObject);
				confirmButtonAction();
			};
			actions.Add(new ButtonData(doOkAction, confirmButtonText));
		}
		else
		{
            UnityAction closeDialog = delegate
			{
				Object.Destroy(dialog.gameObject);
			};
			actions.Add(new ButtonData(closeDialog, confirmButtonText));
		}

		if (cancelButtonAction != null)
		{
			if (cancelButtonText == null)
			{
				cancelButtonText = "CANCEL";
			}

            UnityAction doCancelAction = delegate
			{
				Object.Destroy(dialog.gameObject);
				cancelButtonAction();
			};
			actions.Add(new ButtonData(doCancelAction, cancelButtonText));
		}

		dialog = showDialog(titleString, messageString, actions);

		return dialog;
	}

	public GameObject loadTextAreaPrefab(Transform parent = null)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load(TEXT_AREA_PATH), parent);

		return gameObject;
	}

    public GameObject createTextOverlay()
    {
        GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(TEXT_OVERLAY_PREFAB));

        return prefab;
    }

    public GameObject createFadeScreen()
    {
        GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(FADE_SCREEN_PREFAB));

        return prefab;
    }
}

/*
public class ButtonData
{
    public string label;
    public UnityAction action;

    public ButtonData(string label, UnityAction action)
    {
        this.label = label;
        this.action = action;
    }
}
*/