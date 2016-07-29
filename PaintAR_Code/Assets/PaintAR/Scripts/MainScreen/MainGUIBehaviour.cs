
using UnityEngine;
using System.Collections;

/// <summary>
/// A class to display and control behaviour of two buttons: Play and Help
/// </summary>
public class MainGUIBehaviour : MonoBehaviour
{
    /// <summary>
    /// The texture of Background
    /// </summary>
    public Texture texBackground;
    /// <summary>
    /// The tex play is texture of button Play
    /// </summary>
    public Texture texPlay;
    /// <summary>
    /// The tex help is texture of button Help
    /// </summary>
    public Texture texHelp;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.width *16/9), texBackground);
        btnPlay();
        btnHelp();
    }

    // Make the play button
    void btnPlay()
    {
        if (GUI.Button(new Rect((Screen.width * 0.5f) - (Screen.width * 0.15f), (Screen.height * 0.4f), Screen.width * 0.3f, Screen.width * 0.3f), texPlay, ""))
        { //if button Play is press
            GlobalVariable.Instance.getLevel = "MainScreen"; // set this Scene level is MainScreen
            Application.LoadLevel("PlayScreen"); // Jump into Play Activities scene level
        }
    }

    // Make the help button
    void btnHelp()
    {
        if (GUI.Button(new Rect((Screen.width * 0.5f) - (Screen.width * 0.15f), (Screen.height * 0.9f - Screen.width * 0.3f), Screen.width * 0.3f, Screen.width * 0.3f), texHelp, ""))
        {//if button Help is press
            GlobalVariable.Instance.getLevel = "MainScreen"; //set this Scene level is MainScreen
            Application.LoadLevel("HelpScreen"); //Jump into HelpScreen scene level
        }
    }
}
