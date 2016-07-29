using UnityEngine;
using System.Collections;

public class SwipeControl : MonoBehaviour
{
    /// <summary>
    /// turn this control on or off
    /// </summary>
    public bool controlEnabled = true;
    /// <summary>
    /// If you set it up from another script you can skip the auto-setup! - Don't forget to call Setup() though!!
    /// </summary>
    public bool skipAutoSetup = false;
    /// <summary>
    /// If you don't want to allow mouse or touch input and only want to use this control for animating a value, set this to false.
    /// </summary>
    public bool allowInput = true;
    /// <summary>
    /// When mouse-controlled, should a simple click on either side of the control increases/decreases the value by one?
    /// </summary>
    public bool clickEdgeToSwitch = true;
    /// <summary>
    /// The width of the part.
    /// </summary>
    public float partWidth = 0f;
    /// <summary>
    /// calculated once in the beginning, so we don't have to do costly divisions every frame
    /// </summary>
    private float partFactor = 1.0f;
    /// <summary>
    /// start with this value
    /// </summary>
    public int startValue = 0;
    /// <summary>
    /// current value
    /// </summary>
    public int currentValue = 0;
    /// <summary>
    /// The max value.
    /// </summary>
    public int maxValue;
    /// <summary>
    ///where you can click to start the swipe movement (once you clicked you can drag outside as well)
    /// </summary>
    public Rect mouseRect;
    /// <summary>
    /// The left edge rect for click switch.
    /// </summary>
    public Rect leftEdgeRectForClickSwitch;
    /// <summary>
    /// The right edge rect for click switch.
    /// </summary>
    public Rect rightEdgeRectForClickSwitch;
    /// <summary>
    /// The matrix.
    /// </summary>
    public Matrix4x4 matrix = Matrix4x4.identity;
    /// <summary>
    /// dragging operation in progress?
    /// </summary>
    private bool touched = false;
    /// <summary>
    /// set to 1 for each finger that starts touching the screen within our touchRect
    /// </summary>
    private int[] fingerStartArea = new int[5];
    /// <summary>
    /// set to 1 if mouse starts clicking within touchRect
    /// </summary>
    private int mouseStartArea = 0;
    /// <summary>
    /// current smooth value between 0 and maxValue
    /// </summary>
    public float smoothValue = 0.0f;
    /// <summary>
    /// The smooth start position.
    /// </summary>
    private float smoothStartPos;
    /// <summary>
    /// The smooth drag offset show how far (% of the width of one element) do we have to drag to set it to change currentValue?
    /// </summary>
    private float smoothDragOffset = 0.2f;
    /// <summary>
    /// The last smooth value.
    /// </summary>
    private float lastSmoothValue;
    /// <summary>
    /// The previous smooth value.
    /// </summary>
    private float[] prevSmoothValue = new float[5];
    /// <summary>
    /// needed to make Mathf.SmoothDamp work even if Time.timeScale == 0
    /// </summary>
    private float realtimeStamp;
    /// <summary>
    /// current velocity of Mathf.SmoothDamp().
    /// </summary>
    private float xVelocity;
    /// <summary>
    /// Clamp the maximum speed of Mathf.SmoothDamp()
    /// </summary>
    public float maxSpeed = 20.0f;
    /// <summary>
    /// The m start position.
    /// </summary>
    private Vector2 mStartPos;
    /// <summary>
    /// Touch/Mouse Position turned into a Vector3
    /// </summary>
    private Vector3 pos;
    /// <summary>
    /// transformed Position
    /// </summary>
    private Vector2 tPos;

    /// <summary>
    /// Use this to initilize
    /// </summary>
    /// <returns></returns>
    IEnumerator Start()
    {

        if (clickEdgeToSwitch && !allowInput)
        {
            Debug.LogWarning("You have enabled clickEdgeToSwitch, but it will not work because allowInput is disabled!", this);
        }

        yield return new WaitForSeconds(0.2f);

        if (!skipAutoSetup)
        {
            Setup();
        }
    }

