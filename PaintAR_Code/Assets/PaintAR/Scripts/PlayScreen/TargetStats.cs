using UnityEngine;
using System.Collections;
using AForge;
using System.Collections.Generic;
using Alt.Sketch;

/// <summary>
/// This class hold the states variables and
/// other neccessary infomation of an ImageTarget.
/// It can be access from TrackableEventHandler, TargetScreenCoords and CameraImageAccess script.
/// </summary>
public class TargetStats : MonoBehaviour {
    /// <summary>
    /// TARGET_FOUND value is set to true when the Target was found on Trackable Event.
    /// It will be set to false when it lost, correspondingly.
    /// </summary>
    public bool TARGET_FOUND = false;
    /// <summary>
    /// TARGET_IMAGE_OK value is set to true when we take the screenshot in CameraImageAccess script.
    /// It will be set to false on initialize and when the target lost tracking.
    /// </summary>
    public bool TARGET_IMAGE_OK = false;
    /// <summary>
    /// TARGET_COORDS_OK value is set to true when we retrieve the Target's 4 corners Screen coordinates,
    /// and all 4 coordinates are inside the Screen area.
    /// It will be set to false on initialize and when the target lost tracking. 
    /// </summary>
    public bool TARGET_COORDS_OK = false;
    /// <summary>
    /// TARGET_RENDERED value is set to true when the 3D model of the Target was rendered.
    /// It will be set to false on initialize and when the target lost tracking. 
    /// </summary>
    public bool TARGET_RENDERED = false;
    /// <summary>
    /// IMAGEBITMAP hold the Picture, de-warped and cropped from a
    /// Camera Screenshot of the Image Target as a Bitmap.
    /// </summary>
    public Bitmap IMAGEBITMAP = new Bitmap();
	/// <summary>
	/// CORNERS hold the screen coordinates of 4 corners of the Target.
	/// </summary>
	public List<IntPoint> CORNERS = new List<IntPoint>();
}
