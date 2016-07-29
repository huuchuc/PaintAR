using UnityEngine;
using System.Collections;


public class PlayGUIBehaviour : MonoBehaviour
{
    /// <summary>
    /// The tex menu down is texture of button Menu down in PlayActivities Scene Level
    /// </summary>
    public Texture texMenuDown;
    /// <summary>
    /// The tex menu up is texture of button Menu up in PlayActivities Scene Level
    /// </summary>
    public Texture texMenuUp;
    /// <summary>
    /// The tex screenshot is texture of button Capture Screen
    /// </summary>
    public Texture texScreenshot;
    /// <summary>
    /// The tex help is texture of button Help
    /// </summary>
    public Texture texHelp;
    /// <summary>
    /// The tex back is texture of button Back
    /// </summary>
    public Texture texBack;
    /// <summary>
    /// The tex social sharing is texture of button Sharing to Social Network
    /// </summary>
    public Texture texSocialSharing;
    /// <summary>
    /// The show menu is boolean variable check show Menu popup or not
    /// </summary>
    bool showMenu = false;
    /// <summary>
    /// The dis menu down is boolean variable check button Menu Down is disapear or not
    /// </summary>
    bool disapMenuDown = true;
    /// <summary>
    /// This is to retrieve the ShareScreen's script
    /// </summary>
    ShareScreen share;

    void Start()
    {
        CaptureScreen.Start();
    }

    void OnGUI()
    {
        if (!GlobalVariable.Instance.getCapture)
        {
            if (disapMenuDown)
            { // if button Menu Down is not disapear
                btnMenuDown();
            }

            if (showMenu == true)
            { //show Function Menu
                btnCaptureScreen();
                //sharing to social network code
                btnSharing();
                //help button
                btnHelp();
                //back button
                btnBack();
                //Up menu button
                btnMenuUp();
            }
        }
    }

    void btnMenuDown()
    {
        if (GUI.Button(new Rect((Screen.width / 5f) * 4 + 10, 10, (Screen.width / 5f) - 20, (Screen.width / 5f) - 20), texMenuDown, ""))
        { // when button Menu Down is press
            if (showMenu == false)
            { // will show Function Menu
                showMenu = true;
                disapMenuDown = false; // disapear button Menu Down
            }
            else
                showMenu = false; //set not show Function Menu
        }
    }

    void btnCaptureScreen()
    {
        if (GUI.Button(new Rect((Screen.width / 5f) * 3 + 10, 10, (Screen.width / 5f) - 20, (Screen.width / 5f) - 20), texScreenshot, "")) //if button Screenshot is press
        {
            //Clear screen when function Capture Screen excute
            GlobalVariable.Instance.getCapture = true;
            StartCoroutine(ScreenshotManager.Save("MyScreenshot", "PaintAR", true)); //save image capture to gallery
            if (GlobalVariable.Instance.savedCapture) AndroidUtil.showToast("Saved image to gallery."); //push toast to scene
        }
    }

    void btnSharing()
    {
        if (GUI.Button(new Rect((Screen.width / 5f) * 2 + 10, 10, (Screen.width / 5f) - 20, (Screen.width / 5f) - 20), texSocialSharing, "")) //if button Sharing is press
        {
            GlobalVariable.Instance.getCapture = true;
            share = gameObject.GetComponent<ShareScreen>();
            StartCoroutine(share.ShareScreenshot());
        }
    }

    void btnHelp()
    {
        if (GUI.Button(new Rect((Screen.width / 5f) + 10, 10, (Screen.width / 5f) - 20, (Screen.width / 5f) - 20), texHelp, "")) //if button Help is press
        {
            GlobalVariable.Instance.getLevel = "PlayScreen";// set this scene level is Play Activities
            Application.LoadLevel("HelpScreen"); //Jump into HelpScreen scene level
        }
    }

    void btnBack()
    {
        if (GUI.Button(new Rect(10, 10, (Screen.width / 5f) - 20, (Screen.width / 5f) - 20), texBack, "")) //if button Back is press
        {
            Application.LoadLevel(GlobalVariable.Instance.getLevel); //jump into back scene level
        }
    }

    void btnMenuUp()
    {
        if (GUI.Button(new Rect((Screen.width / 5f) * 4 + 10, 10, (Screen.width / 5f) - 20, (Screen.width / 5f) - 20), texMenuUp, ""))
        { //if button Menu Up is press
            showMenu = false; //set Function Menu is disapear
            disapMenuDown = true; //set Menu Down is show
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
