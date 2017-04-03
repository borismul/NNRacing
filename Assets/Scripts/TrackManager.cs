using UnityEngine;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour {

    public List<Track> tracks = new List<Track>();
    public GameObject tracksObject;

    public Track currentTrack;
    public int currentTrackIndex;
	// Use this for initialization
	void Awake () {

        tracksObject = TrackPoints.instance.gameObject;

        for (int i = 0; i < tracksObject.transform.childCount; i++)
        {
            List<TrackPoint> points = new List<TrackPoint>();

            for(int j = 0; j < tracksObject.transform.GetChild(i).childCount; j++)
            {
                Vector3 trackPointPos = tracksObject.transform.GetChild(i).position;
                if (j != 0)
                    points.Add(new TrackPoint(Vector3.Distance(points[j - 1].position, tracksObject.transform.GetChild(i).GetChild(j).position), tracksObject.transform.GetChild(i).GetChild(j).position));
                else
                    points.Add(new TrackPoint(0, tracksObject.transform.GetChild(i).GetChild(j).position));
            }
            tracks.Add(new Track(points, GetComponent<FitnessTracker>(), false));
        }
	}

    public void SelectTrack(int i)
    {
        GetComponent<FitnessTracker>().SaveCurrentFitness();
        GetComponent<FitnessTracker>().laps = 0;

        if (currentTrack != null)
            currentTrack.Reset();

        currentTrack = tracks[i];
        currentTrackIndex = i;

        GetComponent<CarController>().Reset();
    }

    public void SelectNextTrack()
    {
        currentTrackIndex++;
        GetComponent<FitnessTracker>().SaveCurrentFitness();
        GetComponent<FitnessTracker>().laps = 0;
        if (currentTrack != null)
            currentTrack.Reset();

        if (tracks.Count - 1 < currentTrackIndex)
            return;
        
        currentTrack = tracks[currentTrackIndex];
        GetComponent<CarController>().Reset();
    }

    public int GetTrackCount()
    {
        return tracks.Count;
    }

    public float CheckSetDone(Vector3 carPosition)
    {
        return currentTrack.CheckSetDone(carPosition);
    }
}
