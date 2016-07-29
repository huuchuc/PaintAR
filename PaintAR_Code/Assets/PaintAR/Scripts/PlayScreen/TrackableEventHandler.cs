using UnityEngine;
using System.Collections;
using System.IO;
using Alt.Sketch;
using Alt.Sketch.Ext.AForge.Imaging.Filters;
using System.Threading;
using System.Collections.Generic;
using AForge;

/// <summary>
/// A custom handler that implements the ITrackableEventHandler interface.
/// Control the 3D model to appear/disappear –
/// an automatic reaction to the appearance of the target.
/// </summary>
public class TrackableEventHandler : MonoBehaviour,
                                            ITrackableEventHandler
{   
    /// <summary>
    /// The base of trackable behaviour
    /// </summary>
    private TrackableBehaviour mTrackableBehaviour;
    /// <summary>
    /// The object stands for states variable of the Image Target.
    /// </summary>
    public TargetStats targetStats;
    /// <summary>
    /// The bitmap use in transformImage function, must declare here because transform is a thread
    /// </summary>
    static Bitmap iBitmap;
    /// <summary>
    /// The corners use in transformImage function, must declare here because transform is a thread
    /// </summary>
    static List<IntPoint> corners;
    /// <summary>
    /// Result of the camera image after transformation, in byte array format
    /// </summary>
    static byte[] result;
    /// <summary>
    /// Check if transformation is complete
    /// </summary>
    static bool transformOK = false;
    /// <summary>
    /// transform must be declared here because we want to abort it on tracking lost
    /// </summary>
    static Thread transform;

    /// <summary>
    /// Use this for initialize
    /// </summary>
    void Start()
    {
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
        {
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
        }

    }

    /// <summary>
    /// Implementation of the ITrackableEventHandler function called when the
    /// tracking state changes.
    /// </summary>
    public void OnTrackableStateChanged(
                                    TrackableBehaviour.Status previousStatus,
                                    TrackableBehaviour.Status newStatus)
    {
        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED ||
            newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {   
            // Set the state variables
            targetStats.TARGET_FOUND = true;
            targetStats.TARGET_IMAGE_OK = false;
            targetStats.TARGET_COORDS_OK = false;
            targetStats.TARGET_RENDERED = false;
            targetStats.IMAGEBITMAP = new Bitmap();
            targetStats.CORNERS = new List<IntPoint>();
            result = null;

            StartCoroutine(OnTrackingFound());
        }
        else
        {
            // Reset the state variables
            targetStats.TARGET_FOUND = false;
            targetStats.TARGET_IMAGE_OK = false;
            targetStats.TARGET_COORDS_OK = false;
            targetStats.TARGET_RENDERED = false;

            OnTrackingLost();
        }
    }

    /// <summary>
    /// Process to "turn on" the 3D model after we have enough input for it
    /// </summary>
    /// <returns></returns>
    IEnumerator OnTrackingFound()
    {
        Debug.Log("PaintAR: Trackable " + mTrackableBehaviour.TrackableName + " found");
        
        // Wait for getting target screen coordinates and the camera image
        while (targetStats.TARGET_COORDS_OK == false || targetStats.TARGET_IMAGE_OK == false)
        {
            yield return new WaitForSeconds(.2f);
        }

        // Transform Image should run on another thread to avoid drop of frame
        transform = new Thread(new ThreadStart(TransformImage));
        Debug.Log("PaintAR: Transform threadstate: " + transform.ThreadState);

        iBitmap = targetStats.IMAGEBITMAP;
        corners = targetStats.CORNERS;

        transform.Start();

        // Wait for transformation complete
        while (transformOK == false)
        {
            if (targetStats.TARGET_FOUND == false)
            {   
                // finish when lost tracking
                yield break;
            }
            else
            {
                yield return null;
            }
        }

        transformOK = false;
        targetStats.TARGET_FOUND = true;
        targetStats.TARGET_IMAGE_OK = true;
        targetStats.TARGET_COORDS_OK = true;
        targetStats.TARGET_RENDERED = true;

        // the 3D models are children of ImageTarget on Inspector
        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
       
        // Enable rendering:
        foreach (Renderer component in rendererComponents)
        {
            //declare texture
            Texture2D texture = new Texture2D(4, 4);

            // copy result byte array to texture
            texture.LoadImage(result);

            // apply texture on the 3D model
            component.renderer.material.mainTexture = texture;

            // render the 3D model
            component.enabled = true;
        }

        result = null;
    }

    /// <summary>
    /// Behaviour when the target's lost tracking
    /// The 3D model should disappear
    /// </summary>
    private void OnTrackingLost()
    {
        
        // Reset
        iBitmap = new Bitmap();
        corners = new List<IntPoint>();
        targetStats.IMAGEBITMAP = new Bitmap();
        targetStats.CORNERS = new List<IntPoint>();
        result = null;

        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
        Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

        // Disable rendering:
        foreach (Renderer component in rendererComponents)
        {
            component.enabled = false;
        }

        // Disable colliders:
        foreach (Collider component in colliderComponents)
        {
            component.enabled = false;
        }
        Debug.Log("PaintAR: Trackable " + mTrackableBehaviour.TrackableName + " lost");
        try
        {
            if (transform.ThreadState == ThreadState.Running)
            {
                Debug.Log("PaintAR: Try to abort transform");
                transform.Abort();
            }
        }
        catch (System.Exception)
        {
            
            throw;
        }

    }

    /// <summary>
    /// The image target caught on screen was a Quadrilateral part of a bitmap.
    /// We need to de-warp the bitmap and crop it to retrieve the target only.
    /// This is needed to use target as a texture for the 3D model.
    /// It should run on another thread to avoid drop of frame
    /// </summary>
    public static void TransformImage()
    {
        try
        {
            Debug.Log("PaintAR: start Transform");
            transformOK = false;

            // define filter
            // Parameter: 4 corners's coordinates, new image width and height.
            QuadrilateralTransformation filter = new QuadrilateralTransformation(corners, 720, 480);
            // Apply filter
            Bitmap target = filter.Apply(iBitmap);

            //Save to result
            MemoryStream stream = new MemoryStream();
            target.Save(stream, target.RawFormat);
            result = stream.ToArray();

            Debug.Log("PaintAR: finish Transform");
            transformOK = true;
        }
        catch (ThreadAbortException)
        {
            Debug.Log("PaintAR: Abort Transform thread");
        }
    }
}