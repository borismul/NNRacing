using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTrackController : MonoBehaviour {

    public Track track;

    TrackPoint currentPoint;
    public TrackPoint nextPoint;

    public int pointNum;

    FitnessTracker tracker;

    CarController car;

    void Awake()
    {
        tracker = GetComponent<FitnessTracker>();
        car = GetComponent<CarController>();
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

        GetComponent<FitnessTracker>().TotalLapDistance = track.length;

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

        for (int i = 0; i < 50; i++)
        {
            if (startPoint + i == track.trackPoints.Count)
                break;

            startPoint = pointNum;
            int nextPointIndex = startPoint + i;

            if (nextPointIndex >= track.trackPoints.Count)
                nextPointIndex = 0;

            if (CheckDistance(carPosition, track.trackPoints[nextPointIndex]) < CheckDistance(carPosition, currentPoint))
            {

                for (int j = 0; j <= i; j++)
                {
                    if (startPoint + j >= track.trackPoints.Count || track.trackPoints[startPoint + j].isDone)
                        continue;

                    if (!(tracker.laps == 0 && j + startPoint == 0))
                        track.trackPoints[startPoint + j].SetDone(car);

                    currentPoint = nextPoint;
                    nextPoint = track.trackPoints[pointNum];
                    totDistance += track.trackPoints[startPoint + j].distance;
                    pointNum++;

                    if (pointNum == track.trackPoints.Count)
                    {
                        tracker.laps++;

                        if (tracker.laps == GA_Parameters.laps)
                            break;

                        Reset();
                    }

                }
            }
        }

        startPoint = pointNum;

        for (int i = 1; i < 2; i++)
        {
            if (startPoint - i < 0 || startPoint - i + 2 >= track.trackPoints.Count)
                break;

            if (CheckDistance(carPosition, track.trackPoints[startPoint - i]) < CheckDistance(carPosition, track.trackPoints[startPoint - i + 1]) && CheckDistance(carPosition, track.trackPoints[startPoint - i + 2]) > CheckDistance(carPosition, track.trackPoints[startPoint - i]))
            {
                totDistance -= track.trackPoints[startPoint - i].distance;
                track.trackPoints[startPoint - i].isDone = false;
                nextPoint = track.trackPoints[startPoint - i + 1];
                currentPoint = track.trackPoints[startPoint - i];
                pointNum--;
                startPoint = pointNum;

            }

        }

        startPoint = pointNum;

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

    public Vector3 NextPosition(int pointsAhead)
    {
        if (pointNum - 1 + pointsAhead > track.trackPoints.Count - 1)
            return track.trackPoints[pointNum - 1 + pointsAhead - track.trackPoints.Count].position;
        else
            return track.trackPoints[pointNum - 1 + pointsAhead].position;

    }

    public Quaternion NextRotation(int pointsAhead)
    {
        int currentPoint = pointNum - 1 + pointsAhead;
        int nextPoint = currentPoint + 1;
        Vector3 from;
        Vector3 to;
        if (currentPoint > track.trackPoints.Count - 1)
            from = track.trackPoints[currentPoint - track.trackPoints.Count].position;
        else
            from = track.trackPoints[currentPoint].position;

        if (nextPoint > track.trackPoints.Count - 1)
            to = track.trackPoints[nextPoint - track.trackPoints.Count].position;
        else
            to = track.trackPoints[nextPoint].position;

        return Quaternion.LookRotation(to - from);

    }

    public Vector3 NextPosition(Vector3 position, float distance)
    {
        float curDistance = CheckDistance(position, true);
        position = NextPosition(2);
        float prevDistance;
        int pointsAhead = 3;
        Vector3 nextPos = Vector3.zero;

        while (curDistance < distance)
        {
            nextPos = NextPosition(pointsAhead);
            prevDistance = curDistance;
            curDistance += Vector3.Distance(nextPos, position);
            position = nextPos;
            pointsAhead++;
        }

        Vector3 prevPos = NextPosition(pointsAhead - 2);
        float curPointsDistance = Vector3.Distance(prevPos, nextPos);
        Vector3 outPoint = Vector3.Lerp(nextPos, prevPos, (curDistance - distance) / curPointsDistance);

        //print(curDistance - Vector3.Distance(outPoint, nextPos));
        return outPoint;

    }

    public Quaternion NextRotation(Vector3 position, float distance)
    {
        float curDistance = CheckDistance(position, true);
        position = NextPosition(2);
        float prevDistance;
        int pointsAhead = 3;
        Vector3 nextPos = Vector3.zero;


        while (curDistance < distance)
        {
            nextPos = NextPosition(pointsAhead);
            prevDistance = curDistance;
            curDistance += Vector3.Distance(nextPos, position);
            position = nextPos;
            pointsAhead++;
        }

        Vector3 prevPos = NextPosition(pointsAhead - 2);
        Quaternion prevRot = NextRotation(pointsAhead - 2);
        Quaternion nextRot = NextRotation(pointsAhead - 1);

        float curPointsDistance = Vector3.Distance(prevPos, nextPos);
        Quaternion outPoint = Quaternion.Lerp(nextRot, prevRot, (curDistance - distance) / curPointsDistance);

        //print(curDistance - Vector3.Distance(outPoint, nextPos));
        return outPoint;

    }

    public Vector2 NormalizedNextPosition(Vector3 position, Quaternion rotation, float distance)
    {
        Vector3 nextPos =  NextPosition(position, distance) - position;

        rotation = Quaternion.Euler(0, -rotation.eulerAngles.y, 0);
        nextPos = rotation * nextPos;
        Vector2 nextPos2D = new Vector2(nextPos.x, nextPos.z).normalized;
        return nextPos2D;
    }

    public Vector2 NormalizedNextPosition(Vector3 position, Quaternion rotation, int pointsAhead)
    {
        Vector3 nextPos = NextPosition(pointsAhead) - position;
        rotation = Quaternion.Euler(0, -rotation.eulerAngles.y, 0);
        nextPos = rotation * nextPos;


        Vector2 nextPos2D = new Vector2(nextPos.x, nextPos.z).normalized;
        return nextPos2D;
    }

    public Quaternion CurrentRotation()
    {
        return Quaternion.LookRotation(nextPoint.position - currentPoint.position);
    }
}
