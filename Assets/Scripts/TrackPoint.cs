using UnityEngine;
using System.Collections.Generic;

public class TrackPoint
{
    public float distance;
    public bool isDone;
    //public Material mat;
    public Vector3 position;

    public bool isTimePoint;

    public TrackPoint(float distance, Vector3 position, bool isTimePoint)
    {
        this.distance = distance;
        this.position = position;
        this.isTimePoint = isTimePoint;
    }
    public void SetDone(CarController controller)
    {
        if (controller.threaded)
            return;

        isDone = true;

        if (isTimePoint)
        {

            controller.SetLastTimePoint();
        }
    }

    public void Reset()
    {
        isDone = false;
    }

    public static List<TrackPoint> CreateTrackPointList(List<Vector3> trackPoints, out float length)
    {
        List<TrackPoint> realTrackPoints = new List<TrackPoint>();
        length = 0;
        int count = 0;
        for (int i = 0; i < trackPoints.Count; i++)
        {
            if (i != 0)
            {
                float distance = Vector3.Distance(trackPoints[i - 1], trackPoints[i]);
                length += distance;
                
                if ((int)(length/450) == count && trackPoints.Count - i > 200)
                {
                    count++;
                    realTrackPoints.Add(new TrackPoint(distance, trackPoints[i], true));
                }
                else
                    realTrackPoints.Add(new TrackPoint(distance, trackPoints[i], false));
            }
            else
            {
                count++;
                realTrackPoints.Add(new TrackPoint(0, trackPoints[0], true));
            }
        }

        return realTrackPoints;
    }

}
