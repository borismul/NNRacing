using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneryActivator : MonoBehaviour {

    float[] cutoffRangesShort;
    float[] cutoffRangesFar;
    float[] cutoffRangesGrass;

    MeshRenderer ren;

    public bool farObject;
    public bool grassObject;

    private void Start()
    {
        cutoffRangesShort = new float[6] { 30, 50, 100, 200, 300, Mathf.Infinity };
        cutoffRangesFar = new float[6] { 100, 200, 400, 600, 1000, Mathf.Infinity };
        cutoffRangesGrass = new float[6] { 10, 20, 40, 100, 200, Mathf.Infinity };
        ren = GetComponent<MeshRenderer>();
    }
    // Update is called once per frame
    void Updates()
    {
        int index = QualitySettings.GetQualityLevel();
        if (!farObject && !grassObject)
        {
            if (Vector3.Distance(CameraController.instance.gameObject.transform.position, transform.position) > cutoffRangesShort[index])
                ren.enabled = false;
            else
                ren.enabled = true;
        }
        else if(!grassObject)
        {
            if (Vector3.Distance(CameraController.instance.gameObject.transform.position, transform.position) > cutoffRangesFar[index])
                ren.enabled = false;
            else
                ren.enabled = true;
        }
        else
        {
            if (Vector3.Distance(CameraController.instance.gameObject.transform.position, transform.position) > cutoffRangesGrass[index])
                ren.enabled = false;
            else
                ren.enabled = true;
        }
	}
}
