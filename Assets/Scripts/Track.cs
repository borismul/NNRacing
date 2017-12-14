using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Track
{
    public string trackName;
    public Texture2D texture;
    public List<TrackPoint> trackPoints;
    public float length = 0;
    public bool hasLaps;



    //FitnessTracker tracker;

    public Track(string name, Texture2D texture, List<Vector3> trackPoints)
    {
        trackName = name;
        this.texture = texture;
        this.trackPoints = TrackPoint.CreateTrackPointList(trackPoints, out length);
        // Check if lap is a loop so car can do multiple laps
        if (Vector3.Distance(this.trackPoints[0].position, this.trackPoints[trackPoints.Count - 1].position) < 10)
        {
            hasLaps = true;
        }
    }
}

