using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class isoPathfinder : MonoBehaviour {

    public isoPoint[] points;
    public LineRenderer lr;


    // Use this for initialization
    void Start () {
        points = GetComponentsInChildren<isoPoint>();
        int curIndex = 0;
        lr.numPositions = points.GetLength(0);
        foreach (isoPoint curPoint in points) {
            lr.SetPosition(curIndex, curPoint.transform.position);
            curIndex += 1;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
