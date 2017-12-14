using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneryActivator : MonoBehaviour {

    public static float[] cutoffRangesShort = new float[6] { 30, 50, 100, 200, 300, 1000 };
    public static float[] cutoffRangesFarnew = new  float[6] { 100, 200, 400, 600, 1000, 1000};
    public static float[] cutoffRangesGrass = new float[6] { 10, 20, 40, 100, 200, 300 };

    MeshRenderer ren;

    public bool farObject;
    public bool grassObject;

    public Camera cam;

    private void Start()
    {
        ren = GetComponent<MeshRenderer>();

    }

    private void Update()
    {
        if (grassObject)
            if (Vector3.Distance(CameraController.instance.gameObject.transform.position, transform.position) > 100)
                ren.enabled = false;
            else
                ren.enabled = true;

        else if (farObject)
            if (Vector3.Distance(CameraController.instance.gameObject.transform.position, transform.position) > 400)
                ren.enabled = false;
            else
                ren.enabled = true;

        else
            if(Vector3.Distance(CameraController.instance.gameObject.transform.position, transform.position) > 200)
                ren.enabled = false;
            else
                ren.enabled = true;
    }
}
