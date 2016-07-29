using UnityEngine;
using System.Collections;

/// <summary>
/// We need to set a specific Frame Format for your PaintARCamera.
/// This script is to do this, in a robust behaviour with scene change / reloading or pausing / resuming the app at any time.
/// </summary>
public class ImageFormat : MonoBehaviour, ITrackerEventHandler
{   
    /// <summary>
    /// Contain the debug message
    /// </summary>
    private string mDebugMsg = "";
    /// <summary>
    /// Contain the image info
    /// </summary>
    private string mImageInfo = "";
    /// <summary>
    /// The pixel format of an image.
    /// Enumerator: UNKNOWN_FORMAT, RGB565, RGB888, GRAYSCALE, YUV, RGBA8888
    /// </summary>
    private Image.PIXEL_FORMAT mFormat = Image.PIXEL_FORMAT.RGB888;
    /// <summary>
    /// check if register format was success
    /// </summary>
    private bool mRegisteredFormat = false;
    /// <summary>
    /// check if QCAR is initialized
    /// </summary>
    private bool mQCARInitialized = false;

    // Use this for initialization
    void Start()
    {
        QCARBehaviour qcar = (QCARBehaviour)FindObjectOfType(typeof(QCARBehaviour));
        if (qcar)
        {
            qcar.RegisterTrackerEventHandler(this);
        }
        else
        {
            Debug.LogError("Could not find QCARBehaviour (i.e. ARCamera) in the scene");
        }

    }

    /// <summary>
    /// Implementing OnInitialized() of ITrackerEventHandler interface
    /// </summary>
    public void OnInitialized()
    {
        mQCARInitialized = true;
        mRegisteredFormat = false;
    }

    /// <summary>
    /// Implementing OnTrackablesUpdated()
    /// of ITrackerEventHandler interface
    /// </summary>
    public void OnTrackablesUpdated()
    {
        
    }

    /// <summary>
    /// This is called whenever the app gets paused
    /// </summary>
    /// <param name="paused"></param>
    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            // invalidate registered format if app has been paused
            mRegisteredFormat = false;
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // Skip if QCAR has not been initialized yet
        if (mQCARInitialized)
        {
            if (!mRegisteredFormat)
            {
                //first time update or first resume after pause
                //see OnApplicationPaused() above
                mFormat = Image.PIXEL_FORMAT.RGB888;// Choose the Frame Format you want here
                CameraDevice.Instance.SetFrameFormat(mFormat, false);
                if (CameraDevice.Instance.SetFrameFormat(mFormat, true))
                {
                    mDebugMsg = mFormat.ToString() + " successfully set.";
                    mRegisteredFormat = true;
                }
                else
                {
                    mDebugMsg = "Failed to set RGB888.";
                    mFormat = Image.PIXEL_FORMAT.UNKNOWN_FORMAT;
                }
            }

            Image img = CameraDevice.Instance.GetCameraImage(mFormat);
            if (img != null)
            {
                mImageInfo = img.Width + " x " + img.Height + " " + "Pixels:" + img.Pixels[0] + ", " + img.Pixels[1] + " ...";
            }
            else
            {
                mImageInfo = "Can't get Image for " + mFormat.ToString();
            }
        }
    }


}