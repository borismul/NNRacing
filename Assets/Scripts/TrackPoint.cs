using UnityEngine;
using System.Collections;

public class TrackPoint : MonoBehaviour
{
    public float distance;
    public bool isDone;
    public Material mat;

    void Start()
    {
        Material matCopy = new Material(mat);
        GetComponent<MeshRenderer>().material = new Material(matCopy);
        matCopy.color = Color.red;
    }
    public void SetDone()
    {
        isDone = true;
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    public void SetNotDone()
    {
        isDone = false;
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

}
