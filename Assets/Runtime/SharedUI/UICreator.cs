using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class UICreator
{
	private const string BUTTON_PATH = "UI/Button";
	private const string TEXT_AREA_PATH = "UI/TextArea";
	private const string DIALOG_PATH = "UI/DialogBox";

    public ButtonView addButton(GameObject container, ButtonVO vo)
    {
        string buttonPrefabResourceKey = BUTTON_PATH;

        if (vo.prefabResourceKey != null)
        {
            buttonPrefabResourceKey = vo.prefabResourceKey;
        }

        GameObject buttonPrefab = (GameObject)Object.Instantiate(Resources.Load(buttonPrefabResourceKey), container.transform);
        ButtonView buttonView = buttonPrefab.GetComponent<ButtonView>();
        string textColor = null;
        buttonView.button.onClick.RemoveAllListeners();
        buttonView.button.onClick.AddListener(vo.action);
        buttonView.labelText.text = vo.label;

        if (vo.textColor != null)
        {
            textColor = vo.textColor;
        }

        Color newColor;

        if (textColor != null)
        {
            ColorUtility.TryParseHtmlString(textColor, out newColor);
            buttonView.labelText.color = newColor;
        }

        if (vo.backgroundColor != null)
        {
            ColorUtility.TryParseHtmlString(vo.backgroundColor, out newColor);
            buttonView.buttonImage.color = newColor;
        }

        buttonView.button.interactable = vo.interactable;

        return buttonView;
    }

    public DialogView showDialog(string title, string description, List<ButtonVO> buttons = null, string prefabResourceKey = DIALOG_PATH)
	{
		GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(prefabResourceKey));
		DialogView dialogView = prefab.GetComponent<DialogView>();

        updateDialog(dialogView, title, description, buttons);
			
		dialogView.fadeIn();

		return dialogView;
	}
		
    public void updateDialog(DialogView dialogView, string title, string description, List<ButtonVO> buttons = null)
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

            foreach (ButtonVO vo in buttons)
            {
                addButton(dialogView.buttonContainer, vo);
                buttonIndex++;
            }
        }
    }

    public DialogView showErrorDialog(string errorString, System.Action actionToRetry = null, string buttonText = null, bool allowCancel = false, System.Action actionOnClose = null)
	{
		DialogView dialog = null;
		List<ButtonVO> actions = new List<ButtonVO>();

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
			actions.Add(new ButtonVO(retryAction, buttonText));
		}
		else
		{
			if (buttonText == null)
			{
				buttonText = "OK";
			}

			actions.Add(new ButtonVO(closeDialog, buttonText));
		}

		if (allowCancel)
		{
			actions.Add(new ButtonVO(closeDialog, "CANCEL"));
		}

		dialog = showDialog("ERROR", errorString, actions);

		return dialog;
	}

	public DialogView showConfirmationDialog(string titleString, string messageString, 
											 System.Action confirmButtonAction = null, string confirmButtonText = null, 
		                                     System.Action cancelButtonAction = null, string cancelButtonText = null)
	{
		DialogView dialog = null;
		List<ButtonVO> actions = new List<ButtonVO>();

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
			actions.Add(new ButtonVO(doOkAction, confirmButtonText));
		}
		else
		{
            UnityAction closeDialog = delegate
			{
				Object.Destroy(dialog.gameObject);
			};
			actions.Add(new ButtonVO(closeDialog, confirmButtonText));
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
			actions.Add(new ButtonVO(doCancelAction, cancelButtonText));
		}

		dialog = showDialog(titleString, messageString, actions);

		return dialog;
	}

	public GameObject loadTextAreaPrefab(Transform parent = null)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load(TEXT_AREA_PATH), parent);

		return gameObject;
	}

    private static string FADE_SCREEN_PREFAB = "UI/ScreenFade";
    private static string TEXT_OVERLAY_PREFAB = "UI/TextOverlay";

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

public class ButtonVO
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

	public ButtonVO(UnityAction action, string label = null, string iconResourceKey = null, string prefabResourceKey = null)
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

	public void makeGoldButton()
	{
		textColor = GOLD_TEXT_COLOR;
		backgroundColor = GOLD_BG_COLOR;
	}
}
