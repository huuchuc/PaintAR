using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
    }

    /// <summary>
    ///Update is called once per frame
    ///rotate at 90 degrees per second
    /// </summary>
    void Update()
    {
        // rotate at 90 degrees per second
        transform.Rotate(Vector3.down * Time.deltaTime * 30);
    }
}