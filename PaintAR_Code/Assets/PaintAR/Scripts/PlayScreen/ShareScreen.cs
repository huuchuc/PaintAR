using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
/// This class have responsibility of taking a screenshot and send it to other apps via Intent
/// </summary>
public class ShareScreen : MonoBehaviour
{
    /// <summary>
    /// This is to check if the screenshot taking process was completed or not.
    /// </summary>
    private bool isProcessing = false;

    /// <summary>
    /// Update each frame: nothing
    /// </summary>
    void Update()
    {

    }
    /// <summary>
    /// The process of taking screenshot and share to other application available in the device.
    /// </summary>
    /// <returns></returns>
    public IEnumerator ShareScreenshot()
    {
        isProcessing = true;

        // Taking screenshot
        // wait for graphics to render
        yield return new WaitForEndOfFrame();

        // create the texture
        Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);

        // put buffer into texture
        screenTexture.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);

        // apply
        screenTexture.Apply();

        // copy texture to byte array
        byte[] dataToSave = screenTexture.EncodeToPNG();

        // set path to save file
        string destination = Path.Combine(Application.persistentDataPath, "PaintARscreen.png");
        File.WriteAllBytes(destination, dataToSave);

        // Capture is done, so we will turn on the button on screen.
        GlobalVariable.Instance.getCapture = false;

        if (!Application.isEditor)
        {
            // block to open the file and share it ------------START
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + destination);

            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);

            intentObject.Call<AndroidJavaObject>("setType", "image/jpeg");
            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

            // option one:
            currentActivity.Call("startActivity", intentObject);

            // option two:
            //AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "YO BRO! WANNA SHARE?");
            //currentActivity.Call("startActivity", jChooser);

            // block to open the file and share it ------------END

        }
        isProcessing = false;
    }
}
