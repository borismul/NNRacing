using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour {

    float angle = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        angle += Time.deltaTime * 50;
        transform.rotation = Quaternion.Euler(0, angle, 0);

        if (Input.GetKey(KeyCode.UpArrow))
            transform.position = transform.position + transform.forward * Time.deltaTime * 10;
	}
}