    /// <summary>
    /// Use this to calculate initial value
    /// </summary>
    public void Setup()
    {
        partFactor = 1.0f / (partWidth * 0.5f);
        smoothValue = (float)currentValue; //Set smoothValue to the currentValue - tip: setting currentValue to -1 and startValue to 0 makes the first image appear at the start...
        currentValue = startValue; //Apply Start-value

        if (mouseRect != new Rect(0, 0, 0, 0))
        {
            SetMouseRect(mouseRect);
        }

        if (leftEdgeRectForClickSwitch == new Rect(0, 0, 0, 0)) CalculateEdgeRectsFromMouseRect(); //Only do this if not set in the inspector	

        if (matrix == Matrix4x4.zero) matrix = Matrix4x4.identity.inverse;
    }

    /// <summary>
    /// Use this to set mouse area
    /// </summary>
    /// <param name="myRect"></param>
    public void SetMouseRect(Rect myRect)
    {
        mouseRect = myRect;
    }

    /// <summary>
    /// Use this to calculate distance between edge and mouse
    /// </summary>
    public void CalculateEdgeRectsFromMouseRect() { CalculateEdgeRectsFromMouseRect(mouseRect); }
    public void CalculateEdgeRectsFromMouseRect(Rect myRect)
    {
        leftEdgeRectForClickSwitch.x = myRect.x;
        leftEdgeRectForClickSwitch.y = myRect.y;
        leftEdgeRectForClickSwitch.width = myRect.width * 0.5f;
        leftEdgeRectForClickSwitch.height = myRect.height;

        rightEdgeRectForClickSwitch.x = myRect.x + myRect.width * 0.5f;
        rightEdgeRectForClickSwitch.y = myRect.y;
        rightEdgeRectForClickSwitch.width = myRect.width * 0.5f;
        rightEdgeRectForClickSwitch.height = myRect.height;
    }

    /// <summary>
    /// Set area for edge touch
    /// </summary>
    /// <param name="leftRect"></param>
    /// <param name="rightRect"></param>
    public void SetEdgeRects(Rect leftRect, Rect rightRect)
    {
        leftEdgeRectForClickSwitch = leftRect;
        rightEdgeRectForClickSwitch = rightRect;
    }

    /// <summary>
    /// determine average value of an array
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    float GetAvgValue(float[] arr)
    {

        float sum = 0.0f;
        for (int i = 0; i < arr.Length; i++)
        {
            sum += arr[i];
        }

        return sum / arr.Length;

    }

