using UnityEngine;
using System.Collections;

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
        //GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    public void Reset()
    {
        isDone = false;
        //GetComponent<MeshRenderer>().material.color = Color.red;
    }

}
