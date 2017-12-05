using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneryActivator : MonoBehaviour {

    public float cutoffRange = 100;
    MeshRenderer ren;

    private void Start()
    {
        ren = GetComponent<MeshRenderer>();
    }
    // Update is called once per frame
    void Updates ()
    {
        if (Vector3.Distance(CameraController.instance.gameObject.transform.position, transform.position) > cutoffRange)
            ren.enabled = false;
        else
            ren.enabled = true;
	}
}
