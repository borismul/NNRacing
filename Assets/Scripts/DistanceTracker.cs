using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class DistanceTracker : MonoBehaviour {
    public NewCarController car;
    public Text distanceText;
    public Text timeText;

    public List<TrackPoint> trackpoints1 = new List<TrackPoint>();
    public List<TrackPoint> trackpoints2 = new List<TrackPoint>();

    public List<TrackPoint> curTrackPoints;


    public TrackPoint currentPoint;
    public TrackPoint nextPoint;

    int trackPointNum = 1;

    public float distance;
    public float partialDistance;

    public static DistanceTracker instance;

    public float time;

    float penalty = 1;

    int startNode;
	// Use this for initialization
	void Awake () {
        instance = this;
        TrackPoint prevPoint = null;
	    for(int i = 0; i < transform.GetChild(0).childCount; i++)
        { 
            TrackPoint point = transform.GetChild(0).GetChild(i).GetComponent<TrackPoint>();
            trackpoints1.Add(point);
            if (prevPoint != null)
                point.distance = Vector3.Distance(prevPoint.transform.position, point.transform.position);
            else
                point.distance = 0;

            prevPoint = point;

        }

        //for (int i = transform.GetChild(1).childCount-1; i >= 0; i--)
        //{
        //    TrackPoint point = transform.GetChild(1).GetChild(i).GetComponent<TrackPoint>();
        //    trackpoints2.Add(point);
        //    if (prevPoint != null)
        //        point.distance = Vector3.Distance(prevPoint.transform.position, point.transform.position);
        //    else
        //        point.distance = 0;

        //    prevPoint = point;

        //}

        for (int i = 0; i < transform.GetChild(1).childCount; i++)
        {
            TrackPoint point = transform.GetChild(1).GetChild(i).GetComponent<TrackPoint>();
            trackpoints2.Add(point);
            if (prevPoint != null)
                point.distance = Vector3.Distance(prevPoint.transform.position, point.transform.position);
            else
                point.distance = 0;

            prevPoint = point;

        }

        curTrackPoints = trackpoints1;
        currentPoint = curTrackPoints[0];
        nextPoint = curTrackPoints[1];
        distanceText.text = "Current Fitness: " + 0.ToString();

    }

    // Update is called once per frames
    public bool UpdateDistance (float time)
    {
        if (Vector3.Distance(nextPoint.transform.position, car.transform.position) > 20)
        {
            return false;
        }

        if (Vector3.Distance(currentPoint.transform.position, car.transform.position) > Vector3.Distance(nextPoint.transform.position, car.transform.position))
        {
            nextPoint.SetDone();
            if (curTrackPoints.Count > trackPointNum + 1)
                trackPointNum++;
            else
                trackPointNum = 0;

            currentPoint = nextPoint;

            nextPoint = curTrackPoints[trackPointNum];
            partialDistance += nextPoint.distance;

            if (trackPointNum == curTrackPoints.Count - 1)
                return false;
        }

        distance = (partialDistance - (nextPoint.transform.position - car.transform.position).magnitude) *penalty;

        float fitness = (Mathf.Pow(distance, 1f));
        distanceText.text = "Current Fitness: " + fitness.ToString("F2");

        return true;
    }

    public void NextTrack()
    {
        for (int i = 0; i < curTrackPoints.Count; i++)
        {
            curTrackPoints[i].SetNotDone();
        }

        if (curTrackPoints == trackpoints1)
            curTrackPoints = trackpoints2;
        else
            DistanceTracker.instance.curTrackPoints = DistanceTracker.instance.trackpoints1;

        startNode = NeuralNetwork.rand.Next(0,0 /*DistanceTracker.instance.curTrackPoints.Count - 1*/);

        currentPoint = curTrackPoints[startNode];

        if (curTrackPoints.Count > startNode)
            nextPoint = curTrackPoints[startNode+1];
        else
        {
            nextPoint = curTrackPoints[0];
        }

        if (curTrackPoints.Count > startNode+1)
            trackPointNum = startNode+1;
        else
            trackPointNum = 0;



    }

    public void CompleteReset()
    {
        penalty = 1;
        for (int i = 0; i < curTrackPoints.Count; i++)
        {
            curTrackPoints[i].SetNotDone();
        }

        curTrackPoints = trackpoints1;

        startNode = NeuralNetwork.rand.Next(0, 0 /*DistanceTracker.instance.curTrackPoints.Count - 1*/);

        currentPoint = curTrackPoints[startNode];

        if (curTrackPoints.Count > startNode)
            nextPoint = curTrackPoints[startNode + 1];
        else
        {
            nextPoint = curTrackPoints[0];
        }

        partialDistance = 0;

        distance = 0;

        if (curTrackPoints.Count > startNode + 1)
            trackPointNum = startNode + 1;
        else
            trackPointNum = 0;
    }

    public void Penalty(float penalty)
    {
        this.penalty *= penalty;

    }

}
