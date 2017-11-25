using UnityEngine;
using System.Collections.Generic;

public class TrackPoint
{
    public float distance;
    public bool isDone;
    //public Material mat;
    public Vector3 position;

    public TrackPoint(float distance, Vector3 position)
    {
        this.distance = distance;
        this.position = position;
    }
    public void SetDone()
    {
        isDone = true;
    }

    public void Reset()
    {
        isDone = false;
    }

    public static List<TrackPoint> CreateTrackPointList(List<Vector3> trackPoints)
    {
        List<TrackPoint> realTrackPoints = new List<TrackPoint>();

        for (int i = 0; i < trackPoints.Count; i++)
        {
            if (i != 0)
            {
                float distance = Vector3.Distance(trackPoints[i - 1], trackPoints[i]);
                realTrackPoints.Add(new TrackPoint(distance, trackPoints[i]));
            }
            else
                realTrackPoints.Add(new TrackPoint(0, trackPoints[0]));
        }

        return realTrackPoints;
    }

}
