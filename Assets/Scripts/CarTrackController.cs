using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTrackController : MonoBehaviour {

    public Track track;

    TrackPoint currentPoint;
    public TrackPoint nextPoint;

    public int pointNum;

    FitnessTracker tracker;

    void Awake()
    {
        tracker = GetComponent<FitnessTracker>();
    }

    public void Reset()
    {
        foreach (TrackPoint point in track.trackPoints)
        {
            point.Reset();
        }

        currentPoint = track.trackPoints[0];
        nextPoint = track.trackPoints[1];
        pointNum = 2;
    }

    public void SetTrack(Track track)
    {
        this.track = track;
        currentPoint = track.trackPoints[0];
        nextPoint = track.trackPoints[1];

        foreach (TrackPoint point in track.trackPoints)
        {
            point.Reset();
        }

        pointNum = 2;
    }

    public float CheckSetDone(Vector3 carPosition)
    {
        int startPoint = pointNum;
        float totDistance = 0;

        for (int i = 1; i < 1; i++)
        {
            if (startPoint - i < 0)
                break;

            if (CheckDistance(carPosition, track.trackPoints[startPoint - i]) < CheckDistance(carPosition, currentPoint))
            {
                totDistance -= currentPoint.distance;
                nextPoint = currentPoint;
                currentPoint = track.trackPoints[startPoint - i];
                return totDistance;
            }
        }

        for (int i = 0; i < 5; i++)
        {
            if (startPoint + i == track.trackPoints.Count)
                break;

            if (CheckDistance(carPosition, track.trackPoints[startPoint + i]) < CheckDistance(carPosition, currentPoint))
            {
                for (int j = 0; j <= i; j++)
                {
                    if (track.trackPoints[startPoint + j].isDone)
                        continue;

                    track.trackPoints[startPoint + j].SetDone();
                    currentPoint = nextPoint;
                    nextPoint = track.trackPoints[pointNum];
                    pointNum++;

                    if (pointNum == track.trackPoints.Count)
                    {
                        tracker.laps++;
                        Reset();

                        currentPoint = track.trackPoints[0];
                        nextPoint = track.trackPoints[1];
                    }

                    totDistance += track.trackPoints[startPoint + j].distance;
                }

            }

        }

        return totDistance;

    }

    public float CheckDistance(Vector3 carPosition, TrackPoint point)
    {
        return Vector3.Distance(carPosition, point.position);
    }

    public float CheckDistance(Vector3 carPosition, bool perpendicular)
    {
        if (perpendicular)
        {
            if (pointNum > 0)
            {
                Vector3 direction = (nextPoint.position - currentPoint.position).normalized;
                return Mathf.Abs(Vector3.Dot(direction, nextPoint.position - carPosition));
            }
            else
            {
                Vector3 direction = (nextPoint.position - track.trackPoints[pointNum].position).normalized;
                return Mathf.Abs(Vector3.Dot(direction, nextPoint.position - carPosition));
            }
        }
        else
            return CheckDistance(carPosition, nextPoint);

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