    /// <summary>
    /// fill all element of an array with a value
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="val"></param>
    void FillArrayWithValue(float[] arr, float val)
    {

        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = val;
        }

    }


    /// <summary>
    /// Run in each frame
    /// </summary>
    void Update()
    {

        if (controlEnabled)
        {

            touched = false;

            if (allowInput)
            {

                if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
                {
                    pos = new Vector3(Input.mousePosition[0], Screen.height - Input.mousePosition[1], 0.0f);
                    tPos = matrix.inverse.MultiplyPoint3x4(pos);

                    //BEGAN
                    if (Input.GetMouseButtonDown(0) && mouseRect.Contains(tPos))
                    {
                        mouseStartArea = 1;
                    }

                    //WHILE MOUSEDOWN
                    if (mouseStartArea == 1)
                    {
                        touched = true;
                        //START
                        if (Input.GetMouseButtonDown(0))
                        {
                            mStartPos = tPos;
                            smoothStartPos = smoothValue + tPos.x * partFactor;
                            FillArrayWithValue(prevSmoothValue, smoothValue);
                        }
                        //DRAGGING
                        smoothValue = smoothStartPos - tPos.x * partFactor;
                        if (smoothValue < -0.12f) { smoothValue = -0.12f; }
                        else if (smoothValue > maxValue + 0.12f) { smoothValue = maxValue + 0.12f; }
                        //END
                        if (Input.GetMouseButtonUp(0))
                        {
                            if ((tPos - mStartPos).sqrMagnitude < 25)
                            {
                                if (clickEdgeToSwitch)
                                {
                                    if (leftEdgeRectForClickSwitch.Contains(tPos))
                                    {
                                        currentValue--;
                                        if (currentValue < 0) currentValue = 0;
                                    }
                                    else if (rightEdgeRectForClickSwitch.Contains(tPos))
                                    {
                                        currentValue++;
                                        if (currentValue > maxValue) currentValue = maxValue;
                                    }
                                }
                            }
                            else
                            {
                                if (currentValue - (smoothValue + (smoothValue - GetAvgValue(prevSmoothValue))) > smoothDragOffset || currentValue - (smoothValue + (smoothValue - GetAvgValue(prevSmoothValue))) < -smoothDragOffset)
                                { //dragged beyond dragOffset to the right
                                    currentValue = (int)Mathf.Round(smoothValue + (smoothValue - GetAvgValue(prevSmoothValue)));
                                    xVelocity = (smoothValue - GetAvgValue(prevSmoothValue));
                                    if (currentValue > maxValue) currentValue = maxValue;
                                    else if (currentValue < 0) currentValue = 0;
                                }
                            }
                            mouseStartArea = 0;
                        }
                        for (int i = 1; i < prevSmoothValue.Length; i++)
                        {
                            prevSmoothValue[i] = prevSmoothValue[i - 1];
                        }
                        prevSmoothValue[0] = smoothValue;
                    }
                }

                //#if UNITY_IPHONE or UNITY_ANDROID
                foreach (Touch touch in Input.touches)
                {
                    pos = new Vector3(touch.position.x, Screen.height - touch.position.y, 0.0f);
                    tPos = matrix.inverse.MultiplyPoint3x4(pos);

                    //BEGAN
                    print(tPos + " inside " + mouseRect + "?");
                    if (touch.phase == TouchPhase.Began && mouseRect.Contains(tPos))
                    {
                        fingerStartArea[touch.fingerId] = 1;
                        print("hit!");
                    }

                    //WHILE FINGER DOWN
                    if (fingerStartArea[touch.fingerId] == 1)
                    { // no touchRect.Contains check because once you touched down you're allowed to drag outside...
                        touched = true;
                        //START
                        if (touch.phase == TouchPhase.Began)
                        {
                            smoothStartPos = smoothValue + tPos.x * partFactor;
                            FillArrayWithValue(prevSmoothValue, smoothValue);
                        }
                        //DRAGGING
                        smoothValue = smoothStartPos - tPos.x * partFactor;
                        if (smoothValue < -0.12f) { smoothValue = -0.12f; }
                        else if (smoothValue > maxValue + 0.12f) { smoothValue = maxValue + 0.12f; }
                        //END
                        if (touch.phase == TouchPhase.Ended)
                        {
                            if (currentValue - (smoothValue + (smoothValue - GetAvgValue(prevSmoothValue))) > smoothDragOffset || currentValue - (smoothValue + (smoothValue - GetAvgValue(prevSmoothValue))) < -smoothDragOffset)
                            { //dragged beyond dragOffset to the right
                                currentValue = (int)Mathf.Round(smoothValue + (smoothValue - GetAvgValue(prevSmoothValue)));
                                xVelocity = (smoothValue - GetAvgValue(prevSmoothValue)); // * -0.10 ;
                                if (currentValue > maxValue) currentValue = maxValue;
                                else if (currentValue < 0) currentValue = 0;
                            }
                        }
                        for (int i = 1; i < prevSmoothValue.Length; i++)
                        {
                            prevSmoothValue[i] = prevSmoothValue[i - 1];
                        }
                        prevSmoothValue[0] = smoothValue;
                    }

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) fingerStartArea[touch.fingerId] = 0;

                }
                //#endif
            }

            if (!touched)
            {
                smoothValue = Mathf.SmoothDamp(smoothValue, (float)currentValue, ref xVelocity, 0.2f, maxSpeed, Time.realtimeSinceStartup - realtimeStamp);
            }
            realtimeStamp = Time.realtimeSinceStartup;
        }

    }

    void OnGUI()
    {
    }

}
