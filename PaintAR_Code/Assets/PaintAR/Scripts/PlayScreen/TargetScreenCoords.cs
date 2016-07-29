using UnityEngine;
using System.Collections;
using AForge;
using System.Collections.Generic;
using System.IO;
using System;
using Vectrosity;

/// <summary>
/// This class main responsibility is to obtain the projected screen coordinates (pixels) of a generic 3D point located on an Image Target plane.
/// In addition, it also take responsibility to draw border when the Target was found.
/// </summary>
public class TargetScreenCoords : MonoBehaviour
{
    /// <summary>
    /// This is to access ImageTargetBehaviour component
    /// </summary>
    private ImageTargetBehaviour mImageTargetBehaviour = null;
    /// <summary>
    /// This is to access to camera image
    /// </summary>
    public CameraImageAccess camAccess;
    /// <summary>
    /// The object stands for states variable of the Image Target.
    /// </summary>
    public TargetStats targetStats;
    /// <summary>
    /// The shift value to add when translate screen point coordinate
    /// </summary>
    private int shiftValue;
    /// <summary>
    /// The screen point coordinate of 4 corners of target
    /// </summary>
    public List<IntPoint> corners;
    /// <summary>
    /// A vector line to draw border;
    /// </summary>
    VectorLine line;
    /// <summary>
    /// An array of points to draw line.
    /// </summary>
    Vector3[] linePoints;
    /// <summary>
    /// Count the number of attempt that target's on screen
    /// When it reach 10, process the screen's coordinate.
    /// This is to avoid shaking.
    /// </summary>
    int countTime = 0;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
        // We retrieve the ImageTargetBehaviour component
        // Note: This only works if this script is attached to an ImageTarget
        mImageTargetBehaviour = GetComponent<ImageTargetBehaviour>();
        /// Run GetTargetCoords each 0.1sec
        InvokeRepeating("GetTargetCoords", 1f, 0.1F);
        VectorLine.SetCamera();
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        //GetTargetCoords();
    }
    /// <summary>
    /// Processing of get target screen coordinate and draw border
    /// </summary>
    void GetTargetCoords()
    {
        if (mImageTargetBehaviour == null)
        {
            Debug.Log("ImageTargetBehaviour not found");
            //return;
        }

        // Destroy previous line
        VectorLine.Destroy(ref line);

        // Only get coordinates after the target was found and befor its 3D model was render.
        if (targetStats.TARGET_FOUND == true && targetStats.TARGET_RENDERED == false)
        {
            Vector2 targetSize = mImageTargetBehaviour.GetSize();
            float targetAspect = targetSize.x / targetSize.y;

            // Note: the target reference plane in Unity is X-Z,
            // while Y is the normal direction to the target plane

            //top-left
            Vector3 pointOnTarget0 = new Vector3(-0.5f, 0, 0.5f / targetAspect);
            // We convert the local point to world coordinates
            Vector3 targetPointInWorldRef0 = transform.TransformPoint(pointOnTarget0);
            // We project the world coordinates to screen coords (pixels)
            Vector3 screenPoint0 = Camera.main.WorldToScreenPoint(targetPointInWorldRef0);

            //top-right
            Vector3 pointOnTarget1 = new Vector3(0.5f, 0, 0.5f / targetAspect);
            Vector3 targetPointInWorldRef1 = transform.TransformPoint(pointOnTarget1);
            Vector3 screenPoint1 = Camera.main.WorldToScreenPoint(targetPointInWorldRef1);

            //bottom-right
            Vector3 pointOnTarget2 = new Vector3(0.5f, 0, -0.5f / targetAspect);
            Vector3 targetPointInWorldRef2 = transform.TransformPoint(pointOnTarget2);
            Vector3 screenPoint2 = Camera.main.WorldToScreenPoint(targetPointInWorldRef2);

            //bottom-left
            Vector3 pointOnTarget3 = new Vector3(-0.5f, 0, -0.5f / targetAspect);            
            Vector3 targetPointInWorldRef3 = transform.TransformPoint(pointOnTarget3);            
            Vector3 screenPoint3 = Camera.main.WorldToScreenPoint(targetPointInWorldRef3);

            // Declare the line
            Vector3[] linePoints = { targetPointInWorldRef0, targetPointInWorldRef1, targetPointInWorldRef2, targetPointInWorldRef3, targetPointInWorldRef0 };
            line = new VectorLine("Borders", linePoints, Color.red, null, (float)Screen.width / 70f, LineType.Continuous, Joins.Weld);
            line.Draw();

            //Debug.Log("top-left(" + screenPoint0.x + ", " + screenPoint0.y + ") top-right(" + screenPoint1.x + ", " + screenPoint1.y + ")  bottom-right(" + screenPoint2.x + ", " + screenPoint2.y + ")  bottom-left(" + screenPoint3.x + ", " + screenPoint3.y + ")");

            //Check if all part of target if on screen
            if (screenPoint0.x > 0 && screenPoint0.x < Screen.width && screenPoint0.y > 0 && screenPoint0.y < Screen.height && screenPoint1.x > 0 && screenPoint1.x < Screen.width && screenPoint1.y > 0 && screenPoint1.y < Screen.height && screenPoint2.x > 0 && screenPoint2.x < Screen.width && screenPoint2.y > 0 && screenPoint2.y < Screen.height && screenPoint3.x > 0 && screenPoint3.x < Screen.width && screenPoint3.y > 0 && screenPoint3.y < Screen.height)
            {
                //Set border line to blue when all part of target inside screen
                line.SetColor(Color.blue);

                countTime++;

                if (targetStats.TARGET_COORDS_OK == false && countTime == 10)
                {                    
                    // Calculate new coordinates since we will use different coordinate system later. 
                    if (camAccess.scaleByHeight == true)
                    {
                        shiftValue = (int)Math.Round((camAccess.imageWidth - Screen.width * camAccess.scalarFactor) / 2);
                        Debug.Log("PaintAR: Shift right: " + shiftValue);

                        // define quadrilateral's corners
                        corners = new List<IntPoint>();
                        corners.Add(new IntPoint((int)(screenPoint0.x * camAccess.scalarFactor) + shiftValue, (int)((Screen.height - screenPoint0.y) * camAccess.scalarFactor)));
                        corners.Add(new IntPoint((int)(screenPoint1.x * camAccess.scalarFactor) + shiftValue, (int)((Screen.height - screenPoint1.y) * camAccess.scalarFactor)));
                        corners.Add(new IntPoint((int)(screenPoint2.x * camAccess.scalarFactor) + shiftValue, (int)((Screen.height - screenPoint2.y) * camAccess.scalarFactor)));
                        corners.Add(new IntPoint((int)(screenPoint3.x * camAccess.scalarFactor) + shiftValue, (int)((Screen.height - screenPoint3.y) * camAccess.scalarFactor)));
                    }

                    else
                    {
                        shiftValue = (int)Math.Round((camAccess.imageHeight - Screen.height * camAccess.scalarFactor) / 2);
                        Debug.Log("PaintAR: Shift down: " + shiftValue);

                        // define quadrilateral's corners
                        corners = new List<IntPoint>();
                        corners.Add(new IntPoint((int)(screenPoint0.x * camAccess.scalarFactor) - shiftValue, (int)((Screen.height - screenPoint0.y) * camAccess.scalarFactor)) + shiftValue);
                        corners.Add(new IntPoint((int)(screenPoint1.x * camAccess.scalarFactor) - shiftValue, (int)((Screen.height - screenPoint1.y) * camAccess.scalarFactor)) + shiftValue);
                        corners.Add(new IntPoint((int)(screenPoint2.x * camAccess.scalarFactor) - shiftValue, (int)((Screen.height - screenPoint2.y) * camAccess.scalarFactor)) + shiftValue);
                        corners.Add(new IntPoint((int)(screenPoint3.x * camAccess.scalarFactor) - shiftValue, (int)((Screen.height - screenPoint3.y) * camAccess.scalarFactor)) + shiftValue);
                    }
                    Debug.Log("PaintAR: Coords: top-left(" + corners[0].X + ", " + corners[0].Y + ") top-right(" + corners[1].X + ", " + corners[1].Y + ")  bottom-right(" + corners[2].X + ", " + corners[2].Y + ")  bottom-left(" + corners[3].X + ", " + corners[3].Y + ")");

                    // Retrieve the camera image
                    StartCoroutine(camAccess.TakeScreenshot());

                    // Pass state and 4 corners to TargetStats
                    targetStats.CORNERS = corners;
                    targetStats.TARGET_COORDS_OK = true;
                    // Reset counting
                    countTime = 0;
                }
            }
            else
            {
                //Set border line to blue when some part of target outside screen
                line.SetColor(Color.red);
            }
        }
        else
        {
            // Reset counting
            countTime = 0;
        }
    }
}