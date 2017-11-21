using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LiveViewUIController : MonoBehaviour {

    public Text timeLeftValue;
    public Text curSpeedValue;
    CarTrainer carTrainer;
    float avgSpeed;
    int count = 0;
	// Use this for initialization
	void Start ()
    {
        carTrainer = CarTrainer.instance;
    }

    // Update is called once per frame
    void Update ()
    {

        avgSpeed = carTrainer.GetCurSpeed();

        timeLeftValue.text = carTrainer.GetTimeLeft().ToString("F2");
        count++;
        if (count > 10)
            UpdateCurSpeed();

    }

    void UpdateCurSpeed()
    {
        curSpeedValue.text = avgSpeed.ToString("F1");
        curSpeedValue.color = new Color(2.0f * (1 - avgSpeed / GA_Parameters.updateRate), 2.0f * (avgSpeed / GA_Parameters.updateRate), 0);
        count = 0;
    }
}
