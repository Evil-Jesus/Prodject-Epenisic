using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController cc = null;

    public Camera curCam = null;
    public float speed = 5;
    public GameObject player = null;
    public GameObject focus = null;

    public void Start()
    {
        cc = this;
        curCam = GetComponent<Camera>();
        player = GameObject.Find("Player");
        focus = player;
    }

    void FixedUpdate()
    {
        float step = speed * Time.deltaTime;
        Vector3 prefix = new Vector3(0, 0, -10);
        curCam.transform.position = Vector3.MoveTowards(curCam.transform.position, focus.transform.position + prefix, step);
    }

    public void reset()
    {
        focus = player;
    }

    public void setFocus(GameObject newFocus)
    {
        focus = newFocus;
    }
}
