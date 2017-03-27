using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public GameObject car;
    Vector3 startDistance;
	// Use this for initialization
	void Awake () {
        startDistance = transform.position - car.transform.position;
	}
	
	// Update is called once per frame
	void LateUpdate () {
        transform.position = car.transform.position + startDistance;
	}
}
