using UnityEngine;
using System.Collections;

/// <summary>
/// This class is to display Help guide as a set of image on screen.
/// It use the help of SwipeControl scripts
/// </summary>
public class ImageDisplay : MonoBehaviour
{
    /// <summary>
    /// Call swipe control class
    /// </summary>
    public SwipeControl swipeCtrl;

    //IMAGES
    /// <summary>
    /// Array of Images, all images need to have the same dimensions!
    /// </summary>
    public Texture2D[] img = new Texture2D[0];
    /// <summary>
    /// Leave empty to use img-dimensions and place in the center of the matrix - this can be used to offset the image from the center of the matrix!
    /// </summary>
    public Rect imgRect;
    //MATRIX
    /// <summary>
    /// Check this to move the matrix to the center of the screen (any value in MatrixPosition will be added to this = offset from center)
    /// </summary>
    public bool centerMatrixOnScreen = true;
    /// <summary>
    /// This is the center of the matrix, use this to position the control - everything will rotate around this point!
    /// </summary>
    public Vector3 matrixPosition = Vector3.zero;
    /// <summary>
    /// The previous matrix position.
    /// </summary>
    private Vector3 prevMatrixPosition;
    /// <summary>
    /// Use this to rotate the GUI formerly known as globalAngle
    /// </summary>
    public float matrixAngle = 0.0f;
    /// <summary>
    /// The previous angle used to check if the Angle changed, so the Quaternion doens't have to be calculated every frame
    /// </summary>
    private float previousAngle;
    /// <summary>
    /// The quaternion
    /// </summary>
    private Quaternion quat = Quaternion.identity;
    /// <summary>
    /// The matrix.
    /// </summary>
    private Matrix4x4 matrix = Matrix4x4.identity;
    /// <summary>
    /// Use the full width (left and right of the image) for swiping?
    /// </summary>
    public bool expandInputAreaToFullWidth = false;

    //DOTS
    /// <summary>
    /// The dot _ First is inactive, second is active. Set Array-length to 0 to hide the dots entirely
    /// </summary>
    public Texture2D[] dot = new Texture2D[2];
    /// <summary>
    /// The dot relative center position is position of the dots, relative to the center of imgRect
    /// </summary>
    public Vector2 dotRelativeCenterPos;

    /// <summary>
    /// enable swipe control
    /// </summary>
    void Awake()
    {

        if (!swipeCtrl) swipeCtrl = gameObject.GetComponent<SwipeControl>(); //Find SwipeControl on same GameObject if none given

        if (imgRect == new Rect(0, 0, 0, 0))
        { //If no rect given, create default rect
            imgRect = new Rect(-(Screen.width * 0.8f) * 0.5f, -(Screen.width * 0.8f) * 0.5f, (Screen.width * 0.8f), (Screen.width * 0.8f));
        }

        //Set up SwipeControl
        swipeCtrl.partWidth = img[0].width;
        swipeCtrl.maxValue = img.Length - 1;
        if (expandInputAreaToFullWidth)
        {
            swipeCtrl.SetMouseRect(new Rect(-Screen.width * 0.5f, imgRect.y, Screen.width, imgRect.height)); // Use image-height for the input-Rect, but full screen-width
        }
        else
        {
            swipeCtrl.SetMouseRect(imgRect); //Use the same Rect as the images for input
        }
        swipeCtrl.CalculateEdgeRectsFromMouseRect(imgRect);
        swipeCtrl.Setup();

        //Determine center position of the Dots
        if (dotRelativeCenterPos == Vector2.zero)
            dotRelativeCenterPos.y = imgRect.height * 0.5f + 14f;
        dotRelativeCenterPos = new Vector2(0, imgRect.height * 0.6f);
        Debug.Log("ImageRect: " + imgRect.height);

        Debug.Log("dotX: " + imgRect.x);

        if (centerMatrixOnScreen)
        {
            matrixPosition.x += Mathf.Round(Screen.width * 0.5f);
            matrixPosition.y += Mathf.Round(Screen.height * 0.5f);
        }

    }

    /// <summary>
    /// set swipe control on screen
    /// </summary>
    void OnGUI()
    {

        // GUI MATRIX
        if (matrixAngle != previousAngle || matrixPosition != prevMatrixPosition)
        { //only calculate new Quaternion if angle changed
            quat.eulerAngles = new Vector3(0.0f, 0.0f, matrixAngle);
            previousAngle = matrixAngle;
            matrix = Matrix4x4.TRS(matrixPosition, quat, Vector3.one);	//If you're no longer tweaking
            prevMatrixPosition = matrixPosition;
            swipeCtrl.matrix = matrix; // Tell SwipeControl to use the same Matrix we use here
        }
        GUI.matrix = matrix;

        // IMAGES
        float offset = swipeCtrl.smoothValue - Mathf.Round(swipeCtrl.smoothValue);
        float mainPos = imgRect.x - (offset * imgRect.width);
        if (Mathf.Round(swipeCtrl.smoothValue) >= 0 && Mathf.Round(swipeCtrl.smoothValue) < img.Length)
        {
            GUI.color = new Color(1f, 1f, 1f, 1f - Mathf.Abs(offset));
            GUI.DrawTexture(new Rect(mainPos, imgRect.y, imgRect.width, imgRect.height), img[(int)Mathf.Round(swipeCtrl.smoothValue)]);
        }
        GUI.color = new Color(1f, 1f, 1f, -offset);
        if (GUI.color.a > 0.0f && Mathf.Round(swipeCtrl.smoothValue) - 1 >= 0 && Mathf.Round(swipeCtrl.smoothValue) - 1 < img.Length)
        {
            GUI.DrawTexture(new Rect(mainPos - imgRect.width, imgRect.y, imgRect.width, imgRect.height), img[(int)Mathf.Round(swipeCtrl.smoothValue) - 1]);
        }
        GUI.color = new Color(1f, 1f, 1f, offset);
        if (GUI.color.a > 0.0f && Mathf.Round(swipeCtrl.smoothValue) + 1 < img.Length && Mathf.Round(swipeCtrl.smoothValue) + 1 >= 0)
        {
            GUI.DrawTexture(new Rect(mainPos + imgRect.width, imgRect.y, imgRect.width, imgRect.height), img[(int)Mathf.Round(swipeCtrl.smoothValue) + 1]);
        }
        GUI.color = new Color(1f, 1f, 1f, 1f);


        // DOTS
        if (dot.Length > 0)
        {
            for (var i = 0; i < img.Length; i++)
            {
                bool activeOrNot = false;
                if (i == Mathf.Round(swipeCtrl.smoothValue)) activeOrNot = true;
                if (!activeOrNot) GUI.DrawTexture(new Rect(dotRelativeCenterPos.x - (img.Length * dot[0].width * 0.5f) + (i * dot[0].width), Mathf.Round(dotRelativeCenterPos.y - (dot[0].height * 0.5f)), dot[0].width, dot[0].height), dot[0]);
                else GUI.DrawTexture(new Rect(Mathf.Round(dotRelativeCenterPos.x - (img.Length * dot[0].width * 0.5f) + (i * dot[0].width)), Mathf.Round(dotRelativeCenterPos.y - (dot[0].height * 0.5f)), dot[0].width, dot[0].height), dot[1]);
            }
        }

    }
}
