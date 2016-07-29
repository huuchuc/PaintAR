using UnityEngine;

using System.Collections;
using System.Threading;
using System.IO;
using Alt.Sketch;
using System;
using Alt.Sketch.Ext.AForge.Imaging.Filters;
using System.Collections.Generic;
using AForge;

/// <summary>
/// This is an options for obtaining the camera image (without augmentation) from Unity.
/// </summary>
public class CameraImageAccess : MonoBehaviour, ITrackerEventHandler
{
    /// <summary>
    /// The pixel format of an image.
    /// Enumerator: UNKNOWN_FORMAT, RGB565, RGB888, GRAYSCALE, YUV, RGBA8888
    /// </summary>
    private Image.PIXEL_FORMAT m_PixelFormat;
    /// <summary>
    /// The object stands for states variable of the Image Target.
    /// </summary>
    public TargetStats targetStats;
    /// <summary>
    /// Data of an image, in byte array format
    /// </summary>
    public static byte[] imageData;
    /// <summary>
    /// The width of the camera image
    /// </summary>
    public float imageWidth;
    /// <summary>
    /// The height of the camera image
    /// </summary>
    public float imageHeight;
    /// <summary>
    /// Set to true if the Screen image should fit the height of the camera image.
    /// Otherwise, set to false.
    /// </summary>
    public bool scaleByHeight;
    /// <summary>
    /// The factor to scale the screen image - or exactly,
    /// the coordinates of the screen image - to the camera image.
    /// </summary>
    public float scalarFactor;
    /// <summary>
    /// An object to work with the camera image.
    /// </summary>
    public static Bitmap iBitmap;
    /// <summary>
    /// After taking the camera image, we need to prepare it for further step.
    /// This is to check if the preparation was done or not.
    /// </summary>
    static bool prepareOK = false;
    /// <summary>
    /// For testing purpose, we need to save the camera image to disk. This is to trigger that.
    /// </summary>
    private bool writeFile = false;
    /// <summary>
    /// The main function of this class cannot work if the frame format was not - or cannot set.
    /// This is to check if it was set or not.
    /// </summary>
    private bool m_RegisteredFormat = false;
    /// <summary>
    /// This is to trigger if we want to log the ifo of camera image or not.
    /// </summary>
    private bool m_LogInfo = true;

    /// <summary>
    /// This method is here because we have implemented ITrackerEventHandler
    /// </summary>
    public void OnInitialized()
    {

    }

    /// <summary>
    /// Use this for initialize
    /// </summary>
    void Start()
    {
        // Set frame format, RGB888 equal to RGB24
        if (Application.platform == RuntimePlatform.Android)
        {
            m_PixelFormat = Image.PIXEL_FORMAT.RGB888;
        }
        else
        {
            m_PixelFormat = Image.PIXEL_FORMAT.GRAYSCALE;
        }

        // Register to have access to trackable events
        QCARBehaviour qcarBehaviour = (QCARBehaviour)FindObjectOfType(typeof(QCARBehaviour));

        if (qcarBehaviour)
        {
            qcarBehaviour.RegisterTrackerEventHandler(this);
        }
    }

    /// <summary>
    /// We can retrieve the image usingfrom this callback to ensure that
    /// we retrieve the latest camera image that matches the current frame.
    /// </summary>
    public void OnTrackablesUpdated()
    {
        if (!m_RegisteredFormat)
        {
            CameraDevice.Instance.SetFrameFormat(m_PixelFormat, true);
            m_RegisteredFormat = true;
        }
        if (m_LogInfo)
        {
            CameraDevice cam = CameraDevice.Instance;
            Image image = cam.GetCameraImage(m_PixelFormat);
            if (image == null)
            {
                Debug.Log(m_PixelFormat + " image is not available yet");
            }
            else
            {
                if (Application.platform == RuntimePlatform.Android)
                {   
                    // On Android portrait mode, the camera image was actually rotated 90 degree
                    // We need to swap the value of width and height for calculating purpose.
                    if (Screen.orientation == ScreenOrientation.Portrait)
                    {
                        imageWidth = image.Height;
                        imageHeight = image.Width;
                    }
                }
                else
                {
                    imageWidth = image.Width;
                    imageHeight = image.Height;
                }

                //Calculate the scale factor after we have the right width and height
                CalculateScaleFactor();

                Debug.Log("Camera image: " + imageWidth + ":" + imageHeight + " Screen: " + Screen.width + ":" + Screen.height + ", ScaleByHeight = " + scaleByHeight + ", scalarFactor = " + scalarFactor);
                m_LogInfo = false;
            }
        }
    }

