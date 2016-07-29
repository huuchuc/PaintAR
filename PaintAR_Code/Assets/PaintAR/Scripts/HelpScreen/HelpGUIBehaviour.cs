using UnityEngine;
using System.Collections;

/// <summary>
/// A class to display and control behaviour of two buttons: Back and Print
/// </summary>
public class HelpGUIBehaviour : MonoBehaviour
{   
    /// <summary>
    /// The texture of background
    /// </summary>
    public Texture texBackground;
    /// <summary>
    /// The tex back is texture of button Back
    /// </summary>
    public Texture texBack;
    /// <summary>
    /// The tex home site is texture of button Print
    /// </summary>
    public Texture texHomeSite;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void OnGUI()
    {
        // Draw background texture;
        GUITexture guiText = gameObject.GetComponent<GUITexture>();
        guiText.pixelInset = new Rect(-Screen.width / 2, Screen.height / 2, Screen.width, -Screen.width * 16 / 9);

        // Make the help button
        btnBack();//back
        btnHomeSite();//homesite
    }

    void btnBack()
    {
        if (GUI.Button(new Rect(10, 10, (Screen.width / 4f) - 20, (Screen.width / 4f) - 20), texBack, ""))
        {
            Application.LoadLevel(GlobalVariable.Instance.getLevel);
            GlobalVariable.Instance.getLevel = "MainScreen";
        }
    }

    void btnHomeSite()
    {
        if (GUI.Button(new Rect((Screen.width / 4f) * 3 + 10, 10, (Screen.width / 4f) - 20, (Screen.width / 4f) - 20), texHomeSite, ""))
        {
            Application.OpenURL("http://fupaintar.appspot.com");
        }
    }
}
