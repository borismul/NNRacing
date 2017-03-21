using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class DistanceTracker : MonoBehaviour {
    public GameObject car;
    public Text distanceText;

    List<TrackPoint> trackpoints = new List<TrackPoint>();

    TrackPoint currentPoint;
    TrackPoint nextPoint;

    int trackPointNum = 1;
	// Use this for initialization
	void Start () {
        TrackPoint prevPoint = null;
	    for(int i = 0; i < transform.childCount; i++)
        { 
            TrackPoint point = transform.GetChild(i).GetComponent<TrackPoint>();
            trackpoints.Add(point);
            if (prevPoint != null)
                point.distance = prevPoint.distance + Vector3.Distance(prevPoint.transform.position, point.transform.position);
            else
                point.distance = 0;

            prevPoint = point;

        }

        currentPoint = trackpoints[0];
        nextPoint = trackpoints[1];

	}
	
	// Update is called once per frames
	void Update ()
    {
        if (Vector3.Distance(currentPoint.transform.position, car.transform.position) > Vector3.Distance(nextPoint.transform.position, car.transform.position))
        {
            nextPoint.SetDone();
            trackPointNum++;
            currentPoint = nextPoint;
            nextPoint = trackpoints[trackPointNum];
            distanceText.text = currentPoint.distance.ToString();
        }
	}
}
