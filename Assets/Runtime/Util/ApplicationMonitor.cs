using UnityEngine;
using signals;

public class ApplicationMonitor : MonoBehaviour
{
	public Signal applicationQuit = new Signal();
	public Signal<bool> applicationPause = new Signal<bool>();
	public Signal<bool> applicationFocus = new Signal<bool>();

	void OnApplicationQuit()
	{
		applicationQuit.Dispatch();
	}

	void OnApplicationPause(bool pauseStatus)
	{
		applicationPause.Dispatch(pauseStatus);
	}

	void OnApplicationFocus(bool hasFocus)
	{
		applicationFocus.Dispatch(hasFocus);
	}
}