    /// <summary>
    /// This method is to set the scaleByHeight and calculate the scalarFactor.
    /// If the screen is "thinner" than the camera, scaleByHeight = true.
    /// Other wise, scaleByHeight = false.
    /// </summary>
    void CalculateScaleFactor()
    {
        if ((float)Screen.width / (float)Screen.height < imageWidth / imageHeight)
        {
            Debug.Log("Scale height");
            scaleByHeight = true;
            scalarFactor = imageHeight / (float)Screen.height;
        }
        else
        {
            Debug.Log("Scale width");
            scaleByHeight = false;
            scalarFactor = imageWidth / (float)Screen.width;
        }
    }

    /// <summary>
    /// This method retrieve the camera image and save it to TargetStats as a Bitmap.
    /// It should run when the target's screen coordinates is OK.
    /// </summary>
    /// <returns></returns>
    public IEnumerator TakeScreenshot()
    {
        CameraDevice cam = CameraDevice.Instance;
        Image image = cam.GetCameraImage(m_PixelFormat);

        if (image == null)
        {
            Debug.Log(m_PixelFormat + " image is not available yet");
            yield return null;
        }
        else
        {   
            // Declare a texture with the size of the image
            Texture2D tex = new Texture2D(image.Width, image.Height, TextureFormat.RGB24, false);
            // Copy Image to the texture
            image.CopyToTexture(tex);
            tex.Apply();
            // Copy texture to byte array
            imageData = tex.EncodeToPNG();
            Destroy(tex);

            // PrepareImage should run on another thread to avoid drop of frame
            Thread prepare = new Thread(new ThreadStart(PrepareImage));
            prepare.Start();

            while (prepareOK == false)
            {
                yield return null;
            }
            if (writeFile)
            {
                // For testing purposes, also write to a file in the project folder
                MemoryStream stream = new MemoryStream();
                iBitmap.Save(stream, iBitmap.RawFormat);
                imageData = stream.ToArray(); 
                if (Application.platform == RuntimePlatform.Android)
                {
                    File.WriteAllBytes(Application.persistentDataPath + "/Screenshot.png", imageData);
                }
                else
                {
                    File.WriteAllBytes("Screenshot.png", imageData);
                }
            }

            prepareOK = false;

            // Pass state and Bitmap to TargetStats
            targetStats.IMAGEBITMAP = iBitmap;
            targetStats.TARGET_IMAGE_OK = true;

            yield return null;
        }
    }

    /// <summary>
    /// We need to Rotate 90 degree and Flip horizontal the camera image to get what we have seen on screen.
    /// It should run on another thread to avoid drop of frame
    /// </summary>
    public static void PrepareImage()
    {
        try
        {
            Debug.Log("PaintAR: start Prepare");
            prepareOK = false;

            MemoryStream stream = new MemoryStream(imageData);
            iBitmap = new Bitmap(stream);
            if (Application.platform == RuntimePlatform.Android)
            {
                iBitmap.Rotate90FlipY();
                Debug.Log("Rotate");
            }
            else
            {
                iBitmap.FlipY();
                Debug.Log("Not rotate");
            }
            Debug.Log("PaintAR: finish Prepare");
            prepareOK = true;
        }
        catch (ThreadAbortException e)
        {
            Debug.Log("PaintAR: Abort Prepare thread");
        }
    }       
}
