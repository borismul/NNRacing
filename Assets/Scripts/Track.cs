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
        pointNum = 2;
        this.tracker = tracker;
    }

    public void Reset()
    {
        foreach (TrackPoint point in trackPoints)
        {
            point.Reset();
        }

        currentPoint = trackPoints[0];

        nextPoint = trackPoints[1];

        pointNum = 2;
    }

    public float CheckSetDone(Vector3 carPosition)
    {
        int startPoint = pointNum;
        float totDistance = 0;
        for (int i = 0; i < 30; i++)
        {
            if (startPoint + i == trackPoints.Count)
                break;

            if (CheckDistance(carPosition, trackPoints[startPoint + i]) < CheckDistance(carPosition, currentPoint))
            {
                for (int j = 0; j <= i; j++)
                {
                    if (trackPoints[startPoint + j].isDone)
                        continue;

                    trackPoints[startPoint + j].SetDone();
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

                    totDistance += trackPoints[startPoint + j].distance;
                }

            }

        }

        return totDistance;

    }

    public float CheckDistance(Vector3 carPosition, TrackPoint point)
    {
        return Vector3.Distance(carPosition, point.position);
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

