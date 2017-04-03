using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Track
{
    List<TrackPoint> trackPoints;
    public bool hasLaps;

    TrackPoint currentPoint;
    TrackPoint nextPoint;

    int pointNum;

    FitnessTracker tracker;

    public Track(List<TrackPoint> trackPoints, FitnessTracker tracker, bool reversed)
    {
        if (!reversed)
            this.trackPoints = trackPoints;
        else
        {
            trackPoints.Reverse();
            this.trackPoints = trackPoints;
        }

        // Check if lap is a loop so car can do multiple laps
        if (Vector3.Distance(trackPoints[0].position, trackPoints[trackPoints.Count - 1].position) < 5)
        {
            hasLaps = true;
        }

        currentPoint = trackPoints[0];
        nextPoint = trackPoints[1];
        pointNum = 0;
        this.tracker = tracker;
    }

    public void Reset()
    {
        foreach(TrackPoint point in trackPoints)
        {
            point.Reset();
        }

        currentPoint = trackPoints[0];

        nextPoint = trackPoints[1];

        pointNum = 0;
    }

    public float CheckSetDone(Vector3 carPosition)
    {
        if (CheckDistance(carPosition) < 4)
        {
            nextPoint.SetDone();
            currentPoint = nextPoint;
            nextPoint = trackPoints[pointNum];
            pointNum++;

            if (pointNum == trackPoints.Count)
            {
                tracker.laps++;
                Reset();

                currentPoint = trackPoints[0];
                nextPoint = trackPoints[1];
            }

            return currentPoint.distance;
        }
        else
        {
            return 0;
        }
    }

    public float CheckDistance(Vector3 carPosition)
    {
        return Vector3.Distance(carPosition, nextPoint.position);
    }

    public Vector3 CurrentPosition()
    {
        return currentPoint.position;
    }

    public Quaternion CurrentRotation()
    {
        return Quaternion.LookRotation(nextPoint.position - currentPoint.position);
    }
}

