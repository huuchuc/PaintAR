using UnityEngine;
using System.Collections;

public class GlobalVariable : Singleton<GlobalVariable>
{
    /// <summary>
    /// The savedCapture will check picture is capture or not
    /// </summary>
    public bool savedCapture = true;
    /// <summary>
    /// The get level is string save back scene level
    /// </summary>
    public string getLevel = "";
    /// <summary>
    /// The get capture is bool variable check show Menu when Capture button is press
    /// </summary>
    public bool getCapture = false;
}
