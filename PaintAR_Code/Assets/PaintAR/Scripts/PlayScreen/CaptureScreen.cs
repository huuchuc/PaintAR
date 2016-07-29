using UnityEngine;
using System.Collections;
using System.IO;

public class CaptureScreen: MonoBehaviour {
	public static void Start ()
	{
		ScreenshotManager.ScreenshotFinishedSaving += ScreenshotSaved;	//start working with capture screen function
	}

	public static void ScreenshotSaved()
	{
		Debug.Log ("screenshot finished saving");
		GlobalVariable.Instance.savedCapture = true;
	}
}