using UnityEngine;
using System.Collections;

public class TrackPoints : MonoBehaviour {

    public static TrackPoints instance;
	void Start()
    {
        instance = this;
    }
}
