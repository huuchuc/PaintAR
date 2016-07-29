#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

public class ScreenshotManager : MonoBehaviour
{

    public static event Action ScreenshotFinishedSaving;
    public static event Action ImageFinishedSaving;
    /// <summary>
    /// The path of folder to save file.
    /// </summary>
    public static string TempPath;

    /// <summary>
    /// Save screenshot to gallery
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="albumName"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static IEnumerator Save(string fileName, string albumName, bool callback = false)
    {
        bool photoSaved = false;
        //add date and time to file name
        string date = System.DateTime.Now.ToString("dd-MM-yy");

        ScreenshotManager.ScreenShotNumber++;

        string screenshotFilename = fileName + "_" + ScreenshotManager.ScreenShotNumber + "_" + date + ".png";

        Debug.Log("Save screenshot " + screenshotFilename);

        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("Android platform detected");

            string androidPath = "/../../../../DCIM/" + albumName + "/" + screenshotFilename;
            string path = Application.persistentDataPath + androidPath;
            string pathonly = Path.GetDirectoryName(path);
            TempPath = androidPath;
            // Create directory for PaintAR
            Directory.CreateDirectory(pathonly);
            Application.CaptureScreenshot(androidPath);
            if (GlobalVariable.Instance.getCapture == true)
            {
                yield return new WaitForSeconds(1.0f);
                GlobalVariable.Instance.getCapture = false;
            }
            AndroidJavaClass obj = new AndroidJavaClass("com.ryanwebb.androidscreenshot.MainActivity");

            while (!photoSaved)
            {
                photoSaved = obj.CallStatic<bool>("scanMedia", path);

                yield return new WaitForSeconds(.5f);
            }

        }
        else
        {

            Application.CaptureScreenshot(screenshotFilename);
            if (GlobalVariable.Instance.getCapture == true)
            {
                yield return new WaitForSeconds(1.0f);
                GlobalVariable.Instance.getCapture = false;
            }

        }

        while (!photoSaved)
        {
            yield return new WaitForSeconds(.5f);
            photoSaved = true;
        }

        if (callback)
            ScreenshotFinishedSaving();

    }

    /// <summary>
    /// Save file when its exist in existance data folder
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static IEnumerator SaveExisting(string filePath, bool callback = false)
    {
        bool photoSaved = false;

        Debug.Log("Save existing file to gallery " + filePath);

        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("Android platform detected");

            AndroidJavaClass obj = new AndroidJavaClass("com.ryanwebb.androidscreenshot.MainActivity");

            while (!photoSaved)
            {
                photoSaved = obj.CallStatic<bool>("scanMedia", filePath);

                yield return new WaitForSeconds(.5f);
            }

        }

        while (!photoSaved)
        {
            yield return new WaitForSeconds(.5f);

            Debug.Log("Save existing file only available in iOS/Android mode!");

            photoSaved = true;
        }

        if (callback)
            ImageFinishedSaving();
    }

    /// <summary>
    /// Option 2: filename + count number
    /// </summary>
    public static int ScreenShotNumber
    {
        set { PlayerPrefs.SetInt("screenShotNumber", value); }

        get { return PlayerPrefs.GetInt("screenShotNumber"); }
    }
}
