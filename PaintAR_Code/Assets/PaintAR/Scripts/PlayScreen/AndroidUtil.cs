using UnityEngine;
using System.Collections;

/// <summary>
/// This class provide access to the functions in AndroidPlugin: androidutil.jar
/// </summary>
public class AndroidUtil : MonoBehaviour
{   
    /// <summary>
    /// Enable call to Android function
    /// </summary>
    public static bool isDebug = true;

    /// <summary>
    /// Retrieve the Android plugin
    /// </summary>
    /// <returns></returns>
    static AndroidJavaObject getUnityPlayerObject()
    {
        AndroidJavaClass parentClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activityObject = parentClass.GetStatic<AndroidJavaObject>("currentActivity");

        return activityObject;
    }

    /// <summary>
    /// retrieve the class
    /// </summary>
    /// <returns></returns>
    static AndroidJavaClass prepareLog()
    {
        return new AndroidJavaClass("com.fu.paintar.AndroidUtil");
    }

    /// <summary>
    /// Call to showToast function
    /// </summary>
    /// <param name="message"></param>
    public static void showToast(string message)
    {
        if (isDebug == true)
        {
            prepareLog().CallStatic("showToast", getUnityPlayerObject(), message);
        }
    }
}
