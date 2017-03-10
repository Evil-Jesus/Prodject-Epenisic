using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public static Camera curCam = null;

    public static void reset()
    {
        curCam.transform.position = Vector3.zero;
    }

    // Use this for initialization
    void Start()
    {
        curCam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
